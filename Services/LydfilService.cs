using ProjektReklamelyd.Models;
using ProjektReklamelyd.Repositories;
using ProjektReklamelyd.Repositories.Interfaces;
using ProjektReklamelyd.Services.Interfaces;
using System.IO;

namespace ProjektReklamelyd.Services
{
    // Servicen holder reglerne for funktion 1 og bruger repositories til at snakke med databasen
    public class LydfilService : ILydfilService
    {
        private ILydfilRepository _lydfilRepository;
        private IMusikgenreRepository _musikgenreRepository;
        private IEmneRepository _emneRepository;

        // De formater, som programmet kan afspille
        private List<string> _tilladteFormater = new List<string> { "mp3", "wav", "wma", "m4a" };

        public LydfilService()
        {
            _lydfilRepository = new LydfilRepository();
            _musikgenreRepository = new MusikgenreRepository();
            _emneRepository = new EmneRepository();
        }

        public List<Lydfil> GetAllLydfiler()
        {
            return _lydfilRepository.GetAll();
        }

        public List<Musikgenre> GetAllMusikgenrer()
        {
            return _musikgenreRepository.GetAll();
        }

        public List<Emne> GetAllEmner()
        {
            return _emneRepository.GetAll();
        }

        // Tjekker reglerne, før en lydfil gemmes.
        // Returnerer en fejlbesked - eller en tom tekst ("") hvis alt er i orden.
        public string ValidateLydfil(Lydfil lydfil)
        {
            if (lydfil.Filsti == "")
            {
                return "Vælg en lydfil på computeren.";
            }
            if (lydfil.Titel == "")
            {
                return "Skriv en titel.";
            }
            if (lydfil.Varighed <= 0)
            {
                return "Varigheden skal være et helt antal sekunder, der er større end 0.";
            }
            if (!_tilladteFormater.Contains(lydfil.Format))
            {
                return "Formatet \"" + lydfil.Format + "\" understøttes ikke. Brug mp3, wav, wma eller m4a.";
            }

            // En ny lydfil skal findes på computeren
            if (lydfil.LydfilId == 0 && !File.Exists(lydfil.Filsti))
            {
                return "Filen findes ikke: " + lydfil.Filsti;
            }

            // Den samme fil må ikke registreres to gange
            foreach (Lydfil andenLydfil in _lydfilRepository.GetAll())
            {
                if (andenLydfil.LydfilId != lydfil.LydfilId && andenLydfil.Filsti.ToLower() == lydfil.Filsti.ToLower())
                {
                    return "Filen er allerede registreret som \"" + andenLydfil.Titel + "\".";
                }
            }

            // Reglerne fra domænemodellen
            if (lydfil is Musik musik && musik.Genre == null)
            {
                return "Musik skal have en musikgenre.";
            }
            if (lydfil is Reklame reklame && reklame.Emner.Count == 0)
            {
                return "En reklame skal handle om mindst ét emne.";
            }
            if (lydfil is Taleindslag taleindslag && taleindslag.Emner.Count == 0)
            {
                return "Et taleindslag skal handle om mindst ét emne.";
            }

            return "";
        }

        // Gemmer lydfilen: en ny lydfil (id = 0) bliver tilføjet, ellers bliver den opdateret
        public void SaveLydfil(Lydfil lydfil)
        {
            if (lydfil.LydfilId == 0)
            {
                _lydfilRepository.Add(lydfil);
            }
            else
            {
                _lydfilRepository.Update(lydfil);
            }
        }

        public void DeleteLydfil(int lydfilId)
        {
            _lydfilRepository.Delete(lydfilId);
        }

        // Opretter en ny musikgenre. Findes genren allerede, bruges den eksisterende.
        public Musikgenre AddMusikgenre(string navn)
        {
            foreach (Musikgenre genre in _musikgenreRepository.GetAll())
            {
                if (genre.Navn.ToLower() == navn.ToLower())
                {
                    return genre;
                }
            }

            Musikgenre nyGenre = new Musikgenre();
            nyGenre.Navn = navn;
            _musikgenreRepository.Add(nyGenre);
            return nyGenre;
        }

        // Opretter et nyt emne. Findes emnet allerede, bruges det eksisterende.
        public Emne AddEmne(string navn)
        {
            foreach (Emne emne in _emneRepository.GetAll())
            {
                if (emne.Navn.ToLower() == navn.ToLower())
                {
                    return emne;
                }
            }

            Emne nytEmne = new Emne();
            nytEmne.Navn = navn;
            _emneRepository.Add(nytEmne);
            return nytEmne;
        }
    }
}
