using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.DB;
using XXF.BaseService.MessageQuque.BusinessMQ.Producter.LoadBalance;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Log;
using XXF.ProjectTool;
using BusinessMQConfig = XXF.BaseService.MessageQuque.BusinessMQ.Common.BusinessMQConfig;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 生产者提供者
    /// </summary>
    public class ProducterProvider : IDisposable
    {
        public BusinessMQConfig Config;
        public string MQPath { get { return mqpath; } set { mqpath = value.ToLower().Trim(); } }
        public BaseLoadBalance LoadBalance = new SystemLoadBalance();
        public string ProducterName = "未定义";
        protected string mqpath;

        private ProducterContext Context;
        private ProducterHeartbeatProtect ProducterHeartbeatProtect;
        private RedisNetCommand NetCommand;

        public ProducterTimeWatchInfo ProducterTimeWatchInfo = new ProducterTimeWatchInfo();

        public ProducterProvider()
        {
            Context = new ProducterContext();
            Context.ProducterProvider = this;
            LoadBalance.Context = Context;
        }

        public void Open()
        {
            try
            {
                DebugHelper.WriteLine(-1, MQPath, "Open", "生产者开始初始化");
                //初始化上下文信息
                ProducterBLL bll = new ProducterBLL();
                SqlHelper.ExcuteSql(Config.ManageConnectString, (c) =>
                {
                    Context.ProducterInfo = bll.GetProducterInfo(c, MQPath, ProducterName);
                    Context.ProducterInfo.LoadBalance = LoadBalance;
                    Context.ManageServerTime = c.GetServerDate();
                });
                Context.IsNeedReload = false;
                Context.LastMQPathUpdateTime = Context.ProducterInfo.MqPathModel.lastupdatetime;

                //获取系统配置信息
                ConfigHelper.LoadConfig(Config.ManageConnectString);

                NetCommand = new RedisNetCommand(ConfigHelper.RedisServer);
                ProducterHeartbeatProtect.Instance(Context).Contexts.Add(Context);//注册上下文
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Open", "生产者初始化成功");
                LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Open", "生产者初始化成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Open", "生产者初始化", exp);
                throw exp;
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objmsg"></param>
        public void SendMessage<T>(T objmsg)
        {
            try
            {
                var json = "";
                ProducterTimeWatchInfo.JsonHelperSerializer += Log.TimeWatchLog.Debug(() =>
                {
                    if (!(objmsg is string))
                        json = new Serialization.JsonHelper().Serializer(objmsg);
                    else
                        json = objmsg as string;
                });
                //发送消息有n次重试机会
                int errortrycount = 0;
                while (errortrycount < Context.ProducterInfo.LoadBalance.SendMessageErrorTryAgainCount)
                {
                    LoadBalanceNodeInfo loadbalancenodeinfo = null;
                    ProducterTimeWatchInfo.GetLoadBalanceNodeInfo += Log.TimeWatchLog.Debug(() =>
                    {
                        loadbalancenodeinfo = Context.ProducterInfo.GetLoadBalanceNodeInfo();
                    });
                    if (loadbalancenodeinfo == null)
                        throw new BusinessMQException(string.Format("无法获取队列{0}的可用的负载均衡数据节点", MQPath));
                    string datanodeconnectstring = new ProducterBLL().GetDataNodeConnectString(SystemParamConfig.Producter_DataNode_ConnectString_Template_ToSendMessage, loadbalancenodeinfo.DataNodeModel);
                    var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(loadbalancenodeinfo.MQPathPartitionModel.partitionid);
                    var manageservertime = Context.ManageServerTime;//.AddSeconds(1);发送消息要比标准时间提前1s，这样消息分表可以提前1s
                    string tablename = PartitionRuleHelper.GetTableName(partitionidinfo.TablePartition, manageservertime);
                    try
                    {
                        ProducterTimeWatchInfo.SendMessage += Log.TimeWatchLog.Debug(() =>
                        {
                            double inserttime = 0;
                            double allinserttime = Log.TimeWatchLog.Debug(() =>
                            {
                                inserttime = Log.TimeWatchLog.Debug(() =>
                                {
                                    tb_messagequeue_dal dal = new tb_messagequeue_dal();
                                    dal.TableName = tablename;
                                    SqlHelper.ExcuteSql(datanodeconnectstring, (c) =>
                                    {
                                        dal.Add2(c, new tb_messagequeue_model()
                                        {
                                            message = json,
                                            mqcreatetime = DateTime.Now,
                                            sqlcreatetime = manageservertime
                                            ,
                                            source = (int)EnumMessageSource.Common,
                                            state = (int)EnumMessageState.CanRead
                                        });
                                        
                                    });
                                });
                            });
                            //ProducterTimeWatchTest.AddMessages(string.Format("总插入消息:{0}s,插入消息:{1}s",allinserttime,inserttime));
                        });
                        NetCommand.SendMessage(mqpath);
                        return;
                    }
                    catch (SqlException exp)
                    {
                        ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "SendMessage", string.Format("发送消息出现节点错误,节点:{0}", loadbalancenodeinfo.DataNodeModel.datanodepartition), exp);
                        Context.ProducterInfo.RemoveMQPathPartition(loadbalancenodeinfo.DataNodeModel.datanodepartition);//数据层出错视为数据节点异常，则移除。将在一定时间内尝试恢复
                        Context.ProducterInfo.LoadBalance.AddError(new ErrorLoadBalancePartitionInfo() { PartitionId = loadbalancenodeinfo.MQPathPartitionModel.partitionid, PartitionIndex = loadbalancenodeinfo.MQPathPartitionModel.partitionindex });
                        //Context.IsNeedReload = true;
                        if (Context.SendMessageErrorTime == null)
                            Context.SendMessageErrorTime = DateTime.Now;
                    }
                    errortrycount++;
                }
                throw new BusinessMQException(string.Format("发送消息出现系统级错误,并超过重试次数,请检查。队列:{0}", MQPath));
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "SendMessage", "生产者发送消息出错", exp);
                throw exp;
            }
        }


        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            try
            {
                ProducterBLL bll = new ProducterBLL();
                SqlHelper.ExcuteSql(Config.ManageConnectString, (c) =>
                {
                    bll.RemoveProducter(c, Context.ProducterInfo.ProducterModel.tempid, Context.ProducterInfo.MqPathModel.id);
                });
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者移除注册信息成功");
                LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者移除注册信息成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者移除注册信息出错", exp);
            }

            try
            {
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者资源开始释放");
                ProducterHeartbeatProtect.Instance(Context).Contexts.Remove(Context);//移除上下文
                Context.Dispose();//释放上下文

                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者资源释放成功");
                LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者资源释放成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "Dispose", "生产者资源释放出错", exp);
                throw exp;
            }

        }
    }
}
