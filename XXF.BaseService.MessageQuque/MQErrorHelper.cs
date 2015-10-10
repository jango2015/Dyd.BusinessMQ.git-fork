using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XXF.BaseService.MessageQuque.Dal;
using XXF.BaseService.MessageQuque.Model;
using XXF.Common;
using XXF.ProjectTool;

namespace XXF.BaseService.MessageQuque
{
    /// <summary>
    /// MQ出错时的处理类
    /// </summary>
    public class MQErrorHelper
    {
        /// <summary>
        /// MQ失败时,临时存储消息到异地，便于后续恢复
        /// </summary>
        /// <param name="resendinfo"></param>
        public static void Error(MQReSendInfo resendinfo)
        {
            if (!string.IsNullOrEmpty(XXF.Common.XXFConfig.MQErrorConnectString))
            {
                SqlHelper.ExcuteSql(XXF.Common.XXFConfig.MQErrorConnectString, (c) =>
                {
                    tb_mqerror_dal errordal = new tb_mqerror_dal();
                    errordal.Add2(c, new tb_mqerror_model() { MQMsgJson = resendinfo.MQMsgJson, MQPath = resendinfo.MQPath, MQType = (byte)resendinfo.MQType, TryCount = 0 }, RandomHelper.Next(1,XXFConfig.MQMaxTablePartitionNum + 1));
                });
            }
        }

    }
    /// <summary>
    /// MQ重发消息
    /// </summary>
    public class MQReSendInfo
    {
        public MQType MQType { get; set; }
        public string MQPath { get; set; }
        public string MQMsgJson { get; set; }
    }
    /// <summary>
    /// MQ类型
    /// </summary>
    public enum MQType
    {
        ActiveMQ = 1,
        RedisMQ = 2,
        BusinessMQ=3,
    }
}
