using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace IVSoftware.Portable
{
    public enum GlyphFormat
    {
        Unicode,
        UnicodeDisplay,
        Xaml,
    }
    /// <summary>
    /// Extracts the friendly glyph names by discovering the 'config.json' 
    /// embedded resource corresponding to the name of glyph font ttf file.
    /// </summary>
    public class GlyphProvider
    {
        /// <summary>
        /// Generates a prototype enum definition for the specified glyph family,
        /// based on the names discovered in its embedded config.json resource.
        /// </summary>
        /// <remarks>
        /// This is intended as a developer aid during debugging and 
        /// the output may require some manual tweaking.
        /// </remarks>
        public static string CreateEnumPrototype(string fontName)
            => FromFontConfigJson(fontName).CreateEnumPrototype();

        /// <summary>
        /// Retrieves the glyph string for the given enum member.
        /// </summary>
        /// <remarks>
        /// If the enum member has a <see cref="DescriptionAttribute"/>, its value is used
        /// for lookup (e.g., "help-circled-alt"). Otherwise the enum name itself is used.
        /// Returns the glyph string in the requested <see cref="GlyphFormat"/>, or
        /// the first character of the key (uppercased) if not found.
        /// </remarks>
        public string this[Enum key, GlyphFormat format = GlyphFormat.Unicode]
            => this[key
                    .GetCustomAttribute<DescriptionAttribute>()
                    ?.Description
                    ?? key.ToString(), format];

        /// <summary>
        /// Retrieves a glyph string in the requested <see cref="GlyphFormat"/>.
        /// </summary>
        /// <remarks>
        /// - This overload is heuristic: 
        ///   The key is normalized (camelCase, kebab, underscore) 
        ///   and matched against known glyphs and returns the first. 
        /// - If a unique match is found, returns the glyph (e.g., "\uE801" or "&#xE801;").  
        /// - If ambiguous or not found, returns the first character of the key, uppercased.
        /// </remarks>
        public string this[string key, GlyphFormat format = GlyphFormat.Unicode]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                    return string.Empty;

                string fallback = key.First().ToString().ToUpper();
                int? code = null;

                var keyParts = localNormalizeKey(key);

                var exactMatches = GlyphLookup.Keys
                    .Where(k => localNormalizeKey(k).SequenceEqual(keyParts, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (exactMatches.Count == 1)
                {
                    code = GlyphLookup[exactMatches[0]].Code;
                }
                else if (exactMatches.Count > 1)
                {
                    Debug.Fail($"Ambiguous match for key '{key}': {string.Join(", ", exactMatches)}");
                    return fallback;
                }

                if (code == null)
                {
                    var partialMatches = GlyphLookup.Keys
                        .Where(k => localNormalizeKey(k).Intersect(keyParts, StringComparer.OrdinalIgnoreCase).Any())
                        .ToList();

                    if (partialMatches.Count == 1)
                    {
                        code = GlyphLookup[partialMatches[0]].Code;
                    }
                    else
                    {
                        if (partialMatches.Count > 1)
                            Debug.Fail($"Ambiguous partial match for key '{key}': {string.Join(", ", partialMatches)}");
                        return fallback;
                    }
                }
                return code.HasValue
                ? format switch
                {
                    GlyphFormat.Unicode => char.ConvertFromUtf32(code.Value),
                    GlyphFormat.UnicodeDisplay => $"U+{code.Value:X4}",
                    GlyphFormat.Xaml => $"&#x{code.Value:X};",
                    _ => fallback
                }
                : fallback;

                #region L o c a l   M e t h o d
                string[] localNormalizeKey(string input)
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return Array.Empty<string>();

                    // Replace delimiters with space, then split camelCase and spaces
                    string sanitized = Regex.Replace(input, @"[-_]", " ");
                    return Regex
                        .Split(sanitized, @"(?<=[a-z0-9])(?=[A-Z])|\s+")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();
                }
                #endregion
            }
        }

        /// <summary>
        /// One option is to drive this using a bindable property in XAML.
        /// </summary>
        public static GlyphProvider FromFontConfigJson(string fontName)
        {
            lock (_lock)
            {
                _ = FontFamilyLookup.TryGetValue(fontName, out var provider);
                return provider ?? new();
            }
        }
        private static readonly object _lock = new object();

        /// <summary>
        /// Discovers and caches embedded font resources across the current app domain, 
        /// building a lookup keyed by the font family name specified in each config.json file.
        /// </summary>
        /// <remarks>
        /// - Scans non-framework assemblies only, filtering resource names that end with config.json.  
        /// - Each JSON is deserialized into a GlyphProvider and added to the lookup.  
        /// - If multiple configs specify the same name, the last one discovered overwrites earlier entries.  
        /// </remarks>
        private static Dictionary<string, GlyphProvider> FontFamilyLookup
        {
            get
            {
                if (_fontFamilyLookup is null)
                {
                    _fontFamilyLookup = new Dictionary<string, GlyphProvider>();

                    foreach (var asm in AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(_ =>
                            !string.IsNullOrEmpty(_.Location) &&
                            !_.Location.Contains("Microsoft.NETCore", StringComparison.OrdinalIgnoreCase) &&
                            !_.Location.Contains("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase)))
                    {
                        foreach (var resourcePath in asm.GetManifestResourceNames())
                        {
                            if (resourcePath.EndsWith("config.json", StringComparison.InvariantCultureIgnoreCase))
                            {
                                using var stream = asm.GetManifestResourceStream(resourcePath) ?? throw new Exception();
                                using var reader = new StreamReader(stream);
                                var json = reader.ReadToEnd();
                                var glyphProvider = JsonConvert.DeserializeObject<GlyphProvider>(json);
                                if (string.IsNullOrWhiteSpace(glyphProvider?.Name))
                                {
                                    Debug.Fail("ADVISORY - Defective config file.");
                                }
                                else
                                {
                                    _fontFamilyLookup[glyphProvider.Name] = glyphProvider;
                                }
                            }
                        }
                    }
                }
                return _fontFamilyLookup;
            }
        }
        private static Dictionary<string, GlyphProvider>? _fontFamilyLookup = null;

        public Dictionary<string, Glyph> GlyphLookup
        {
            get
            {
                if (_glyphLookup is null)
                {
                    _glyphLookup = Glyphs.ToDictionary(_ => _.Css, _ => _);
                }
                return _glyphLookup;
            }
        }
        Dictionary<string, Glyph>? _glyphLookup = null;

        #region J S O N    P R O P E R T I E S
        public string? Name { get; set; }
        public string? CssPrefixText { get; set; }
        public bool CssUseSuffix { get; set; }
        public bool Hinting { get; set; }
        public int UnitsPerEm { get; set; }
        public int Ascent { get; set; }
        public List<Glyph> Glyphs { get; set; } = [];
        #endregion  J S O N    P R O P E R T I E S

        internal string CreateEnumPrototype()
        {
            var enumType = localLintTerm(string.IsNullOrWhiteSpace(Name) ? "Prototype" : Name);

            var builder = new List<string>();
            if(enumType.Contains("glyph", StringComparison.InvariantCultureIgnoreCase) ||
               enumType.Contains("icon", StringComparison.InvariantCultureIgnoreCase))
            {
                builder.Add($"public enum Std{enumType}");
            }
            else
            {
                builder.Add($"public enum Std{enumType}Glyph");
            }
            builder.Add($"{{");
            foreach (var name in GlyphLookup.Keys)
            {
                // Only allow glyph names that can be linted into valid C# identifiers.
                // Regex: letters, digits, underscore, and hyphen (lint strips hyphen/underscore).
                if (!Regex.IsMatch(name, @"^[A-Za-z0-9\-_]+$"))
                {
                    continue;
                }
                builder.Add($"\t[Description(\"{name}\")]");
                builder.Add($"\t{localLintTerm(name)}");
                builder.Add(string.Empty);
            }
            if(builder.Count > 1) 
                builder.Remove(builder.Last());
            builder.Add($"}}");
            var joined = string.Join(Environment.NewLine, builder);
            Debug.WriteLine(joined);
            return joined;

            string localLintTerm(string expr)
            {
                if (string.IsNullOrWhiteSpace(expr)) throw new InvalidOperationException("Requires non-empty term");

                var parts = expr.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var aspirant = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));

                // Ensure identifier does not start with a digit
                if (char.IsDigit(aspirant[0]))
                {
                    aspirant = "_" + aspirant;
                }
                return aspirant;
            }
        }


        public static string ListDomainFontConfigs()
        {
            var builder = new List<string>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmFile;
                try { asmFile = asm.Location; }
                catch { asmFile = "(dynamic / no location)"; }

                string[] resources;
                try { resources = asm.GetManifestResourceNames(); }
                catch { resources = Array.Empty<string>(); }

                foreach (var res in resources.Where(r => r.EndsWith("config.json", StringComparison.OrdinalIgnoreCase)))
                {
                    string? fontName = null;
                    try
                    {
                        using var stream = asm.GetManifestResourceStream(res);
                        using var reader = new StreamReader(stream ?? Stream.Null);
                        var json = reader.ReadToEnd();
                        var provider = JsonConvert.DeserializeObject<GlyphProvider>(json);
                        fontName = provider?.Name;
                    }
                    catch { }

                    builder.Add($"Assembly: {asm.GetName().Name}");
                    builder.Add($"File: {asmFile}");
                    builder.Add($"Resource: {res}");
                    builder.Add($"Family: {fontName}");
                    builder.Add($"");
                }
            }
            var joined = string.Join(Environment.NewLine, builder.SkipLast(1));
            return joined;
        }

        public static string ListDomainFontResources()
        {
            var builder = new List<string>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmFile;
                try { asmFile = asm.Location; }
                catch { asmFile = "(dynamic / no location)"; }

                string[] resources;
                try { resources = asm.GetManifestResourceNames(); }
                catch { resources = Array.Empty<string>(); }

                foreach (var res in resources.Where(r => r.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)))
                {
                    builder.Add($"Assembly: {asm.GetName().Name}");
                    builder.Add($"File: {asmFile}");
                    builder.Add($"Resource: {res}");
                    builder.Add($"");
                }
            }

            var joined = string.Join(Environment.NewLine, builder.SkipLast(1));
            return joined;
        }
        record GlyphConfigReport(
            string AssemblyName,
            string AssemblyFile,
            string ResourceName,
            string? FontFamilyName
        );

        record GlyphFontReport(
            string AssemblyName,
            string AssemblyFile,
            string ResourceName
        );

        /// <summary>
        /// Fallback utility to extract the font file from this package.
        /// </summary>
        public static void CopyBasicsIconsTtf(string destinationFolder)
        {
            const string resourceName = "IVSoftware.Portable.Resources.Fonts.Basics.font.basics-icons.ttf";

            using var stream = typeof(GlyphProvider)
                .Assembly
                .GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Resource not found: {resourceName}");

            var destPath = Path.Combine(destinationFolder, "basics-icons.ttf");
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

            using var file = File.Create(destPath);
            stream.CopyTo(file);
        }

        /// <summary>
        /// Kick off a background task that preloads all GlyphProvider caches.
        /// Safe to call multiple times; subsequent calls are ignored.
        /// </summary>
        public static void BoostCache()
        {
            if (_started) return;
            _started = true;

            Task.Run(() =>
            {
                try
                {
                    // Force discovery of all font families
                    var families = FontFamilyLookup.Keys.ToList();

                    foreach (var family in families)
                    {
                        // Resolve the provider
                        var provider = GlyphProvider.FromFontConfigJson(family);

                        // Touch its glyph lookup to force dictionary construction
                        _ = provider.GlyphLookup.Count;

                        // Optionally: touch the first glyph to also warm up format switches
                        if (provider.GlyphLookup.Count > 0)
                        {
                            var firstKey = provider.GlyphLookup.Keys.First();
                            _ = provider[firstKey, GlyphFormat.Unicode];
                            _ = provider[firstKey, GlyphFormat.Xaml];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GlyphProviderWarmup exception: {ex}");
                }
            });
        }
        private static bool _started = false;

        /// <summary>
        /// Find or optionally create the Resources\Fonts directory
        /// </summary>
        public static bool TryGetFontsDirectory(out string? dir, bool allowCreate = false)
        {
            var baseDir = AppContext.BaseDirectory;

            // Start at the base dir and walk upward until we hit "bin" or root
            var current = new DirectoryInfo(baseDir);
            string? aspirant = null;
            while (current.Parent is not null)
            { 
                if(current.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
                {
                    aspirant = Path.Combine(current.Parent.FullName, "Resources", "Fonts");
                    break;
                }
                current = current.Parent;
            }

            dir = null;
            if (aspirant is not null)
            {
                if (Directory.Exists(aspirant))
                {
                    dir = aspirant;
                }
                else if (allowCreate)
                {
                    Directory.CreateDirectory(aspirant);
                    dir = aspirant;
                    var readmePath = Path.Combine(dir, "autogen-readme.md");
                    if (!File.Exists(readmePath))
                    {
                        File.WriteAllText(readmePath, @"
This folder is created by `IVSoftware.Portable.GlyphProvider`.
It is the default location for copied font resources.
You can safely remove this file once other assets are present.".TrimStart());
                    }
                }
            }
            return dir is not null;
        }

        /// <summary>
        /// Specialized debug-time utility to copy out the 
        /// fonts file structure from this package specifically.
        /// </summary>
        public static bool CopyEmbeddedFontsFromPackage(string? targetDir = null, bool overwrite = false)
        {
            if(targetDir is null)
            {
                if (!TryGetFontsDirectory(out targetDir, true))
                {
                    return false;
                }
            }
            const string CONTAINS = ".Resources.Fonts.";
            var asm = typeof(GlyphProvider).Assembly;
            var names = 
                asm.GetManifestResourceNames()
                .Where(_ => _.Contains(CONTAINS));
            string? 
                resourceRoot = null,
                sub,
                dir,
                fileName,
                fqpath;
            foreach (var resourceName in names)
            {
                using var stream = asm.GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    resourceRoot ??=
                        resourceName[..(resourceName.LastIndexOf(CONTAINS) + CONTAINS.Length)];
                    fileName =
                        string.Join(
                        ".", resourceName.Split('.')
                        .Reverse().Take(2).Reverse());
                    sub =
                        resourceName[resourceRoot.Length..^fileName.Length]
                        .Replace('.', Path.DirectorySeparatorChar);
                    dir = Path.Combine(targetDir!, sub);
                    Directory.CreateDirectory(dir);
                    fqpath = Path.Combine(
                        dir,
                        fileName);
                    if (overwrite || !File.Exists(fqpath))
                    {
                        using var copyToFile = File.Create(fqpath);
                        stream.CopyTo(copyToFile);
                    }
                }
            }
            return true;
        }
    }
}