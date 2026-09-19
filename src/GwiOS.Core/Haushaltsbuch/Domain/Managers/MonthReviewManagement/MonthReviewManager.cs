using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement;

internal sealed class MonthReviewManager(
    IMonthReviewRepository monthReviewRepository,
    IMonthReviewValidator monthReviewValidator,
    TimeProvider timeProvider,
    ILogger<MonthReviewManager> logger) : IMonthReviewManager
{
    private readonly IMonthReviewRepository _monthReviewRepository = monthReviewRepository;
    private readonly IMonthReviewValidator _monthReviewValidator = monthReviewValidator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<MonthReviewManager> _logger = logger;

    public async Task<bool> IsReviewedAsync(DateOnly month, CancellationToken cancellationToken = default)
        => await _monthReviewRepository.ExistsAsync(month, cancellationToken);

    public async Task MarkReviewedAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        MonthReview monthReview = new() { Month = month, ReviewedAt = _timeProvider.GetUtcNow().UtcDateTime };
        if (!_monthReviewValidator.IsValid(monthReview, out List<string> validationErrors))
        {
            throw new MonthReviewValidationException(month, validationErrors);
        }

        await _monthReviewRepository.UpsertAsync(monthReview, cancellationToken);
        _logger.LogInformation("Month marked as reviewed.", new Dictionary<string, string> { ["Month"] = month.ToString("O") });
    }
}
