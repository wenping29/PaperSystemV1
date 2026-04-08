using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.AI.DTOs
{
    public class AIAssistantRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Context { get; set; }

        public string? WritingStyle { get; set; }

        public string? Language { get; set; } = "zh";

        public int MaxLength { get; set; } = 500;

        public decimal Creativity { get; set; } = 0.7m;

        public bool IncludeExamples { get; set; } = true;
    }

    public class AIAssistantResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public string Suggestion { get; set; } = string.Empty;

        public List<string> AlternativePhrases { get; set; } = new List<string>();

        public List<string> ContentExpansions { get; set; } = new List<string>();

        public string? StyleAnalysis { get; set; }

        public decimal ConfidenceScore { get; set; }

        public string ProcessingTime { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class WritingTemplateRequest
    {
        [Required]
        public string TemplateType { get; set; } = string.Empty;

        public Dictionary<string, string>? Parameters { get; set; }

        public string? Language { get; set; } = "zh";
    }

    public class WritingTemplateResponse
    {
        public string TemplateType { get; set; } = string.Empty;

        public string Structure { get; set; } = string.Empty;

        public string Example { get; set; } = string.Empty;

        public List<string> KeyElements { get; set; } = new List<string>();

        public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();

        public string? Tips { get; set; }
    }

    public class ContentGenerationRequest
    {
        [Required]
        public string Topic { get; set; } = string.Empty;

        public string? Outline { get; set; }

        public string? Keywords { get; set; }

        public string? TargetAudience { get; set; }

        public string? Tone { get; set; }

        public string? Language { get; set; } = "zh";

        public int WordCount { get; set; } = 300;
    }

    public class ContentGenerationResponse
    {
        public string Topic { get; set; } = string.Empty;

        public string GeneratedContent { get; set; } = string.Empty;

        public List<string> KeyPoints { get; set; } = new List<string>();

        public string? StructureAnalysis { get; set; }

        public decimal QualityScore { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}