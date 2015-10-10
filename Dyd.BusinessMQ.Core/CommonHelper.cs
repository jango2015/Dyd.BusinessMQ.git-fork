using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyd.BusinessMQ.Core
{
    public class CommonHelper
    {
        public static string ShowTime(DateTime timenow, DateTime time)
        {
            if ((timenow - time) < TimeSpan.FromMinutes(1))
            {
                return string.Format("近{0}秒",(int)(timenow-time).TotalSeconds);
            }
            if ((timenow - time) < TimeSpan.FromMinutes(10))
            {
                return string.Format("近{0}分钟",(timenow-time).TotalMinutes.ToString("f2"));
            }
            return time.ToString("yy-MM-dd HH:mm:ss");
        }
    }
}
