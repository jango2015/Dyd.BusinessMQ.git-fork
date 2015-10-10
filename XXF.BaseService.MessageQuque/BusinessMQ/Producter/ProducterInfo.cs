using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.Producter.LoadBalance;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using XXF.BaseService.MessageQuque.Model;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    public class ProducterInfo
    {
        public tb_producter_model ProducterModel { get; set; }
        public tb_mqpath_model MqPathModel { get; set; }
        public List<tb_mqpath_partition_model> MqPathParitionModel { get; set; }
        public Dictionary<int, tb_datanode_model> DataNodeModelDic { get; set; }
        public BaseLoadBalance LoadBalance {get;set;}

        private object _operatorlock = new object();

        /// <summary>
        /// //顺序轮询节点获取节点及分区,从而达到负载均衡的目的
        /// </summary>
        /// <param name="sendmessagecount"></param>
        /// <returns></returns>
        public LoadBalanceNodeInfo GetLoadBalanceNodeInfo()
        {
            try
            { 
                lock (_operatorlock)
                {
                    LoadBalanceNodeInfo info = new LoadBalanceNodeInfo();
                    var p = LoadBalance.GetLoadBalancePartitionInfo();
                    if (p == null)
                        return null;
                    var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(p.PartitionId);
                    info.DataNodeModel = DataNodeModelDic[partitionidinfo.DataNodePartition]; info.MQPathPartitionModel = p.MQPathParitionModel;
                    return info;
                }
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(-1, "", "GetLoadBalanceNodeInfo", "生产者负载均衡出错", exp);
                throw exp;
            }
        }
        /// <summary>
        /// 更新生产者缓存信息
        /// </summary>
        /// <param name="productinfo"></param>
        public void Update(ProducterInfo productinfo)
        {
            lock (_operatorlock)
            {
                this.MqPathModel = productinfo.MqPathModel;
                this.MqPathParitionModel = productinfo.MqPathParitionModel;
                this.DataNodeModelDic = productinfo.DataNodeModelDic;
                this.LoadBalance.ClearError();
            }
        }
        /// <summary>
        /// 移除生产者某个节点相关的分区信息
        /// </summary>
        /// <param name="datanodepartition"></param>
        public void RemoveMQPathPartition(int datanodepartition)
        {
            lock (_operatorlock)
            {
                List<tb_mqpath_partition_model> remove = new List<tb_mqpath_partition_model>();
                foreach (var p in MqPathParitionModel)
                {
                    var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                    if (partitionidinfo.DataNodePartition == datanodepartition)
                    {
                        remove.Add(p);
                    }
                }
                foreach (var p in remove)
                    MqPathParitionModel.Remove(p);
            }
        }
    }
}
