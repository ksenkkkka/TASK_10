using System.ComponentModel.DataAnnotations;
using Task_10.Web.Components;
using Task_10.Web.Models;
using Task_10.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<StudentsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5210");
});

builder.Services.AddScoped<ObservedStudentsState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

var courses = new List<CourseDto>
{
    new(1, "Blazor Fundamentals", 5),
    new(2, "Databases", 4),
    new(3, "Web API Design", 5),
    new(4, "Software Testing", 3)
};

var students = new List<StudentDto>
{
    new(1, "s10001", "Anna", "Kowalska", "anna.kowalska@example.edu", 2),
    new(2, "s10002", "Piotr", "Nowak", "piotr.nowak@example.edu", 4),
    new(3, "s10003", "Marta", "Zielinska", "marta.zielinska@example.edu", 1)
};

var assignments = new List<StudentCourseAssignment>
{
    new(1, 1, DateTimeOffset.UtcNow.AddDays(-12)),
    new(1, 3, DateTimeOffset.UtcNow.AddDays(-4)),
    new(2, 2, DateTimeOffset.UtcNow.AddDays(-8))
};

app.MapGet("/api/students", () => Results.Ok(students));

app.MapGet("/api/students/{id:int}", (int id) =>
{
    var student = students.FirstOrDefault(x => x.Id == id);
    if (student is null)
    {
        return Results.NotFound("Student was not found.");
    }

    var assignedCourses = assignments
        .Where(x => x.StudentId == id)
        .Join(courses, a => a.CourseId, c => c.Id, (a, c) => new AssignedCourseDto(c.Id, c.Name, c.Ects, a.AssignedAt))
        .OrderBy(x => x.Name)
        .ToList();

    return Results.Ok(new StudentDetailsDto(
        student.Id,
        student.IndexNumber,
        student.FirstName,
        student.LastName,
        student.Email,
        student.Semester,
        assignedCourses));
});

app.MapPost("/api/students", (CreateStudentModel request) =>
{
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
    {
        var errors = validationResults
            .SelectMany(x => x.MemberNames.Select(member => new { member, x.ErrorMessage }))
            .GroupBy(x => x.member)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage ?? "Invalid value.").ToArray());

        return Results.ValidationProblem(errors);
    }

    var nextId = students.Count == 0 ? 1 : students.Max(x => x.Id) + 1;
    var student = new StudentDto(nextId, request.IndexNumber, request.FirstName, request.LastName, request.Email, request.Semester);
    students.Add(student);

    return Results.Created($"/api/students/{student.Id}", student);
});

app.MapGet("/api/courses", () => Results.Ok(courses));

app.MapPost("/api/students/{id:int}/courses", (int id, AssignCourseRequest request) =>
{
    if (students.All(x => x.Id != id))
    {
        return Results.NotFound("Student was not found.");
    }

    if (courses.All(x => x.Id != request.CourseId))
    {
        return Results.BadRequest("Select an existing course.");
    }

    if (assignments.Any(x => x.StudentId == id && x.CourseId == request.CourseId))
    {
        return Results.BadRequest("The student is already assigned to this course.");
    }

    var assignment = new StudentCourseAssignment(id, request.CourseId, DateTimeOffset.UtcNow);
    assignments.Add(assignment);

    return Results.Created($"/api/students/{id}", assignment);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal sealed record StudentCourseAssignment(int StudentId, int CourseId, DateTimeOffset AssignedAt);
