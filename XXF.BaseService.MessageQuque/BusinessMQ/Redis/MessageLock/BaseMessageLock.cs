using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Redis.MessageLock
{
    /// <summary>
    /// 消息合并，消息锁定
    /// 主要为是了减少redis的消息通信和消息处理.(目前机制尚不能完全解决和消息合并，但是可以达到较好效果)
    /// </summary>
    public class BaseMessageLock
    {
        protected object _lock = new object();//对象锁
        protected bool isLock = false;//是否锁定对象
        protected bool isHaveNewMessage = false;//是否有新消息
        protected TimeSpan mergeTime;//合并多少时间内消息

        public BaseMessageLock(TimeSpan mergetime)
        {
            mergeTime = mergetime;
        }

        public virtual void Lock(Action action) { }

        protected void LockRun(Action action)
        {
            lock (_lock)//并发锁,进行并发合并
            {
                try
                {
                    isLock = true;
                    while (isHaveNewMessage == true)//500ms内的请求合并
                    {
                        isHaveNewMessage = false;
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception exp)
                        {
                            throw exp;
                        }
                        finally
                        {
                            System.Threading.Thread.Sleep((int)mergeTime.TotalMilliseconds);
                        }
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    isLock = false;
                }
            }

        }

    }
}
