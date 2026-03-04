using Analytics_BE.Core.Entities;

namespace Analytics_BE.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
