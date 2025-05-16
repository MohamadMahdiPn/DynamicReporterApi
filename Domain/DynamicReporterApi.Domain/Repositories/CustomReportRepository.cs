using DynamicReporterApi.Domain.Data;
using DynamicReporterApi.Domain.Entities;
using DynamicReporterApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace DynamicReporterApi.Domain.Repositories;

public class CustomReportRepository: ICustomReportRepository
{
    #region Constructor
    private readonly ApplicationDbContext _context;
    public CustomReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    #endregion

    public async Task<CustomReport> GetByIdAsync(int id) => await _context.CustomReports.FindAsync(id);
    public async Task<IEnumerable<CustomReport>> GetAllAsync()
    {
        return await _context.CustomReports.ToListAsync();
    }
    public async Task AddAsync(CustomReport report)
    {
        await _context.CustomReports.AddAsync(report);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(CustomReport report)
    {
        _context.CustomReports.Update(report);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var report = await GetByIdAsync(id);
        if (report != null)
        {
            _context.CustomReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<DataTable> ExecuteReportAsync(CustomReport report, Dictionary<string, string> parameterValues)
    {
        using var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = report.SqlQuery;
        cmd.CommandType = CommandType.Text;

        foreach (var param in report.Parameters)
        {
            var paramName = param.Name;
            parameterValues.TryGetValue(paramName, out var rawValue);
            var finalValue = string.IsNullOrEmpty(rawValue) ? param.DefaultValue : rawValue;

            var dbParam = cmd.CreateParameter();
            dbParam.ParameterName = paramName;
            dbParam.Value = string.IsNullOrEmpty(finalValue) ? DBNull.Value : ConvertToType(finalValue, param.DataType);
            cmd.Parameters.Add(dbParam);
        }

        var table = new DataTable();
        using var reader = await cmd.ExecuteReaderAsync();
        table.Load(reader);
        return table;
    }

    private object ConvertToType(string value, string type)
    {
        return type.ToLower() switch
        {
            "int" => int.TryParse(value, out var i) ? i : 0,
            "datetime" => DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue,
            "bool" => bool.TryParse(value, out var b) && b,
            _ => value
        };
    }

}
