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
	public partial class tb_messagequeue_dal
    {
        public virtual bool Add(DbConn PubConn, tb_messagequeue_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),
					//sql数据节点处的创建时间
					new ProcedureParameter("@sqlcreatetime",    model.sqlcreatetime),
					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_messagequeue(mqcreatetime,sqlcreatetime,state,source,message)
										   values(@mqcreatetime,@sqlcreatetime,@state,@source,@message)", Par);
            return rev == 1;

        }

       

        public virtual bool Edit(DbConn PubConn, tb_messagequeue_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),
					//sql数据节点处的创建时间
					new ProcedureParameter("@sqlcreatetime",    model.sqlcreatetime),
					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_messagequeue set mqcreatetime=@mqcreatetime,sqlcreatetime=@sqlcreatetime,state=@state,source=@source,message=@message where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, long id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_messagequeue where id=@id";
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

        public virtual tb_messagequeue_model Get(DbConn PubConn, long id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_messagequeue s WITH(NOLOCK)  where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_messagequeue_model CreateModel(DataRow dr)
        {
            var o = new tb_messagequeue_model();
			
			//消息id号,规则1+数据节点编号+表分区编号+时间分区号+自增id
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Tolong();
			}
			//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
			if(dr.Table.Columns.Contains("mqcreatetime"))
			{
				o.mqcreatetime = dr["mqcreatetime"].ToDateTime();
			}
			//sql数据节点处的创建时间
			if(dr.Table.Columns.Contains("sqlcreatetime"))
			{
				o.sqlcreatetime = dr["sqlcreatetime"].ToDateTime();
			}
			//消息类型,0=可读消息，1=已迁移消息
			if(dr.Table.Columns.Contains("state"))
			{
				o.state = dr["state"].ToByte();
			}
			//来源类型:0 表示 正常发送,1 表示 迁移消息
			if(dr.Table.Columns.Contains("source"))
			{
				o.source = dr["source"].ToByte();
			}
			//消息体（消息内容,以json形式存储，为了阅读考虑）
			if(dr.Table.Columns.Contains("message"))
			{
				o.message = dr["message"].Tostring();
			}
			return o;
        }
    }
}