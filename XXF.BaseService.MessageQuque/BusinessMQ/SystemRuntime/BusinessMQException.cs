using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.Extensions;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// BusinessMQ 错误信息封装
    /// </summary>
    public class BusinessMQException:Exception
    {
        public override string Message
        {
            get
            {
                string message = base.Message.NullToEmpty();
                if (this.InnerException != null)
                {
                    message += "[innerexp]" + this.InnerException.Message.NullToEmpty();
                }

                return message;
            }
        }
        public BusinessMQException(string message):base(message)
        {
 
        }
        public BusinessMQException(string message,Exception innerException):base(message,innerException)
        {
           
            
        }
    }
}
