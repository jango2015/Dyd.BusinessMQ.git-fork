using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_partition_messagequeue_report Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_partition_messagequeue_report_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 分区编号
        /// </summary>
        public int partitionid { get; set; }
        
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime day { get; set; }
        
        /// <summary>
        /// 分区最大消息id
        /// </summary>
        public long mqmaxid { get; set; }
        
        /// <summary>
        /// mq最小消息id
        /// </summary>
        public long mqminid { get; set; }
        
        /// <summary>
        /// 消息数量
        /// </summary>
        public int mqcount { get; set; }
        
        /// <summary>
        /// 当前分区扫描最后更新时间
        /// </summary>
        public DateTime lastupdatetime { get; set; }
        
        /// <summary>
        /// 当前分区扫描创建时间
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}