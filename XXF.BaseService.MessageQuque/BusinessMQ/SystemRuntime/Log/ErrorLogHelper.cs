using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.ProjectTool;
using XXF.Extensions;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log
{
    public class ErrorLogHelper
    {
        public static void WriteLine(int mqpathid, string mqpath, string methodname, string msg,Exception exp)
        {
            if (!string.IsNullOrWhiteSpace(ConfigHelper.LogDBConnectString))
            {
                try
                {
                    SqlHelper.ExcuteSql(ConfigHelper.LogDBConnectString, (c) =>
                    {
                        tb_error_dal dal = new tb_error_dal();
                        dal.Add(c, new tb_error_model() { createtime = DateTime.Now, info = string.Format("错误:{0},exp:{1}", msg.NullToEmpty(), exp.Message.NullToEmpty()), mqpath = mqpath.NullToEmpty(), mqpathid = mqpathid, methodname = methodname.NullToEmpty() });
                    });
                }
                catch (Exception e1)
                {
                    XXF.Log.ErrorLog.Write(string.Format("BusinessMQ插入错误信息时发生错误,mqpathid:{0},mqpath:{1},methodname:{2},msg:{3}", mqpathid, mqpath.NullToEmpty(), methodname.NullToEmpty(), msg.NullToEmpty()), e1);
                }
            }
            else
            {
                XXF.Log.ErrorLog.Write(string.Format("BusinessMQ错误,mqpathid:{0},mqpath:{1},methodname:{2},msg:{3}",mqpathid,mqpath.NullToEmpty(),methodname.NullToEmpty(),msg.NullToEmpty()), exp);
            }
            DebugHelper.WriteLine(mqpathid, mqpath, methodname, "【出错】:" + msg+"exp:"+exp.Message+"strace:"+exp.StackTrace);
        }
    }
}
