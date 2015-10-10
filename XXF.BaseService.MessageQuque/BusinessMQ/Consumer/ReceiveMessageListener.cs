using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using XXF.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者监听器
    /// </summary>
    public class ReceiveMessageListener:IDisposable
    {
        private ConsumerHeartbeatProtect heartbeat = null;
        /// <summary>
        /// 注册监听器
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxReceiveMQThread"></param>
        /// <param name="context"></param>
        public virtual void Register(Action<MQMessage> action,ConsumerContext context)
        {
            DebugHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(), "ReceiveMessageListener", "消费者开始监听器");
            context.Listener = this; context.ActionInfo.InnerAction = action;
            var quque = new ReceiveMessageQuque(context);//注册队列
            heartbeat = new ConsumerHeartbeatProtect(context);//注册心跳
            DebugHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(), "ReceiveMessageListener", "消费者监听器注册成功");
            LogHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(), "ReceiveMessageListener", "消费者监听器注册成功");
        }

        public void Dispose()
        {
            if (heartbeat != null)
                heartbeat.Dispose();
        }
    }
}
