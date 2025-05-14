using DynamicReporterApi.Domain.Entities;
using System.Data;

namespace DynamicReporterApi.Domain.Interfaces;

public interface ICustomReportRepository
{
    Task<DataTable> ExecuteReportAsync(CustomReport report, Dictionary<string, string> parameterValues);
}
