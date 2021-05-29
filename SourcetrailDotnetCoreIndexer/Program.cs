using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SourcetrailDotnetIndexer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (!ProcessCommandLine(args))
            {
                Usage();
                Environment.ExitCode = 1;
                return;
            }
            if (!File.Exists(startAssembly))
            {
                Console.WriteLine("Assembly no found: {0}", startAssembly);
                Environment.ExitCode = 1;
                return;
            }
            try
            {
                // outputPathAndFilename takes precedence if specified
                if (!string.IsNullOrWhiteSpace(outputPathAndFilename))
                    outputPath = Path.GetDirectoryName(outputPathAndFilename);

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                var nameFilter = new NamespaceFilter(nameFilters);

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                assemblyLoader = Assembly.LoadFrom;

                var outFileName = string.IsNullOrWhiteSpace(outputPathAndFilename)
                    ? Path.ChangeExtension(Path.GetFileName(startAssembly), ".srctrldb")
                    : Path.GetFileName(outputPathAndFilename);

                Console.WriteLine("Indexing assembly {0}{1}", startAssembly, Environment.NewLine);
                var sw = Stopwatch.StartNew();

                // note that we can't use MetadataLoadContext, because that does not support resolving metadata-tokens found in IL code
                // and we also can't use ReflectionOnlyLoadFrom, as that is not supported in .net core and .net 5+
                var assembly = Assembly.LoadFrom(startAssembly);

                var indexer = new SourcetrailDotnetIndexer(assembly, nameFilter);

                indexer.Index(Path.Combine(outputPath, outFileName));

                sw.Stop();

                Console.WriteLine("{0}Sourcetrail database has been generated at {1}",
                    Environment.NewLine, Path.Combine(outputPath, outFileName));
                Console.WriteLine("Time taken: {0}", sw.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}An exception occurred:{0}{1}", Environment.NewLine, ex);
                Environment.ExitCode = 2;
            }

            // only useful if running from within VisualStudio
            if (waitAtEnd)
            {
                Console.WriteLine("{0}{0}Press Enter to exit", Environment.NewLine);
                Console.ReadLine();
            }
        }
    }
}
