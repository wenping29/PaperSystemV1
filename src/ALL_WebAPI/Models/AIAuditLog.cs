using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperSystemApi.Models
{
    public class AIAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ServiceType { get; set; } = string.Empty; // Assistant, Review, Scoring

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty; // GetWritingSuggestion, CheckGrammar, ScoreText, etc.

        [Column(TypeName = "json")]
        public string? RequestData { get; set; } // JSON serialized request

        [Column(TypeName = "json")]
        public string? ResponseData { get; set; } // JSON serialized response

        public int StatusCode { get; set; } // HTTP status code or custom status

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public long ProcessingTimeMs { get; set; } // Processing time in milliseconds

        public int TokenCount { get; set; } // Number of tokens used (if available)

        public decimal Cost { get; set; } // Cost in currency (if applicable)

        [MaxLength(50)]
        public string? ModelUsed { get; set; } // AI model used (e.g., gpt-4, chatglm3)

        [MaxLength(100)]
        public string? Provider { get; set; } // AI provider (e.g., OpenAI, Baidu, Mock)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Indexed fields for faster querying
        public DateTime? RequestDate { get; set; } // Date part for partitioning/queries

        [MaxLength(100)]
        public string? ClientIp { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Additional metadata
        [MaxLength(100)]
        public string? CorrelationId { get; set; } // For tracing requests across services

        [MaxLength(100)]
        public string? SessionId { get; set; }
    }

    public enum AIServiceType
    {
        Assistant,
        Review,
        Scoring
    }

    public enum AIRequestType
    {
        GetWritingSuggestion,
        GetWritingTemplate,
        GenerateContent,
        SummarizeText,
        TranslateText,
        GetWritingPrompts,
        CheckGrammar,
        AnalyzeStyle,
        CheckPlagiarism,
        PerformComprehensiveReview,
        GetWritingReport,
        ScoreText,
        GetRubric,
        CreateRubric,
        ScoreBatch,
        SubmitFeedback,
        GenerateGradingReport
    }
}