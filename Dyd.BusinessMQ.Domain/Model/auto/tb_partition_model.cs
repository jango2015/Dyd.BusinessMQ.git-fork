using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Dyd.BusinessMQ.Domain.Model
{
    /// <summary>
    /// tb_partition Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_partition_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 分区id号,规则1+数据节点编号+表分区编号
        /// </summary>
        public int partitionid { get; set; }
        
        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool isused { get; set; }
        
        /// <summary>
        /// 创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}