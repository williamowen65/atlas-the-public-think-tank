namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers
{
    /// <summary>
    /// Represents a customizable filter that users can apply to content queries
    /// </summary>
    /// <remarks>
    /// This class structure matches the client-side filter format used in JavaScript 
    /// for consistent filtering between server and client
    /// </remarks>
    public class ContentFilter
    {

        public string ContentType { get; set; } = "both";
        /// <summary>
        /// Filter range for average vote values
        /// </summary>
        public RangeFilter<double> AvgVoteRange { get; set; } = new RangeFilter<double> { Min = 0.0, Max = 10.0 };

        /// <summary>
        /// Filter range for total vote counts with no upper limit
        /// </summary>
        public NullableMaxRangeFilter<int> TotalVoteCount { get; set; } = new NullableMaxRangeFilter<int> { Min = 0, Max = null };

        /// <summary>
        /// Filter range for content creation dates
        /// </summary>
        public DateRangeFilter DateRange { get; set; } = new();

        /// <summary>
        /// Filter by content tags/categories
        /// </summary>
        public List<string> Tags { get; set; } = new();

        

        /// <summary>
        /// Creates a new instance of the default content filter
        /// </summary>
        public ContentFilter() { }

        /// <summary>
        /// Creates a content filter from a JSON string (typically from a cookie)
        /// </summary>
        /// <param name="json">JSON string representation of filter values</param>
        /// <returns>Populated ContentFilter object</returns>
        public static ContentFilter FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new ContentFilter();

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true,
                    WriteIndented = true
                };

                ContentFilter? filter = System.Text.Json.JsonSerializer.Deserialize<ContentFilter>(json, options);

                if (filter == null)
                {
                    return new ContentFilter();
                }

                filter.AvgVoteRange ??= new RangeFilter<double> { Min = 0.0, Max = 10.0 };
                filter.TotalVoteCount ??= new NullableMaxRangeFilter<int> { Min = 0, Max = null };
                filter.DateRange ??= new DateRangeFilter();
                filter.Tags ??= new List<string>();

                return filter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return new ContentFilter();
            }
        }

        /// <summary>
        /// Converts the filter to a JSON string for storage (e.g., in a cookie)
        /// </summary>
        /// <returns>JSON string representation of the filter</returns>
        public string ToJson()
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                IncludeFields = true
            };

            return System.Text.Json.JsonSerializer.Serialize(this, options);
        }
    }

    /// <summary>
    /// Represents a numeric range filter with minimum and maximum values
    /// </summary>
    /// <typeparam name="T">The numeric type for the range (int, double, etc.)</typeparam>
    public class RangeFilter<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// Minimum value in the range
        /// </summary>
        public T Min { get; set; }

        /// <summary>
        /// Maximum value in the range
        /// </summary>
        public T Max { get; set; }

        
    }


    /// <summary>
    /// Represents a numeric range filter with minimum value and nullable maximum value
    /// </summary>
    /// <typeparam name="T">The numeric type for the range (int, double, etc.)</typeparam>
    public class NullableMaxRangeFilter<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// Minimum value in the range
        /// </summary>
        public T Min { get; set; }

        /// <summary>
        /// Maximum value in the range, null means no upper limit
        /// </summary>
        public T? Max { get; set; }

       
    }

    /// <summary>
    /// Represents a date range filter with from and to dates
    /// </summary>
    public class DateRangeFilter
    {
        /// <summary>
        /// Start date for the filter range
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// End date for the filter range
        /// </summary>
        public DateTime? To { get; set; }

    }
}
