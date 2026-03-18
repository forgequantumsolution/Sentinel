using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicFormDraftRepository
    {
        Task<DynamicFormDraft?> GetByFormAndUserAsync(Guid formId, Guid userId);
        Task AddAsync(DynamicFormDraft draft);
        Task UpdateAsync(DynamicFormDraft draft);
        Task DeleteAsync(Guid id);
    }
}
