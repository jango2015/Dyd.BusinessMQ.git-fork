using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using XXF.Redis;

namespace XXF.BaseService.MessageQuque
{
    /// <summary>
    /// 简单Redis消息队列使用类
    /// </summary>
    public class SimpleRedisMQ
    {
        private const string MQTag = "Dyd.MQ.";
        private RedisClient client;
        public RedisClient GetClient { get { return client; } }

        public SimpleRedisMQ(RedisClient redislient)
        {
            client = redislient;
        }

        public SimpleRedisMQ(RedisDb redisdb)
        {
            client = redisdb.GetClient();
        }

        /// <summary>
        /// 发送消息
        /// 支持json可序列化,byte[],string类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queuename"></param>
        /// <param name="obj"></param>
        public void SendMessage<T>(string queuename,T obj)
        {
            byte[] data=null;
            if (!(obj is string)&&!(obj is byte[]))
            {
                var json = new Serialization.JsonHelper().Serializer(obj);
                data = XXF.Db.LibConvert.StrToBytes(json);
            }
            else if(obj is string)
            {
                data = XXF.Db.LibConvert.StrToBytes(obj as string);
            }
            else if (obj is byte[])
                data = obj as byte[];
            client.LPush(MQTag + queuename, data);
        }
        /// <summary>
        /// 非阻塞接收消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queuename"></param>
        /// <returns></returns>
        public T ReceiveMessages<T>(string queuename)
        {
            var bs = client.RPop(MQTag + queuename);
            if (typeof(T) == typeof(byte[]))
                return (T)Convert.ChangeType(bs, typeof(T));
            string json = XXF.Db.LibConvert.BytesToStr(bs);
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(json,typeof(T));
            return new Serialization.JsonHelper().Deserialize<T>(json);
        }

       /// <summary>
       /// 阻塞形式接收消息
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="queuename"></param>
       /// <param name="timeoutsecs"></param>
       /// <returns></returns>
        public T ReceiveMessageWait<T>(string queuename, int timeoutsecs)
        {
            var bs = client.BRPopValue(MQTag + queuename, timeoutsecs);
            if (typeof(T) == typeof(byte[]))
                return (T)Convert.ChangeType(bs, typeof(T));
            string json = XXF.Db.LibConvert.BytesToStr(bs);
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(json, typeof(T));
            return new Serialization.JsonHelper().Deserialize<T>(json);
        }
    }
}
