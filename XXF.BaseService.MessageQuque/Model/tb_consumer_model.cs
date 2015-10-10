using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace XXF.BaseService.MessageQuque.Model
{
    /// <summary>
    /// tb_consumer Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_consumer_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 消费者临时id(消费者启动后唯一,Guid转long)
        /// </summary>
        public long tempid { get; set; }
        
        /// <summary>
        /// 消费者clinet的id
        /// </summary>
        public int consumerclientid { get; set; }
        
        /// <summary>
        /// 支持的分区顺序号(支持多个顺序号)
        /// </summary>
        public string partitionindexs { get; set; }
        
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string clientname { get; set; }
        
        /// <summary>
        /// 最后心跳时间(以当前库时间为准)
        /// </summary>
        public DateTime lastheartbeat { get; set; }
        
        /// <summary>
        /// 上一次更新时间(以当前库时间为准)
        /// </summary>
        public DateTime lastupdatetime { get; set; }
        
        /// <summary>
        /// 客户端创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}