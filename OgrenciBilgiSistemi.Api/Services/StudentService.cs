#region Kütüphane Referansları
using Microsoft.Data.SqlClient;
using StudentTrackingSystem.Api.Models; // Doğru referans: .Api.Models
using System.Data;
#endregion

namespace StudentTrackingSystem.Api.Services
{
    #region Öğrenci ve Yoklama Servisi
    public class StudentService
    {
        #region Özel Alanlar ve Başlatıcı
        private readonly string _connectionString;

        public StudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion

        #region Öğrenci Listeleme Görevi
        public async Task<List<Student>> GetStudentsByClassIdAsync(int classId)
        {
            var students = new List<Student>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"SELECT OgrenciId, OgrenciAdSoyad, OgrenciGorsel
                                     FROM Ogrenciler 
                                     WHERE BirimId = @classId AND OgrenciDurum = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string rawFileName = reader["OgrenciGorsel"]?.ToString() ?? string.Empty;
                                string finalPath = string.IsNullOrEmpty(rawFileName) ? "user_icon.png" : rawFileName;

                                students.Add(new Student
                                {
                                    Id = (int)reader["OgrenciId"],
                                    FullName = reader["OgrenciAdSoyad"].ToString() ?? string.Empty,
                                    ImagePath = finalPath
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Öğrenci Listesi Hatası: {ex.Message}");
            }
            return students;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            Student student = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"SELECT OgrenciId, OgrenciAdSoyad, OgrenciGorsel, BirimId, ParentId 
                                     FROM Ogrenciler 
                                     WHERE OgrenciId = @studentId AND OgrenciDurum = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                student = new Student
                                {
                                    Id = (int)reader["OgrenciId"],
                                    FullName = reader["OgrenciAdSoyad"].ToString(),
                                    ImagePath = reader["OgrenciGorsel"]?.ToString(),
                                    UnitId = reader["BirimId"] as int?,
                                    ParentId = reader["ParentId"] as int?
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Öğrenci Getirme Hatası: {ex.Message}");
            }
            return student;
        }
        #endregion

        #region Sınıf Yoklama Görevleri
        public async Task<Dictionary<int, int>> GetExistingAttendanceAsync(int classId, int lessonNumber)
        {
            var attendanceDict = new Dictionary<int, int>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string dersColumn = $"Ders{lessonNumber}";
                    string query = $@"
                        SELECT SY.OgrenciId, SY.{dersColumn} 
                        FROM SinifYoklama SY 
                        INNER JOIN Ogrenciler O ON SY.OgrenciId = O.OgrenciId 
                        WHERE O.BirimId = @classId 
                        AND CAST(SY.OlusturulmaTarihi AS DATE) = CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@classId", classId);
                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (reader[dersColumn] != DBNull.Value)
                                {
                                    attendanceDict.Add((int)reader["OgrenciId"], (int)reader[dersColumn]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Yoklama Çekme Hatası: {ex.Message}");
            }
            return attendanceDict;
        }

        public async Task SaveBulkAttendanceAsync(IEnumerable<(int StudentId, int StatusId)> attendanceData, int classId, int teacherId, int lessonNumber)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string dersColumn = $"Ders{lessonNumber}";
                        foreach (var record in attendanceData)
                        {
                            string query = $@"
                        DECLARE @Bugun DATE = CAST(GETDATE() AS DATE);
                        MERGE INTO SinifYoklama AS target
                        USING (SELECT @studentId AS OgrenciId) AS source
                        ON (target.OgrenciId = source.OgrenciId AND CAST(target.OlusturulmaTarihi AS DATE) = @Bugun)
                        WHEN MATCHED THEN
                            UPDATE SET {dersColumn} = @statusId, GuncellenmeTarihi = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT (OgrenciId, OgretmenId, {dersColumn}, OlusturulmaTarihi)
                            VALUES (@studentId, @teacherId, @statusId, GETDATE());";

                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@studentId", record.StudentId);
                                cmd.Parameters.AddWithValue("@statusId", record.StatusId);
                                cmd.Parameters.AddWithValue("@teacherId", teacherId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"SQL Kayıt Hatası: {ex.Message}");
                    }
                }
            }
        }
        #endregion

