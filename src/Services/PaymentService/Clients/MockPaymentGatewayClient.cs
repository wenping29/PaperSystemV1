using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Interfaces;

namespace PaymentService.Clients
{
    public class MockPaymentGatewayClient : IPaymentGatewayClient
    {
        public string GatewayName => "Mock";

        private readonly IConfiguration _configuration;
        private readonly ILogger<MockPaymentGatewayClient> _logger;
        private readonly Random _random = new Random();

        public MockPaymentGatewayClient(
            IConfiguration configuration,
            ILogger<MockPaymentGatewayClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GatewayPaymentResponse> CreatePaymentAsync(GatewayPaymentRequest request)
        {
            await Task.Delay(100); // 模拟网络延迟

            _logger.LogInformation("创建模拟支付，交易号: {TransactionNo}, 金额: {Amount}", request.TransactionNo, request.Amount);

            // 模拟成功率
            var success = _random.NextDouble() > 0.1; // 90%成功率

            if (!success)
            {
                return new GatewayPaymentResponse
                {
                    Success = false,
                    ErrorCode = "MOCK_ERROR",
                    ErrorMessage = "模拟支付创建失败"
                };
            }

            // 生成模拟的网关交易号
            var gatewayTransactionNo = $"MOCK{Guid.NewGuid():N}".ToUpper();

            // 模拟支付URL和二维码
            var paymentUrl = $"https://mock-payment.example.com/pay/{request.TransactionNo}";
            var qrCode = $"https://mock-payment.example.com/qr/{request.TransactionNo}";

            return new GatewayPaymentResponse
            {
                Success = true,
                GatewayTransactionNo = gatewayTransactionNo,
                PaymentUrl = paymentUrl,
                QrCode = qrCode,
                GatewayData = new Dictionary<string, object>
                {
                    { "mock_data", "这是模拟支付数据" },
                    { "transaction_no", request.TransactionNo },
                    { "amount", request.Amount },
                    { "currency", request.Currency },
                    { "timestamp", DateTime.UtcNow }
                }
            };
        }

        public async Task<GatewayPaymentQueryResponse> QueryPaymentAsync(string transactionNo)
        {
            await Task.Delay(50);

            _logger.LogInformation("查询模拟支付，交易号: {TransactionNo}", transactionNo);

            // 模拟支付状态：70%成功，20%处理中，10%失败
            var statusRandom = _random.NextDouble();
            var status = statusRandom switch
            {
                < 0.7 => "SUCCESS",
                < 0.9 => "PROCESSING",
                _ => "FAILED"
            };

            var gatewayTransactionNo = $"MOCK{Guid.NewGuid():N}".ToUpper();

            return new GatewayPaymentQueryResponse
            {
                Success = true,
                Status = status,
                Amount = 100.00m, // 模拟金额
                Currency = "CNY",
                GatewayTransactionNo = gatewayTransactionNo,
                PaidAt = status == "SUCCESS" ? DateTime.UtcNow.AddMinutes(-5) : (DateTime?)null,
                GatewayData = new Dictionary<string, object>
                {
                    { "mock_query", "模拟查询结果" },
                    { "status", status },
                    { "timestamp", DateTime.UtcNow }
                }
            };
        }

        public async Task<GatewayPaymentQueryResponse> QueryPaymentByGatewayNoAsync(string gatewayTransactionNo)
        {
            await Task.Delay(50);

            _logger.LogInformation("通过网关号查询模拟支付: {GatewayNo}", gatewayTransactionNo);

            return await QueryPaymentAsync(gatewayTransactionNo);
        }

        public async Task<bool> ClosePaymentAsync(string transactionNo)
        {
            await Task.Delay(50);

            _logger.LogInformation("关闭模拟支付: {TransactionNo}", transactionNo);

            return true;
        }

        public async Task<GatewayRefundResponse> CreateRefundAsync(GatewayRefundRequest request)
        {
            await Task.Delay(150);

            _logger.LogInformation("创建模拟退款，交易号: {TransactionNo}, 退款单号: {RefundNo}, 金额: {Amount}",
                request.TransactionNo, request.RefundNo, request.Amount);

            // 模拟成功率
            var success = _random.NextDouble() > 0.2; // 80%成功率

            if (!success)
            {
                return new GatewayRefundResponse
                {
                    Success = false,
                    ErrorCode = "MOCK_REFUND_ERROR",
                    ErrorMessage = "模拟退款创建失败"
                };
            }

            // 生成模拟的网关退款单号
            var gatewayRefundNo = $"MOCK_REF{Guid.NewGuid():N}".ToUpper();

            return new GatewayRefundResponse
            {
                Success = true,
                GatewayRefundNo = gatewayRefundNo,
                GatewayData = new Dictionary<string, object>
                {
                    { "mock_refund_data", "这是模拟退款数据" },
                    { "transaction_no", request.TransactionNo },
                    { "refund_no", request.RefundNo },
                    { "amount", request.Amount },
                    { "currency", request.Currency },
                    { "timestamp", DateTime.UtcNow }
                }
            };
        }

        public async Task<GatewayRefundQueryResponse> QueryRefundAsync(string refundNo)
        {
            await Task.Delay(50);

            _logger.LogInformation("查询模拟退款，退款单号: {RefundNo}", refundNo);

            // 模拟退款状态
            var statusRandom = _random.NextDouble();
            var status = statusRandom switch
            {
                < 0.6 => "SUCCESS",
                < 0.8 => "PROCESSING",
                < 0.95 => "PENDING",
                _ => "FAILED"
            };

            var gatewayRefundNo = $"MOCK_REF{Guid.NewGuid():N}".ToUpper();

            return new GatewayRefundQueryResponse
            {
                Success = true,
                Status = status,
                Amount = 50.00m, // 模拟退款金额
                Currency = "CNY",
                GatewayRefundNo = gatewayRefundNo,
                RefundedAt = status == "SUCCESS" ? DateTime.UtcNow.AddMinutes(-2) : (DateTime?)null,
                GatewayData = new Dictionary<string, object>
                {
                    { "mock_refund_query", "模拟退款查询结果" },
                    { "status", status },
                    { "timestamp", DateTime.UtcNow }
                }
            };
        }

        public async Task<GatewayRefundQueryResponse> QueryRefundByGatewayNoAsync(string gatewayRefundNo)
        {
            await Task.Delay(50);

            _logger.LogInformation("通过网关号查询模拟退款: {GatewayRefundNo}", gatewayRefundNo);

            return await QueryRefundAsync(gatewayRefundNo);
        }

        public async Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData)
        {
            await Task.Delay(10);

            _logger.LogInformation("验证模拟回调数据");

            // 模拟验证：检查必要字段
            var requiredFields = new[] { "transaction_no", "status", "signature" };
            foreach (var field in requiredFields)
            {
                if (!callbackData.ContainsKey(field))
                {
                    _logger.LogWarning("模拟回调缺少必要字段: {Field}", field);
                    return false;
                }
            }

            return true;
        }

        public async Task<Dictionary<string, string>> ParseCallbackAsync(Dictionary<string, string> callbackData)
        {
            await Task.Delay(10);

            _logger.LogInformation("解析模拟回调数据");

            // 简单返回原始数据，实际可能需要转换
            return callbackData;
        }

        public async Task<bool> ValidateConfigAsync()
        {
            await Task.Delay(10);

            // 模拟配置验证
            var configValid = !string.IsNullOrEmpty(_configuration["Payment:DefaultCurrency"]);

            _logger.LogInformation("模拟支付网关配置验证: {Valid}", configValid);

            return configValid;
        }

        public string GenerateSignature(Dictionary<string, string> parameters)
        {
            // 模拟签名生成
            var signature = $"MOCK_SIGN_{Guid.NewGuid():N}";
            _logger.LogInformation("生成模拟签名: {Signature}", signature);
            return signature;
        }
    }
}