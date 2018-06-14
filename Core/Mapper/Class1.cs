using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Core
{
    public class Mapper
    {
        private static Dictionary<KeyValuePair<Type, Type>, object> _maps = new Dictionary<KeyValuePair<Type, Type>, object>();

        private static PropertyInfo[] TFromProperties;
        private static PropertyInfo[] TToProperties;
        private static FieldInfo[] TFromFields;
        private static FieldInfo[] TToFields;

        public static void AddMap2(Type type_From, Type type_To) 
        {
            _maps.Add(new KeyValuePair<Type, Type>(type_From, type_To), null);
        }

        public static void AddMap<TFrom, TTo>(Action<TFrom, TTo> Map = null)
            where TFrom : class
            where TTo : class
        {
            _maps.Add(new KeyValuePair<Type, Type>(typeof(TFrom), typeof(TTo)), Map);
        }

        public static void Map2(object vFrom, object vTo)
        {
            ////var key = new KeyValuePair<Type, Type>(typeof(FromType), typeof(ToType));
            ////var map = (Action<FromType, ToType>)_maps[key];

            //var hasMapping = _maps.Any(x => x.Key.Equals(key));

            ////if (!hasMapping)
            ////    throw new Exception(
            ////        string.Format("No map defined for {0} => {1}",
            ////            typeof(FromType).Name, typeof(ToType).Name));

            var tFrom = vFrom.GetType();
            var tTo = vTo.GetType();

            TFromProperties = tFrom.GetProperties();
            TFromFields = tFrom.GetFields();
            TToProperties = tTo.GetProperties();
            TToFields = tTo.GetFields();

            SyncProperties(vFrom, vTo);
            SyncFields(vFrom, vTo);

            ////// Sync and mapped data, override anything that auto synced with mapping action.
            ////if (map != null)
            ////    map(vFrom, vTo);
        }

        public static void Map<FromType, ToType>(FromType From, ToType To)
        {
            var key = new KeyValuePair<Type, Type>(typeof(FromType), typeof(ToType));
            var map = (Action<FromType, ToType>)_maps[key];

            var hasMapping = _maps.Any(x => x.Key.Equals(key));

            if (!hasMapping)
                throw new Exception(
                    string.Format("No map defined for {0} => {1}",
                        typeof(FromType).Name, typeof(ToType).Name));

            var tFrom = typeof(FromType);
            var tTo = typeof(ToType);

            TFromProperties = tFrom.GetProperties();
            TFromFields = tFrom.GetFields();
            TToProperties = tTo.GetProperties();
            TToFields = tTo.GetFields();

            SyncProperties(From, To);
            SyncFields(From, To);

            // Sync and mapped data, override anything that auto synced with mapping action.
            if (map != null)
                map(From, To);
        }

        private static void SyncProperties<FromType, ToType>(FromType objFrom, ToType objTo)
        {
            var fromProperties = TFromProperties;
            var toProperties = TToProperties;
            var toFields = TToFields;

            if (fromProperties != null && fromProperties.Count() > 0)
            {
                foreach (var fromProperty in fromProperties)
                {
                    if (toProperties.Any(x => x.Name == fromProperty.Name))
                    {
                        var destinationProperty = toProperties.Where(x => x.Name == fromProperty.Name).FirstOrDefault();

                        if (MatchingProps(fromProperty, destinationProperty))
                        {
                            var val = fromProperty.GetValue(objFrom, null);
                            destinationProperty.SetValue(objTo, Convert.ChangeType(val, fromProperty.PropertyType), null);
                        }
                    }

                    if (toFields.Any(x => x.Name == fromProperty.Name))
                    {
                        var destinationField = toFields.Where(x => x.Name == fromProperty.Name).FirstOrDefault();

                        if (MatchingPropertyToField(fromProperty, destinationField))
                        {
                            var val = fromProperty.GetValue(objFrom, null);
                            destinationField.SetValue(objTo, val);
                        }
                    }
                }
            }
        }

        private static void SyncFields<FromType, ToType>(FromType objFrom, ToType objTo)
        {
            var fromFields = TFromFields;
            var toFields = TToFields;
            var toProperties = TToProperties;

            if (fromFields != null && fromFields.Count() > 0)
            {
                foreach (var fromField in fromFields)
                {
                    if (toFields.Any(x => x.Name == fromField.Name))
                    {
                        var destinationField = toFields.Where(x => x.Name == fromField.Name).FirstOrDefault();

                        if (MatchingFields(fromField, destinationField))
                        {
                            var val = fromField.GetValue(objFrom);
                            destinationField.SetValue(objTo, val);
                        }
                    }

                    if (toProperties.Any(x => x.Name == fromField.Name))
                    {
                        var destinationProperty = toProperties.Where(x => x.Name == fromField.Name).FirstOrDefault();

                        if (MatchingFieldToProperty(fromField, destinationProperty))
                        {
                            var val = fromField.GetValue(objFrom);
                            destinationProperty.SetValue(objTo, val, null);
                        }
                    }
                }
            }
        }

        // Rules...
        static Func<PropertyInfo, PropertyInfo, bool> MatchingProps = (T1, T2) => T1.Name == T2.Name && T1.PropertyType.Name == T2.PropertyType.Name;
        static Func<FieldInfo, FieldInfo, bool> MatchingFields = (T1, T2) => T1.Name == T2.Name && T1.FieldType.Name == T2.FieldType.Name;
        static Func<PropertyInfo, FieldInfo, bool> MatchingPropertyToField = (T1, T2) => T1.Name == T2.Name && T1.PropertyType.Name == T2.FieldType.Name;
        static Func<FieldInfo, PropertyInfo, bool> MatchingFieldToProperty = (T1, T2) => T1.Name == T2.Name && T1.FieldType.Name == T2.PropertyType.Name;

    }
}
