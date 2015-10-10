using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace Dyd.BusinessMQ.Web.Base
{
    public class RedisHelper
    {
        public static void SendMessage(string redisserverip, string channal,BusinessMQNetCommand command)
        {
            var manager = new XXF.Redis.RedisManager();
            using (var c = manager.GetPoolClient(redisserverip, 1, 1))
            {
                var i = c.GetClient().PublishMessage(channal, new XXF.Serialization.JsonHelper().Serializer(command));
            }
        }
    }
}