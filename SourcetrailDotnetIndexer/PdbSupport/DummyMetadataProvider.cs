using Microsoft.DiaSymReader;
using System;
using System.Reflection;

namespace SourcetrailDotnetIndexer.PdbSupport
{
    /// <summary>
    /// Placeholder instance
    /// <para></para>
    /// the unmanaged API requires an instance of the <see cref="ISymReaderMetadataProvider"/> interface, 
    /// see <see cref="PdbReader.CollectMethods(System.IO.Stream)"/>
    /// </summary>
    class DummyMetadataProvider : ISymReaderMetadataProvider
    {
        public unsafe bool TryGetStandaloneSignature(int standaloneSignatureToken, out byte* signature, out int length)
        {
            throw new NotImplementedException();
        }

        public bool TryGetTypeDefinitionInfo(int typeDefinitionToken, out string namespaceName, out string typeName, out TypeAttributes attributes)
        {
            throw new NotImplementedException();
        }

        public bool TryGetTypeReferenceInfo(int typeReferenceToken, out string namespaceName, out string typeName)
        {
            throw new NotImplementedException();
        }
    }
}
