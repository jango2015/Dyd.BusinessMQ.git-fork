using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace XXF.BaseService.MessageQuque.Model
{
    /// <summary>
    /// tb_mqerror Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_mqerror_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public long ID { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Byte TryCount { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Byte MQType { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string MQPath { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string MQMsgJson { get; set; }
        
    }
}