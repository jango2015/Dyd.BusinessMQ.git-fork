using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;
using XXF.ProjectTool;
using System.Linq;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace Dyd.BusinessMQ.Domain.Dal
{
    /*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
    public partial class tb_messagequeue_dal
    {
        public string TableName { get; set; }

        public long GetMaxID(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select max(id) from {0} s WITH(NOLOCK)", TableName));
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0] is DBNull)
                        return 0;
                    return Convert.ToInt64(ds.Tables[0].Rows[0][0]);
                }
                return 0;
            });

        }

        public long GetMinID(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select min(id) from {0} s WITH(NOLOCK)", TableName));
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0] is DBNull)
                        return 0;
                    return Convert.ToInt64(ds.Tables[0].Rows[0][0]);
                }
                return 0;
            });

        }
        public long GetMaxID(DbConn PubConn,DateTime time)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select max(id) from {0}   s  WITH(NOLOCK) where mqcreatetime>@time", TableName));
                DataSet ds = new DataSet();
                ps.Add("@time", time);
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0] is DBNull)
                        return 0;
                    return Convert.ToInt64(ds.Tables[0].Rows[0][0]);
                }
                return 0;
            });

        }

        public long GetMinID(DbConn PubConn,DateTime time)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select min(id) from {0}  s  WITH(NOLOCK) where mqcreatetime>@time", TableName));
                DataSet ds = new DataSet();
                ps.Add("@time", time);
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0] is DBNull)
                        return 0;
                    return Convert.ToInt64(ds.Tables[0].Rows[0][0]);
                }
                return 0;
            });

        }

        public int GetCount(DbConn PubConn,DateTime time)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select count(0) from {0} s WITH(NOLOCK) where mqcreatetime>@time ", TableName));
                DataSet ds = new DataSet();
                ps.Add("@time", time);
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0][0] is DBNull)
                        return 0;
                    return Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                }
                return 0;
            });

        }

       

        public KeyValuePair<long, DateTime> GetIDCreateTime(DbConn PubConn, long id)
        {
            return SqlHelper.Visit((ps) =>
            {
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select top 1 id,sqlcreatetime from {0} s WITH(NOLOCK) where id>=@id order by id asc", TableName));
                ps.Add("id", id);
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return new KeyValuePair<long, DateTime>(Convert.ToInt64(ds.Tables[0].Rows[0]["id"]), Convert.ToDateTime(ds.Tables[0].Rows[0]["sqlcreatetime"]));
                }
                return new KeyValuePair<long, DateTime>();
            });

        }

        public List<tb_messagequeue_model> GetListMoreThanID(DbConn PubConn, int topcount, long id)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_messagequeue_model> rs = new List<tb_messagequeue_model>();
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(string.Format(@"select top {1} * from {0} s WITH(NOLOCK) where id>@id", TableName, topcount));
                ps.Add("id", id);
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                        rs.Add(CreateModel(dr));
                }
                return rs;
            });

        }

        public virtual bool AddMove(DbConn PubConn, tb_messagequeue_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),

					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message)   
                };
            int rev = PubConn.ExecuteSql(string.Format(@"insert into {0}(mqcreatetime,sqlcreatetime,state,source,message)
										   values(@mqcreatetime,getdate(),@state,@source,@message)", TableName), Par);
            return rev == 1;

        }

        public virtual bool SetState(DbConn PubConn, long id, byte state)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                  					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",   state),
            };
            Par.Add(new ProcedureParameter("@id", id));

            int rev = PubConn.ExecuteSql(string.Format("update {0} set state=@state where id=@id", TableName), Par);
            return rev == 1;

        }

        /// <summary>
        ///根据节点获取所有分区
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<int> GetTablePartitions(DbConn conn)
        {
            var rs = new List<int>();
            foreach (var d in GetDataNodeTable(conn))
            {
                if (!rs.Contains(d.TablePartition))
                    rs.Add(d.TablePartition);
            }
            return rs;
        }
        /// <summary>
        ///根据节点获取某个分区的时间
        /// </summary>
        public List<DateTime> GetDayPartitions(DbConn conn, int tablepartition)
        {
            var rs = new List<DateTime>();
            foreach (var d in GetDataNodeTable(conn))
            {
                if (d.TablePartition == tablepartition && !rs.Contains(d.Day))
                    rs.Add(d.Day);
            }
            return rs;
        }
        /// <summary>
        /// 获取指定node节点下的所有表名
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<TableInfo> GetDataNodeTable(DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
           {
               string sql = "SELECT Name FROM SysObjects  WITH(NOLOCK) Where XType='U' and name like 'tb_messagequeue_%' order BY Name DESC";
               List<TableInfo> list = new List<TableInfo>();
               DataTable dt = conn.SqlToDataTable(sql, null);
               if (dt != null && dt.Rows.Count > 0)
               {
                   foreach (DataRow dr in dt.Rows)
                   {
                       list.Add(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableInfo(Convert.ToString(dr["Name"])));
                   }
               }
               return list;
           });
        }

        public List<TableInfo> GetDataNodeTable(DbConn conn, DateTime fromcreatedate)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "SELECT Name FROM SysObjects  WITH(NOLOCK)  Where XType='U' and name like 'tb_messagequeue_%' and crdate>=@crdate order by name desc";
                ps.Add("crdate", fromcreatedate);
                List<TableInfo> list = new List<TableInfo>();
                DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        list.Add(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableInfo(Convert.ToString(dr["Name"])));
                    }
                }
                return list;
            });
        }

        /// <summary>
        /// 获取消息的数量
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetMsgCount(DbConn conn, string tableName, int state)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = string.Format("SELECT COUNT(1) FROM {0} WITH(NOLOCK) WHERE state=@state", tableName);
                ps.Add("@state", state);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                if (obj != DBNull.Value && obj != null)
                {
                    return LibConvert.ObjToInt(obj);
                }
                return 0;
            });
        }
        public IList<tb_messagequeue_model> GetPageList(DbConn conn, int pageIndex, int pageSize, string id, ref int count)
        {
            int tempCount = 0;
            IList<tb_messagequeue_model> list = new List<tb_messagequeue_model>();
            var result = SqlHelper.Visit((ps) =>
            {
                StringBuilder where = new StringBuilder(" WHERE 1=1");
                if (!string.IsNullOrWhiteSpace(id))
                {
                    where.AppendFormat(" AND id={0}", id);
                }
                string sql = string.Format("SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS rownum,* FROM {0}  WITH(NOLOCK)", TableName) + where;
                string countSql = string.Format("SELECT COUNT(1) FROM {0} WITH(NOLOCK)", TableName) + where;
                object obj = conn.ExecuteScalar(countSql, null);
                if (obj != DBNull.Value && obj != null)
                {
                    tempCount = LibConvert.ObjToInt(obj);
                }
                string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), ") A WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
                DataTable dt = conn.SqlToDataTable(sqlPage, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_messagequeue_model model = CreateModel(dr);
                        list.Add(model);
                    }
                }
                return list;
            });
            count = tempCount;
            return result;
        }
        //public IList<tb_messagequeue_model> GetPageList(long id, int pageIndex, int pageSize, ref int count)
        //{
        //    MQIDInfo info = PartitionRuleHelper.GetMQIDInfo(id);
        //    string node = string.Empty;
        //    string partition = string.Empty;

        //    if (info.DataNodePartition < 10)
        //    {
        //        node = string.Concat("0", info.DataNodePartition.Tostring());
        //    }
        //    else
        //    {
        //        node = info.DataNodePartition.Tostring();
        //    }
        //    if (info.TablePartition < 10)
        //    {
        //        partition = string.Concat("0", info.TablePartition.Tostring());
        //    }
        //    else
        //    {
        //        partition = info.TablePartition.Tostring();
        //    }
        //    string tableName = string.Concat("tb_messagequeue_", node, "_", info.Day.ToString("yyMMdd"));
        //    using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
        //    {
        //        conn.Open();
        //        return this.GetPageList(conn, tableName, pageIndex, pageSize, id, 0, ref count);
        //    }
        //}
        public bool Delete(DbConn conn, long id, string tableName)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = string.Format("DELETE FROM {0} WHERE id=@id", tableName);
                ps.Add("@id", id);
                return conn.ExecuteSql(sql, ps.ToParameters()) > 0;
            });
        }
        public bool Add(DbConn conn, string tableName, tb_messagequeue_model model)
        {
            return SqlHelper.Visit((ps) =>
          {
              string sql = string.Format("insert into {0}(mqcreatetime,sqlcreatetime,state,source,message)  values(@mqcreatetime,@sqlcreatetime,@state,@source,@message)", tableName);
              List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),
					//sql数据节点处的创建时间
					new ProcedureParameter("@sqlcreatetime",    model.sqlcreatetime),
					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message)   
                };
              int rev = conn.ExecuteSql(sql, Par);
              return rev == 1;
          });
        }
        public tb_messagequeue_model GetModel(DbConn conn, long id, string tableName)
        {
            return SqlHelper.Visit((ps) =>
           {
               tb_messagequeue_model model = null;
               string sql = string.Format("SELECT TOP 1 * FROM {0}  WITH(NOLOCK)  WHERE id=@id", tableName);
               ps.Add("@id", id);
               DataTable dt = conn.SqlToDataTable(sql, ps.ToParameters());
               if (dt != null && dt.Rows.Count > 0)
               {
                   foreach (DataRow dr in dt.Rows)
                   {
                       model = CreateModel(dr);
                   }
               }
               return model;
           });
        }
        public bool Update(DbConn conn, tb_messagequeue_model model, string tableName)
        {
            return SqlHelper.Visit((ps) =>
          {
              List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),
					//sql数据节点处的创建时间
					new ProcedureParameter("@sqlcreatetime",    model.sqlcreatetime),
					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message)
            };
              Par.Add(new ProcedureParameter("@id", model.id));

              string sql = string.Format("update {0} set mqcreatetime=@mqcreatetime,sqlcreatetime=@sqlcreatetime,state=@state,source=@source,message=@message where id=@id", tableName);
              int rev = conn.ExecuteSql(sql, Par);
              return rev == 1;
          });
        }

        public string GetMaxMqTable(DbConn conn, string node)
        {
            IDictionary<int, string> dic = this.GetMqByPartitionId(conn, node);
            if (dic == null || dic.Count <= 0) return string.Empty;

            string tableName = "tb_messagequeue_";
            string result = string.Concat(tableName, node, "_", dic.Keys.FirstOrDefault());
            return result;
        }
        /// <summary>
        /// 根据节点获取所有的分区集合
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public IDictionary<int, string> GetMqByPartitionId(DbConn conn, string node)
        {
            ICollection<string> array = this.GetDataNodeTableString(conn);
            IDictionary<int, string> dic = new Dictionary<int, string>();
            if (array == null || array.Count <= 0) return null;

            foreach (var item in array)
            {
                IList<string> result = item.Split('_');
                if (result != null && result.Count > 3)
                {
                    int date = LibConvert.ObjToInt(result[3]);
                    string nodeId = result[2].Tostring();
                    if (!dic.ContainsKey(date) && nodeId.Equals(node))
                    {
                        dic.Add(date, result[2]);
                    }
                }
            }
            return dic;
        }
        public ICollection<string> GetTableNameListByPartition(DbConn conn, string partitionId)
        {
            ICollection<string> array = this.GetDataNodeTableString(conn);
            if (array == null || array.Count <= 0) return null;

            IList<string> list = new List<string>();

            foreach (var item in array)
            {
                IList<string> result = item.Split('_');
                if (result != null && result.Count > 3)
                {
                    string partition = result[2].Tostring();
                    if (partition.Equals(partitionId))
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 获取指定node节点下的所有表名
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public ICollection<string> GetDataNodeTableString(DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "SELECT Name FROM SysObjects  WITH(NOLOCK)  Where XType='U' order BY Name DESC";
                IList<string> list = new List<string>();
                DataTable dt = conn.SqlToDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        list.Add(dr["Name"].Tostring());
                    }
                }
                return list;
            });
        }
        /// <summary>
        /// 取得最后一天未消费的数量
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="lastMqId"></param>
        /// <returns></returns>
        public long GetLastDayNonMsgCount(DbConn conn, long lastMqId)
        {
            return SqlHelper.Visit((ps) =>
            {
                long maxId = this.GetMaxID(conn);
                if (maxId == 0)
                {
                    return 0;
                    //var tablenameinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableInfo(tableName);
                    //var mqidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(lastMqId);
                    //maxId = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQID(new MQIDInfo() { AutoID = 0, DataNodePartition = mqidinfo.DataNodePartition, TablePartition = tablenameinfo.TablePartition, Day = tablenameinfo.Day });
                }
                var lastMqInfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(lastMqId);
                long count=0;
                if (lastMqInfo.AutoID == 0)
                    count= maxId - lastMqId;
                else
                    count = maxId - lastMqId;
                if (count > 0)
                    return count;
                return 0;
            });
        }
        /// <summary>
        /// 取得最后一天已消费的数量
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="lastMqId"></param>
        /// <returns></returns>
        public long GetLastDayMsgCount(DbConn conn, long lastMqId)
        {
            return SqlHelper.Visit((ps) =>
             {
                 long minId = this.GetMinID(conn);
                 if (minId == 0)
                 {
                     return 0;
                     //var tablenameinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableInfo(tableName);
                     //var mqidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(lastMqId);
                     //minId = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQID(new MQIDInfo() { AutoID = 0, DataNodePartition = mqidinfo.DataNodePartition, TablePartition = tablenameinfo.TablePartition, Day = tablenameinfo.Day });
                 }
                 var lastMqInfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(lastMqId);
                 if (lastMqInfo.AutoID == 0)
                     return 0;
                 long count = (lastMqId - minId)+1;
                 if (count > 0)
                     return count;
                 return 0;
             });
        }
    }
}