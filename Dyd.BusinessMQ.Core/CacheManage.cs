using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Dyd.BusinessMQ.Core
{
    public static class CacheManage
    {
        #region 变量
        /// <summary>
        /// 内存缓存
        /// </summary>
        private static System.Web.Caching.Cache webCache = HttpRuntime.Cache;
        #endregion

        #region 添加缓存对象
        /// <summary>
        /// 添加缓存对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="files">缓存依赖对象</param>
        public static void Add(string key, object obj, params string[] files)
        {
            webCache.Insert(key, obj, new CacheDependency(files), System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
        }
        /// <summary>
        /// 添加缓存对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="obj">缓存对象</param>
        public static void Add(string key, object obj)
        {
            webCache.Insert(key, obj, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
        }
        /// <summary>
        /// 添加缓存对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dateTime">缓存过期时间</param>
        public static void Add(string key, object obj, DateTime dateTime)
        {
            webCache.Insert(key, obj, null, dateTime, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
        }
        #endregion

        #region 移除缓存对象
        /// <summary>
        /// 移除缓存对象
        /// </summary>
        /// <param name="key">键值</param>
        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            webCache.Remove(key);
        }
        #endregion

        #region 返回一个指定的对象
        /// <summary>
        /// 返回一个指定的对象
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns></returns>
        public static object Retrieve(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            return webCache.Get(key);
        }
        #endregion
    }
}
