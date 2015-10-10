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
    public class NodeController : BaseController
    {
        private tb_datanode_dal nodeDal = new tb_datanode_dal();
        private tb_partition_dal partitionDal = new tb_partition_dal();
        private int pageSize = 30;
        //
        // GET: /DataNode/Node/

        public ActionResult Index(int pageIndex = 1)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                int count = 0;
                IList<tb_datanode_model> list = nodeDal.GetPageList(conn, pageIndex, pageSize, ref count);
                PagedList<tb_datanode_model> pageList = new PagedList<tb_datanode_model>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }
               
                return View(pageList);
            }
        }
        public ActionResult Delete(int id)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                var model = new tb_datanode_dal().Get(conn,id);
                bool isExistPartion = partitionDal.IsExistPartition(conn, model.datanodepartition);
                if (isExistPartion)
                {
                    return Json(new { code = -1, msg = "该节点存在分区，请先删除分区，再进行删除节点操作" });
                }
                else
                {
                    bool flag = nodeDal.Delete(conn, id);
                    if (flag)
                    {
                        return Json(new { code = 1, msg = "删除成功" });
                    }
                    return Json(new { code = -1, msg = "删除失败" });
                }
            }
        }
        [HttpPost]
        public ActionResult Update(tb_datanode_model model)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    if (model.datanodepartition < 1 || model.datanodepartition >= 100)
                    {
                        throw new Exception("节点编号只允许1~99之间");
                    }
                    if (nodeDal.IsExist(conn, model.id, model.datanodepartition))
                    {
                        throw new Exception("节点编号已存在");
                    }
                    if (nodeDal.Edit(conn, model) == false)
                    {
                        throw new Exception("更新错误");
                    }
                }
                return RedirectToAction("index");
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("Error", exp.Message);
                return View(model);
            }
        }
        public ActionResult Update(int id)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                tb_datanode_model model = nodeDal.Get(conn, id);
                return View(model);
            }
        }
        public ActionResult Add()
        {
            IList<string> usednodes = new List<string>();
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                usednodes = new tb_datanode_dal().GetNodeList(conn);
            }
            List<string> unUsednodes = new List<string>();
            for (var i = 1; i < 100; i++)
            {
                var nodeid = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(i);
                //var partition = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionID(new XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionIDInfo() { DataNodePartition = nodeid, TablePartition = i });
                if (!usednodes.Contains(nodeid))
                    unUsednodes.Add(nodeid);
            }
            ViewBag.unusednodes = unUsednodes;
            return View();
        }
        [HttpPost]
        public ActionResult Add(tb_datanode_model model)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    if (model.datanodepartition < 1 || model.datanodepartition >= 100)
                    {
                        throw new Exception("节点编号只允许1~99之间");
                    }
                    if (nodeDal.IsExist(conn, model.id, model.datanodepartition))
                    {
                        throw new Exception("节点编号已存在");
                    }
                    if (nodeDal.Add(conn, model) == false)
                    {
                        throw new Exception("更新错误");
                    }
                }
                return RedirectToAction("index");
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("Error", exp.Message);
                return View(model);
            }
        }
    }
}
