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
	public partial class tb_partition_messagequeue_report_dal
    {
        public virtual bool Add(DbConn PubConn, tb_partition_messagequeue_report_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//分区编号
					new ProcedureParameter("@partitionid",    model.partitionid),
					//日期
					new ProcedureParameter("@day",    model.day),
					//分区最大消息id
					new ProcedureParameter("@mqmaxid",    model.mqmaxid),
					//mq最小消息id
					new ProcedureParameter("@mqminid",    model.mqminid),
					//消息数量
					new ProcedureParameter("@mqcount",    model.mqcount),
					//当前分区扫描最后更新时间
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//当前分区扫描创建时间
					new ProcedureParameter("@createtime",    model.createtime)   
                };
            int rev = PubConn.ExecuteSql(@"insert into tb_partition_messagequeue_report(partitionid,day,mqmaxid,mqminid,mqcount,lastupdatetime,createtime)
										   values(@partitionid,@day,@mqmaxid,@mqminid,@mqcount,@lastupdatetime,@createtime)", Par);
            return rev == 1;

        }

        public virtual bool Edit(DbConn PubConn, tb_partition_messagequeue_report_model model)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>()
            {
                    
					//分区编号
					new ProcedureParameter("@partitionid",    model.partitionid),
					//日期
					new ProcedureParameter("@day",    model.day),
					//分区最大消息id
					new ProcedureParameter("@mqmaxid",    model.mqmaxid),
					//mq最小消息id
					new ProcedureParameter("@mqminid",    model.mqminid),
					//消息数量
					new ProcedureParameter("@mqcount",    model.mqcount),
					//当前分区扫描最后更新时间
					new ProcedureParameter("@lastupdatetime",    model.lastupdatetime),
					//当前分区扫描创建时间
					new ProcedureParameter("@createtime",    model.createtime)
            };
			Par.Add(new ProcedureParameter("@id",  model.id));

            int rev = PubConn.ExecuteSql("update tb_partition_messagequeue_report set partitionid=@partitionid,day=@day,mqmaxid=@mqmaxid,mqminid=@mqminid,mqcount=@mqcount,lastupdatetime=@lastupdatetime,createtime=@createtime where id=@id", Par);
            return rev == 1;

        }

        public virtual bool Delete(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id",  id));

            string Sql = "delete from tb_partition_messagequeue_report where id=@id";
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

        public virtual tb_partition_messagequeue_report_model Get(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select s.* from tb_partition_messagequeue_report s WITH(NOLOCK) where s.id=@id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
				return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

		public virtual tb_partition_messagequeue_report_model CreateModel(DataRow dr)
        {
            var o = new tb_partition_messagequeue_report_model();
			
			//
			if(dr.Table.Columns.Contains("id"))
			{
				o.id = dr["id"].Toint();
			}
			//分区编号
			if(dr.Table.Columns.Contains("partitionid"))
			{
				o.partitionid = dr["partitionid"].Toint();
			}
			//日期
			if(dr.Table.Columns.Contains("day"))
			{
				o.day = dr["day"].ToDateTime();
			}
			//分区最大消息id
			if(dr.Table.Columns.Contains("mqmaxid"))
			{
				o.mqmaxid = dr["mqmaxid"].Tolong();
			}
			//mq最小消息id
			if(dr.Table.Columns.Contains("mqminid"))
			{
				o.mqminid = dr["mqminid"].Tolong();
			}
			//消息数量
			if(dr.Table.Columns.Contains("mqcount"))
			{
				o.mqcount = dr["mqcount"].Toint();
			}
			//当前分区扫描最后更新时间
			if(dr.Table.Columns.Contains("lastupdatetime"))
			{
				o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
			}
			//当前分区扫描创建时间
			if(dr.Table.Columns.Contains("createtime"))
			{
				o.createtime = dr["createtime"].ToDateTime();
			}
			return o;
        }
    }
}