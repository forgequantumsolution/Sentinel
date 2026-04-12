using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IRuleFieldService
    {
        List<RuleFieldDto> GetRuleFields();
    }
}
