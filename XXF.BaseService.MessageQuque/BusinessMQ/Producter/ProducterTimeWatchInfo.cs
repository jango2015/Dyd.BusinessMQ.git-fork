using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    public class ProducterTimeWatchInfo
    {
        public double GetLoadBalanceNodeInfo = 0;
        public double JsonHelperSerializer = 0;
        public double SendMessage = 0;
    }

    public class ProducterTimeWatchTest
    {
        public static List<string> Messages = new List<string>();
        public static void AddMessages(string message)
        {
            Messages.Add(message);
        }
    }
}
