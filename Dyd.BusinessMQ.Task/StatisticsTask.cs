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
    public class StatisticsTask : BaseMQTask
    {
        private tb_consumer_dal dal = new tb_consumer_dal();
        private tb_partition_dal partitionDal = new tb_partition_dal();
        private tb_datanode_dal dataNodeDal = new tb_datanode_dal();
        private tb_messagequeue_dal msgDal = new tb_messagequeue_dal();
        private tb_partition_messagequeue_report_dal reportDal = new tb_partition_messagequeue_report_dal();

        public override void Run()
        {
            List<tb_datanode_model> nodeList = new List<tb_datanode_model>();
            List<tb_partition_model> list = new List<tb_partition_model>();

            using (DbConn conn = DbConfig.CreateConn(this.AppConfig["BusinessMQManageConnectString"]))
            {
                conn.Open();
                nodeList = dataNodeDal.List(conn);
                list = partitionDal.GetAllCanUsePartitionList(conn);
            }

            foreach (var item in nodeList)
            {
                string dataNode = PartitionRuleHelper.GetDataNodeName(item.datanodepartition);
                string nodeConn = string.Format("server={0};Initial Catalog={1};User ID={2};Password={3};", item.serverip, dataNode, item.username, item.password);

                try
                {
                    using (DbConn dataNodeConn = DbConfig.CreateConn(nodeConn))
                    {
                        dataNodeConn.Open();
                        foreach (var value in list)
                        {
                            PartitionIDInfo info = PartitionRuleHelper.GetPartitionIDInfo(value.partitionid);
                            string partition = PartitionRuleHelper.PartitionNameRule(info.TablePartition);
                            ICollection<string> tableList = msgDal.GetTableNameListByPartition(dataNodeConn, partition);
                            if (tableList != null && tableList.Count > 0)
                            {
                                using (DbConn conn = DbConfig.CreateConn(this.AppConfig["BusinessMQManageConnectString"]))
                                {
                                    foreach (var table in tableList)
                                    {
                                        if (!dataNodeConn.TableIsExist(table))
                                        {
                                            continue;
                                        }
                                        long maxId = msgDal.GetMaxID(dataNodeConn, table);
                                        long minId = msgDal.GetMinID(dataNodeConn, table);
                                        tb_partition_messagequeue_report_model model = new tb_partition_messagequeue_report_model();
                                        model.partitionid = value.partitionid;
                                        if (maxId > 0 && minId > 0)
                                        {
                                            MQIDInfo mqInfo = PartitionRuleHelper.GetMQIDInfo(maxId);
                                            model.day = mqInfo.Day;
                                            model.lastupdatetime = DateTime.Now;
                                            model.createtime = DateTime.Now;
                                            model.mqmaxid = maxId;
                                            model.mqminid = minId;
                                            reportDal.AddReport(conn, model);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.OpenOperator.Error("节点不存在", ex);
                    continue;
                }
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
