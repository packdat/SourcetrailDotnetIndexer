namespace SourcetrailDotnetIndexer.PdbSupport
{
    /// <summary>
    /// The interface for a PDB-Reader
    /// </summary>
    interface IPdbReader
    {
        /// <summary>
        /// Retrieves the method with the specified token
        /// </summary>
        /// <param name="methodToken">the Metadata-Token for a method</param>
        /// <returns>a <see cref="PdbMethod"/> or null if no method with the specified token could be found</returns>
        PdbMethod GetMethod(int methodToken);
    }
}
