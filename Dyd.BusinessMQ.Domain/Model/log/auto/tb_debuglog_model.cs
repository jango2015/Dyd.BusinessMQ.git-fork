using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_debuglog Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_debuglog_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public long id { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int mqpathid { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string mqpath { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string methodname { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string info { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}