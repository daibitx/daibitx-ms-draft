namespace Daibitx.Captcha.Abstractions
{
    /// <summary>
    /// CAPTCHA configuration options
    /// </summary>
    public class CaptchaOptions
    {
        /// <summary>
        /// Image width (pixels)
        /// </summary>
        public int Width { get; set; } = 120;

        /// <summary>
        /// Image height (pixels)
        /// </summary>
        public int Height { get; set; } = 40;

        /// <summary>
        /// CAPTCHA code length
        /// </summary>
        public int CodeLength { get; set; } = 4;

        /// <summary>
        /// CAPTCHA character set
        /// </summary>
        public string Charset { get; set; } = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>
        /// Whether to enable noise
        /// </summary>
        public bool Noise { get; set; } = true;

        /// <summary>
        /// Noise density (0-100)
        /// </summary>
        public int NoiseDensity { get; set; } = 15;

        /// <summary>
        /// Whether to enable character distortion
        /// </summary>
        public bool Distort { get; set; } = true;

        /// <summary>
        /// Distortion level (0-100)
        /// </summary>
        public int DistortLevel { get; set; } = 30;

        /// <summary>
        /// Whether to enable random curve interference lines
        /// </summary>
        public bool RandomCurve { get; set; } = true;

        /// <summary>
        /// Curve count
        /// </summary>
        public int CurveCount { get; set; } = 2;

        /// <summary>
        /// Background color (hexadecimal, e.g., #FFFFFF)
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Foreground color (hexadecimal, e.g., #000000)
        /// </summary>
        public string ForegroundColor { get; set; } = "#000000";

        /// <summary>
        /// Minimum font size
        /// </summary>
        public int MinFontSize { get; set; } = 20;

        /// <summary>
        /// Maximum font size
        /// </summary>
        public int MaxFontSize { get; set; } = 28;

        /// <summary>
        /// Font family name (e.g., Arial, Microsoft YaHei)
        /// </summary>
        public string FontFamily { get; set; } = "Arial";
    }
}