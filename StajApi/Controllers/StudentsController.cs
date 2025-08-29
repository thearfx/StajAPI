// StajApi/Controllers/StudentsController.cs
using Microsoft.AspNetCore.Mvc;
using StajApi.Models;
using StajApi.Services;
using StajApi.Core.Infrastructure.Utilities; // RoarResponse için
using StajApi.Entities.Enums; // RoarResponseCodeType için
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System; // StringComparison için gerekli

namespace StajApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        // GET: api/Students
        [HttpGet]
        [ProducesResponseType(typeof(RoarResponse<List<Student>>), StatusCodes.Status200OK)]
        public RoarResponse<List<Student>> GetStudents([FromQuery] string? name = null) // name parametresi eklendi
        {
            try
            {
                List<Student> students;
                string message;
                RoarResponseCodeType responseType;

                // Eğer name parametresi "ali" ise (büyük/küçük harf duyarsız)
                if (!string.IsNullOrEmpty(name) && name.Equals("ali", StringComparison.OrdinalIgnoreCase))
                {
                    // Dikkat: Servisten "Mehmet" isimli öğrencileri çekiyoruz
                    // Bu çağrı, "Mehmet" adını içeren TÜM öğrencileri getirecektir.
                    students = _studentService.GetAllStudents("Mehmet").ToList();
                    message = "Özel durum: 'ali' adıyla arama yapıldı. Mehmet isimli öğrenciler listelendi.";
                    responseType = RoarResponseCodeType.Info;
                }
                else
                {
                    // Diğer durumlarda normal filtreleme
                    students = _studentService.GetAllStudents(name).ToList();
                    message = "Öğrenciler başarıyla listelendi.";
                    responseType = RoarResponseCodeType.Success;
                }

                return new RoarResponse<List<Student>>(true, message, students)
                {
                    ResponseType = responseType,
                    TotalCount = students.Count,
                    HttpStatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öğrencileri listelerken bir hata oluştu.");

                return new RoarResponse<List<Student>>(false, "Öğrencileri listeleme sırasında beklenmeyen bir hata oluştu.", default(List<Student>))
                {
                    ResponseType = RoarResponseCodeType.Error,
                    Exception = ex,
                    HttpStatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
        

        // GET: api/Students/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoarResponse<StudentDataResponse>), StatusCodes.Status200OK)]
        public RoarResponse<StudentDataResponse> GetStudent(int id)
        {
            var student = _studentService.GetStudentById(id);

            if (student == null)
            {
                return new RoarResponse<StudentDataResponse>(false, $"Aradığınız {id} ID'li öğrenci sistemde kayıtlı değil.", default(StudentDataResponse))
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status404NotFound
                };
            }

            var studentDataResponse = new StudentDataResponse(student);

            return new RoarResponse<StudentDataResponse>(true, "Öğrenci başarıyla getirildi.", studentDataResponse)
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }

        // POST: api/Students
        [HttpPost]
        [ProducesResponseType(typeof(RoarResponse<Student>), StatusCodes.Status200OK)]
        public RoarResponse<Student> CreateStudent(Student student)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                                       .SelectMany(x => x.Value.Errors)
                                       .Select(x => x.ErrorMessage)
                                       .ToList();

                var idParseError = ModelState["Id"]?.Errors.FirstOrDefault(e => e.ErrorMessage.Contains("converted to System.Int32"));

                string errorMessage = "Gönderilen öğrenci bilgilerinde eksik veya hatalı alanlar var.";
                if (idParseError != null)
                {
                    errorMessage = $"ID alanı geçersiz formatta. Lütfen ID'yi sayısal bir değer olarak girin. Hata: {idParseError.ErrorMessage}";
                }
                else if (errors.Any())
                {
                    errorMessage = string.Join(" ", errors);
                }

                return new RoarResponse<Student>(false, errorMessage, default(Student))
                {
                    Content = string.Join(" ", errors),
                    ResponseType = RoarResponseCodeType.ValidationError,
                    HttpStatusCode = StatusCodes.Status400BadRequest
                };
            }

            var createdStudent = _studentService.AddStudent(student);

            return new RoarResponse<Student>(true, "Öğrenci başarıyla oluşturuldu.", createdStudent)
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status201Created
            };
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RoarResponse<object>), StatusCodes.Status200OK)]
        public RoarResponse<object> UpdateStudent(int id, Student updatedStudent)
        {
            if (id != updatedStudent.Id)
            {
                return new RoarResponse<object>(false, "URL'deki ID ile istek gövdesindeki ID eşleşmiyor.", default(object))
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status400BadRequest
                };
            }

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values.SelectMany(v => v.Errors)
                                                        .Select(e => e.ErrorMessage)
                                                        .ToList();
                return new RoarResponse<object>(false, "Güncelleme bilgilerinde eksik veya hatalı alanlar var.", default(object))
                {
                    Content = string.Join(" ", validationErrors),
                    ResponseType = RoarResponseCodeType.ValidationError,
                    HttpStatusCode = StatusCodes.Status400BadRequest
                };
            }

            var isUpdated = _studentService.UpdateStudent(updatedStudent);
            if (!isUpdated)
            {
                return new RoarResponse<object>(false, "Belirtilen ID'ye sahip öğrenci bulunamadığı için güncelleme yapılamadı.", default(object))
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status404NotFound
                };
            }

            return new RoarResponse<object>(true, "Öğrenci başarıyla güncellendi.", default(object))
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(RoarResponse<object>), StatusCodes.Status200OK)]
        public RoarResponse<object> DeleteStudent(int id)
        {
            // Metodun başında 'isDeleted' kontrolü yapıldığı için
            // bu bloktan sonra her zaman bir değer döndürülmeli.
            // Önceki kodda sadece 'if' bloğu vardı, 'else' durumu veya doğrudan dönüş yoktu.
            var isDeleted = _studentService.DeleteStudent(id);

            if (!isDeleted)
            {
                return new RoarResponse<object>(false, "Belirtilen ID'ye sahip öğrenci bulunamadığı için silme işlemi yapılamadı.", default(object))
                {
                    ResponseType = RoarResponseCodeType.Error,
                    HttpStatusCode = StatusCodes.Status404NotFound
                };
            }
            // Başarılı durum: Artık her senaryoda bir return ifadesi var
            return new RoarResponse<object>(true, "Öğrenci başarıyla silindi.", default(object))
            {
                ResponseType = RoarResponseCodeType.Success,
                HttpStatusCode = StatusCodes.Status200OK
            };
        }
    }
}