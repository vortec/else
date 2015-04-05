namespace Else.Extensibility
{
    public enum ProviderInterest
    {
        /// <summary>
        /// Provider has no interest in providing results for the query.
        /// </summary>
        None,

        /// <summary>
        /// Provider shares control over results with other plugins.
        /// </summary>
        Shared,

        /// <summary>
        /// The fallback
        /// </summary>
        Fallback,

        /// <summary>
        /// Provider demands exclusive control over the results for the query.
        /// </summary>
        Exclusive
    }
}
