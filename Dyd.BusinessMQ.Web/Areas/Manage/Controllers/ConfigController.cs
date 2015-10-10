using Dyd.BusinessMQ.Core;
using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Model;
using Dyd.BusinessMQ.Web.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XXF.Db;

namespace Dyd.BusinessMQ.Web.Areas.Manage.Controllers
{
    public class ConfigController : BaseController
    {
        private Dyd.BusinessMQ.Domain.Dal.tb_config_dal configDal = new Dyd.BusinessMQ.Domain.Dal.tb_config_dal();
        // GET: /Manage/Config/

        public ActionResult Index()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                IList<tb_config_model> list = configDal.GetList(conn);
                return View(list);
            }
        }
        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Delete(string key)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                bool flag = configDal.Delete(conn, key);
                if (flag)
                {
                    CacheManage.Remove("configCache");
                    return Json(new { code = 1, msg = "删除成功" });
                }
                return Json(new { code = -1, msg = "删除失败" });
            }
        }
        [HttpPost]
        public ActionResult Update(tb_config_model model)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                bool flag = configDal.Edit(conn, model);
                if (flag)
                {
                    CacheManage.Remove("configCache");
                    return RedirectToAction("index");
                }
                else
                {
                    ModelState.AddModelError("Error", "更新错误");
                    return View(model);
                }
            }
        }
        public ActionResult Update(string key)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_config_model model = configDal.Get(conn, key);
                return View(model);
            }
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(tb_config_model model)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                bool flag = configDal.Add(conn, model);
                if (flag)
                {
                    CacheManage.Remove("configCache");
                    return RedirectToAction("index");
                }
                else
                {
                    ModelState.AddModelError("Error", "更新错误");
                    return View(model);
                }
            }
        }
    }
}
