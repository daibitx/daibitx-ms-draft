using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daibitx.Captcha.Abstractions
{
    /// <summary>
    /// 验证码配置选项
    /// </summary>
    public class CaptchaOptions
    {
        /// <summary>
        /// 图片宽度（像素）
        /// </summary>
        public int Width { get; set; } = 120;

        /// <summary>
        /// 图片高度（像素）
        /// </summary>
        public int Height { get; set; } = 40;

        /// <summary>
        /// 验证码字符长度
        /// </summary>
        public int CodeLength { get; set; } = 4;

        /// <summary>
        /// 验证码字符集
        /// </summary>
        public string Charset { get; set; } = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        /// <summary>
        /// 是否启用噪点
        /// </summary>
        public bool Noise { get; set; } = true;

        /// <summary>
        /// 噪点密度（0-100）
        /// </summary>
        public int NoiseDensity { get; set; } = 15;

        /// <summary>
        /// 是否启用字符扭曲
        /// </summary>
        public bool Distort { get; set; } = true;

        /// <summary>
        /// 扭曲程度（0-100）
        /// </summary>
        public int DistortLevel { get; set; } = 30;

        /// <summary>
        /// 是否启用随机曲线干扰线
        /// </summary>
        public bool RandomCurve { get; set; } = true;

        /// <summary>
        /// 曲线数量
        /// </summary>
        public int CurveCount { get; set; } = 2;

        /// <summary>
        /// 背景颜色（十六进制，如 #FFFFFF）
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// 前景颜色（十六进制，如 #000000）
        /// </summary>
        public string ForegroundColor { get; set; } = "#000000";

        /// <summary>
        /// 最小字体大小
        /// </summary>
        public int MinFontSize { get; set; } = 20;

        /// <summary>
        /// 最大字体大小
        /// </summary>
        public int MaxFontSize { get; set; } = 28;

        /// <summary>
        /// 字体名称（如 Arial, Microsoft YaHei）
        /// </summary>
        public string FontFamily { get; set; } = "Arial";
    }
}
