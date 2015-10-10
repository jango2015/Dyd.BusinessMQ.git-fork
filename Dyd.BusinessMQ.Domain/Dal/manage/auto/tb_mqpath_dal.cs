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
	public partial class tb_mqpath_dal
    {
        public virtual bool Add(DbConn PubConn, tb_mqpath_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//mq路径
					new ProcedureParameter("@mqpath",    model.mqpath),
					//该路径下mq,配置最后更新时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//mq创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_mqpath(mqpath,lastupdatetime,createtime)
										   values(@mqpath,@lastupdatetime,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_mqpath_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//mq路径
					new ProcedureParameter("@mqpath",    model.mqpath),
					//该路径下mq,配置最后更新时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//mq创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_mqpath set mqpath=@mqpath,lastupdatetime=@lastupdatetime,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_mqpath where id=@id";
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

        public virtual tb_mqpath_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_mqpath s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
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