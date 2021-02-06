using System;
using System.Reflection;

namespace SourcetrailDotnetIndexer
{
    /// <summary>
    /// Contains information about a method stored in a sourcetrail database
    /// </summary>
    class CollectedMethod
    {
        /// <summary>
        /// The method definition itself
        /// </summary>
        public MethodBase Method { get; set; }

        /// <summary>
        /// The symbold-id of the class, the method is a member of
        /// </summary>
        public int ClassId { get; set; }

        /// <summary>
        /// The symbold-id of the method
        /// </summary>
        public int MethodId { get; set; }

        public CollectedMethod(MethodBase method, int methodId, int classId)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            if (methodId <= 0 || classId <= 0)
                throw new ArgumentException("Method-id and Class-id must be greater than zero");

            MethodId = methodId;
            ClassId = classId;
        }
    }
}
