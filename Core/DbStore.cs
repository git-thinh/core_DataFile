using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Core
{
    public interface IRequest
    {
        void Request(Msg m);
    }

    public class DbStore : IRequest
    {
        #region [ === VARIABLE - C'TOR === ]

        private readonly ILog log;

        private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> storeLock;
        private readonly ConcurrentDictionary<string, IList> storeData;

        private readonly ISender sender;

        public DbStore(ILog _log, ISender _sender)
            : this(_log)
        {
            sender = _sender;
        }

        public DbStore(ILog _log)
        {
            log = _log;
            // The higher the concurrencyLevel, the higher the theoretical number of operations
            // that could be performed concurrently on the ConcurrentDictionary.  However, global
            // operations like resizing the dictionary take longer as the concurrencyLevel rises. 
            // For the purposes of this example, we'll compromise at ProcessorCount * 2.
            storeLock = new ConcurrentDictionary<string, ReaderWriterLockSlim>(Environment.ProcessorCount * 2, 101);
            storeData = new ConcurrentDictionary<string, IList>(Environment.ProcessorCount * 2, 101);
        }

        #endregion

        public void Request(Msg m)
        {
            switch (m.DataAction)
            {
                case DataAction.DB_RESULT:
                    break;
                case DataAction.DB_SELECT:
                    item_Select(m);
                    break;
                case DataAction.DB_ADD:
                    Item_AddOrUpdate(m);
                    break;
                case DataAction.DB_REG_MODEL:
                    Model mo = (Model)m.Data;
                    DbType.Add(mo.ToString(), mo.Namespace + "." + mo.ClassName, log);
                    break;
                default:
                    break;
            }
        }

        #region [ === ITEM === ]

        private bool Item_Remove(object item)
        {
            //int index = -1;
            //if (item == null) return false;

            //Type type = item.GetType();
            //string key = type.FullName;

            //index = type_GetIndex(key);
            //if (index == -1)
            //    index = type_AddOrUpdate(type);
            //else
            //{
            //    IList list = null;
            //    ReaderWriterLockSlim rw = null;

            //    lock (_lockRW)
            //        storeLock.TryGetValue(index, out rw);

            //    if (rw != null)
            //    {
            //        using (rw.WriteLock())
            //        {
            //            if (storeData.TryGetValue(index, out list))
            //            {
            //                int index_it = list.IndexOf(item);
            //                if (index_it != -1)
            //                {
            //                    list[index_it] = null;
            //                    storeData[index] = list;
            //                    return true;
            //                }
            //            }
            //        }
            //    }
            //}
            return false;
        }

        private int Item_AddOrUpdate(Msg m)
        {
            string type_name = m.DataType;
            object item = m.Data;

            if (DbType.Exist(type_name) == false) DbType.Add(item.GetType());

            int index = -1;
            if (string.IsNullOrEmpty(type_name)) return index;

            IList list = null;
            ReaderWriterLockSlim rw = null;

            if (storeLock.TryGetValue(type_name, out rw) && rw != null)
            {
                /////////////////////////////////////////////
                // UPDATE
                using (rw.WriteLock())
                {
                    if (storeData.TryGetValue(type_name, out list) && list != null)
                    {
                        index = list.Count;
                        var pro = item.GetType().GetProperty("_index");
                        pro.SetValue(item, index, null);

                        list.Add(item);
                        storeData[type_name] = list;
                    }
                }
            }
            else
            {
                /////////////////////////////////////////////
                // ADD NEW
                if (storeLock.ContainsKey(type_name) == false)
                {
                    rw = new ReaderWriterLockSlim();
                    storeLock.TryAdd(type_name, rw);
                }
                using (rw.WriteLock())
                {
                    if (storeData.ContainsKey(type_name) == false)
                    {
                        Type type = item.GetType();
                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(type);
                        var instance = Activator.CreateInstance(constructedListType);
                        IList list_new = (IList)instance;

                        index = 0;
                        var pro = item.GetType().GetProperty("_index");
                        pro.SetValue(item, index, null);

                        list_new.Add(item);

                        storeData.TryAdd(type_name, list_new);
                    }
                }
            }

            return index;
        }

        //private int Item_AddOrUpdate(object item)
        //{
        //    if (item == null) return -1;
        //    string type_name = item.GetType().FullName;
        //    return Item_AddOrUpdate(type_name, item);
        //}

        private int[] item_Select(Msg m)
        {
            string type_name = m.DataType,
                condition = m.Data as string,
                select = m.Message as string;

            if (string.IsNullOrEmpty(type_name) || string.IsNullOrEmpty(condition)) return new int[] { };

            Type type = DbType.Get(type_name);

            IList list = null;
            ReaderWriterLockSlim rw = null;

            if (storeLock.TryGetValue(type_name, out rw) && rw != null)
            {
                using (rw.WriteLock())
                    if (storeData.TryGetValue(type_name, out list) && list != null)
                    {
                        try
                        {
                            var li = list.AsQueryable().Where(condition);
                            if (!string.IsNullOrEmpty(select))
                                li = li.Select(select);

                            var li0 = list.AsQueryable().Where(condition).ToNonAnonymousList(type); 
                            //string.Join(",", type.GetProperties().Select(x => x.Name).ToArray());
                            ////var ls0 = li.AsQueryable().Select(type, "new (\"0\" as Id,it.Key.Language, it.Key.VersionId)");
                            //string s1 = JsonConvert.SerializeObject(li, Formatting.Indented);

                            var ls = li.ToListDynamicClass();

                            Msg o1 = new Msg()
                            {
                                Status = true,
                                _Type = type,
                                DataAction = DataAction.DB_RESULT,
                                Data = JsonConvert.SerializeObject(ls[0], Formatting.Indented)
                            };
                            o1.PostToURL("http://127.0.0.1:10101/");
                        }
                        catch (Exception ex)
                        {
                        }
                    }
            }
            return new int[] { };
        }//end function



        #endregion
    }
}


