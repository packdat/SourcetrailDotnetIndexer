using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SourcetrailDotnetIndexer
{
    delegate Assembly AssemblyLoadDelegate(string path);

    partial class Program
    {
        static string startAssembly;
        static string[] assemblySearchPaths;
        static string[] nameFilters;
        static string[] namespacesToFollow;
        static string outputPath;
        static string outputPathAndFilename;
        static bool waitAtEnd;
        // we use Assembly.ReflectionOnlyLoadFrom for the DotnetIndexer
        // and Assembly.LoadFrom for the DotnetcoreIndexer
        static AssemblyLoadDelegate assemblyLoader;

        static void Usage()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine("SourcetrailDotnetIndexer v{0}.{1}", versionInfo.FileMajorPart, versionInfo.FileMinorPart);
            Console.WriteLine("Arguments:");
            Console.WriteLine(" -i  InputAssembly");
            Console.WriteLine("     Specifies the full path of the assembly to index");
            Console.WriteLine(" -o  OutputPath");
            Console.WriteLine("     Specifies the name of the folder, where the output will be generated");
            Console.WriteLine("     The output filename is always the input filename with the extension .srctrldb");
            Console.WriteLine(" -of OutputFilename");
            Console.WriteLine("     Full path and filename of the generated database");
            Console.WriteLine("     If both -o and -of are specified, -of takes precedence");
            Console.WriteLine(" -s  SearchPath");
            Console.WriteLine("     Specifies a folder, where additional assemblies are located");
            Console.WriteLine("     This switch can be used multiple times");
            Console.WriteLine(" -f  Namespace Filter");
            Console.WriteLine("     Specifies a regex that is used to exclude types from matching namespaces");
            Console.WriteLine("     This switch can be used multiple times");
            Console.WriteLine(" -fn Namespace Filter");
            Console.WriteLine("     Specifies a regex that specifies namespaces that are allowed to be followed");
            Console.WriteLine("     (by default, only types from the InputAssembly are collected");
            Console.WriteLine("     This is basically the opposite of -f");
            Console.WriteLine("     This switch can be used multiple times");
            Console.WriteLine(" -ff File Path");
            Console.WriteLine("     Specifies the path to a text-file with namespaces that are allowed to be followed");
            Console.WriteLine("     This may be more convenient than specifying multiple namespaces with the -fn switch");
            Console.WriteLine("     Every line in the specified file contains a regex matching one or more namespaces");
            Console.WriteLine(" -w");
            Console.WriteLine("     If specified, waits for the user to press enter before exiting");
            Console.WriteLine("     Intended when running from inside VS to keep the console-window open");
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var asmName = new AssemblyName(args.Name);
            var path = Path.GetDirectoryName(startAssembly);
            var asmPath = Path.Combine(path, asmName.Name + ".dll");
            if (File.Exists(asmPath))
            {
                Console.WriteLine("Load: {0}", asmPath);
                var asm = assemblyLoader(asmPath);
                return asm;
            }
            foreach (var basePath in assemblySearchPaths)
            {
                asmPath = Path.Combine(basePath, asmName.Name + ".dll");
                if (File.Exists(asmPath))
                {
                    Console.WriteLine("Load: {0}", asmPath);
                    var asm = assemblyLoader(asmPath);
                    return asm;
                }
            }
            Console.WriteLine("Unable to resolve assembly {0}.", args.Name);
            Console.WriteLine("Hint: Specify additional assembly-locations using the -s switch.");
            return null;
        }

        private static bool ProcessCommandLine(string[] args)
        {
            var searchPaths = new List<string>();
            var filters = new List<string>();
            var followFilters = new List<string>();
            var i = 0;
            while (i < args.Length)
            {
                var arg = args[i];
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                    arg = arg.Substring(1);
                switch (arg.ToLowerInvariant())
                {
                    case "i":   // input assembly
                        i++;
                        if (i < args.Length)
                            startAssembly = args[i];
                        else
                            return false;
                        break;
                    case "s":   // search paths (for dependent assemblies)
                        i++;
                        if (i < args.Length)
                            searchPaths.Add(args[i]);
                        else
                            return false;
                        break;
                    case "f":   // name filters
                        i++;
                        if (i < args.Length)
                            filters.Add(args[i]);
                        else
                            return false;
                        break;
                    case "fn":   // namespaces to follow
                        i++;
                        if (i < args.Length)
                            followFilters.Add(args[i]);
                        else
                            return false;
                        break;
                    case "ff":   // path to a file with namespaces to follow
                        i++;
                        if (i < args.Length)
                        {
                            if (!File.Exists(args[i]))
                            {
                                Console.WriteLine("The file '{0}' does not exist", args[i]);
                                return false;
                            }
                            followFilters.AddRange(ReadTextFile(args[i]));
                        }
                        else
                            return false;
                        break;
                    case "o":   // output path
                        i++;
                        if (i < args.Length)
                            outputPath = args[i];
                        else
                            return false;
                        break;
                    case "of":   // output path and filename
                        i++;
                        if (i < args.Length)
                            outputPathAndFilename = args[i];
                        else
                            return false;
                        break;
                    case "w":
                        waitAtEnd = true;
                        break;
                    default:
                        Console.WriteLine("Unrecognized argument: {0}", arg);
                        break;
                }
                i++;
            }
            assemblySearchPaths = searchPaths.ToArray();
            nameFilters = filters.Count > 0 ? filters.ToArray() : null;
            namespacesToFollow = followFilters.Count > 0 ? followFilters.ToArray() : null;

            return !string.IsNullOrWhiteSpace(startAssembly) 
                && (!string.IsNullOrWhiteSpace(outputPath) || !string.IsNullOrWhiteSpace(outputPathAndFilename));
        }

        protected static IEnumerable<string> ReadTextFile(string filePath)
        {
            var encoding = new UTF8Encoding(false, true);
            return File.ReadAllLines(filePath, encoding)
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"));
        }
    }
}
