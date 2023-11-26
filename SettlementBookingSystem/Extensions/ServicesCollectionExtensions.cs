using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.Configuration;
using SettlementBookingSystem.Application.Exceptions;
using SettlementBookingSystem.Application.Settings;
using SettlementBookingSystem.ProblemDetails;

namespace SettlementBookingSystem.Extensions
{
    [ExcludeFromCodeCoverage]
    internal static class ServicesCollectionExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SettlementBookingSystem", Version = "v1" });
            });
        }

        public static void ConfigureDbContext(this IServiceCollection services)
        {
            services.AddDbContext<Infrastructure.BookingDbContext>(opt => opt.UseInMemoryDatabase("Booking"));
        }

        public static void ConfigureProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.IncludeExceptionDetails = (ctx, ex) => { return false; };
                // Configure problem details per exception type here.
                options.Map<ConflictException>(ex => new ConflictProblemDetails(ex));
                options.Map<ValidationException>(ex => new BadRequestProblemDetails(ex));
                options.Map<AggregateException>(ex =>
                {
                    return ex.InnerException switch
                    {
                        null => new UnhandledExceptionProblemDetails(ex),
                        ValidationException validation => new BadRequestProblemDetails(validation),
                        ConflictException conflict => new ConflictProblemDetails(conflict),
                        _ => new UnhandledExceptionProblemDetails(ex.InnerException),
                    };
                });

                // This must always be last as this will serve to handle unhandled exceptions.
                options.Map<Exception>(ex => new UnhandledExceptionProblemDetails(ex));
            });
        }

        public static void ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BookingSettings>().Bind(configuration.GetSection(BookingSettings.Section));
        }

    }
}
