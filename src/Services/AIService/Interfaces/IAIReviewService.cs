using System.Threading.Tasks;
using AIService.DTOs;

namespace AIService.Interfaces
{
    public interface IAIReviewService
    {
        Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request);

        Task<StyleAnalysisResponse> AnalyzeStyleAsync(StyleAnalysisRequest request);

        Task<PlagiarismCheckResponse> CheckPlagiarismAsync(PlagiarismCheckRequest request);

        Task<ComprehensiveReviewResponse> PerformComprehensiveReviewAsync(ComprehensiveReviewRequest request);

        Task<string> GetWritingReportAsync(string text, string reportType = "basic");
    }
}