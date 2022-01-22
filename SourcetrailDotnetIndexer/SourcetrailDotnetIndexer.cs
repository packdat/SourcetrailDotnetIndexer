using SourcetrailDotnetIndexer.PdbSupport;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SourcetrailDotnetIndexer
{
    partial class SourcetrailDotnetIndexer
    {
        private readonly Assembly[] assemblies;
        private readonly NamespaceFilter nameFilter;
        private readonly NamespaceFilter namespaceFollowFilter;

        // list of methods that we have to analyze after collecting all types
        private readonly List<CollectedMethod> collectedMethods = new List<CollectedMethod>();

        public SourcetrailDotnetIndexer(Assembly[] assemblies, NamespaceFilter nameFilter, NamespaceFilter namespaceFollowFilter)
        {
            this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            this.nameFilter = nameFilter ?? throw new ArgumentNullException(nameof(nameFilter));
            this.namespaceFollowFilter = namespaceFollowFilter ?? throw new ArgumentNullException(nameof(namespaceFollowFilter));
        }

        public void Index(string outputFileName)
        {
            // create the Sourcetrail data collector
            var dataCollector = new DataCollector(outputFileName);

            var pdbLocator = new PdbLocator();
            // set up the type handler
            var typeHandler = new TypeHandler(assemblies, nameFilter, namespaceFollowFilter, dataCollector, pdbLocator);
            typeHandler.MethodCollected += (sender, args) => collectedMethods.Add(args.CollectedMethod);

            foreach (var assembly in assemblies)
            {
                Console.WriteLine("Indexing assembly {0}{1}", assembly.Location, Environment.NewLine);

                pdbLocator.AddAssembly(assembly);
                try
                {
                    Console.WriteLine("Collecting types...");
                    // collect all types first
                    foreach (var type in assembly.GetTypes())
                    {
                        typeHandler.AddToDbIfValid(type);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception indexing assemby '{0}'\r\n{1}", assembly.Location, ex);
                }
            }

            Console.WriteLine("{1}Collected {0} types{1}", Cache.CollectedTypes.Count, Environment.NewLine);

            if (typeHandler.SkippedGlobalTypeCount > 0)
            {
                Console.WriteLine("{0} global type(s) were skipped during indexing", typeHandler.SkippedGlobalTypeCount);
                Console.WriteLine("If you want to include these types, specify the -ag switch");
                Console.WriteLine();
            }

            // set up the visitor for parsed methods
            var referenceVisitor = new MethodReferenceVisitor(typeHandler, dataCollector, pdbLocator);
            var ilParser = new ILParser(referenceVisitor);
            referenceVisitor.ParseMethod += (sender, args) => CollectReferencesFromILCode(
                ilParser,
                args.CollectedMethod.Method, args.CollectedMethod.MethodId, args.CollectedMethod.ClassId);
            // parse IL of colected methods
            HandleCollectedMethods(ilParser);

            dataCollector.Dispose();
        }

        private void HandleCollectedMethods(ILParser ilParser)
        {
            Console.WriteLine("Parsing IL... ({0} methods){1}", collectedMethods.Count, Environment.NewLine);
            // dive into methods and collect, what they reference
            for (var i = 0; i < collectedMethods.Count; i++)
            {
                var method = collectedMethods[i];
                CollectReferencesFromILCode(ilParser, method.Method, method.MethodId, method.ClassId);
            }
        }

        private void CollectReferencesFromILCode(ILParser ilParser, MethodBase method, int methodId, int classId)
        {
            ilParser.Parse(method, methodId, classId);
        }
    }
}
