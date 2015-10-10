using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    ///// <summary>
    ///// 消息类型
    ///// </summary>
    //public enum EnumMessageType
    //{
    //    Json=0,
    //    Text=1,
    //}
    /// <summary>
    /// 枚举消息状态
    /// </summary>
    public enum EnumMessageState
    {
        [Description("可读")]
        CanRead=0,
        /// <summary>
        /// 1=
        /// </summary>
        [Description("已迁移")]
        Moved=1,
        /// <summary>
        /// 
        /// </summary>
        [Description("已删除")]
        Deleted=2,
        
    }
    /// <summary>
    /// 枚举消息的来源
    /// </summary>
    public enum EnumMessageSource
    {
        ///0=
        [Description("正常")]
        Common = 0,
        ///1=
        [Description("已迁移")]
        Moved = 1
    }
    /// <summary>
    /// 系统配置的key枚举
    /// </summary>
    public enum EnumSystemConfigKey
    {
        RedisServer,
        DebugMqpath,
        LogDBConnectString,
        MQCreateTableSql,
    }
    /// <summary>
    /// MQPath分区类型枚举
    /// </summary>
    public enum EnumMqPathPartitionState
    {
        [Description("活动")]
        Running=0,

        [Description("待删")]
        WaitConsumeCompleted=100,
    }
}
