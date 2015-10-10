using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.ProjectTool;
using XXF.Extensions;

namespace Dyd.BusinessMQ.Task
{
    public class BaseMQTask : XXF.BaseService.TaskManager.BaseDllTask
    {
        public BaseMQTask():base()
        {

        }

        public virtual string GetDataNodeConnectString(tb_datanode_model model)
        {
            return SystemParamConfig.Consumer_DataNode_ConnectString_Template.Replace("{server}", model.serverip).Replace("{password}", model.password)
                           .Replace("{username}", model.username).Replace("{database}", SystemParamConfig.DataNode_DataBaseName_Prefix + model.datanodepartition.ToString().PadLeft(2, '0')).Replace("{server}", model.serverip);
        }

        public virtual void Error(string manageconnectstring, string message, Exception exp1)
        {
            try
            {
                string info = message.NullToEmpty();
                if (exp1 != null)
                {
                    info += "【exp】" + exp1.Message.NullToEmpty();
                }

                SqlHelper.ExcuteSql(ConfigHelper.LogDBConnectString, (c) =>
               {
                   new tb_error_dal().Add(c, new tb_error_model() { createtime = DateTime.Now, methodname = this.GetType().Name, mqpath = "", mqpathid = -1, info = info });
               });
            }
            catch (Exception exp)
            {
                this.OpenOperator.Error(message, exp1);
            }
        }

        public virtual void Error(string manageconnectstring, string message, List<Exception> exp1)
        {
            if (exp1 == null || exp1.Count == 0)
                return;
            int i = 1;
            string info = "";
            foreach (var e in exp1)
            {
                if (exp1 != null)
                {
                    info += i+"." + e.Message.NullToEmpty()+"\r\n";
                }
                i++;
            }
            Error(manageconnectstring,message,new Exception(info));
        }

        public override void Run() { }
    }

    public class ErrorInfo
    {
        public string message;
        public Exception exp;
    }
}
