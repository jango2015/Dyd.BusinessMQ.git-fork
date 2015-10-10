using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.DB;
using XXF.Log;
using XXF.ProjectTool;
using XXF.Extensions;
using System.Collections.Concurrent;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 生产者心跳守护
    /// </summary>
    public class ProducterHeartbeatProtect : IDisposable
    {
        /// <summary>
        /// 上下文集合,多线程安全集合
        /// </summary>
        public SynchronousContextList Contexts = new SynchronousContextList();//上下文集合,安全集合

        private CancellationTokenSource cancelSource;
        private static ProducterHeartbeatProtect producterHeartbeatProtect = null;//单例实例
        private static object _instancelock = new object();//单例实例锁
        private static object _contextupdatelock = new object();//上下文更新锁

        private RedisNetCommandListener redislistener = null;

        private ProducterHeartbeatProtect(ProducterContext context)
        {
            try
            {
                DebugHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"ProducterHeartbeatProtect", "生产者心跳守护开始注册");
                cancelSource = new CancellationTokenSource();

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    HeatbeatRun();//注册心跳
                }, cancelSource.Token);
                DebugHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"ProducterHeartbeatProtect", "生产者心跳守护心跳注册成功");

                redislistener = new RedisNetCommandListener(ConfigHelper.RedisServer); redislistener.Name = "生产者";
                redislistener.Register((channel, msg) =>
                {
                    RedisListenerCommand(channel, msg);
                }, cancelSource, context.ProducterProvider.MQPath, SystemParamConfig.Redis_Channel);
                DebugHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"ProducterHeartbeatProtect", "生产者心跳redis监听注册成功");
                LogHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"ProducterHeartbeatProtect", "生产者心跳守护注册成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"ProducterHeartbeatProtect", "生产者心跳初始化错误", exp);
                throw exp;
            }

        }
        /// <summary>
        /// 单例实例
        /// </summary>
        /// <returns></returns>
        public static ProducterHeartbeatProtect Instance(ProducterContext currentcontext)
        {
            if (producterHeartbeatProtect != null)
                return producterHeartbeatProtect;
            lock (_instancelock)
            {
                producterHeartbeatProtect = new ProducterHeartbeatProtect(currentcontext);
                return producterHeartbeatProtect;
            }
        }

        private void HeatbeatRun()
        {
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    foreach (var context in Contexts.CopyToList())
                    {
                        ProducterBLL bll = new ProducterBLL(); 
                        SqlHelper.ExcuteSql(context.ProducterProvider.Config.ManageConnectString, (c) =>
                        {
                            context.ManageServerTime = c.GetServerDate();//重新校准时间
                            bll.ProducterHeartbeat(c, context.ProducterInfo.ProducterModel.tempid, context.ProducterInfo.ProducterModel.mqpathid);
                        });
                        CheckMqPathUpdate(context);
                    }
                }
                catch (Exception exp)
                {
                    ErrorLogHelper.WriteLine(-1, "", "HeatbeatRun", "生产者心跳循环错误", exp);
                }
                System.Threading.Thread.Sleep(SystemParamConfig.Producter_HeatBeat_Every_Time * 1000);
                DebugHelper.WriteLine(-1, "", "HeatbeatRun", "生产者心跳循环一轮结束");
            }
        }

        private void CheckMqPathUpdate(ProducterContext context)
        {
            lock (_contextupdatelock)
            {
                string mqpath = "";
                try
                {
                    if (context.Disposeing == true)
                        return;
                    mqpath = context.ProducterInfo.MqPathModel.mqpath;
                    //检查当前队列是否有更新，有更新则重启producter
                    var lastupdatetime = GetLastUpdateTimeOfMqPath(context);
                    if (context.LastMQPathUpdateTime < lastupdatetime || context.IsNeedReload)
                    {
                        ProducterBLL bll = new ProducterBLL(); ProducterInfo productinfo = null;
                        SqlHelper.ExcuteSql(context.ProducterProvider.Config.ManageConnectString, (c) =>
                        {
                            productinfo = bll.GetProducterInfo(c, context.ProducterProvider.MQPath,context.ProducterProvider.ProducterName);
                        });
                        context.ProducterInfo.Update(productinfo);
                        context.IsNeedReload = false;
                        context.LastMQPathUpdateTime = lastupdatetime;
                        redislistener.RedisServerIp = ConfigHelper.RedisServer;
                    }
                    //检查发送错误,错误发生超过一分钟自动重启来解决错误状态
                    if (context.SendMessageErrorTime != null && (DateTime.Now - context.SendMessageErrorTime) > TimeSpan.FromSeconds(SystemParamConfig.Producter_SendError_Clear_Time))
                    {
                        context.IsNeedReload = true; context.SendMessageErrorTime = null;
                    }
                }
                catch (Exception exp)
                {
                    ErrorLogHelper.WriteLine(context.GetMQPathID(), context.GetMQPath(),"CheckMqPathUpdate", "生产者检测队列是否更新错误", exp);
                }
            }
        }

        private void RedisListenerCommand(string channel, string msg)
        {
            if (Contexts.Count == 0)
                return;
            try
            {
                DebugHelper.WriteLine(-1, "", "RedisListenerCommand", "生产者心跳接收到redis消息开始处理");
                foreach (var context in Contexts.CopyToList())
                {
                    var command = new XXF.Serialization.JsonHelper().Deserialize<BusinessMQNetCommand>(msg);
                    if ((command.CommandReceiver == EnumCommandReceiver.All || command.CommandReceiver == EnumCommandReceiver.Producter) && command.MqPath.ToLower() == context.ProducterProvider.MQPath.ToLower())
                    {
                        if (command.CommandType == EnumCommandType.Register)
                        {
                            context.IsNeedReload = true;
                            CheckMqPathUpdate(context);
                        }
                    }
                }
                DebugHelper.WriteLine(-1, "", "RedisListenerCommand", "生产者心跳接收到redis消息处理完毕");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(-1, "", "RedisListenerCommand", "生产者Redis命令监听出错", exp);
            }
        }

        private DateTime GetLastUpdateTimeOfMqPath(ProducterContext context)
        {
            DB.ProducterBLL bll = new DB.ProducterBLL(); DateTime dt = DateTime.MinValue;
            SqlHelper.ExcuteSql(context.ProducterProvider.Config.ManageConnectString, (c) =>
            {
                dt = bll.GetMqPathModel(c, context.ProducterProvider.MQPath).lastupdatetime;
            });
            return dt;
        }

        public void Dispose()
        {
            if (cancelSource != null)
                cancelSource.Cancel();
        }
    }
}
