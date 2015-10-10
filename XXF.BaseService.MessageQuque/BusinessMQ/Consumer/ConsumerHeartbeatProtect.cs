using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.Log;
using XXF.ProjectTool;
using XXF.Extensions;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者心跳守护
    /// </summary>
    public class ConsumerHeartbeatProtect : IDisposable
    {
        private CancellationTokenSource cancelSource;
        private Consumer.ConsumerContext Context;
        private DateTime _lastupdatetimeofmqpath;
        private object _heartbeatrunlock = new object();
        private RedisNetCommandListener redislistener = null;
        public ConsumerHeartbeatProtect(ConsumerContext context)
        {
            try
            {
               
                Context = context;
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ConsumerHeartbeatProtect", "消费者准备心跳注册");

                _lastupdatetimeofmqpath = GetLastUpdateTimeOfMqPath();
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect", "消费者获取队列最后更新时间成功");

                cancelSource = new CancellationTokenSource();
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    HeartBeatRun();//开启心跳检查
                }, cancelSource.Token);
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect", "消费者开启心跳循环成功");

                redislistener = new RedisNetCommandListener(ConfigHelper.RedisServer); redislistener.Name = "消费者" + context.ConsumerProvider.Client;
                redislistener.Register((channel, msg) =>
                {
                    RedisListenerCommand(channel, msg);
                }, cancelSource, Context.ConsumerProvider.MQPath, SystemParamConfig.Redis_Channel);
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect", "消费者开启Redis消息订阅成功");
                LogHelper.WriteLine(Context.ConsumerInfo.MQPathModel.id, Context.ConsumerInfo.MQPathModel.mqpath, "ConsumerHeartbeatProtect", "消费者心跳注册成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect", "消费者心跳守护初始化", exp);
                throw exp;
            }
        }

        //心跳循环
        private void HeartBeatRun()
        {
            try
            {
                while (!cancelSource.IsCancellationRequested)
                {
                    HeartBeatTask();
                    System.Threading.Thread.Sleep(SystemParamConfig.Consumer_HeatBeat_Every_Time * 1000);
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-HeartBeatRun", "心跳循环一轮完毕");
                }
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.ConsumerInfo.MQPathModel.id, Context.ConsumerInfo.MQPathModel.mqpath, "HeartBeatRun", "消费者心跳循环方法", exp);
            }
        }
        //心跳任务
        private void HeartBeatTask()
        {
            lock (_heartbeatrunlock)
            {
                try
                {
                    //更新节点心跳
                    DB.ConsumerBLL bll = new DB.ConsumerBLL();
                    SqlHelper.ExcuteSql(Context.ConsumerProvider.Config.ManageConnectString, (c) =>
                    {
                        bll.ConsumerHeartbeat(c, Context.ConsumerInfo.ConsumerModel.tempid, Context.ConsumerInfo.ConsumerModel.consumerclientid);
                    });
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-HeartBeatTask", "更新节点心跳完毕");
                    //检查当前队列是否有更新，有更新则重启customer
                    var lastupdatetime = GetLastUpdateTimeOfMqPath();
                    if (_lastupdatetimeofmqpath < lastupdatetime||Context.IsNeedReload == true)
                    {
                        Context.IsNeedReload = false;
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-HeartBeatTask", "检测到队列更新准备重启");
                        //Context.ConsumerProvider.ReRegister();
                        Context.ConsumerProvider.Dispose();
                        MethodInfo method = typeof(ConsumerProvider).GetMethod("RegisterReceiveMQListener", BindingFlags.Instance | BindingFlags.Public);
                        method = method.MakeGenericMethod(Context.ActionInfo.ReturnType);
                        method.Invoke(Context.ConsumerProvider, new object[] { Context.ActionInfo.Action });
                        _lastupdatetimeofmqpath = lastupdatetime;
                        redislistener.RedisServerIp = ConfigHelper.RedisServer;
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-HeartBeatTask", "检测到队列更新重启成功");
                        LogHelper.WriteLine(Context.ConsumerInfo.MQPathModel.id, Context.ConsumerInfo.MQPathModel.mqpath, "队列更新重启", "消费者检测到队列更新重启成功");
                    }
                }
                catch (Exception exp)
                {
                    ErrorLogHelper.WriteLine(Context.ConsumerInfo.MQPathModel.id, Context.ConsumerInfo.MQPathModel.mqpath, "HeartBeatTask", "消费者心跳循环出错", exp);
                }
            }

        }

        private void RedisListenerCommand(string channel, string msg)
        {
            try
            {
                if (!cancelSource.IsCancellationRequested)
                { 
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-RedisListenerCommand", "检测到redis消息,msg:"+msg.NullToEmpty());
                var command = new XXF.Serialization.JsonHelper().Deserialize<BusinessMQNetCommand>(msg);
                if ((command.CommandReceiver == EnumCommandReceiver.All || command.CommandReceiver == EnumCommandReceiver.Consumer) && command.MqPath.ToLower() == Context.ConsumerProvider.MQPath.ToLower())
                {
                    if (command.CommandType == EnumCommandType.Register)
                    {
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(),"ConsumerHeartbeatProtect-RedisListenerCommand", "检测到redis消息:" + EnumCommandType.Register.Tostring());
                        Context.IsNeedReload = true;
                        HeartBeatTask();
                    }
                }
                }
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.ConsumerInfo.MQPathModel.id, Context.ConsumerInfo.MQPathModel.mqpath, "RedisListenerCommand", "消费者Redis注册监听", exp);
            }
        }

        private DateTime GetLastUpdateTimeOfMqPath()
        {
            DB.ConsumerBLL bll = new DB.ConsumerBLL(); DateTime dt = DateTime.MinValue;
            SqlHelper.ExcuteSql(Context.ConsumerProvider.Config.ManageConnectString, (c) =>
            {
                var t = bll.GetLastUpdateTimeOfMqPath(c, Context.ConsumerProvider.MQPath);
                if(t == null)
                    throw new BusinessMQException(string.Format("检测到队列{0}最后更新时间为null", Context.ConsumerProvider.MQPath));
                dt = t.Value;
            });
            return dt;
        }

        public void Dispose()
        {
            if (redislistener != null)
                redislistener.Dispose();
            if (cancelSource != null)
                cancelSource.Cancel();

        }
    }
}
