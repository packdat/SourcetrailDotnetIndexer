using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SourcetrailDotnetIndexer.PdbSupport
{
    /// <summary>
    /// Helper class to locate PDB files for .NET assemblies
    /// </summary>
    class PdbLocator
    {
        // TODO: supply user-defined search-paths

        private readonly Dictionary<Assembly, IPdbReader> pdbReaders = new Dictionary<Assembly, IPdbReader>();

        /// <summary>
        /// Registers the specified assembly and attempts to locate and load the PDB for it
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> for which the PDB should be loaded</param>
        public void AddAssembly(Assembly assembly)
        {
            try
            {
                var pdbFilename = Path.ChangeExtension(assembly.Location, ".pdb");
                // legacy .net uses the "old" pdb-format while .net core uses the new "portable pdb" format
                // select the correct implementation based on the project that is being build
                if (File.Exists(pdbFilename))
                {
#if NETCORE
                    var reader = new SourcetrailDotnetCoreIndexer.PdbSupport.PortablePdbReader(pdbFilename);
#else
                    var reader = new PdbReader(pdbFilename);
#endif
                    pdbReaders[assembly] = reader;
                    Console.WriteLine("Loaded: {0}", reader.Filename);
                }
                else
                    Console.WriteLine("PDB: {0} does not exist", pdbFilename);
            }
            catch (ArgumentException)
            { }
        }

        /// <summary>
        /// Gets a <see cref="PdbReader"/> for the specified assembly
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/>for which a <see cref="PdbReader"/> should be retrieved</param>
        /// <returns>a <see cref="PdbReader"/> for the assembly or null if the assembly was not previously registered</returns>
        public IPdbReader GetPdbReaderForAssembly(Assembly assembly)
        {
            return pdbReaders.TryGetValue(assembly, out var reader) ? reader : null;
        }
    }
}
