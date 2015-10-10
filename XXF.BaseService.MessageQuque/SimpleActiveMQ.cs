using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.Util;

namespace XXF.BaseService.MessageQuque
{
    /// <summary>
    /// 简单ActiveMQ基础服务使用类
    /// </summary>
    public class SimpleActiveMQ:IDisposable
    {
        public XXF.ActiveMQ.ActiveMQConnection Connection { get; set; }
        public ISession Session { get; set; }

        public SimpleActiveMQ(XXF.ActiveMQ.ActiveMQConnection connection, ISession session)
        {
            Connection = connection;
            Session = session;
        }
        /// <summary>
        /// 消息转换成对象（仅支持json/普通文本）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static T GetMessage<T>(IMessage msg)
        {
            ITextMessage text = msg as ITextMessage;
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(text.Text, typeof(T));
            return new Serialization.JsonHelper().Deserialize<T>(text.Text);
        }

        /// <summary>
        /// 发送消息
        /// 支持json可序列化,byte[],string类型
        /// mqpath:"queue://FOO.BAR",topic://FOO.BAR 示例
        /// </summary>
        public void SendMessage<T>(T obj, string mqpath, Dictionary<string, string> propertydic, ActiveMQMsgDeliveryMode mode = ActiveMQMsgDeliveryMode.Persistent)
        {
            var json = "";
            if (!(obj is string))
                json = new Serialization.JsonHelper().Serializer(obj);
            else
                json = obj as string;
            IDestination destination = SessionUtil.GetDestination(Session, mqpath);
            //通过会话创建生产者，方法里面new出来的是MQ中的Queue
            IMessageProducer prod = Session.CreateProducer(destination);
            //创建一个发送的消息对象
            ITextMessage message = prod.CreateTextMessage();
            //给这个对象赋实际的消息
            message.Text = json;
            //设置消息对象的属性，这个很重要哦，是Queue的过滤条件，也是P2P消息的唯一指定属性
            if (propertydic != null)
            {
                foreach (var map in propertydic)
                {
                    message.Properties.SetString(map.Key,map.Value);
                }
            }
            //生产者把消息发送出去，几个枚举参数MsgDeliveryMode是否长链，MsgPriority消息优先级别，发送最小单位，当然还有其他重载
            prod.Send(message, (MsgDeliveryMode)mode, MsgPriority.Normal, TimeSpan.MinValue);
        }

        /// <summary>
        /// 发送消息
        /// 支持json可序列化,byte[],string类型
        /// mqpath:"queue://FOO.BAR",topic://FOO.BAR 示例
        /// </summary>
        public void SendMessage(ActiveMQSendMessageParams param)
        {
            SendMessage<string>(param.objjson, param.mqpath, param.propertydic, param.mode);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="action"></param>
        /// <param name="clientid"></param>
        /// <param name="mqpath"></param>
        /// <param name="propertyselect"></param>
        /// <param name="mode"></param>
        public void RegisterReceiveMessageListener(Action<IMessage> action, string clientid, string mqpath, string propertyselect = "", ActiveMQMsgDeliveryMode mode = ActiveMQMsgDeliveryMode.Persistent)
        {
            IDestination destination = SessionUtil.GetDestination(Session, mqpath);
            //通过会话创建一个消费者，这里就是Queue这种会话类型的监听参数设置
            IMessageConsumer consumer = CreateMessageConsumer(destination, clientid, propertyselect, mode);
            //注册监听事件
            consumer.Listener += new MessageListener(action);
        }

        private IMessageConsumer CreateMessageConsumer(IDestination destination,string clientid, string propertyselect = "", ActiveMQMsgDeliveryMode mode = ActiveMQMsgDeliveryMode.Persistent)
        {
             IMessageConsumer consumer =null;
            if(mode == ActiveMQMsgDeliveryMode.NonPersistent)
                consumer = Session.CreateConsumer(destination, propertyselect);
            else
                consumer = Session.CreateDurableConsumer((ITopic)destination, clientid, (string.IsNullOrEmpty(propertyselect) ? null:propertyselect),false);
            return consumer;
        }
        /// <summary>
        /// 手工资源释放关闭
        /// </summary>
        public void Close()
        {
            Dispose();
        }
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (Session != null)
                Session.Close();
            if (Connection != null)
            {
                if (Connection.Connection.IsStarted)
                    Connection.Connection.Stop();
                Connection.Dispose();

            }
        }
    }
    /// <summary>
    /// actviemq持久化类型
    /// </summary>
    public enum ActiveMQMsgDeliveryMode
    {
        /// <summary>
        /// 持久化
        /// </summary>
        Persistent = 0,
        /// <summary>
        /// 非持久化
        /// </summary>
        NonPersistent = 1,
    }
    /// <summary>
    /// ActiveMQ发送消息参数
    /// </summary>
    public class ActiveMQSendMessageParams
    {
        public string tcpconnectstring { get; set; }
        public string objjson { get; set; }
        public string mqpath { get; set; }
        public Dictionary<string, string> propertydic { get; set; }
        public ActiveMQMsgDeliveryMode mode { get; set; }
    }
}
