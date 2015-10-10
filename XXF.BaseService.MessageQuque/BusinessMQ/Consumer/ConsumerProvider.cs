using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.DB;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using XXF.ProjectTool;
using BusinessMQConfig = XXF.BaseService.MessageQuque.BusinessMQ.Common.BusinessMQConfig;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者使用提供类
    /// </summary>
    public class ConsumerProvider : IDisposable
    {
        public BusinessMQConfig Config;
        public string MQPath { get { return mqpath; } set { mqpath = value.ToLower().Trim(); } }
        protected string mqpath;
        /// <summary>
        /// 当前客户端业务的唯一标识
        /// </summary>
        public string Client { get { return client; } set { client = value.ToLower().Trim(); } }
        protected string client;
        /// <summary>
        /// 客户端名称,仅用于显示区分
        /// </summary>
        public string ClientName = "未定义";
        /// <summary>
        /// 消费者处理线程数量（默认自动设置为当前可用分区数量，为性能最佳，过多或者过少，将被系统重置,调试时可以设置为1）
        /// </summary>
        public int MaxReceiveMQThread = 0;
        public List<int> PartitionIndexs = new List<int>();

        private ConsumerContext Context;

        public ConsumerProvider()
        {

        }
        /// <summary>
        /// 注册消息循环
        /// </summary>
        public void RegisterReceiveMQListener<T>(Action<BusinessMQResponse<T>> action)
        {
            try
            {
                if (Context != null)
                { throw new BusinessMQException("当前实例不能打开多个Consumer监听"); }
                PartitionIndexs = (from o in PartitionIndexs orderby o select o).Distinct().ToList();
                Context = new ConsumerContext(); Context.ConsumerProvider = this;
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterReceiveMQListener", "消费者开始注册消息回调");
                //注册信息
                RegisterConsumerInfo();
                //注册消息回调
                Context.Listener = new ReceiveMessageListener(); Context.ActionInfo = new ConsumerActionInfo() { Action = action, ReturnType = typeof(T) };
                Context.Listener.Register((c) =>
                {
                    BusinessMQResponse<T> r = new BusinessMQResponse<T>(); r.InnerObject = c; r.ObjMsg = ((MQMessage)c).MessageObj<T>();
                    action.Invoke(r);
                }, Context);
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterReceiveMQListener", "注册消费者监听成功");
                LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterReceiveMQListener", "注册消费者监听成功");

            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterReceiveMQListener", "消费者注册MQ监听出错", exp);

                try
                {
                    this.Dispose();
                }
                catch { }
                throw exp;
            }
        }

        private void RegisterConsumerInfo()
        {
            ConsumerBLL bll = new ConsumerBLL();
            DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "消费者开始注册消费者信息");
            //注册消费者
            SqlHelper.ExcuteSql(Config.ManageConnectString, (c) =>
            {
                try
                {
                    //c.BeginTransaction();
                    //取消注册用户信息
                    if (Context.ConsumerInfo != null && Context.ConsumerInfo.ConsumerModel != null)
                        bll.RemoveConsumer(c, Context.ConsumerInfo.ConsumerModel.tempid, Context.ConsumerInfo.ConsumerModel.consumerclientid);

                    Context.ConsumerInfo = new ConsumerInfo(); Context.ConsumerInfo.ConsumerPartitionModels = new List<Model.tb_consumer_partition_model>();
                    //注册并获取clientid
                    Context.ConsumerInfo.ConsumerClientModel = bll.RegisterClient(c, Client);
                    if (Context.ConsumerInfo.ConsumerClientModel == null)
                        throw new BusinessMQException("客户端注册client失败！");
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "注册并获取clientid成功");
                    //清理过期不心跳消费端,并检验partitionindex不重复,并注册消费者
                    Context.ConsumerInfo.ConsumerModel = bll.RegisterConsumer(c, Context.ConsumerInfo.ConsumerClientModel.id, ClientName, PartitionIndexs);
                    if (Context.ConsumerInfo.ConsumerModel == null)
                        throw new BusinessMQException("当前客户端注册consumer失败！");
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "清理过期不心跳消费端,并检验partitionindex不重复,并注册消费者成功");
                    //消费者订阅队列信息
                    Context.ConsumerInfo.MQPathModel = bll.GetMQPath(c, this.MQPath);
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "消费者订阅队列信息");
                    //注册并更新消费者端分区信息
                    foreach (var partitionindex in PartitionIndexs)
                    {
                        var model = bll.RegisterConsumerPartition(c, Context.ConsumerInfo.ConsumerClientModel.id, partitionindex, MQPath, Context.ConsumerInfo.ConsumerModel.tempid);
                        if (model != null)
                            Context.ConsumerInfo.ConsumerPartitionModels.Add(model);
                    }
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "注册并更新消费者端分区信息");
                    //获取分区节点信息缓存
                    List<int> datanodepartition = new List<int>();
                    foreach (var p in Context.ConsumerInfo.ConsumerPartitionModels)
                    {
                        var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                        if (!datanodepartition.Contains(partitionidinfo.DataNodePartition))
                            datanodepartition.Add(partitionidinfo.DataNodePartition);
                    }
                    Context.ConsumerInfo.DataNodeModelDic = bll.GetDataNodeModelsDic(c, datanodepartition);
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "获取分区节点信息缓存");
                    //校准服务器时间
                    Context.ManageServerTime = c.GetServerDate();
                    //c.Commit();
                }
                catch (Exception exp)
                {
                    //c.Rollback();
                    throw exp;
                }

            });
            DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "注册消费者信息完毕");
            //获取系统配置信息
            ConfigHelper.LoadConfig(Config.ManageConnectString);
            DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "RegisterConsumerInfo", "获取系统配置信息");
        }


        public void Dispose()
        {
            if (Context != null)
            {
                try
                {
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "开始消费者资源释放");
                    if (Context != null)
                        Context.Dispose();
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "消费者上下文资源释放成功");
                }
                catch (Exception exp)
                {
                    ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "消费者资源释放出错", exp);
                    throw new BusinessMQException("释放订阅客户端消息处理资源失败", exp);
                }
                finally
                {
                    try
                    {
                        ConsumerBLL bll = new ConsumerBLL();
                        //取消注册
                        SqlHelper.ExcuteSql(Config.ManageConnectString, (c) =>
                        {
                            bll.RemoveConsumer(c, Context.ConsumerInfo.ConsumerModel.tempid, Context.ConsumerInfo.ConsumerModel.consumerclientid);
                        });
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "消费者注销注册信息成功");
                        LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "消费者资源释放成功");
                    }
                    catch (Exception exp1)
                    {
                        ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "消费者资源释放出错", exp1);
                        throw new BusinessMQException("移除订阅客户端注册信息失败", exp1);
                    }
                    finally
                    {
                        Context = null;
                    }
                }
               
            }
        }
    }
}
