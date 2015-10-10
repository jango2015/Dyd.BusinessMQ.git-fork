using Dyd.BusinessMQ.Domain;
using Dyd.BusinessMQ.Domain.Dal;
using Dyd.BusinessMQ.Domain.Model.manage;
using Dyd.BusinessMQ.Web.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XXF.Db;
using Dyd.BusinessMQ.Domain.Model;
using Webdiyer.WebControls.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.ProConsum.Controllers
{
    public class ConsumerController : BaseController
    {
        //
        // GET: /ProConsum/Consumer/

        private tb_consumer_dal dal = new tb_consumer_dal();
        public ActionResult Index(string partitionid, string consumerclientid, string mqpathid, int pageIndex = 1, int pageSize = 30)
        {
            ViewBag.partitionid = partitionid; ViewBag.consumerclientid = consumerclientid; ViewBag.mqpathid = mqpathid;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                ViewBag.ServerDate = conn.GetServerDate();
                int count = 0;
                var list = dal.GetPageList2(conn, partitionid, consumerclientid, mqpathid, pageIndex, pageSize, ref count);
                PagedList<RegisterdConsumersModel> pageList = new PagedList<RegisterdConsumersModel>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }

                return View(pageList);
            }
        }
        [HttpPost]
        public ActionResult Delete(int id, int mqpathid)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    try
                    {
                        conn.Open();
                        conn.BeginTransaction();

                        var item = new tb_consumer_partition_dal().Get(conn, id);
                        new tb_consumer_dal().DeleteNotOnLineByClientID(conn, item.consumerclientid, 20);
                        var consumer = new tb_consumer_dal().GetByTempId(conn, item.lastconsumertempid);
                        if (consumer != null && consumer.tempid == 0)
                        {
                            throw new Exception("当前消费者未处于正常离线状态,若是非正常断线状态,可以在20s超时后尝试");
                        }
                        new tb_consumer_partition_dal().Delete(conn, id);
                        conn.Commit();

                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }
                }
                this.SendCommandToRedistReStart(mqpathid, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumCommandReceiver.Consumer);
                return Json(new { code = 1, msg = "删除成功" });
            }

            catch (Exception e)
            {
                return Json(new { code = -1, msg = e.Message });
            }

        }
        [HttpPost]
        public ActionResult ClearNotOnLineByClientID(int id, int mqpathid)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    try
                    {
                        conn.Open();
                        conn.BeginTransaction();
                        var item = new tb_consumer_partition_dal().Get(conn, id);
                        new tb_consumer_dal().DeleteNotOnLineByClientID(conn, item.consumerclientid, 20);

                        conn.Commit();

                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }

                    return Json(new { code = 1, msg = "清理完毕" });
                }
            }
            catch (Exception e)
            {
                return Json(new { code = -1, msg = e.Message });
            }

        }
        public ActionResult UpdateLastMqID(int id, string mqpathid)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                var o = new tb_consumer_partition_dal().Get(conn, id);
                ViewBag.id = id;
                ViewBag.lastmqid = o.lastmqid; ViewBag.mqpathid = mqpathid;

            }
            return View();

        }
        [HttpPost]
        public ActionResult SaveUpdateLastMqID(int id, long lastmqid, int mqpathid)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                try
                {
                    if (new tb_consumer_partition_dal().UpdateLastMqIdByPartitionId(conn, id, lastmqid) > 0)
                    {
                        this.SendCommandToRedistReStart(mqpathid, XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumCommandReceiver.Consumer);
                        return RedirectToAction("index");
                    }
                    else
                    {
                        ModelState.AddModelError("Error", "更新失败");
                        return View();
                    }
                }
                catch (Exception exp)
                {
                    ModelState.AddModelError("Error", exp.Message);
                    return View();
                }
            }
        }
    }
}
