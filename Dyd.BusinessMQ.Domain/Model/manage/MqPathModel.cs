using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Extensions;

namespace Dyd.BusinessMQ.Domain.Model.manage
{
    public class MqPathModel : tb_mqpath_model
    {
        public int ProductCount { get; set; }
        public int NonProductCount { get; set; }
        public int Connsumer { get; set; }
        public int NonConnsumer { get; set; }
        public int Message { get; set; }
        public int NonMessage { get; set; }
        public int Partition { get; set; }
        public int NonPartition { get; set; }


        public  MqPathModel CreateModel(DataRow dr)
        {
            var o = new MqPathModel();

            //
            if (dr.Table.Columns.Contains("id"))
            {
                o.id = dr["id"].Toint();
            }
            //mq路径
            if (dr.Table.Columns.Contains("mqpath"))
            {
                o.mqpath = dr["mqpath"].Tostring();
            }
            //该路径下mq,配置最后更新时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("lastupdatetime"))
            {
                o.lastupdatetime = dr["lastupdatetime"].ToDateTime();
            }
            //mq创建时间(以当前库时间为准)
            if (dr.Table.Columns.Contains("createtime"))
            {
                o.createtime = dr["createtime"].ToDateTime();
            }
            return o;
        }
    }
}
