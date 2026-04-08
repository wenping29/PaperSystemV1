using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.AI.DTOs
{
    public class GrammarCheckRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Language { get; set; } = "zh";

        public bool CheckSpelling { get; set; } = true;

        public bool CheckPunctuation { get; set; } = true;

        public bool CheckGrammar { get; set; } = true;
    }

    public class GrammarCheckResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public string CorrectedText { get; set; } = string.Empty;

        public List<GrammarError> Errors { get; set; } = new List<GrammarError>();

        public int ErrorCount { get; set; }

        public decimal GrammarScore { get; set; }

        public string? Summary { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class GrammarError
    {
        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        public string SuggestedCorrection { get; set; } = string.Empty;

        public string Severity { get; set; } = "Medium";
    }

    public class StyleAnalysisRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? TargetStyle { get; set; }

        public string? Language { get; set; } = "zh";

        public bool AnalyzeReadability { get; set; } = true;

        public bool AnalyzeVocabulary { get; set; } = true;

        public bool AnalyzeTone { get; set; } = true;
    }

    public class StyleAnalysisResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public decimal ReadabilityScore { get; set; }

        public decimal FormalityScore { get; set; }

        public decimal VocabularyRichness { get; set; }

        public string? DetectedTone { get; set; }

        public List<StyleSuggestion> Suggestions { get; set; } = new List<StyleSuggestion>();

        public string? OverallAssessment { get; set; }

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    public class StyleSuggestion
    {
        public string Area { get; set; } = string.Empty;

        public string CurrentStatus { get; set; } = string.Empty;

        public string Suggestion { get; set; } = string.Empty;

        public string Impact { get; set; } = string.Empty;
    }

    public class PlagiarismCheckRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Author { get; set; }

        public bool CheckInternet { get; set; } = true;

        public bool CheckInternalDatabase { get; set; } = true;

        public decimal SimilarityThreshold { get; set; } = 0.8m;
    }

    public class PlagiarismCheckResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public decimal SimilarityScore { get; set; }

        public bool IsPlagiarized { get; set; }

        public List<PlagiarismMatch> Matches { get; set; } = new List<PlagiarismMatch>();

        public string? ReportUrl { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class PlagiarismMatch
    {
        public string Source { get; set; } = string.Empty;

        public string SourceUrl { get; set; } = string.Empty;

        public decimal SimilarityPercentage { get; set; }

        public string MatchedText { get; set; } = string.Empty;

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }
    }

    public class ComprehensiveReviewRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Context { get; set; }

        public string? TargetAudience { get; set; }

        public string? Purpose { get; set; }

        public bool IncludeGrammarCheck { get; set; } = true;

        public bool IncludeStyleAnalysis { get; set; } = true;

        public bool IncludePlagiarismCheck { get; set; } = false;
    }

    public class ComprehensiveReviewResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public GrammarCheckResponse? GrammarCheck { get; set; }

        public StyleAnalysisResponse? StyleAnalysis { get; set; }

        public PlagiarismCheckResponse? PlagiarismCheck { get; set; }

        public string? OverallFeedback { get; set; }

        public decimal OverallScore { get; set; }

        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    }
}