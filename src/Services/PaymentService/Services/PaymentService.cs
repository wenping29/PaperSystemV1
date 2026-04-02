using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Interfaces;
using PaymentService.Factories;

namespace PaymentService.Services
{
    public class IPaymentService : IIPaymentService
    {
        private readonly IPaymentTransactionRepository _paymentRepository;
        private readonly IRefundTransactionRepository _refundRepository;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IPaymentService> _logger;

        public IPaymentService(
            IPaymentTransactionRepository paymentRepository,
            IRefundTransactionRepository refundRepository,
            IPaymentGatewayFactory gatewayFactory,
            IConfiguration configuration,
            ILogger<IPaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _refundRepository = refundRepository;
            _gatewayFactory = gatewayFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, Guid userId, string? userEmail = null)
        {
            try
            {
                _logger.LogInformation("创建支付，用户: {UserId}, 金额: {Amount} {Currency}", userId, request.Amount, request.Currency);

                // 验证请求
                var validationResult = await ValidatePaymentRequestAsync(request);
                if (!validationResult)
                {
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = "支付请求验证失败",
                        ErrorCode = "VALIDATION_ERROR"
                    };
                }

                // 生成交易号
                var transactionNo = await GenerateTransactionNoAsync();

                // 创建支付交易记录
                var payment = new PaymentTransaction
                {
                    TransactionNo = transactionNo,
                    UserId = userId,
                    UserEmail = userEmail,
                    PaymentGateway = request.PaymentGateway,
                    PaymentType = request.PaymentType,
                    Currency = request.Currency,
                    Amount = request.Amount,
                    Status = PaymentStatus.Pending.ToString(),
                    Description = request.ProductDescription,
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    ProductDescription = request.ProductDescription,
                    TargetUserId = request.TargetUserId,
                    TargetUserName = request.TargetUserName,
                    Message = request.Message,
                    Anonymous = request.Anonymous ? "true" : "false",
                    Channel = request.Channel,
                    ReturnUrl = request.ReturnUrl,
                    Attach = request.Attach,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(30), // 默认30分钟过期
                    ClientIp = null, // 需要从HttpContext获取
                    UserAgent = null // 需要从HttpContext获取
                };

                // 保存到数据库
                payment = await _paymentRepository.AddAsync(payment);

                // 调用支付网关创建支付
                var gatewayRequest = new GatewayPaymentRequest
                {
                    TransactionNo = transactionNo,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Subject = request.ProductName,
                    Body = request.ProductDescription ?? request.ProductName,
                    Channel = request.Channel ?? "web",
                    ReturnUrl = request.ReturnUrl ?? _configuration["Payment:CallbackBaseUrl"] + "/return",
                    NotifyUrl = _configuration["Payment:NotifyUrl"] ?? _configuration["Payment:CallbackBaseUrl"] + "/notify",
                    ExtraParams = request.Metadata
                };

                var gatewayClient = _gatewayFactory.GetClient(request.PaymentGateway);
                var gatewayResponse = await gatewayClient.CreatePaymentAsync(gatewayRequest);

                if (!gatewayResponse.Success)
                {
                    // 更新支付状态为失败
                    await _paymentRepository.UpdateStatusAsync(payment.Id, PaymentStatus.Failed.ToString());

                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = $"支付网关错误: {gatewayResponse.ErrorMessage}",
                        ErrorCode = gatewayResponse.ErrorCode
                    };
                }

                // 更新网关交易号
                if (!string.IsNullOrEmpty(gatewayResponse.GatewayTransactionNo))
                {
                    await _paymentRepository.UpdateStatusAsync(payment.Id, PaymentStatus.Processing.ToString(), gatewayResponse.GatewayTransactionNo);
                }

                // 返回支付数据
                var paymentData = new PaymentData
                {
                    TransactionId = payment.Id,
                    TransactionNo = payment.TransactionNo,
                    PaymentGateway = payment.PaymentGateway,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt,
                    ExpiredAt = payment.ExpiredAt,
                    GatewayPaymentUrl = gatewayResponse.PaymentUrl,
                    GatewayQrCode = gatewayResponse.QrCode,
                    GatewayResponse = gatewayResponse.GatewayData
                };

