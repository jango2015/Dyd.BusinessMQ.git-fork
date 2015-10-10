using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Db;

namespace Dyd.BusinessMQ.Task
{
    /// <summary>
    /// MQ消息表表数量统计任务
    /// 统计当前30天内的消息
    /// </summary>
    public class MQTableCountStatisticsTask : BaseMQTask
    {

        public MQTableCountStatisticsTask()
            : base()
        { }



        public override void Run()
        {
            ConfigHelper.LoadConfig(this.AppConfig["BusinessMQManageConnectString"]);

            List<tb_datanode_model> nodeList = new List<tb_datanode_model>();

            using (DbConn conn = DbConfig.CreateConn(this.AppConfig["BusinessMQManageConnectString"]))
            {
                conn.Open();
                List<tb_partition_model> createTimeList = new tb_consumer_dal().GetClientCreateTime(conn);
                var serverdate = conn.GetServerDate().Date; var timenow = conn.GetServerDate();
                nodeList = new tb_datanode_dal().List(conn);
                List<Exception> exps = new List<Exception>();
                foreach (var item in nodeList)
                {
                    try
                    {
                        string dataNode = PartitionRuleHelper.GetDataNodeName(item.datanodepartition);
                        string nodeConn = string.Format("server={0};Initial Catalog={1};User ID={2};Password={3};", item.serverip, dataNode, item.username, item.password);
                        string partitionId = PartitionRuleHelper.PartitionNameRule(item.datanodepartition);
                        string t = PartitionRuleHelper.GetDataNodeName(item.datanodepartition);

                        using (DbConn dataNodeConn = DbConfig.CreateConn(nodeConn))
                        {
                            dataNodeConn.Open();
                            var tablesinfo = new tb_messagequeue_dal().GetDataNodeTable(dataNodeConn, serverdate.AddDays(-30));
                            foreach (var table in tablesinfo)
                            {
                                string tablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(table.TablePartition, table.Day);

                                TableInfo info = PartitionRuleHelper.GetTableInfo(tablename);
                                var beginId = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQID(new MQIDInfo() { AutoID = 0, DataNodePartition = item.datanodepartition, Day = table.Day, TablePartition = table.TablePartition });

                                int mqPartitionId = PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = item.datanodepartition, TablePartition = table.TablePartition });
                                tb_partition_model timeModel = createTimeList.Where(q => q.partitionid == mqPartitionId).FirstOrDefault();
                                DateTime time = DateTime.Parse("2015-01-01");
                                if (timeModel != null)
                                {
                                    time = timeModel.createtime;
                                }
                                var dal = new tb_messagequeue_dal(); dal.TableName = tablename;

                                int count = dal.GetCount(dataNodeConn, time);
                                long maxId = dal.GetMaxID(dataNodeConn, time); maxId = (maxId == 0 ? beginId : maxId);
                                long minId = dal.GetMinID(dataNodeConn, time); minId = (minId == 0 ? beginId : minId);
                                tb_partition_messagequeue_report_model model = new tb_partition_messagequeue_report_model();
                                model.partitionid = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = item.datanodepartition, TablePartition = table.TablePartition });

                                MQIDInfo mqInfo = PartitionRuleHelper.GetMQIDInfo(maxId);
                                model.day = mqInfo.Day;
                                model.lastupdatetime = timenow;
                                model.createtime = timenow;
                                model.mqmaxid = maxId;
                                model.mqminid = minId;
                                model.mqcount = count;
                                new tb_partition_messagequeue_report_dal().AddReport(conn, model);
                            }

                        }
                    }
                    catch (Exception exp)
                    {
                        exps.Add(exp);
                    }

                }
                Error(this.AppConfig["BusinessMQManageConnectString"], "消息统计出错", exps);
            }



        }

        public override void TestRun()
        {
            /*测试环境下任务的配置信息需要手工填写,正式环境下需要配置在任务配置中心里面*/
            this.AppConfig = new XXF.BaseService.TaskManager.SystemRuntime.TaskAppConfigInfo();
            this.AppConfig.Add("BusinessMQManageConnectString", "server=192.168.17.201;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;");
            base.TestRun();
        }
    }
}
