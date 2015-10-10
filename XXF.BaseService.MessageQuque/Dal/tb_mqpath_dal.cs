using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.Dal
{
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
	public partial class tb_mqpath_dal
    {
        public virtual tb_mqpath_model Get(DbConn PubConn, string mqpath)
        {
            return SqlHelper.Visit((ps) => {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@mqpath", mqpath));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_mqpath s WITH(NOLOCK) where s.mqpath=@mqpath");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
           
        }

        public virtual DateTime? GetLastUpdateTimeOfMqPath(DbConn PubConn, string mqpath)
        {
            return SqlHelper.Visit<DateTime?>((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@mqpath", mqpath));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select lastupdatetime from tb_mqpath s WITH(NOLOCK) where s.mqpath=@mqpath");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var o = ds.Tables[0].Rows[0]["lastupdatetime"];
                    if (Convert.IsDBNull(o))
                        return null;
                    else
                        return Convert.ToDateTime(o);
                }
                return null;
            });

        }

       

		public virtual tb_mqpath_model CreateModel(DataRow dr)
        {
            var o = new tb_mqpath_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//mq路径
			if(dr.Table.Columns.Contains("mqpath"))
			{
				o.mqpath = dr["mqpath"].Tostring();
			}
			//该路径下mq,配置最后更新时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("lastupdatetime"))
			{
				o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
			}
			//mq创建时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}