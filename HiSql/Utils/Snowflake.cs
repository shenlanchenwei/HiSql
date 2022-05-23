﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiSql
{


    public enum SnowType
    {
        IdWorker=0,
        IdSnow=1

    }


    /// <summary>
    /// 雪花ID生成工具
    /// </summary>
    public class Snowflake 
    {
        static IdGenerate idGenerate = null;

        static long _tick = -1L;

        static int _workerid = 0;


        static SnowType snowType=SnowType.IdSnow;


        /// <summary>
        /// 指定雪花ID生成引擎
        /// </summary>

        public static SnowType SnowType
        {
            get { return snowType; }
            set { snowType = value; }
        }

        /// <summary>
        ///  设置时间戳
        /// </summary>
        public static long TickTick
        {
            get {
                if (_tick < 0L)
                    _tick=(long) (new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)- new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                return _tick; }
            set {
                idGenerate = getIdGenerate();
                _tick = value;
            }
        }

        /// <summary>
        /// 指定机器编码 0-31之间
        /// </summary>
        public static int WorkerId
        {
            get { return _workerid; }
            set {
                if (value >= 0 && value <= 31)
                {
                    idGenerate = getIdGenerate();
                    _workerid = value;
                }
                else
                {
                    throw new Exception($"机器码只允许0-31之间");
                }
            }
        }


        static IdGenerate getIdGenerate()
        {
            if (snowType == SnowType.IdWorker)
            {
                return new IdWorker(_workerid, TickTick);
            }else
                return new IdSnow(_workerid, TickTick);
        }

        /// <summary>
        /// 生成雪花ID
        /// </summary>
        /// <param name="workid"></param>
        /// <returns></returns>
        public static long NextId()
        {
            if (idGenerate == null)
                idGenerate = getIdGenerate();

            return idGenerate.NextId();
        }
    }
}
