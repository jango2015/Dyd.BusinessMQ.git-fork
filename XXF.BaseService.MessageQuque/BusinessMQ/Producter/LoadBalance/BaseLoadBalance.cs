using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter.LoadBalance
{
    public abstract class BaseLoadBalance
    {
        /// <summary>
        /// 所有历史出错的分区信息（程序启动后所有的出错分区信息）
        /// </summary>
        protected Dictionary<string, ErrorLoadBalancePartitionInfo> allErrorHistoryPartitionInfos = new Dictionary<string, ErrorLoadBalancePartitionInfo>();
        /// <summary>
        /// 当前出错的分区信息（生产者心跳后,错误的分区将被清空，重新尝试使用，以便故障恢复）
        /// </summary>
        protected Dictionary<string, ErrorLoadBalancePartitionInfo> currentErrorPartitionInfos = new Dictionary<string, ErrorLoadBalancePartitionInfo>();
        /// <summary>
        /// 当前可用的分区信息（故障转移的分区，将被移除）
        /// </summary>
        protected List<tb_mqpath_partition_model> MQPathParitionModels { get { return Context.ProducterInfo.MqPathParitionModel; } }

        public ProducterContext Context { get; set; }

        protected object _errorlog = new object();

        public int SendMessageErrorTryAgainCount = SystemRuntime.SystemParamConfig.Producter_SendMessageError_TryAgainCount;
        

        public virtual LoadBalancePartitionInfo GetLoadBalancePartitionInfo()
        {
            return null;
        }

        public virtual void AddError(ErrorLoadBalancePartitionInfo info)
        {
            lock(_errorlog)
            {
                if (!currentErrorPartitionInfos.ContainsKey(info.HashCode()))
                {
                    currentErrorPartitionInfos.Add(info.HashCode(),info);
                }
                if (!allErrorHistoryPartitionInfos.ContainsKey(info.HashCode()))
                {
                    allErrorHistoryPartitionInfos.Add(info.HashCode(), info);
                }
            }
        }

        public virtual void ClearError()
        {
            lock (_errorlog)
            {
                currentErrorPartitionInfos.Clear();
            }
        }
    }

    public class LoadBalancePartitionInfo
    {
        public int PartitionId {get;set;}
        public int PartitionIndex{get;set;}
        public tb_mqpath_partition_model MQPathParitionModel { get; set; }
    }

    public class ErrorLoadBalancePartitionInfo
    {
        public int PartitionId { get; set; }
        public int PartitionIndex { get; set; }

        public string HashCode()
        {
            return PartitionId + "_" + PartitionIndex;
        } 
    }
}
