using ProjektReklamelyd.Models;

namespace ProjektReklamelyd.Services.Interfaces
{
    public interface ILydfilService
    {
        List<Lydfil> GetAllLydfiler();
        List<Musikgenre> GetAllMusikgenrer();
        List<Emne> GetAllEmner();

        string ValidateLydfil(Lydfil lydfil);
        void SaveLydfil(Lydfil lydfil);
        void DeleteLydfil(int lydfilId);

        Musikgenre AddMusikgenre(string navn);
        Emne AddEmne(string navn);
    }
}
