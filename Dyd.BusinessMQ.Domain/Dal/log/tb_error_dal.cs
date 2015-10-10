using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_error_dal
    {
        /// <summary>
        /// errorLog
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<tb_error_model> GetPageList(DbConn conn, DateTime? startTime, DateTime? endTime, string mqpathid, string mqpath, string methodname, string info, int pageSize, int pageIndex, ref int count)
        {
            int tempCount = 0;
            var result = SqlHelper.Visit((ps) =>
           {
               IList<tb_error_model> list = new List<tb_error_model>();
               StringBuilder where = new StringBuilder();
               List<ProcedureParameter> parameters = new List<ProcedureParameter>();
               where.Append(" WHERE 1=1");
               if (startTime != null && endTime != null)
               {
                   parameters.Add(new ProcedureParameter("startTime", startTime.Value.ToString("yyyy-MM-dd")));
                   parameters.Add(new ProcedureParameter("endTime", endTime.Value.ToString("yyyy-MM-dd")));
                   where.Append(" AND createtime>=@startTime AND createtime<=@endTime ");
               }
               if (!string.IsNullOrWhiteSpace(mqpathid))
               {
                   parameters.Add(new ProcedureParameter("mqpathid", mqpathid));
                   where.Append(" AND mqpathid=@mqpathid ");
               }
               if (!string.IsNullOrWhiteSpace(mqpath))
               {
                   parameters.Add(new ProcedureParameter("mqpath", mqpath));
                   where.Append(" AND mqpath=@mqpath ");
               }
               if (!string.IsNullOrWhiteSpace(methodname))
               {
                   parameters.Add(new ProcedureParameter("methodname", methodname));
                   where.Append(" AND methodname like '%'+@methodname+'%' ");
               }
               if (!string.IsNullOrWhiteSpace(info))
               {
                   parameters.Add(new ProcedureParameter("info", info));
                   where.Append(" AND info like '%'+@info+'%' ");
               }
               StringBuilder sql = new StringBuilder();
               sql.Append("SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS rownum,* FROM tb_error WITH(NOLOCK)");
               string countSql = string.Concat("SELECT COUNT(1) FROM tb_error WITH(NOLOCK) ", where.ToString());
               object obj = conn.ExecuteScalar(countSql, parameters);
               if (obj != DBNull.Value && obj != null)
               {
                   tempCount = LibConvert.ObjToInt(obj);
               }
               string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), where.ToString(), ") A WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
               DataTable dt = conn.SqlToDataTable(sqlPage, parameters);
               if (dt != null && dt.Rows.Count > 0)
               {
                   foreach (DataRow dr in dt.Rows)
                   {
                       tb_error_model model = CreateModel(dr);
                       list.Add(model);
                   }
               }
               return list;
           });
            count = tempCount;
            return result;
        }

        public void DeleteAll(DbConn PubConn)
        {
            SqlHelper.Visit((ps) =>
            {
                string Sql = "truncate table tb_error";
                int rev = PubConn.ExecuteSql(Sql, ps.ToParameters());
                return rev;
            });
        }
    }
}
