using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Extensions;

namespace Dyd.BusinessMQ.Domain.Model
{
    public class RegisterdConsumersModel
    {
        public tb_consumer_model consumermodel;
        public tb_consumer_partition_model consumerpartitionmodel;
        public tb_consumer_client_model consumerclientmodel;
        public long msgCount { get; set; }
        public long nonMsgCount { get; set; }
        public string mqpath { get; set; }
        public int mqpathid { get; set; }

        public RegisterdConsumersModel(DataRow dr)
        {
// s.id as tb_consumer_id,tempid as tb_consumer_tempid,s.consumerclientid as tb_consumer_consumerclientid,s.partitionindexs as tb_consumer_partitionindexs
//,s.clientname as tb_consumer_clientname,s.lastheartbeat as tb_consumer_lastheartbeat,s.lastupdatetime as tb_consumer_lastupdatetime,s.createtime as tb_consumer_createtime, 
//c.id as tb_consumer_client_id, c.client as tb_consumer_client_client,c.createtime as tb_consumer_client_createtime, p.id as tb_consumer_partition_id,p.consumerclientid as tb_consumer_partition_consumerclientid
//,p.partitionindex as tb_consumer_partition_partitionindex,p.partitionid as tb_consumer_partition_partitionid,p.lastconsumertempid as tb_consumer_partition_lastconsumertempid,p.lastmqid as tb_consumer_partition_lastmqid
//,p.lastupdatetime as tb_consumer_partition_lastupdatetime,p.createtime as tb_consumer_partition_createtime
            consumermodel = new tb_consumer_model();
            //
            if (dr.Table.Columns.Contains("tb_consumer_id"))
            {
                consumermodel.id = dr["tb_consumer_id"].Toint();
            }
            //消费者临时id(消费者启动后唯一,Guid转long)
            if (dr.Table.Columns.Contains("tb_consumer_tempid"))
            {
                consumermodel.tempid = dr["tb_consumer_tempid"].Tolong();
            }
            //消费者clinet的id
            if (dr.Table.Columns.Contains("tb_consumer_consumerclientid"))
            {
                consumermodel.consumerclientid = dr["tb_consumer_consumerclientid"].Toint();
            }
            //支持的分区顺序号(支持多个顺序号)
            if (dr.Table.Columns.Contains("tb_consumer_partitionindexs"))
            {
                consumermodel.partitionindexs = dr["tb_consumer_partitionindexs"].Tostring();
            }
            //客户端名称
            if (dr.Table.Columns.Contains("tb_consumer_clientname"))
            {
                consumermodel.clientname = dr["tb_consumer_clientname"].Tostring();
            }
            //最后心跳时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("tb_consumer_lastheartbeat"))
            {
                consumermodel.lastheartbeat = dr["tb_consumer_lastheartbeat"].ToDateTime();
            }
            //上一次更新时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("tb_consumer_lastupdatetime"))
            {
                consumermodel.lastupdatetime = dr["tb_consumer_lastupdatetime"].ToDateTime();
            }
            //客户端创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("tb_consumer_createtime"))
            {
                consumermodel.createtime = dr["tb_consumer_createtime"].ToDateTime();
            }

            consumerclientmodel = new tb_consumer_client_model();
            //
            if (dr.Table.Columns.Contains("tb_consumer_client_id"))
            {
                consumerclientmodel.id = dr["tb_consumer_client_id"].Toint();
            }
            //客户端（消费者client，相同业务消费者注册必须一致）
            if (dr.Table.Columns.Contains("tb_consumer_client_client"))
            {
                consumerclientmodel.client = dr["tb_consumer_client_client"].Tostring();
            }
            //当前消费者创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("createtime"))
            {
                consumerclientmodel.createtime = dr["tb_consumer_client_createtime"].ToDateTime();
            }

             consumerpartitionmodel = new tb_consumer_partition_model();

            //
             if (dr.Table.Columns.Contains("tb_consumer_partition_id"))
            {
                consumerpartitionmodel.id = dr["tb_consumer_partition_id"].Toint();
            }
            //消费者客户端ID
             if (dr.Table.Columns.Contains("tb_consumer_partition_consumerclientid"))
            {
                consumerpartitionmodel.consumerclientid = dr["tb_consumer_partition_consumerclientid"].Toint();
            }
            //
             if (dr.Table.Columns.Contains("tb_consumer_partition_partitionindex"))
            {
                consumerpartitionmodel.partitionindex = dr["tb_consumer_partition_partitionindex"].Toint();
            }
            //分区表ID
             if (dr.Table.Columns.Contains("tb_consumer_partition_partitionid"))
            {
                consumerpartitionmodel.partitionid = dr["tb_consumer_partition_partitionid"].Toint();
            }
            //
             if (dr.Table.Columns.Contains("tb_consumer_partition_lastconsumertempid"))
            {
                consumerpartitionmodel.lastconsumertempid = dr["tb_consumer_partition_lastconsumertempid"].Tolong();
            }
            //最后消费的MQID
             if (dr.Table.Columns.Contains("tb_consumer_partition_lastmqid"))
            {
                consumerpartitionmodel.lastmqid = dr["tb_consumer_partition_lastmqid"].Tolong();
            }
            //消费者最后执行时间(以当前库时间为准)
             if (dr.Table.Columns.Contains("tb_consumer_partition_lastupdatetime"))
            {
                consumerpartitionmodel.lastupdatetime = dr["tb_consumer_partition_lastupdatetime"].ToDateTime();
            }
            //消费者分区创建时间(以当前库时间为准)
             if (dr.Table.Columns.Contains("tb_consumer_partition_createtime"))
            {
                consumerpartitionmodel.createtime = dr["tb_consumer_partition_createtime"].ToDateTime();
            }
        }
    }
}
