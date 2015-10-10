using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;
using XXF.ProjectTool;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace XXF.BaseService.MessageQuque.Dal
{
    /*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
    public partial class tb_mqpath_partition_dal
    {
        public virtual List<tb_mqpath_partition_model> GetList(DbConn PubConn, int mqpathid)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_mqpath_partition_model> rs = new List<tb_mqpath_partition_model>();
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@mqpathid", mqpathid)); Par.Add(new ProcedureParameter("@state", (int)EnumMqPathPartitionState.Running));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_mqpath_partition s WITH(NOLOCK) where mqpathid=@mqpathid and state=@state order by partitionindex");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                        rs.Add(CreateModel(dr));
                }
                return rs;
            });
        }

        public virtual tb_mqpath_partition_model GetOfProducter(DbConn PubConn, int partitionindex, int mqpathid)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@partitionindex", partitionindex)); Par.Add(new ProcedureParameter("@mqpathid", mqpathid)); Par.Add(new ProcedureParameter("@state", (int)EnumMqPathPartitionState.Running));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_mqpath_partition s WITH(NOLOCK) where s.partitionindex=@partitionindex and mqpathid=@mqpathid and state=@state");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
        }

        public virtual tb_mqpath_partition_model GetOfConsumer(DbConn PubConn, int partitionindex, int mqpathid)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@partitionindex", partitionindex)); Par.Add(new ProcedureParameter("@mqpathid", mqpathid));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_mqpath_partition s WITH(NOLOCK) where s.partitionindex=@partitionindex and mqpathid=@mqpathid");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });
        }
        
        public virtual tb_mqpath_partition_model CreateModel(DataRow dr)
        {
            var o = new tb_mqpath_partition_model();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //某路径下的mq的id
            if (dr.Table.Columns.Contains("mqpathid"))
            {
                o.mqpathid = dr["mqpathid"].Toint();
            }
            //分区id编号
            if (dr.Table.Columns.Contains("partitionid"))
            {
                o.partitionid = dr["partitionid"].Toint();
            }
            //分区顺序号(某个路径下mq的顺序号)
            if (dr.Table.Columns.Contains("partitionindex"))
            {
                o.partitionindex = dr["partitionindex"].Toint();
            }
            //某路径下mq的状态,1 运行中，0 待数据迁移或停止，-1 待删除
            if (dr.Table.Columns.Contains("state"))
            {
                o.state = dr["state"].ToByte();
            }
            //创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("createtime"))
            {
                o.createtime = dr["createtime"].ToDateTime();
            }
            return o;
        }
    }
}