using Dyd.BusinessMQ.Domain.Model;
using Dyd.BusinessMQ.Domain.Model.manage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_mqpath_dal
    {
        private tb_producter_dal proDal = new tb_producter_dal();
        private tb_mqpath_partition_dal parDal = new tb_mqpath_partition_dal();
        private tb_messagequeue_dal msgDal = new tb_messagequeue_dal();

        public virtual bool Add2(DbConn PubConn, string mqpath)
        {
            return SqlHelper.Visit((ps) =>
               {
                   List<ProcedureParameter> Par = new List<ProcedureParameter>()
                    {
					    //mq路径
					    new ProcedureParameter("@mqpath",    mqpath),
                    };
                   int rev = PubConn.ExecuteSql(@"insert into tb_mqpath(mqpath,lastupdatetime,createtime)
										   values(@mqpath,getdate(),getdate())", Par);
                   return rev == 1;
               });

        }

        public IList<MqPathModel> GetPageList(DbConn conn, string mqpathid, string mqpath, int pageIndex, int pageSize, ref int count)
        {
            int tempCount = 0;
            IList<MqPathModel> list = new List<MqPathModel>();
            MqPathModel createM = new MqPathModel();
            var result = SqlHelper.Visit((ps) =>
            {
                StringBuilder where = new StringBuilder(" WHERE 1=1");
                if (!string.IsNullOrEmpty(mqpath))
                {
                    where.AppendFormat(" AND mqpath LIKE '%{0}%'", mqpath);
                }
                if (!string.IsNullOrEmpty(mqpathid))
                {
                    where.AppendFormat(" AND id = '{0}'", mqpathid);
                }
                string sql = "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS rownum,* FROM tb_mqpath  WITH(NOLOCK)";
                string countSql = "SELECT COUNT(1) FROM tb_mqpath WITH(NOLOCK)" + where;
                object obj = conn.ExecuteScalar(countSql, null);
                if (obj != DBNull.Value && obj != null)
                {
                    tempCount = LibConvert.ObjToInt(obj);
                }
                string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), where.ToString(), ") as t WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
                DataTable dt = conn.SqlToDataTable(sqlPage, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        MqPathModel model = createM.CreateModel(dr);

                        model.ProductCount = proDal.GetProductCount(conn, model.id, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.SystemParamConfig.Producter_HeatBeat_Every_Time);
                        model.NonProductCount = proDal.GetNonProductCount(conn, model.id, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.SystemParamConfig.Producter_HeatBeat_Every_Time);

                        model.Connsumer = new tb_consumer_partition_dal().GetActiveConsumerCount(conn,model.id);
                        model.NonConnsumer = new tb_consumer_partition_dal().GetLogoutConsumerCount(conn, model.id);

                        model.Partition = parDal.GetPartitionCountByState(conn, (int)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMqPathPartitionState.Running, model.id);
                        model.NonPartition = parDal.GetPartitionCountByState(conn, (int)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMqPathPartitionState.WaitConsumeCompleted, model.id);

                        list.Add(model);
                    }
                }
                return list;
            });
            count = tempCount;
            return result;
        }

        public virtual tb_mqpath_model GetByPartitionID(DbConn PubConn, int partitionid)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@partitionid", partitionid));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select m.* from tb_mqpath_partition s,tb_mqpath m WITH(NOLOCK) where s.partitionid=@partitionid and s.mqpathid=m.id");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }
        /// <summary>
        ///获取所有队列
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public IList<tb_mqpath_model> GetAllMaPath(DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
            {
                IList<tb_mqpath_model> list = new List<tb_mqpath_model>();
                string sql = "SELECT * FROM tb_mqpath WITH(NOLOCK)";
                DataTable dt = conn.SqlToDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_mqpath_model model = CreateModel(dr);
                        list.Add(model);
                    }
                }
                return list;
            });
        }

        public virtual bool UpdateLastUpdateTime(DbConn PubConn, int id)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@id", id));

            string Sql = "update tb_mqpath set [lastupdatetime]=getdate() where id=@id";
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
    }
}
