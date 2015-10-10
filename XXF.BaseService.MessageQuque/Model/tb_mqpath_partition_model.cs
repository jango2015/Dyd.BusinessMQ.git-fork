using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace XXF.BaseService.MessageQuque.Model
{
    /// <summary>
    /// tb_mqpath_partition Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_mqpath_partition_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 某路径下的mq的id
        /// </summary>
        public int mqpathid { get; set; }
        
        /// <summary>
        /// 分区id编号
        /// </summary>
        public int partitionid { get; set; }
        
        /// <summary>
        /// 分区顺序号(某个路径下mq的顺序号)
        /// </summary>
        public int partitionindex { get; set; }
        
        /// <summary>
        /// 某路径下mq的状态,1 运行中，0 待数据迁移或停止，-1 待删除
        /// </summary>
        public Byte state { get; set; }
        
        /// <summary>
        /// 创建时间(以当前库时间为准)
        /// </summary>
        public DateTime createtime { get; set; }
        
    }
}