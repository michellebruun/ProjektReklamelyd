using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjektReklamelyd.Models;
using ProjektReklamelyd.Repositories.Interfaces;

namespace ProjektReklamelyd.Repositories
{
    // Henter og gemmer lydfiler i databasen (tabellerne Lydfil og LydfilEmne)
    public class LydfilRepository : ILydfilRepository
    {
        private readonly string _connectionString;

        public LydfilRepository()
        {
            // Connection string hentes fra appsettings.json
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public List<Lydfil> GetAll()
        {
            List<Lydfil> lydfiler = new List<Lydfil>();

            // LEFT JOIN henter også genrens navn. Kun Musik har en genre - for de andre er den NULL.
            string query = "SELECT Lydfil.LydfilId, Lydfil.Titel, Lydfil.Filsti, Lydfil.Varighed, Lydfil.Format, " +
                           "Lydfil.LydfilType, Lydfil.MusikgenreId, Musikgenre.Navn AS GenreNavn " +
                           "FROM Lydfil LEFT JOIN Musikgenre ON Lydfil.MusikgenreId = Musikgenre.MusikgenreId " +
                           "ORDER BY Lydfil.Titel";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lydfil lydfil = LavLydfil(reader);
                        lydfiler.Add(lydfil);
                    }
                }
            }

            // Reklamer og taleindslag skal også have deres emner med
            foreach (Lydfil lydfil in lydfiler)
            {
                if (lydfil is Reklame reklame)
                {
                    reklame.Emner = HentEmner(reklame.LydfilId);
                }
                else if (lydfil is Taleindslag taleindslag)
                {
                    taleindslag.Emner = HentEmner(taleindslag.LydfilId);
                }
            }

            return lydfiler;
        }

        public void Add(Lydfil lydfil)
        {
            // OUTPUT INSERTED.LydfilId giver os det nye id, som databasen har lavet
            string query = "INSERT INTO Lydfil (Titel, Filsti, Varighed, Format, LydfilType, MusikgenreId) " +
                           "OUTPUT INSERTED.LydfilId " +
                           "VALUES (@Titel, @Filsti, @Varighed, @Format, @LydfilType, @MusikgenreId)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                TilfoejParametre(command, lydfil);
                connection.Open();
                lydfil.LydfilId = Convert.ToInt32(command.ExecuteScalar());
            }

            GemEmner(lydfil);
        }

        public void Update(Lydfil lydfil)
        {
            string query = "UPDATE Lydfil SET Titel = @Titel, Filsti = @Filsti, Varighed = @Varighed, Format = @Format, " +
                           "LydfilType = @LydfilType, MusikgenreId = @MusikgenreId " +
                           "WHERE LydfilId = @LydfilId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                TilfoejParametre(command, lydfil);
                command.Parameters.AddWithValue("@LydfilId", lydfil.LydfilId);
                connection.Open();
                command.ExecuteNonQuery();
            }

            GemEmner(lydfil);
        }

        public void Delete(int lydfilId)
        {
            // Emnerne i LydfilEmne slettes først - ellers må vi ikke slette lydfilen (fremmednøgle)
            string query = "DELETE FROM LydfilEmne WHERE LydfilId = @LydfilId; " +
                           "DELETE FROM Lydfil WHERE LydfilId = @LydfilId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LydfilId", lydfilId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Laver det rigtige objekt (Musik, Reklame, Taleindslag eller Jingle) ud fra kolonnen LydfilType
        private Lydfil LavLydfil(SqlDataReader reader)
        {
            string type = (string)reader["LydfilType"];
            Lydfil lydfil;

            if (type == "Musik")
            {
                Musik musik = new Musik();
                if (reader["MusikgenreId"] != DBNull.Value)
                {
                    Musikgenre genre = new Musikgenre();
                    genre.MusikgenreId = (int)reader["MusikgenreId"];
                    genre.Navn = (string)reader["GenreNavn"];
                    musik.Genre = genre;
                }
                lydfil = musik;
            }
            else if (type == "Reklame")
            {
                lydfil = new Reklame();
            }
            else if (type == "Taleindslag")
            {
                lydfil = new Taleindslag();
            }
            else
            {
                lydfil = new Jingle();
            }

            // Det, som alle lydfiler har
            lydfil.LydfilId = (int)reader["LydfilId"];
            lydfil.Titel = (string)reader["Titel"];
            lydfil.Filsti = (string)reader["Filsti"];
            lydfil.Varighed = (int)reader["Varighed"];
            lydfil.Format = (string)reader["Format"];

            return lydfil;
        }

        // Henter de emner, som en reklame eller et taleindslag handler om
        private List<Emne> HentEmner(int lydfilId)
        {
            List<Emne> emner = new List<Emne>();
            string query = "SELECT Emne.EmneId, Emne.Navn FROM LydfilEmne " +
                           "JOIN Emne ON LydfilEmne.EmneId = Emne.EmneId " +
                           "WHERE LydfilEmne.LydfilId = @LydfilId " +
                           "ORDER BY Emne.Navn";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LydfilId", lydfilId);
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

        // Gemmer hvilke emner en reklame eller et taleindslag handler om (tabellen LydfilEmne)
        private void GemEmner(Lydfil lydfil)
        {
            List<Emne> emner = new List<Emne>();
            if (lydfil is Reklame reklame)
            {
                emner = reklame.Emner;
            }
            else if (lydfil is Taleindslag taleindslag)
            {
                emner = taleindslag.Emner;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Først slettes de gamle emner (vigtigt, når en lydfil bliver redigeret)
                SqlCommand sletCommand = new SqlCommand("DELETE FROM LydfilEmne WHERE LydfilId = @LydfilId", connection);
                sletCommand.Parameters.AddWithValue("@LydfilId", lydfil.LydfilId);
                sletCommand.ExecuteNonQuery();

                // Derefter gemmes de valgte emner
                foreach (Emne emne in emner)
                {
                    SqlCommand command = new SqlCommand("INSERT INTO LydfilEmne (LydfilId, EmneId) VALUES (@LydfilId, @EmneId)", connection);
                    command.Parameters.AddWithValue("@LydfilId", lydfil.LydfilId);
                    command.Parameters.AddWithValue("@EmneId", emne.EmneId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Parametrene som både Add og Update bruger
        private void TilfoejParametre(SqlCommand command, Lydfil lydfil)
        {
            command.Parameters.AddWithValue("@Titel", lydfil.Titel);
            command.Parameters.AddWithValue("@Filsti", lydfil.Filsti);
            command.Parameters.AddWithValue("@Varighed", lydfil.Varighed);
            command.Parameters.AddWithValue("@Format", lydfil.Format);
            command.Parameters.AddWithValue("@LydfilType", lydfil.Type);

            // Kun Musik har en genre. For de andre typer gemmes NULL i databasen.
            if (lydfil is Musik musik && musik.Genre != null)
            {
                command.Parameters.AddWithValue("@MusikgenreId", musik.Genre.MusikgenreId);
            }
            else
            {
                command.Parameters.AddWithValue("@MusikgenreId", DBNull.Value);
            }
        }
    }
}
