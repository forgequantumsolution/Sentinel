using Core.Entities;

namespace Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
