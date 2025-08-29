using StajApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace StajApi.Data
{
    public static class StaticDataStore
    {
        public static List<Student> Students { get; private set; }

        static StaticDataStore()
        {
            Students = new List<Student>
            {
                new Student { Id = 1, Name = "Ayşe Yılmaz", Major = "Bilgisayar Mühendisliği", Grade = 3 },
                new Student { Id = 2, Name = "Can Demir", Major = "Elektrik Mühendisliği", Grade = 2 },
                new Student { Id = 3, Name = "Elif Kaya", Major = "Yazılım Mühendisliği", Grade = 4 },
                new Student { Id = 4, Name = "Mehmet Toprak", Major = "Makine Mühendisliği", Grade = 1 }
            };
        }

        public static int GetNextStudentId()
        {
            return Students.Any() ? Students.Max(s => s.Id) + 1 : 1;
        }
    }
}