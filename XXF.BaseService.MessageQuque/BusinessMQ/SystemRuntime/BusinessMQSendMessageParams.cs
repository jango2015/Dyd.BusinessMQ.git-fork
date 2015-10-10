using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 消息发送参数封装
    /// </summary>
    public class BusinessMQSendMessageParams
    {
        public string manageconnectstring { get; set; }
        public string objjson {get;set;}
        public string mqpath { get; set; } 
    }
}
