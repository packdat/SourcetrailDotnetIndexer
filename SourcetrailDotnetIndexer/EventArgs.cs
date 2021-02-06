using CoatiSoftware.SourcetrailDB;
using System;

namespace SourcetrailDotnetIndexer
{
    class CollectedMethodEventArgs : EventArgs
    {
        public CollectedMethod CollectedMethod { get; }
        public CollectedMethodEventArgs(CollectedMethod method)
        {
            CollectedMethod = method;
        }
    }

    class SymbolEventArgs : EventArgs
    {
        public string Name { get; }
        public SymbolKind Kind { get; }
        public SymbolEventArgs(string name, SymbolKind kind)
        {
            Name = name;
            Kind = kind;
        }
    }

    class ReferenceEventArgs : EventArgs
    {
        public int SourceSymbolId { get; }
        public int ReferenceSymbolId { get; }
        public ReferenceKind ReferenceKind { get; }
        public ReferenceEventArgs(int sourceId, int referenceId, ReferenceKind referenceKind)
        {
            SourceSymbolId = sourceId;
            ReferenceSymbolId = referenceId;
            ReferenceKind = referenceKind;
        }
    }
}
