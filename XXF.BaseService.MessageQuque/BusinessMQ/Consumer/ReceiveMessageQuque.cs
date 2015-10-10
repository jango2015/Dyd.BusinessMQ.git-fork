using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.DB;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Log;
using XXF.ProjectTool;
using System.Threading.Tasks;
using XXF.BaseService.MessageQuque.BusinessMQ.Redis.MessageLock;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者内部消息队列（消息缓存）
    /// </summary>
    public class ReceiveMessageQuque : IDisposable
    {
        private PartitionQueue<MQMessage> Queue = new PartitionQueue<MQMessage>();
        private Dictionary<int, long> _lastpullququeiddic = new Dictionary<int, long>();//上一次拉取的缓存消息的消息maxid
        private ConsumerContext Context;
        private CancellationTokenSource cancelSource;
        private Dictionary<string, string> exsittablenames = new Dictionary<string, string>();//已经验证存在的表名缓存
        private RedisNetCommandListener redislistener;
        private BaseMessageLock messagelock = new ReceiveMessageLock(TimeSpan.FromMilliseconds(500));
        private object _tiggerGetMessagesTaskLock = new object();
        private Dictionary<int, Exception> errorpartitions = new Dictionary<int, Exception>();//分区错误信息缓存

        public ReceiveMessageQuque(ConsumerContext context)
        {
            Context = context; Context.Quque = this;
            Create();
        }

        private void Create()
        {
            try
            {
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Create", "消费者开始创建内部消息队列");
                Init();
                cancelSource = new CancellationTokenSource();

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Run();
                }, cancelSource.Token);
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Create", "消费者内部消息队列注册消息循环成功");

                redislistener = new RedisNetCommandListener(ConfigHelper.RedisServer); redislistener.Name = "消费者内部消息队列";
                redislistener.Register((channel, msg) =>
                {
                    RedisListenerCommand(channel, msg);
                }, cancelSource, Context.ConsumerProvider.MQPath, SystemParamConfig.Redis_Channel_Quque + "." + Context.ConsumerProvider.MQPath);
                DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Create", "消费者内部消息队列注册redis消息监听成功");

                LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Create", "消费者内部队列注册成功");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Create", "消费者内部消息队列创建失败", exp);
                throw exp;
            }
        }

        private void Init()
        {
            DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Init", "消费者内部消息队列Init");
            this.Queue.Clear();//清空消息缓存
            _lastpullququeiddic = new Dictionary<int, long>();//清空消息拉取最后id标记
            exsittablenames = new Dictionary<string, string>();//已经验证存在的表名缓存
            errorpartitions = new Dictionary<int, Exception>();//分区错误信息缓存

            //初始化最后的mqid
            foreach (var partition in Context.ConsumerInfo.ConsumerPartitionModels)
            {
                _lastpullququeiddic.Add(partition.partitionid, partition.lastmqid);
            }
            if (Context.ConsumerProvider.MaxReceiveMQThread <= 0 || (Context.ConsumerProvider.MaxReceiveMQThread > Context.ConsumerInfo.ConsumerPartitionModels.Count))
            {
                if (Context.ConsumerInfo.ConsumerPartitionModels.Count > 0)
                    Context.ConsumerProvider.MaxReceiveMQThread = Context.ConsumerInfo.ConsumerPartitionModels.Count;
                else
                    Context.ConsumerProvider.MaxReceiveMQThread = 1;
            }
            LogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Init", "消费者内部消息队列Init");
        }


        private void Run()
        {
            while (!cancelSource.IsCancellationRequested)
            {
                bool issleep = true;
                try
                {
                    issleep = TiggerMessagesTask();
                    DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Run", "消费者内部消息队列循环一轮完成");
                }
                catch (Exception exp)
                {
                    ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-Run", "消费者内部消息队列循环出错。", exp);
                }
                finally
                {
                    if (issleep == true && !cancelSource.IsCancellationRequested)
                        System.Threading.Thread.Sleep(SystemParamConfig.Consumer_ReceiveMessageQuque_Every_Sleep_Time * 1000);
                }
            }
        }

        private void RedisListenerCommand(string channel, string msg)
        {

            try
            {
                if (!cancelSource.IsCancellationRequested)
                {
                    messagelock.Lock(() =>
                    {
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-RedisListenerCommand", "消费者Redis开始执行消息命令");
                        this.TiggerMessagesTask();
                        DebugHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-RedisListenerCommand", "消费者Redis消息命令执行完毕");
                    });
                }
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-RedisListenerCommand", "消费者Redis监听消息命令", exp);
            }

        }

        /// <summary>
        /// 触发消息队列消息拉取任务
        /// </summary>
        private bool TiggerMessagesTask()
        {
            bool ifsleep = true;//是否要休眠，除非有大量消息堆积则不休眠
            DebugHelper.TimeWatch(Context.GetMQPathID(), Context.GetMQPath(), "TiggerMessagesTask", () =>
            {
                lock (_tiggerGetMessagesTaskLock)//仅一个任务在进行扫描
                {
                    //若无数据，则检查manage数据库当前时间，若manage数据库时间已改变，且数据库已创建新的日表，则使用新表。
                    SqlHelper.ExcuteSql(Context.ConsumerProvider.Config.ManageConnectString, (c) =>
                    {
                        //serverdate = c.GetServerDate();
                        Context.ManageServerTime = c.GetServerDate();
                    });

                    //从各个分区拉消息并缓存
                    GetMessagesFromPartitions();
                    //获取缓存中最长消息分区队列长度
                    int maxcountofpartition = Queue.MaxCountOfPartition();
                    //从各个分区缓存推送消息消费
                    SendMessagesToConsume();

                    //队列长度检测及判断是否需要持续拉去数据
                    if (maxcountofpartition == SystemParamConfig.Consumer_ReceiveMessageQuque_EVERY_PULL_COUNT)//有部分分区有大量消息堆积的情况
                    {
                        ifsleep = false;
                    }
                    if (maxcountofpartition > SystemParamConfig.Consumer_ReceiveMessageQuque_EVERY_PULL_COUNT)//有队列溢出异常
                    {
                        throw new BusinessMQException(string.Format("最大缓存分区队列长度为:{0},超过{1},队列过长溢出", maxcountofpartition, SystemParamConfig.Consumer_ReceiveMessageQuque_EVERY_PULL_COUNT));
                    }
                }
            });
            return ifsleep;
        }

        private void SendMessagesToConsume()
        {
            Parallel.ForEach(Context.ConsumerInfo.ConsumerPartitionModels, new ParallelOptions() { CancellationToken = cancelSource.Token, MaxDegreeOfParallelism = Context.ConsumerProvider.MaxReceiveMQThread }, (partition) =>
            {
            //Context.ConsumerInfo.ConsumerPartitionModels.ForEach((partition) =>
            //{
                var m = Queue.Dequeue(partition.partitionid);
                while (m != null)
                {
                    if (cancelSource.IsCancellationRequested)
                        return;
                    try
                    {
                        Context.ActionInfo.InnerAction(m);
                        if (m.IsMarkFinished == false)
                            throw new BusinessMQException(string.Format("当前消息未标记为已消费状态,json:{0},id:{1}", m.Model.message, m.Model.id));//抛出错误且该分区的消息处理将停滞。
                        else
                            m = Queue.Dequeue(partition.partitionid);
                    }
                    catch (Exception exp)
                    {
                        string msg = string.Format("MQ消息处理出错,json:{0},id:{1}", m.Model.message, m.Model.id);
                        if (!errorpartitions.ContainsKey(partition.partitionid))
                            errorpartitions.Add(partition.partitionid, new BusinessMQException(msg, exp));
                        ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "ReceiveMessageQuque-SendMessagesToConsume", msg, exp);
                        return;
                    }
                }
            });
        }

        private void GetMessagesFromPartitions()
        {
            foreach (var partition in Context.ConsumerInfo.ConsumerPartitionModels)
            {
                if (cancelSource.IsCancellationRequested)
                { break; }
                try
                {
                    if (errorpartitions.ContainsKey(partition.partitionid))
                        throw new BusinessMQException("分区消息处理出现错误", errorpartitions[partition.partitionid]);
                    var scanlastmqid = _lastpullququeiddic[partition.partitionid]; //上次扫描的mqid
                    var mqidinfo = PartitionRuleHelper.GetMQIDInfo(scanlastmqid);//解析上次扫描的mqid信息

                    ConsumerBLL consumerbll = new ConsumerBLL(); var partionidinfo = PartitionRuleHelper.GetPartitionIDInfo(partition.partitionid);//解析分区信息
                    string datanodeconnectstring = consumerbll.GetDataNodeConnectString(SystemParamConfig.Consumer_DataNode_ConnectString_Template, Context.ConsumerInfo.DataNodeModelDic[partionidinfo.DataNodePartition]);//获取节点连接

                    var messages = GetMessagesOfPatition(partition, scanlastmqid, mqidinfo, datanodeconnectstring, partionidinfo);

                    if (messages.Count > 0)
                    {
                        ConsumeMessages(messages, partition, mqidinfo);
                    }
                    else
                    {
                        CheckIfScanNewTable(partition, mqidinfo, datanodeconnectstring, partionidinfo);
                    }


                }
                catch (Exception exp)
                {
                    ErrorLog.Write(string.Format("MQ消费者端消息循环出错,clientid:{0},partitionid:{1}", partition.consumerclientid, partition.partitionid), exp);
                }
            }
        }

        private List<tb_messagequeue_model> GetMessagesOfPatition(tb_consumer_partition_model partition, long scanlastmqid, MQIDInfo mqidinfo, string datanodeconnectstring, PartitionIDInfo partionidinfo)
        {
            //从数据库取数据,检查数据时间，和数据状态，无问题则插入队列，并修改上一次扫描的mqid
            List<tb_messagequeue_model> messages = new List<tb_messagequeue_model>();
            SqlHelper.ExcuteSql(datanodeconnectstring, (c) =>
            {
                string tablename = PartitionRuleHelper.GetTableName(partionidinfo.TablePartition, mqidinfo.Day);
                if (exsittablenames.ContainsKey(tablename) || c.TableIsExist(tablename))
                {
                    AddExsitTableNameCache(tablename);
                    tb_messagequeue_dal dal = new tb_messagequeue_dal(); dal.TableName = tablename;
                    messages = dal.GetMessages(c, scanlastmqid, SystemParamConfig.Consumer_ReceiveMessageQuque_EVERY_PULL_COUNT);
                }
            });
            return messages;
        }

        private void ConsumeMessages(List<tb_messagequeue_model> messages, tb_consumer_partition_model partition, MQIDInfo mqidinfo)
        {
            long maxmqid = -1;
            List<MQMessage> mqmessages = new List<MQMessage>();
            foreach (var m in messages)
            {
                //若是第二天的数据或已迁移的数据则跳过
                if ((m.sqlcreatetime < mqidinfo.Day.Date.AddDays(1)) && (m.state == (byte)EnumMessageState.CanRead))
                {
                    mqmessages.Add(new MQMessage() { Model = m, Context = Context });
                }
                maxmqid = m.id;
            }
            if (mqmessages.Count > 0)
            {
                Queue.Enqueue(partition.partitionid, mqmessages);
            }
            if (maxmqid > 0)
            {
                if (_lastpullququeiddic[partition.partitionid] <= maxmqid && !cancelSource.IsCancellationRequested)
                    _lastpullququeiddic[partition.partitionid] = maxmqid;
                else
                    throw new BusinessMQException(string.Format("检测到消费者端拉去消息时出现消息乱序问题,partitionid:{0},maxmqid:{1},缓存maxmqid:{2}", partition.partitionid, maxmqid, _lastpullququeiddic[partition.partitionid]));
            }
        }

        private void CheckIfScanNewTable(tb_consumer_partition_model partition, MQIDInfo mqidinfo, string datanodeconnectstring, PartitionIDInfo partionidinfo)
        {
            var serverdate = Context.ManageServerTime.AddSeconds(-1);//此处延迟1秒，停顿消费者日分区表之间的切换
            if (serverdate != null && serverdate.Date > mqidinfo.Day.Date)//服务器时间和消息时间对应不上
            {
                SqlHelper.ExcuteSql(datanodeconnectstring, (c) =>
                    {
                        DateTime t = mqidinfo.Day.Date.AddDays(1);
                        while (serverdate.Date >= t)//查找最后的可用消息表,然后跳出
                        {
                            string tablename = PartitionRuleHelper.GetTableName(partionidinfo.TablePartition, t);
                            if (exsittablenames.ContainsKey(tablename) || c.TableIsExist(tablename))
                            {
                                AddExsitTableNameCache(tablename);
                                _lastpullququeiddic[partition.partitionid] = PartitionRuleHelper.GetMQID(new MQIDInfo()
                                {
                                    AutoID = 0,
                                    DataNodePartition = mqidinfo.DataNodePartition,
                                    TablePartition = mqidinfo.TablePartition,
                                    Day = t
                                });//重设第二天最小的id
                                break;
                            }

                            t = t.AddDays(1);
                        }
                    });
            }
        }

        private void AddExsitTableNameCache(string tablename)
        {
            if (!exsittablenames.ContainsKey(tablename))
                exsittablenames.Add(tablename, null);
        }

        public void Dispose()
        {
            if (redislistener != null)
                redislistener.Dispose();

            if (cancelSource != null)
            {
                cancelSource.Cancel();
                Init();
            }

        }


    }
}
