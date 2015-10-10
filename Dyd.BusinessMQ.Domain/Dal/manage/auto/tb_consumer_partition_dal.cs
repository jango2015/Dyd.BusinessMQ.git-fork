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
	public partial class tb_consumer_partition_dal
    {
        public virtual bool Add(DbConn PubConn, tb_consumer_partition_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//消费者客户端ID
					new ProcedureParameter("@consumerclientid",    model.consumerclientid),
					//
					new ProcedureParameter("@partitionindex",    model.partitionindex),
					//分区表ID
					new ProcedureParameter("@partitionid",    model.partitionid),
					//
					new ProcedureParameter("@lastconsumertempid",    model.lastconsumertempid),
					//最后消费的MQID
					new ProcedureParameter("@lastmqid",    model.lastmqid),
					//消费者最后执行时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//消费者分区创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_consumer_partition(consumerclientid,partitionindex,partitionid,lastconsumertempid,lastmqid,lastupdatetime,createtime)
										   values(@consumerclientid,@partitionindex,@partitionid,@lastconsumertempid,@lastmqid,@lastupdatetime,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_consumer_partition_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//消费者客户端ID
					new ProcedureParameter("@consumerclientid",    model.consumerclientid),
					//
					new ProcedureParameter("@partitionindex",    model.partitionindex),
					//分区表ID
					new ProcedureParameter("@partitionid",    model.partitionid),
					//
					new ProcedureParameter("@lastconsumertempid",    model.lastconsumertempid),
					//最后消费的MQID
					new ProcedureParameter("@lastmqid",    model.lastmqid),
					//消费者最后执行时间(以当前库时间为准)
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//消费者分区创建时间(以当前库时间为准)
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_consumer_partition set consumerclientid=@consumerclientid,partitionindex=@partitionindex,partitionid=@partitionid,lastconsumertempid=@lastconsumertempid,lastmqid=@lastmqid,lastupdatetime=@lastupdatetime,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_consumer_partition where id=@id";
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

        public virtual tb_consumer_partition_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_consumer_partition s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_consumer_partition_model CreateModel(DataRow dr)
        {
            var o = new tb_consumer_partition_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//消费者客户端ID
			if(dr.Table.Columns.Contains("consumerclientid"))
			{
				o.consumerclientid = dr["consumerclientid"].Toint();
			}
			//
			if(dr.Table.Columns.Contains("partitionindex"))
			{
				o.partitionindex = dr["partitionindex"].Toint();
			}
			//分区表ID
			if(dr.Table.Columns.Contains("partitionid"))
			{
				o.partitionid = dr["partitionid"].Toint();
			}
			//
			if(dr.Table.Columns.Contains("lastconsumertempid"))
			{
				o.lastconsumertempid = dr["lastconsumertempid"].Tolong();
			}
			//最后消费的MQID
			if(dr.Table.Columns.Contains("lastmqid"))
			{
				o.lastmqid = dr["lastmqid"].Tolong();
			}
			//消费者最后执行时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("lastupdatetime"))
			{
				o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
			}
			//消费者分区创建时间(以当前库时间为准)
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}