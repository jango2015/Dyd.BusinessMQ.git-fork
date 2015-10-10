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
    public partial class tb_consumer_client_dal
    {

        public virtual tb_consumer_client_model GetByClient(DbConn PubConn, string client)
        {
            return SqlHelper.Visit((ps) =>
            {
                ps.Add("@client", client);
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select top 1 s.* from tb_consumer_client s WITH(NOLOCK) where s.client=@client");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
        }

        public virtual void Add2(DbConn PubConn, string client)
        {
            SqlHelper.Visit((ps) =>
            {
                ps.Add("@client", client);
                PubConn.ExecuteSql(@"insert into tb_consumer_client(client,createtime)
                                        values(@client,getdate())", ps.ToParameters());
                return true;
            });
        }



        public virtual tb_consumer_client_model CreateModel(DataRow dr)
        {
            var o = new tb_consumer_client_model();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //客户端（消费者client，相同业务消费者注册必须一致）
            if (dr.Table.Columns.Contains("client"))
            {
                o.client = dr["client"].Tostring();
            }
            //当前消费者创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("createtime"))
            {
                o.createtime = dr["createtime"].ToDateTime();
            }
            return o;
        }
    }
}