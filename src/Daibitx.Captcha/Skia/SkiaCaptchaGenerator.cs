using Daibitx.Captcha.Abstractions;
using SkiaSharp;

namespace Daibitx.Captcha.Skia
{
    /// <summary>
    /// SkiaSharp-based CAPTCHA generator implementation
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
        /// Generate CAPTCHA
        /// </summary>
        /// <param name="options">Configuration options</param>
        /// <returns>CAPTCHA result</returns>
        public CaptchaResult Generate()
        {
            string code = GenerateRandomCode(options);

            using var bitmap = new SKBitmap(options.Width, options.Height);
            using var canvas = new SKCanvas(bitmap);
            using var paint = new SKPaint();

            DrawBackground(canvas, options);

            if (options.Noise)
            {
                DrawNoise(canvas, options);
            }

            if (options.RandomCurve)
            {
                DrawRandomCurves(canvas, options);
            }

            DrawCaptchaText(canvas, code, options);

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
        /// Generate random CAPTCHA code text
        /// </summary>
        private static string GenerateRandomCode(CaptchaOptions options)
        {
            if (string.IsNullOrEmpty(options.Charset))
            {
                throw new ArgumentException("Character set cannot be empty", "options.Charset");
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
        /// Draw background
        /// </summary>
        private static void DrawBackground(SKCanvas canvas, CaptchaOptions options)
        {
            var backgroundColor = SKColor.Parse(options.BackgroundColor);
            canvas.Clear(backgroundColor);
        }

        /// <summary>
        /// Draw noise
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

            for (int i = 0; i < pixelCount; i++)
            {
                int x = _random.Next(options.Width);
                int y = _random.Next(options.Height);
                canvas.DrawPoint(x, y, paint);
            }

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
        /// Draw random curve interference lines
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
        /// Draw CAPTCHA text
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

                var textBounds = new SKRect();
                paint.MeasureText(text, ref textBounds);

                float x = i * charWidth + (charWidth - textBounds.Width) / 2;
                float y = baseY + textBounds.Height / 2;

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
        /// Create text drawing paint
        /// </summary>
        private static SKPaint CreateTextPaint(CaptchaOptions options, SKColor color)
        {
            int fontSize = _random.Next(options.MinFontSize, options.MaxFontSize + 1);

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
        /// Apply distortion effect
        /// </summary>
        private static void ApplyDistortion(SKCanvas canvas, string text, float x, float y, SKPaint paint, CaptchaOptions options)
        {
            canvas.Save();

            var textBounds = new SKRect();
            paint.MeasureText(text, ref textBounds);
            float centerX = x + textBounds.Width / 2;
            float centerY = y;

            canvas.Translate(centerX, centerY);

            float rotation = (float)(_random.NextDouble() * 2 - 1) * options.DistortLevel * 0.5f;
            canvas.RotateDegrees(rotation);

            float scaleX = 1 + (float)(_random.NextDouble() * 0.3 - 0.15) * options.DistortLevel / 50f;
            float scaleY = 1 + (float)(_random.NextDouble() * 0.3 - 0.15) * options.DistortLevel / 50f;
            canvas.Scale(scaleX, scaleY);

            canvas.Translate(-centerX, -centerY);

            canvas.DrawText(text, x, y, paint);

            canvas.Restore();
        }
    }
}