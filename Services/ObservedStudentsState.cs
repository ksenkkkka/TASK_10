using Task_10.Web.Models;

namespace Task_10.Web.Services;

public sealed class ObservedStudentsState
{
    private readonly Dictionary<int, StudentDto> observed = [];

    public event Action? Changed;

    public int Count => observed.Count;
    public IReadOnlyCollection<StudentDto> Students => observed.Values.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();

    public bool IsObserved(int studentId) => observed.ContainsKey(studentId);

    public void Toggle(StudentDto student)
    {
        if (!observed.Remove(student.Id))
        {
            observed[student.Id] = student;
        }

        Changed?.Invoke();
    }

    public void Remove(int studentId)
    {
        if (observed.Remove(studentId))
        {
            Changed?.Invoke();
        }
    }
}
