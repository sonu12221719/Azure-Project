using Azure;
using Azure.Data.Tables;
namespace DemoFunctionApp
{
    public class CourseEntity : ITableEntity
    {
        // ✅ Required by Azure Table Storage
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;      // CourseId
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // ✅ Custom properties

        public string CourseID { get; set; }
        public string CourseName { get; set; } = default!;
        public string Rating { get; set; }

    }
}