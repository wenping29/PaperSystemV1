using System.Threading.Tasks;
using PaperSystemApi.AI.DTOs;

namespace PaperSystemApi.AI.Interfaces
{
    public interface IAIScoringService
    {
        Task<ScoringResponse> ScoreTextAsync(ScoringRequest request);

        Task<RubricDefinition> GetRubricAsync(string rubricId);

        Task<RubricDefinition> CreateRubricAsync(RubricDefinition rubric);

        Task<BatchScoringResponse> ScoreBatchAsync(BatchScoringRequest request);

        Task<FeedbackResponse> SubmitFeedbackAsync(FeedbackRequest request);

        Task<string> GenerateGradingReportAsync(string textId, string rubricId = "default");
    }
}