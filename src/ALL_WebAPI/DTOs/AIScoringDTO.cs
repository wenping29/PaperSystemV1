using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.DTOs
{
    public class ScoringRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? Category { get; set; }

        public string? GradingRubric { get; set; }

        public string? Language { get; set; } = "zh";

        public bool IncludeDetailedFeedback { get; set; } = true;

        public bool IncludeComparativeAnalysis { get; set; } = false;
    }

    public class ScoringResponse
    {
        public string OriginalText { get; set; } = string.Empty;

        public decimal OverallScore { get; set; }

        public Dictionary<string, decimal> DimensionScores { get; set; } = new Dictionary<string, decimal>();

        public List<ScoringDimension> Dimensions { get; set; } = new List<ScoringDimension>();

        public string? OverallFeedback { get; set; }

        public List<ImprovementSuggestion> ImprovementSuggestions { get; set; } = new List<ImprovementSuggestion>();

        public string? Grade { get; set; }

        public decimal ConfidenceLevel { get; set; }

        public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
    }

    public class ScoringDimension
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal Weight { get; set; }

        public string? Feedback { get; set; }

        public List<string> Strengths { get; set; } = new List<string>();

        public List<string> Weaknesses { get; set; } = new List<string>();
    }

    public class ImprovementSuggestion
    {
        public string Area { get; set; } = string.Empty;

        public string CurrentStatus { get; set; } = string.Empty;

        public string Suggestion { get; set; } = string.Empty;

        public string ExpectedImprovement { get; set; } = string.Empty;

        public int Priority { get; set; } = 1;
    }

    public class RubricDefinition
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public List<RubricDimension> Dimensions { get; set; } = new List<RubricDimension>();

        public Dictionary<string, decimal> WeightDistribution { get; set; } = new Dictionary<string, decimal>();

        public string? GradeBoundaries { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class RubricDimension
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Weight { get; set; }

        public List<RubricCriterion> Criteria { get; set; } = new List<RubricCriterion>();
    }

    public class RubricCriterion
    {
        public string Level { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public List<string> Indicators { get; set; } = new List<string>();
    }

    public class BatchScoringRequest
    {
        [Required]
        public List<ScoringRequest> Texts { get; set; } = new List<ScoringRequest>();

        public string? RubricId { get; set; }

        public bool GenerateComparativeReport { get; set; } = false;
    }

    public class BatchScoringResponse
    {
        public List<ScoringResponse> Results { get; set; } = new List<ScoringResponse>();

        public BatchScoringSummary Summary { get; set; } = new BatchScoringSummary();

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class BatchScoringSummary
    {
        public int TotalTexts { get; set; }

        public decimal AverageScore { get; set; }

        public decimal HighestScore { get; set; }

        public decimal LowestScore { get; set; }

        public Dictionary<string, decimal> AverageDimensionScores { get; set; } = new Dictionary<string, decimal>();

        public string? ComparativeAnalysis { get; set; }
    }

    public class FeedbackRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public decimal Score { get; set; }

        public string? Feedback { get; set; }

        public bool IsAccurate { get; set; }

        public string? UserComment { get; set; }
    }

    public class FeedbackResponse
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}