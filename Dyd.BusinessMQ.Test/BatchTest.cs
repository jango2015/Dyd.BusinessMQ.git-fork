using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XXF.BaseService.MessageQuque.BusinessMQ.Producter;
using XXF.ProjectTool;

namespace Dyd.BusinessMQ.Test
{
    public partial class BatchTest : Form
    {
        string ms = @"1) 计算机或相关专业本科及以上学历；
2) 具有4年以上.NET/Java开发经验，熟悉常用的设计模式和开源框架，有大型互联网项目经验或业内知名产品研发经验者优先；
3) 精通面向对象分析设计方法，熟悉系统分析设计工具，逻辑能力佳；
4) 熟悉WCF，熟悉SOA构架；
5) 熟悉SqlServer/MySQL数据库管理系统、相关技术及工具，具有大数据以及高并发的系统数据库开发经验优先；
6) 良好的学习能力、逻辑思维能力，善于沟通和团队合作，勇于创新和接受挑战。；
7) 熟悉以下技术领域中的且经验丰富者优先：
a、 参与过大型或超大型平台建设架构设计、搭建者优先考虑；
b、 熟悉分布式系统基础设施中常用的技术，如缓存（Varnish、Memcache、Redis）、消息中间件(Rabbit MQ、Active MQ、Kafka、NSQ)等。
c、 熟悉高并发、高可靠性系统的设计、监控的原理，如负载均衡系统、集群和应用监控、流量控制、性能优化、日志收集和分析等；
d、 了解分布式存储和分布式计算，熟悉SOA, 有服务治理的经验，服务调用框架。
";
        public BatchTest()
        {
            InitializeComponent();
            config = new XXF.ProjectTool.SendMessageBusinessMQConfig() { ManageConnectString = "server=10.251.255.135;Initial Catalog=dyd_bs_MQ_manage;User ID=sa;Password=2014Xx~!@#;" };
        }
        SendMessageBusinessMQConfig config;
        private void button1_Click(object sender, EventArgs e)
        {
            var time = XXF.Log.TimeWatchLog.Debug(() =>
            {
                int count = Convert.ToInt32(this.tbcount.Text.Trim());
                //System.Threading.Tasks.Parallel.For(0, count, new System.Threading.Tasks.ParallelOptions() { MaxDegreeOfParallelism = Convert.ToInt32(tbbingxing.Text.Trim()) }, (i) =>
                //{
                List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                for (int i = 0; i < count; i++)
                {
                    var t = new System.Threading.Tasks.TaskFactory().StartNew(() =>
                    {
                        if (XXF.ProjectTool.MQHelper.SendMessage<string>(
                       config, //管理中心数据库
                        this.tbPath.Text, //队列路径 .分隔,类似类的namespace,是队列的唯一标识，要提前告知运维在消息中心注册，方可使用。
                       ms + ms) == false)//发送对象
                        {
                            throw new Exception("发送失败");
                        }
                    });
                    tasks.Add(t);
                }
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
            });
            //});

            MessageBox.Show("耗时:" + time);

            //string timeline = "";
            //foreach (var m in ProducterTimeWatchTest.Messages)
            //{
            //    timeline += m + "\r\n";
            //}
            //System.IO.File.WriteAllText("timewatch.report.txt", timeline);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var time = XXF.Log.TimeWatchLog.Debug(() =>
                {
                    if (XXF.ProjectTool.MQHelper.SendMessage<string>(
                    config,
                    "dyd.mytest2", //队列路径 .分隔,类似类的namespace,是队列的唯一标识，要提前告知运维在消息中心注册，方可使用。
                    "-1") == false)//发送对象
                    {
                        throw new Exception("发送失败");
                    }
                });

                MessageBox.Show("耗时:" + time);
            }
            catch (Exception exp)
            {
                MessageBox.Show("错误:" + exp.Message);
            }
        }

        private void BatchTest_Load(object sender, EventArgs e)
        {

        }
    }
}
