using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Db;

namespace Dyd.BusinessMQ.Domain
{
    public static class DataConfig
    {
        private static tb_config_dal configDal = new tb_config_dal();
        private static tb_datanode_dal nodeDal = new tb_datanode_dal();

        public static string MqManage = System.Configuration.ConfigurationManager.AppSettings["MqMangeConnectString"].ToString();

        private static string ConfigConn(string key)
        {
            string mqConn = MqManage;
            using (DbConn conn = DbConfig.CreateConn(mqConn))
            {
                conn.Open();
                IList<tb_config_model> list = configDal.GetCacheList(conn, "configCache");
                string connect = string.Empty;
                if (list != null && list.Count > 0)
                {
                    tb_config_model model = list.Where(q => q.key.Equals(key)).FirstOrDefault();
                    if (model != null)
                    {
                        connect = model.value;
                    }
                }
                return connect;
            }
        }

        public static string LogConn = ConfigConn(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumSystemConfigKey.LogDBConnectString.ToString());

        public static string MQCreateTableSql = ConfigConn(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumSystemConfigKey.MQCreateTableSql.ToString());

        public static string RedisServer = ConfigConn(XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumSystemConfigKey.RedisServer.ToString());
    

        public static string DataNodeParConn(string node)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_datanode_model model = nodeDal.GetModelByPartitionId(conn, LibConvert.ObjToInt(node));
                if (model != null)
                {
                    //string configNodeConn = ConfigConn("DataNodeConnectString");
                    return string.Format("server={0};Initial Catalog=dyd_bs_MQ_datanode_{1};User ID={2};Password={3};", model.serverip, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(LibConvert.ObjToInt(node))
                        , model.username, model.password);
                }
                return "";
            }
        }
    }
}
