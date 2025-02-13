using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HiSql.TabLog.Interface
{
    public enum OperationType
    {
        Insert,
        Update,
        Delete
    }

    public abstract class IOperationLog
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public OperationType OperationType { get; set; }
    }

    public abstract class ICredentialModule<CredentialT, LogT>
        where CredentialT : ICredential<LogT>
        where LogT : IOperationLog
    {
        /// <summary>
        /// 生成凭证ID
        /// </summary>
        /// <returns></returns>
        protected abstract Task<CredentialT> GenerateCredential();

        /// <summary>
        /// 开始操作包，返回凭证ID
        /// </summary>
        /// <returns></returns>
        public async Task<T2> Execute<T2>(Func<Task<Tuple<T2, List<LogT>>>> operationFunc)
        {
            var executeResult = await operationFunc();
            _ = Task.Run(async () =>
            {
                var credential = await GenerateCredential();
                credential.OperationLogs = executeResult.Item2;
                await SaveCredential(credential);
            });
            return executeResult.Item1;
        }

        /// <summary>
        /// 保存操作包
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        protected abstract Task SaveCredential(CredentialT credential);
    }
}
