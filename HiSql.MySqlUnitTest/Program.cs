﻿using System;

namespace HiSql.MySqlUnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            HiSqlClient sqlClient = Demo_Init.GetSqlClient();

            Demo_Query.Init(sqlClient);
            //Demo_Update.Init(sqlClient);
            //DemoCodeFirst.Init(sqlClient);
            //Demo_Insert.Init(sqlClient);

            //Demo_Delete.Init(sqlClient);
        }
    }
}
