using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_config Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_config_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 配置Key
        /// </summary>
        public string key { get; set; }
        
        /// <summary>
        /// 配置Value
        /// </summary>
        public string value { get; set; }
        
        /// <summary>
        /// 配置备注信息
        /// </summary>
        public string remark { get; set; }
        
    }
}