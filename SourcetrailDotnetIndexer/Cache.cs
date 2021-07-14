using System;
using System.Collections.Generic;
using System.Reflection;

namespace SourcetrailDotnetIndexer
{
    /// <summary>
    /// Stores all data collected while indexing
    /// </summary>
    public static class Cache
    {
        // the collected namespaces
        private static readonly Dictionary<string, int> namespaces = new Dictionary<string, int>();
        // used to speed things up
        // maps an interface type to the classes that implement that interface
        private static readonly Dictionary<Type, List<Type>> interfaceImplementations = new Dictionary<Type, List<Type>>();
        // the types that are already collected with their symbolId
        private static readonly Dictionary<Type, int> collectedTypes = new Dictionary<Type, int>();
        // the assemblies that we have followed
        private static readonly Dictionary<Assembly, int> collectedAssemblies = new Dictionary<Assembly, int>();

        public static IDictionary<string, int> Namespaces
        {
            get { return namespaces; }
        }

        public static IDictionary<Type, List<Type>> InterfaceImplementations
        {
            get { return interfaceImplementations; }
        }

        public static IDictionary<Type, int> CollectedTypes
        {
            get { return collectedTypes; }
        }

        public static IDictionary<Assembly, int> CollectedAssemblies
        {
            get { return collectedAssemblies; }
        }
    }
}
