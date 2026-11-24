using ProjektReklamelyd.Models;

namespace ProjektReklamelyd.Repositories.Interfaces
{
    public interface IMusikgenreRepository
    {
        List<Musikgenre> GetAll();
        void Add(Musikgenre musikgenre);
    }
}
