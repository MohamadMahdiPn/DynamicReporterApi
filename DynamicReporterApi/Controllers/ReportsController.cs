using DynamicReporterApi.Domain.Data;
using DynamicReporterApi.Domain.Entities;
using DynamicReporterApi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;

namespace DynamicReporterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        #region Constructor
        private readonly ICustomReportRepository _reportRepo;
        private readonly ApplicationDbContext _context;

        public ReportsController(ICustomReportRepository reportRepo, ApplicationDbContext context)
        {
            _reportRepo = reportRepo;
            _context = context;
        }

        #endregion
      

        [HttpPost("add")]
        public async Task<IActionResult> AddReport([FromBody] AddReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query cannot be empty");

            var report = new CustomReport
            {
                Title = request.Title,
                Description = request.Description,
                SqlQuery = request.Query,
                Parameters = request.Parameters.Select(p => new ReportParameter
                {
                    Name = p.Name,
                    DataType = p.DataType,
                    DefaultValue = p.DefaultValue
                }).ToList()
            };

            _context.CustomReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { report.Id, report.Title });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportToExcel([FromBody] RunReportRequest request)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var report = await _context.CustomReports
                .Include(r => r.Parameters)
                .FirstOrDefaultAsync(r => r.Id == request.ReportId);

            if (report == null)
                return NotFound("Report not found");

            try
            {
                using var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = report.SqlQuery;
                foreach (var param in report.Parameters)
                {
                    var value = request.Parameters.TryGetValue(param.Name, out var val)
                        ? val
                        : param.DefaultValue;

                    command.Parameters.Add(new SqlParameter(param.Name, value));
                }
               

                using var reader = await command.ExecuteReaderAsync();
                var table = new DataTable();
                table.Load(reader);

                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Report");

                // Add header
                for (int i = 0; i < table.Columns.Count; i++)
                    ws.Cells[1, i + 1].Value = table.Columns[i].ColumnName;

                // Add rows
                for (int r = 0; r < table.Rows.Count; r++)
                for (int c = 0; c < table.Columns.Count; c++)
                    ws.Cells[r + 2, c + 1].Value = table.Rows[r][c];

                // Stream to response
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{report.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunReport([FromBody] RunReportRequest request)
        {
            var report = await _context.CustomReports
                .Include(r => r.Parameters)
                .FirstOrDefaultAsync(r => r.Id == request.ReportId);

            if (report == null)
                return NotFound("Report not found");

            try
            {
                using var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = report.SqlQuery;
                command.CommandType = report.IsStoredProcedure
                    ? CommandType.StoredProcedure
                    : CommandType.Text;

                // Add parameters if needed
                foreach (var param in report.Parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = param.Name;

                    // Try get value from request, fallback to default
                    if (request.Parameters != null && request.Parameters.TryGetValue(param.Name, out var val))
                        dbParam.Value = val;
                    else
                        dbParam.Value = param.DefaultValue;

                    command.Parameters.Add(dbParam);
                }

                using var reader = await command.ExecuteReaderAsync();
                var resultTable = new DataTable();
                resultTable.Load(reader);

                // Return rows as JSON
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in resultTable.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in resultTable.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    rows.Add(dict);
                }

                return Ok(rows);
            }
            catch (Exception ex)
            {
                return BadRequest($"Execution error: {ex.Message}");
            }
        }
    }



    public class RunReportRequest
    {
        public int ReportId { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
    public class AddReportRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public string Query { get; set; }
        public List<ReportParameterDto> Parameters { get; set; } = new();
    }

    public class ReportParameterDto
    {
        public string Name { get; set; } // e.g. "@StartDate"
        public string DataType { get; set; } // e.g. "string", "int", "datetime"
        public string? DefaultValue { get; set; }
    }
}
