using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.Dal
{
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
	public partial class tb_producter_dal
    {
        public bool DeleteNotOnLineByMqpathid(DbConn PubConn, int mqpathid, int maxtimeoutsenconds)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("@mqpathid", mqpathid); ps.Add("@maxtimeoutsenconds", maxtimeoutsenconds);
                string Sql = "delete from tb_producter where mqpathid=@mqpathid and DATEDIFF(S,lastheartbeat,getdate())>@maxtimeoutsenconds";
                int rev = PubConn.ExecuteSql(Sql, ps.ToParameters());
                return true;
            });
        }

        public bool DeleteClient(DbConn PubConn, int mqpathid, long tempid)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("@mqpathid", mqpathid); ps.Add("@tempid", tempid);
                string Sql = "delete from tb_producter where mqpathid=@mqpathid and tempid=@tempid";
                int rev = PubConn.ExecuteSql(Sql, ps.ToParameters());
                return true;
            });
        }

        public bool ClientHeatbeat(DbConn PubConn, int mqpathid, long tempid)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("@mqpathid", mqpathid); ps.Add("@tempid", tempid);
                string Sql = "update tb_producter WITH (ROWLOCK) set lastheartbeat=getdate() where mqpathid=@mqpathid and tempid=@tempid";
                int rev = PubConn.ExecuteSql(Sql, ps.ToParameters());
                return true;
            });
        }


        public virtual bool Add2(DbConn PubConn, tb_producter_model model)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//生产者临时id(消费者启动后唯一,Guid转long)
					new ProcedureParameter("@tempid",    model.tempid),
					//生产者名称
					new ProcedureParameter("@productername",    model.productername),
					//ip地址
					new ProcedureParameter("@ip",    model.ip),
					//队列id
					new ProcedureParameter("@mqpathid",    model.mqpathid),
                    ////生产者最后心跳时间
                    //new ProcedureParameter("@lastheartbeat",    model.lastheartbeat),
                    ////生产者创建时间
                    //new ProcedureParameter("@createtime",    model.createtime)   
                };
                int rev = PubConn.ExecuteSql(@"insert into tb_producter(tempid,productername,ip,mqpathid,lastheartbeat,createtime)
										   values(@tempid,@productername,@ip,@mqpathid,getdate(),getdate())", Par);
                return rev == 1;
            });

        }

        public virtual tb_producter_model Get(DbConn PubConn, long tempid, int mqpathid)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@tempid", tempid)); Par.Add(new ProcedureParameter("@mqpathid", mqpathid));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_producter s WITH(NOLOCK) where s.tempid=@tempid and mqpathid=@mqpathid");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
        }

       

		public virtual tb_producter_model CreateModel(DataRow dr)
        {
            var o = new tb_producter_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//生产者临时id(消费者启动后唯一,Guid转long)
			if(dr.Table.Columns.Contains("tempid"))
			{
				o.tempid = dr["tempid"].Tolong();
			}
			//生产者名称
			if(dr.Table.Columns.Contains("productername"))
			{
				o.productername = dr["productername"].Tostring();
			}
			//ip地址
			if(dr.Table.Columns.Contains("ip"))
			{
				o.ip = dr["ip"].Tostring();
			}
			//队列id
			if(dr.Table.Columns.Contains("mqpathid"))
			{
				o.mqpathid = dr["mqpathid"].Toint();
			}
			//生产者最后心跳时间
			if(dr.Table.Columns.Contains("lastheartbeat"))
			{
				o.lastheartbeat = dr["lastheartbeat"].ToDateTime();
			}
			//生产者创建时间
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}