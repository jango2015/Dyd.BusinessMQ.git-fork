using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Common
{
    /// <summary>
    /// 业务消息响应
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BusinessMQResponse<T>
    {
        /// <summary>
        /// 消息响应实体
        /// </summary>
        public T ObjMsg { get; set; }
        /// <summary>
        /// 消息内部响应
        /// </summary>
        public object InnerObject { get; set; }
        /// <summary>
        /// 标记消息已处理,请在消息处理成功后调用,否则消息在下次重启或消息循环传递终止
        /// </summary>
        public void MarkFinished()
        {
            var innerobject = InnerObject as MQMessage;
            innerobject.MarkFinished();
            
        }
    }
}
