using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 一般类库
    /// </summary>
    public class CommonHelper
    {
        /// <summary>
        /// 生成相对唯一的guidid long类型
        /// </summary>
        /// <returns></returns>
        public static long GenerateIntID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        /// <summary>
        /// 获取订阅路径
        /// </summary>
        /// <param name="mqpath"></param>
        /// <returns></returns>
        public static string GetSubscribeChannelPath(string mqpath)
        {
            return SystemParamConfig.Redis_Channel + "." + mqpath.ToLower();
        }
        /// <summary>
        /// 获取当前服务器默认ip信息
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultIP()
        {
            try
            {
                IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                return ipAddr.ToString();
            }
            catch (Exception exp)
            { }
            return "";
        }
    }
}
