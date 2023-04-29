namespace JumpKingRavensMod.API
{
    /// <summary>
    /// An interface representing a filter which can check against an exclusion list
    /// </summary>
    public interface IExcludedTermFilter
    {
        /// <summary>
        /// Returns whether the text to check contains an excluded term or not
        /// </summary>
        bool ContainsExcludedTerm(string textToCheck);
    }
}
