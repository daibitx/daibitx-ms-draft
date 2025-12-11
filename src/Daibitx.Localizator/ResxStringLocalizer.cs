using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Daibitx.Localizator
{
    public sealed class ResxStringLocalizer : IStringLocalizer
    {
        private readonly ResxResourceProvider _resourceProvider;

        public ResxStringLocalizer(ResxResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                    throw new ArgumentException(nameof(name));
                var culture = CultureInfo.CurrentUICulture.Name;
                var value = _resourceProvider.GetString(culture, name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null) throw new ArgumentNullException(nameof(name));

                var culture = CultureInfo.CurrentUICulture.Name;
                var format = _resourceProvider.GetString(culture, name);

                if (format == null)
                {
                    return new LocalizedString(name, name, resourceNotFound: true);
                }

                try
                {
                    var value = string.Format(CultureInfo.CurrentCulture, format, arguments);
                    return new LocalizedString(name, value, resourceNotFound: false);
                }
                catch (FormatException)
                {
                    return new LocalizedString(name, format, resourceNotFound: false);
                }
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;

            if (includeParentCultures)
            {
                var cultures = GetCultureHierarchy(culture);
                var seenKeys = new HashSet<string>();

                foreach (var currentCulture in cultures)
                {
                    var strings = GetStringsForCulture(currentCulture);
                    foreach (var localizedString in strings)
                    {
                        if (seenKeys.Add(localizedString.Name))
                        {
                            yield return localizedString;
                        }
                    }
                }
            }
            else
            {
                var strings = GetStringsForCulture(culture);
                foreach (var localizedString in strings)
                {
                    yield return localizedString;
                }
            }
        }

        private IEnumerable<LocalizedString> GetStringsForCulture(string culture)
        {
            var keys = _resourceProvider.GetAllKeys(culture);
            foreach (var key in keys)
            {
                var value = _resourceProvider.GetString(culture, key);
                yield return new LocalizedString(key, value ?? key, value == null);
            }
        }

        private IEnumerable<string> GetCultureHierarchy(string culture)
        {
            var current = CultureInfo.GetCultureInfo(culture);

            while (current != CultureInfo.InvariantCulture)
            {
                yield return current.Name;
                current = current.Parent;
            }
        }
    }
}
