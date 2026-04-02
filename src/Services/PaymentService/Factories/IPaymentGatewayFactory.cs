using System;
using PaymentService.Interfaces;

namespace PaymentService.Factories
{
    /// <summary>
    /// 支付网关工厂接口
    /// </summary>
    public interface IPaymentGatewayFactory
    {
        /// <summary>
        /// 根据支付网关名称获取对应的客户端
        /// </summary>
        /// <param name="gatewayName">支付网关名称（如 "Alipay", "WeChatPay", "Mock"）</param>
        /// <returns>支付网关客户端实例</returns>
        IPaymentGatewayClient GetClient(string gatewayName);
    }
}