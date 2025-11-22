using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ_9
{
    public interface IDatabase
    {
        bool IsConnected { get; }
        void Connect();
        void Save(Order order);
        Order GetOrder(int id);
    }
}
