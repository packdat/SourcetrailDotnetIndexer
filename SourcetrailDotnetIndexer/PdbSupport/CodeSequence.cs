using System.Diagnostics;

namespace SourcetrailDotnetIndexer.PdbSupport
{ 
    /// <summary>
    /// Specifies a range of code obtained from a PDB-file
    /// </summary>
    [DebuggerDisplay("@{ILStartOffset}: ({StartLine} {StartColumn})..({EndLine} {EndColumn})")]
    class CodeSequence
    {
        /// <summary>
        /// The offset in the IL of a method, where this sequence starts
        /// </summary>
        public int ILStartOffset { get; set; }
        /// <summary>
        /// The line in the source-code, where this sequence starts
        /// </summary>
        public int StartLine { get; set; }
        /// <summary>
        /// The line in the source-code, where this sequence ends
        /// </summary>
        public int EndLine { get; set; }
        /// <summary>
        /// The column in the source-code, where this sequence starts
        /// </summary>
        public int StartColumn { get; set; }
        /// <summary>
        /// The column in the source-code, where this sequence ends
        /// </summary>
        public int EndColumn { get; set; }
    }
}
