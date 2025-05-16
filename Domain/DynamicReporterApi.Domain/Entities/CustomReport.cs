using System.ComponentModel.DataAnnotations;

namespace DynamicReporterApi.Domain.Entities;

public class CustomReport
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public string SqlQuery { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsStoredProcedure { get; set; }
    public virtual List<ReportParameter> Parameters { get; set; } = new();

    public bool IsSafeQuery(string sql)
    {
        var lowered = sql.ToLowerInvariant();
        return lowered.StartsWith("select") &&
               !lowered.Contains("insert") &&
               !lowered.Contains("update") &&
               !lowered.Contains("delete") &&
               !lowered.Contains("drop") &&
               !lowered.Contains("exec");
    }

}
public class ReportParameter
{
    public int Id { get; set; }

    public int CustomReportId { get; set; }
    public virtual CustomReport CustomReport { get; set; }

    [Required]
    public string Name { get; set; } // e.g., "@StartDate"

    [Required]
    public string DataType { get; set; } // "string", "int", "datetime", etc.

    public string? DefaultValue { get; set; }
}
