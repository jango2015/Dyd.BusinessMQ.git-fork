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
using Webdiyer.WebControls.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.Log.Controllers
{
    public class LogController : BaseController
    {
        private tb_log_dal logDal = new tb_log_dal();
        private tb_debuglog_dal debugDal = new tb_debuglog_dal();
        private tb_error_dal errorDal = new tb_error_dal();
        //
        // GET: /Log/Error/
        public ActionResult LogIndex(DateTime? startTime, DateTime? endTime, string mqpathid, string methodname, string info, int pageIndex = 1, int pageSize = 30)
        {
            int count = 0;
            string mqpath = "";
            GetQueryInfo(ref startTime, ref endTime, mqpathid, mqpath, methodname, info);
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                IList<tb_log_model> list = logDal.GetPageList(conn, startTime, endTime, mqpathid, mqpath, methodname, info, pageSize, pageIndex, ref count);
                PagedList<tb_log_model> pageList = new PagedList<tb_log_model>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("LogList", pageList);
                }
                return View(pageList);
            }
        }
        /// <summary>
        /// DebugLog
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult DebugIndex(DateTime? startTime, DateTime? endTime, string mqpathid, string methodname, string info, int pageIndex = 1, int pageSize = 30)
        {
            int count = 0; string mqpath = "";
            GetQueryInfo(ref startTime,ref endTime, mqpathid,mqpath, methodname, info);
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                IList<tb_debuglog_model> list = debugDal.GetPageList(conn, startTime, endTime, mqpathid, mqpath, methodname,info, pageSize, pageIndex, ref count);
                PagedList<tb_debuglog_model> pageList = new PagedList<tb_debuglog_model>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("DebugList", pageList);
                }
                return View(pageList);
            }
        }

        public ActionResult ErrorIndex(DateTime? startTime, DateTime? endTime, string mqpathid, string methodname, string info, int pageIndex = 1, int pageSize = 30)
        {
            int count = 0;
            string mqpath = "";
            GetQueryInfo(ref startTime, ref endTime, mqpathid, mqpath, methodname, info);
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                IList<tb_error_model> list = errorDal.GetPageList(conn, startTime, endTime, mqpathid, mqpath, methodname, info, pageSize, pageIndex, ref count);
                PagedList<tb_error_model> pageList = new PagedList<tb_error_model>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("ErrorList", pageList);
                }
                return View(pageList);
            }
        }

        private void GetQueryInfo(ref DateTime? startTime, ref DateTime? endTime, string mqpathid, string mqpath, string methodname, string info)
        {
            mqpath = "";
            if (startTime != null)
            {
                startTime = startTime.Value;
            }
            else
            {
                startTime = DateTime.Now.AddDays(-30);
            }
            if (endTime != null)
            {
                endTime = endTime.Value;
            }
            else
            {
                endTime = DateTime.Now.AddDays(1);
            }
            if (!string.IsNullOrEmpty(mqpathid))
            {
                int mqpathidint = 0;
                if (int.TryParse(mqpathid, out mqpathidint) == true)
                { mqpathid = mqpathidint + ""; }
                else
                { mqpath = mqpathid; }
            }
            ViewBag.startTime = startTime; ViewBag.endTime = endTime; ViewBag.mqpathid = mqpathid + mqpath; ViewBag.methodname = methodname; ViewBag.info = info;
        }

        public ActionResult DebugDeleteAll()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                debugDal.DeleteAll(conn);
                return Json(new { code = 1, msg = "删除成功" });
            }
        }
        public ActionResult ErrorDeleteAll()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                errorDal.DeleteAll(conn);
                return Json(new { code = 1, msg = "删除成功" });
            }
        }
        public ActionResult LogDeleteAll()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.LogConn))
            {
                conn.Open();
                logDal.DeleteAll(conn);
                return Json(new { code = 1, msg = "删除成功" });
            }
        }
    }
}
