namespace ProjektReklamelyd.Models
{
    // Lydfil er en abstrakt klasse: man kan ikke lave "bare en lydfil".
    // En lydfil er altid enten Musik, Reklame, Taleindslag eller Jingle (se domænemodellen).
    public abstract class Lydfil
    {
        public int LydfilId { get; set; }
        public string Titel { get; set; } = "";
        public string Filsti { get; set; } = "";
        public int Varighed { get; set; }          // i hele sekunder
        public string Format { get; set; } = "";   // fx "mp3" eller "wav"

        // Hver underklasse fortæller selv, hvilken type den er ("Musik", "Reklame" osv.)
        public abstract string Type { get; }

        // Hver underklasse viser selv sin kategori som tekst (genre eller emner)
        public abstract string Kategori { get; }

        // Varigheden vist som minutter:sekunder - fx bliver 215 sekunder til "3:35"
        public string VarighedTekst
        {
            get
            {
                int minutter = Varighed / 60;
                int sekunder = Varighed % 60;
                return minutter + ":" + sekunder.ToString("00");
            }
        }
    }
}
