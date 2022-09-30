using System.IO;
using System.Threading.Tasks;

namespace OrderItemsReserver.Services;
public interface IStorage
{
    Task Save(Stream fileStream, string name);
}
