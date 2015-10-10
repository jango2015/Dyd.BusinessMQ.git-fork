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

using XXF.Db;
using Webdiyer.WebControls.Mvc;
using XXF.ProjectTool;
using XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime;

namespace Dyd.BusinessMQ.Web.Areas.DataNode.Controllers
{
    public class QueuePartitionController : BaseController
    {
        //
        // GET: /ProConsum/QueuePartition/

        private tb_mqpath_partition_dal pathPartitionDal = new tb_mqpath_partition_dal();
        private tb_partition_dal partitionDal = new tb_partition_dal();
        private tb_mqpath_dal mqPathDal = new tb_mqpath_dal();

        public ActionResult Index(string mqpathid, string partitionId, int pageIndex = 1, int pageSize = 30)
        {
            ViewBag.mqpathid = mqpathid; ViewBag.partitionId = partitionId;
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                int count = 0;
                IList<MqPathPartitionModel> list = pathPartitionDal.GetPageList(conn, mqpathid, partitionId, pageIndex, pageSize, ref count);
                var partitionids = list.Select(c => c.mqpath_partition_model.partitionid).ToList();
                Dictionary<int, List<ConsumerPartitionModel>> rs = new Dictionary<int, List<ConsumerPartitionModel>>();
                foreach (var o in new tb_consumer_partition_dal().ListByPartitionIds(conn, partitionids))
                {
                    if (!rs.ContainsKey(o.consumerpartitionmodel.partitionid))
                    {
                        rs.Add(o.consumerpartitionmodel.partitionid, new List<ConsumerPartitionModel>());
                    }
                    rs[o.consumerpartitionmodel.partitionid].Add(o); ;
                }
                ViewBag.consumers = rs;
                PagedList<MqPathPartitionModel> pageList = new PagedList<MqPathPartitionModel>(list, pageIndex, pageSize, count);
                if (Request.IsAjaxRequest())
                {
                    return PartialView("List", pageList);
                }
                ViewBag.ServerDate = conn.GetServerDate();
                return View(pageList);
            }
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add(tb_mqpath_partition_model model)
        {
            try
            {
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    try
                    {
                        conn.Open();
                        conn.BeginTransaction();
                        model.state = (byte)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMqPathPartitionState.Running;
                        model.partitionindex = new tb_mqpath_partition_dal().GetMaxPartitionIndexOfMqPath(conn, model.mqpathid) + 1;
                        if (new tb_mqpath_dal().Get(conn, model.mqpathid) == null)
                        {
                            throw new Exception("无法找到队列");
                        }
                        if (new tb_mqpath_partition_dal().GetByPartitionId(conn, model.partitionid) != null)
                        {
                            throw new Exception("分区已被使用");
                        }
                        if (new tb_mqpath_partition_dal().CheckMaxPartitionIndexOfMqPathIsRunning(conn, model.mqpathid) == false)
                        {
                            throw new Exception("最后的分区未处于正常使用状态,若分区正在待删状态,请删除完毕后新增分区。");
                        }
                        if (new tb_mqpath_partition_dal().Add2(conn, model))
                        {
                            new tb_partition_dal().UpdateIsUsed(conn, 1, model.partitionid);
                            var partitioninfo = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetPartitionIDInfo(model.partitionid);
                            //创建3天的表
                            var serverdate = conn.GetServerDate().Date;
                            for (int i = 0; i < 3; i++)
                            {
                                var currentdate = serverdate.AddDays(i);
                                var tablename = XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.GetTableName(partitioninfo.TablePartition, currentdate);//
                                SqlHelper.ExcuteSql(DataConfig.DataNodeParConn(partitioninfo.DataNodePartition + ""), (c) =>
                                {
                                    bool exsit = c.TableIsExist(tablename);
                                    if (exsit != true)
                                    {
                                        string cmd = DataConfig.MQCreateTableSql.Replace("{tablepartiton}", XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(partitioninfo.TablePartition))
                                            .Replace("{daypartition}", currentdate.ToString("yyMMdd")).Replace("{datanodepartiton}", XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.PartitionRuleHelper.PartitionNameRule(partitioninfo.DataNodePartition));
                                        c.ExecuteSql(cmd, new List<XXF.Db.ProcedureParameter>());
                                    }
                                });
                            }
                            conn.Commit();

                        }
                        else
                            throw new Exception("更新错误");

                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }
                }
                ReStartQuque(model.mqpathid);
                return RedirectToAction("index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error", e.Message);
                return View(model);
            }


        }

        public ActionResult SetState(int partitionid, int state)
        {
            try
            {
                tb_mqpath_partition_model model = null;
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    conn.Open();
                    model = new tb_mqpath_partition_dal().GetByPartitionId(conn, partitionid);
                    new tb_mqpath_partition_dal().SetState(conn, partitionid, state);
                }
                SendCommandToRedistReStart(model.mqpathid, EnumCommandReceiver.Producter);
                return Json(new { code = 1, msg = "成功" });
            }
            catch (Exception exp)
            {
                return Json(new { code = -1, msg = exp.Message });
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                tb_mqpath_partition_model model = null;
                using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
                {
                    try
                    {
                        conn.Open();
                        conn.BeginTransaction();
                        model = pathPartitionDal.Get(conn, id);
                        if (model == null)
                            throw new Exception("分区不存在");

                        if (model.state == (int)XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime.EnumMqPathPartitionState.Running)
                        {
                            throw new Exception("正在运行中");
                        }
                        if (model.partitionindex != new tb_mqpath_partition_dal().GetMaxPartitionIndex(conn, model.mqpathid))
                        {
                            throw new Exception("分区请优先移除最大的分区顺序的分区");
                        }
                        //移除该分区的所有消费者信息
                        new tb_consumer_partition_dal().Delete2(conn, model.partitionid);
                        pathPartitionDal.Delete(conn, id);
                        new tb_partition_dal().UpdateIsUsed(conn, 0, model.partitionid);
                        conn.Commit();

                    }
                    catch (Exception exp)
                    {
                        conn.Rollback();
                        throw exp;
                    }
                }
                ReStartQuque(model.mqpathid);
                return Json(new { code = 1, msg = "成功" });
            }
            catch (Exception e)
            {

                return Json(new { code = -1, msg = e.Message });
            }
        }
        /// <summary>
        /// 获取所有分区节点
        /// </summary>
        /// <returns></returns>
        public JsonResult GetNode()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                IList<string> list = new tb_datanode_dal().GetNodeList(conn);
                if (list != null && list.Count > 0)
                {
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 获取所有分区节点
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPartition(string datanodeid)
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                List<tb_partition_model> list = partitionDal.GetAllCanUsePartitionList(conn,Convert.ToInt32( datanodeid));
                if (list != null && list.Count > 0)
                {
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 获取所有队列
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMqPath()
        {
            using (DbConn conn = DbConfig.CreateConn(DataConfig.MqManage))
            {
                conn.Open();
                IList<tb_mqpath_model> list = mqPathDal.GetAllMaPath(conn);
                if (list != null && list.Count > 0)
                {
                    var m = (from a in list
                             select new
                             {
                                 key = a.id,
                                 value = a.mqpath
                             }).Distinct();

                    return Json(m, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
