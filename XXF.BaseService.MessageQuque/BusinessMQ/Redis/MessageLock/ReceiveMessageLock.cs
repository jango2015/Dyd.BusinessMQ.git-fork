using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis.MessageLock;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Redis
{
    /// <summary>
    /// 接收消息合并
    /// 接收消息处理允许阻塞
    /// </summary>
    public class ReceiveMessageLock : BaseMessageLock
    {
        public ReceiveMessageLock(TimeSpan mergetime)
            : base(mergetime)
        {
           
        }
        public override void Lock(Action action)
        {
            isHaveNewMessage = true;
            if (isLock==false)//判断当前是否并发，并发情况下则跳过，视为请求合并
            {
                LockRun(action);//同步
            }
        }

    }
}
