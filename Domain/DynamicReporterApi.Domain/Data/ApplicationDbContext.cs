using DynamicReporterApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DynamicReporterApi.Domain.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    :DbContext(options)
{
    public virtual DbSet<CustomReport> CustomReports { get; set; }
    public virtual DbSet<ReportParameter> ReportParameters { get; set; }



}
