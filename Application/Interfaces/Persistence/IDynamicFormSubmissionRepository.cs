using Core.Entities;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicFormSubmissionRepository
    {
        Task<DynamicFormSubmission?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicFormSubmission>> GetByFormIdAsync(Guid formId, PageRequest pageRequest);
        Task AddAsync(DynamicFormSubmission submission);
        Task UpdateAsync(DynamicFormSubmission submission);
        Task DeleteAsync(Guid id);
    }
}
