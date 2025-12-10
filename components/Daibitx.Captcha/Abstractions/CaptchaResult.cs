namespace Daibitx.Captcha.Abstractions
{
    /// <summary>
    /// CAPTCHA generation result
    /// </summary>
    public class CaptchaResult
    {
        /// <summary>
        /// CAPTCHA code text
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// CAPTCHA image byte array
        /// </summary>
        public byte[] ImageBytes { get; set; } = default!;

        /// <summary>
        /// Image content type (defaults to image/png)
        /// </summary>
        public string ContentType { get; set; } = "image/png";
    }
}