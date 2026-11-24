namespace ProjektReklamelyd.Models
{
    // En reklame "handler om" et eller flere emner (se domænemodellen)
    public class Reklame : Lydfil
    {
        public List<Emne> Emner { get; set; } = new List<Emne>();

        public override string Type
        {
            get { return "Reklame"; }
        }

        // string.Join sætter emnernes navne sammen med komma, fx "Shopping, Tilbud"
        public override string Kategori
        {
            get { return string.Join(", ", Emner); }
        }
    }
}
