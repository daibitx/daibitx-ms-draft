using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Daibitx.Localizator
{
    public class ResxResourceProvider : IDisposable
    {
        /// <summary>
        /// Dictionary[Culture][key]=value
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _store
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly ResxLocalizationOptions _options;
        private FileSystemWatcher? _watcher;
        private readonly CancellationTokenSource _cts = new();
        private readonly ILogger<ResxResourceProvider> logger;
        private readonly Regex _cultureRegex;

        public ResxResourceProvider(ResxLocalizationOptions options, ILogger<ResxResourceProvider> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Compile regex for better performance
            _cultureRegex = new Regex(CulturePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            // Initial loading of all qualified .resx files
            LoadAllResources();

            if (_options.EnableWatch)
                StartWatcher();
        }

        /// <summary>
        /// Initial loading of all qualified .resx files
        /// </summary>
        private void LoadAllResources()
        {
            var domainPath = AppDomain.CurrentDomain.BaseDirectory;
            var searchOption = _options.ScanSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                var resxFiles = Directory.GetFiles(domainPath, "*.resx", searchOption);
                var validFiles = resxFiles.Where(f => _cultureRegex.IsMatch(Path.GetFileName(f)));

                foreach (var file in validFiles)
                {
                    LoadResourceFile(file);
                }

                logger.LogInformation("Initial loading completed, loaded {Count} resource files", validFiles.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initially load resource files");
            }
        }

        /// <summary>
        /// Load a single .resx file into memory
        /// </summary>
        private void LoadResourceFile(string filePath)
        {
            try
            {
                var culture = ExtractCultureFromFileName(filePath);
                if (string.IsNullOrEmpty(culture))
                    return;

                var resources = ParseResxFile(filePath);
                var cultureDict = _store.GetOrAdd(culture, _ =>
                    new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase));

                // Clear and reload
                cultureDict.Clear();
                foreach (var kvp in resources)
                {
                    cultureDict[kvp.Key] = kvp.Value;
                }

                logger.LogInformation("Loaded resource file: {FileName} (Culture: {Culture})", Path.GetFileName(filePath), culture);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load resource file: {FilePath}", filePath);
            }
        }

        /// <summary>
        /// Extract culture code from file name
        /// </summary>
        private string? ExtractCultureFromFileName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var match = _cultureRegex.Match(fileName);
            if (match.Success)
            {
                return match.Groups[2].Value; // Group 2 is the culture code
            }
            return null;
        }

        /// <summary>
        /// Parse .resx file content
        /// </summary>
        private Dictionary<string, string> ParseResxFile(string filePath)
        {
            var resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(filePath))
                return resources;

            try
            {
                var doc = XDocument.Load(filePath);
                var dataElements = doc.Root?.Elements("data");

                if (dataElements != null)
                {
                    foreach (var data in dataElements)
                    {
                        var name = data.Attribute("name")?.Value;
                        var value = data.Element("value")?.Value;

                        if (!string.IsNullOrEmpty(name) && value != null)
                        {
                            resources[name] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse resx file: {FilePath}", filePath);
            }

            return resources;
        }

        private void StartWatcher()
        {
            var domainPath = AppDomain.CurrentDomain.BaseDirectory;
            _watcher = new FileSystemWatcher(domainPath, "*.resx")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                IncludeSubdirectories = _options.ScanSubDirectories,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Renamed += OnRenamed;
            _watcher.Deleted += OnDeleted;

            logger.LogInformation("File watcher started, monitoring path: {Path}", domainPath);
        }

        // Regex: xxx.xx-xx.resx or xxx.zh-Hans-CN.resx
        private const string CulturePattern = @"^(.+)\.([a-z]{2,3}(-[A-Z0-9]{2,4})?)\.resx$";

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Only process files matching culture format
                if (!_cultureRegex.IsMatch(e.Name))
                    return;

                logger.LogDebug("Detected resource file change: {FilePath}", e.FullPath);

                // Add delay to avoid file locking, load after 100ms
                Task.Delay(100, _cts.Token).ContinueWith(_ => LoadResourceFile(e.FullPath), TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process file change event: {FilePath}", e.FullPath);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                // Remove old culture data first
                if (_cultureRegex.IsMatch(e.OldName))
                {
                    RemoveFileEntry(e.OldFullPath);
                }

                // Load new file
                if (_cultureRegex.IsMatch(e.Name))
                {
                    LoadResourceFile(e.FullPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process file rename event: {OldName} -> {NewName}", e.OldName, e.Name);
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                RemoveFileEntry(e.FullPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process file deletion event: {FilePath}", e.FullPath);
            }
        }

        /// <summary>
        /// Remove resource entry for the specified path
        /// </summary>
        private void RemoveFileEntry(string path)
        {
            var culture = ExtractCultureFromFileName(path);
            if (!string.IsNullOrEmpty(culture) && _store.TryRemove(culture, out _))
            {
                logger.LogInformation("Removed resource culture: {Culture} (File: {FileName})", culture, Path.GetFileName(path));
            }
        }

        /// <summary>
        /// Get translation for the specified culture and key
        /// </summary>
        public string? GetString(string culture, string key)
        {
            if (_store.TryGetValue(culture, out var cultureDict) &&
                cultureDict.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// Get all loaded cultures
        /// </summary>
        public IReadOnlyList<string> GetCultures() => _store.Keys.ToList();

        public IReadOnlyList<string> GetAllKeys(string culture)
        {
            if (!_store.TryGetValue(culture, out var keyValuePairs))
            {
                return Enumerable.Empty<string>().ToList();
            }
            else
            {
                return keyValuePairs.Keys.ToList();
            }

        }
        public void Dispose()
        {
            _cts.Cancel();
            _watcher?.Dispose();
            logger.LogInformation("Resource provider disposed");
        }
    }
}