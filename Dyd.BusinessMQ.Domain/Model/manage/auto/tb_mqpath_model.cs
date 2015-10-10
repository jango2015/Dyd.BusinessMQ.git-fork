using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_mqpath Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_mqpath_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// mq路径
        /// </summary>
        public string mqpath { get; set; }
        
        /// <summary>
        /// 该路径下mq,配置最后更新时间(以当前库时间为准)
        /// </summary>
        public DateTime lastupdatetime { get; set; }
        
        /// <summary>
        /// mq创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}