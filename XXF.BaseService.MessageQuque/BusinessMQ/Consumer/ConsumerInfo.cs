using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者信息
    /// </summary>
    public class ConsumerInfo
    {
        /// <summary>
        /// 消费者客户端Client相关信息
        /// </summary>
        public tb_consumer_client_model ConsumerClientModel { get; set; }
        /// <summary>
        /// 消费者信息
        /// </summary>
        public tb_consumer_model ConsumerModel { get; set; }
        /// <summary>
        /// 消费者当前使用的可用分区信息
        /// </summary>
        public List<tb_consumer_partition_model> ConsumerPartitionModels { get; set; }
        /// <summary>
        /// 消费者相关分区节点信息
        /// </summary>
        public Dictionary <int,tb_datanode_model> DataNodeModelDic { get; set; }
        /// <summary>
        /// 消费者订阅队列信息
        /// </summary>
        public tb_mqpath_model  MQPathModel { get; set; }

    }
    /// <summary>
    /// 消费者注册回调相关信息
    /// </summary>
    public class ConsumerActionInfo
    {
        /// <summary>
        /// 注册的回调地址Aciton
        /// </summary>
        public object Action { get; set; }
        /// <summary>
        /// 消费者注册的消息实体类型
        /// </summary>
        public Type ReturnType { get; set; }
        /// <summary>
        /// 内部的消息回调Action
        /// </summary>
        public Action<MQMessage> InnerAction { get; set; }
    }
}
