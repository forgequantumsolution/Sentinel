using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Core.Entities;

namespace Infrastructure.Services
{
    /// <summary>
    /// Background service that processes bulk upload jobs asynchronously.
    /// This prevents blocking the main thread during large bulk operations.
    /// </summary>
    public class BulkUploadBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BulkUploadBackgroundService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Cached count of available Field columns in DynamicFormRecord entity.
        /// Computed once using reflection and cached for performance.
        /// </summary>
        private static readonly Lazy<int> _maxColumnCount = new Lazy<int>(() =>
        {
            // Use reflection to count properties matching the pattern "Field{N}" in DynamicFormRecord
            var fieldPattern = new Regex(@"^Field\d+$", RegexOptions.Compiled);
            return typeof(DynamicFormRecord)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Count(p => fieldPattern.IsMatch(p.Name));
        });

        /// <summary>
        /// Gets the maximum number of columns available for field definitions.
        /// This is dynamically computed from DynamicFormRecord entity using reflection.
        /// </summary>
        public static int MaxColumnCount => _maxColumnCount.Value;

        public BulkUploadBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BulkUploadBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BulkUploadBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing bulk upload jobs.");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("BulkUploadBackgroundService is stopping.");
        }

        private async Task ProcessPendingJobsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IBulkUploadJobRepository>();
            var formRepository = scope.ServiceProvider.GetRequiredService<IDynamicFormRepository>();

            var pendingJobs = await jobRepository.GetPendingJobsAsync();

            foreach (var job in pendingJobs)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await ProcessJobAsync(job, jobRepository, formRepository, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(
            BulkUploadJob job,
            IBulkUploadJobRepository jobRepository,
            IDynamicFormRepository formRepository,
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing bulk upload job {JobId} with {TotalItems} items.", job.Id, job.TotalItems);

            // Mark job as processing
            job.Status = BulkUploadJobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            await jobRepository.UpdateAsync(job);

            var errors = new List<BulkUploadErrorDetail>();

            try
            {
                var dto = JsonSerializer.Deserialize<BulkCreateDynamicFormDto>(job.PayloadJson);
                if (dto?.Forms == null || dto.Forms.Count == 0)
                {
                    job.Status = BulkUploadJobStatus.Failed;
                    job.ErrorDetails = "No forms found in payload.";
                    job.CompletedAt = DateTime.UtcNow;
                    await jobRepository.UpdateAsync(job);
                    return;
                }

                for (int i = 0; i < dto.Forms.Count; i++)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Bulk upload job {JobId} was cancelled.", job.Id);
                        break;
                    }

                    var formDto = dto.Forms[i];

                    try
                    {
                        // Validate field count doesn't exceed column count (dynamically determined via reflection)
                        var fieldDefinitionCount = formDto.FieldDefinitions?.Count ?? 0;
                        if (fieldDefinitionCount > MaxColumnCount)
                        {
                            throw new InvalidOperationException(
                                $"Field definition count ({fieldDefinitionCount}) exceeds maximum allowed columns ({MaxColumnCount}). " +
                                $"Please reduce the number of field definitions to {MaxColumnCount} or fewer.");
                        }

                        var form = new DynamicForm
                        {
                            Name = formDto.Name,
                            Description = formDto.Description,
                            ConfigJson = formDto.ConfigJson,
                            IsActive = formDto.IsActive,
                            CreatedAt = DateTime.UtcNow,
                            CreatedById = job.CreatedById,
                            OrganizationId = job.OrganizationId,
                            FieldDefinitions = formDto.FieldDefinitions?.Select((fd, index) => new DynamicFormFieldDefinition
                            {
                                // Auto-map to DynamicFormRecord columns (Field1, Field2, etc.) based on index
                                ColumnName = string.IsNullOrWhiteSpace(fd.ColumnName) 
                                    ? $"Field{index + 1}" 
                                    : fd.ColumnName,
                                FieldName = fd.FieldName,
                                FieldType = fd.FieldType,
                                IsRequired = fd.IsRequired,
                                ValidationRules = fd.ValidationRules,
                                OrganizationId = job.OrganizationId
                            }).ToList() ?? new List<DynamicFormFieldDefinition>()
                        };

                        await formRepository.AddAsync(form);
                        job.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create form at index {Index} in job {JobId}.", i, job.Id);
                        job.FailureCount++;
                        errors.Add(new BulkUploadErrorDetail
                        {
                            Index = i,
                            FormName = formDto.Name,
                            Error = ex.Message
                        });
                    }

                    job.ProcessedItems++;

                    // Update progress periodically (every 10 items or at the end)
                    if (job.ProcessedItems % 10 == 0 || job.ProcessedItems == job.TotalItems)
                    {
                        await jobRepository.UpdateAsync(job);
                    }
                }

                // Determine final status
                if (job.FailureCount == 0)
                {
                    job.Status = BulkUploadJobStatus.Completed;
                }
                else if (job.SuccessCount == 0)
                {
                    job.Status = BulkUploadJobStatus.Failed;
                }
                else
                {
                    job.Status = BulkUploadJobStatus.PartiallyCompleted;
                }

                if (errors.Count > 0)
                {
                    job.ErrorDetails = JsonSerializer.Serialize(errors);
                }

                job.CompletedAt = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);

                _logger.LogInformation(
                    "Completed bulk upload job {JobId}. Success: {SuccessCount}, Failed: {FailureCount}",
                    job.Id, job.SuccessCount, job.FailureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error processing bulk upload job {JobId}.", job.Id);
                job.Status = BulkUploadJobStatus.Failed;
                job.ErrorDetails = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                await jobRepository.UpdateAsync(job);
            }
        }
    }

    /// <summary>
    /// Detail of an error that occurred during bulk upload processing.
    /// </summary>
    public class BulkUploadErrorDetail
    {
        public int Index { get; set; }
        public string? FormName { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
