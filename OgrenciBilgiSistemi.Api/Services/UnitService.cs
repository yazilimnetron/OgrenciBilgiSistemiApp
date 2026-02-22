#region Kütüphane Referansları
using Microsoft.Data.SqlClient;
using StudentTrackingSystem.Api.Models;
using System;
using System.Threading.Tasks;
#endregion

namespace StudentTrackingSystem.Api.Services
{
    public class UnitService
    {
        #region Özel Alanlar ve Başlatıcı
        private readonly string _connectionString;

        // API standartlarına uygun olarak IConfiguration üzerinden bağlantı alıyoruz
        public UnitService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #endregion

        #region Birim Getirme Görevi
        /// <summary>
        /// Verilen ID'ye göre birimin ismini ve sınıf olup olmadığını getirir.
        /// </summary>
        public async Task<Unit> GetUnitByIdAsync(int unitId)
        {
            Unit unit = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT BirimId, BirimAd, BirimSinifMi FROM Birimler WHERE BirimId = @unitId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@unitId", unitId);
                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                unit = new Unit
                                {
                                    Id = (int)reader["BirimId"],
                                    Name = reader["BirimAd"].ToString() ?? string.Empty,
                                    IsClass = (bool)reader["BirimSinifMi"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimini API seviyesine taşıyoruz
                throw new Exception($"Birim bilgisi çekilemedi: {ex.Message}");
            }

            return unit;
        }
        #endregion
    }
}