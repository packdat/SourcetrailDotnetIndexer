using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SourcetrailDotnetIndexer.PdbSupport
{
    /// <summary>
    /// Contains information about a method obtained from a program database (PDB)
    /// </summary>
    [DebuggerDisplay("{Token} {LanguageName} {DocumentName}")]
    class PdbMethod
    {
        /// <summary>
        /// The metadata token of this method
        /// </summary>
        public int Token { get; private set; }

        /// <summary>
        /// The filename, where this method is defined in
        /// </summary>
        public string DocumentName { get; private set; }

        /// <summary>
        /// The language, this method is written in
        /// </summary>
        public string LanguageName { get; private set; }

        private readonly List<CodeSequence> sequences;

        /// <summary>
        /// Creates a new <see cref="PdbMethod"/>
        /// </summary>
        /// <param name="token">The metadata token of the method</param>
        /// <param name="documentName">The name of the document, the method is defined in</param>
        /// <param name="languageName">The language, the method is written in</param>
        public PdbMethod(int token, string documentName, string languageName)
        {
            Token = token;
            DocumentName = documentName;
            LanguageName = languageName;
            sequences = new List<CodeSequence>();
        }

        /// <summary>
        /// Adds a code sequence for this method.
        /// </summary>
        /// <param name="ilStartOffset">The IL (Intermediate Language) offset, where the sequence begins</param>
        /// <param name="startLine">The line in the source-code, where the sequence begins</param>
        /// <param name="startColumn">The column in the source-code, where the sequence begins</param>
        /// <param name="endLine">The line in the source-code, where the sequence end</param>
        /// <param name="endColumn">The column in the source-code, where the sequence ends</param>
        public void AddSequence(int ilStartOffset, int startLine, int startColumn, int endLine, int endColumn)
        {
            var seq = new CodeSequence
            {
                ILStartOffset = ilStartOffset,
                StartLine = startLine,
                StartColumn = startColumn,
                EndLine = endLine,
                EndColumn = endColumn
            };
            sequences.Add(seq);
        }

        /// <summary>
        /// Gets the <see cref="CodeSequence"/> for the specified IL-offset
        /// </summary>
        /// <param name="ilOffset"></param>
        /// <returns>a <see cref="CodeSequence"/> containing the specified IL-offset or null, if no such sequence exist</returns>
        public CodeSequence GetSequenceForILOffset(int ilOffset)
        {
            return sequences.LastOrDefault(s => s.ILStartOffset <= ilOffset);
        }
    }
}
