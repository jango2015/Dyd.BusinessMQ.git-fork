using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Extensions;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// MQ消息封装
    /// </summary>
    public class MQMessage
    {
        /// <summary>
        /// 转消息为T实体类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T MessageObj<T>()
        {

            try
            {
                if (typeof(T) == typeof(string))
                    return (T)Convert.ChangeType(Model.message, typeof(T));
                else
                    return new Serialization.JsonHelper().Deserialize<T>(Model.message);
            }
            catch (Exception exp)
            {
                throw new Exception("当前消息反序列化失败,json:" + Model.message.NullToEmpty(), exp);
            }
        }
        /// <summary>
        /// 基础消息信息
        /// </summary>
        public tb_messagequeue_model Model { get; set; }
        /// <summary>
        /// 当前消费者上下文
        /// </summary>
        public Consumer.ConsumerContext Context { get; set; }
        /// <summary>
        /// 是否已经消费
        /// </summary>
        public bool IsMarkFinished { get { return _isMarkFinished; } }
        private bool _isMarkFinished = false;
        /// <summary>
        /// 标记消息为已消费
        /// </summary>
        public void MarkFinished()
        {
            int consumerclientid = 0; int partitionid = 0; long mqid = 0;
            try
            {
                var mqidinfo = PartitionRuleHelper.GetMQIDInfo(Model.id); partitionid = PartitionRuleHelper.GetPartitionID(new PartitionIDInfo() { DataNodePartition = mqidinfo.DataNodePartition, TablePartition = mqidinfo.TablePartition });
                tb_consumer_partition_dal dal = new tb_consumer_partition_dal();
                int result = 0; consumerclientid = Context.ConsumerInfo.ConsumerClientModel.id; mqid = this.Model.id;
                //尝试标记消息已消费
                bool isupdatesuccess = false; int tryerrorcount = 0;
                while (isupdatesuccess == false)
                {
                    try
                    {
                        SqlHelper.ExcuteSql(Context.ConsumerProvider.Config.ManageConnectString, (c) =>
                        {
                            result = dal.UpdateLastMQID(c, consumerclientid, partitionid, mqid);
                        });
                        isupdatesuccess = true;
                    }
                    catch (Exception exp)
                    {
                        tryerrorcount++;
                        if (tryerrorcount <= SystemParamConfig.Consumer_TrySetMessageRead_FailCount)
                        {
                            System.Threading.Thread.Sleep((int)(SystemParamConfig.Consumer_TrySetMessageRead_ErrorSleepTime * 1000));
                            Log.ErrorLogHelper.WriteLine(Context.GetMQPathID(), Context.GetMQPath(), "MarkFinished", string.Format("标记消息'已完成'出错,尝试第{0}次", tryerrorcount + ""), exp);
                        }
                        else
                        {
                            throw new BusinessMQException(string.Format("标记消息'已完成'出错,放弃第{0}次重试!", tryerrorcount + ""), exp);
                        }
                    }
                }
                if (result != 1)
                    throw new BusinessMQException(string.Format("当前消息消费完成后更新失败,可能是系统级消息乱序消费导致。consumerclientid:{0},partitionid:{1},mqid:{2}",
                        Context.ConsumerInfo.ConsumerClientModel.id, partitionid, this.Model.id));
                //消费者端标记为已消费
                _isMarkFinished = true;
            }
            catch (Exception exp)
            {
                throw new BusinessMQException(string.Format("当前消息消费标记“已完成”时出错,consumerclientid:{0},partitionid:{1},mqid:{2}", consumerclientid, partitionid, mqid), exp);
            }
        }
    }
}
