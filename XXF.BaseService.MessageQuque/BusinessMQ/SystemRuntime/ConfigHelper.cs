using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;
using XXF.BaseService.MessageQuque.Model;
using XXF.Log;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// RedisServer
        /// </summary>
        public static string RedisServer = "";
        /// <summary>
        /// DebugMqpath
        /// </summary>
        public static string DebugMqpath = "";
        /// <summary>
        /// LogDBConnectString
        /// </summary>
        public static string LogDBConnectString = "";
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="manageconnectstring"></param>
        public static void LoadConfig(string manageconnectstring)
        {
            try
            {
                List<tb_config_model> configs = new List<tb_config_model>();
                SqlHelper.ExcuteSql(manageconnectstring, (c) => { configs = new DB.ConsumerBLL().GetConfig(c); });
                foreach (var c in configs)
                {
                    if (c.key.ToLower() == EnumSystemConfigKey.RedisServer.ToString().ToLower())
                    {
                        RedisServer = c.value;
                    }
                    else if (c.key.ToLower() == EnumSystemConfigKey.DebugMqpath.ToString().ToLower())
                    {
                        DebugMqpath = c.value;
                    }
                    else if (c.key.ToLower() == EnumSystemConfigKey.LogDBConnectString.ToString().ToLower())
                    {
                        LogDBConnectString = c.value;
                    }

                }
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(-1, "", "LoadConfig", "初始化系统配置表信息出错", exp);
            }
        }
    }
}
