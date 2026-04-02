using System.ComponentModel.DataAnnotations;

namespace PaymentService.Options
{
    /// <summary>
    /// 支付宝配置选项
    /// </summary>
    public class AlipayOptions
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        [Required(ErrorMessage = "支付宝AppId不能为空")]
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 应用私钥
        /// </summary>
        [Required(ErrorMessage = "支付宝应用私钥不能为空")]
        public string AppPrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// 支付宝公钥
        /// </summary>
        [Required(ErrorMessage = "支付宝公钥不能为空")]
        public string AlipayPublicKey { get; set; } = string.Empty;

        /// <summary>
        /// 签名类型，默认RSA2
        /// </summary>
        public string SignType { get; set; } = "RSA2";

        /// <summary>
        /// 字符编码，默认UTF-8
        /// </summary>
        public string Charset { get; set; } = "UTF-8";

        /// <summary>
        /// 支付宝网关地址
        /// </summary>
        public string GatewayUrl { get; set; } = "https://openapi.alipay.com/gateway.do";

        /// <summary>
        /// 是否使用沙箱环境
        /// </summary>
        public bool Sandbox { get; set; } = false;

        /// <summary>
        /// 沙箱环境网关地址
        /// </summary>
        public string SandboxGatewayUrl { get; set; } = "https://openapi-sandbox.dl.alipaydev.com/gateway.do";

        /// <summary>
        /// 获取实际使用的网关地址
        /// </summary>
        public string GetGatewayUrl()
        {
            return Sandbox ? SandboxGatewayUrl : GatewayUrl;
        }

        /// <summary>
        /// 应用公钥证书路径（可选，证书模式使用）
        /// </summary>
        public string? AppCertPath { get; set; }

        /// <summary>
        /// 支付宝公钥证书路径（可选，证书模式使用）
        /// </summary>
        public string? AlipayCertPath { get; set; }

        /// <summary>
        /// 支付宝根证书路径（可选，证书模式使用）
        /// </summary>
        public string? AlipayRootCertPath { get; set; }

        /// <summary>
        /// 是否使用证书模式
        /// </summary>
        public bool UseCertMode => !string.IsNullOrEmpty(AppCertPath);

        /// <summary>
        /// 回调通知地址（可选，可在创建支付时单独指定）
        /// </summary>
        public string? NotifyUrl { get; set; }

        /// <summary>
        /// 支付成功前端跳转地址（可选）
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}