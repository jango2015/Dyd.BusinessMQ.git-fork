using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Task
{
    /// <summary>
    /// 消费者端异常任务
    /// </summary>
    public class ConsumerAbNormalTask : BaseMQTask
    {
        int runcount = 0;
        //消费者端异常停止，没有心跳
        //消费者端长时间无消费，但是还有多余消息
        //某个分区没有指定消费者
        public ConsumerAbNormalTask()
            : base()
        { }

        /// <summary>
        /// 任务调度平台根据发布的任务时间配置，定时回调运行方法
        /// 开发人员的任务插件必须要重载并该方法
        /// </summary>
        public override void Run()
        {
            runcount++;

            ConfigHelper.LoadConfig(this.AppConfig["BusinessMQManageConnectString"]);

            List<tb_partition_model> userdpartitions = new List<tb_partition_model>();
            Dictionary<int, tb_datanode_model> datanodemodels = new Dictionary<int, tb_datanode_model>();
            List<RegisterdConsumersModel> registeredconsumermodels = new List<RegisterdConsumersModel>();
            List<tb_consumer_model> consumers = new List<tb_consumer_model>();
            DateTime servertime = DateTime.Now;
            SqlHelper.ExcuteSql(this.AppConfig["BusinessMQManageConnectString"], (c) =>
            {
                tb_partition_dal partitiondal = new tb_partition_dal();
                userdpartitions = partitiondal.List(c, true);
                tb_datanode_dal datanodedal = new tb_datanode_dal();
                var ms = datanodedal.List(c);
                foreach (var m in ms)
                {
                    datanodemodels.Add(m.datanodepartition, m);
                }
                servertime = c.GetServerDate();

                //获取当前注册的消费者
                tb_consumer_dal consumerdal = new tb_consumer_dal();
                registeredconsumermodels = consumerdal.GetRegisterdConsumers(c);
                consumers = consumerdal.GetAllList(c);
            });
            //消费者端心跳停止,异常退出检测(超过1分钟)
            List<Exception> exps = new List<Exception>();
            foreach (var c in consumers)
            {
                if ((servertime - c.lastheartbeat) > TimeSpan.FromMinutes(1))
                {
                    exps.Add(new Exception(string.Format("当前消费者tempid:{0},consumerclientid:{1}", c.tempid, c.consumerclientid)));
                }
            }
            Error(this.AppConfig["BusinessMQManageConnectString"], "消费者端心跳停止,异常退出检测(超过1分钟),若是异常停止,请手工清除消费者中断", exps);
            //消费者端长时间无消费，但是还有多余消息（超过10分钟未消费,但还有多余消息未消费）
            exps.Clear();
            foreach (var c in registeredconsumermodels)
            {
                if ((servertime - c.consumerpartitionmodel.lastupdatetime) > TimeSpan.FromMinutes(10))
                {
                    var partitionidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(c.consumerpartitionmodel.partitionid);
                    var tablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitionidinfo.TablePartition, servertime.Date);
                    SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitionidinfo.DataNodePartition]), (conn) =>
                    {
                        tb_messagequeue_dal messagedal = new tb_messagequeue_dal(); messagedal.TableName = tablename;
                        long maxmqid = messagedal.GetMaxID(conn);
                        if (maxmqid > c.consumerpartitionmodel.lastmqid && (servertime - messagedal.GetIDCreateTime(conn, maxmqid).Value > TimeSpan.FromMinutes(10)))//还有消息未消费，且该消息的创建时间是10分钟前
                        {
                            exps.Add(new Exception(string.Format("当前消费者tempid:{0},consumerclientid:{1},client:{2},clientname:{3}", c.consumermodel.tempid,
                                                   c.consumermodel.consumerclientid, c.consumerclientmodel.client, c.consumermodel.clientname)));
                        }
                    });
                }
            }
            Error(this.AppConfig["BusinessMQManageConnectString"], "消费者端长时间无消费，但是还有多余消息（超过已创建超过10分钟的消息未消费）,可能是消费者处理逻辑异常", exps);
            //某个分区没有指定消费者(分区没有消费信息,或者分区消费最后消费时间超过1个小时无消费)
            exps.Clear();
            if (runcount % (12*6) == 0)
            {
                foreach (var u in userdpartitions)
                {
                    bool isfind = false;
                    foreach (var c in registeredconsumermodels)
                    {
                        if (c.consumerpartitionmodel.partitionid == u.partitionid)
                        {
                            isfind = true; break;
                        }
                    }
                    if (isfind == false)
                    {
                        SqlHelper.ExcuteSql(this.AppConfig["BusinessMQManageConnectString"], (c) =>
                        {
                            tb_consumer_partition_dal dal = new tb_consumer_partition_dal();
                            var model = dal.GetByPartitionId(c, u.partitionid);
                            if (model == null)
                                exps.Add(new Exception(string.Format("分区{0}未指定消费者", u.partitionid)));
                            //else if((servertime - model.lastupdatetime) > TimeSpan.FromHours(24))
                            //     exps.Add(new Exception(string.Format("分区{0}消费者已注销,且有1天未消费", u.partitionid)));

                        });

                    }
                }
                Error(this.AppConfig["BusinessMQManageConnectString"], "某个分区没有指定消费者(分区没有消费信息)", exps);
            }
        }

        /// <summary>
        /// 开发人员自测运行入口
        /// 需要将项目配置为->控制台应用程序，写好Program类和Main入口函数
        /// </summary>
        public override void TestRun()
        {
            /*测试环境下任务的配置信息需要手工填写,正式环境下需要配置在任务配置中心里面*/
            this.AppConfig = new XXF.BaseService.TaskManager.SystemRuntime.TaskAppConfigInfo();
            this.AppConfig.Add("BusinessMQManageConnectString", "server=192.168.17.201;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;");

            base.TestRun();
        }
    }
}
