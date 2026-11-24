using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjektReklamelyd.Models;
using ProjektReklamelyd.Repositories.Interfaces;

namespace ProjektReklamelyd.Repositories
{
    // Henter og gemmer emner i databasen (tabellen Emne)
    public class EmneRepository : IEmneRepository
    {
        private readonly string _connectionString;

        public EmneRepository()
        {
            // Connection string hentes fra appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public List<Emne> GetAll()
        {
            List<Emne> emner = new List<Emne>();
            string query = "SELECT EmneId, Navn FROM Emne ORDER BY Navn";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Emne emne = new Emne();
                        emne.EmneId = (int)reader["EmneId"];
                        emne.Navn = (string)reader["Navn"];
                        emner.Add(emne);
                    }
                }
            }

            return emner;
        }

        public void Add(Emne emne)
        {
            // OUTPUT INSERTED.EmneId giver os det nye id, som databasen har lavet
            string query = "INSERT INTO Emne (Navn) OUTPUT INSERTED.EmneId VALUES (@Navn)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Navn", emne.Navn);
                connection.Open();
                emne.EmneId = Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
