using StajApi.Data;
using StajApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace StajApi.Services
{
    public class StudentService : IStudentService
    {
        // nameFilter parametresi eklendi
        public IEnumerable<Student> GetAllStudents(string? nameFilter = null)
        {
            var query = StaticDataStore.Students.AsEnumerable(); // Sorguyu IEnumerable olarak başlatalım

            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                // Eğer nameFilter varsa, isme göre filtreleme yap
                query = query.Where(s => s.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
            }

            return query;
        }

        public Student? GetStudentById(int id)
        {
            return StaticDataStore.Students.FirstOrDefault(s => s.Id == id);
        }

        public Student AddStudent(Student student)
        {
            student.Id = StaticDataStore.GetNextStudentId();
            StaticDataStore.Students.Add(student);
            return student;
        }

        public bool UpdateStudent(Student student)
        {
            var existingStudent = StaticDataStore.Students.FirstOrDefault(s => s.Id == student.Id);
            if (existingStudent == null)
            {
                return false;
            }

            existingStudent.Name = student.Name;
            existingStudent.Major = student.Major;
            existingStudent.Grade = student.Grade;
            return true;
        }

        public bool DeleteStudent(int id)
        {
            var studentToRemove = StaticDataStore.Students.FirstOrDefault(s => s.Id == id);
            if (studentToRemove == null)
            {
                return false;
            }
            StaticDataStore.Students.Remove(studentToRemove);
            return true;
        }
    }
}