namespace ProjektReklamelyd.Models
{
    // En jingle har hverken genre eller emner
    public class Jingle : Lydfil
    {
        public override string Type
        {
            get { return "Jingle"; }
        }

        public override string Kategori
        {
            get { return "-"; }
        }
    }
}
