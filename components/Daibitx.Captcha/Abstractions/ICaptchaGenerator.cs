namespace Daibitx.Captcha.Abstractions
{
    public interface ICaptchaGenerator
    {
        /// <summary>
        /// Captcha Generator
        /// </summary>
        /// <returns></returns>
        CaptchaResult Generate();
    }

}
