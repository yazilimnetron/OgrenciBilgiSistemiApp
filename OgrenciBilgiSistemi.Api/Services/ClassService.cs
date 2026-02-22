using Microsoft.Data.SqlClient;
using StudentTrackingSystem.Api.Models;

namespace StudentTrackingSystem.Api.Services
{
    public class ClassService
    {
        private readonly string _connectionString;

        public ClassService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<UnitWithCountDto>> GetAllClassesWithStudentCountAsync()
        {
            var resultList = new List<UnitWithCountDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT BirimId, BirimAd, BirimSinifMi, BirimDurum,
                           (SELECT COUNT(*) FROM Ogrenciler
                            WHERE BirimId = B.BirimId AND OgrenciDurum = 1) as OgrenciSayisi
                    FROM Birimler B
                    WHERE BirimSinifMi = 1 AND BirimDurum = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            resultList.Add(new UnitWithCountDto
                            {
                                UnitData = new Unit
                                {
                                    Id = (int)reader["BirimId"],
                                    Name = reader["BirimAd"].ToString(),
                                    IsClass = (bool)reader["BirimSinifMi"],
                                    IsActive = (bool)reader["BirimDurum"]
                                },
                                StudentCount = (int)reader["OgrenciSayisi"]
                            });
                        }
                    }
                }
            }
            return resultList;
        }
    }
}