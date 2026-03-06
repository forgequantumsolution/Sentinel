using System.Threading.Tasks;
using Core.Models;

namespace Application.Interfaces.Services
{
    public interface IGraphService
    {
        Task<GraphData> GetGraphDataAsync(GraphConfigRequest config);
    }
}
