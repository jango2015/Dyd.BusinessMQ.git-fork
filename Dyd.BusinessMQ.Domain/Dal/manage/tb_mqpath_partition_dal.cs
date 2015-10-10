using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;
using XXF.ProjectTool;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using Dyd.BusinessMQ.Domain.Model.manage;
using System.Linq;

namespace Dyd.BusinessMQ.Domain.Dal
{
	public partial class tb_mqpath_partition_dal
	{
		private tb_messagequeue_dal mqDal = new tb_messagequeue_dal();
		private tb_consumer_partition_dal consumerDal = new tb_consumer_partition_dal();
		private tb_partition_dal partitionDal = new tb_partition_dal();

		public virtual bool Add2(DbConn PubConn, tb_mqpath_partition_model model)
		{

			List<ProcedureParameter> Par = new List<ProcedureParameter>()
				{
					
					//某路径下的mq的id
					new ProcedureParameter("@mqpathid",    model.mqpathid),
					//分区id编号
					new ProcedureParameter("@partitionid",    model.partitionid),
					//分区顺序号(某个路径下mq的顺序号)
					new ProcedureParameter("@partitionindex",    model.partitionindex),
					//某路径下mq的状态,1 运行中，0 待数据迁移或停止，-1 待删除
					new ProcedureParameter("@state",    model.state),
				};
			int rev = PubConn.ExecuteSql(@"insert into tb_mqpath_partition(mqpathid,partitionid,partitionindex,state,createtime)
										   values(@mqpathid,@partitionid,@partitionindex,@state,getdate())", Par);
			return rev == 1;

		}

        public  tb_mqpath_partition_model GetByPartitionId(DbConn PubConn, int partitionId)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@partitionId", partitionId));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select top 1 s.* from tb_mqpath_partition s WITH(NOLOCK)  where s.partitionId=@partitionId");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return CreateModel(ds.Tables[0].Rows[0]);
            }
            return null;
        }

        public int GetMaxPartitionIndexOfMqPath(DbConn PubConn, int mqpathid)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@mqpathid", mqpathid));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select max(partitionindex) from tb_mqpath_partition s WITH(NOLOCK)  where s.mqpathid=@mqpathid");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0][0] is DBNull)
                    return 0;
                return Convert.ToInt32(ds.Tables[0].Rows[0][0]);
            }
            return 0;
        }

        public bool CheckMaxPartitionIndexOfMqPathIsRunning(DbConn PubConn, int mqpathid)
        {
            List<ProcedureParameter> Par = new List<ProcedureParameter>();
            Par.Add(new ProcedureParameter("@mqpathid", mqpathid));
            Par.Add(new ProcedureParameter("@state", (int)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMqPathPartitionState.Running));
            StringBuilder stringSql = new StringBuilder();
            stringSql.Append(@"select top 1 id from tb_mqpath_partition s WITH(NOLOCK)  where s.mqpathid=@mqpathid and partitionindex = (select max(partitionindex) from tb_mqpath_partition s WITH(NOLOCK)  where s.mqpathid=@mqpathid) and state=@state");
            DataSet ds = new DataSet();
            PubConn.SqlToDataSet(ds, stringSql.ToString(), Par);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0][0] is DBNull)
                    return false;
                return true;
            }
            return true;
        }

		public List<tb_mqpath_partition_model> GetListByState(DbConn PubConn, byte state)
		{
			return SqlHelper.Visit((ps) =>
			{
				List<tb_mqpath_partition_model> rs = new List<tb_mqpath_partition_model>();

				StringBuilder stringSql = new StringBuilder();
				ps.Add("@state", state);
                stringSql.Append(@"select s.* from tb_mqpath_partition s  WITH(NOLOCK)  where state=@state");
				DataSet ds = new DataSet();
				PubConn.SqlToDataSet(ds, stringSql.ToString(), ps.ToParameters());
				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[0].Rows)
						rs.Add(CreateModel(dr));
				}
				return rs;
			});
		}
		public IList<MqPathPartitionModel> GetPageList(DbConn conn, string mqPathid, string partitionId, int pageIndex, int pageSize, ref int count)
		{
			int tempCount = 0;
			IList<MqPathPartitionModel> list = new List<MqPathPartitionModel>();

			var result = SqlHelper.Visit((ps) =>
			{
				StringBuilder where = new StringBuilder("");
                if (!string.IsNullOrWhiteSpace(mqPathid))
				{
                    if (!mqPathid.isint())
                        where.AppendFormat(" AND m.mqpath like '%'+'{0}'+'%'", mqPathid);
                    else
                        where.AppendFormat(" AND m.id ='{0}'", mqPathid);
				}
				if (!string.IsNullOrWhiteSpace(partitionId))
				{
					where.AppendFormat(" AND p.partitionId={0}", partitionId);
				}
                string sql = "SELECT ROW_NUMBER() OVER(ORDER BY p.mqpathid DESC,p.[partitionindex] desc) AS rownum,p.*,m.mqpath,(select max(partitionindex) from tb_mqpath_partition p1 where p.mqpathid=p1.mqpathid) as maxpartitionindex FROM tb_mqpath_partition p WITH(NOLOCK),tb_mqpath m with(nolock) where p.mqpathid=m.id";
				string countSql = "SELECT COUNT(1) FROM tb_mqpath_partition p WITH(NOLOCK),tb_mqpath m with(nolock) where p.mqpathid=m.id" + where.ToString();
				object obj = conn.ExecuteScalar(countSql, null);
				if (obj != DBNull.Value && obj != null)
				{
					tempCount = LibConvert.ObjToInt(obj);
				}
				string sqlPage = string.Concat("SELECT * FROM (", sql.ToString(), where.ToString(), ") A WHERE rownum BETWEEN ", ((pageIndex - 1) * pageSize + 1), " AND ", pageSize * pageIndex);
				DataTable dt = conn.SqlToDataTable(sqlPage, null);
				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						MqPathPartitionModel m = new MqPathPartitionModel();
						m.mqpath_partition_model = CreateModel(dr);
						m.mqpath = Convert.ToString(dr["mqpath"]);
                        m.maxpartitionindex = Convert.ToInt32(dr["maxpartitionindex"]);
						list.Add(m);
					}
				}
				return list;
			});
			count = tempCount;
			return result;
		}
		private bool IsOnline(DateTime time, int sec)
		{
			TimeSpan span = DateTime.Now - time;
			if (span.Seconds > sec)
				return false;
			return true;
		}

		/// <summary>
		/// 根据分区id获取队列数量
		/// </summary>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		public int GetPartitionQueue(DbConn conn, int partitionId, EnumMqPathPartitionState state)
		{
			return SqlHelper.Visit((ps) =>
			{
				string sql = "SELECT COUNT(1) FROM tb_mqpath_partition WITH(NOLOCK) WHERE partitionId=@partitionId AND stage=@stage";
				ps.Add("@partitionId", partitionId);
				ps.Add("@stage", Convert.ToInt32(state));
				object obj = conn.ExecuteScalar(sql, ps.ToParameters());
				if (obj != DBNull.Value && obj != null)
				{
					return Convert.ToInt32(obj);
				}
				return 0;
			});
		}
		/// <summary>
		/// 删除队列
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="mqNodeConn"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		internal int IsDeleted(DbConn conn, DbConn mqNodeConn, int id, int partitionId)
		{
			return SqlHelper.Visit((ps) =>
			{
				//tb_mqpath_partition_model model = Get(conn, id);
				//if (model == null) return -50;      //数据不存在

				//if (model.state == Convert.ToInt32(EnumMqPathPartitionState.Running))
				//    return -45;            //正在运行
				//PartitionIDInfo infoModel = PartitionRuleHelper.GetPartitionIDInfo(partitionId);
				//if (infoModel == null)
				//    return -40;      //分区格式错误

				//string maxTableName = mqDal.GetMaxMqTable(mqNodeConn, PartitionRuleHelper.PartitionNameRule(infoModel.DataNodePartition));

				//long maxMqId = mqDal.GetMaxID(mqNodeConn, maxTableName);      //获取messageQueue最大id
				//long lastMqId = consumerDal.GetLastMqIdByPartitionId(conn, partitionId);   //获取tb_consumer_partition上次更新的id

				//if (maxMqId - lastMqId > 0)
				//    return -30;              //还存在处理的队列

				return 1;
			});
		}
		/// <summary>
		/// 删除队列
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="mqNodeConn"></param>
		/// <param name="id"></param>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		public int DeleteMqQueue(DbConn conn, DbConn mqNodeConn, int id, int partitionId)
		{
			int flag = this.IsDeleted(conn, mqNodeConn, id, partitionId);
			if (flag != 1) return flag;
			try
			{
				conn.BeginTransaction();
				if (this.DeletePartition(conn, partitionId))
				{
					if (!partitionDal.UpdateIsUsed(conn, 0, partitionId))
						throw new Exception("更新出错");
				}
				else
				{
					throw new Exception("删除出错");
				}
				conn.Commit();
			}
			catch (Exception ex)
			{
				conn.Rollback();
				XXF.Log.ErrorLog.Write("删除MQ队列出错:", ex);
				return -1;
			}
			return 1;
		}
		/// <summary>
		/// 删除分区对应关系
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="partitionId"></param>
		/// <returns></returns>
		public bool DeletePartition(DbConn conn, int partitionId)
		{
			return SqlHelper.Visit((ps) =>
			{
				string sql = "DELETE FROM tb_mqpath_partition WHERE partitionId=@partitionId";
				ps.Add("@partitionId", partitionId);
				return conn.ExecuteSql(sql, ps.ToParameters()) > 0;
			});
		}
		/// <summary>
		/// 删除mqpath对应关系
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="mqpathId"></param>
		/// <returns></returns>
		public bool DeletePath(DbConn conn, int mqpathId)
		{
			return SqlHelper.Visit((ps) =>
			{
				string sql = "DELETE FROM tb_mqpath_partition WHERE mqpathid=@mqpathid";
				ps.Add("@mqpathid", mqpathId);
				return conn.ExecuteSql(sql, ps.ToParameters()) > 0;
			});
		}

		public int GetCountOfPartition(DbConn conn,int mqpathid)
		{
			return SqlHelper.Visit((ps) =>
			{
				ps.Add("@mqpathid", mqpathid);
                string sql = "select count(0) FROM tb_mqpath_partition WITH(NOLOCK)  WHERE mqpathid=@mqpathid";
				return Convert.ToInt32(conn.ExecuteScalar(sql,ps.ToParameters()));
			});
		}

		
        public int GetPartitionCountByState(DbConn conn, int state, int mqPathId)
		{
			return SqlHelper.Visit((ps) =>
			{
                string sql = "select COUNT(1) from  tb_mqpath_partition WITH(NOLOCK),tb_partition WITH(NOLOCK) where tb_partition.partitionid=tb_mqpath_partition.partitionid and tb_mqpath_partition.state=@state and mqpathid=@mqpathId";
				ps.Add("mqpathId", mqPathId);
                ps.Add("state", state);
				object obj = conn.ExecuteScalar(sql, ps.ToParameters());
				if (obj != DBNull.Value && obj != null)
				{
					return LibConvert.ObjToInt(obj);
				}
				return 0;
			});
		}
        
        public int SetState(DbConn conn, int partitionid,int state)
		{
			return SqlHelper.Visit((ps) =>
			{
                string sql = "update tb_mqpath_partition set state=@state WHERE partitionid=@partitionid";
                ps.Add("partitionid", partitionid);
                ps.Add("state", state);
				object obj = conn.ExecuteScalar(sql, ps.ToParameters());
				if (obj != DBNull.Value && obj != null)
				{
					return LibConvert.ObjToInt(obj);
				}
				return 0;
			});
		}

        public int GetMaxPartitionIndex(DbConn conn, int mqpathid)
		{
			return SqlHelper.Visit((ps) =>
			{
                string sql = "select max(partitionindex) from tb_mqpath_partition p1 WITH(NOLOCK)  where p1.mqpathid=@mqpathid";
                ps.Add("mqpathid", mqpathid);
				object obj = conn.ExecuteScalar(sql, ps.ToParameters());
				if (obj != DBNull.Value && obj != null)
				{
					return LibConvert.ObjToInt(obj);
				}
				return 0;
			});
		}
	}
}