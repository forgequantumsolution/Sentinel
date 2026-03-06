using System.Threading.Tasks;
using Core.Models;

namespace Application.Interfaces.Services
{
    public interface IDataEntryService
    {
        Task<bool> InsertDataAsync(DataEntryRequest request);
    }
}
