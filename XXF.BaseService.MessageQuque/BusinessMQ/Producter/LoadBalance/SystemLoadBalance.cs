using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter.LoadBalance
{
    public class SystemLoadBalance:BaseLoadBalance
    {
        protected int SendMessageCount = 0;
        public override LoadBalancePartitionInfo GetLoadBalancePartitionInfo()
        {
            System.Threading.Interlocked.Increment(ref SendMessageCount);
            if (SendMessageCount > 1000000000)
                SendMessageCount = 1;

            if (this.MQPathParitionModels.Count > 0)
            {
                LoadBalanceNodeInfo info = new LoadBalanceNodeInfo();
                int index = (SendMessageCount - 1) % MQPathParitionModels.Count; var partion = MQPathParitionModels[index]; var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(partion.partitionid);
                return new LoadBalancePartitionInfo() { PartitionId = partion.partitionid, PartitionIndex = partion.partitionindex, MQPathParitionModel = partion };
            }
            else
            {
                ErrorLogHelper.WriteLine(-1, "", "SystemLoadBalance-LoadBalancePartitionInfo", "系统默认生产者负载均衡出错:当前可用分区数为0",new Exception());
                return null;
            }
        }
    }
}
