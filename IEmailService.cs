using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ_9
{
    public interface IEmailService
    {
        void SendOrderConfirmation(string customerEmail, int orderId);
    }
}
