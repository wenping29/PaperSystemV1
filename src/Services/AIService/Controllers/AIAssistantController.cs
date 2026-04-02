using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AIService.DTOs;
using AIService.Interfaces;

namespace AIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要身份验证
    public class AIAssistantController : ControllerBase
    {
        private readonly IAIAssistantService _assistantService;
        private readonly ILogger<AIAssistantController> _logger;

        public AIAssistantController(
            IAIAssistantService assistantService,
            ILogger<AIAssistantController> logger)
        {
            _assistantService = assistantService;
            _logger = logger;
        }

        /// <summary>
        /// 获取写作建议
        /// </summary>
        [HttpPost("suggestion")]
        [ProducesResponseType(typeof(AIAssistantResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AIAssistantResponse>> GetWritingSuggestion([FromBody] AIAssistantRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _assistantService.GetWritingSuggestionAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作建议失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取写作模板
        /// </summary>
        [HttpPost("template")]
        [ProducesResponseType(typeof(WritingTemplateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WritingTemplateResponse>> GetWritingTemplate([FromBody] WritingTemplateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _assistantService.GetWritingTemplateAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作模板失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 生成内容
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ContentGenerationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ContentGenerationResponse>> GenerateContent([FromBody] ContentGenerationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _assistantService.GenerateContentAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成内容失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 文本摘要
        /// </summary>
        [HttpPost("summarize")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> SummarizeText([FromBody] SummarizeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("文本不能为空");
                }

                var response = await _assistantService.SummarizeTextAsync(request.Text, request.MaxLength);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文本摘要失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 文本翻译
        /// </summary>
        [HttpPost("translate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> TranslateText([FromBody] TranslateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("文本不能为空");
                }

                if (string.IsNullOrWhiteSpace(request.SourceLanguage) || string.IsNullOrWhiteSpace(request.TargetLanguage))
                {
                    return BadRequest("源语言和目标语言不能为空");
                }

                var response = await _assistantService.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文本翻译失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取写作提示
        /// </summary>
        [HttpGet("prompts")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<string>>> GetWritingPrompts(
            [FromQuery] string category = "general",
            [FromQuery] int count = 5)
        {
            try
            {
                if (count <= 0 || count > 20)
                {
                    return BadRequest("数量必须在1到20之间");
                }

                var response = await _assistantService.GetWritingPromptsAsync(category, count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取写作提示失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 健康检查
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "AIAssistant",
                timestamp = DateTime.UtcNow
            });
        }
    }

    // 请求DTO（用于简化端点）
    public class SummarizeRequest
    {
        public string Text { get; set; } = string.Empty;
        public int MaxLength { get; set; } = 200;
    }

    public class TranslateRequest
    {
        public string Text { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = "zh";
        public string TargetLanguage { get; set; } = "en";
    }
}