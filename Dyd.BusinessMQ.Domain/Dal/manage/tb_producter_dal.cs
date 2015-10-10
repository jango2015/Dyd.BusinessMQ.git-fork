using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.ProjectTool;
using Dyd.BusinessMQ.Domain.Model.manage;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_producter_dal
    {
        public IList<tb_producterview_model> GetPageList(DbConn conn, string mqpathid, string name, string ip, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<tb_producterview_model> list = new List<tb_producterview_model>();
            var result = SqlHelper.Visit((ps) =>
            {
                StringBuilder where = new StringBuilder("");// WHERE 1=1
                if (!string.IsNullOrEmpty(name))
                {
                    where.AppendFormat(" AND p.productername LIKE '%{0}%'", name);
                }
                if (!string.IsNullOrEmpty(ip))
                {
                    where.AppendFormat(" AND p.ip='{0}'", ip);
                }
                if (!string.IsNullOrWhiteSpace(mqpathid))
                {
                    int temp = 0;
                    if (int.TryParse(mqpathid, out temp))
                    {
                        where.AppendFormat(" and (p.mqpathid='{0}')", mqpathid);
                    }
                    else
                    {
                        where.AppendFormat(" and (m.mqpath like '%'+'{0}'+'%')", mqpathid);
                    }
                }
                string sql = "SELECT ROW_NUMBER() OVER(ORDER BY p.Id DESC) AS rownum,p.*,m.mqpath FROM tb_producter p WITH(NOLOCK),tb_mqpath m WITH(NOLOCK) where p.mqpathid=m.id ";
                string countSql = "SELECT COUNT(1) FROM tb_producter p WITH(NOLOCK),tb_mqpath m WITH(NOLOCK) where p.mqpathid=m.id " + where;
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
                        tb_producterview_model model = new tb_producterview_model();
                        model.ProducterModel = CreateModel(dr);
                        model.mqpath = Convert.ToString(dr["mqpath"]);
                        list.Add(model);
                    }
                }
                return list;
            });
            count = tempCount;
            return result;
        } 
        
        public int GetProductCount(DbConn conn, int mqPathId,int sec)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = string.Format("SELECT COUNT(1) FROM tb_producter WITH(NOLOCK) WHERE mqpathid=@mqpathid AND datediff(s,lastheartbeat,getdate())<{0}",sec);
                ps.Add("@mqpathid", mqPathId);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                if (obj != DBNull.Value && obj != null)
                {
                    return LibConvert.ObjToInt(obj);
                }
                return 0;
            });
        }
        public int GetNonProductCount(DbConn conn, int mqPathId, int sec)
        {
            return SqlHelper.Visit((ps) =>
            {
                string sql = string.Format("SELECT COUNT(1) FROM tb_producter WITH(NOLOCK) WHERE mqpathid=@mqpathid AND datediff(s,lastheartbeat,getdate())>={0}", sec);
                ps.Add("@mqpathid", mqPathId);
                object obj = conn.ExecuteScalar(sql, ps.ToParameters());
                if (obj != DBNull.Value && obj != null)
                {
                    return LibConvert.ObjToInt(obj);
                }
                return 0;
            });
        }

        public bool DeleteOffLine(DbConn PubConn, int sec)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@sec", sec));

            string Sql = "delete from tb_producter where datediff(s,lastheartbeat,getdate())>=@sec";
            int rev = PubConn.ExecuteSql(Sql, Par);
            if (rev == 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
