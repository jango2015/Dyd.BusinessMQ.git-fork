using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    public class SystemParamConfig
    {
        ///// <summary>
        ///// 消费者接收消息队列接收消息中间间隔时间 单位:s
        ///// </summary>
        //public static int Consumer_ReceiveMessageListener_SLEEP_TIME_WHILE_NO_MESSAGE = 5;
        /// <summary>
        /// 消费者接收消息队列每次拉取的消息量
        /// </summary>
        public static int Consumer_ReceiveMessageQuque_EVERY_PULL_COUNT = 100;
        ///// <summary>
        ///// 消费者接收消息队列当前队列最小长度
        ///// </summary>
        //public static int Consumer_ReceiveMessageQuque_MIN_QUQUE_LENGTH = 2000;
        /// <summary>
        /// 消费者接收消息队列消息小于每次拉取的消息量时,睡眠时间 单位:s
        /// </summary>
        public static int Consumer_ReceiveMessageQuque_Every_Sleep_Time = 10;
        /// <summary>
        /// 消费者端的心跳超时时间
        /// </summary>
        public static int Consumer_ConsumerHeartbeat_MAX_TIME_OUT = 10;
        /// <summary>
        /// 是否开启消费者端的不加锁读取消息（withnolock）
        /// </summary>
        public static bool Consumer_ReadMessage_WithNolock = true;
        /// <summary>
        /// 尝试设置消息已读失败重试次数
        /// </summary>
        public static int Consumer_TrySetMessageRead_FailCount = 3;
        /// <summary>
        /// 尝试设置消息已读失败重试睡眠时间 s
        /// </summary>
        public static double Consumer_TrySetMessageRead_ErrorSleepTime = 1;
        /// <summary>
        /// 消费者数据节点数据库连接字符串模板
        /// (消费者端不设置超时，使用默认，当数据节点故障，连接超时会有10秒多的停顿，但是不会影响整体性能，这种情况是合理的)
        /// </summary>
        public static string Consumer_DataNode_ConnectString_Template = "server={server};Initial Catalog={database};User ID={username};Password={password};Max Pool Size=1;Pooling=true;";
        /// <summary>
        /// 数据节点数据库名前缀
        /// </summary>
        public static string DataNode_DataBaseName_Prefix = "dyd_bs_MQ_datanode_";       
        /// <summary>
        /// 消费者心跳时间 单位:s
        /// </summary>
        public static int Consumer_HeatBeat_Every_Time = 5;
        /// <summary>
        /// 内部Redis发布订阅通讯最大连接池
        /// </summary>
        public static int Redis_MaxConnectPoolSize = 2;
        /// <summary>
        /// 内部Redis发布订阅通讯通道名
        /// </summary>
        public static string Redis_Channel = "BusinessMQ.Redis.Channel";
        /// <summary>
        /// 队列更新Redis发布订阅通道名
        /// </summary>
        public static string Redis_Channel_Quque = "BusinessMQ.Redis.Quque";
        /// <summary>
        /// Redis发布订阅通讯通道注册失败间隔重试时间
        /// </summary>
        public static int Redis_Subscribe_FailConnect_ReConnect_Every_Time = 5;
        /// <summary>
        /// 生产者心跳时间 单位:s
        /// </summary>
        public static int Producter_HeatBeat_Every_Time = 10;
        /// <summary>
        /// 生产者端的心跳超时时间 s (用于清理超时的生产者)
        /// </summary>
        public static int Producter_Heartbeat_MAX_TIME_OUT = 60;
        /// <summary>
        /// 生产者发送消息错误后,自动重启解决错误的处理间隔时间，便于清理错误，重置状态
        /// </summary>
        public static int Producter_SendError_Clear_Time = 60;
        /// <summary>
        /// 生产者发送消息错误后,消息重试发送次数
        /// </summary>
        public static int Producter_SendMessageError_TryAgainCount = 5;
        /// <summary>
        /// 生产者数据节点数据库连接字符串模板(超过1秒未连接节点，视为节点异常,进行故障转移),仅发送消息时使用
        /// </summary>
        public static string Producter_DataNode_ConnectString_Template_ToSendMessage = "server={server};Initial Catalog={database};User ID={username};Password={password};Max Pool Size=40;Pooling=true;connection timeout=1";
       
    }
}
