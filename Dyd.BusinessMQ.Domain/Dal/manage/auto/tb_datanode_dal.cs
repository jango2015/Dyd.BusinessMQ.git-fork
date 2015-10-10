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
	public partial class tb_datanode_dal
    {
        public virtual bool Add(DbConn PubConn, tb_datanode_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//
					new ProcedureParameter("@datanodepartition",    model.datanodepartition),
					//
					new ProcedureParameter("@serverip",    model.serverip),
					//
					new ProcedureParameter("@username",    model.username),
					//
					new ProcedureParameter("@password",    model.password)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_datanode(datanodepartition,serverip,username,password)
										   values(@datanodepartition,@serverip,@username,@password)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_datanode_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//
					new ProcedureParameter("@datanodepartition",    model.datanodepartition),
					//
					new ProcedureParameter("@serverip",    model.serverip),
					//
					new ProcedureParameter("@username",    model.username),
					//
					new ProcedureParameter("@password",    model.password)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_datanode set datanodepartition=@datanodepartition,serverip=@serverip,username=@username,password=@password where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_datanode where id=@id";
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

        public virtual tb_datanode_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_datanode s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_datanode_model CreateModel(DataRow dr)
        {
            var o = new tb_datanode_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//
			if(dr.Table.Columns.Contains("datanodepartition"))
			{
				o.datanodepartition = dr["datanodepartition"].Toint();
			}
			//
			if(dr.Table.Columns.Contains("serverip"))
			{
				o.serverip = dr["serverip"].Tostring();
			}
			//
			if(dr.Table.Columns.Contains("username"))
			{
				o.username = dr["username"].Tostring();
			}
			//
			if(dr.Table.Columns.Contains("password"))
			{
				o.password = dr["password"].Tostring();
			}
			return o;
        }
    }
}