using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Daibitx.Extension.Modularize.EventBus
{
    public static class EventMsgMapper
    {
        private static readonly ConcurrentDictionary<(Type Source, Type Target), Action<object, object>>
      _mapperCache = new();

        private static readonly ConcurrentDictionary<(Type Source, Type Target), Delegate>
            _converterCache = new();

        public static void RegisterConverter<TSource, TTarget>(Func<TSource, TTarget> converter)
        {
            _converterCache[(typeof(TSource), typeof(TTarget))] = converter;
        }

        public static TTarget Map<TTarget>(object source) where TTarget : new()
        {
            if (source == null)
                return default!;

            var target = new TTarget();
            Map(source, target);
            return target;
        }

        public static void Map(object source, object target)
        {
            if (source == null || target == null) return;

            var key = (source.GetType(), target.GetType());

            var mapper = _mapperCache.GetOrAdd(key, k => CreateMapper(k.Source, k.Target));
            mapper(source, target);
        }

        private static Action<object, object> CreateMapper(Type sourceType, Type targetType)
        {
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var mappers = new List<Action<object, object>>();

            foreach (var tProp in targetProps)
            {
                if (!tProp.CanWrite) continue;

                var sProp = sourceProps.FirstOrDefault(p => p.Name == tProp.Name && p.CanRead);
                if (sProp == null) continue;

                mappers.Add(CreatePropertyMapper(sProp, tProp));
            }

            return (src, dst) =>
            {
                foreach (var m in mappers)
                    m(src, dst);
            };
        }

        private static Action<object, object> CreatePropertyMapper(PropertyInfo sProp, PropertyInfo tProp)
        {
            var sType = sProp.PropertyType;
            var tType = tProp.PropertyType;

            // 如果是简单类型，包含强制转换
            if (IsSimple(sType) && IsSimple(tType))
            {
                return (src, dst) =>
                {
                    var val = sProp.GetValue(src);
                    tProp.SetValue(dst, ConvertSimple(val, tType));
                };
            }

            // 数组
            if (sType.IsArray && tType.IsArray)
                return CreateArrayMapper(sProp, tProp);

            // 字典
            if (IsDictionary(sType) && IsDictionary(tType))
                return CreateDictionaryMapper(sProp, tProp);

            // 集合（List、HashSet…）
            if (IsEnumerable(sType) && IsEnumerable(tType))
                return CreateEnumerableMapper(sProp, tProp);

            // 复杂类型递归映射
            return (src, dst) =>
            {
                var sVal = sProp.GetValue(src);
                if (sVal == null)
                {
                    tProp.SetValue(dst, null);
                    return;
                }

                var tVal = Activator.CreateInstance(tType);
                Map(sVal, tVal);
                tProp.SetValue(dst, tVal);
            };
        }

        private static Action<object, object> CreateArrayMapper(PropertyInfo sProp, PropertyInfo tProp)
        {
            var sElem = sProp.PropertyType.GetElementType();
            var tElem = tProp.PropertyType.GetElementType();

            return (src, dst) =>
            {
                var arr = (Array)sProp.GetValue(src);
                if (arr == null)
                {
                    tProp.SetValue(dst, null);
                    return;
                }

                var newArr = Array.CreateInstance(tElem, arr.Length);

                for (int i = 0; i < arr.Length; i++)
                {
                    newArr.SetValue(MapCollectionElement(arr.GetValue(i), sElem, tElem), i);
                }

                tProp.SetValue(dst, newArr);
            };
        }

        private static Action<object, object> CreateDictionaryMapper(PropertyInfo sProp, PropertyInfo tProp)
        {
            var sArgs = sProp.PropertyType.GetGenericArguments();
            var tArgs = tProp.PropertyType.GetGenericArguments();

            return (src, dst) =>
            {
                var dict = (IDictionary)sProp.GetValue(src);
                if (dict == null)
                {
                    tProp.SetValue(dst, null);
                    return;
                }

                var newDict = (IDictionary)Activator.CreateInstance(tProp.PropertyType);

                foreach (var key in dict.Keys)
                {
                    var newKey = MapCollectionElement(key, sArgs[0], tArgs[0]);
                    var newVal = MapCollectionElement(dict[key], sArgs[1], tArgs[1]);
                    newDict[newKey] = newVal;
                }

                tProp.SetValue(dst, newDict);
            };
        }

        private static Action<object, object> CreateEnumerableMapper(PropertyInfo sProp, PropertyInfo tProp)
        {
            var sElem = sProp.PropertyType.GetGenericArguments().First();
            var tElem = tProp.PropertyType.GetGenericArguments().First();

            return (src, dst) =>
            {
                var list = (IEnumerable)sProp.GetValue(src);
                if (list == null)
                {
                    tProp.SetValue(dst, null);
                    return;
                }

                var newList = (IList)Activator.CreateInstance(tProp.PropertyType);

                foreach (var item in list)
                {
                    newList.Add(MapCollectionElement(item, sElem, tElem));
                }

                tProp.SetValue(dst, newList);
            };
        }

        private static object MapCollectionElement(object src, Type sType, Type tType)
        {
            if (src == null) return null;

            if (IsSimple(sType) && IsSimple(tType))
                return ConvertSimple(src, tType);

            var target = Activator.CreateInstance(tType);
            Map(src, target);
            return target;
        }

        private static bool IsSimple(Type type) =>
            type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(Guid);

        private static bool IsDictionary(Type type) =>
            type.IsGenericType &&
            typeof(IDictionary).IsAssignableFrom(type);

        private static bool IsEnumerable(Type type) =>
            typeof(IEnumerable).IsAssignableFrom(type) &&
            type != typeof(string);

        private static object ConvertSimple(object value, Type targetType)
        {
            if (value == null) return null;

            var sourceType = value.GetType();

            // 1. 已注册的自定义转换器
            if (_converterCache.TryGetValue((sourceType, targetType), out var converter))
                return converter.DynamicInvoke(value);

            if (sourceType.IsEnum && targetType.IsEnum)
            {
                var intValue = (int)Convert.ChangeType(value, typeof(int));
                return Enum.ToObject(targetType, intValue);
            }
            // 2. enum→string / string→enum
            if (targetType.IsEnum && sourceType == typeof(string))
                return Enum.Parse(targetType, value.ToString());

            if (sourceType.IsEnum && targetType == typeof(string))
                return value.ToString();

            // 3. 其他基础类型转换
            return Convert.ChangeType(value, targetType);
        }
    }
}
