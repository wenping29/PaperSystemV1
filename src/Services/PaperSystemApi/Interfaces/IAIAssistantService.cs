using System.Collections.Generic;
using System.Threading.Tasks;
using PaperSystemApi.DTOs;

namespace PaperSystemApi.Interfaces
{
    public interface IAIAssistantService
    {
        Task<AIAssistantResponse> GetWritingSuggestionAsync(AIAssistantRequest request);

        Task<WritingTemplateResponse> GetWritingTemplateAsync(WritingTemplateRequest request);

        Task<ContentGenerationResponse> GenerateContentAsync(ContentGenerationRequest request);

        Task<string> SummarizeTextAsync(string text, int maxLength = 200);

        Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);

        Task<List<string>> GetWritingPromptsAsync(string category = "general", int count = 5);
    }
}