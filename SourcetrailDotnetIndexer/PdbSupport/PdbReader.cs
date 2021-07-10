using Microsoft.DiaSymReader;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;

namespace SourcetrailDotnetIndexer.PdbSupport
{
    /// <summary>
    /// Reads source locations from a program database (PDB)
    /// </summary>
    class PdbReader : IPdbReader
    {
        private ISymUnmanagedReader5 reader;
        private readonly Dictionary<int, PdbMethod> methodsByToken;

        /// <summary>
        /// The filename of the PDB
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PdbReader"/> for the specified file
        /// </summary>
        /// <param name="filename">Filename of the PDB</param>
        /// <exception cref="ArgumentNullException">filename is null or empty or whitespace</exception>
        /// <exception cref="FileNotFoundException">file does not exist</exception>
        public PdbReader(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename))
                throw new FileNotFoundException("File does not exist: ", filename);

            Filename = filename;

            using (var pdbStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                methodsByToken = new Dictionary<int, PdbMethod>();
                try
                {
                    CollectMethods(pdbStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occurred reading the debug-information from {0}\r\n{1} -> {2}",
                        filename, ex.GetType().Name, ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets the method identified by a metadata token
        /// </summary>
        /// <param name="methodToken"></param>
        /// <returns>The method represented by the metadata token or null if no method with the token could be found</returns>
        public PdbMethod GetMethod(int methodToken)
        {
            return methodsByToken.TryGetValue(methodToken, out var method) ? method : null;
        }

        private void CollectMethods(Stream pdbStream)
        {
            reader = SymUnmanagedReaderFactory.CreateReader<ISymUnmanagedReader5>(pdbStream, new DummyMetadataProvider());
            var docs = reader.GetDocuments();
            foreach (var doc in docs)
            {
                var language = doc.GetLanguage();
                // Note that we're lying here by specifying "cpp" as the language when it is C# in reality
                // (this is so we get some syntax-highlighting in Sourcetrail)
                var languageName = language == SymLanguageType.CSharp ? "cpp"
                    : language == SymLanguageType.Basic ? "basic" : "c";
                var methods = reader.GetMethodsInDocument(doc);
                foreach (var method in methods)
                {
                    var token = method.GetToken();
                    var pdbMethod = new PdbMethod(token, doc.GetName(), languageName);

                    var sequencePoints = method.GetSequencePoints();
                    foreach (var sp in sequencePoints)
                    {
                        if (sp.IsHidden)
                            continue;
                        pdbMethod.AddSequence(sp.Offset, sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn);
                    }

                    methodsByToken[token] = pdbMethod;
                }
            }
        }
    }
}
