using ProjektReklamelyd.Models;

namespace ProjektReklamelyd.Repositories.Interfaces
{
    public interface ILydfilRepository
    {
        List<Lydfil> GetAll();
        void Add(Lydfil lydfil);
        void Update(Lydfil lydfil);
        void Delete(int lydfilId);
    }
}
