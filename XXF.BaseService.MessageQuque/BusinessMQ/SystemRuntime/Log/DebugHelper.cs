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
    public class DebugHelper
    {
        public static void TimeWatch(int mqpathid, string mqpath, string methodname, Action action)
        {
            var startTime = DateTime.Now;
            action.Invoke();
            var d = (DateTime.Now - startTime).TotalSeconds;
            WriteLine(mqpathid,mqpath,methodname,string.Format("耗时:{0}",d+""));
        }

        public static void WriteLine(int mqpathid,string mqpath,string methodname,string info)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(info + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                if (!string.IsNullOrWhiteSpace(ConfigHelper.DebugMqpath) && ConfigHelper.DebugMqpath.ToLower() == mqpath.ToLower())
                {
                    if (!string.IsNullOrWhiteSpace(ConfigHelper.LogDBConnectString))
                    {
                        SqlHelper.ExcuteSql(ConfigHelper.LogDBConnectString, (c) =>
                        {
                            tb_debuglog_dal dal = new tb_debuglog_dal();
                            dal.Add(c, new tb_debuglog_model() { createtime = DateTime.Now, info = info, mqpath = mqpath, mqpathid = mqpathid, methodname = methodname });
                        });

                    }
                }
            }
            catch (Exception exp)
            {
                XXF.Log.ErrorLog.Write(string.Format("BusinessMQ插入Debug信息时发生错误,mqpathid:{0},mqpath:{1},methodname:{2},info:{3}", mqpathid, mqpath.NullToEmpty(), methodname.NullToEmpty(), info.NullToEmpty()), exp);
            }
        }
    }
}
