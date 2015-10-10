using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Log;
using XXF.ProjectTool;
using XXF.Extensions;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using ServiceStack.Redis;
using XXF.Redis;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Redis
{
    /// <summary>
    /// Redis 网络命令监听器
    /// </summary>
    public class RedisNetCommandListener : IDisposable
    {
        public string RedisServerIp;
        private CancellationTokenSource cancelSource;
        private RedisDb redisDb;//当前监听连接
        private string mqPath;
        private string channelName;
        private bool isdisposeing = false;//监听释放标记
        public string Name;

        public RedisNetCommandListener(string redisserverip)
        {
            RedisServerIp = redisserverip;
        }

        public void Register(Action<string, string> action, CancellationTokenSource cancelsource, string mqpath, string channel)
        {
            cancelSource = cancelsource; mqPath = mqpath; channelName = channel;
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                NetSubscribe(action, mqpath, channel);//开启及时网络订阅
            }, cancelSource.Token);
        }

        private void NetSubscribe(Action<string, string> action, string mqpath, string channel)
        {
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        CloseRedisClient();
                        RedisSubscribe(action, mqpath, channel);
                    }
                    catch (Exception exp)
                    {
                        if (isdisposeing == false)
                            ErrorLogHelper.WriteLine(-1, mqpath, "NetSubscribe", string.Format("MQ心跳redis订阅通信消息出错,请检查redis服务器,订阅名:{0}", Name), exp);
                    }
                    System.Threading.Thread.Sleep(SystemParamConfig.Redis_Subscribe_FailConnect_ReConnect_Every_Time * 1000);
                }
                catch (Exception exp)
                {
                    // when thread is sleeping,but we cancel this thread,may throw thread abort error 
                }
            }
        }

        private void RedisSubscribe(Action<string, string> action, string mqpath, string channelname)
        {
            var manager = new XXF.Redis.RedisManager();
            redisDb = manager.CreateClient(RedisServerIp.Split(':')[0], Convert.ToInt32(RedisServerIp.Split(':')[1]), "");
            using (var subscription = redisDb.GetClient().CreateSubscription())
            {
                subscription.OnSubscribe = channel =>
                {

                    //订阅事件
                };
                subscription.OnUnSubscribe = channel =>
                {

                    //退订事件
                };
                subscription.OnMessage = (channel, msg) =>
                {
                    try
                    {
                        if (msg == "RedisNetCommandListener-Close" || isdisposeing==true)//关闭通道
                        {
                            try { subscription.UnSubscribeFromChannels(channel); }
                            catch { }
                        }
                        else
                        { 
                            if (action != null)
                                action.Invoke(channel, msg);
                        }
                    }
                    catch (Exception exp)
                    {
                        ErrorLogHelper.WriteLine(-1, mqpath, "RedisSubscribe", string.Format("MQredis订阅通信消息,通道:{1},处理消息{0},订阅名:{2}出错", msg.NullToEmpty(), channelname, Name), exp);
                    }
                };
                subscription.SubscribeToChannels(channelname);
            }
        }

        private void CloseRedisClient()
        {
            try
            {
                if (redisDb != null)
                {

                    if (redisDb.GetClient() != null)
                    {
                        redisDb.GetClient().Quit();
                    }
                    //redisDb.Dispose();
                    redisDb = null;
                }
            }
            catch (Exception exp)
            { }
        }

        public void Dispose()
        {
            isdisposeing = true;
            try
            {
                try
                {
                    //通知关闭监听通道
                    if (redisDb != null)
                    {
                        var manager = new XXF.Redis.RedisManager();
                        using (var db = manager.CreateClient(RedisServerIp.Split(':')[0], Convert.ToInt32(RedisServerIp.Split(':')[1]), ""))
                        {
                            db.GetClient().PublishMessage(channelName, "RedisNetCommandListener-Close");
                            db.GetClient().Quit();
                        }
                    }
                }
                catch { }
                //关闭当前连接
                CloseRedisClient();
            }
            catch (Exception exp)
            { }
        }
    }
}
