namespace ProjektReklamelyd.Models
{
    // Et taleindslag "handler om" et eller flere emner (se domænemodellen)
    public class Taleindslag : Lydfil
    {
        public List<Emne> Emner { get; set; } = new List<Emne>();

        public override string Type
        {
            get { return "Taleindslag"; }
        }

        // string.Join sætter emnernes navne sammen med komma, fx "Sport, Nyheder"
        public override string Kategori
        {
            get { return string.Join(", ", Emner); }
        }
    }
}
