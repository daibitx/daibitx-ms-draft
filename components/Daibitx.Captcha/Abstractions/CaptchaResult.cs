namespace Daibitx.Captcha.Abstractions
{
    /// <summary>
    /// 验证码生成结果
    /// </summary>
    public class CaptchaResult
    {
        /// <summary>
        /// 验证码文本
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// 验证码图片字节数组
        /// </summary>
        public byte[] ImageBytes { get; set; } = default!;

        /// <summary>
        /// 图片内容类型（默认为 image/png）
        /// </summary>
        public string ContentType { get; set; } = "image/png";
    }
}
