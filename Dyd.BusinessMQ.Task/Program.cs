using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyd.BusinessMQ.Task
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConsumerAbNormalTask task = new ConsumerAbNormalTask();

            task.TestRun();
        }
    }
}
