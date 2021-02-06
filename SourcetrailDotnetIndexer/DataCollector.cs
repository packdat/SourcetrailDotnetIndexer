using CoatiSoftware.SourcetrailDB;
using System;
using System.Collections.Generic;

namespace SourcetrailDotnetIndexer
{
    /// <summary>
    /// Responsible for storing data in the sourcetrail-db
    /// </summary>
    internal class DataCollector : IDisposable
    {
        // names of symbols (types, methods, etc.) with their symbolId
        private readonly Dictionary<string, int> collectedSymbols = new Dictionary<string, int>();

        public DataCollector(string outputFileName)
        {
            sourcetraildb.open(outputFileName);
            sourcetraildb.clear();
            sourcetraildb.beginTransaction();
        }

        public void Dispose()
        {
            sourcetraildb.commitTransaction();
            //sourcetraildb.optimizeDatabaseMemory();
            sourcetraildb.close();
        }

        public int CollectSymbol(string fullName, SymbolKind kind, string prefix = "", string postfix = "")
        {
            // caching the collected symbols drastically reduces execution time
            var identifier = prefix + fullName + postfix;
            if (collectedSymbols.TryGetValue(identifier, out int symbolId))
                return symbolId;

            symbolId = sourcetraildb.recordSymbol(NameHelper.SerializeName(fullName, prefix, postfix));
            collectedSymbols[identifier] = symbolId;
            if (symbolId <= 0)
            {
                var err = sourcetraildb.getLastError();
                throw new InvalidOperationException("Sourcetrail DB error: " + err);
            }
            sourcetraildb.recordSymbolDefinitionKind(symbolId, DefinitionKind.DEFINITION_EXPLICIT);
            sourcetraildb.recordSymbolKind(symbolId, kind);
            return symbolId;
        }

        public int CollectReference(int soureSymbolId, int referenceSymbolId, ReferenceKind referenceKind)
        {
            return sourcetraildb.recordReference(soureSymbolId, referenceSymbolId, referenceKind);
        }
    }
}
