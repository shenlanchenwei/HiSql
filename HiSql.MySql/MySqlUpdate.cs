﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HiSql
{
    public class MySqlUpdate : UpdateProvider
    {
        Dictionary<string, TabInfo> dictabinfo = new Dictionary<string, TabInfo>();

        StringBuilder sb = new StringBuilder();
        public MySqlUpdate() : base()
        {

        }

        public override string ToSql()
        {
            dictabinfo = new Dictionary<string, TabInfo>();
            sb = new StringBuilder();
            checkData();
            return sb.ToString(); ;

        }



        #region 私有方法

        void checkData()
        {
            //SqlServerDM sqldm = null;
            TabInfo tabinfo;
            string sql_where = string.Empty;
            if (this.Table != null)
            {
                //sqldm=  Instance.CreateInstance<SqlServerDM>($"{Constants.NameSpace}.{this.Context.CurrentConnectionConfig.DbType.ToString()}{DbInterFace.DM.ToString()}");

                //sqldm.Context = this.Context;
                tabinfo = Context.DMInitalize.GetTabStruct(this.Table.TabName);
                dictabinfo.Add(this.Table.TabName, tabinfo);
            }
            else
                throw new Exception("未指定要更新的表");

            if (this.Data.Count > 0)
            {
                Type type = this.Data[0].GetType();
                bool _isdic = type == typeof(Dictionary<string, string>);

                List<string> _field = new List<string>();
                bool _isonly = false;
                if (this.FieldsOnly.Count > 0)
                {
                    _isonly = true;
                    //只更新这些些字段
                    foreach (FieldDefinition fieldDefinition in this.FieldsOnly)
                    {
                        if (_field.Where(f => f.ToLower() == fieldDefinition.FieldName.ToLower()).Count() == 0)
                            _field.Add(fieldDefinition.FieldName);
                    }
                }

                else if (this.FieldsExclude.Count > 0)
                {
                    _isonly = false;
                    //除了这些这段其它都更新
                    foreach (FieldDefinition fieldDefinition in this.FieldsExclude)
                    {
                        if (_field.Where(f => f.ToLower() == fieldDefinition.FieldName.ToLower()).Count() == 0)
                            _field.Add(fieldDefinition.FieldName);
                    }
                }

                if (this.Filters != null && this.Filters.IsHiSqlWhere && !string.IsNullOrEmpty(this.Filters.HiSqlWhere.Trim()))
                {
                    sql_where = Context.DMTab.BuilderWhereSql(new List<TableDefinition> { this.Table }, dictabinfo, null, this.Filters.WhereParse.Result, false);
                }
                else if (this.Wheres.Count > 0)
                {
                    sql_where = Context.DMTab.BuilderWhereSql(new List<TableDefinition> { this.Table }, dictabinfo, null, this.Wheres, false);
                }

                foreach (object obj in this.Data)
                {
                    Tuple<Dictionary<string, string>, Dictionary<string, string>> result = this.CheckData(_isdic,this.Table, obj, (IDMInitalize)Context.DMInitalize, tabinfo, _field, _isonly);
                    if (result.Item1.Count > 0)
                    {
                        sb.AppendLine(Context.DMTab.BuildUpdateSql(tabinfo, this.Table, result.Item1, result.Item2, sql_where));
                    }
                }
            }
            else
                throw new Exception("无可更新数据");
        }
        #endregion
    }
}
