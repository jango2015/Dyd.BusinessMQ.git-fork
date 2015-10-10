using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Producter;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Db;

namespace XXF.BaseService.MessageQuque.BusinessMQ.DB
{
    public class ProducterBLL:BaseBLL
    {
        public ProducterInfo GetProducterInfo(DbConn PubConn, string mqpath,string productername)
        {
            ProducterInfo r = new ProducterInfo();
            tb_mqpath_dal mqpathdal = new tb_mqpath_dal();
            r.MqPathModel = mqpathdal.Get(PubConn,mqpath);
            if (r.MqPathModel == null)
                throw new BusinessMQException(string.Format("当前mqpath:{0}未在MQ中注册队列,请联系管理员注册成功后使用。",mqpath));
            tb_mqpath_partition_dal mqpathpartitiondal = new tb_mqpath_partition_dal();
            r.MqPathParitionModel = mqpathpartitiondal.GetList(PubConn,r.MqPathModel.id);
            if (r.MqPathParitionModel == null||r.MqPathParitionModel.Count==0)
                throw new BusinessMQException(string.Format("当前mqpath:{0}未在MQ中分配相应的分区,请联系管理员分配分区后使用。", mqpath));

            //注册生产者
            r.ProducterModel = this.RegisterProducter(PubConn, r.MqPathModel.id, productername);
            //获取分区相关节点
            List<int> datanodepartition = new List<int>();
            foreach (var p in r.MqPathParitionModel)
            {
                var partitionidinfo = PartitionRuleHelper.GetPartitionIDInfo(p.partitionid);
                if (!datanodepartition.Contains(partitionidinfo.DataNodePartition))
                    datanodepartition.Add(partitionidinfo.DataNodePartition);
            }
            r.DataNodeModelDic = this.GetDataNodeModelsDic(PubConn, datanodepartition);
            
            return r;
        }

        public tb_producter_model RegisterProducter(DbConn PubConn, int mqpathid,string productername)
        {
            long tempid = CommonHelper.GenerateIntID();
            tb_producter_dal dal = new tb_producter_dal();
            dal.DeleteNotOnLineByMqpathid(PubConn, mqpathid, SystemParamConfig.Producter_Heartbeat_MAX_TIME_OUT);
            dal.Add2(PubConn, new tb_producter_model() { tempid = tempid, ip = CommonHelper.GetDefaultIP(), mqpathid=mqpathid, productername=productername });
            return dal.Get(PubConn, tempid, mqpathid);
        }

        public void RemoveProducter(DbConn PubConn, long tempid, int mqpathid)
        {
            tb_producter_dal dal = new tb_producter_dal();
            dal.DeleteClient(PubConn, mqpathid, tempid);

        }

        public void ProducterHeartbeat(DbConn PubConn, long tempid, int mqpathid)
        {
            tb_producter_dal dal = new tb_producter_dal();
            dal.ClientHeatbeat(PubConn, mqpathid, tempid);
        }
    }
}
