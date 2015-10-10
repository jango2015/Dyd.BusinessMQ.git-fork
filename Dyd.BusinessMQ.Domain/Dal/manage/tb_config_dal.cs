using Dyd.BusinessMQ.Core;
using Dyd.BusinessMQ.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XXF.Db;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Domain.Dal
{
    public partial class tb_config_dal
    {
        /// <summary>
        /// 获取所有配置信息
        /// </summary>
        /// <returns></returns>
        public IList<tb_config_model> GetList(DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
            {
                IList<tb_config_model> list = new List<tb_config_model>();
                DataTable dt = null;
                string sql = "SELECT * FROM tb_config WITH(NOLOCK)";
                dt = conn.SqlToDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tb_config_model model = CreateModel(dr);
                        list.Add(model);
                    }
                }
                return list;
            });
        }
        /// <summary>
        /// 缓存
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<tb_config_model> GetCacheList(DbConn conn, string key)
        {
            IList<tb_config_model> cacheList = CacheManage.Retrieve(key) as List<tb_config_model>;
            if (cacheList != null && cacheList.Count > 0)
            {
                return cacheList;
            }
            IList<tb_config_model> list = this.GetList(conn);
            CacheManage.Add(key, list);
            return list;
        }
        /// <summary>
        /// Add
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Add(tb_config_model model, DbConn conn)
        {
            return SqlHelper.Visit((ps) =>
            {
                tb_config_model config = Get(conn, model.key);
                if (config == null)
                    return Add(conn, model);
                return false;
            });
        }
    }
}
