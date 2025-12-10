using Daibitx.Captcha.Abstractions;
using SkiaSharp;

namespace Daibitx.Captcha.Skia
{
    /// <summary>
    /// 基于 SkiaSharp 的验证码生成器实现
    /// </summary>
    public class SkiaCaptchaGenerator : ICaptchaGenerator
    {
        private static readonly Random _random = new Random();
        private readonly CaptchaOptions options;
        public SkiaCaptchaGenerator(CaptchaOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="options">配置选项</param>
        /// <returns>验证码结果</returns>
        public CaptchaResult Generate()
        {
            // 生成随机验证码文本
            string code = GenerateRandomCode(options);

            // 创建图片
            using var bitmap = new SKBitmap(options.Width, options.Height);
            using var canvas = new SKCanvas(bitmap);
            using var paint = new SKPaint();

            // 绘制背景
            DrawBackground(canvas, options);

            // 绘制噪点
            if (options.Noise)
            {
                DrawNoise(canvas, options);
            }

            // 绘制随机曲线
            if (options.RandomCurve)
            {
                DrawRandomCurves(canvas, options);
            }

            // 绘制验证码文本
            DrawCaptchaText(canvas, code, options);

            // 编码为 PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var imageBytes = data.ToArray();

            return new CaptchaResult
            {
                Code = code,
                ImageBytes = imageBytes,
                ContentType = "image/png"
            };
        }

        /// <summary>
        /// 生成随机验证码文本
        /// </summary>
        private static string GenerateRandomCode(CaptchaOptions options)
        {
            if (string.IsNullOrEmpty(options.Charset))
            {
                throw new ArgumentException("字符集不能为空", "options.Charset");
            }

            var chars = options.Charset.ToCharArray();
            var codeChars = new char[options.CodeLength];

            for (int i = 0; i < options.CodeLength; i++)
            {
                codeChars[i] = chars[_random.Next(chars.Length)];
            }

            return new string(codeChars);
        }

        /// <summary>
        /// 绘制背景
        /// </summary>
        private static void DrawBackground(SKCanvas canvas, CaptchaOptions options)
        {
            var backgroundColor = SKColor.Parse(options.BackgroundColor);
            canvas.Clear(backgroundColor);
        }

        /// <summary>
        /// 绘制噪点
        /// </summary>
        private static void DrawNoise(SKCanvas canvas, CaptchaOptions options)
        {
            var foregroundColor = SKColor.Parse(options.ForegroundColor);
            var paint = new SKPaint
            {
                Color = foregroundColor,
                IsAntialias = true
            };

            int pixelCount = (options.Width * options.Height * options.NoiseDensity) / 1000;

            // 绘制随机像素点
            for (int i = 0; i < pixelCount; i++)
            {
                int x = _random.Next(options.Width);
                int y = _random.Next(options.Height);
                canvas.DrawPoint(x, y, paint);
            }

            // 绘制随机短线
            for (int i = 0; i < pixelCount / 5; i++)
            {
                int x1 = _random.Next(options.Width);
                int y1 = _random.Next(options.Height);
                int x2 = x1 + _random.Next(-5, 6);
                int y2 = y1 + _random.Next(-5, 6);
                canvas.DrawLine(x1, y1, x2, y2, paint);
            }
        }

        /// <summary>
        /// 绘制随机曲线干扰线
        /// </summary>
        private static void DrawRandomCurves(SKCanvas canvas, CaptchaOptions options)
        {
            var foregroundColor = SKColor.Parse(options.ForegroundColor);
            var paint = new SKPaint
            {
                Color = foregroundColor,
                IsAntialias = true,
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke
            };

            for (int i = 0; i < options.CurveCount; i++)
            {
                using var path = new SKPath();
                int startX = _random.Next(options.Width);
                int startY = _random.Next(options.Height);
                path.MoveTo(startX, startY);

                int pointCount = _random.Next(3, 6);
                for (int j = 0; j < pointCount; j++)
                {
                    int x = _random.Next(options.Width);
                    int y = _random.Next(options.Height);
                    path.LineTo(x, y);
                }

                canvas.DrawPath(path, paint);
            }
        }

        /// <summary>
        /// 绘制验证码文本
        /// </summary>
        private static void DrawCaptchaText(SKCanvas canvas, string code, CaptchaOptions options)
        {
            var foregroundColor = SKColor.Parse(options.ForegroundColor);
            float charWidth = (float)options.Width / code.Length;
            float baseY = options.Height / 2f;

            for (int i = 0; i < code.Length; i++)
            {
                var paint = CreateTextPaint(options, foregroundColor);
                var text = code[i].ToString();

                // 测量文本
                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                // 计算位置
                float x = i * charWidth + (charWidth - textBounds.Width) / 2;
                float y = baseY + textBounds.Height / 2;

                // 应用扭曲效果
                if (options.Distort)
                {
                    ApplyDistortion(canvas, text, x, y, paint, options);
                }
                else
                {
                    canvas.DrawText(text, x, y, paint);
                }
            }
        }

        /// <summary>
        /// 创建文本绘制画笔
        /// </summary>
        private static SKPaint CreateTextPaint(CaptchaOptions options, SKColor color)
        {
            int fontSize = _random.Next(options.MinFontSize, options.MaxFontSize + 1);

            // 尝试加载指定字体
            SKTypeface? typeface = null;
            try
            {
                typeface = SKTypeface.FromFamilyName(options.FontFamily);
                if (typeface.FamilyName == "null" || typeface.FamilyName == null)
                {
                    typeface = null;
                }
            }
            catch
            {
                typeface = null;
            }

            // 如果指定字体加载失败，使用默认字体
            if (typeface == null)
            {
                typeface = SKTypeface.FromFamilyName("Arial") ?? SKTypeface.Default;
            }

            return new SKPaint
            {
                Color = color,
                IsAntialias = true,
                TextSize = fontSize,
                Typeface = typeface,
                TextAlign = SKTextAlign.Center
            };
        }

        /// <summary>
        /// 应用扭曲效果
        /// </summary>
        private static void ApplyDistortion(SKCanvas canvas, string text, float x, float y, SKPaint paint, CaptchaOptions options)
        {
            // 保存当前状态
            canvas.Save();

            // 移动到文本中心
            var textBounds = new SKRect();
            paint.MeasureText(text, ref textBounds);
            float centerX = x + textBounds.Width / 2;
            float centerY = y;

            canvas.Translate(centerX, centerY);

            // 随机旋转
            float rotation = (float)(_random.NextDouble() * 2 - 1) * options.DistortLevel * 0.5f;
            canvas.RotateDegrees(rotation);

            // 随机缩放
            float scaleX = 1 + (float)(_random.NextDouble() * 0.3 - 0.15) * options.DistortLevel / 50f;
            float scaleY = 1 + (float)(_random.NextDouble() * 0.3 - 0.15) * options.DistortLevel / 50f;
            canvas.Scale(scaleX, scaleY);

            // 移回原位置
            canvas.Translate(-centerX, -centerY);

            // 绘制文本
            canvas.DrawText(text, x, y, paint);

            // 恢复状态
            canvas.Restore();
        }
    }
}