using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.Dal
{
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
	public partial class tb_error_dal
    {
        public virtual bool Add(DbConn PubConn, tb_error_model model)
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
            int rev = PubConn.ExecuteSql(@"insert into tb_error(mqpathid,mqpath,methodname,info,createtime)
										   values(@mqpathid,@mqpath,@methodname,@info,@createtime)", Par);
            return rev == 1;

        }

        
		public virtual tb_error_model CreateModel(DataRow dr)
        {
            var o = new tb_error_model();
			
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