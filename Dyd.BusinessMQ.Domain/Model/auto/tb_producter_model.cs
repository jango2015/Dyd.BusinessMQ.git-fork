using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_producter Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_producter_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 生产者临时id(消费者启动后唯一,Guid转long)
        /// </summary>
        public long tempid { get; set; }
        
        /// <summary>
        /// 生产者名称
        /// </summary>
        public string productername { get; set; }
        
        /// <summary>
        /// ip地址
        /// </summary>
        public string ip { get; set; }
        
        /// <summary>
        /// 队列id
        /// </summary>
        public int mqpathid { get; set; }
        
        /// <summary>
        /// 生产者最后心跳时间
        /// </summary>
        public DateTime lastheartbeat { get; set; }
        
        /// <summary>
        /// 生产者创建时间
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}