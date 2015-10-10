using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Test
{
    /// <summary>
    /// BusinessMQ公司内部sdk二次封装使用Demo,兼容activemq及极端错误时的消息存档
    /// 极端消息错误存档需要配置:MQErrorConnectString
    /// </summary>
    public class BusinessMQDemo
    {
        public void SendMessageDemo(string msg)
        {
            //发送字符串示例
            XXF.ProjectTool.MQHelper.SendMessage<string>(
                new XXF.ProjectTool.SendMessageBusinessMQConfig() { ManageConnectString = "server=192.168.17.237;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;" }, //管理中心数据库
                "dyd.test", //队列路径 .分隔,类似类的namespace,是队列的唯一标识，要提前告知运维在消息中心注册，方可使用。
                msg);//发送对象
            //发送对象示例
            /* var obj = new message { text = "文字", num = 1 };
             XXF.ProjectTool.MQHelper.SendMessage<message>(
            new XXF.ProjectTool.SendMessageBusinessMQConfig() { ManageConnectString = "server=192.168.17.237;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;" }, //管理中心数据库
             "test.diayadian.obj",
             obj);*/
        }

        private MQConnInfo ConnInfo;
        public void ReceiveMessageDemo(Action<string> action)
        {
            //注册消费者消息,ConnInfo务必要在程序关闭后关掉（dispose）。否则导致异常终止,要人工等待连接超时后，方可重新注册。
            ConnInfo = XXF.ProjectTool.MQHelper.ReceiveMessage<string>(new XXF.ProjectTool.ReceiveMessageBusinessMQConfig()
            {
                ManageConnectString = "server=192.168.17.237;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;",
                MaxReceiveMQThread = 1,//并行处理的线程数,一般为1足够,若消息处理慢,又想并行消费,则考虑 正在使用的分区=并行处理线程数 为并行效率极端最优,但cpu消耗应该不小。
                PartitionIndexs = new List<int> {1,2,3,4,5,6,7,8 }//消费者订阅的分区顺序号,从1开始
            },
                "dyd.test", //接收的队列要正确
                "dyd.test.customer1", //clientid,接收消息的(消费者)唯一标示,一旦注册以后，不能更改，业务下线废弃后必须要告知运维，删除消费者注册。
                (r) =>//接收消息的回调
                {
                    /*
                     * 这些编写业务代码
                     * 编写的时候要注意考虑，业务处理失败的情况。
                     * 1.重试失败n次。
                     * 2.重试还不行，则标记消息已被处理。然后跳过该消息处理，自己另外文档记录这种情况。
                     * 消息被消费完毕，一定要调用MarkFinished，标记消息被消费完毕。
                     */
                    action.Invoke(r.ObjMsg);

                    r.MarkFinished();//告知BusinessMQ改消息已经被消费,被处理完毕。否则消息会报错,该分区下的消息消费被终止。
                });


        }
        /// <summary>
        /// 关闭消息订阅连接
        /// </summary>
        public void CloseReceiveMessage()
        {
            //注册消费者消息,ConnInfo务必要在程序关闭后关掉（dispose）。否则导致异常终止,要人工等待连接超时后，方可重新注册。
            if (ConnInfo != null)
            { 
                ConnInfo.Dispose();
                ConnInfo = null;
            }
        }
    }

    public class message
    {
        public string text { get; set; }
        public int num { get; set; }
    }
}
