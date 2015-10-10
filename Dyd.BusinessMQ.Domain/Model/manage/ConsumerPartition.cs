using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BusinessMQ.Domain.Model.manage
{
    public class ConsumerPartition
    {
        public int PartitionId { get; set; }
        public bool IsOnline { get; set; }
        public int NonMsg { get; set; }
        public int Msg { get; set; }
        public int ConsumerId { get; set; }
        public string ConsumerName { get; set; }
        public bool IsOnlineConsumer { get; set; }
    }
}
