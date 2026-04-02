using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentService.DTOs;
using PaymentService.Helpers;
using PaymentService.Interfaces;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要身份验证
    public class PaymentController : ControllerBase
    {
        private readonly IIPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IIPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// 创建支付
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreatePaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                var userEmail = JwtHelper.GetUserEmailFromHttpContext(HttpContext);

                var response = await _paymentService.CreatePaymentAsync(request, userId.Value, userEmail);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 查询支付详情
        /// </summary>
        [HttpGet("query/{transactionNo}")]
        [ProducesResponseType(typeof(PaymentQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentQueryResponse>> QueryPayment(string transactionNo)
        {
            try
            {
                if (string.IsNullOrEmpty(transactionNo))
                {
                    return BadRequest(new { message = "交易号不能为空" });
                }

                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                var response = await _paymentService.QueryPaymentAsync(transactionNo, userId.Value);

                if (!response.Success)
                {
                    if (response.ErrorCode == "NOT_FOUND")
                    {
                        return NotFound(response);
                    }
                    else if (response.ErrorCode == "UNAUTHORIZED")
                    {
                        return Unauthorized(response);
                    }
                    else
                    {
                        return BadRequest(response);
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取用户支付列表
        /// </summary>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<PaymentDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PaymentDetail>>> GetUserPayments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var payments = await _paymentService.GetUserPaymentsAsync(userId.Value, page, pageSize);
                var paymentDetails = new List<PaymentDetail>();

                foreach (var payment in payments)
                {
                    var detail = new PaymentDetail
                    {
                        Id = payment.Id,
                        TransactionNo = payment.TransactionNo,
                        GatewayTransactionNo = payment.GatewayTransactionNo,
                        UserId = payment.UserId,
                        UserEmail = payment.UserEmail,
                        PaymentGateway = payment.PaymentGateway,
                        PaymentType = payment.PaymentType,
                        Amount = payment.Amount,
                        Fee = payment.Fee,
                        RefundedAmount = payment.RefundedAmount,
                        Currency = payment.Currency,
                        Status = payment.Status,
                        Description = payment.Description,
                        ProductName = payment.ProductName,
                        ProductDescription = payment.ProductDescription,
                        TargetUserId = payment.TargetUserId,
                        TargetUserName = payment.TargetUserName,
                        Message = payment.Message,
                        Anonymous = payment.Anonymous == "true",
                        CreatedAt = payment.CreatedAt,
                        UpdatedAt = payment.UpdatedAt,
                        PaidAt = payment.PaidAt,
                        ExpiredAt = payment.ExpiredAt
                    };
                    paymentDetails.Add(detail);
                }

                return Ok(paymentDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户支付列表失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 创建退款
        /// </summary>
        [HttpPost("refund")]
        [ProducesResponseType(typeof(CreateRefundResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateRefundResponse>> CreateRefund([FromBody] CreateRefundRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                var response = await _paymentService.CreateRefundAsync(request, userId.Value);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建退款失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 查询退款详情
        /// </summary>
        [HttpGet("refund/{refundNo}")]
        [ProducesResponseType(typeof(RefundQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RefundQueryResponse>> QueryRefund(string refundNo)
        {
            try
            {
                if (string.IsNullOrEmpty(refundNo))
                {
                    return BadRequest(new { message = "退款单号不能为空" });
                }

                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                var response = await _paymentService.QueryRefundAsync(refundNo, userId.Value);

                if (!response.Success)
                {
                    if (response.ErrorCode == "NOT_FOUND")
                    {
                        return NotFound(response);
                    }
                    else if (response.ErrorCode == "UNAUTHORIZED")
                    {
                        return Unauthorized(response);
                    }
                    else
                    {
                        return BadRequest(response);
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询退款失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取用户退款列表
        /// </summary>
        [HttpGet("refunds/list")]
        [ProducesResponseType(typeof(List<RefundDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RefundDetail>>> GetUserRefunds(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var refunds = await _paymentService.GetUserRefundsAsync(userId.Value, page, pageSize);
                var refundDetails = new List<RefundDetail>();

                foreach (var refund in refunds)
                {
                    var detail = new RefundDetail
                    {
                        Id = refund.Id,
                        RefundNo = refund.RefundNo,
                        GatewayRefundNo = refund.GatewayRefundNo,
                        PaymentTransactionId = refund.PaymentTransactionId,
                        PaymentGateway = refund.PaymentGateway,
                        Amount = refund.Amount,
                        Fee = refund.Fee,
                        Currency = refund.Currency,
                        Status = refund.Status,
                        Source = refund.Source,
                        Reason = refund.Reason,
                        Description = refund.Description,
                        UserId = refund.UserId,
                        UserEmail = refund.UserEmail,
                        ReviewerId = refund.ReviewerId,
                        ReviewerName = refund.ReviewerName,
                        ReviewedAt = refund.ReviewedAt,
                        ReviewComment = refund.ReviewComment,
                        CreatedAt = refund.CreatedAt,
                        UpdatedAt = refund.UpdatedAt,
                        RefundedAt = refund.RefundedAt
                    };
                    refundDetails.Add(detail);
                }

                return Ok(refundDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户退款列表失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }


        /// <summary>
        /// 支付宝支付回调
        /// </summary>
        [HttpPost("callback/alipay")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaymentCallbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentCallbackResponse>> AlipayCallback([FromForm] Dictionary<string, string> formData)
        {
            return await ProcessCallbackAsync(formData, "Alipay");
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        [HttpPost("callback/wechatpay")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaymentCallbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentCallbackResponse>> WeChatPayCallback([FromForm] Dictionary<string, string> formData)
        {
            return await ProcessCallbackAsync(formData, "WeChatPay");
        }

        /// <summary>
        /// 退款回调（支付网关调用）
        /// </summary>
        [HttpPost("callback/refund")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaymentCallbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentCallbackResponse>> RefundCallback([FromForm] Dictionary<string, string> formData)
        {
            try
            {
                _logger.LogInformation("收到退款回调，参数数量: {Count}", formData.Count);

                // 这里应该处理退款回调
                // 暂时返回成功
                return Ok(new PaymentCallbackResponse
                {
                    Success = true,
                    Message = "退款回调接收成功",
                    ProcessedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理退款回调失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取用户支付统计
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetPaymentStats()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromHttpContext(HttpContext);
                if (userId == null)
                {
                    return Unauthorized(new { message = "无法获取用户身份" });
                }

                var totalPaid = await _paymentService.GetUserTotalPaidAsync(userId.Value);
                var totalRefunded = await _paymentService.GetUserTotalRefundedAsync(userId.Value);

                return Ok(new
                {
                    total_paid = totalPaid,
                    total_refunded = totalRefunded,
                    net_paid = totalPaid - totalRefunded,
                    currency = "CNY"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取支付统计失败");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "服务器内部错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 处理支付回调的私有方法
        /// </summary>
        private async Task<ActionResult<PaymentCallbackResponse>> ProcessCallbackAsync(
            Dictionary<string, string> formData, string gateway)
        {
            try
            {
                _logger.LogInformation("收到{Gateway}支付回调，参数数量: {Count}", gateway, formData.Count);

                // 解析回调数据
                var request = new PaymentCallbackRequest
                {
                    Gateway = gateway,
                    GatewayData = formData,
                    Signature = formData.GetValueOrDefault("sign")
                };

                // 根据网关类型解析不同字段
                if (gateway.Equals("Alipay", StringComparison.OrdinalIgnoreCase))
                {
                    request.TransactionNo = formData.GetValueOrDefault("out_trade_no") ?? string.Empty;
                    request.GatewayTransactionNo = formData.GetValueOrDefault("trade_no");
                    request.Status = formData.GetValueOrDefault("trade_status") ?? string.Empty;

                    if (formData.TryGetValue("total_amount", out var totalAmountStr) && decimal.TryParse(totalAmountStr, out var totalAmount))
                    {
                        request.Amount = totalAmount;
                    }

                    if (formData.TryGetValue("gmt_payment", out var gmtPaymentStr) && DateTime.TryParse(gmtPaymentStr, out var gmtPayment))
                    {
                        request.PaidAt = gmtPayment;
                    }
                }
                else if (gateway.Equals("WeChatPay", StringComparison.OrdinalIgnoreCase))
                {
                    request.TransactionNo = formData.GetValueOrDefault("out_trade_no") ?? string.Empty;
                    request.GatewayTransactionNo = formData.GetValueOrDefault("transaction_id");
                    request.Status = formData.GetValueOrDefault("result_code") ?? string.Empty;

                    if (formData.TryGetValue("total_fee", out var totalFeeStr) && int.TryParse(totalFeeStr, out int totalFee))
                    {
                        request.Amount = totalFee / 100m; // 微信支付以分为单位
                    }

                    if (formData.TryGetValue("time_end", out var timeEndStr))
                    {
                        if (DateTime.TryParseExact(timeEndStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var timeEnd))
                        {
                            request.PaidAt = timeEnd;
                        }
                    }
                }
                else
                {
                    // 默认尝试解析通用字段
                    request.TransactionNo = formData.GetValueOrDefault("transaction_no") ?? formData.GetValueOrDefault("out_trade_no") ?? string.Empty;
                    request.GatewayTransactionNo = formData.GetValueOrDefault("gateway_transaction_no") ?? formData.GetValueOrDefault("trade_no");
                    request.Status = formData.GetValueOrDefault("status") ?? formData.GetValueOrDefault("trade_status") ?? string.Empty;
                }

                var response = await _paymentService.ProcessPaymentCallbackAsync(request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理{Gateway}支付回调失败", gateway);
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
                service = "Payment",
                timestamp = DateTime.UtcNow
            });
        }
    }
}