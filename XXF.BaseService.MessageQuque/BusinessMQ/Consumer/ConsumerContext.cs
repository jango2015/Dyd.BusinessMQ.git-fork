using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消费者上下文
    /// </summary>
    public class ConsumerContext:IDisposable
    {
        /// <summary>
        /// 消费者提供者
        /// </summary>
        public ConsumerProvider ConsumerProvider { get; set; }
        /// <summary>
        /// 消费者信息
        /// </summary>
        public ConsumerInfo ConsumerInfo { get; set; }
        /// <summary>
        /// 消费者监听器
        /// </summary>
        public ReceiveMessageListener Listener { get; set; }
        /// <summary>
        /// 消费者内部消息队列
        /// </summary>
        public ReceiveMessageQuque Quque { get; set; }
        /// <summary>
        /// 消费者回调信息
        /// </summary>
        public ConsumerActionInfo ActionInfo { get; set; }

        /// <summary>
        /// 服务器标准时间(管理中心时间)
        /// </summary>
        public DateTime ManageServerTime { get { return DateTime.Now - _calibrateTimeSpan; } set { _calibrateTimeSpan = DateTime.Now - value; } }
        private TimeSpan _calibrateTimeSpan = TimeSpan.FromSeconds(0);//服务器标准时间和当前服务器时间的校准时间间隔

        /// <summary>
        /// 整个上下文信息是否需要重新获取启动
        /// </summary>
        public bool IsNeedReload { get; set; }

        public int GetMQPathID()
        {
            if (ConsumerInfo != null && ConsumerInfo.MQPathModel != null)
                return ConsumerInfo.MQPathModel.id;
            else
                return -1;

        }

        public string GetMQPath()
        {
            if (ConsumerInfo != null && ConsumerInfo.MQPathModel != null)
                return ConsumerInfo.MQPathModel.mqpath;
            else
                return "";

        }

        public void Dispose()
        {
            if (Quque != null)
            {
                Quque.Dispose();
                Quque = null;
            }
            if (Listener != null)
            { 
                Listener.Dispose();
                Listener = null;
            }

        }
    }
}
