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
    public partial class tb_datanode_dal
    {
        public virtual List<tb_datanode_model> List(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_datanode_model> rs = new List<tb_datanode_model>();

                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_datanode s  WITH(NOLOCK) ");
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
        /// nodeList
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<tb_datanode_model> GetPageList(DbConn conn, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<tb_datanode_model> list = new List<tb_datanode_model>();
            var result = SqlHelper.Visit((ps) =>
             {
                 string sql = "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS rownum,* FROM tb_datanode WITH(NOLOCK)";
                 string countSql = "SELECT COUNT(1) FROM tb_datanode WITH(NOLOCK) ";
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
                         tb_datanode_model model = CreateModel(dr);
                         list.Add(model);
                     }
                 }
                 return list;
             });
            count = tempCount;
            return result;
        }

        /// <summary>
        /// nodeList
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool IsExist(DbConn PubConn, int notid, int datanodepartition)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("datanodepartition", datanodepartition);
                List<tb_datanode_model> rs = new List<tb_datanode_model>();

                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select top 1 id from tb_datanode s  WITH(NOLOCK)  where datanodepartition=@datanodepartition");
                if (notid > 0)
                {
                    ps.Add("id", notid);
                    stringSql.Append(" and id<>@id");
                }
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                return false;
            });
        }
        /// <summary>
        /// 获取所有节点
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public IList<string> GetNodeList(DbConn conn)
        {
           return SqlHelper.Visit((ps) =>
           {
               string sql = "SELECT datanodepartition FROM tb_datanode  WITH(NOLOCK)  ORDER BY  datanodepartition";
               IList<string> list = new List<string>();
               DataTable dt = conn.SqlToDataTable(sql, null);
               string temp = string.Empty;
               if (dt != null && dt.Rows.Count > 0)
               {
                   foreach (DataRow dr in dt.Rows)
                   {
                       int node = LibConvert.ObjToInt(dr["datanodepartition"]);
                       list.Add(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(node));
                   }
               }
               return list;
           });
        }
        public tb_datanode_model GetModelByPartitionId(DbConn conn, int dataNode)
        {
            return SqlHelper.Visit((ps) =>
           {
               List<ProcedureParameter> Par = new List<ProcedureParameter>();
               Par.Add(new ProcedureParameter("@partitionid", dataNode));
               StringBuilder stringSql = new StringBuilder();
               stringSql.Append(@"select s.* from tb_datanode s  WITH(NOLOCK)  where s.datanodepartition=@partitionid");
               DataSet ds = new DataSet();
               conn.SqlToDataSet(ds, stringSql.ToString(), Par);
               if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
               {
                   return CreateModel(ds.Tables[0].Rows[0]);
               }
               return null;
           });
        }
    }
}