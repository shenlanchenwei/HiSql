﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiSql.MySqlUnitTest
{
    class Demo_DbCode
    {
        public static void Init(HiSqlClient sqlClient)
        {
            Demo_AddColumn(sqlClient); //ok
            //Demo_ReColumn(sqlClient);//ok
            //Demo_ModiColumn(sqlClient); //ok
            //Demo_DelColumn(sqlClient);////ok
            //Demo_Tables(sqlClient);//ok
            //Demo_View(sqlClient);//ok
            //Demo_AllTables(sqlClient);//ok
            //Demo_GlobalTables(sqlClient);//  delay
            //Demo_ModiTable(sqlClient);//ok

            //Demo_DropView(sqlClient); //ok
            //Demo_CreateView(sqlClient);//ok
            //Demo_ModiView(sqlClient);//ok

            //Demo_IndexList(sqlClient);//ok
            //Demo_Index_Create(sqlClient);//ok
            //Demo_ReTable(sqlClient);//ok

            //Demo_TableDataCount(sqlClient);
            //Demo_TablesPaging(sqlClient);
            //Demo_ViewsPaging(sqlClient);
            //Demo_AllTablesPaging(sqlClient);

            //Demo_Primary_Create(sqlClient);
        }
        static void Demo_AllTablesPaging(HiSqlClient sqlClient)
        {
            int total = 0;
            List<TableInfo> lsttales = sqlClient.DbFirst.GetAllTables("Hi", 11, 1, out total);
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }

        }
        static void Demo_ViewsPaging(HiSqlClient sqlClient)
        {
            int total = 0;
            List<TableInfo> lsttales = sqlClient.DbFirst.GetViews("HI", 11, 1, out total);
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
        }
        static void Demo_TablesPaging(HiSqlClient sqlClient)
        {
            int total = 0;
            List<TableInfo> lsttales = sqlClient.DbFirst.GetTables("HI", 11, 1, out total);
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($" {tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
            Console.WriteLine($"总数 {total}");

        }
        static void Demo_TableDataCount(HiSqlClient sqlClient)
        {
            int total = 0;
            int lsttales = sqlClient.DbFirst.GetTableDataCount("Hi_FieldModel");
            Console.WriteLine($" {lsttales} ");
        }


        static void Demo_ModiTable(HiSqlClient sqlClient)
        {
            //OpLevel.Execute  表示执行并返回生成的SQL
            //OpLevel.Check 表示仅做检测失败时返回消息且检测成功时返因生成的SQL
            var tabinfo = sqlClient.Context.DMInitalize.GetTabStruct("htest03");

            TabInfo _tabcopy = ClassExtensions.DeepCopy<TabInfo>(tabinfo);
            //_tabcopy.Columns.RemoveAt(4);
            //HiColumn newcol = ClassExtensions.DeepCopy<HiColumn>(_tabcopy.Columns[1]);
            //newcol.FieldName = "Testne32";
            //newcol.ReFieldName = "Testne32";
            //_tabcopy.Columns.Add(newcol);
            //_tabcopy.Columns[1].ReFieldName = "UName_85";

            //_tabcopy.Columns[4].IsRequire = true;

            _tabcopy.PrimaryKey.ForEach(x => {
                x.IsPrimary = false;
            });

            _tabcopy.Columns.ForEach(t => {
                if (t.FieldName == "SID" //|| t.FieldName == "Age"
                )
                {
                    t.IsPrimary = true;
                }
            });

            var rtn = sqlClient.DbFirst.ModiTable(_tabcopy, OpLevel.Execute);
            if (rtn.Item1)
            {
                Console.WriteLine(rtn.Item2);//输出成功消息
                Console.WriteLine(rtn.Item3);//输出 生成的SQL
            }
            else
                Console.WriteLine(rtn.Item2);//输出失败原因

        }

        static void Demo_ReTable(HiSqlClient sqlClient)
        {
            //OpLevel.Execute  表示执行并返回生成的SQL
            //OpLevel.Check 表示仅做检测失败时返回消息且检测成功时返因生成的SQL
            var rtn = sqlClient.DbFirst.ReTable("htest03_1", "htest03", OpLevel.Execute);
            if (rtn.Item1)
            {
                Console.WriteLine(rtn.Item2);//输出成功消息
                Console.WriteLine(rtn.Item3);//输出重命名表 生成的SQL
            }
            else
                Console.WriteLine(rtn.Item2);//输出重命名失败原因

        }
        static void Demo_Primary_Create(HiSqlClient sqlClient)
        {
            //删除主键
            List<TabIndex> lstindex = sqlClient.DbFirst.GetTabIndexs("htest01").Where(t => t.IndexType == "Key_Index").ToList();
            foreach (var item in lstindex)
            {
                var rtndel = sqlClient.DbFirst.DelPrimaryKey(item.TabName, OpLevel.Execute);
                if (rtndel.Item1)
                    Console.WriteLine(rtndel.Item3);
                else
                    Console.WriteLine(rtndel.Item2);
            }

            //创建主键
            TabInfo tabInfo = sqlClient.Context.DMInitalize.GetTabStruct("htest01");
            List<HiColumn> hiColumns = tabInfo.Columns.Where(c => c.FieldName == "SID").ToList();
            hiColumns.ForEach((c) => {
                c.IsPrimary = true;
            });
            var rtn = sqlClient.DbFirst.CreatePrimaryKey("htest01", hiColumns, OpLevel.Execute);
            if (rtn.Item1)
                Console.WriteLine(rtn.Item3);
            else
                Console.WriteLine(rtn.Item2);

        }
        static void Demo_Index_Create(HiSqlClient sqlClient)
        {


            TabInfo tabInfo = sqlClient.Context.DMInitalize.GetTabStruct("htest01");
            List<HiColumn> hiColumns = tabInfo.Columns.Where(c => c.FieldName == "ModiTime" || c.FieldName == "ModiName").ToList();
            var rtn = sqlClient.DbFirst.CreateIndex("htest01", "idx_htest01_ModiTime123", hiColumns, OpLevel.Execute);
            if (rtn.Item1)
                Console.WriteLine(rtn.Item3);
            else
                Console.WriteLine(rtn.Item2);

            rtn = sqlClient.DbFirst.DelIndex("htest01", "idx_htest01_ModiTime123", OpLevel.Execute);

            if (rtn.Item1)
                Console.WriteLine(rtn.Item3);
            else
                Console.WriteLine(rtn.Item2);
        }

        static void Demo_IndexList(HiSqlClient sqlClient)
        {
            List<TabIndex> lstindex = sqlClient.DbFirst.GetTabIndexs("Hi_FieldModel");

            foreach (TabIndex tabIndex in lstindex)
            {
                Console.WriteLine($"TabName:{tabIndex.TabName} IndexName:{tabIndex.IndexName} IndexType:{tabIndex.IndexType}");
            }

            List<TabIndexDetail> lstindexdetails = sqlClient.DbFirst.GetTabIndexDetail("Hi_FieldModel", "primary");
            foreach (TabIndexDetail tabIndexDetail in lstindexdetails)
            {
                Console.WriteLine($"TabName:{tabIndexDetail.TabName} IndexName:{tabIndexDetail.IndexName} IndexType:{tabIndexDetail.IndexType} ColumnName:{tabIndexDetail.ColumnName}");

            }
        }

        static void Demo_Tables(HiSqlClient sqlClient)
        {
            List<TableInfo> lsttales = sqlClient.DbFirst.GetTables();
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
        }

        static void Demo_View(HiSqlClient sqlClient)
        {
            List<TableInfo> lsttales = sqlClient.DbFirst.GetViews();
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
        }
        static void Demo_GlobalTables(HiSqlClient sqlClient)
        {
            List<TableInfo> lsttales = sqlClient.DbFirst.GetGlobalTempTables();
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
        }

        static void Demo_AllTables(HiSqlClient sqlClient)
        {
            List<TableInfo> lsttales = sqlClient.DbFirst.GetAllTables();
            foreach (TableInfo tableInfo in lsttales)
            {
                Console.WriteLine($"{tableInfo.TabName}  {tableInfo.TabReName}  {tableInfo.TabDescript}  {tableInfo.TableType} 表结构:{tableInfo.HasTabStruct}");
            }
        }

        static void Demo_CreateView(HiSqlClient sqlClient)
        {
            var rtn = sqlClient.DbFirst.CreateView("vw_FModel_11",
                sqlClient.HiSql("select a.TabName,b.TabReName,b.TabDescript,a.FieldName,a.SortNum,a.FieldType from Hi_FieldModel as a inner join Hi_TabModel as b on a.TabName=b.TabName").ToSql(),

                OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
            Console.WriteLine(rtn.Item3);
        }

        static void Demo_ModiView(HiSqlClient sqlClient)
        {
            var rtn = sqlClient.DbFirst.ModiView("vw_FModel",
                sqlClient.HiSql("select a.TabName,b.TabReName,b.TabDescript,a.FieldName,a.SortNum,a.FieldType from Hi_FieldModel as a inner join Hi_TabModel as b on a.TabName=b.TabName where b.TabType in (0,1)").ToSql(),

                OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
            Console.WriteLine(rtn.Item3);
        }

        static void Demo_DropView(HiSqlClient sqlClient)
        {
            var rtn = sqlClient.DbFirst.DropView("vw_FModel_11",

                OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
            Console.WriteLine(rtn.Item3);
        }

        static void Demo_AddColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "HTest01",
                FieldName = "TestAA",
                FieldType = HiType.VARCHAR,
                FieldLen = 50,
                DBDefault = HiTypeDBDefault.EMPTY,
                DefaultValue = "",
                FieldDesc = "测试字AA段添加"

            };

            var rtn = sqlClient.DbFirst.AddColumn("htest01", column, OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
        }

        static void Demo_DelColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "htest01",
                FieldName = "TestAdd",
                FieldType = HiType.VARCHAR,
                FieldLen = 50,
                DBDefault = HiTypeDBDefault.EMPTY

            };

            var rtn = sqlClient.DbFirst.DelColumn("htest01", column, OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
        }

        static void Demo_ModiColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "htest01",
                FieldName = "TestAdd",
                FieldType = HiType.VARCHAR,
                FieldLen = 500,
                DBDefault = HiTypeDBDefault.VALUE,
                DefaultValue = "TGM",
                FieldDesc = "测试字段修改"

            };

            var rtn = sqlClient.DbFirst.ModiColumn(column.TabName, column, OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
            Console.WriteLine(rtn.Item3);



            Console.WriteLine(rtn.Item2);
        }

        static void Demo_ReColumn(HiSqlClient sqlClient)
        {
            //OpLevel.Execute  表示执行并返回生成的SQL
            //OpLevel.Check 表示仅做检测失败时返回消息且检测成功时返因生成的SQL
            HiColumn column = new HiColumn()
            {
                TabName = "htest02",
                FieldName = "UName",//UName
                ReFieldName = "UName_01",//UName_01
                FieldType = HiType.VARCHAR,
                FieldLen = 50,
                DBDefault = HiTypeDBDefault.VALUE,
                DefaultValue = "TGM",
                FieldDesc = "测试字段变更"

            };

            var rtn = sqlClient.DbFirst.ReColumn(column.TabName, column, OpLevel.Execute);
            if (rtn.Item1)
            {
                Console.WriteLine(rtn.Item2);//输出成功消息
                Console.WriteLine(rtn.Item3);//输出 生成的SQL
            }
            else
                Console.WriteLine(rtn.Item2);//输出失败原因
        }
    }
}
