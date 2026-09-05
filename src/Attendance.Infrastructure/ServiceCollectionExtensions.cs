using Attendance.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Attendance.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendanceInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseInMemoryDatabase(connectionString));

        services.AddScoped<IAttendancePersistenceContext, AttendancePersistenceContext>();
        services.AddScoped<AttendanceCommandService>();
        services.AddScoped<AttendanceQueryService>();

        return services;
    }
}
