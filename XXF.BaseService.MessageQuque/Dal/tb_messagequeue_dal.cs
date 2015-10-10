using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;
using XXF.ProjectTool;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace XXF.BaseService.MessageQuque.Dal
{
    /*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
    public partial class tb_messagequeue_dal
    {
        public string TableName { get; set; }

        public long GetMaxId(DbConn PubConn)
        {
            return SqlHelper.Visit((ps) =>
            {
                string cmd = string.Format("select max(id) from {0} s WITH(NOLOCK)", TableName);
                var o = PubConn.ExecuteScalar(cmd, null);
                if (o == null || o is DBNull)
                    return -1;
                else
                    return Convert.ToInt64(o);
            });

        }

        public List<tb_messagequeue_model> GetMessages(DbConn PubConn, long lastmaxmessageid, int topcount)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_messagequeue_model> rs = new List<tb_messagequeue_model>();
                ps.Add("@lastmaxmessageid", lastmaxmessageid);
                string cmd = string.Format("select top {1} * from {0} s {2} where id>@lastmaxmessageid order by id asc", TableName, topcount, (SystemParamConfig.Consumer_ReadMessage_WithNolock==true?"with (nolock)":""));
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, cmd, ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var o = CreateModel(dr);
                        rs.Add(o);
                    }
                }
                return rs;
            });

        }

        public virtual bool Add2(DbConn PubConn, tb_messagequeue_model model)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
					new ProcedureParameter("@mqcreatetime",    model.mqcreatetime),
					//消息类型,0=可读消息，1=已迁移消息
					new ProcedureParameter("@state",    model.state),
					//来源类型:0 表示 正常发送,1 表示 迁移消息
					new ProcedureParameter("@source",    model.source),
					//消息体（消息内容,以json形式存储，为了阅读考虑）
					new ProcedureParameter("@message",    model.message),
                    //sql数据节点处的创建时间(以管理中心时间为准)
					new ProcedureParameter("@sqlcreatetime",    model.sqlcreatetime)   
                };
            int rev = PubConn.ExecuteSql(string.Format(@"insert into {0}(mqcreatetime,sqlcreatetime,state,source,message) values(@mqcreatetime,@sqlcreatetime,@state,@source,@message)", TableName), Par);
            return rev == 1;

        }

       

        public virtual tb_messagequeue_model CreateModel(DataRow dr)
        {
            var o = new tb_messagequeue_model();

            //消息id号,规则1+数据节点编号+表分区编号+时间分区号+自增id
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Tolong();
            }
            //mq在生产者端的创建时间（生产者端时间可能跟服务器时间不一致）
            if (dr.Table.Columns.Contains("mqcreatetime"))
            {
                o.mqcreatetime = dr["mqcreatetime"].ToDateTime();
            }
            //sql数据节点处的创建时间
            if (dr.Table.Columns.Contains("sqlcreatetime"))
            {
                o.sqlcreatetime = dr["sqlcreatetime"].ToDateTime();
            }
            //消息类型,0=可读消息，1=已迁移消息
            if (dr.Table.Columns.Contains("state"))
            {
                o.state = dr["state"].ToByte();
            }
            //来源类型:0 表示 正常发送,1 表示 迁移消息
            if (dr.Table.Columns.Contains("source"))
            {
                o.source = dr["source"].ToByte();
            }
            //消息体（消息内容,以json形式存储，为了阅读考虑）
            if (dr.Table.Columns.Contains("message"))
            {
                o.message = dr["message"].Tostring();
            }
            return o;
        }
    }
}