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

namespace Dyd.BusinessMQ.Web.Areas.DataNode.Controllers
{
    public class PartitionController : BaseController
    {
        private tb_partition_dal dal = new tb_partition_dal();
        //
        // GET: /DataNode/Partition/

        public ActionResult Index(string partitionid,string nodeId,int used=-1, int pageIndex = 1, int pageSize = 30)
        {
            ViewBag.nodeId = nodeId; ViewBag.used = used; ViewBag.partitionid = partitionid;
            int nnodeid =( string.IsNullOrWhiteSpace(nodeId)?0: Convert.ToInt32(nodeId));
            
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open(); ViewBag.ServerDate = conn.GetServerDate();
                int count = 0;
                IList<tb_partition_model> list = dal.GetPageList(conn,partitionid, nnodeid,used, pageIndex, pageSize, ref count);
                PagedList<tb_partition_model> pageList = new PagedList<tb_partition_model>(list, pageIndex, pageSize, count);
                ViewBag.Nodes = new tb_datanode_dal().GetNodeList(conn);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }
                return View(pageList);
            }
        }
        public ActionResult Delete(int partitionId)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                int result = dal.DeletePartition(conn, partitionId);
                switch (result)
                {
                    case 0:
                        return Json(new { code = -1, msg = "分区正在使用，无法删除，请先删除该分区下的队列" });
                    case 1:
                        return Json(new { code = 1, msg = "删除成功" });
                    case -1:
                        return Json(new { code = -1, msg = "数据不存在" });
                    case -2:
                        return Json(new { code = -1, msg = "删除失败" });
                    default:
                        return Json(new { code = -1, msg = "未知错误" });
                }
            }
        }
        public ActionResult Add(int nodeId=0)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                ViewBag.nodeId = nodeId;
                int count=0;
                ViewBag.datanodes = new tb_datanode_dal().GetPageList(conn, 1, 100, ref count);
            }
            return View();
        }
        public ActionResult PartitionsCount(int nodeId)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                int count = 0;
                new tb_partition_dal().GetPageList(conn,"",nodeId,-1, 1, 100, ref count);
                return Json(new { code = 1, count = count });
            }
        }
        [HttpPost]
        public ActionResult Add(int nodeid,int count)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    try
                    {
                        conn.BeginTransaction();
                        int r = 0;
                        var partitons = new tb_partition_dal().GetPageList(conn, "", nodeid, -1, 1, 100, ref r);
                        List<int> usedpartitionids = new List<int>();
                        foreach (var d in partitons)
                        {
                            if (!usedpartitionids.Contains(d.partitionid))
                                usedpartitionids.Add(d.partitionid);
                        }
                        List<int> canusepartitionids = new List<int>();
                        for (var i = 1; i < 100; i++)
                        {
                            var partition = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionID(new XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionIDInfo() { DataNodePartition = nodeid, TablePartition =i  });
                            if (!usedpartitionids.Contains(partition))
                                canusepartitionids.Add(partition);
                        }
                        for (int i = 0; i < count; i++)
                        {
                            tb_partition_model model = new tb_partition_model();
                            model.isused = false;
                            model.partitionid = canusepartitionids[i];
                            dal.AddPartition(conn, model);
                        }
                        conn.Commit();
                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }
                }
                return RedirectToAction("index", new { nodeId = nodeid });
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("Error","添加失败"+ exp.Message);
                return View();
            }
           
        }
    }
}
