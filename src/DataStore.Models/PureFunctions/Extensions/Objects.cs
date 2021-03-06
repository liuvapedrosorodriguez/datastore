﻿namespace DataStore.Models.PureFunctions.Extensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public static class Objects
    {
        private static readonly JsonSerializerSettings DeSerialisationSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto //for things with $type use that type when deserializing
        };

        private static readonly JsonSerializerSettings SerialisationSettings = new JsonSerializerSettings
        {
            //apply $type to all objects where the class of the instance being stored != the class/interface of the property it is stored under
            TypeNameHandling = TypeNameHandling.Auto 
        };

        private static readonly char[] SystemTypeChars =
        {
            '<',
            '>',
            '+'
        };

        /// <summary>
        ///     a simpler cast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class
        {
            return obj as T;
        }

        public static JToken AsJson(this object value)
        {
            JToken json = null;
            if (value != null)
            {
                json = JToken.FromObject(
                    value,
                    new JsonSerializer
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }

            return json;
        }

        public static T AsJson<T>(this object value) where T : JToken
        {
            return (T)AsJson(value);
        }

        public static T Cast<T>(this object o)
        {
            return (T)o;
        }

        public static T Clone<T>(this T source)
        {
            return source.ToJsonString().FromJsonString<T>();
        }

        public static T Clone<T>(this object source)
        {
            return source.ToJsonString().FromJsonString<T>();
        }

        /// <summary>
        ///     copies the values of matching properties from one object to another regardless of type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="exclude"></param>
        public static void CopyProperties(this object source, object destination, params string[] exclude)
        {
            // If any this null throw an exception
            if (source == null || destination == null) throw new Exception("Source or/and Destination Objects are null");

            // Getting the Types of the objects
            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead && targetProperty != null && targetProperty.GetSetMethod(true) != null
                                && !targetProperty.GetSetMethod(true).IsPrivate && (targetProperty.GetSetMethod(true).Attributes & MethodAttributes.Static) == 0
                                && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType) && !exclude.Contains(targetProperty.Name)
                          select new
                          {
                              sourceProperty = srcProp,
                              targetProperty
                          };

            // map the properties
            foreach (var props in results) props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
        }

        public static T FromJsonString<T>(this string source)
        {
            return source == null ? default(T) : JsonConvert.DeserializeObject<T>(source, DeSerialisationSettings);
        }

        /// <summary>
        ///     get property name from current instance
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(this TObject type, Expression<Func<TObject, object>> propertyRefExpr)
        {
            // usage: obj.GetPropertyName(o => o.Member)
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        /// <summary>
        ///     get property name from any class
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
            // usage: Objects.GetPropertyName<SomeClass>(sc => sc.Member)
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        /// <summary>
        ///     get static property name from any class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetStaticPropertyName<TResult>(Expression<Func<TResult>> expression)
        {
            // usage: Objects.GetStaticPropertyName(t => t.StaticProperty)
            return GetPropertyNameCore(expression);
        }
        
        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsSystemType(this Type type)
        {
            return type.Namespace == null || type.Namespace.StartsWith("System") || type.Name.IndexOfAny(SystemTypeChars) >= 0;
        }

        /// <summary>
        ///     perform an operation on any class inline, (e.g. new Object().Op(o => someoperationon(o));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static T Op<T>(this T obj, Action<T> op)
        {
            op(obj);
            return obj;
        }


        public static string ToJsonString(this object source, Formatting formatting = Formatting.None)
        {
            return source == null ? null : JsonConvert.SerializeObject(source, formatting, SerialisationSettings);
        }

        private static string GetPropertyNameCore(Expression propertyRefExpr)
        {
            if (propertyRefExpr == null) throw new ArgumentNullException(nameof(propertyRefExpr), "propertyRefExpr is null.");

            var memberExpr = propertyRefExpr as MemberExpression;
            if (memberExpr == null)
            {
                var unaryExpr = propertyRefExpr as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.", nameof(propertyRefExpr));
        }

       
    }
}