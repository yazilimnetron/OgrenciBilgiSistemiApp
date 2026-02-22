using Microsoft.Data.SqlClient;
using StudentTrackingSystem.Api.Models;

namespace StudentTrackingSystem.Api.Services
{
    public class LoginService
    {
        private readonly string _connectionString;

        // Bağlantı cümlesini appsettings.json'dan alacak şekilde kurucu metot
        public LoginService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            K.KullaniciId,
                            K.KullaniciAdi,
                            K.BirimId,
                            K.KullaniciDurum,
                            K.Sifre
                        FROM Kullanicilar K
                        WHERE K.KullaniciAdi = @username
                          AND K.Sifre = @password
                          AND K.KullaniciDurum = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    Id = (int)reader["KullaniciId"],
                                    Username = reader["KullaniciAdi"].ToString(),
                                    UnitId = reader["BirimId"] != DBNull.Value ? (int)reader["BirimId"] : null,
                                    IsActive = Convert.ToBoolean(reader["KullaniciDurum"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // API tarafında loglama mekanizması buraya eklenebilir
                throw new Exception("Veritabanı bağlantı hatası: " + ex.Message);
            }

            return null;
        }
    }
}