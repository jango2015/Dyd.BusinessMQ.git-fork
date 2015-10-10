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
	public partial class tb_debuglog_dal
    {
        public virtual bool Add(DbConn PubConn, tb_debuglog_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//
					new ProcedureParameter("@mqpathid",    model.mqpathid),
					//
					new ProcedureParameter("@mqpath",    model.mqpath),
					//
					new ProcedureParameter("@methodname",    model.methodname),
					//
					new ProcedureParameter("@info",    model.info),
					//
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_debuglog(mqpathid,mqpath,methodname,info,createtime)
										   values(@mqpathid,@mqpath,@methodname,@info,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_debuglog_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//
					new ProcedureParameter("@mqpathid",    model.mqpathid),
					//
					new ProcedureParameter("@mqpath",    model.mqpath),
					//
					new ProcedureParameter("@methodname",    model.methodname),
					//
					new ProcedureParameter("@info",    model.info),
					//
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_debuglog set mqpathid=@mqpathid,mqpath=@mqpath,methodname=@methodname,info=@info,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, long id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_debuglog where id=@id";
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

        public virtual tb_debuglog_model Get(DbConn PubConn, long id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_debuglog s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_debuglog_model CreateModel(DataRow dr)
        {
            var o = new tb_debuglog_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Tolong();
			}
			//
			if(dr.Table.Columns.Contains("mqpathid"))
			{
				o.mqpathid = dr["mqpathid"].Toint();
			}
			//
			if(dr.Table.Columns.Contains("mqpath"))
			{
				o.mqpath = dr["mqpath"].Tostring();
			}
			//
			if(dr.Table.Columns.Contains("methodname"))
			{
				o.methodname = dr["methodname"].Tostring();
			}
			//
			if(dr.Table.Columns.Contains("info"))
			{
				o.info = dr["info"].Tostring();
			}
			//
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}