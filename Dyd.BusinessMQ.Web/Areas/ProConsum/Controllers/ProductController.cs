using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using Dyd.BusinessMQ.Web.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using XXF.Db;
using Dyd.BusinessMQ.Domain.Model.manage;
using Webdiyer.WebControls.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.ProConsum.Controllers
{
    public class ProductController : BaseController
    {
        private tb_producter_dal dal = new tb_producter_dal();
        //
        // GET: /ProConsum/Product/

        public ActionResult Index(string mqpathid,string name, string ip, int pageIndex = 1, int pageSize = 30)
        {
            ViewBag.mqpathid = mqpathid;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open(); ViewBag.ServerDate = conn.GetServerDate();
                int count = 0;
                IList<tb_producterview_model> list = dal.GetPageList(conn,mqpathid, name, ip, pageIndex, pageSize, ref count);
                PagedList<tb_producterview_model> pageList = new PagedList<tb_producterview_model>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }
                ViewBag.ServerDate = conn.GetServerDate();
                return View(pageList);
            }
        }
        public ActionResult Update(int id)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_producter_model model = dal.Get(conn, id);
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Update(tb_producter_model model)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_producter_model result = dal.Get(conn, model.id);
                if (result != null)
                {
                    result.mqpathid = model.mqpathid;
                    result.productername = model.productername;
                    result.ip = model.ip;
                    if (dal.Edit(conn,result))
                    {
                        return RedirectToAction("index");
                    }
                }
                ModelState.AddModelError("Error", "更新错误");
                return View(result);
            }
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                bool flag = dal.Delete(conn, id); 
                if (flag)
                {
                    return Json(new { code = 1, msg = "删除成功" });
                }
                else
                {
                    return Json(new { code = -1, msg = "删除失败" });
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteOffLine()
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    bool flag = dal.DeleteOffLine(conn, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.SystemParamConfig.Producter_Heartbeat_MAX_TIME_OUT);
                   
                }
                return Json(new { code = 1, msg = "离线清理成功" });
            }
            catch (Exception exp)
            {
                return Json(new { code = -1, msg = exp.Message});
            }
        }
    }
}
