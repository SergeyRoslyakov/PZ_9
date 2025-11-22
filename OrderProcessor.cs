using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ_9
{
    public class OrderProcessor
    {
        private readonly IDatabase _database;
        private readonly IEmailService _emailService;
        private const decimal EMAIL_SENDING_THRESHOLD = 100m;

        public OrderProcessor(IDatabase database, IEmailService emailService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public bool ProcessOrder(Order order)
        {
            ValidateOrder(order);

            if (!IsValidOrderAmount(order))
                return false;

            EnsureDatabaseConnection();

            return TryProcessOrder(order);
        }

        private void ValidateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
        }

        private bool IsValidOrderAmount(Order order)
        {
            return order.TotalAmount > 0;
        }

        private void EnsureDatabaseConnection()
        {
            if (!_database.IsConnected)
            {
                _database.Connect();
            }
        }

        private bool TryProcessOrder(Order order)
        {
            try
            {
                SaveOrderToDatabase(order);
                SendConfirmationEmailIfNeeded(order);
                MarkOrderAsProcessed(order);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SaveOrderToDatabase(Order order)
        {
            _database.Save(order);
        }

        private void SendConfirmationEmailIfNeeded(Order order)
        {
            if (order.TotalAmount > EMAIL_SENDING_THRESHOLD)
            {
                _emailService.SendOrderConfirmation(order.CustomerEmail, order.Id);
            }
        }

        private void MarkOrderAsProcessed(Order order)
        {
            order.IsProcessed = true;
        }
    }
}
