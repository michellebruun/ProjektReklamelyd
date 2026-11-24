using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjektReklamelyd.Models;
using ProjektReklamelyd.Repositories.Interfaces;

namespace ProjektReklamelyd.Repositories
{
    // Henter og gemmer musikgenrer i databasen (tabellen Musikgenre)
    public class MusikgenreRepository : IMusikgenreRepository
    {
        private readonly string _connectionString;

        public MusikgenreRepository()
        {
            // Connection string hentes fra appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public List<Musikgenre> GetAll()
        {
            List<Musikgenre> genrer = new List<Musikgenre>();
            string query = "SELECT MusikgenreId, Navn FROM Musikgenre ORDER BY Navn";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Musikgenre genre = new Musikgenre();
                        genre.MusikgenreId = (int)reader["MusikgenreId"];
                        genre.Navn = (string)reader["Navn"];
                        genrer.Add(genre);
                    }
                }
            }

            return genrer;
        }

        public void Add(Musikgenre musikgenre)
        {
            // OUTPUT INSERTED.MusikgenreId giver os det nye id, som databasen har lavet
            string query = "INSERT INTO Musikgenre (Navn) OUTPUT INSERTED.MusikgenreId VALUES (@Navn)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Navn", musikgenre.Navn);
                connection.Open();
                musikgenre.MusikgenreId = Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
