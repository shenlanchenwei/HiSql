﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HiSql
{
    public static class ClassExtensions
    {
        //将当前类的属性赋给另外一个类


        public static T deepCloneProperty<T>(T thisValue, T t) where T :class {
            if (thisValue == null) return t;
            var aType = thisValue.GetType();
            var bType = t.GetType();
            List<PropertyInfo> aList = aType.GetProperties().ToList<PropertyInfo>();
            var canList = aList.Where(it => it.CanRead && it.CanWrite).ToList();
            foreach (var aProp in canList)
            {
                var bProp = bType.GetProperty(aProp.Name);
                if (bProp != null)
                {
                    object o = aProp.GetValue(thisValue, null);
                    bProp.SetValue(t, Convert.ChangeType(o, aProp.PropertyType), null);
                }
            }
            return t;
        }

        public static T DeepCopy<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制   

                //DataContractSerializer ser = new DataContractSerializer(typeof(object));
                //ser.WriteObject(objectStream, RealObject);
                //return (T)ser.ReadObject(objectStream);

                //已经过时的处理方式

                BinaryFormatter formatter = new BinaryFormatter();
                
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }   

        public static T  MoveCross<T>(this T thisValue ,T t) where T : class
        {
            return deepCloneProperty(thisValue, t);
        }
        public static T1 MoveCross<T, T1>(this T thisValue, T1 t) where T : class where T1 : class
        {
            var aType = thisValue.GetType();
            var bType = t.GetType();
            List<PropertyInfo> aList = aType.GetProperties().ToList<PropertyInfo>();
            List<PropertyInfo> bList = bType.GetProperties().ToList<PropertyInfo>();
            var canList = aList.Where(it => it.CanRead).ToList();
            foreach (var prop in canList)
            {
                
                var bProp = bType.GetProperty(prop.Name);
                if (bProp == null) continue;
                //必须两个属性的相同
                if (bProp!=null &&  prop.PropertyType == bProp.PropertyType && bProp.CanWrite==true)
                {
                    bProp.SetValue(t, prop.GetValue(thisValue, null), null);
                }

            }
            return t;
        }

        /// <summary>
        /// 对数据进行分类汇总
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="thisValue"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<T1> Collect<T, T1>(this List<T> thisValue, T1 t) where T : class where T1 : class,new() {

            var aType = typeof(T);
            var bType =typeof(T1);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            List<PropertyInfo> aList = aType.GetProperties().ToList<PropertyInfo>();
            List<PropertyInfo> bList = bType.GetProperties().ToList<PropertyInfo>();

            //创建索引 增加性能
            Dictionary<string, T1> indexDic = new Dictionary<string, T1>();
            List<T1> result = new List<T1>();

            var aStrLst = aList.Where(it => it.PropertyType.IsIn(Constants.StringType, Constants.DateTimeOffsetType, Constants.TimeSpanType, Constants.BoolType) && it.CanRead).ToList();
            var bNumLst = bList.Where(it => it.PropertyType.IsIn(Constants.IntType, Constants.LongType, Constants.DobType, Constants.FloatType, Constants.ShortType, Constants.DecType)
            && it.CanWrite).ToList();
            var bStrLst = bList.Where(it => it.PropertyType.IsIn(Constants.StringType, Constants.DateTimeOffsetType, Constants.TimeSpanType, Constants.BoolType) && it.CanWrite).ToList();



            //sw.Stop();
            //TimeSpan ts2 = sw.Elapsed;
            //Console.WriteLine("Collect 初始 总共花费{0}ms.", ts2.TotalMilliseconds);
            //var key= string.Join("-",(string[]) bNumLst.Select(it => it.Name.ToMd5()).ToArray());
            string _keyvalue = string.Empty;
            //sw.Reset();
            //sw.Start();
            PropertyInfo aProp = null;
            foreach (var aTemp in thisValue)
            {
                _keyvalue = string.Empty;
                
                foreach (var bProp in bStrLst)
                {
                    aProp = aType.GetProperty(bProp.Name);
                    _keyvalue += aProp.GetValue(aTemp, null).ToString();
                }
                //_keyvalue = _keyvalue.ToMd5();
                T1 item ;
                if (!indexDic.ContainsKey(_keyvalue))
                {
                    //item = (T1)Activator.CreateInstance(bType);
                    item = new T1();
                    foreach (var bProp in bStrLst)
                    {
                        aProp = aType.GetProperty(bProp.Name);
                        bProp.SetValue(item, aProp.GetValue(aTemp, null));
                    }
                    foreach (var bProp in bNumLst)
                    {
                        var btypinfo = aType.GetProperty(bProp.Name);
                        //类型要一致 目前是强校验 其实也可以做成隐式转换
                        if (btypinfo != null && btypinfo.PropertyType == bProp.PropertyType)
                        {
                            bProp.SetValue(item, btypinfo.GetValue(aTemp, null));
                        }
                        else
                        {
                            bProp.SetValue(item, 0);
                        }
                    }
                    indexDic.Add(_keyvalue, item);
                }
                else
                {
                    item = (T1)indexDic[_keyvalue];
                    foreach (var bProp in bNumLst)
                    {
                        var btypinfo = aType.GetProperty(bProp.Name);

                        //var btypinfo = _dic_a_info[bProp.Name];

                        //类型要一致 目前是强校验 其实也可以做成隐式转换
                        if (btypinfo != null && btypinfo.PropertyType == bProp.PropertyType)
                        {
                            //temp += bProp.GetValue(aTemp, null) ;
                            if (btypinfo.PropertyType == Constants.IntType)
                            {
                                int _tmp = (int)bProp.GetValue(item);
                                _tmp += (int)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else if (btypinfo.PropertyType == Constants.LongType)
                            {
                                long _tmp = (long)bProp.GetValue(item);
                                _tmp += (long)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else if (btypinfo.PropertyType == Constants.DobType)
                            {
                                double _tmp = (double)bProp.GetValue(item);
                                _tmp += (double)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else if (btypinfo.PropertyType == Constants.FloatType)
                            {
                                float _tmp = (float)bProp.GetValue(item);
                                _tmp += (float)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else if (btypinfo.PropertyType == Constants.ShortType)
                            {
                                short _tmp = (short)bProp.GetValue(item);
                                _tmp += (short)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else if (btypinfo.PropertyType == Constants.DecType)
                            {
                                decimal _tmp = (decimal)bProp.GetValue(item);
                                _tmp += (decimal)btypinfo.GetValue(aTemp, null);
                                bProp.SetValue(item, _tmp);
                                continue;
                            }
                            else
                                bProp.SetValue(item, 0);

                        }
                        else
                        {
                            bProp.SetValue(item, 0);
                        }
                    }
                    //indexDic[_keyvalue] = item;
                }
            }
            //sw.Stop();
            //ts2 = sw.Elapsed;
            //Console.WriteLine("Collect 转换 初始 总共花费{0}ms.", ts2.TotalMilliseconds);

            foreach (string key in indexDic.Keys)
            {
                result.Add(indexDic[key]);
            }
            return result;
        }

        public static T CloneProperoty<T>(this T thisValue) where T : class
        {
            var t = (T)Activator.CreateInstance(typeof(T));
            return deepCloneProperty(thisValue, t);
        }
    }



}