        #region Öğrenci Detay ve Geçmiş
        public async Task<Dictionary<string, string>> GetStudentFullDetailsAsync(int studentId)
        {
            var details = new Dictionary<string, string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            s.OgrenciAdSoyad, s.OgrenciNo, s.OgrenciKartNo, s.OgrenciGorsel,
                            u.BirimAd,
                            p.VeliAdSoyad, p.VeliTelefon, p.VeliEmail, p.VeliMeslek, p.VeliIsYeri, p.VeliAdres,
                            t.OgretmenAdSoyad, srv.Plaka
                        FROM Ogrenciler s
                        LEFT JOIN Birimler u ON s.BirimId = u.BirimId
                        LEFT JOIN OgrenciVeliler p ON s.OgrenciVeliId = p.OgrenciVeliId
                        LEFT JOIN Ogretmenler t ON s.OgretmenId = t.OgretmenId
                        LEFT JOIN Servisler srv ON srv.ServisId = s.ServisId
                        WHERE s.OgrenciId = @studentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@studentId", studentId);
                        await connection.OpenAsync();

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                details.Add("StudentName", reader["OgrenciAdSoyad"]?.ToString() ?? "Bilinmiyor");
                                details.Add("StudentNo", reader["OgrenciNo"]?.ToString() ?? "-");
                                details.Add("CardNo", reader["OgrenciKartNo"]?.ToString() ?? "-");

                                string rawFileName = reader["OgrenciGorsel"]?.ToString() ?? "";
                                details.Add("ImagePath", string.IsNullOrEmpty(rawFileName) ? "user_icon.png" : rawFileName.Trim().ToLower());

                                details.Add("ClassName", reader["BirimAd"]?.ToString() ?? "Atanmamış");
                                details.Add("ParentName", reader["VeliAdSoyad"]?.ToString() ?? "Belirtilmemiş");
                                details.Add("ParentPhone", reader["VeliTelefon"]?.ToString() ?? "-");
                                details.Add("ParentEmail", reader["VeliEmail"]?.ToString() ?? "-");
                                details.Add("ParentJob", reader["VeliMeslek"]?.ToString() ?? "-");
                                details.Add("ParentWork", reader["VeliIsYeri"]?.ToString() ?? "-");
                                details.Add("Address", reader["VeliAdres"]?.ToString() ?? "-");
                                details.Add("TeacherName", reader["OgretmenAdSoyad"]?.ToString() ?? "Atanmamış");
                                details.Add("PlateNumber", reader["Plaka"]?.ToString() ?? "Kullanmıyor");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Detay Getirme Hatası: {ex.Message}");
            }
            return details;
        }

        public async Task<List<ClassAttendance>> GetStudentWeeklyAttendanceAsync(int studentId, DateTime start, DateTime end)
        {
            var records = new List<ClassAttendance>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            SinifYoklamaId, OgrenciId, OgretmenId,
                            Ders1, Ders2, Ders3, Ders4, Ders5, Ders6, Ders7, Ders8,
                            OlusturulmaTarihi
                        FROM SinifYoklama 
                        WHERE OgrenciId = @studentId 
                        AND CAST(OlusturulmaTarihi AS DATE) >= @start 
                        AND CAST(OlusturulmaTarihi AS DATE) <= @end 
                        ORDER BY OlusturulmaTarihi ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        cmd.Parameters.AddWithValue("@start", start.Date);
                        cmd.Parameters.AddWithValue("@end", end.Date);

                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                records.Add(new ClassAttendance
                                {
                                    AttendanceId = (int)reader["SinifYoklamaId"],
                                    StudentId = (int)reader["OgrenciId"],
                                    TeacherId = (int)reader["OgretmenId"],
                                    Lesson1 = reader["Ders1"] as int?,
                                    Lesson2 = reader["Ders2"] as int?,
                                    Lesson3 = reader["Ders3"] as int?,
                                    Lesson4 = reader["Ders4"] as int?,
                                    Lesson5 = reader["Ders5"] as int?,
                                    Lesson6 = reader["Ders6"] as int?,
                                    Lesson7 = reader["Ders7"] as int?,
                                    Lesson8 = reader["Ders8"] as int?,
                                    CreatedAt = Convert.ToDateTime(reader["OlusturulmaTarihi"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Haftalık Yoklama Hatası: {ex.Message}");
            }
            return records;
        }
        #endregion
    }
    #endregion
}