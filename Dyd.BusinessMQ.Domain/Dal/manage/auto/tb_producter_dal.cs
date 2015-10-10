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
	public partial class tb_producter_dal
    {
        public virtual bool Add(DbConn PubConn, tb_producter_model model)
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
					//生产者最后心跳时间
					new ProcedureParameter("@lastheartbeat",    model.lastheartbeat),
					//生产者创建时间
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_producter(tempid,productername,ip,mqpathid,lastheartbeat,createtime)
										   values(@tempid,@productername,@ip,@mqpathid,@lastheartbeat,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_producter_model model)
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
					//生产者最后心跳时间
					new ProcedureParameter("@lastheartbeat",    model.lastheartbeat),
					//生产者创建时间
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_producter set tempid=@tempid,productername=@productername,ip=@ip,mqpathid=@mqpathid,lastheartbeat=@lastheartbeat,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_producter where id=@id";
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

        public virtual tb_producter_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_producter s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
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