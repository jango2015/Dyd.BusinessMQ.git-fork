using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_partition_dal
    {
        public virtual bool Add2(DbConn PubConn, tb_partition_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//是否已使用
					new ProcedureParameter("@isused",    model.isused),
                 new ProcedureParameter("@partitionid",    model.partitionid)   ,
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_partition(partitionid,isused,createtime)
										   values(@partitionid,@isused,getdate())", Par);
            return rev == 1;

        }

        public virtual List<tb_partition_model> List(DbConn PubConn, bool isused)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_partition_model> rs = new List<tb_partition_model>();
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_partition s WITH(NOLOCK)  where s.isused=@isused");
                ps.Add("isused", isused);
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
        /// <summary>
        /// 获取所有分区
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<tb_partition_model> GetAllCanUsePartitionList(DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_partition_model> rs = new List<tb_partition_model>();
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select * from tb_partition with(nolock) where partitionid not in (select partitionid from tb_mqpath_partition with(nolock))  order by partitionid asc");
                DataSet ds = new DataSet();
                conn.SqlToDataSet(ds, stringSql.ToString(), null);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                        rs.Add(CreateModel(dr));
                }
                return rs;
            });
        }
        /// <summary>
        /// 获取所有分区
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<tb_partition_model> GetAllCanUsePartitionList(DbConn conn,int datapartitionid)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_partition_model> rs = new List<tb_partition_model>();
                StringBuilder stringSql = new StringBuilder();
                stringSql.AppendFormat(@"select * from tb_partition with(nolock) where partitionid not in (select partitionid from tb_mqpath_partition with(nolock)) and partitionid LIKE '1{0}%'  order by partitionid asc", XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(datapartitionid));
                DataSet ds = new DataSet();
                conn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                        rs.Add(CreateModel(dr));
                }
                return rs;
            });
        }
        /// <summary>
        /// 根据node节点判断是否存在分区
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExistPartition(DbConn conn, int id)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = string.Format("SELECT COUNT(1) FROM tb_partition WITH(NOLOCK) WHERE partitionid LIKE '1{0}%'", XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(id));
                object obj = conn.ExecuteScalar(sql, null);
                if (obj != DBNull.Value && obj != null)
                    return (Convert.ToInt32(obj)>0?true:false);
                return false;
            });
        }
        /// <summary>
        /// 根据node节点取得分区的节点前缀
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public string GetNodePartition(int nodeId)
        {
            return SqlHelper.Visit((ps) =>
           {
               StringBuilder result = new StringBuilder("1");
               if (nodeId < 10)
               {
                   result.Append("0" + nodeId.Tostring());
               }
               else
               {
                   result.Append(nodeId.Tostring());
               }
               return result.ToString();
           });
        }
        /// <summary>
        /// 删除分区，0:正在使用，1，删除成功；-2：删除失败，-1，数据不存在
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="partitionId"></param>
        /// <returns></returns>
        public int DeletePartition(DbConn conn, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
            {
                tb_partition_model model = Get(conn, partitionId);
                if (model != null)
                {
                    if (model.isused)
                        return 0;      //正在使用，不允许删除
                    else
                    {
                        if (Delete(conn, partitionId))
                            return 1;      //删除成功
                        else
                            return -2;   //删除失败
                    }
                }
                else
                {
                    return -1;    //不存在
                }
            });
        }
        /// <summary>
        /// 添加分区
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int AddPartition(DbConn conn, tb_partition_model model)
        {
            return SqlHelper.Visit((ps) =>
            {
                tb_partition_model result = Get(conn, model.partitionid);
                if (result != null)
                    return 0;          //已存在
                return Add2(conn, model) ? 1 : -1;
            });
        }
        public bool UpdateIsUsed(DbConn conn, int isUsed, int partitionId)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = "UPDATE [tb_partition] SET isused=@isused WHERE partitionId=@partitionId";
                ps.Add("@isused", isUsed);
                ps.Add("@partitionId", partitionId);
                return conn.ExecuteSql(sql, ps.ToParameters()) > 0;
            });
        }
        /// <summary>
        /// 根据node节点获取分区节点
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="nodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<tb_partition_model> GetPageList(DbConn conn,string partitionid, int nodeId,int used, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<tb_partition_model> list = new List<tb_partition_model>();
            var result = SqlHelper.Visit((ps) =>
             {
                 StringBuilder where = new StringBuilder(" WHERE 1=1 ");
                 string sql = "SELECT ROW_NUMBER() OVER(ORDER BY createtime DESC) AS rownum,* FROM tb_partition WITH(NOLOCK)";
                 if (nodeId > 0)
                 {
                     string partitionId = this.GetNodePartition(nodeId);
                     where.AppendFormat("AND partitionid LIKE '{0}%' ", partitionId);
                 } 
                 if (used >= 0)
                 {
                     where.AppendFormat("AND isused = '{0}' ", used);
                 }
                 if (!string.IsNullOrWhiteSpace(partitionid))
                 {
                     where.AppendFormat("AND partitionid = '{0}' ", partitionid);
                 }
                 string countSql = string.Concat("SELECT COUNT(1) FROM tb_partition WITH(NOLOCK)", where);
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
                         tb_partition_model model = CreateModel(dr);
                         list.Add(model);
                     }
                 }
                 return list;
             });
            count = tempCount;
            return result;
        }
    }
}