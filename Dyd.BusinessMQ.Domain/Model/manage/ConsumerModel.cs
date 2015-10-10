using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Extensions;

namespace Dyd.BusinessMQ.Domain.Model.manage
{
    public class ConsumerModel : tb_consumer_model
    {
        public IList<ConsumerPartition> PartitionList { get; set; }
        public ConsumerModel CreateModel(DataRow dr)
        {
            var o = new ConsumerModel();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //消费者临时id(消费者启动后唯一,Guid转long)
            if (dr.Table.Columns.Contains("tempid"))
            {
                o.tempid = dr["tempid"].Tolong();
            }
            //消费者clinet的id
            if (dr.Table.Columns.Contains("consumerclientid"))
            {
                o.consumerclientid = dr["consumerclientid"].Toint();
            }
            //支持的分区顺序号(支持多个顺序号)
            if (dr.Table.Columns.Contains("partitionindexs"))
            {
                o.partitionindexs = dr["partitionindexs"].Tostring();
            }
            //客户端名称
            if (dr.Table.Columns.Contains("clientname"))
            {
                o.clientname = dr["clientname"].Tostring();
            }
            //最后心跳时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("lastheartbeat"))
            {
                o.lastheartbeat = dr["lastheartbeat"].ToDateTime();
            }
            //上一次更新时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("lastupdatetime"))
            {
                o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
            }
            //客户端创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("createtime"))
            {
                o.createtime = dr["createtime"].ToDateTime();
            }
            return o;
        }
    }
}
