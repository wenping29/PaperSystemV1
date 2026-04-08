using System.Threading.Tasks;
using PaperSystemApi.AI.DTOs;

namespace PaperSystemApi.Models
{
    public interface IApiClient
    {
        // AI Assistant 功能
        Task<AIAssistantResponse> GetWritingSuggestionAsync(AIAssistantRequest request);
        Task<WritingTemplateResponse> GetWritingTemplateAsync(WritingTemplateRequest request);
        Task<ContentGenerationResponse> GenerateContentAsync(ContentGenerationRequest request);
        Task<string> SummarizeTextAsync(string text, int maxLength = 200);
        Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
        Task<string[]> GetWritingPromptsAsync(string category = "general", int count = 5);

        // AI Review 功能（后续实现）
        Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request);
        Task<StyleAnalysisResponse> AnalyzeStyleAsync(StyleAnalysisRequest request);
        Task<PlagiarismCheckResponse> CheckPlagiarismAsync(PlagiarismCheckRequest request);
        Task<ComprehensiveReviewResponse> PerformComprehensiveReviewAsync(ComprehensiveReviewRequest request);

        // AI Scoring 功能（后续实现）
        Task<ScoringResponse> ScoreTextAsync(ScoringRequest request);
        Task<RubricDefinition> GetRubricAsync(string rubricId);
        Task<RubricDefinition> CreateRubricAsync(RubricDefinition rubric);
        Task<BatchScoringResponse> ScoreBatchAsync(BatchScoringRequest request);

        // 通用方法
        Task<bool> ValidateApiKeyAsync();
        Task<decimal> GetCostEstimateAsync(string model, int tokenCount);
    }
}