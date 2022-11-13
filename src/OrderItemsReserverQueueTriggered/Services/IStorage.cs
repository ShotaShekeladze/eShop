using System.IO;
using System.Threading.Tasks;

namespace OrderItemsReserverQueueTriggered.Services;

public interface IStorage
{
    Task Save(Stream fileStream, string name);
}
