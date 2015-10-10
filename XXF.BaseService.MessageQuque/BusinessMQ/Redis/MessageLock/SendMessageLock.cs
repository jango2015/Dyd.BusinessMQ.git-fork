using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis.MessageLock;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 发送消息锁
    /// 发送消息合并,合并n时间内的请求
    /// 发送消息不允许阻塞
    /// </summary>
    public class SendMessageLock:BaseMessageLock
    {
        private Action<Action> lockRunAction;
        public SendMessageLock(TimeSpan mergetime):base(mergetime)
        {
            lockRunAction = (c) => { LockRun(c); };
        }

        public override void Lock(Action action)
        {
            isHaveNewMessage=true;
            if (isLock==false)//判断当前是否并发，并发情况下则跳过，视为请求合并
            {
                lockRunAction.BeginInvoke(action,null,null);//异步
            }
        }

        
    }
}
