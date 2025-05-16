using DynamicReporterApi.Domain.Entities;
using System.Data;

namespace DynamicReporterApi.Domain.Interfaces;

public interface ICustomReportRepository
{
    Task<DataTable> ExecuteReportAsync(CustomReport report, Dictionary<string, string> parameterValues);
    Task<CustomReport> GetByIdAsync(int id);
    Task<IEnumerable<CustomReport>> GetAllAsync();
    Task AddAsync(CustomReport report);
    Task UpdateAsync(CustomReport report);
    Task DeleteAsync(int id);
}
