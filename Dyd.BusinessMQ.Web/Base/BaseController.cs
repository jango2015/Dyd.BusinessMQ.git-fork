using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Db;

namespace Dyd.BusinessMQ.Web.Base
{
    [AuthCheck]
    public class BaseController : Controller
    {

        public virtual void ReStartQuque(int mqpathid)
        {
            tb_mqpath_model model;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                try
                {
                    conn.Open();
                    conn.BeginTransaction();
                    model = new tb_mqpath_dal().Get(conn, mqpathid);
                    new tb_mqpath_dal().UpdateLastUpdateTime(conn, mqpathid);
                    conn.Commit();

                }
                catch (Exception exp)
                {
                    conn.Rollback();
                    throw exp;
                }

                SendCommandToRedistReStart(mqpathid, EnumCommandReceiver.All);


            }
        }

        public virtual void SendCommandToRedistReStart(int mqpathid, EnumCommandReceiver receivier)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                var model = new tb_mqpath_dal().Get(conn, mqpathid);
                RedisHelper.SendMessage(DataConfig.RedisServer,
                    XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.SystemParamConfig.Redis_Channel,
                    new XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.BusinessMQNetCommand() { MqPath = model.mqpath, CommandType = EnumCommandType.Register, CommandReceiver = receivier });
            }
           
        }

    }
}