                return new CreatePaymentResponse
                {
                    Success = true,
                    Message = "支付创建成功",
                    Data = paymentData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付失败");
                return new CreatePaymentResponse
                {
                    Success = false,
                    Message = "创建支付时发生错误",
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        public async Task<PaymentQueryResponse> QueryPaymentAsync(string transactionNo, Guid userId)
        {
            try
            {
                var payment = await _paymentRepository.GetByTransactionNoAsync(transactionNo);
                if (payment == null)
                {
                    return new PaymentQueryResponse
                    {
                        Success = false,
                        Message = "支付交易不存在",
                        ErrorCode = "NOT_FOUND"
                    };
                }

                // 检查用户权限
                if (payment.UserId != userId)
                {
                    return new PaymentQueryResponse
                    {
                        Success = false,
                        Message = "无权访问该支付交易",
                        ErrorCode = "UNAUTHORIZED"
                    };
                }

                var paymentDetail = MapToPaymentDetail(payment);

                return new PaymentQueryResponse
                {
                    Success = true,
                    Message = "查询成功",
                    Data = paymentDetail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付失败");
                return new PaymentQueryResponse
                {
                    Success = false,
                    Message = "查询支付时发生错误",
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        public async Task<PaymentQueryResponse> QueryPaymentByGatewayAsync(string gatewayTransactionNo)
        {
            try
            {
                var payment = await _paymentRepository.GetByGatewayTransactionNoAsync(gatewayTransactionNo);
                if (payment == null)
                {
                    return new PaymentQueryResponse
                    {
                        Success = false,
                        Message = "支付交易不存在",
                        ErrorCode = "NOT_FOUND"
                    };
                }

                var paymentDetail = MapToPaymentDetail(payment);

                return new PaymentQueryResponse
                {
                    Success = true,
                    Message = "查询成功",
                    Data = paymentDetail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过网关查询支付失败");
                return new PaymentQueryResponse
                {
                    Success = false,
                    Message = "查询支付时发生错误",
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(string transactionNo, string status, string? gatewayTransactionNo = null, DateTime? paidAt = null)
        {
            try
            {
                var payment = await _paymentRepository.GetByTransactionNoAsync(transactionNo);
                if (payment == null)
                    return false;

                return await _paymentRepository.UpdateStatusAsync(payment.Id, status, gatewayTransactionNo, paidAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新支付状态失败");
                return false;
            }
        }

        public async Task<List<PaymentTransaction>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var payments = await _paymentRepository.GetByUserIdAsync(userId, page, pageSize);
                return payments.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户支付列表失败");
                return new List<PaymentTransaction>();
            }
        }

        public async Task<PaymentTransaction?> GetPaymentByTransactionNoAsync(string transactionNo)
        {
            return await _paymentRepository.GetByTransactionNoAsync(transactionNo);
        }

        public async Task<CreateRefundResponse> CreateRefundAsync(CreateRefundRequest request, Guid userId)
        {
            try
            {
                _logger.LogInformation("创建退款，交易号: {TransactionNo}, 金额: {Amount}", request.TransactionNo, request.Amount);

                // 查询支付交易
                var payment = await _paymentRepository.GetByTransactionNoAsync(request.TransactionNo);
                if (payment == null)
                {
                    return new CreateRefundResponse
                    {
                        Success = false,
                        Message = "支付交易不存在",
                        ErrorCode = "NOT_FOUND"
                    };
                }

                // 检查用户权限
                if (payment.UserId != userId)
                {
                    return new CreateRefundResponse
                    {
                        Success = false,
                        Message = "无权操作该支付交易",
                        ErrorCode = "UNAUTHORIZED"
                    };
                }

                // 检查支付状态
                if (payment.Status != PaymentStatus.Success.ToString())
                {
                    return new CreateRefundResponse
                    {
                        Success = false,
                        Message = "支付未成功，不能退款",
                        ErrorCode = "INVALID_STATUS"
                    };
                }

                // 计算可退款金额
                var alreadyRefunded = payment.RefundedAmount ?? 0;
                var availableToRefund = payment.Amount - alreadyRefunded;

                // 确定退款金额
                var refundAmount = request.FullRefund ? availableToRefund : Math.Min(request.Amount, availableToRefund);

                if (refundAmount <= 0)
                {
                    return new CreateRefundResponse
                    {
                        Success = false,
                        Message = "无可退款金额",
                        ErrorCode = "NO_AVAILABLE_AMOUNT"
                    };
                }

                // 生成退款单号
                var refundNo = await GenerateRefundNoAsync();

                // 创建退款记录
                var refund = new RefundTransaction
                {
                    RefundNo = refundNo,
                    PaymentTransactionId = payment.Id,
                    PaymentGateway = payment.PaymentGateway,
                    Currency = payment.Currency,
                    Amount = refundAmount,
                    Status = RefundStatus.Pending.ToString(),
                    Source = RefundSource.UserRequest.ToString(),
                    UserId = userId,
                    UserEmail = payment.UserEmail,
                    Reason = request.Reason,
                    Description = request.Description
                };

                refund = await _refundRepository.AddAsync(refund);

                // 调用支付网关创建退款
                var gatewayRequest = new GatewayRefundRequest
                {
                    TransactionNo = payment.TransactionNo,
                    GatewayTransactionNo = payment.GatewayTransactionNo,
                    RefundNo = refundNo,
                    Amount = refundAmount,
                    Currency = payment.Currency,
                    Reason = request.Reason
                };

                var gatewayClient = _gatewayFactory.GetClient(payment.PaymentGateway);
                var gatewayResponse = await gatewayClient.CreateRefundAsync(gatewayRequest);

                if (!gatewayResponse.Success)
                {
                    // 更新退款状态为失败
                    await _refundRepository.UpdateStatusAsync(refund.Id, RefundStatus.Failed.ToString());

                    return new CreateRefundResponse
                    {
                        Success = false,
                        Message = $"退款网关错误: {gatewayResponse.ErrorMessage}",
                        ErrorCode = gatewayResponse.ErrorCode
                    };
                }

                // 更新网关退款单号
                if (!string.IsNullOrEmpty(gatewayResponse.GatewayRefundNo))
                {
                    await _refundRepository.UpdateStatusAsync(refund.Id, RefundStatus.Processing.ToString(), gatewayResponse.GatewayRefundNo);
                }

                var refundData = new RefundData
                {
                    RefundId = refund.Id,
                    RefundNo = refund.RefundNo,
                    PaymentTransactionId = refund.PaymentTransactionId,
                    PaymentGateway = refund.PaymentGateway,
                    Amount = refund.Amount,
                    Currency = refund.Currency,
                    Status = refund.Status,
                    Reason = refund.Reason,
                    CreatedAt = refund.CreatedAt
                };

                return new CreateRefundResponse
                {
                    Success = true,
                    Message = "退款申请已提交",
                    Data = refundData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建退款失败");
                return new CreateRefundResponse
                {
                    Success = false,
                    Message = "创建退款时发生错误",
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        public async Task<RefundQueryResponse> QueryRefundAsync(string refundNo, Guid userId)
        {
            try
            {
                var refund = await _refundRepository.GetByRefundNoAsync(refundNo);
                if (refund == null)
                {
                    return new RefundQueryResponse
                    {
                        Success = false,
                        Message = "退款记录不存在",
                        ErrorCode = "NOT_FOUND"
                    };
                }

                // 检查用户权限
                if (refund.UserId != userId)
                {
                    return new RefundQueryResponse
                    {
                        Success = false,
                        Message = "无权访问该退款记录",
                        ErrorCode = "UNAUTHORIZED"
                    };
                }

                var refundDetail = MapToRefundDetail(refund);

                return new RefundQueryResponse
                {
                    Success = true,
                    Message = "查询成功",
                    Data = refundDetail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询退款失败");
                return new RefundQueryResponse
                {
                    Success = false,
                    Message = "查询退款时发生错误",
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        public async Task<bool> UpdateRefundStatusAsync(string refundNo, string status, string? gatewayRefundNo = null, DateTime? refundedAt = null)
        {
            try
            {
                var refund = await _refundRepository.GetByRefundNoAsync(refundNo);
                if (refund == null)
                    return false;

                return await _refundRepository.UpdateStatusAsync(refund.Id, status, gatewayRefundNo, refundedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新退款状态失败");
                return false;
            }
        }

        public async Task<List<RefundTransaction>> GetPaymentRefundsAsync(long paymentTransactionId)
        {
            try
            {
                var refunds = await _refundRepository.GetByPaymentTransactionIdAsync(paymentTransactionId);
                return refunds.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取支付退款列表失败");
                return new List<RefundTransaction>();
            }
        }

        public async Task<List<RefundTransaction>> GetUserRefundsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var refunds = await _refundRepository.GetByUserIdAsync(userId, page, pageSize);
                return refunds.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户退款列表失败");
                return new List<RefundTransaction>();
            }
        }

        public async Task<PaymentCallbackResponse> ProcessPaymentCallbackAsync(PaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("处理支付回调，交易号: {TransactionNo}", request.TransactionNo);

                // 验证签名
                var gateway = !string.IsNullOrEmpty(request.Gateway) ? request.Gateway : "Alipay";
                var isValid = await VerifyPaymentSignatureAsync(request.GatewayData?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>(), gateway);
                if (!isValid)
                {
                    _logger.LogWarning("支付回调签名验证失败");
                    return new PaymentCallbackResponse
                    {
                        Success = false,
                        Message = "签名验证失败"
                    };
                }

                // 查找支付交易
                var payment = await _paymentRepository.GetByTransactionNoAsync(request.TransactionNo);
                if (payment == null)
                {
                    _logger.LogWarning("支付交易不存在: {TransactionNo}", request.TransactionNo);
                    return new PaymentCallbackResponse
                    {
                        Success = false,
                        Message = "支付交易不存在"
                    };
                }

                // 更新支付状态
                var status = request.Status;
                if (status == "SUCCESS" || status == "TRADE_SUCCESS")
                {
                    await _paymentRepository.UpdateStatusAsync(
                        payment.Id,
                        PaymentStatus.Success.ToString(),
                        request.GatewayTransactionNo,
                        request.PaidAt);
                }
                else if (status == "FAILED" || status == "TRADE_FAILED")
                {
                    await _paymentRepository.UpdateStatusAsync(
                        payment.Id,
                        PaymentStatus.Failed.ToString(),
                        request.GatewayTransactionNo);
                }
                else if (status == "CLOSED")
                {
                    await _paymentRepository.UpdateStatusAsync(
                        payment.Id,
                        PaymentStatus.Cancelled.ToString(),
                        request.GatewayTransactionNo);
                }

                return new PaymentCallbackResponse
                {
                    Success = true,
                    Message = "回调处理成功",
                    ProcessedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付回调失败");
                return new PaymentCallbackResponse
                {
                    Success = false,
                    Message = "处理回调时发生错误"
                };
            }
        }

        public async Task<bool> VerifyPaymentSignatureAsync(Dictionary<string, string> parameters, string gateway)
        {
            // 这里应该实现具体的签名验证逻辑
            // 暂时返回true用于测试
            return await Task.FromResult(true);
        }

        public async Task<decimal> GetUserTotalPaidAsync(Guid userId)
        {
            return await _paymentRepository.GetTotalAmountByUserIdAsync(userId);
        }

        public async Task<decimal> GetUserTotalRefundedAsync(Guid userId)
        {
            return await _refundRepository.GetTotalRefundedAmountByUserIdAsync(userId);
        }

        public async Task<Dictionary<string, decimal>> GetPaymentStatsAsync(DateTime startDate, DateTime endDate)
        {
            // 简化的统计实现
            var stats = new Dictionary<string, decimal>
            {
                { "total_amount", 0 },
                { "success_count", 0 },
                { "refund_amount", 0 }
            };

            // 实际应该从数据库聚合查询
            return await Task.FromResult(stats);
        }

        public async Task<string> GenerateTransactionNoAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            var transactionNo = $"PAY{timestamp}{random}";

            // 检查是否已存在
            var exists = await _paymentRepository.ExistsTransactionNoAsync(transactionNo);
            if (exists)
            {
                // 如果已存在，递归生成新的
                return await GenerateTransactionNoAsync();
            }

            return transactionNo;
        }

        public async Task<string> GenerateRefundNoAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            var refundNo = $"REF{timestamp}{random}";

            // 检查是否已存在
            var exists = await _refundRepository.ExistsRefundNoAsync(refundNo);
            if (exists)
            {
                // 如果已存在，递归生成新的
                return await GenerateRefundNoAsync();
            }

            return refundNo;
        }

        public async Task<bool> ValidatePaymentRequestAsync(CreatePaymentRequest request)
        {
            if (request.Amount <= 0)
                return false;

            if (string.IsNullOrEmpty(request.ProductName))
                return false;

            if (!new[] { "Alipay", "WeChatPay", "PayPal", "Stripe" }.Contains(request.PaymentGateway))
                return false;

            if (!new[] { "Donation", "Reward", "Subscription", "Purchase" }.Contains(request.PaymentType))
                return false;

            return await Task.FromResult(true);
        }

        private PaymentDetail MapToPaymentDetail(PaymentTransaction payment)
        {
            return new PaymentDetail
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
        }

        private RefundDetail MapToRefundDetail(RefundTransaction refund)
        {
            return new RefundDetail
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
        }
    }
}