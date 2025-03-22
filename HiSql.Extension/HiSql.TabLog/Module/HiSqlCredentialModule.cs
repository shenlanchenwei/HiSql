using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using HiSql.Common.Entities.TabLog;
using HiSql.Interface.TabLog;
using HiSql.TabLog.Ext;
using HiSql.TabLog.Interface;
using HiSql.TabLog.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HiSql.TabLog.Module
{
    public class HiSqlCredentialModule : ICredentialModule
    {
        public HiSqlCredentialModule() { }

        protected override Task<Credential> InitCredential()
        {
            var credential = new Credential();
            return Task.FromResult(credential);
        }

        /// <summary>
        /// 回滚操作
        /// </summary>
        /// <param name="credentialId"></param>
        /// <param name="rollbackClient"></param>
        /// <param name="state"></param>
        /// <param name="operateUserName"></param>
        /// <returns></returns>
        public override async Task<Credential> RollbackCredential(
            string credentialId,
            HiSqlClient rollbackClient,
            object state,
            string operateUserName
        )
        {
            var mangerObj = (Hi_TabManager)state;
            List<Th_DetailLog> operateList;
            //回滚操作
            using (var logClient = InstallTableLog.GetSqlClientByName(mangerObj.DbServer))
            {
                var operateCredential = logClient
                    .Query(mangerObj.MainTabLog)
                    .Field("*")
                    .Where(
                        new Filter
                        {
                            { "LogId", OperType.EQ, credentialId },
                            { "TabName", OperType.EQ, mangerObj.TabName }
                        }
                    )
                    .ToList<Th_MainLog>()
                    .FirstOrDefault();
                if (operateCredential == null)
                    throw new Exception("该凭证不存在!");
                if (operateCredential.IsRecover == 1)
                    throw new Exception("该凭证已被回滚!");

                operateList = logClient
                    .Query(mangerObj.DetailTabLog)
                    .Field("*")
                    .Where(
                        new Filter
                        {
                            { "LogId", OperType.EQ, credentialId },
                            { "TabName", OperType.EQ, mangerObj.TabName }
                        }
                    )
                    .ToList<Th_DetailLog>();

                var modifyRows = new List<IDictionary<string, object>>();
                var delRows = new List<IDictionary<string, string>>();
                List<OperationType> operationTypes = new List<OperationType>();
                foreach (var item in operateList)
                {
                    switch (item.ActionModel)
                    {
                        case "C":
                            var tempDelRows = JsonConvert.DeserializeObject<
                                List<IDictionary<string, string>>
                            >(item.NewVal);
                            delRows.AddRange(tempDelRows);
                            break;
                        case "M":
                            var tempModifyRows = JsonConvert.DeserializeObject<
                                List<IDictionary<string, object>>
                            >(item.OldVal);
                            modifyRows.AddRange(tempModifyRows);
                            break;
                        case "D":
                            var tempCreateRows = JsonConvert.DeserializeObject<
                                List<IDictionary<string, object>>
                            >(item.OldVal);
                            modifyRows.AddRange(tempCreateRows);
                            break;
                        default:
                            throw new Exception("不支持的操作类型:" + item.ActionModel);
                    }
                }

                var logs = await ApplyDataOperate(
                    rollbackClient,
                    modifyRows,
                    delRows,
                    mangerObj.TabName,
                    operationTypes
                );
                var upCount = await logClient
                    .Update(mangerObj.MainTabLog)
                    .Set(new { IsRecover = 1 })
                    .Where(
                        new Filter
                        {
                            { "LogId", OperType.EQ, credentialId },
                            { "TabName", OperType.EQ, mangerObj.TabName },
                            { "IsRecover", OperType.EQ, 0 }
                        }
                    )
                    .ExecCommandAsync();
                if (upCount < 1)
                    throw new Exception("该凭证已被回滚,或凭证不存在!");

                var newCredential = await SaveCredential(
                    logs,
                    mangerObj,
                    operateUserName,
                    credentialId
                );
                return newCredential;
            }
        }

        /// <summary>
        /// 应用数据操作
        /// </summary>
        /// <param name="mainClient"></param>
        /// <param name="tableName"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public override async Task<List<OperationLog>> ApplyDataOperate(
            HiSqlClient mainClient,
            List<IDictionary<string, object>> modifyRows,
            List<IDictionary<string, string>> delRows,
            string tableName,
            List<OperationType> operationTypes
        )
        {
            var operateLogs = new List<OperationLog>();
            var tableInfo = mainClient.DbFirst.GetTabStruct(tableName);
            var primaryJsonList = JArray.FromObject(
                tableInfo.Columns.Where(r => r.IsPrimary).ToList()
            );
            var randm = new Random().Next(100000, 999999);
            var tempTableName = "#" + tableName + "_" + randm;
            var primaryList = new List<HiColumn>();
            //如果主键列少于1个,则抛出异常
            if (primaryJsonList.Count < 1)
                throw new Exception("表没有主键!");
            foreach (var primary in primaryJsonList)
            {
                var column = primary.ToObject<HiColumn>();
                column.TabName = tempTableName;
                primaryList.Add(column);
            }

            var tempTableInfo = new TabInfo
            {
                Columns = primaryList,
                TabModel = new HiTable { TabName = tempTableName }
            };

            var delLog = new OperationLog
            {
                NewValue = new List<IDictionary<string, object>>(0),
                OldValue = new List<IDictionary<string, object>>(),
                OperationType = OperationType.Delete,
                TableName = tableName,
            };
            var addLog = new OperationLog
            {
                NewValue = new List<IDictionary<string, object>>(0),
                OldValue = new List<IDictionary<string, object>>(),
                OperationType = OperationType.Insert,
                TableName = tableName,
            };
            var updateLog = new OperationLog
            {
                NewValue = new List<IDictionary<string, object>>(0),
                OldValue = new List<IDictionary<string, object>>(),
                OperationType = OperationType.Update,
                TableName = tableName,
            };
            // 如果operationTypes里包含删除和更新操作则需要创建临时表
            if (operationTypes.Contains(OperationType.Update) || operationTypes.Contains(OperationType.Delete))
            {
                //创建临时表
                var createResult = mainClient.DbFirst.CreateTable(tempTableInfo);
                var tempTableDataList = new List<IDictionary<string, object>>();

                foreach (var row in modifyRows)
                {
                    var tempRow = new Dictionary<string, object>();
                    foreach (var primary in primaryList)
                        tempRow[primary.FieldName] = row[primary.FieldName];
                    tempTableDataList.Add(tempRow);
                }
                foreach (var row in delRows)
                {
                    var tempRow = new Dictionary<string, object>();
                    foreach (var primary in primaryList)
                        tempRow[primary.FieldName] = row[primary.FieldName];
                    tempTableDataList.Add(tempRow);
                }
                //将新增行和修改行插入临时表
                var insertResult = mainClient.Insert(tempTableName, tempTableDataList).ExecCommand();

                //查询出新增行和修改行
                //var queryR = mainClient.Query(tempTableName).As("t1").Field("*").ToTable();

                var fields = new List<string>();
                foreach (var field in tableInfo.Columns)
                    fields.Add("t2." + field.FieldName);

                var query = mainClient
                    .Query(tempTableName)
                    .As("t1")
                    .Field(fields.ToArray())
                    .Join(tableName, JoinType.Left)
                    .As("t2");
                //string joinStr = "";
                //循环主键列生成连接条件
                var obj = new JoinOn();
                foreach (var primary in primaryList)
                    obj.Add("t1." + primary.FieldName, "t2." + primary.FieldName);

                query = query.On(obj);
                var updateOldList = query.ToEObject();
                //删除临时表
                mainClient.DbFirst.DropTable(tempTableName);
                var oldDataMap = new Dictionary<string, IDictionary<string, object>>();
                if (updateOldList.Count > 0)
                    foreach (var item in updateOldList)
                    {
                        var row = item as IDictionary<string, object>;
                        var key = "";
                        foreach (var col in primaryList)
                        {
                            var keyValue = row[col.FieldName];
                            if (keyValue == null)
                                break;

                            key += keyValue.ToString();
                        }
                        //如果是空数据跳出循环
                        if (key == "" && updateOldList.Count == 1)
                            break;
                        oldDataMap[key] = row;
                    }

                foreach (var row in delRows)
                {
                    var key = "";
                    foreach (var col in primaryList)
                        key += row[col.FieldName].ToString();
                    if (oldDataMap.TryGetValue(key, out var rowValue))
                    {
                        //删除行
                        delLog.OldValue.Add(rowValue);
                        //删除字典中的键值对
                        oldDataMap.Remove(key);
                    }
                }
                //找出新增行和修改行
                foreach (var row in modifyRows)
                {
                    var key = "";
                    foreach (var col in primaryList)
                        key += row[col.FieldName].ToString();
                    if (oldDataMap.TryGetValue(key, out var rowValue))
                    {
                        //修改行
                        updateLog.OldValue.Add(rowValue);
                        updateLog.NewValue.Add(row);
                        //删除字典中的键值对
                        oldDataMap.Remove(key);
                    }
                    else
                    {
                        //新增行
                        addLog.NewValue.Add(row);
                    }
                }
            }
            else
            {
                addLog.NewValue.AddRange(modifyRows);
            }
            if (delLog.OldValue.Count > 0)
                operateLogs.Add(delLog);
            if (addLog.NewValue.Count > 0)
                operateLogs.Add(addLog);
            if (updateLog.OldValue.Count > 0)
                operateLogs.Add(updateLog);
            return operateLogs;
        }

        protected override Task SaveCredential(Credential credential)
        {
            var state = (Hi_TabManager)credential.State;
            credential.CredentialId = SnroNumber.NewNumber(state.SNRO, state.SNUM);
            TabLogQueue.EnqueueLog(credential);
            return Task.CompletedTask;
        }

        protected Hi_TabManager GetTableLogSetting(string tableName, HiSqlClient client)
        {
            var cacheKey = "HiSqlOperateAndLog:" + tableName;
            var tableLogSetting = CacheContext.Cache.GetCache<Hi_TabManager>(cacheKey);
            if (tableLogSetting != null)
                return tableLogSetting;
            var managerTabName = typeof(Hi_TabManager).Name;
            var settingList = client
                .Query(managerTabName)
                .Field("*")
                .Where(new Filter { { "TabName", OperType.EQ, tableName } })
                .ToList<Hi_TabManager>();
            if (settingList == null || settingList.Count < 1)
                return null;
            var setting = settingList.First();
            //初始化自增
            InstallTableLog.InitTableLog(client, setting);
            if (setting != null)
                CacheContext.Cache.SetCache(cacheKey, setting);
            return setting;
        }

        /// <summary>
        /// 获取表日志数据详情
        /// </summary>
        /// <param name="state"></param>
        /// <param name="queryWhereBuilder"></param>
        /// <returns></returns>
        public static async Task<Tuple<List<ExpandoObject>, int>> GetTableDetailLogs(
            object state,
            Func<IQuery, IQuery> queryWhereBuilder
        )
        {
            var tabManagerObj = (Hi_TabManager)state;

            //MCount,CCount,DCount,IsRecover,RefLogId
            using (var queryClient = InstallTableLog.GetSqlClientByName(tabManagerObj.DbServer))
            {
                IQuery query = queryClient
                    .Query(tabManagerObj.DetailTabLog)
                    .As("t1")
                    .Field(
                        "t1.*",
                        "t2.IsRecover",
                        "t2.MCount",
                        "t2.CCount",
                        "t2.DCount",
                        "t2.RefLogId"
                    )
                    .Join(tabManagerObj.MainTabLog, JoinType.Right)
                    .As("t2");
                query = query
                    .On(new JoinOn { { "t1.LogId", "t2.LogId" }, { "t1.TabName", "t2.TabName" } })
                    .Sort("t1.CreateTime desc");
                query = queryWhereBuilder(query);
                var totalCount = 0;
                var tempSql = query.ToSql();
                var list = await query.ToEObjectAsync(ref totalCount);
                return Tuple.Create(list, totalCount);
            }
        }

        /// <summary>
        /// 获取日志主表数据
        /// </summary>
        /// <param name="state"></param>
        /// <param name="queryWhereBuilder"></param>
        /// <returns></returns>
        public static async Task<Tuple<List<ExpandoObject>, int>> GetTableMainLogs(
            Hi_TabManager state,
            Func<IQuery, IQuery> queryWhereBuilder
        )
        {
            //MCount,CCount,DCount,IsRecover,RefLogId
            using (var queryClient = InstallTableLog.GetSqlClientByName(state.DbServer))
            {
                IQuery query = queryClient.Query(state.MainTabLog).Sort("CreateTime desc");
                query = queryWhereBuilder(query);
                var totalCount = 0;
                var tempSql = query.ToSql();
                var list = await query.ToEObjectAsync(ref totalCount);
                return Tuple.Create(list, totalCount);
            }
        }

        public override async Task RecordLog(
            HiSqlProvider sqlProvider,
            string tableName,
            List<Dictionary<string, object>> data,
            Func<Task<bool>> func, List<OperationType> operationTypes
        )
        {
            Stopwatch watch;
            using (var sqlClient = sqlProvider.CloneClient())
            {
                await Execute(
                    async (tabName) =>
                    {
                        //获取表日志设置耗时
                        watch = Stopwatch.StartNew();
                        var state = GetTableLogSetting(tableName, sqlClient);
                        watch.Stop();
                        Console.WriteLine($"获取表日志设置耗时：{watch.ElapsedMilliseconds}ms");
                        var addData = data.Select(r => (IDictionary<string, object>)r).ToList();
                        //应用数据操作耗时
                        watch = Stopwatch.StartNew();
                        var operateLogs = await ApplyDataOperate(
                            sqlClient,
                            addData,
                            new List<IDictionary<string, string>>(0),
                            tableName,
                            operationTypes
                        );
                        watch.Stop();
                        Console.WriteLine($"应用数据操作耗时：{watch.ElapsedMilliseconds}ms");
                        return Tuple.Create(operateLogs, state as object);
                    },
                    tableName,
                    sqlClient.CurrentConnectionConfig.User
                );
            }
            //执行原方法耗时
            watch = Stopwatch.StartNew();
            await func();
            watch.Stop();
            Console.WriteLine($"执行原方法耗时：{watch.ElapsedMilliseconds}ms");
        }


    }
}