/*
var l0 = list.AsQueryable().Select("_index").ToListDynamicClass();
                            string s0 = JsonConvert.SerializeObject(l0, Formatting.Indented);
                            var l1 = list.AsQueryable().Select("new(_index)");//.ToListDynamicClass();
                            string s1 = JsonConvert.SerializeObject(l1, Formatting.Indented);

                            var l2 = list.AsQueryable().Where(condition);//.ToListDynamicClass();
                            string s2 = JsonConvert.SerializeObject(l2, Formatting.Indented);

                            DynamicProperty[] props = new DynamicProperty[] 
                            {  
                                new DynamicProperty("Name", typeof(string)),  
                                new DynamicProperty("Birthday", typeof(DateTime)) 
                            };

                            Type type2 = DynamicExpression.CreateClass(props);
                            object o3 = Activator.CreateInstance(type2);
                            type2.GetProperty("Name").SetValue(o3, "Albert", null);
                            type2.GetProperty("Birthday").SetValue(o3, new DateTime(1879, 3, 14), null);

                            string s3 = JsonConvert.SerializeObject(o3, Formatting.Indented);

                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(type2);
                            var instance = Activator.CreateInstance(constructedListType);
                            IList ll3 = (IList)instance;

                            ll3.Add(o3);


                            LambdaExpression e4 = DynamicExpression.ParseLambda(type, typeof(bool), "Username = @0", "thinh");
                            LambdaExpression e5 = DynamicExpression.ParseLambda(type, typeof(bool), "Username = @0", "tu");
                            var ll5 = list.AsQueryable().Where("@0(it) or @1(it)", e4, e5);
                            string s5 = JsonConvert.SerializeObject(ll5, Formatting.Indented);
                             
                            var ll6 = list.AsQueryable().Select("_index").Cast<int>().ToArray();// .ToListDynamicClass();
                            string s6 = JsonConvert.SerializeObject(ll6, Formatting.Indented);

                            //int[] a = new int[ll6.Count];
                            //ll6.CopyTo(a, 0);

                            string ss = "";
                            ////////// a reference parameter
                            ////////var x = Expression.Parameter(type, "x");
                            ////////// contains method
                            ////////var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            ////////// reference a field
                            //////////var fieldExpression = Expression.Property(instance, "PropertyName");
                            ////////MemberExpression fieldExpression = Expression.PropertyOrField(x, "Username");
                            ////////// your value
                            ////////var valueExpression = Expression.Constant("thinh");
                            ////////// call the contains from a property and apply the value
                            ////////var containsValueExpression = Expression.Call(fieldExpression, containsMethod, valueExpression);

                            //////////// create your final lambda Expression
                            //////////var filterLambda = Expression.Lambda<Func<YourType, bool>>(containsValueExpression, x);
                            //////////// apply on your query
                            //////////var ll6 = list.AsQueryable().Where(finalLambda);


                            ////////////ParameterExpression param = Expression.Parameter(type, "x");
                            ////////////MemberExpression left = Expression.PropertyOrField(param, "Columns");
                            ////////////List<string> listTest = new List<string>();
                            //////////////listTest.Add(criteria);
                            ////////////ConstantExpression consToCompare = Expression.Constant(listTest, typeof(List<string>)); //???
                            ////////////var body = Expression.Equal(left, consToCompare);
                            //////////////var lambda = Expression.Lambda<Func<Item, bool>>(body, param);  
                            ////////////////var lambda = {x => (x.Columns = value(System.Collections.Generic.List`1[System.String]))} 
                            //////////////lambda = {x => (x.Columns[3] = value(System.Collections.Generic.List`1[System.String]))} 

                            //////////LambdaExpression e = DynamicExpression.ParseLambda(type, typeof(bool), "City = @0 and Orders.Count >= @1", "London", 10); 
                            //////////LambdaExpression e = DynamicExpression.ParseLambda(typeof(Customer), typeof(bool), "City = @0 and Orders.Count >= @1","London", 10); 
                            //////////Expression<Func<Customer, bool>> e1 = c => c.City == "London";
                            //////////Expression<Func<Customer, bool>> e2 = DynamicExpression.ParseLambda<Customer, bool>("Orders.Count >= 10");
                            //////////IQueryable<Customer> query = db.Customers.Where("@0(it) and @1(it)", e1, e2);


 
                             * products.OrderBy("Category.CategoryName, UnitPrice descending");
                             * orders.Where("OrderDate >= DateTime(2007, 1, 1)");
                             * customers.Where("Orders.Any(Total >= 1000)");

                            var q_prim = t_data
                                        .Where("FILTER == @0 && COLLAPSED_IDENTIFIER == @1 && RESPONSE_TYPE == @2", "Company Overall", "Company Overall", "Favorable")
                                        .OrderBy("FILTER, TYPE")
                                        .Select('new{@0}', strQueryFields);
                            var dynamic = trans.AsQueryable()
                                        .GroupBy("new(AccountNumber)", "it")
                                        .Select(@"new(  Key.AccountNumber as groupId,
                                                        ToArray(Value) as groupList,
                                                        it as data)");

var mySum = trans
    .AsQueryable()
    .GroupBy("new(Type)", "new(Value)")
    .Select("new(Key.Type as Type, Sum(Value) as Total)");

 





















































































*/