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
	public partial class tb_consumer_dal
    {
        public virtual bool Add(DbConn PubConn, tb_consumer_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//消费者临时id(消费者启动后唯一,Guid转long)
					new ProcedureParameter("@tempid",    model.tempid),
					//消费者clinet的id
					new ProcedureParameter("@consumerclientid",    model.consumerclientid),
					//支持的分区顺序号(支持多个顺序号)
					new ProcedureParameter("@partitionindexs",    model.partitionindexs),
					//客户端名称
					new ProcedureParameter("@clientname",    model.clientname),
					//最后心跳时间(以当前库时间为准)
					new ProcedureParameter("@lastheartbeat",    model.lastheartbeat),
					//上一次更新时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//客户端创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_consumer(tempid,consumerclientid,partitionindexs,clientname,lastheartbeat,lastupdatetime,createtime)
										   values(@tempid,@consumerclientid,@partitionindexs,@clientname,@lastheartbeat,@lastupdatetime,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_consumer_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//消费者临时id(消费者启动后唯一,Guid转long)
					new ProcedureParameter("@tempid",    model.tempid),
					//消费者clinet的id
					new ProcedureParameter("@consumerclientid",    model.consumerclientid),
					//支持的分区顺序号(支持多个顺序号)
					new ProcedureParameter("@partitionindexs",    model.partitionindexs),
					//客户端名称
					new ProcedureParameter("@clientname",    model.clientname),
					//最后心跳时间(以当前库时间为准)
					new ProcedureParameter("@lastheartbeat",    model.lastheartbeat),
					//上一次更新时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//客户端创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_consumer set tempid=@tempid,consumerclientid=@consumerclientid,partitionindexs=@partitionindexs,clientname=@clientname,lastheartbeat=@lastheartbeat,lastupdatetime=@lastupdatetime,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_consumer where id=@id";
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

        public virtual tb_consumer_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_consumer s where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_consumer_model CreateModel(DataRow dr)
        {
            var o = new tb_consumer_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//消费者临时id(消费者启动后唯一,Guid转long)
			if(dr.Table.Columns.Contains("tempid"))
			{
				o.tempid = dr["tempid"].Tolong();
			}
			//消费者clinet的id
			if(dr.Table.Columns.Contains("consumerclientid"))
			{
				o.consumerclientid = dr["consumerclientid"].Toint();
			}
			//支持的分区顺序号(支持多个顺序号)
			if(dr.Table.Columns.Contains("partitionindexs"))
			{
				o.partitionindexs = dr["partitionindexs"].Tostring();
			}
			//客户端名称
			if(dr.Table.Columns.Contains("clientname"))
			{
				o.clientname = dr["clientname"].Tostring();
			}
			//最后心跳时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("lastheartbeat"))
			{
				o.lastheartbeat = dr["lastheartbeat"].ToDateTime();
			}
			//上一次更新时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("lastupdatetime"))
			{
				o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
			}
			//客户端创建时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}