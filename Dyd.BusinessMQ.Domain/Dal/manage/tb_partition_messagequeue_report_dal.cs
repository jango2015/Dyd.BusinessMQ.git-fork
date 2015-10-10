using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_partition_messagequeue_report_dal
    {

        public virtual bool Add2(DbConn PubConn, tb_partition_messagequeue_report_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//分区编号
					new ProcedureParameter("@partitionid",    model.partitionid),
					//日期
					new ProcedureParameter("@day",    model.day),
					//分区最大消息id
					new ProcedureParameter("@mqmaxid",    model.mqmaxid),
					//mq最小消息id
					new ProcedureParameter("@mqminid",    model.mqminid),
					//消息数量
					new ProcedureParameter("@mqcount",    model.mqcount),
                    ////当前分区扫描最后更新时间
                    //new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
                    ////当前分区扫描创建时间
                    //new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_partition_messagequeue_report(partitionid,day,mqmaxid,mqminid,mqcount,lastupdatetime,createtime)
										   values(@partitionid,@day,@mqmaxid,@mqminid,@mqcount,getdate(),getdate())", Par);
            return rev == 1;

        }

        public virtual bool Edit2(DbConn PubConn, tb_partition_messagequeue_report_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//分区编号
					new ProcedureParameter("@partitionid",    model.partitionid),
					//日期
					new ProcedureParameter("@day",    model.day),
					//分区最大消息id
					new ProcedureParameter("@mqmaxid",    model.mqmaxid),
					//mq最小消息id
					new ProcedureParameter("@mqminid",    model.mqminid),
					//消息数量
					new ProcedureParameter("@mqcount",    model.mqcount),
            };
            Par.Add(new ProcedureParameter("@id", model.id));

            int rev = PubConn.ExecuteSql("update tb_partition_messagequeue_report set partitionid=@partitionid,day=@day,mqmaxid=@mqmaxid,mqminid=@mqminid,mqcount=@mqcount,lastupdatetime=getdate() where id=@id", Par);
            return rev == 1;

        }

        public bool AddReport(DbConn conn, tb_partition_messagequeue_report_model model)
        {
            return SqlHelper.Visit((ps) =>
         {
             string sql = "SELECT ID from tb_partition_messagequeue_report WITH(NOLOCK) WHERE partitionid=@partitionid AND day=@day";
             ps.Add("@partitionid", model.partitionid);
             ps.Add("@day", model.day);
             object obj = conn.ExecuteScalar(sql, ps.ToParameters());
             if (obj != DBNull.Value && obj != null)
             {
                 int id = Convert.ToInt32(obj);
                 return this.UpdateMsgCount(conn, model.mqmaxid, model.mqminid, model.mqcount, id);
             }
             else
             {
                 return Add2(conn, model);
             }
         });
        }
        public bool UpdateMsgCount(DbConn conn, long maxId, long minId, int count, int id)
        {
            return SqlHelper.Visit((ps) =>
           {
               string sql = "update tb_partition_messagequeue_report set mqmaxid=@mqmaxid,mqminid=@mqminid,mqcount=@mqcount,lastupdatetime=getdate() where id=@id";
               ps.Add("mqmaxid", maxId);
               ps.Add("mqminid", minId);
               ps.Add("mqcount", count);
               ps.Add("id", id);
               int rev = conn.ExecuteSql(sql, ps.ToParameters());
               if (rev > 0)
               {
                   return true;
               }
               return false;
           });
        }
        /// <summary>
        /// 取得已消费的数量
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="lastMqId"></param>
        /// <returns></returns>
        public long GetMsgCount(DbConn conn, long lastMqId)
        {
            return SqlHelper.Visit((ps) =>
            {
                MQIDInfo info = PartitionRuleHelper.GetMQIDInfo(lastMqId);
                string sql = "SELECT SUM(mqcount) FROM [tb_partition_messagequeue_report] WITH(NOLOCK) WHERE [day]<@day AND partitionid=@partitionid";
                ps.Add("@day", info.Day);
                ps.Add("@partitionid", PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = info.DataNodePartition, TablePartition = info.TablePartition }));
                string tableName = PartitionRuleHelper.GetTableName(info.TablePartition, info.Day);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                long msgCount = 0;
                if (obj != DBNull.Value && obj != null)
                {
                    msgCount = LibConvert.ObjToInt64(obj);
                }

                long lastCount = 0;
                using (DbConn nodeConn = DbConfig.CreateConn(DataConfig.DataNodeParConn(PartitionRuleHelper.PartitionNameRule(info.DataNodePartition))))
                {
                    nodeConn.Open();
                    var dal = new tb_messagequeue_dal(); dal.TableName = tableName;
                    lastCount = dal.GetLastDayMsgCount(nodeConn, lastMqId);
                }

                return msgCount + lastCount;
            });
        }
        /// <summary>
        /// 取得未消费的数量
        /// </summary>
        /// <param name="lastMqId"></param>
        /// <returns></returns>
        public long GetNonMsgCount(DbConn conn, long lastMqId)
        {
            return SqlHelper.Visit((ps) =>
          {
              MQIDInfo info = PartitionRuleHelper.GetMQIDInfo(lastMqId);
              var currentday = conn.GetServerDate().Date;

              string sql = "SELECT SUM(mqcount) FROM [tb_partition_messagequeue_report] WITH(NOLOCK) WHERE [day]>@day AND partitionid=@partitionid AND [day]<>@currentday";
              ps.Add("@day", info.Day); ps.Add("@currentday", currentday);
              ps.Add("@partitionid", PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = info.DataNodePartition, TablePartition = info.TablePartition }));

              object obj = conn.ExecuteScalar(sql, ps.ToParameters());
              long msgCount = 0;
              if (obj != DBNull.Value && obj != null)
              {
                  msgCount = LibConvert.ObjToInt64(obj);
              }

              long firstCount = 0; long lastCount = 0;
              using (DbConn nodeConn = DbConfig.CreateConn(DataConfig.DataNodeParConn(PartitionRuleHelper.PartitionNameRule(info.DataNodePartition))))
              {
                  nodeConn.Open();
                  string firsttableName = PartitionRuleHelper.GetTableName(info.TablePartition, info.Day);
                  var msgDal = new tb_messagequeue_dal(); msgDal.TableName = firsttableName;
                  firstCount = msgDal.GetLastDayNonMsgCount(nodeConn, lastMqId);
                  if (info.Day != currentday)//不是今天
                  {
                      string lasttableName = PartitionRuleHelper.GetTableName(info.TablePartition, currentday);
                      var dal = new tb_messagequeue_dal(); dal.TableName = lasttableName;
                      long maxmqid = dal.GetMaxID(nodeConn);
                      if (lastMqId == 0)
                          lastCount = 0;
                      else
                          lastCount = dal.GetLastDayMsgCount(nodeConn, maxmqid);
                  }
              }
              //最后一天剩余
              return msgCount + firstCount + lastCount;
          });
        }
    }
}
