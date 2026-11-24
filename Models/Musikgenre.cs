namespace ProjektReklamelyd.Models
{
    // En musikgenre, fx "Pop", "Rock" eller "Jazz"
    public class Musikgenre
    {
        public int MusikgenreId { get; set; }
        public string Navn { get; set; } = "";

        // ToString bestemmer, hvordan genren vises som tekst
        public override string ToString()
        {
            return Navn;
        }
    }
}
