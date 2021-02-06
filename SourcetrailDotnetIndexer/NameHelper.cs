using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourcetrailDotnetIndexer
{
    /// <summary>
    /// Contains static method which aid in building names for types and type-members
    /// </summary>
    static class NameHelper
    {
        private static readonly Dictionary<Type, string> nameMap = new Dictionary<Type, string>
        {
            { typeof(void), "void" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(string), "string" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(DateTime), "DateTime" }
        };

        /// <summary>
        /// Returns the shortened name of some well-known types.
        /// <para></para>Returns the "prettified" full name for all other types.
        /// <param name="type">The type for which to get the name</param>
        /// <param name="nameOnly">If true, returns only the name of the type itself (without namespace)</param>
        /// </summary>
        public static string TranslateTypeName(Type type, bool nameOnly = false)
        {
            if (nameMap.TryGetValue(type, out string name))
                return name;
            return MakePrettyName(type, nameOnly);
        }

        /// <summary>
        /// Attempts to "prettify" type-names
        /// <param name="type">The type for which to get the name</param>
        /// <param name="nameOnly">If true, returns only the name of the type itself (without namespace)</param>
        /// </summary>
        public static string MakePrettyName(Type type, bool nameOnly = false)
        {
            var genericArguments = type.GetGenericArguments();
            var typeDefeninition = type.IsGenericParameter || nameOnly
                ? type.Name
                : type.Namespace + "." + (type.DeclaringType != null ? type.DeclaringType.Name + "." : "") + type.Name;
            if (genericArguments.Length == 0 || typeDefeninition.IndexOf("`") < 0)
            {
                return typeDefeninition;
            }
            var unmangledName = typeDefeninition.Substring(0, typeDefeninition.IndexOf("`", StringComparison.Ordinal));
            // for the generics, we use the short names. TODO: make configurable
            return unmangledName + "<" + string.Join(", ", genericArguments.Select(a => TranslateTypeName(a, true))) + ">";
        }

        /// <summary>
        /// Converts a type-name into a JSON representation as expected by SourcetrailDB
        /// </summary>
        /// <param name="fullName">Name of type, should be the "pretty" name</param>
        /// <param name="prefix">optional prefix (e.g. return type of a method)</param>
        /// <param name="postfix">optional suffix (e.g. method-parameters)</param>
        /// <returns></returns>
        public static string SerializeName(string fullName, string prefix = "", string postfix = "")
        {
            /*
             * expected format:
             * {
             *   "name_delimiter" : "."
             *   "name_elements" : [
             *     {
             *       "prefix" : "",
             *       "name" : "",
             *       "postfix" : ""
             *     },
             *     ...
             *   ]
             * }
             */
            fullName = fullName.Replace('+', '.');          // account for nested types
            var parts = new List<string>();
            int genStartIndex, genEndIndex;
            if ((genStartIndex = fullName.IndexOf('<')) > 0
                && ((genEndIndex = fullName.LastIndexOf('>')) > genStartIndex))     // generic type ?
            {
                var p1 = fullName.Substring(0, genStartIndex);
                var p2 = fullName.Substring(genStartIndex, genEndIndex - genStartIndex + 1);      // treat generics part as single string
                // use different delimiters for the generics part, sourcetrail would otherwise treat these as nested types
                p1 += p2.Replace('.', ':') + fullName.Substring(genEndIndex + 1);
                parts.AddRange(p1.Split('.'));
            }
            else
                parts.AddRange(fullName.Split('.'));
            var pre = "";
            var post = "";
            var sb = new StringBuilder("{ \"name_delimiter\" : \".\", \"name_elements\" : [ ");
            for (var i = 0; i < parts.Count; i++)
            {
                sb.AppendFormat("{{ \"prefix\" : \"{0}\", \"name\" : \"{1}\", \"postfix\" : \"{2}\" }}", pre, parts[i], post);
                if (i + 1 < parts.Count)
                {
                    sb.AppendLine(",");
                }
                if (i + 2 == parts.Count)
                {
                    // apply pre-/post-fix to last element
                    pre = prefix;
                    post = postfix;
                }
            }
            sb.Append("] }");
            return sb.ToString();
        }
    }
}
