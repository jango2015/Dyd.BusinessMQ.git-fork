using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_messagequeue Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_messagequeue_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 消息id号,规则1+数据节点编号+表分区编号+时间分区号+自增id
        /// </summary>
        public long id { get; set; }
        
        /// <summary>
        /// mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
        /// </summary>
        public DateTime mqcreatetime { get; set; }
        
        /// <summary>
        /// sql数据节点处的创建时间
        /// </summary>
        public DateTime sqlcreatetime { get; set; }
        
        /// <summary>
        /// 消息类型,0=可读消息，1=已迁移消息
        /// </summary>
        public Byte state { get; set; }
        
        /// <summary>
        /// 来源类型:0 表示 正常发送,1 表示 迁移消息
        /// </summary>
        public Byte source { get; set; }
        
        /// <summary>
        /// 消息体（消息内容,以json形式存储，为了阅读考虑）
        /// </summary>
        public string message { get; set; }
        
    }
}