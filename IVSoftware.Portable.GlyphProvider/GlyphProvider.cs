using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
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
                            if (resourcePath.Contains("config.json", StringComparison.InvariantCultureIgnoreCase))
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
                builder.Add($"\t[Description(\"{name}\")]");
                builder.Add($"\t{localLintTerm(name)},\n");
            }
            builder.Add($"}}");
            var joined = string.Join(Environment.NewLine, builder);
            Debug.WriteLine(joined);
            return joined;

            string localLintTerm(string expr)
            {
                if (string.IsNullOrWhiteSpace(expr)) throw new InvalidOperationException("Requires non-empty term");

                var parts = expr.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
            }
        }
    }

    public class Glyph
    {
        public string Uid { get; set; } = string.Empty;
        public string Css { get; set; } = string.Empty;
        public int Code { get; set; }
        public string Src { get; set; } = string.Empty;
        public bool? Selected { get; set; }
        public SvgDetails Svg { get; set; } = new();
        public List<string> Search { get; set; } = new();
    }

    public class SvgDetails
    {
        public string Path { get; set; } = string.Empty;
        public int Width { get; set; }
    }
}