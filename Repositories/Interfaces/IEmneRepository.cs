using ProjektReklamelyd.Models;

namespace ProjektReklamelyd.Repositories.Interfaces
{
    public interface IEmneRepository
    {
        List<Emne> GetAll();
        void Add(Emne emne);
    }
}
