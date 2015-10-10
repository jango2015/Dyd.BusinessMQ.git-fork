using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XXF.BaseService.MessageQuque.BusinessMQ.SystemRuntime
{
    /// <summary>
    /// 分区规则帮助类
    /// </summary>
    public class PartitionRuleHelper
    {
        /// <summary>
        /// 分区命名规则
        /// </summary>
        /// <param name="partitionid"></param>
        /// <returns></returns>
        public static string PartitionNameRule(int partitionid)
        {
            return (partitionid + "").PadLeft(2, '0');
        }
        /// <summary>
        /// 获取消息表名
        /// </summary>
        /// <param name="tablepartition"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static string GetTableName(int tablepartition,DateTime day)
        {
            return "tb_messagequeue_" + PartitionNameRule(tablepartition) + "_" + day.ToString("yyMMdd");
        }
        /// <summary>
        /// 根据消息表名获取隐藏的规则信息
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static TableInfo GetTableInfo(string tablename)
        {
            string partition = tablename.Replace("tb_messagequeue_", "");string[] partitioninfo = partition.Split('_');
            return new TableInfo() { TablePartition = Convert.ToInt32(partitioninfo[0]), Day = DateTime.ParseExact(partitioninfo[1], "yyMMdd", CultureInfo.InvariantCulture) };
        }
        /// <summary>
        /// 获取数据节点名
        /// </summary>
        /// <param name="datanodepartition"></param>
        /// <returns></returns>
        public static string GetDataNodeName(int datanodepartition)
        {
            return SystemParamConfig.DataNode_DataBaseName_Prefix + PartitionNameRule(datanodepartition);
        }

        /// <summary>
        /// 获取MQ消息ID 消息id号,规则1+数据节点编号+表分区编号+时间分区号+自增id
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static long GetMQID(MQIDInfo info)
        {
            string id = "1" + PartitionNameRule(info.DataNodePartition) + PartitionNameRule(info.TablePartition) + info.Day.ToString("yyMMdd") + info.AutoID.ToString().PadLeft(8, '0');
            return Convert.ToInt64(id);
        }
        /// <summary>
        /// 获取MQID隐藏的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MQIDInfo GetMQIDInfo(long id)
        {
            MQIDInfo info = new MQIDInfo();string example = "1010115062900000000";string strid = id.ToString();
            if(strid.Length!=example.Length)
                throw new Exception("消息Id格式不正确:"+id);
            info.DataNodePartition = Convert.ToInt32( strid.Substring(1,2));
            info.TablePartition = Convert.ToInt32(strid.Substring(3,2));
            info.Day = DateTime.ParseExact(strid.Substring(5, 6), "yyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            info.AutoID = Convert.ToInt32(strid.Substring(11,8));
            return info;
        }
        /// <summary>
        /// 获取分区id号  分区id号,规则1+数据节点编号+表分区编号
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static int GetPartitionID(PartitionIDInfo info)
        {
            //PartitionIDInfo info = new PartitionIDInfo();string example = "10101";string strid = id.ToString();
            string id = "1" + PartitionNameRule(info.DataNodePartition) + PartitionNameRule(info.TablePartition );
            return Convert.ToInt32(id);
        }
        /// <summary>
        /// 获取分区id号隐藏的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PartitionIDInfo GetPartitionIDInfo(int id)
        {
            PartitionIDInfo info = new PartitionIDInfo(); string example = "10101"; string strid = id.ToString();
            if(strid.Length!=example.Length)
                throw new Exception("分区Id格式不正确:"+id);
            info.DataNodePartition = Convert.ToInt32(strid.Substring(1, 2));
            info.TablePartition = Convert.ToInt32(strid.Substring(3, 2));
            return info;
        }
    }
    /// <summary>
    /// MQID号隐藏的规则信息
    /// </summary>
    public class MQIDInfo
    {
        public int DataNodePartition { get; set; }
        public int TablePartition { get; set; }
        public DateTime Day { get; set; }
        public int AutoID { get; set; }
    }
    /// <summary>
    /// 分区ID号隐藏的规则信息
    /// </summary>
    public class PartitionIDInfo
    {
         public int TablePartition { get; set; }
         public int DataNodePartition { get; set; }
    }
    /// <summary>
    /// 获取消息表隐藏的规则信息
    /// </summary>
    public class TableInfo
    {
        public int TablePartition { get; set; }
        public DateTime Day { get; set; }
    }
}
