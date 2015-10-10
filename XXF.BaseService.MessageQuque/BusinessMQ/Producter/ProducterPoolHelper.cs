using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.Log;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 简单Producter连接池
    /// 单个mqpath只用一个连接
    /// </summary>
    public class ProducterPoolHelper : IDisposable
    {
        private static Dictionary<string, ProducterProvider> Pool = new Dictionary<string, ProducterProvider>();
        private static object _poollock = new object();
        private static object _singletonlock = new object();
        private static ProducterPoolHelper _singleton = null;

        private ProducterPoolHelper()
        { }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            try
            {
                if (_singleton != null)
                    _singleton.Dispose();
                LogHelper.WriteLine(-1, "", "CurrentDomain_ProcessExit", "当前域进程退出时释放生产者完毕");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(-1, "", "CurrentDomain_ProcessExit", "当前域进程退出时释放生产者连接池出错", exp);
                throw exp;
            }
        }
        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            try
            {
                if (_singleton != null)
                    _singleton.Dispose();
                LogHelper.WriteLine(-1, "", "CurrentDomain_DomainUnload", "当前域域卸载时释放生产者完毕");
            }
            catch (Exception exp)
            {
                ErrorLogHelper.WriteLine(-1, "", "CurrentDomain_DomainUnload", "当前域域卸载释放生产者连接池出错", exp);
                throw exp;
            }
        }

        /// <summary>
        /// 从连接池中获取生产者
        /// </summary>
        /// <param name="config"></param>
        /// <param name="mqpath"></param>
        /// <returns></returns>
        public static ProducterProvider GetPool(BusinessMQConfig config, string mqpath)
        {
            //创建单例
            if (_singleton == null)
            {
                lock (_singletonlock)
                {
                    if (_singleton == null)
                    {
                        _singleton = new ProducterPoolHelper();
                        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
                        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                    }
                }
            }
            //查找生产者
            ProducterProvider provider = null; mqpath = mqpath.ToLower();
            if (Pool.ContainsKey(mqpath))
            {
                provider = Pool[mqpath];
            }
            if (provider == null)
            {
                lock (_poollock)
                {
                    if (Pool.ContainsKey(mqpath))
                    {
                        provider = Pool[mqpath];
                    }
                    if (provider == null)
                    {
                        var pt = new ProducterProvider();
                        pt.Config = config; pt.MQPath = mqpath;
                        pt.Open();
                        Pool.Add(mqpath, pt);
                        provider = Pool[mqpath];
                    }
                }
            }
            return provider;
        }

        public void Dispose()
        {
            if (_singleton != null)
            {
                Exception firstexp = null;
                foreach (var item in Pool)
                {
                    try
                    {
                        if (item.Value != null)
                            item.Value.Dispose();
                    }
                    catch (Exception exp)
                    {
                        if (firstexp == null)
                            firstexp = exp;
                    }
                }
                Pool = new Dictionary<string, ProducterProvider>();
                _singleton = null;
                if (firstexp != null)
                    throw firstexp;
            }
        }
    }
}
