using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;
using XXF.ProjectTool;
using Dyd.BusinessMQ.Domain.Model.manage;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_consumer_partition_dal
    {
        private tb_partition_messagequeue_report_dal reportDal = new tb_partition_messagequeue_report_dal();
        public long GetLastMqIdByPartitionId(DbConn conn, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
           {
               string sql = string.Format("SELECT lastmqid FROM tb_consumer_partition WITH(NOLOCK) WHERE partitionId={0}", partitionId.Tostring());
               object obj = conn.ExecuteScalar(sql, null);
               if (obj != DBNull.Value && obj != null)
               {
                   return LibConvert.ObjToInt64(obj);
               }
               return 0;
           });
        }

        public bool Delete2(DbConn PubConn, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
          {
              List<ProcedureParameter> Par = new List<ProcedureParameter>();
              Par.Add(new ProcedureParameter("@partitionId", partitionId));

              string Sql = "delete from tb_consumer_partition where partitionId=@partitionId";
              int rev = PubConn.ExecuteSql(Sql, Par);
              if (rev == 1)
              {
                  return true;
              }
              else
              {
                  return false;
              }
          });
        }
        
        public int UpdateLastMqIdByPartitionId(DbConn conn, int id, long lastmqid)
        {
            return SqlHelper.Visit((ps) =>
           {
               string sql = string.Format("update tb_consumer_partition set lastmqid={1}  WHERE id={0}", id, lastmqid);
               return conn.ExecuteSql(sql, null);
           });
        }


        public List<ConsumerPartitionModel> ListByPartitionIds(DbConn conn, List<int> partitionids)
        {
            return SqlHelper.Visit((ps) =>
            {
                var pps = ps.ToParameters();
                List<ConsumerPartitionModel> list = new List<ConsumerPartitionModel>();
                if (partitionids.Count > 0)
                {
                    string sql = string.Format("SELECT p.*,c.client FROM tb_consumer_partition  p WITH(NOLOCK),tb_consumer_client c WITH(NOLOCK) WHERE p.consumerclientid=c.id and p.partitionid in ({0})", SqlHelper.CmdIn<int>(pps, partitionids));
                    DataTable dt = conn.SqlToDataTable(sql, pps);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            ConsumerPartitionModel m = new ConsumerPartitionModel();
                            m.consumerpartitionmodel = CreateModel(dr);
                            m.msgCount = reportDal.GetMsgCount(conn, m.consumerpartitionmodel.lastmqid);
                            m.nonMsgCount = reportDal.GetNonMsgCount(conn,m.consumerpartitionmodel.lastmqid);
                            m.client = Convert.ToString(dr["client"]);
                            list.Add(m);
                        }
                    }
                }
                return list;
            });
        }

        public IList<tb_consumer_partition_model> GetPartitionByConsumerId(DbConn conn, int consumerClientId)
        {
            return SqlHelper.Visit((ps) =>
            {
                IList<tb_consumer_partition_model> list = new List<tb_consumer_partition_model>();
                string sql = "SELECT * FROM tb_consumer_partition WITH(NOLOCK) WHERE consumerclientid=@id";
                ps.Add("@id", consumerClientId);
                DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_consumer_partition_model model = CreateModel(dr);
                        list.Add(model);
                    }
                }
                return list;
            });
        }
        public IList<int> GetPartitionByPartitionId(DbConn conn, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
            {
                IList<int> list = new List<int>();
                string sql = "SELECT * FROM tb_consumer_partition WITH(NOLOCK) WHERE partitionId=@id";
                ps.Add("@id", partitionId);
                DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_consumer_partition_model model = CreateModel(dr);
                        list.Add(model.consumerclientid);
                    }
                }
                return list;
            });
        }

        public tb_consumer_partition_model GetByPartitionId(DbConn conn, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
            {
                IList<int> list = new List<int>();
                string sql = "SELECT top 1 * FROM tb_consumer_partition WITH(NOLOCK) WHERE partitionId=@id";
                ps.Add("@id", partitionId);
                DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
                tb_consumer_partition_model model = null;
                if (dt != null && dt.Rows.Count > 0)
                {
                    model = CreateModel(dt.Rows[0]);
                }
                return model;
            });
        }

        public int GetActiveConsumerCount(DbConn conn, int mqpathId)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "SELECT COUNT (p.consumerclientid) FROM tb_consumer_partition p WITH(NOLOCK),tb_consumer c WITH(NOLOCK),tb_mqpath_partition m WITH(NOLOCK) WHERE p.lastconsumertempid=c.tempid and p.partitionid=m.partitionid and m.mqpathid=@mqpathid";
                ps.Add("@mqpathid", mqpathId);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                if (obj != DBNull.Value && obj != null)
                {
                    return LibConvert.ObjToInt(obj);
                }
                return 0;
            });
        }

        public int GetLogoutConsumerCount(DbConn conn, int mqpathId)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "SELECT COUNT(p.consumerclientid) FROM tb_consumer_partition p WITH(NOLOCK),tb_mqpath_partition m WITH(NOLOCK) where p.partitionid=m.partitionid  and m.mqpathid=@mqpathid and p.lastconsumertempid not in (select distinct(lastconsumertempid) from  tb_consumer WITH(NOLOCK))";
                ps.Add("@mqpathid", mqpathId);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                if (obj != DBNull.Value && obj != null)
                {
                    return LibConvert.ObjToInt(obj);
                }
                return 0;
            });
        }
    }
}