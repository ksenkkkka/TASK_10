using System.ComponentModel.DataAnnotations;

namespace Task_10.Web.Models;

public sealed record StudentDto(int Id, string IndexNumber, string FirstName, string LastName, string Email, int Semester);
public sealed record CourseDto(int Id, string Name, int Ects);
public sealed record AssignedCourseDto(int Id, string Name, int Ects, DateTimeOffset AssignedAt);
public sealed record StudentDetailsDto(int Id, string IndexNumber, string FirstName, string LastName, string Email, int Semester, List<AssignedCourseDto> Courses);

public sealed class CreateStudentModel
{
    [Required(ErrorMessage = "Index number is required.")]
    public string IndexNumber { get; set; } = "";

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = "";

    [Range(1, 8, ErrorMessage = "Semester must be between 1 and 8.")]
    public int Semester { get; set; } = 1;
}

public sealed class AssignCourseModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a course.")]
    public int CourseId { get; set; }
}

public sealed record AssignCourseRequest(int CourseId);
