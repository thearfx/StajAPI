using StajApi.Models;
using System.Collections.Generic;

namespace StajApi.Services
{
    public interface IStudentService
    {
        IEnumerable<Student> GetAllStudents(string? nameFilter = null); // nameFilter parametresi eklendi
        Student? GetStudentById(int id);
        Student AddStudent(Student student);
        bool UpdateStudent(Student student);
        bool DeleteStudent(int id);
    }
}