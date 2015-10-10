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
    /// MQ每日建表任务
    /// </summary>
    public class MQCreateTableDayTask : BaseMQTask
    {
        public MQCreateTableDayTask()
            : base()
        { }
        /// <summary>
        /// 任务调度平台根据发布的任务时间配置，定时回调运行方法
        /// 开发人员的任务插件必须要重载并该方法
        /// </summary>
        public override void Run()
        {
            /* 
             * this.OpenOperator 用于任务调度平台提供给第三方使用的所有api接口封装
             */
            ConfigHelper.LoadConfig(this.AppConfig["BusinessMQManageConnectString"]);
            var sql = @"";
            List<tb_partition_model> userdpartitions = new List<tb_partition_model>();
            Dictionary<int, tb_datanode_model> datanodemodels = new Dictionary<int, tb_datanode_model>();
            DateTime servertime = DateTime.Now;
            SqlHelper.ExcuteSql(this.AppConfig["BusinessMQManageConnectString"], (c) =>
            {
                tb_config_dal configdal = new tb_config_dal();
                sql = configdal.Get(c, "MQCreateTableSql").value;
                tb_partition_dal partitiondal = new tb_partition_dal();
                userdpartitions = partitiondal.List(c, true);
                tb_datanode_dal datanodedal = new tb_datanode_dal();
                var ms = datanodedal.List(c);
                foreach (var m in ms)
                {
                    datanodemodels.Add(m.datanodepartition, m);
                }
                servertime = c.GetServerDate();
            });

            if (string.IsNullOrWhiteSpace(sql))
            {
                Error(this.AppConfig["BusinessMQManageConnectString"], "MQCreateTableSql未配置参数", new Exception("MQ未找到或者未配置MQCreateTableSql参数,用于每天创建消息分区表"));
                return;
            }

            foreach (var p in userdpartitions)
            {
                var partitioninfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                DateTime currenttime = servertime.Date;
                while (currenttime <= servertime.Date.AddDays(3))//预创建3天的表
                {
                    var tablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, currenttime);//
                    SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitioninfo.DataNodePartition]), (c) =>
                    {
                        bool exsit = c.TableIsExist(tablename);
                        if (exsit != true)
                        {
                            string cmd = sql.Replace("{tablepartiton}", XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(partitioninfo.TablePartition ))
                                .Replace("{daypartition}", currenttime.ToString("yyMMdd")).Replace("{datanodepartiton}",XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(partitioninfo.DataNodePartition));
                            c.ExecuteSql(cmd,new List<XXF.Db.ProcedureParameter>());
                        }
                    });
                    currenttime = currenttime.AddDays(1);
                }
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
