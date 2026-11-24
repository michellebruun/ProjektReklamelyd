namespace ProjektReklamelyd.Models
{
    // Musik "har genre": præcis én musikgenre (se domænemodellen)
    public class Musik : Lydfil
    {
        public Musikgenre? Genre { get; set; }

        public override string Type
        {
            get { return "Musik"; }
        }

        public override string Kategori
        {
            get
            {
                if (Genre == null)
                {
                    return "";
                }
                return Genre.Navn;
            }
        }
    }
}
