using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 网络命令
    /// </summary>
    public class BusinessMQNetCommand
    {
       public EnumCommandType CommandType {get;set;}
       public EnumCommandReceiver CommandReceiver { get; set; }
       public string MqPath { get; set; }
    }
    /// <summary>
    /// 网络命令类型
    /// </summary>
    public enum EnumCommandType
    {
        Register
    }
    /// <summary>
    /// 网络命令接收者类型
    /// </summary>
    public enum EnumCommandReceiver
    {
         All=0,
         Consumer=1,
         Producter=2,      
    }
}
