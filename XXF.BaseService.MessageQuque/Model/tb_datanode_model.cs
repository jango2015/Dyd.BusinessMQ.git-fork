using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace XXF.BaseService.MessageQuque.Model
{
    /// <summary>
    /// tb_datanode Data Structure.
    /// </summary>
    [Serializable]
    public partial class tb_datanode_model
    {
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
        
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int datanodepartition { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string serverip { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string username { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string password { get; set; }
        
    }
}