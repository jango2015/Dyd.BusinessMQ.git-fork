using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.Consumer
{
    /// <summary>
    /// 消息分区队列 多线程安全
    /// </summary>
    public class PartitionQueue<T>
    {
        private Dictionary<int, Queue<T>> partitionQuque = new Dictionary<int, Queue<T>>();
        private object _ququelock = new object();

        public void Enqueue(int patition, List<T> Messages)
        {
            lock (_ququelock)
            {
                var q = GetQueque(patition);
                foreach (var m in Messages)
                    q.Enqueue(m);
            }

        }

        public T Dequeue(int patition)
        {
            lock (_ququelock)
            {
                var q = GetQueque(patition);
                if (q.Count != 0)
                    return q.Dequeue();
                else
                    return default(T);

            }
        }


        public void Clear()
        {
            lock (_ququelock)
            {
                partitionQuque.Clear();
            }
        }
        /// <summary>
        /// 获取所有分区中最大的队列数量（不安全方式）
        /// </summary>
        public int MaxCountOfPartition()
        {
            int count = 0;
            try
            {
                foreach (var p in partitionQuque)
                {
                    count = Math.Max(p.Value.Count,count);
                }
            }
            catch (Exception exp)
            { }

            return count;
        }

        private Queue<T> GetQueque(int partition)
        {
            if (!partitionQuque.ContainsKey(partition))
            {
                Queue<T> q = new Queue<T>();
                partitionQuque.Add(partition, q);
            }

            return partitionQuque[partition];
        }

    }
}
