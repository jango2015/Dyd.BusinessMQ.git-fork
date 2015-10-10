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
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;
using System.Globalization;
using Webdiyer.WebControls.Mvc;

namespace Dyd.BusinessMQ.Web.Areas.Message.Controllers
{
    public class MessageController : BaseController
    {
        //
        // GET: /Message/Message/

        private tb_datanode_dal nodeDal = new tb_datanode_dal();

        public ActionResult Index(string node = "", string tablepartition = "", string daypartition = "", string mqid= "",bool isclick=false, int pageIndex = 1, int pageSize = 30)
        {
            ViewBag.mqid = mqid;
            ViewBag.node = node;
            ViewBag.tablepartition = tablepartition; ViewBag.daypartition = daypartition; ViewBag.isclick = isclick;
            int count = 0;
            IList<tb_messagequeue_model> list = new List<tb_messagequeue_model>();
            PagedList<tb_messagequeue_model> pageList = new PagedList<tb_messagequeue_model>(list, pageIndex, pageSize, count);
            if (!string.IsNullOrWhiteSpace(node))
            {
                
                using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
                {
                    conn.Open();
                    string tablename = PartitionRuleHelper.GetTableName(Convert.ToInt32(tablepartition), DateTime.ParseExact(daypartition, "yyMMdd", CultureInfo.InvariantCulture));
                    tb_messagequeue_dal dal = new tb_messagequeue_dal(); dal.TableName = tablename;
                    list = dal.GetPageList(conn, pageIndex, pageSize, mqid, ref count);
                    pageList = new PagedList<tb_messagequeue_model>(list, pageIndex, pageSize, count);
                }
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("List", pageList);
            }
            return View(pageList);
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(tb_messagequeue_model model, string node, string tablepartition,string daypartition)
        {
            DateTime serverdate = DateTime.Now;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                serverdate = conn.GetServerDate();
            }
            using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
            {
                conn.Open();
                model.sqlcreatetime = serverdate;
                model.mqcreatetime = DateTime.Now;
                string tablename = PartitionRuleHelper.GetTableName(Convert.ToInt32(tablepartition), DateTime.ParseExact(daypartition, "yyMMdd", CultureInfo.InvariantCulture));
                var dal =  new tb_messagequeue_dal();dal.TableName=tablename;
                if (dal.Add(conn, tablename, model))
                {
                    return RedirectToAction("index");
                }
                else
                {
                    ModelState.AddModelError("Error", "更新错误");
                    return View(model);
                }
            }
        }
        [HttpPost]
        public ActionResult Delete(long id, string node)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
            {
                conn.Open();
                var mqidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(Convert.ToInt64(id));
                string tablename = PartitionRuleHelper.GetTableName(mqidinfo.TablePartition, mqidinfo.Day);
                var dal = new tb_messagequeue_dal(); dal.TableName = tablename;
                if (dal.SetState(conn,id,(byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMessageState.Deleted))
                {
                    return Json(new { code = 1, msg = "删除成功" });
                }
                return Json(new { code = -1, msg = "删除失败" });
            }
        }
        public ActionResult Update(long id, string node)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
            {
                conn.Open();
                ViewBag.node = node;
                var mqidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(Convert.ToInt64(id));
                ViewBag.tablepartition = mqidinfo.TablePartition; ViewBag.daypartition = mqidinfo.Day.ToString("yyMMdd");
                string tablename = PartitionRuleHelper.GetTableName(mqidinfo.TablePartition, mqidinfo.Day);
                var dal = new tb_messagequeue_dal(); dal.TableName = tablename;
                tb_messagequeue_model model = dal.GetModel(conn, id, tablename);
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Update(tb_messagequeue_model model, string node, long id)
        {
            ViewBag.node = node;
            var mqidinfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(Convert.ToInt64(id));
            ViewBag.tablepartition = mqidinfo.TablePartition; ViewBag.daypartition = mqidinfo.Day.ToString("yyMMdd");
            string tablename = PartitionRuleHelper.GetTableName(mqidinfo.TablePartition, mqidinfo.Day);
            var dal = new tb_messagequeue_dal(); dal.TableName = tablename;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
            {
                conn.Open();
                tb_messagequeue_model result = dal.GetModel(conn, id, tablename);
                if (result != null)
                {
                    result.message = model.message;
                    result.state = model.state;
                    result.source = model.source;
                    if (dal.Update(conn, result, tablename))
                    {
                        return RedirectToAction("Index", new { node =node, tablepartition = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule( mqidinfo.TablePartition), daypartition = mqidinfo.Day.ToString("yyMMdd"), mqid = id });
                    }
                }
                ModelState.AddModelError("Error", "更新错误");
                return View(result);
            }
        }
        public JsonResult GetNodeList()
        {
            IList<string> array = new List<string>();
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                array = nodeDal.GetNodeList(conn);
                if (array != null && array.Count > 0)
                {
                    return Json(array, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPartitionList(string node)
        {
            try
            {

                using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
                {
                    conn.Open();
                    var array = new tb_messagequeue_dal().GetTablePartitions(conn).Select(c => XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(c)).ToList();
                    if (array != null && array.Count > 0)
                    {
                        return Json(array, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetDayList(string node, string tablepartition)
        {
            try
            {
                string mqpath = "";
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    tb_mqpath_dal mqpathdal = new tb_mqpath_dal();
                    var model = mqpathdal.GetByPartitionID(conn, PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = Convert.ToInt32(node), TablePartition = Convert.ToInt32(tablepartition) }));
                    if(model!=null)
                        mqpath = model.mqpath;
                }
                using (DbConn conn = DbConfig.CreateConn(DataConfig.DataNodeParConn(node)))
                {
                    conn.Open();
                    var array = new tb_messagequeue_dal().GetDayPartitions(conn, Convert.ToInt32(tablepartition)).Select(c => c.ToString("yyMMdd")).ToList();
                    if (array != null && array.Count > 0)
                    {
                        return Json(new { array, mqpath }, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetMQInfo(string mqid)
        {
            try
            {
                var info = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetMQIDInfo(Convert.ToInt64(mqid));
                return Json(new { DataNodePartition=XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(info.DataNodePartition), TablePartition=XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(info.TablePartition), day = info.Day.ToString("yyMMdd") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        } 
        
    }
}
