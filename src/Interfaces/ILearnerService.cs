using Lisa.Models.Entities;

namespace Lisa.Interfaces;

public interface ILearnerService
{
    Task<bool> DisableLearnerAsync(Guid learnerId, string reason);
    Task<bool> EnableLearnerAsync(Guid learnerId);
    Task<IEnumerable<Learner>> GetDisabledLearnersAsync();
    Task<IEnumerable<Learner>> GetAllLearnersIncludingDisabledAsync();
    Task<Learner?> GetByIdIncludingDisabledAsync(Guid id);
}
