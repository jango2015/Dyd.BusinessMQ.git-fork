using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.BusinessMQ.Consumer;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Test
{
    public class MQConsumerTaskDemo : XXF.BaseService.TaskManager.BaseDllTask
    {

        private MQConnInfo ConnInfo;
        public void ReceiveMessageDemo(Action<string> action)
        {


        }

        public override void Run()
        {
            if (ConnInfo == null)
            {
                //注册消费者消息,ConnInfo务必要在程序关闭后关掉（dispose）。否则导致异常终止,要人工等待连接超时后，方可重新注册。
                ConnInfo = XXF.ProjectTool.MQHelper.ReceiveMessage<string>(new XXF.ProjectTool.ReceiveMessageBusinessMQConfig()
                {
                    ManageConnectString = "server=192.168.17.201;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=Xx~!@#;",
                    MaxReceiveMQThread = 1,//并行处理的线程数,一般为1足够,若消息处理慢,又想并行消费,则考虑 正在使用的分区=并行处理线程数 为并行效率极端最优,但cpu消耗应该不小。
                    PartitionIndexs = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }//消费者订阅的分区顺序号,从1开始
                },
                    "dyd.mytest", //接收的队列要正确
                    "dyd.mytest.customer1", //clientid,接收消息的(消费者)唯一标示,一旦注册以后，不能更改，业务下线废弃后必须要告知运维，删除消费者注册。
                    (r) =>//接收消息的回调
                    {
                        this.OpenOperator.Log("测试消息接收测试:" + r.ObjMsg);
                        r.MarkFinished();//告知BusinessMQ改消息已经被消费,被处理完毕。否则消息会报错,该分区下的消息消费被终止。
                    });
            }
            this.OpenOperator.Log("测试消息注册成功");
        }

        public override void Dispose()
        {
            if (ConnInfo != null)
            {
                ConnInfo.Dispose();
                ConnInfo = null;
            }
        }
    }
}
