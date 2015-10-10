using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 线程安全上下文集合
    /// </summary>
    public class SynchronousContextList : List<ProducterContext>
    {
        private object _lock = new object();
        public void Add(ProducterContext context)
        {
            lock (_lock)
            {
                foreach (var o in this)
                {
                    if (o.ProducterProvider.MQPath == context.ProducterProvider.MQPath)
                        throw new BusinessMQException(string.Format("队列{0}的上下文已注册", context.ProducterProvider.MQPath));
                }
                base.Add(context);
            }
        }

        public void Remove(ProducterContext context)
        {
            lock (_lock)
            {
                ProducterContext t = null;
                foreach (var o in this)
                {
                    if (o.ProducterProvider.MQPath == context.ProducterProvider.MQPath)
                        t = o;
                }
                if(t!=null)
                    base.Remove(t);
            }
        }
        /// <summary>
        /// 拷贝快照
        /// </summary>
        /// <returns></returns>
        public List<ProducterContext> CopyToList()
        {
            lock (_lock)
            {
                List<ProducterContext> t = new List<ProducterContext>();
                foreach (var o in this)
                    t.Add(o);
                return t;
            }
        }
    }
}
