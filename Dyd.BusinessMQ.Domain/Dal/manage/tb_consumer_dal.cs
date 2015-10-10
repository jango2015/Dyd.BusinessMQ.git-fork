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
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_consumer_dal
    {
        private tb_consumer_partition_dal partitionDal = new tb_consumer_partition_dal();
        private tb_messagequeue_dal msgDal = new tb_messagequeue_dal();
        private tb_consumer_client_dal clientDal = new tb_consumer_client_dal();
        private tb_partition_messagequeue_report_dal reportDal = new tb_partition_messagequeue_report_dal();

        public List<RegisterdConsumersModel> GetRegisterdConsumers(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.id as tb_consumer_id,tempid as tb_consumer_tempid,s.consumerclientid as tb_consumer_consumerclientid,s.partitionindexs as tb_consumer_partitionindexs
,s.clientname as tb_consumer_clientname,s.lastheartbeat as tb_consumer_lastheartbeat,s.lastupdatetime as tb_consumer_lastupdatetime,s.createtime as tb_consumer_createtime, 
c.id as tb_consumer_client_id, c.client as tb_consumer_client_client,c.createtime as tb_consumer_client_createtime, p.id as tb_consumer_partition_id,p.consumerclientid as tb_consumer_partition_consumerclientid
,p.partitionindex as tb_consumer_partition_partitionindex,p.partitionid as tb_consumer_partition_partitionid,p.lastconsumertempid as tb_consumer_partition_lastconsumertempid,p.lastmqid as tb_consumer_partition_lastmqid
,p.lastupdatetime as tb_consumer_partition_lastupdatetime,p.createtime as tb_consumer_partition_createtime
from tb_consumer s WITH(NOLOCK),tb_consumer_client c WITH(NOLOCK),tb_consumer_partition p WITH(NOLOCK) where s.consumerclientid=c.id and s.consumerclientid=p.consumerclientid and s.tempid=p.lastconsumertempid");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                List<RegisterdConsumersModel> rs = new List<RegisterdConsumersModel>();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        RegisterdConsumersModel m = new RegisterdConsumersModel(dr);

                        rs.Add(m);
                    }

                }
                return rs;
            });
        }

        public IList<RegisterdConsumersModel> GetPageList2(DbConn conn, string partitionid, string consumerclientid, string mqpathid, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<RegisterdConsumersModel> list = new List<RegisterdConsumersModel>();
            ConsumerModel cm = new ConsumerModel();
            var result = SqlHelper.Visit((ps) =>
            {
                StringBuilder where = new StringBuilder(" WHERE 1=1 ");
                if (!string.IsNullOrEmpty(consumerclientid))
                {
                    if (!consumerclientid.isint())
                        where.AppendFormat(" AND  (c.client LIKE '%{0}%')", consumerclientid);
                    else
                        where.AppendFormat(" AND  (c.id = '{0}')", consumerclientid);
                }
                if (!string.IsNullOrEmpty(partitionid))
                {
                    where.AppendFormat(" AND  (p.partitionid='{0}')", partitionid);
                }
                if (!string.IsNullOrEmpty(mqpathid))
                {
                    if (!mqpathid.isint())
                    {
                        where.AppendFormat(" AND  (m.mqpath like '%{0}%')", mqpathid);
                    }
                    else
                    {
                        where.AppendFormat(" AND  (m.id='{0}')", mqpathid);
                    }
                }
                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY p.consumerclientid DESC,p.partitionindex desc) AS rownum, s.id as tb_consumer_id,tempid as tb_consumer_tempid,s.consumerclientid as tb_consumer_consumerclientid,s.partitionindexs as tb_consumer_partitionindexs
,s.clientname as tb_consumer_clientname,s.lastheartbeat as tb_consumer_lastheartbeat,s.lastupdatetime as tb_consumer_lastupdatetime,s.createtime as tb_consumer_createtime, 
c.id as tb_consumer_client_id, c.client as tb_consumer_client_client,c.createtime as tb_consumer_client_createtime, p.id as tb_consumer_partition_id,p.consumerclientid as tb_consumer_partition_consumerclientid
,p.partitionindex as tb_consumer_partition_partitionindex,p.partitionid as tb_consumer_partition_partitionid,p.lastconsumertempid as tb_consumer_partition_lastconsumertempid,p.lastmqid as tb_consumer_partition_lastmqid
,p.lastupdatetime as tb_consumer_partition_lastupdatetime,p.createtime as tb_consumer_partition_createtime,m.mqpath as tb_mqpath_mqpath,m.id as tb_mqpath_id
from tb_consumer_partition p WITH(NOLOCK) left join tb_consumer s WITH(NOLOCK) on s.tempid=p.lastconsumertempid left join tb_consumer_client c WITH(NOLOCK) on p.consumerclientid=c.id  left join tb_mqpath_partition mp with (nolock) on mp.partitionid=p.partitionid left join tb_mqpath m with (nolock) on m.id=mp.mqpathid ";
                string countSql = "SELECT COUNT(p.id) from tb_consumer_partition p WITH(NOLOCK) left join tb_consumer s WITH(NOLOCK) on s.tempid=p.lastconsumertempid left join tb_consumer_client c WITH(NOLOCK) on p.consumerclientid=c.id   left join tb_mqpath_partition mp with (nolock) on mp.partitionid=p.partitionid left join tb_mqpath m with (nolock) on m.id=mp.mqpathid " + where.ToString();
                object obj = conn.ExecuteScalar(countSql, null);
                if (obj != DBNull.Value && obj != null)
                {
                    tempCount = LibConvert.ObjToInt(obj);
                }
                string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), where.ToString(), ") A WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
                DataTable dt = conn.SqlToDataTable(sqlPage, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        RegisterdConsumersModel model = new RegisterdConsumersModel(dr);
                        model.msgCount = reportDal.GetMsgCount(conn, model.consumerpartitionmodel.lastmqid);
                        model.nonMsgCount = reportDal.GetNonMsgCount(conn, model.consumerpartitionmodel.lastmqid);
                        model.mqpath = dr["tb_mqpath_mqpath"].Tostring();
                        model.mqpathid = dr["tb_mqpath_id"].Toint();
                        list.Add(model);
                    }

                }
                return list;
            });
            count = tempCount;
            return result;
        }
        public IList<ConsumerModel> GetPageList(DbConn conn, string name, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<ConsumerModel> list = new List<ConsumerModel>();
            ConsumerModel cm = new ConsumerModel();
            var result = SqlHelper.Visit((ps) =>
            {
                StringBuilder where = new StringBuilder(" WHERE 1=1 ");
                if (!string.IsNullOrEmpty(name))
                {
                    where.AppendFormat(" AND  clientname LIKE '%{0}%'", name);
                }
                string sql = "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS rownum,* FROM tb_consumer WITH(NOLOCK)";
                string countSql = "SELECT COUNT(1) FROM tb_consumer WITH(NOLOCK) " + where.ToString();
                object obj = conn.ExecuteScalar(countSql, null);
                if (obj != DBNull.Value && obj != null)
                {
                    tempCount = LibConvert.ObjToInt(obj);
                }
                string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), where.ToString(), ") A WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
                DataTable dt = conn.SqlToDataTable(sqlPage, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ConsumerModel model = cm.CreateModel(dr);

                        IList<tb_consumer_partition_model> consumerList = partitionDal.GetPartitionByConsumerId(conn, model.consumerclientid);
                        if (consumerList != null && consumerList.Count > 0)
                        {
                            IList<ConsumerPartition> partitionList = new List<ConsumerPartition>();

                            foreach (var item in consumerList)
                            {
                                ConsumerPartition m = new ConsumerPartition();
                                m.PartitionId = item.partitionid;

                                PartitionIDInfo partitionInfo = PartitionRuleHelper.GetPartitionIDInfo(item.partitionid);
                                string node = string.Empty;
                                if (partitionInfo.DataNodePartition < 10)
                                {
                                    node = "0" + partitionInfo.DataNodePartition.Tostring();
                                }
                                else
                                {
                                    node = partitionInfo.DataNodePartition.Tostring();
                                }

                                using (DbConn nodeConn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
                                {
                                    nodeConn.Open();
                                    tb_partition_model partitionModel = new tb_partition_dal().Get(conn, item.partitionid);
                                    if (partitionModel != null)
                                    {
                                        m.IsOnline = partitionModel.isused;
                                    }
                                    string table = msgDal.GetMaxMqTable(nodeConn, node);
                                    m.Msg = msgDal.GetMsgCount(nodeConn, table, 0);
                                    m.NonMsg = msgDal.GetMsgCount(nodeConn, table, 1);
                                    partitionList.Add(m);
                                }
                            }
                            model.PartitionList = partitionList;
                        }
                        list.Add(model);
                    }
                }
                return list;
            });
            count = tempCount;
            return result;
        }
        public bool DeleteNotOnLineByClientID(DbConn PubConn, int consumerclientid, int maxtimeoutsenconds)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("@consumerclientid", consumerclientid); ps.Add("@maxtimeoutsenconds", maxtimeoutsenconds);
                string Sql = "delete from tb_consumer where consumerclientid=@consumerclientid and DATEDIFF(S,lastheartbeat,getdate())>@maxtimeoutsenconds";
                int rev = PubConn.ExecuteSql(Sql, ps.ToParameters());
                return true;
            });
        }
        public IList<tb_consumer_model> GetConsumerList(DbConn conn, ICollection<int> array)
        {
            return SqlHelper.Visit((ps) =>
            {
                if (array == null || array.Count <= 0) return null;
                IList<tb_consumer_model> list = new List<tb_consumer_model>();
                foreach (var item in array)
                {
                    tb_consumer_model model = GetModel(conn, item);
                    if (model != null)
                        list.Add(model);
                }
                return list;
            });
        }
        public tb_consumer_model GetModel(DbConn conn, int consumerclientid)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "SELECT * FROM  tb_consumer  WITH(NOLOCK)  WHERE consumerclientid=@id";
                ps.Add("@id", consumerclientid);
                DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_consumer_model model = CreateModel(dr);
                        return model;
                    }
                }
                return null;
            });
        }

        public virtual tb_consumer_model GetByTempId(DbConn PubConn, long tempid)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@tempid", tempid));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_consumer s  WITH(NOLOCK)  where s.tempid=@tempid");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

        public virtual List<tb_consumer_model> GetAllList(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_consumer_model> rs = new List<tb_consumer_model>();
                string sql = "SELECT * FROM  tb_consumer  WITH(NOLOCK) ";
                DataTable dt = PubConn.SqlToDataTable(sql, ps.ToParameters());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_consumer_model model = CreateModel(dr);
                        rs.Add(model);
                    }
                }
                return rs;
            });

        }
        public List<tb_partition_model> GetClientCreateTime(DbConn conn)
        {
            try
            {
                List<tb_partition_model> list = new List<tb_partition_model>();
                string sql = @"select partitionid,clientname,tb_consumer_partition.createtime from tb_consumer_partition,[tb_consumer]  WITH(NOLOCK) 
                                where [tb_consumer].tempid=tb_consumer_partition.lastconsumertempid";
                DataTable dt = conn.SqlToDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_partition_model model = new tb_partition_model();
                        model.partitionid = Convert.ToInt32(dr["partitionid"]);
                        model.createtime = Convert.ToDateTime(dr["createtime"]);
                        list.Add(model);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}