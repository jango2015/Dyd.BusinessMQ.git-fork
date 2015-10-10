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
    /// 数据异常任务
    /// </summary>
    public class DataAbNormalTask : BaseMQTask
    {
        //当天数据超过7000万
        //(今天，明天，后天)某张表未创建

        public DataAbNormalTask()
            : base()
        { }

        /// <summary>
        /// 任务调度平台根据发布的任务时间配置，定时回调运行方法
        /// 开发人员的任务插件必须要重载并该方法
        /// </summary>
        public override void Run()
        {
            ConfigHelper.LoadConfig(this.AppConfig["BusinessMQManageConnectString"]);
            List<tb_partition_model> userdpartitions = new List<tb_partition_model>();
            Dictionary<int, tb_datanode_model> datanodemodels = new Dictionary<int, tb_datanode_model>();
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
            });

            List<Exception> exps = new List<Exception>();
            foreach (var p in userdpartitions)
            {
                var partitioninfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                List<DateTime> days = new List<DateTime>() { servertime.Date, servertime.Date.AddDays(1), servertime.Date.AddDays(2) };
                //检查三天表创建情况
                foreach (var day in days)
                {
                    var tablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, day);//
                    SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitioninfo.DataNodePartition]), (c) =>
                    {
                        bool exsit = c.TableIsExist(tablename);
                        if (exsit != true)
                        {
                            exps.Add(new Exception(string.Format("分区{0}表{1}不存在", p.partitionid, tablename)));
                        }
                    });
                }

                //检测当天的数据量是否过大
                var todaytablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, servertime.Date);//
                SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitioninfo.DataNodePartition]), (c) =>
                {
                    tb_messagequeue_dal dal = new tb_messagequeue_dal(); dal.TableName = todaytablename;
                    var maxid = dal.GetMaxID(c);
                    if (maxid % 100000000 >= 70000000)
                    {
                        exps.Add(new Exception(string.Format("数据量超过70000000,当前最大mqid:{0}", maxid)));
                    }
                });
            }
            Error(this.AppConfig["BusinessMQManageConnectString"], "数据异常任务之表创建检查", exps);
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
