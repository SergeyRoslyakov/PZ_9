using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace PZ_9
{
    public class OrderProcessorTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly OrderProcessor _orderProcessor;

        public OrderProcessorTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _mockEmailService = new Mock<IEmailService>();
            _orderProcessor = new OrderProcessor(_mockDatabase.Object, _mockEmailService.Object);
        }

        [Fact]
        public void ProcessOrder_WhenOrderIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _orderProcessor.ProcessOrder(null));
        }

        [Fact]
        public void ProcessOrder_WhenTotalAmountIsZero_ReturnsFalse()
        {
            // Arrange
            var order = new Order { TotalAmount = 0 };

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            Assert.False(result);
            Assert.False(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenTotalAmountIsNegative_ReturnsFalse()
        {
            // Arrange
            var order = new Order { TotalAmount = -50 };

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            Assert.False(result);
            Assert.False(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenDatabaseNotConnected_ConnectsToDatabase()
        {
            // Arrange
            var order = new Order { TotalAmount = 50, Id = 1, CustomerEmail = "test@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(false);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockDatabase.Verify(db => db.Connect(), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void ProcessOrder_WhenDatabaseAlreadyConnected_DoesNotConnectAgain()
        {
            // Arrange
            var order = new Order { TotalAmount = 50, Id = 1, CustomerEmail = "test@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockDatabase.Verify(db => db.Connect(), Times.Never);
            Assert.True(result);
        }

        [Fact]
        public void ProcessOrder_WhenTotalAmountGreaterThan100_SendsEmailConfirmation()
        {
            // Arrange
            var order = new Order { TotalAmount = 150, Id = 1, CustomerEmail = "customer@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockEmailService.Verify(es => es.SendOrderConfirmation("customer@test.com", 1), Times.Once);
            Assert.True(result);
            Assert.True(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenTotalAmountExactly100_DoesNotSendEmail()
        {
            // Arrange
            var order = new Order { TotalAmount = 100, Id = 1, CustomerEmail = "customer@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockEmailService.Verify(es => es.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            Assert.True(result);
            Assert.True(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenTotalAmountLessThan100_DoesNotSendEmail()
        {
            // Arrange
            var order = new Order { TotalAmount = 99, Id = 1, CustomerEmail = "customer@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockEmailService.Verify(es => es.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            Assert.True(result);
            Assert.True(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenDatabaseSaveFails_ReturnsFalseAndOrderNotProcessed()
        {
            // Arrange
            var order = new Order { TotalAmount = 50, Id = 1, CustomerEmail = "test@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);
            _mockDatabase.Setup(db => db.Save(order)).Throws(new Exception("Database error"));

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            Assert.False(result);
            Assert.False(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenEmailServiceFails_ReturnsFalseAndOrderNotProcessed()
        {
            // Arrange
            var order = new Order { TotalAmount = 150, Id = 1, CustomerEmail = "test@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);
            _mockEmailService.Setup(es => es.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()))
                           .Throws(new Exception("Email error"));

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            Assert.False(result);
            Assert.False(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenSuccessful_SavesOrderToDatabase()
        {
            // Arrange
            var order = new Order { TotalAmount = 50, Id = 1, CustomerEmail = "test@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockDatabase.Verify(db => db.Save(order), Times.Once);
            Assert.True(result);
            Assert.True(order.IsProcessed);
        }

        [Fact]
        public void ProcessOrder_WhenEmailSentAndDatabaseSaveSuccess_ReturnsTrue()
        {
            // Arrange
            var order = new Order { TotalAmount = 200, Id = 1, CustomerEmail = "customer@test.com" };
            _mockDatabase.Setup(db => db.IsConnected).Returns(true);

            // Act
            var result = _orderProcessor.ProcessOrder(order);

            // Assert
            _mockDatabase.Verify(db => db.Save(order), Times.Once);
            _mockEmailService.Verify(es => es.SendOrderConfirmation("customer@test.com", 1), Times.Once);
            Assert.True(result);
            Assert.True(order.IsProcessed);
        }
    }
}
