using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SourcetrailDotnetIndexer
{
    /// <summary>
    /// Used to filter out references to namespaces we're not interested in (e.g. System)
    /// </summary>
    class NamespaceFilter
    {
        private readonly List<string> filterPatterns;

        /// <summary>
        /// Constrcuts a new <see cref="NamespaceFilter"/> with the specified regex-patterns
        /// </summary>
        /// <param name="patterns"></param>
        public NamespaceFilter(IEnumerable<string> patterns)
        {
            filterPatterns = new List<string>(patterns ?? Array.Empty<string>());
        }

        /// <summary>
        /// Determines, whether the specified name matches any of the regex-patterns defined for this instance
        /// </summary>
        /// <param name="name">The name to check</param>
        /// <returns>true, if none of the patterns matches <b><paramref name="name"/></b> otherwise false</returns>
        public bool IsValid(string name)
        {
            if (name is null)
                return false;

            foreach (var pattern in filterPatterns)
            {
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified name matches any of the regex-patterns defined for this instance<br/>
        /// This is the opposite of <see cref="IsValid(string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns>false, if none of the patterns matches <b><paramref name="name"/></b> otherwise true</returns>
        public bool Matches(string name)
        {
            return !IsValid(name);
        }
    }
}
