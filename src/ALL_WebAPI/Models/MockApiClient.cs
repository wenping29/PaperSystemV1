using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaperSystemApi.DTOs;

namespace PaperSystemApi.Models
{
    public class MockApiClient : IApiClient
    {
        private readonly Random _random = new Random();

        public async Task<AIAssistantResponse> GetWritingSuggestionAsync(AIAssistantRequest request)
        {
            await SimulateDelay();

            var suggestion = $"这里是对您文本的建议：'{request.Text}'。尝试使用更生动的词汇，并确保句子结构多样。";
            var alternativePhrases = new List<string>
            {
                "替代表达1",
                "替代表达2",
                "替代表达3"
            };
            var contentExpansions = new List<string>
            {
                "您可以扩展关于...的内容",
                "考虑添加一个例子来说明...",
                "引入一个相关的概念..."
            };

            return new AIAssistantResponse
            {
                OriginalText = request.Text,
                Suggestion = suggestion,
                AlternativePhrases = alternativePhrases,
                ContentExpansions = contentExpansions,
                StyleAnalysis = "文本风格较为正式，可以考虑加入一些个人化表达。",
                ConfidenceScore = 0.85m,
                ProcessingTime = "250ms",
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<WritingTemplateResponse> GetWritingTemplateAsync(WritingTemplateRequest request)
        {
            await SimulateDelay();

            var templateType = request.TemplateType.ToLower();
            var structure = templateType switch
            {
                "essay" => "引言 -> 正文（3个段落） -> 结论",
                "report" => "摘要 -> 引言 -> 方法 -> 结果 -> 讨论 -> 结论",
                "article" => "标题 -> 引子 -> 主体 -> 结尾",
                "email" => "称呼 -> 正文 -> 结尾敬语 -> 签名",
                _ => "引言 -> 主体 -> 结论"
            };

            var example = templateType switch
            {
                "essay" => "关于人工智能的论文示例...",
                "report" => "项目进度报告示例...",
                "article" => "科技文章示例...",
                "email" => "商务邮件示例...",
                _ => "通用写作示例..."
            };

            var keyElements = new List<string>
            {
                "清晰的主题陈述",
                "逻辑连贯的段落",
                "有力的结论"
            };

            var placeholders = new Dictionary<string, string>
            {
                { "title", "文章标题" },
                { "introduction", "引言内容" },
                { "body", "正文内容" },
                { "conclusion", "结论内容" }
            };

            return new WritingTemplateResponse
            {
                TemplateType = request.TemplateType,
                Structure = structure,
                Example = example,
                KeyElements = keyElements,
                Placeholders = placeholders,
                Tips = "确保每个部分都有明确的过渡句。"
            };
        }

        public async Task<ContentGenerationResponse> GenerateContentAsync(ContentGenerationRequest request)
        {
            await SimulateDelay();

            var generatedContent = $"这是关于'{request.Topic}'的生成内容。根据您提供的关键词'{request.Keywords}'和目标受众'{request.TargetAudience}'，我生成了以下内容：\n\n" +
                                  "人工智能正在改变我们的生活方式。从自动化到智能决策，AI技术已经在各个领域展现出巨大潜力。未来，随着技术的进一步发展，我们可以期待更多创新应用。";

            var keyPoints = new List<string>
            {
                "人工智能的定义和历史",
                "当前AI应用领域",
                "未来发展趋势",
                "伦理和社会影响"
            };

            return new ContentGenerationResponse
            {
                Topic = request.Topic,
                GeneratedContent = generatedContent,
                KeyPoints = keyPoints,
                StructureAnalysis = "内容结构完整，包含引言、主体和结论。",
                QualityScore = 0.88m,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<string> SummarizeTextAsync(string text, int maxLength = 200)
        {
            await SimulateDelay();

            if (text.Length <= maxLength)
                return text;

            var summary = text.Substring(0, Math.Min(maxLength, text.Length)) + "...";
            return $"摘要：{summary}";
        }

        public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            await SimulateDelay();

            return $"[{sourceLanguage}->{targetLanguage}] {text} (模拟翻译)";
        }

        public async Task<string[]> GetWritingPromptsAsync(string category = "general", int count = 5)
        {
            await SimulateDelay();

            var prompts = new List<string>
            {
                "写一篇关于未来科技的短文",
                "描述一个你最喜欢的季节",
                "人工智能对社会的影响",
                "一次难忘的旅行经历",
                "如何保持工作与生活的平衡",
                "环境保护的重要性",
                "数字时代的隐私问题",
                "传统文化的现代价值"
            };

            return prompts.Take(count).ToArray();
        }

        // AI Review 功能 - 模拟实现
        public async Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request)
        {
            await SimulateDelay();

            var errors = new List<GrammarError>();
            if (_random.NextDouble() > 0.5)
            {
                errors.Add(new GrammarError
                {
                    Type = "Spelling",
                    Description = "可能的拼写错误",
                    StartPosition = 10,
                    EndPosition = 15,
                    SuggestedCorrection = "建议的修正",
                    Severity = "Low"
                });
            }

            return new GrammarCheckResponse
            {
                OriginalText = request.Text,
                CorrectedText = request.Text + " (已校正)",
                Errors = errors,
                ErrorCount = errors.Count,
                GrammarScore = 0.92m,
                Summary = "文本语法基本正确，有少量建议改进",
                CheckedAt = DateTime.UtcNow
            };
        }

        public async Task<StyleAnalysisResponse> AnalyzeStyleAsync(StyleAnalysisRequest request)
        {
            await SimulateDelay();

            return new StyleAnalysisResponse
            {
                OriginalText = request.Text,
                ReadabilityScore = 0.78m,
                FormalityScore = 0.65m,
                VocabularyRichness = 0.72m,
                DetectedTone = "中性",
                Suggestions = new List<StyleSuggestion>
                {
                    new StyleSuggestion
                    {
                        Area = "句子长度",
                        CurrentStatus = "句子偏长",
                        Suggestion = "尝试使用更短的句子",
                        Impact = "提高可读性"
                    }
                },
                OverallAssessment = "文本风格适中，可读性良好",
                AnalyzedAt = DateTime.UtcNow
            };
        }

        public async Task<PlagiarismCheckResponse> CheckPlagiarismAsync(PlagiarismCheckRequest request)
        {
            await SimulateDelay();

            return new PlagiarismCheckResponse
            {
                OriginalText = request.Text,
                SimilarityScore = 0.12m,
                IsPlagiarized = false,
                Matches = new List<PlagiarismMatch>(),
                CheckedAt = DateTime.UtcNow
            };
        }

        public async Task<ComprehensiveReviewResponse> PerformComprehensiveReviewAsync(ComprehensiveReviewRequest request)
        {
            await SimulateDelay();

            var grammarCheck = await CheckGrammarAsync(new GrammarCheckRequest { Text = request.Text });
            var styleAnalysis = await AnalyzeStyleAsync(new StyleAnalysisRequest { Text = request.Text });

            return new ComprehensiveReviewResponse
            {
                OriginalText = request.Text,
                GrammarCheck = grammarCheck,
                StyleAnalysis = styleAnalysis,
                OverallFeedback = "文本整体质量良好，有少量改进空间",
                OverallScore = 0.85m,
                ReviewedAt = DateTime.UtcNow
            };
        }

        // AI Scoring 功能 - 模拟实现
        public async Task<ScoringResponse> ScoreTextAsync(ScoringRequest request)
        {
            await SimulateDelay();

            var dimensions = new List<ScoringDimension>
            {
                new ScoringDimension
                {
                    Name = "内容质量",
                    Description = "内容的深度和相关性",
                    Score = 8.5m,
                    Weight = 0.4m,
                    Feedback = "内容相关但可以更深入",
                    Strengths = new List<string> { "主题明确", "结构清晰" },
                    Weaknesses = new List<string> { "缺乏具体例子", "论证不够充分" }
                },
                new ScoringDimension
                {
                    Name = "语言表达",
                    Description = "语法和词汇使用",
                    Score = 9.0m,
                    Weight = 0.3m,
                    Feedback = "语言表达流畅",
                    Strengths = new List<string> { "语法正确", "词汇丰富" },
                    Weaknesses = new List<string> { "少量重复表达" }
                }
            };

            var suggestions = new List<ImprovementSuggestion>
            {
                new ImprovementSuggestion
                {
                    Area = "内容扩展",
                    CurrentStatus = "内容较为基础",
                    Suggestion = "添加具体案例和数据支持",
                    ExpectedImprovement = "提高内容深度和说服力",
                    Priority = 1
                }
            };

            return new ScoringResponse
            {
                OriginalText = request.Text,
                OverallScore = 8.7m,
                DimensionScores = new Dictionary<string, decimal>
                {
                    { "内容质量", 8.5m },
                    { "语言表达", 9.0m }
                },
                Dimensions = dimensions,
                OverallFeedback = "总体表现良好，有提升空间",
                ImprovementSuggestions = suggestions,
                Grade = "B+",
                ConfidenceLevel = 0.88m,
                ScoredAt = DateTime.UtcNow
            };
        }

        public async Task<RubricDefinition> GetRubricAsync(string rubricId)
        {
            await SimulateDelay();

            return new RubricDefinition
            {
                Name = "标准写作评分标准",
                Description = "用于评估一般写作的质量",
                Category = "写作",
                Dimensions = new List<RubricDimension>
                {
                    new RubricDimension
                    {
                        Name = "内容质量",
                        Weight = 0.4m,
                        Criteria = new List<RubricCriterion>
                        {
                            new RubricCriterion { Level = "优秀", Description = "内容深刻，论证充分", Score = 10.0m },
                            new RubricCriterion { Level = "良好", Description = "内容相关，论证基本充分", Score = 8.0m },
                            new RubricCriterion { Level = "及格", Description = "内容基本相关", Score = 6.0m }
                        }
                    }
                },
                WeightDistribution = new Dictionary<string, decimal>
                {
                    { "内容质量", 0.4m },
                    { "语言表达", 0.3m },
                    { "结构组织", 0.3m }
                },
                IsDefault = true
            };
        }

        public async Task<RubricDefinition> CreateRubricAsync(RubricDefinition rubric)
        {
            await SimulateDelay();
            return rubric; // 模拟创建，直接返回
        }

        public async Task<BatchScoringResponse> ScoreBatchAsync(BatchScoringRequest request)
        {
            await SimulateDelay();

            var results = new List<ScoringResponse>();
            foreach (var textRequest in request.Texts)
            {
                var result = await ScoreTextAsync(textRequest);
                results.Add(result);
            }

            return new BatchScoringResponse
            {
                Results = results,
                Summary = new BatchScoringSummary
                {
                    TotalTexts = results.Count,
                    AverageScore = results.Average(r => r.OverallScore),
                    HighestScore = results.Max(r => r.OverallScore),
                    LowestScore = results.Min(r => r.OverallScore),
                    AverageDimensionScores = new Dictionary<string, decimal>()
                },
                ProcessedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateApiKeyAsync()
        {
            await SimulateDelay();
            return true; // 模拟验证通过
        }

        public async Task<decimal> GetCostEstimateAsync(string model, int tokenCount)
        {
            await SimulateDelay();
            return tokenCount * 0.0001m; // 模拟成本计算
        }

        private async Task SimulateDelay(int minMs = 100, int maxMs = 500)
        {
            var delay = _random.Next(minMs, maxMs);
            await Task.Delay(delay);
        }
    }
}