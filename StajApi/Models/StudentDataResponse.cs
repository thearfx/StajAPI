using Newtonsoft.Json;

namespace StajApi.Models
{
    public class StudentDataResponse
    {
        [JsonProperty("student")]
        public Student Student { get; set; }

        public StudentDataResponse(Student student)
        {
            Student = student;
        }
    }
}