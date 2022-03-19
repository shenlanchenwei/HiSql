﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiSql.HanaUnitTest
{
    class Demo_DbCode
    {
        public static void Init(HiSqlClient sqlClient)
        {
            //Create_Table(sqlClient); //ok
            //Demo_AddColumn(sqlClient); //ok
             Demo_ReColumn(sqlClient);//
            //Demo_ModiColumn(sqlClient); //
            //Demo_DelColumn(sqlClient);////
            //Demo_Tables(sqlClient);//
            //Demo_View(sqlClient);//
            //Demo_AllTables(sqlClient);//
            //Demo_GlobalTables(sqlClient);//  delay
            //Demo_ModiTable(sqlClient);//

            //Demo_DropView(sqlClient); //
            // Demo_CreateView(sqlClient);//
            //Demo_ModiView(sqlClient);//

            //Demo_IndexList(sqlClient);//
            //Demo_Index_Create(sqlClient);//
            // Demo_ReTable(sqlClient);//

        }
        static void Create_Table(HiSqlClient sqlClient)
        {
            sqlClient.CodeFirst.CreateTable(typeof(Table.Hi_Test02));
        }

        static void Demo_ModiTable(HiSqlClient sqlClient)
        {
            //OpLevel.Execute  表示执行并返回生成的SQL
            //OpLevel.Check 表示仅做检测失败时返回消息且检测成功时返因生成的SQL
            var tabinfo = sqlClient.Context.DMInitalize.GetTabStruct("hi_test03");

            TabInfo _tabcopy = ClassExtensions.DeepCopy<TabInfo>(tabinfo);
            //_tabcopy.Columns.RemoveAt(4);

            //HiColumn newcol = ClassExtensions.DeepCopy<HiColumn>(_tabcopy.Columns[1]);
            //newcol.FieldName = "Testname3";
            //newcol.ReFieldName = "Testname3";
            //_tabcopy.Columns.Add(newcol);
            _tabcopy.Columns[1].ReFieldName = "UName_04";

            //_tabcopy.Columns[4].IsRequire = true;

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
            var rtn = sqlClient.DbFirst.ReTable("hi_test02", "hi_test02_01", OpLevel.Execute);
            if (rtn.Item1)
            {
                Console.WriteLine(rtn.Item2);//输出成功消息
                Console.WriteLine(rtn.Item3);//输出重命名表 生成的SQL
            }
            else
                Console.WriteLine(rtn.Item2);//输出重命名失败原因

        }

        static void Demo_Index_Create(HiSqlClient sqlClient)
        {


            TabInfo tabInfo = sqlClient.Context.DMInitalize.GetTabStruct("hi_test01");
            List<HiColumn> hiColumns = tabInfo.Columns.Where(c => c.FieldName == "ModiTime" || c.FieldName == "ModiName").ToList();
            var rtn = sqlClient.DbFirst.CreateIndex("hi_test01", "idx_hi_test01_ModiTime123", hiColumns, OpLevel.Execute);
            if (rtn.Item1)
                Console.WriteLine(rtn.Item3);
            else
                Console.WriteLine(rtn.Item2);

            rtn = sqlClient.DbFirst.DelIndex("hi_test01", "idx_hi_test01_ModiTime123", OpLevel.Execute);

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

            List<TabIndexDetail> lstindexdetails = sqlClient.DbFirst.GetTabIndexDetail("Hi_FieldModel", "PK_Hi_FieldModel_ed721f6b-296a-447e-ac67-7d02fd8e338c");
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
            var rtn = sqlClient.DbFirst.DropView("vw_FModel",

                OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
            Console.WriteLine(rtn.Item3);
        }

        static void Demo_AddColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "hi_test01",
                FieldName = "TestAdd3a",
                FieldType = HiType.VARCHAR,
                FieldLen = 50,
                DBDefault = HiTypeDBDefault.EMPTY,
                DefaultValue = "",
                FieldDesc = "测试字段添加aa"

            };

            var rtn = sqlClient.DbFirst.AddColumn("hi_test01", column, OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
        }

        static void Demo_DelColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "hi_test01",
                FieldName = "TestAdd",
                FieldType = HiType.VARCHAR,
                FieldLen = 50,
                DBDefault = HiTypeDBDefault.EMPTY

            };

            var rtn = sqlClient.DbFirst.DelColumn("hi_test01", column, OpLevel.Execute);

            Console.WriteLine(rtn.Item2);
        }

        static void Demo_ModiColumn(HiSqlClient sqlClient)
        {
            HiColumn column = new HiColumn()
            {
                TabName = "hi_test01",
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
                TabName = "hi_test01",
                FieldName = "UName",
                ReFieldName = "UName_01",
                FieldType = HiType.VARCHAR,
                FieldLen = 500,
                DBDefault = HiTypeDBDefault.VALUE,
                DefaultValue = "TGM",
                FieldDesc = "测试字段变qq更"

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