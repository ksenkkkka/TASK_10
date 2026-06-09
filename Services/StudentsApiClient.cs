using System.Net.Http.Json;
using Task_10.Web.Models;

namespace Task_10.Web.Services;

public sealed class StudentsApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<StudentDto>> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<StudentDto>>("/api/students", cancellationToken) ?? [];
    }

    public async Task<StudentDetailsDto?> GetStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<StudentDetailsDto>($"/api/students/{id}", cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<CourseDto>>("/api/courses", cancellationToken) ?? [];
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/students", model, cancellationToken);
        await EnsureSuccessWithMessageAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<StudentDto>(cancellationToken))!;
    }

    public async Task AssignCourseAsync(int studentId, int courseId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/students/{studentId}/courses", new AssignCourseRequest(courseId), cancellationToken);
        await EnsureSuccessWithMessageAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessWithMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? $"API request failed with status {(int)response.StatusCode}."
            : message);
    }
}
