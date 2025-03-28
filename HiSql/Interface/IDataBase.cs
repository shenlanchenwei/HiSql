﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
namespace HiSql
{
    /// <summary>
    /// 数据库基础操作接口(各类数据库操作都要实现以下接口)
    /// </summary>
    public partial interface IDataBase
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; set; }

        /// <summary>
        /// 事务
        /// </summary>
        IDbTransaction Transaction { get; set; }

        /// <summary>
        /// 连接上下文
        /// </summary>
        HiSqlProvider Context { get; set; }

        // <summary>
        /// 是否启用SQL日志记录
        /// </summary>
        bool IsLogSql { get; set; }



        #region  执行前,后,超时 等监控

        /// <summary>
        /// 执行前调用的方法
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        void ExecBefore(string sql, HiParameter[] param);


        /// <summary>
        /// sql执行后调用的方法
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        void ExecAfter(string sql, HiParameter[] param);


        #endregion`


        /// <summary>
        /// 执行行SQL的类型(Text,StoredProcedure,TableDirect)
        /// </summary>
        CommandType CmdTyp { get; set; }





        /// <summary>
        /// 返回IDataReader接口（标参)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDataReader GetDataReader(string sql, object parameters);
        /// <summary>
        /// 返回IDataReader接口（HiParameter)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>

        IDataReader GetDataReader(string sql, params HiParameter[] parameters);


        /// <summary>
        /// 返回映射好的DataTable(标参)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        //DataTable GetDataTable(string sql, object parameters);




        /// <summary>
        /// 返回映射好的DataTable(HiParameter）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable GetDataTable(string sql, params HiParameter[] parameters);


        DataSet GetDataSet(List<string> sqls, List<HiParameter[]> parameters);

        /// <summary>
        /// 执行Sql语句 并返回受影响的行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecCommandSynch(string sql, params HiParameter[] parameters);


        /// <summary>
        /// 批量
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tabInfo"></param>
        /// <param name="columnMapping"></param>
        /// <returns></returns>
        int ExecBulkCopyCommand(DataTable sourceTable, TabInfo tabInfo, Dictionary<string, string> columnMapping = null);


        /// <summary>
        /// 批量插入数据对象
        /// </summary>
        /// <param name="lstdata"></param>
        /// <param name="tabInfo"></param>
        /// <param name="columnMapping"></param>
        /// <returns></returns>
        int ExecBulkCopyCommand<T>(List<T> lstdata, TabInfo tabInfo, Dictionary<string, string> columnMapping = null);

        /// <summary>
        /// 异步批量写入
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tabInfo"></param>
        /// <param name="columnMapping"></param>
        /// <returns></returns>
        Task<int> ExecBulkCopyCommandAsync(DataTable sourceTable, TabInfo tabInfo, Dictionary<string, string> columnMapping = null);

        /// <summary>
        /// 批量异步插入数据对象
        /// </summary>
        /// <param name="lstdata"></param>
        /// <param name="tabInfo"></param>
        /// <param name="columnMapping"></param>
        /// <returns></returns>
        Task<int> ExecBulkCopyCommandAsync<T>(List<T> lstdata, TabInfo tabInfo, Dictionary<string, string> columnMapping = null);

        /// <summary>
        /// 执行Sql语句 并返回受影响的行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<int> ExecCommandAsync(string sql, object parameters);


        /// <summary>
        /// 执行Sql语句 并返回受影响的行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecCommand(string sql, params HiParameter[] parameters);

        /// <summary>
        /// 执行Sql语句 并返回受影响的行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<int> ExecCommandAsync(string sql, params HiParameter[] parameters);

        /// <summary>
        /// 返回结果的首行首列 其它忽略
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>
        ///
        /// </returns>
        object ExecScalar(string sql);


        /// <summary>
        /// 返回结果的首行首列 其它忽略
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>
        ///
        /// </returns>
        Task<object> ExecScalarAsync(string sql);

        /// <summary>
        /// 释放对象
        /// </summary>
        void Dispose();
        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
        //打开连接
        void Open();


        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginTran();

        /// <summary>
        /// 开启事务 并指定锁定级别
        /// </summary>
        /// <param name="iso"></param>
        void BeginTran(IsolationLevel iso);

        /// <summary>
        /// 事务回滚
        /// </summary>
        void RollBackTran();

        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTran();
        Task<IDataReader> GetDataReaderAsync(string sql, params HiParameter[] parameters);
    }
}
