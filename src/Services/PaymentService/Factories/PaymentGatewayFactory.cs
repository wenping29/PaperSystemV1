using System;
using System.Collections.Generic;
using PaymentService.Clients;
using PaymentService.Interfaces;

namespace PaymentService.Factories
{
    /// <summary>
    /// 支付网关工厂实现
    /// </summary>
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly Dictionary<string, IPaymentGatewayClient> _clients;

        /// <summary>
        /// 构造函数，注入所有支付网关客户端
        /// </summary>
        public PaymentGatewayFactory(
            MockPaymentGatewayClient mockClient,
            AlipayGatewayClient alipayClient,
            WeChatPayGatewayClient wechatPayClient)
        {
            _clients = new Dictionary<string, IPaymentGatewayClient>(StringComparer.OrdinalIgnoreCase)
            {
                ["Mock"] = mockClient,
                ["Alipay"] = alipayClient,
                ["WeChatPay"] = wechatPayClient
            };
        }

        /// <summary>
        /// 根据支付网关名称获取对应的客户端
        /// </summary>
        public IPaymentGatewayClient GetClient(string gatewayName)
        {
            if (string.IsNullOrEmpty(gatewayName))
            {
                throw new ArgumentException("支付网关名称不能为空", nameof(gatewayName));
            }

            if (_clients.TryGetValue(gatewayName, out var client))
            {
                return client;
            }

            throw new ArgumentException($"不支持的支付网关: {gatewayName}. 支持的网关: {string.Join(", ", _clients.Keys)}");
        }
    }
}