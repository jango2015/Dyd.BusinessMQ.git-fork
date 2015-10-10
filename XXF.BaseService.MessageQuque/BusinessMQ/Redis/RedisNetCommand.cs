using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis.MessageLock;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// redis网络命令发送
    /// </summary>
    public class RedisNetCommand
    {
        private string redisServerIp;
        private BaseMessageLock messagelock = new SendMessageLock(TimeSpan.FromMilliseconds(500));

        public RedisNetCommand(string rediserverip)
        {
            redisServerIp = rediserverip;
        }
        /// <summary>
        /// 可丢消息发送,当前消息并发情况下丢弃
        /// </summary>
        /// <param name="mqpath"></param>
        public void SendMessage(string mqpath)
        {

            messagelock.Lock(() =>
            {
                try
                {
                    var manager = new XXF.Redis.RedisManager();
                    using (var c = manager.GetPoolClient(redisServerIp, SystemParamConfig.Redis_MaxConnectPoolSize, SystemParamConfig.Redis_MaxConnectPoolSize))
                    {
                        var i = c.GetClient().PublishMessage(SystemParamConfig.Redis_Channel_Quque + "." + mqpath, "1");
                    }
                }
                catch (Exception exp)
                {

                }
            });
        }
       

    }
}
