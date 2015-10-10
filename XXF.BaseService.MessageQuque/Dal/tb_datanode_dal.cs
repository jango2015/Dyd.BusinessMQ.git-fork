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
    public partial class tb_datanode_dal
    {
        public virtual List<tb_datanode_model> List(DbConn PubConn, List<int> datanodepartitions)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<tb_datanode_model> rs = new List<tb_datanode_model>();
                if (datanodepartitions.Count > 0)
                {
                    List<ProcedureParameter> Par = new List<ProcedureParameter>();
                    StringBuilder stringSql = new StringBuilder();
                    stringSql.Append(string.Format(@"select s.* from tb_datanode s WITH(NOLOCK) where datanodepartition in ({0})", SqlHelper.CmdIn<int>(Par, datanodepartitions)));
                    DataSet ds = new DataSet();
                    PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                            rs.Add(CreateModel(dr));
                    }
                }
                return rs;
            });

        }

        public virtual tb_datanode_model Get2(DbConn PubConn, int datanodepartition)
        {
            return SqlHelper.Visit((ps) =>
            {
                List<ProcedureParameter> Par = new List<ProcedureParameter>();
                Par.Add(new ProcedureParameter("@datanodepartition", datanodepartition));
                StringBuilder stringSql = new StringBuilder();
                stringSql.Append(@"select s.* from tb_datanode s WITH(NOLOCK) where s.datanodepartition=@datanodepartition");
                DataSet ds = new DataSet();
                PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return CreateModel(ds.Tables[0].Rows[0]);
                }
                return null;
            });

        }

       

        public virtual tb_datanode_model CreateModel(DataRow dr)
        {
            var o = new tb_datanode_model();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("datanodepartition"))
            {
                o.datanodepartition = dr["datanodepartition"].Toint();
            }
            //
            if (dr.Table.Columns.Contains("serverip"))
            {
                o.serverip = dr["serverip"].Tostring();
            }
            //
            if (dr.Table.Columns.Contains("username"))
            {
                o.username = dr["username"].Tostring();
            }
            //
            if (dr.Table.Columns.Contains("password"))
            {
                o.password = dr["password"].Tostring();
            }
            return o;
        }
    }
}