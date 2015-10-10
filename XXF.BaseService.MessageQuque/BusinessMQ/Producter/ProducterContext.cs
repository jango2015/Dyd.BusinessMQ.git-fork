using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Producter
{
    /// <summary>
    /// 生产者上下文信息
    /// </summary>
    public class ProducterContext:IDisposable
    {
        /// <summary>
        /// 生产者提供者信息
        /// </summary>
        public ProducterProvider ProducterProvider { get; set; }
        /// <summary>
        /// 生产者信息
        /// </summary>
        public ProducterInfo ProducterInfo { get; set; }
        /// <summary>
        /// 整个上下文信息是否需要重新获取启动
        /// </summary>
        public bool IsNeedReload { get; set; }
        /// <summary>
        /// 上次生成者错误时间(本地程序时间)
        /// </summary>
        public DateTime? SendMessageErrorTime { get; set; }
        /// <summary>
        /// 上一次MQPath的更新时间缓存
        /// </summary>
        public DateTime LastMQPathUpdateTime { get; set; }
        /// <summary>
        /// 服务器标准时间(管理中心时间)
        /// </summary>
        public DateTime ManageServerTime { get { return DateTime.Now - _calibrateTimeSpan; } set { _calibrateTimeSpan = DateTime.Now - value; } }
        private TimeSpan _calibrateTimeSpan = TimeSpan.FromSeconds(0);//服务器标准时间和当前服务器时间的校准时间间隔
        /// <summary>
        /// 当前上下文是否已经释放
        /// </summary>
        public bool Disposeing = false;

        public int GetMQPathID()
        {
            if (ProducterInfo != null && ProducterInfo.MqPathModel != null)
                return ProducterInfo.MqPathModel.id;
            else
                return -1;

        }

        public string GetMQPath()
        {
            if (ProducterInfo != null && ProducterInfo.MqPathModel != null)
                return ProducterInfo.MqPathModel.mqpath;
            else
                return "";

        }

        /// <summary>
        /// 上下文释放
        /// </summary>
        public void Dispose()
        {
            Disposeing = true;
            ProducterProvider = null;
            ProducterInfo = null;
        }
    }
}
