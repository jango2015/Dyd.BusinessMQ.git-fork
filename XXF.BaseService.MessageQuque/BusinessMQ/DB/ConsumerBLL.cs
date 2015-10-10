using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Db;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.BusinessMQ.DB
{
    public class ConsumerBLL : BaseBLL
    {
        public virtual string GetDataNodeConnectString(DbConn PubConn, int datanodepartition)
        {
            tb_datanode_dal dal = new tb_datanode_dal();
            var model = dal.Get2(PubConn, datanodepartition);
            if (model == null)
                throw new BusinessMQException("当前数据节点不存在:" + datanodepartition);
            return GetDataNodeConnectString(SystemParamConfig.Consumer_DataNode_ConnectString_Template, model);
        }

        public tb_consumer_client_model RegisterClient(DbConn PubConn, string client)
        {
            tb_consumer_client_dal dal = new tb_consumer_client_dal();
            var clientmodel = dal.GetByClient(PubConn, client);
            if (clientmodel==null)
            {
                dal.Add2(PubConn,client);
                clientmodel = dal.GetByClient(PubConn,client);
            }
            return clientmodel;
        }

        public tb_consumer_model RegisterConsumer(DbConn PubConn, int clientid, string clientname, List<int> partitionindexs)
        {
            long tempid =  CommonHelper.GenerateIntID();
            tb_consumer_dal dal = new tb_consumer_dal();
            dal.DeleteNotOnLineByClientID(PubConn, clientid, SystemParamConfig.Consumer_ConsumerHeartbeat_MAX_TIME_OUT);
            List<int> usedpartitionindexs = dal.GetRegisterPartitionIndexs(PubConn, clientid);
            var conflictpartitionindexs = (from o in usedpartitionindexs from n in partitionindexs where o == n select o).ToList();
            if (conflictpartitionindexs != null && conflictpartitionindexs.Count > 0)
            {
                throw new BusinessMQException(string.Format("当前分区序号已经被注册使用中,冲突分区序号为:{0},可能是上次消费者异常终止导致消费者依然在注册中,请尝试在{1}秒系统超时后重试。",
                    string.Join(",",conflictpartitionindexs.ToArray()), SystemParamConfig.Consumer_ConsumerHeartbeat_MAX_TIME_OUT));
            }
            dal.Add2(PubConn, new tb_consumer_model() { clientname = clientname, consumerclientid = clientid, partitionindexs = string.Join(",", partitionindexs.ToArray()), tempid = tempid});
            return dal.Get(PubConn, tempid,clientid);
        }

        public void RemoveConsumer(DbConn PubConn, long tempid,int clientid)
        {
            tb_consumer_dal dal = new tb_consumer_dal();
            dal.DeleteClient(PubConn, clientid, tempid);

        }

        public void ConsumerHeartbeat(DbConn PubConn, long tempid, int clientid)
        {
            tb_consumer_dal dal = new tb_consumer_dal();
            dal.ClientHeatbeat(PubConn, clientid, tempid);
        }

        public tb_mqpath_model GetMQPath(DbConn PubConn,string mqpath)
        {
            tb_mqpath_dal mqpathdal = new tb_mqpath_dal();
            var mqpathmodel = mqpathdal.Get(PubConn, mqpath);
            if (mqpathmodel == null)
                throw new BusinessMQException("当前队列不存在:" + mqpath);
            return mqpathmodel;
        }

        public DateTime? GetLastUpdateTimeOfMqPath(DbConn PubConn, string mqpath)
        {
            tb_mqpath_dal mqpathdal = new tb_mqpath_dal();
            return mqpathdal.GetLastUpdateTimeOfMqPath(PubConn, mqpath);
        }

        public tb_consumer_partition_model RegisterConsumerPartition(DbConn PubConn, int clientid, int partitionindex,string mqpath, long tempid)
        {
            var mqpathmodel = GetMQPath(PubConn, mqpath);
            tb_mqpath_partition_dal mqpathpartitiondal = new tb_mqpath_partition_dal();
            var mqpathpartitionmodel = mqpathpartitiondal.GetOfConsumer(PubConn, partitionindex,mqpathmodel.id);
            if (mqpathpartitionmodel == null)
                return null;
            tb_consumer_partition_dal dal = new tb_consumer_partition_dal();
            if (dal.Edit2(PubConn, new tb_consumer_partition_model() { consumerclientid = clientid, lastconsumertempid = tempid, partitionid = mqpathpartitionmodel.partitionid, partitionindex = partitionindex }) <= 0)
            {
                var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(mqpathpartitionmodel.partitionid); var datanodename = PartitionRuleHelper.GetDataNodeName(partitionidinfo.DataNodePartition);
                long maxid = -1; DateTime serverdate = PubConn.GetServerDate(); string tablename = PartitionRuleHelper.GetTableName(partitionidinfo.TablePartition,serverdate);
                SqlHelper.ExcuteSql(this.GetDataNodeConnectString(PubConn, partitionidinfo.DataNodePartition), (c) => {
                    var exist = c.TableIsExist(tablename); if (!exist) { throw new BusinessMQException(string.Format("当前数据节点{0},表{1}不存在", partitionidinfo.DataNodePartition,tablename)); }
                    tb_messagequeue_dal mqdal = new tb_messagequeue_dal(); mqdal.TableName = tablename;
                    maxid = mqdal.GetMaxId(c);
                    if (maxid <= 0)
                        maxid = PartitionRuleHelper.GetMQID(new MQIDInfo() { AutoID=0, DataNodePartition=partitionidinfo.DataNodePartition, Day=serverdate, TablePartition=partitionidinfo.TablePartition });
                });
                dal.Add2(PubConn, new tb_consumer_partition_model() { consumerclientid = clientid, lastconsumertempid = tempid, lastmqid = maxid, partitionid = mqpathpartitionmodel.partitionid, partitionindex = partitionindex });
            }
            return dal.Get(PubConn, clientid, mqpathpartitionmodel.partitionid);
        }

    
    }
}
