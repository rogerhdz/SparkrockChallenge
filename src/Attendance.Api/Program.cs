using Attendance.Application;
using Attendance.Domain;
using Attendance.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAttendanceInfrastructure("AttendanceDb");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();

    if (!dbContext.Schools.Any())
    {
        dbContext.Schools.Add(new School
        {
            Id = 1,
            Name = "North Elementary",
            AbsenceAlertThreshold = 10
        });
    }

    if (!dbContext.AttendanceCodes.Any())
    {
        dbContext.AttendanceCodes.AddRange(
            new AttendanceCode { Id = 1, CodeValue = "P", Description = "Present", IsAbsent = false, IsExcused = false, IsActive = true },
            new AttendanceCode { Id = 2, CodeValue = "A", Description = "Absent", IsAbsent = true, IsExcused = false, IsActive = true },
            new AttendanceCode { Id = 3, CodeValue = "T", Description = "Tardy", IsAbsent = false, IsExcused = false, IsActive = true });
    }

    dbContext.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/attendance/bulk", async (AttendanceBatchRequestDto request, HttpRequest httpRequest, AttendanceCommandService service) =>
{
    var resolvedSchoolId = request.SchoolId;

    if (!resolvedSchoolId.HasValue && httpRequest.Query.TryGetValue("schoolId", out var schoolIdQuery))
    {
        if (int.TryParse(schoolIdQuery.ToString(), out var parsedSchoolId))
        {
            resolvedSchoolId = parsedSchoolId;
        }
    }

    if (!resolvedSchoolId.HasValue || resolvedSchoolId <= 0)
    {
        return Results.BadRequest("A valid schoolId is required in the request body or query string.");
    }

    var result = await service.ProcessBatchAsync(resolvedSchoolId.Value, request.Records);
    return Results.Ok(result);
});

app.MapGet("/api/attendance/history/{studentId:int}/{schoolId:int}", async (int studentId, int schoolId, AttendanceQueryService service, string? schoolYear = null) =>
{
    var result = await service.GetHistoryAsync(studentId, schoolId, schoolYear);
    return Results.Ok(result);
});

app.MapGet("/api/attendance/chronic-status/{studentId:int}/{schoolId:int}", async (int studentId, int schoolId, AttendanceQueryService service, string? schoolYear = null) =>
{
    var result = await service.GetChronicStatusAsync(studentId, schoolId, schoolYear);
    return Results.Ok(result);
});

app.Run();
