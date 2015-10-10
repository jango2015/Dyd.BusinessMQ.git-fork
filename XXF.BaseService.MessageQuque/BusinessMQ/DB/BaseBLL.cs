using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Db;

namespace XXF.BaseService.MessageQuque.BusinessMQ.DB
{
    public class BaseBLL
    {
        //public virtual string GetRedisServerIP(DbConn PubConn)
        //{
        //    string serverip = GetConfigValue(PubConn, EnumSystemConfigKey.RedisServer.ToString());
        //    return serverip;
        //}

        public virtual string GetDataNodeConnectString(string template,tb_datanode_model model)
        {
            return template.Replace("{server}", model.serverip).Replace("{password}", model.password)
                           .Replace("{username}", model.username).Replace("{database}", SystemParamConfig.DataNode_DataBaseName_Prefix + model.datanodepartition.ToString().PadLeft(2, '0')).Replace("{server}", model.serverip);
        }

        //public virtual string GetConfigValue(DbConn PubConn, string key)
        //{
        //    tb_config_dal dal = new tb_config_dal();
        //    var value = dal.Get2(PubConn, key);
        //    return value;
        //}

        public virtual List<tb_config_model> GetConfig(DbConn PubConn)
        {
            tb_config_dal dal = new tb_config_dal();
            var rs = dal.List(PubConn);
            return rs;
        }

        public tb_mqpath_model GetMqPathModel(DbConn PubConn, string mqpath)
        {
            tb_mqpath_dal mqpathdal = new tb_mqpath_dal();
            var mqpathmodel = mqpathdal.Get(PubConn, mqpath);
            if (mqpathmodel == null)
                throw new BusinessMQException("当前队列不存在:" + mqpath);
            return mqpathmodel;
        }

        public Dictionary<int, Model.tb_datanode_model> GetDataNodeModelsDic(DbConn PubConn, List<int> datanodepartition)
        {

            tb_datanode_dal datanodedal = new tb_datanode_dal();
            var rs = new Dictionary<int, Model.tb_datanode_model>();
            foreach (var o in datanodedal.List(PubConn, datanodepartition))
                rs.Add(o.datanodepartition, o);
            return rs;
        }
    }
}
