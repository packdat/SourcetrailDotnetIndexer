using SourcetrailDotnetIndexer.PdbSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace SourcetrailDotnetCoreIndexer.PdbSupport
{
    /// <summary>
    /// Reads source locations from a program database (PDB) in portable-pdb format
    /// </summary>
    class PortablePdbReader : IPdbReader
    {
        private readonly Dictionary<int, PdbMethod> methodsByToken;

        /// <summary>
        /// The filename of the PDB
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PortablePdbReader"/> for the specified file
        /// </summary>
        /// <param name="filename">Filename of the PDB</param>
        /// <exception cref="ArgumentNullException">filename is null or empty or whitespace</exception>
        /// <exception cref="FileNotFoundException">file does not exist</exception>
        public PortablePdbReader(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename))
                throw new FileNotFoundException("File does not exist: ", filename);

            Filename = filename;

            using var pdbStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
            var reader = provider.GetMetadataReader();

            methodsByToken = new Dictionary<int, PdbMethod>();
            try
            {
                CollectMethods(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred reading the debug-information from {0}\r\n{1}",
                    filename, ex);
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

        private void CollectMethods(MetadataReader reader)
        {
            foreach (var mh in reader.MethodDebugInformation)
            {
                if (mh.IsNil)
                    continue;
                var mdbg = reader.GetMethodDebugInformation(mh);
                if (mdbg.Document.IsNil)
                    continue;
                // TODO: maybe we should add more error-checking here...
                var doc = reader.GetDocument(mdbg.Document);
                var mdh = mh.ToDefinitionHandle();
                var token = reader.GetToken(mdh);
                var language = reader.GetGuid(doc.Language);
                // Note that we're lying here by specifying "cpp" as the language when it is C# in reality
                // (this is so we get some syntax-highlighting in Sourcetrail)
                var languageName = language == SymLanguageType.CSharp ? "cpp"
                    : language == SymLanguageType.Basic ? "basic" : "c";
                var method = new PdbMethod(token, reader.GetString(doc.Name), languageName);
                foreach (var sph in mdbg.GetSequencePoints())
                {
                    if (sph.IsHidden)
                        continue;
                    method.AddSequence(sph.Offset, sph.StartLine, sph.StartColumn, sph.EndLine, sph.EndColumn);
                }
                methodsByToken[token] = method;
            }
        }
    }
}
