using DynamicReporterApi.Domain.Data;
using DynamicReporterApi.Domain.Interfaces;
using DynamicReporterApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicReporterApi.Domain;

public static class DynamicReporterApiDomainRegistration
{
    public static IServiceCollection ConfigureDomainServiceRegistration(this IServiceCollection services,
        IConfiguration configuration)
    {
        #region ConnectionString

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseLazyLoadingProxies().UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.CommandTimeout(120));

        });


        #endregion



        services.AddScoped<ICustomReportRepository,CustomReportRepository>();

        return services;
    }
}