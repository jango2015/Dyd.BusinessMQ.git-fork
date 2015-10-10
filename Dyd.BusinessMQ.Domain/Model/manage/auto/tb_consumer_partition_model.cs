using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_consumer_partition Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_consumer_partition_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 消费者客户端ID
        /// </summary>
        public int consumerclientid { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int partitionindex { get; set; }
        
        /// <summary>
        /// 分区表ID
        /// </summary>
        public int partitionid { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public long lastconsumertempid { get; set; }
        
        /// <summary>
        /// 最后消费的MQID
        /// </summary>
        public long lastmqid { get; set; }
        
        /// <summary>
        /// 消费者最后执行时间(以当前库时间为准)
        /// </summary>
        public DateTime lastupdatetime { get; set; }
        
        /// <summary>
        /// 消费者分区创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}