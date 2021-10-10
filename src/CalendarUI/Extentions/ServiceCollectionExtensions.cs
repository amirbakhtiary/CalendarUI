using AutoMapper;
using CalendarUI.Data.Database;
using CalendarUI.Data.Repository;
using CalendarUI.Infrastructure.AutoMapper;
using CalendarUI.Models;
using CalendarUI.Service.Query;
using CalendarUI.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace CalendarUI.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomizeControllers(this IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            return services;
        }

        public static IServiceCollection AddCustomizeService(this IServiceCollection services)
        {
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddTransient<IValidator<CreateAppointmentModel>, CreateAppointmentModelValidator>();
            services.AddTransient<IValidator<UpdateAppointmentModel>, UpdateAppointmentModelValidator>();

            return services;
        }

        public static IServiceCollection AddCustomizedDataStore(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddDbContext<CalendarContext>(options => options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection"),
                        b =>
                        {
                            b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            b.MigrationsHistoryTable($"__{nameof(CalendarContext)}");
                        }));

            return services;
        }

        public static IServiceCollection ConfigAutoMapper(this IServiceCollection services)
        {
            services.AddTransient(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            }).CreateMapper());
            return services;
        }

        public static IServiceCollection ConfigMediatR(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.Load(typeof(GetAppointmentByIdQuery).GetTypeInfo().Assembly.GetName().Name));
            return services;
        }


        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());

            return services;
        }

        public static void UpdateDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var calendarContext = serviceScope.ServiceProvider.GetService<CalendarContext>();
            calendarContext.Database.Migrate();
        }
    }
}
