using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BusinessMQ.Domain.Model.manage
{
    public class ConsumerPartitionModel
    {
        public tb_consumer_partition_model consumerpartitionmodel;
        public string client;
        public long msgCount { get; set; }
        public long nonMsgCount { get; set; }
    }
}
