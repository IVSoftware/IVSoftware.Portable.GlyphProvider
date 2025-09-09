using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public GlyphProvider(Assembly? asm = null) => Assembly = asm;

        [JsonIgnore]
        public Assembly? Assembly { get; }

        [JsonIgnore]
        public string Key { get; private set; } = null!;

        /// <summary>
        /// Generates a prototype enum definition for the specified glyph family,
        /// based on the names discovered in its embedded config.json resource.
        /// </summary>
        /// <remarks>
        /// This is intended as a developer aid during debugging and 
        /// the output may require some manual tweaking.
        /// </remarks>
        public static string CreateEnumPrototype(Assembly asm, string fontName)
            => FromFontConfigJson(asm, fontName).CreateEnumPrototype();

        /// <summary>
        /// Returns the glyph string in the requested GlyphFormat or
        /// the first character of the key (uppercased) if not found.
        /// </summary>
        /// <remarks>
        /// - Basically: 
        ///   We have no control over the glyph names found in the
        ///   config.json file, and these are often in kebab format. Likewise,
        ///   while we offer a utility to make a linted enum from the config.json
        ///   there is no requirement for the end user dev to take this approach.
        /// - So, for this to work:
        ///   Preferred: 
        ///     Make a [Description] attribute with the exact case-sensitive
        ///     css name from the config.json file. This way, the enum member
        ///     gets to be something like "Help" instead of "help-circled-alt"
        ///   Otherwise:
        ///     The StdEnum member bust be a case-sensitive match, bearing
        ///     in mind that that spaces and hyphens are not allowed. Which
        ///     translates to "this almost never works".
        /// </remarks>
        public string this[Enum stdEnum, GlyphFormat format = GlyphFormat.Unicode]
        {
            get
            {
                throw new NotImplementedException("ToDo");
            }
        }

        /// <summary>
        /// Retrieves a glyph string in the requested GlyphFormat.
        /// </summary>
        /// <remarks>
        /// - This overload is heuristic: 
        ///   The key is normalized (camelCase, kebab, underscore) 
        ///   and matched against known glyphs and returns the first. 
        /// - If a unique match is found, returns the glyph (e.g., "\uE801" or "&#xE801;").  
        /// - If ambiguous or not found, returns the first character of the key, uppercased.
        /// </remarks>
        public string this[Assembly asm, string fuzzyKey, GlyphFormat format = GlyphFormat.Unicode]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(fuzzyKey))
                    return string.Empty;

                string fallback = fuzzyKey.First().ToString().ToUpper();
                int? code = null;

                var keyParts = localNormalizeKey(fuzzyKey);

                var exactMatches = GlyphLookup.Keys
                    .Where(k => localNormalizeKey(k).SequenceEqual(keyParts, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (exactMatches.Count == 1)
                {
                    code = GlyphLookup[exactMatches[0]].Code;
                }
                else if (exactMatches.Count > 1)
                {
                    Debug.Fail($"Ambiguous match for key '{fuzzyKey}': {string.Join(", ", exactMatches)}");
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
                            Debug.Fail($"Ambiguous partial match for key '{fuzzyKey}': {string.Join(", ", partialMatches)}");
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

        private static readonly object _lock = new object();

        /// <summary>
        /// One option is to drive this using a bindable property in XAML.
        /// </summary>
        [Obsolete]
        public static GlyphProvider FromFontConfigJson(Assembly asm, string fontName)
        {
            lock (_lock)
            {
                _ = FontFamilyLookupProvider.TryGetValue(fontName, out var provider);
                return provider ?? new(asm);
            }
        }

        public static GlyphProvider FromFontConfigJson(Enum stdEnum)
        {
            lock (_lock)
            {
                _ = FontFamilyLookupProvider.TryGetValue(stdEnum, out var provider);
                return provider ?? new();
            }
        }

        static class FontFamilyLookupProvider
        {
            static TaskCompletionSource _ready = new ();
            static FontFamilyLookupProvider()
            {
                Task.Run(() =>
                {
                    var all = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(_ =>
                            !string.IsNullOrEmpty(_.Location) &&
                            !_.Location.Contains("Microsoft.NETCore", StringComparison.OrdinalIgnoreCase) &&
                            !_.Location.Contains("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    { }
#if DEBUG
                    if (AppDomain.CurrentDomain
                        .GetAssemblies().FirstOrDefault(_ => _.GetName().Name?.Contains("IVSoftware.Portable") == true)
                        is { } ivs)
                    {
                        var ivsNames = ivs.GetManifestResourceNames();
                    }
#endif

                    foreach (var asm in all)
                    {
                        var cMe1 = asm.GetName().Name;
                        var cMe2 = asm.GetManifestResourceNames();
                        foreach (var resourcePath in cMe2)
                        {
                            if (resourcePath.EndsWith("config.json", StringComparison.InvariantCultureIgnoreCase))
                            {
                                using var stream = asm.GetManifestResourceStream(resourcePath) ?? throw new Exception();
                                using var reader = new StreamReader(stream);
                                var json = reader.ReadToEnd();
                                var glyphProvider = new GlyphProvider(asm);
                                JsonConvert.PopulateObject(json, glyphProvider);
                                if (string.IsNullOrWhiteSpace(glyphProvider?.Name))
                                {
                                    Debug.Fail("ADVISORY - Defective config file.");
                                }
                                _impl[glyphProvider.Key] = glyphProvider;
                            }
                        }
                    }
                    _ready.SetResult();
                });
            }
#if false

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
#endif
            private static Dictionary<string, GlyphProvider> _impl = new();

            public static string[] Keys => _impl.Keys.ToArray();

            //public static GlyphProvider this[Enum stdEnum]
            //{
            //    get 
            //    {
            //        if (_impl.TryGetValue(stdEnum.GetType().FullName!, out var glyphProvider))
            //        {
            //            return glyphProvider;
            //        }
            //        else return null;
            //    }
            //    set => throw new NotImplementedException("ToDo");
            //}

            //public GlyphProvider this[Assembly asm, string css]
            //{
            //    set
            //    {
            //        var key = $"{asm.GetName().Name}.{localToPascalCase(css)}";
            //        if(_impl.ContainsKey(key))
            //        {
            //            Debug.Fail("ADVISORY - Check concurrency.");
            //        }
            //        _impl[key] = value;
            //    }
            //}
            public static bool TryGetValue(Enum stdEnum, out GlyphProvider? provider)
            {
                var key = stdEnum.ToAssemblyKey();
                if(Keys.Any())
                {
                    foreach (var knownkey in Keys)
                    {

                    }                    
                }
                return _impl.TryGetValue(key, out provider);
            }

            [Obsolete]
            internal static bool TryGetValue(string fontName, out GlyphProvider provider)
            {
                throw new NotImplementedException();
            }

            public static async Task WaitAsync() =>
                await _ready.Task.ConfigureAwait(false);
        }

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

        enum DefaultId {  Create };
        static uint _autoIdCount = 0;
        static DefaultId getAutoId() => (DefaultId)(++_autoIdCount);
        public string Name
        {
            get => _name;
            set
            {
                if (!Equals(_name, value))
                {
                    _name = value;
                    Key = $"{Assembly?.GetName().Name ?? getAutoId().ToFullKey()}.{localToPascalCase(_name)}";
                }
                #region L o c a l F x		
                string localToPascalCase(string input)
                {
                    if (string.IsNullOrWhiteSpace(input))
                        throw new ArgumentException("Requires non-empty input", nameof(input));

                    // Split on hyphen, underscore, or whitespace
                    var parts = input.Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var sb = new StringBuilder(input.Length);
                    foreach (var part in parts)
                    {
                        sb.Append(char.ToUpperInvariant(part[0]));
                        if (part.Length > 1)
                            sb.Append(part.Substring(1));
                    }

                    // Ensure identifier does not start with a digit
                    if (char.IsDigit(sb[0]))
                        sb.Insert(0, '_');

                    return sb.ToString();
                }
                #endregion L o c a l F x
            }
        }
        string _name = string.Empty;

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
        public static bool CopyEmbeddedFontsFromPackage(
                string? targetDir = null,
                bool overwrite = false,
                bool includeEnumDefinition = false
            )
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
                enumPath = null,
                dir = null,
                sub,
                fileName,
                fqPath;
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
                    fqPath = Path.Combine(
                        dir,
                        fileName);
                    if (overwrite || !File.Exists(fqPath))
                    {
                        using var copyToFile = File.Create(fqPath);
                        stream.CopyTo(copyToFile);
                    }
                }
            }
            if (includeEnumDefinition && dir is not null)
            {
                enumPath ??= Path.Combine(dir, $"{nameof(IconBasics)}.Enum.cs");
                if (overwrite || !File.Exists(enumPath))
                {
                    var builder = new List<string>();
                    builder.Add($"using System.ComponentModel;");
                    builder.Add($"");
                    builder.Add($"namespace {typeof(IconBasics).Namespace};");
                    builder.Add($"public enum {nameof(IconBasics)}");
                    builder.Add($"{{");
                    foreach (var member in Enum.GetValues<IconBasics>())
                    {
                        if (member.GetCustomAttribute<DescriptionAttribute>()?.Description is { } description)
                        {
                            builder.Add($"\t[Description(\"{description}\")]");
                        }
                        builder.Add($"\t{member.ToString()},");
                    }
                    builder.Add($"}}");
                    builder.Add($"");

                    var joined = string.Join(Environment.NewLine, builder);
                    File.WriteAllText(enumPath, joined);
                }
            }
            return true;
        }

        public static Dictionary<T, Glyph?> GetGlyphs<T>(bool allowAppDomainFallback = false)
            where T : Enum
        {
            var keys = FontFamilyLookupProvider.Keys;
            { }
            throw new NotImplementedException("ToDo");
        }

        /// <summary>
        /// Kick off a background task that preloads all GlyphProvider caches.
        /// Safe to call multiple times; subsequent calls are ignored.
        /// </summary>
        public static async Task BoostCache()
        {
            if (_started) return;
            _started = true;

            _ = FontFamilyLookupProvider.Keys;
            await FontFamilyLookupProvider.WaitAsync();
        }
        private static bool _started = false;
    }
}