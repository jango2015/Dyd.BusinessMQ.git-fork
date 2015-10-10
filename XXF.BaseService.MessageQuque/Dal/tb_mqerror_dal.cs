using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XXF.Extensions;
using XXF.Db;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.Dal
{
	/*代码自动生成工具自动生成,不要在这里写自己的代码，否则会被自动覆盖哦 - 车毅*/
	public partial class tb_mqerror_dal
    {
        public virtual bool Add2(DbConn PubConn, tb_mqerror_model model, int tablePartitionNo)
        {

            List<ProcedureParameter> Par = new List<ProcedureParameter>()
                {
					
					//
					new ProcedureParameter("@TryCount",    model.TryCount),
					//
					new ProcedureParameter("@MQType",    model.MQType),
					//
					new ProcedureParameter("@MQPath",    model.MQPath),
					//
					new ProcedureParameter("@MQMsgJson",    model.MQMsgJson)   
                };
            int rev = PubConn.ExecuteSql(string.Format(@"insert into tb_mqerror{0}(TryCount,MQType,MQPath,MQMsgJson)
										   values(@TryCount,@MQType,@MQPath,@MQMsgJson)",tablePartitionNo), Par);
            return rev == 1;

        }

       
    }
}