namespace SeidorSmallWebApi.CustomControllers.Extensions
{
    public static class UrlExtensions
    {
        public static string ReplaceSkipValue(this string originalUrl, int newSkipValue)
        {
            // Parse the original URL
            var uri = new Uri(originalUrl);

            // Get the query parameters
            var queryParts = System.Web.HttpUtility.ParseQueryString(uri.Query);

            // Replace or add the 'skip' parameter
            queryParts["skip"] = newSkipValue.ToString();

            // Construct the updated URL
            var updatedUriBuilder = new UriBuilder(uri)
            {
                Query = queryParts.ToString()
            };

            return updatedUriBuilder.Uri.ToString();
        }
    }
}
