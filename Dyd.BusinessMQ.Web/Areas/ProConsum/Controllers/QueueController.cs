using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model;
using Dyd.BusinessMQ.Domain.Model.manage;
using Dyd.BusinessMQ.Web.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using XXF.Db;
using Webdiyer.WebControls.Mvc;
using XXF.Extensions;

namespace Dyd.BusinessMQ.Web.Areas.ProConsum.Controllers
{
    public class QueueController : BaseController
    {
        //
        // GET: /DataNode/Queue/

        private tb_mqpath_partition_dal dal = new tb_mqpath_partition_dal();
        private tb_mqpath_dal pathDal = new tb_mqpath_dal();

        public ActionResult Index(string mqpathid = "", string mqpath = "", int pageIndex = 1, int pageSize = 30, int partitionId = 0)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                int count = 0;
                ViewBag.mqpathid = mqpathid; ViewBag.mqpath = mqpath;
                ViewBag.partitionId = partitionId;
                ViewBag.ServerDate = conn.GetServerDate();
                IList<MqPathModel> list = pathDal.GetPageList(conn, mqpathid, mqpath, pageIndex, pageSize, ref count);
                PagedList<MqPathModel> pageList = new PagedList<MqPathModel>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }
                return View(pageList);
            }
        }
        public ActionResult Update(int id)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_mqpath_model model = pathDal.Get(conn, id);
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Update(tb_mqpath_model model)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_mqpath_model result = pathDal.Get(conn, model.id);
                if (result != null)
                {
                    result.mqpath = model.mqpath;
                    result.lastupdatetime = DateTime.Now;
                    if (pathDal.Edit(conn, result))
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
        public ActionResult Add(string mqpath)
        {
            try
            {
                if (mqpath.isint() == true)
                    throw new Exception("队列名不允许为数字");
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    if (pathDal.Add2(conn, mqpath.ToLower()))
                    {
                        return RedirectToAction("index");
                    }
                    throw new Exception("添加错误");
                }
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("Error", exp.Message);
                return View(mqpath);
            }
        }
        public ActionResult Delete(int id)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    try
                    {
                        conn.Open();
                        conn.BeginTransaction();
                        var countofpatitions = dal.GetCountOfPartition(conn, id);
                        if (countofpatitions > 0)
                            throw new Exception("请先删除该队列全部分区后删除该队列");
                        new tb_mqpath_dal().Delete(conn, id);
                        conn.Commit();
                        return Json(new { code = 1, msg = "删除成功" });
                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }

                }
            }
            catch (Exception exp)
            {

                return Json(new { code = -1, msg = exp.Message });
            }
        }



        public ActionResult ReStart(int id)
        {
            try
            {
                ReStartQuque(id);
                return Json(new { code = 1, msg = "已通知消费者和生产者更新" });

            }
            catch (Exception exp)
            {

                return Json(new { code = -1, msg = exp.Message });
            }
        }
    }
}
