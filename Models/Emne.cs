namespace ProjektReklamelyd.Models
{
    // Et emne, som en reklame eller et taleindslag kan handle om, fx "Shopping" eller "Sport"
    public class Emne
    {
        public int EmneId { get; set; }
        public string Navn { get; set; } = "";

        // ToString bestemmer, hvordan emnet vises som tekst
        public override string ToString()
        {
            return Navn;
        }
    }
}
