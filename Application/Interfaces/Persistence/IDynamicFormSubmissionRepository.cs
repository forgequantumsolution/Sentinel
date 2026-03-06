using Analytics_BE.Core.Entities;
using Application.Common.Pagination;

namespace Analytics_BE.Application.Interfaces.Persistence
{
    public interface IDynamicFormSubmissionRepository
    {
        Task<DynamicFormSubmission?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicFormSubmission>> GetByFormIdAsync(Guid formId, PageRequest pageRequest);
        Task AddAsync(DynamicFormSubmission submission);
    }
}
