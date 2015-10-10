using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_consumer_client Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_consumer_client_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 客户端（消费者client，相同业务消费者注册必须一致）
        /// </summary>
        public string client { get; set; }
        
        /// <summary>
        /// 当前消费者创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}