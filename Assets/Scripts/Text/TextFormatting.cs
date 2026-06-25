using System;
using System.Linq;
using System.Reflection;

namespace Toreole.Turnbased.Text
{
    public static class TextFormatting
    {
        public static string Format(object obj, string format)
        {
            return FormatInternal(obj, format.TrimStart(':'), obj.GetType());
        }

        // this kind of sucks
        // why dont we just use 
        // where T : IFormattable
        public static string Format<T>(T obj, string format)
        {
            return FormatInternal(obj, format, typeof(T));
        }

        private static string FormatInternal(object obj, string format, Type type)
        {
            if (string.IsNullOrEmpty(format))
                return obj.ToString();
            var toStringWithStringParam = type.GetRuntimeMethods().Where(it => it.Name.Equals("ToString"))
                .Where(it => it.GetParameters().Length == 1
                          && it.GetParameters().First().ParameterType.Equals(typeof(string))
                ).FirstOrDefault();
            if (toStringWithStringParam != null)
            {
                return (string)toStringWithStringParam.Invoke(obj, new object[] { format });
            }
            //UnityEngine.Debug.LogWarning($"Attempted to format object of type {type.Name} " +
            //    $"with format {format}, but no such ToString overload exists");
            return obj.ToString();
        }

    }
}
