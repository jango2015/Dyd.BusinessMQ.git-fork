using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;

namespace Dyd.BusinessMQ.Domain.Dal
{
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
	public partial class tb_config_dal
    {
        public virtual bool Add(DbConn PubConn, tb_config_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//配置Value
					new ProcedureParameter("@value",    model.value),
					//配置备注信息
					new ProcedureParameter("@remark",    model.remark)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_config(value,remark)
										   values(@value,@remark)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_config_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//配置Value
					new ProcedureParameter("@value",    model.value),
					//配置备注信息
					new ProcedureParameter("@remark",    model.remark)
            };
			Par.Add(new ProcedureParameter("@key",  model.key));

            int rev = PubConn.ExecuteSql("update tb_config set value=@value,remark=@remark where key=@key", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, string key)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@key",  key));

            string Sql = "delete from tb_config where key=@key";
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

        public virtual tb_config_model Get(DbConn PubConn, string key)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@key", key));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_config s where s.key=@key");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_config_model CreateModel(DataRow dr)
        {
            var o = new tb_config_model();
			
			//配置Key
			if(dr.Table.Columns.Contains("key"))
			{
				o.key = dr["key"].Tostring();
			}
			//配置Value
			if(dr.Table.Columns.Contains("value"))
			{
				o.value = dr["value"].Tostring();
			}
			//配置备注信息
			if(dr.Table.Columns.Contains("remark"))
			{
				o.remark = dr["remark"].Tostring();
			}
			return o;
        }
    }
}