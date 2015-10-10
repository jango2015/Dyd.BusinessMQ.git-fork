using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 生产者负载均衡信息
    /// </summary>
    public class LoadBalanceNodeInfo
    {
        public tb_datanode_model DataNodeModel { get; set; }
        public tb_mqpath_partition_model MQPathPartitionModel { get; set; }
    }
}
