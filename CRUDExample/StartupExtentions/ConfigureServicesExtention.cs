using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
    public static class ConfigureServicesExtention
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            services.AddTransient<ResponseHeaderActionFilter>();
            services.AddControllersWithViews(options =>
            {
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

                options.Filters.Add(new ResponseHeaderActionFilter(logger)
                {
                    Key = "My-Key-From-Global",
                    Value = "My-Value-From-Global",
                    Order = 2
                });
            });
            services.AddScoped<IPersonsService, PersonsService>();
            services.AddScoped<ICountriesService, CountriesService>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();
            services.AddScoped<ICountriesRepository, CountriesRepository>();

            if (environment.IsEnvironment("Test") == false)
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                });


            return services;
        }
    }
}
