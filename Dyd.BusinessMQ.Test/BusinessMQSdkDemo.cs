using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Common;
using XXF.BaseService.MessageQuque.BusinessMQ.Consumer;
using XXF.BaseService.MessageQuque.BusinessMQ.Producter;

namespace Dyd.BusinessMQ.Test
{
    /// <summary>
    /// BusinessMQ sdk Demo
    /// </summary>
    public class BusinessMQSdkDemo
    {
        public void SendMessageDemo(string msg)
        {
            //发送字符串示例
            var p = ProducterPoolHelper.GetPool(new BusinessMQConfig() { ManageConnectString = "server=192.168.17.201;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;" },//管理中心数据库
                "dyd.mytest3");//队列路径 .分隔,类似类的namespace,是队列的唯一标识，要提前告知运维在消息中心注册，方可使用。
            p.SendMessage(@"1");
            //发送对象示例
            /* var obj = new message2 { text = "文字", num = 1 };
              var p = ProducterPoolHelper.GetPool(new BusinessMQConfig() { ManageConnectString = "server=192.168.17.237;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;" },//管理中心数据库
                "test.diayadian.obj");//队列路径 .分隔,类似类的namespace,是队列的唯一标识，要提前告知运维在消息中心注册，方可使用。
            p.SendMessage<message>(obj);
            */
        }

        private ConsumerProvider Consumer;
        public void ReceiveMessageDemo(Action<string> action)
        {
            if (Consumer == null)
            {
                Consumer = new ConsumerProvider();
                Consumer.Client = "dyd.mytest3.customer1";//clientid,接收消息的(消费者)唯一标示,一旦注册以后，不能更改，业务下线废弃后必须要告知运维，删除消费者注册。
                Consumer.ClientName = "客户端名称";//这个相对随意些，主要是用来自己识别的，要简短
                Consumer.Config = new BusinessMQConfig() { ManageConnectString = "server=192.168.17.201;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;" };
                Consumer.MaxReceiveMQThread = 1;//并行处理的线程数,一般为1足够,若消息处理慢,又想并行消费,则考虑 正在使用的分区=并行处理线程数 为并行效率极端最优,但cpu消耗应该不小。
                Consumer.MQPath = "dyd.mytest3";//接收的队列要正确
                Consumer.PartitionIndexs = new List<int>() { 1, 2, 3,4, 5, 6, 7, 8 };//消费者订阅的分区顺序号,从1开始
                Consumer.RegisterReceiveMQListener<string>((r) =>
                {
                    /*
                       * 这些编写业务代码
                       * 编写的时候要注意考虑，业务处理失败的情况。
                       * 1.重试失败n次。
                       * 2.重试还不行，则标记消息已被处理。然后跳过该消息处理，自己另外文档记录这种情况。
                       * 消息被消费完毕，一定要调用MarkFinished，标记消息被消费完毕。
                       */
                    action.Invoke(r.ObjMsg);
                    r.MarkFinished();
                });
            }

        }
        /// <summary>
        /// 关闭消息订阅连接
        /// </summary>
        public void CloseReceiveMessage()
        {
            //注册消费者消息,消费者务必要在程序关闭后关掉（dispose）。否则导致异常终止,要人工等待连接超时后，方可重新注册。
            if (Consumer != null)
            {
                Consumer.Dispose();
                Consumer = null;
            }
        }
    }

    public class message2
    {
        public string text { get; set; }
        public int num { get; set; }
    }
}
