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
    /// 每日数据迁移任务（废弃）
    /// </summary>
    public class DataMoveEveryDayTask : BaseMQTask
    {
        /// <summary>
        /// 任务调度平台根据发布的任务时间配置，定时回调运行方法
        /// 开发人员的任务插件必须要重载并该方法
        /// </summary>
        public override void Run()
        {
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

            foreach (var p in userdpartitions)
            {
                var partitioninfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                DateTime fromdate = servertime.Date.AddDays(-3).Date;//自动迁移3天内的消息至今天，否则需要手工迁移
                while (fromdate < servertime.Date)
                {
                    var fromtimetablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, fromdate);
                    try
                    {
                        MoveDataToToday(fromdate, servertime.Date, p, datanodemodels);
                    }
                    catch (Exception exp)
                    {
                        this.OpenOperator.Error(string.Format("数据迁移出错,partitionid:{0},fromdate:{1},todate:{2}", p.partitionid,fromdate,servertime.Date), exp);
                    }
                    fromdate = fromdate.AddDays(1);
                }
            }

        }

        private void MoveDataToToday(DateTime fromdate, DateTime serverdate, tb_partition_model p, Dictionary<int, tb_datanode_model> datanodemodels)
        {
            var partitioninfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
            var fromtablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, fromdate);
            var currenttablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, serverdate);
            //二分法查找起始点
            long startmovemqid = StartMoveMQID(fromtablename, partitioninfo, datanodemodels, p, fromdate);
            if (startmovemqid <= -1)
                return;
            //为了解决可能出现的并发插入，网络极度延迟可能导致的数据顺序乱掉，起始点再往前追溯1000条
            startmovemqid = startmovemqid - 1000;
            if (startmovemqid <= 0)
                startmovemqid = 1;
            //从起始点开始批量扫描
            tb_messagequeue_dal dal = new tb_messagequeue_dal(); dal.TableName = fromtablename; var moveMessages = new List<tb_messagequeue_model>();
            do
            {
                SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitioninfo.DataNodePartition]), (c) =>
                {
                   moveMessages = dal.GetListMoreThanID(c,1000,startmovemqid);
                });
                //对每条数据进行逐条迁移 
                 foreach (var message in moveMessages)
                {
                    startmovemqid = Math.Max(startmovemqid, message.id);//起始id自增
                    if (message.sqlcreatetime.Date > fromdate.Date && message.state == (byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMessageState.CanRead)
                    {
                        SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[partitioninfo.DataNodePartition]), (c) =>
                      {
                          tb_messagequeue_dal fromdal = new tb_messagequeue_dal(); fromdal.TableName = fromtablename;
                          tb_messagequeue_dal todal = new tb_messagequeue_dal(); todal.TableName = currenttablename;
                          todal.AddMove(c,new tb_messagequeue_model()
                          {
                              message = message.message,
                              state = (byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMessageState.CanRead,
                           mqcreatetime=message.mqcreatetime, source=(byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMessageSource.Moved });//迁移到最新使用表
                           fromdal.SetState(c, message.id, (byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMessageState.Moved);//原消息设置为已迁移
                      });
                    }
                }
            } while (moveMessages.Count>0);
        }

        private long StartMoveMQID(string fromtablename, PartitionIDInfo info, Dictionary<int, tb_datanode_model> datanodemodels, tb_partition_model p, DateTime tabletime)
        {
            long r = -1;
            SqlHelper.ExcuteSql(this.GetDataNodeConnectString(datanodemodels[info.DataNodePartition]), (c) =>
            {
                bool exsit = c.TableIsExist(fromtablename);
                if (exsit == true)
                {
                    tb_messagequeue_dal dal = new tb_messagequeue_dal(); dal.TableName = fromtablename;
                    long maxid = dal.GetMaxID(c); long minid = dal.GetMinID(c);
                    int findcount = 0;
                    while (findcount<=30)//二分查找算法,超过30次意味着算法若对，查询过一亿的数据，说明算法有问题
                    {
                        findcount++;
                        KeyValuePair<long, DateTime> mininfo = dal.GetIDCreateTime(c, minid); KeyValuePair<long, DateTime> maxinfo = dal.GetIDCreateTime(c, maxid);
                        if (mininfo.Value.Date <= tabletime.Date && maxinfo.Value.Date > tabletime.Date)//最小id小于等于当天，最大id>当天，说明中间有数据要迁移
                        {
                            var midid = (long)((maxinfo.Key + mininfo.Key) / 2); KeyValuePair<long, DateTime> midinfo = dal.GetIDCreateTime(c, midid);
                            if (midinfo.Value.Date <= tabletime.Date)//往大的查找
                            {
                                minid = midid;
                            }
                            else//往小的查找
                            {
                                maxid = minid;
                            }
                            System.Threading.Thread.Sleep(50);//避免给数据库大的压力
                        }
                        else if (mininfo.Value.Date > tabletime.Date)//最小id的时间超过表当天
                        {
                            r = mininfo.Key; return;
                        }
                        else if (maxinfo.Value.Date <= tabletime.Date)//最大id的时间小于或等同当天
                        {
                            r = maxinfo.Key; return;
                        }
                    }
                    if (findcount >= 30)
                        throw new Exception("二分查找算法错误，查找次数超过30次");
                }
            });
            return r;
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
