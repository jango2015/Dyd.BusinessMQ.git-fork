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
    public class LogHelper
    {
        public static void WriteLine(int mqpathid, string mqpath, string methodname, string info)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigHelper.LogDBConnectString))
                {
                    SqlHelper.ExcuteSql(ConfigHelper.LogDBConnectString, (c) =>
                    {
                        tb_log_dal dal = new tb_log_dal();
                        dal.Add(c, new tb_log_model() { createtime = DateTime.Now, info = info, mqpath = mqpath, mqpathid = mqpathid, methodname = methodname });
                    });

                }
            }
            catch (Exception exp)
            {
                XXF.Log.ErrorLog.Write(string.Format("BusinessMQ插入Log信息时发生错误,mqpathid:{0},mqpath:{1},methodname:{2},info:{3}", mqpathid, mqpath.NullToEmpty(), methodname.NullToEmpty(), info.NullToEmpty()), exp);
            }
            DebugHelper.WriteLine(mqpathid, mqpath, methodname, "【记录】:" + info);
        }
    }
}
