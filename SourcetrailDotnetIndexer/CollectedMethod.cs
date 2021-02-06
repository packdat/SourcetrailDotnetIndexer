using System.Reflection;

namespace SourcetrailDotnetIndexer
{
    class CollectedMethod
    {
        public MethodBase Method { get; set; }
        public int ClassId { get; set; }
        public int MethodId { get; set; }
        public CollectedMethod(MethodBase method, int methodId, int classId)
        {
            Method = method;
            MethodId = methodId;
            ClassId = classId;
        }
    }
}
