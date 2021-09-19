namespace SourcetrailDotnetIndexer
{
    static class GlobalOptions
    {
        /// <summary>
        /// Whether to collect all types, which are invoked by methods (without storing all members of that type)
        /// </summary>
        public static bool CollectAllInvocations { get; set; }

        /// <summary>
        /// Whether to collect all types, which are referenced by methods (without storing all members of that type)
        /// </summary>
        /// TODO: Is this useful at all ? (one would typically use -ami to achieve basically the same...)
        public static bool CollectAllTypesReferencedByMethods { get; set; }
    }
}