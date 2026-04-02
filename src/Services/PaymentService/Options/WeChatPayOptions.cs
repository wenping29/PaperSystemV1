using System.ComponentModel.DataAnnotations;

namespace PaymentService.Options
{
    /// <summary>
    /// 微信支付配置选项
    /// </summary>
    public class WeChatPayOptions
    {
        /// <summary>
        /// 应用ID（公众号/小程序/APP）
        /// </summary>
        [Required(ErrorMessage = "微信支付AppId不能为空")]
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 商户号
        /// </summary>
        [Required(ErrorMessage = "微信支付商户号不能为空")]
        public string MchId { get; set; } = string.Empty;

        /// <summary>
        /// API密钥（V2密钥）
        /// </summary>
        [Required(ErrorMessage = "微信支付API密钥不能为空")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// API密钥V3（V3密钥）
        /// </summary>
        public string? ApiV3Key { get; set; }

        /// <summary>
        /// 商户证书路径（.p12或.pem文件）
        /// </summary>
        [Required(ErrorMessage = "微信支付证书路径不能为空")]
        public string CertPath { get; set; } = string.Empty;

        /// <summary>
        /// 商户证书密码（可选）
        /// </summary>
        public string? CertPassword { get; set; }

        /// <summary>
        /// 是否使用沙箱环境
        /// </summary>
        public bool Sandbox { get; set; } = true;

        /// <summary>
        /// 是否使用V3接口
        /// </summary>
        public bool UseV3 { get; set; } = true;

        /// <summary>
        /// 回调通知地址（可选，可在创建支付时单独指定）
        /// </summary>
        public string? NotifyUrl { get; set; }

        /// <summary>
        /// 支付成功前端跳转地址（可选）
        /// </summary>
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// 退款通知地址（可选）
        /// </summary>
        public string? RefundNotifyUrl { get; set; }

        /// <summary>
        /// API域名（国内）
        /// </summary>
        public string ApiDomain { get; set; } = "https://api.mch.weixin.qq.com";

        /// <summary>
        /// 沙箱API域名
        /// </summary>
        public string SandboxApiDomain { get; set; } = "https://api.mch.weixin.qq.com/sandboxnew";

        /// <summary>
        /// 获取实际使用的API域名
        /// </summary>
        public string GetApiDomain()
        {
            return Sandbox ? SandboxApiDomain : ApiDomain;
        }
    }
}