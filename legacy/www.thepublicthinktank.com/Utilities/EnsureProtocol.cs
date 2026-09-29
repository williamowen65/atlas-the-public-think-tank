namespace atlas_the_public_think_tank.Utilities
{
    public static class UrlHelpers
    {
        /// <summary>
        /// Ensures a URL has a protocol (http or https). 
        /// Defaults to https if no protocol is present.
        /// </summary>
        /// <param name="url">The URL to process</param>
        /// <param name="defaultProtocol">The default protocol to use (default: "https")</param>
        /// <returns>URL with protocol prepended</returns>
        public static string EnsureProtocol(string url, string defaultProtocol = "https")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            // Already has a protocol
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            // Prepend the default protocol
            return $"{defaultProtocol}://{url}";
        }

        /// <summary>
        /// Extension method version for string objects
        /// </summary>
        public static string WithProtocol(this string url, string defaultProtocol = "https")
        {
            return EnsureProtocol(url, defaultProtocol);
        }
    }
}