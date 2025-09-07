
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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

                var exactMatches = _glyphLookup.Keys
                    .Where(k => localNormalizeKey(k).SequenceEqual(keyParts, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (exactMatches.Count == 1)
                {
                    code = _glyphLookup[exactMatches[0]].Code;
                }
                else if (exactMatches.Count > 1)
                {
                    Debug.Fail($"Ambiguous match for key '{key}': {string.Join(", ", exactMatches)}");
                    return fallback;
                }

                if (code == null)
                {
                    var partialMatches = _glyphLookup.Keys
                        .Where(k => localNormalizeKey(k).Intersect(keyParts, StringComparer.OrdinalIgnoreCase).Any())
                        .ToList();

                    if (partialMatches.Count == 1)
                    {
                        code = _glyphLookup[partialMatches[0]].Code;
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
        /// Typically this is driven by a bindable property in XAML.
        /// </summary>

        public static GlyphProvider FromFontConfigJson(string fontName)
        {
            lock (_lock)
            {
                if (!_fontFamilyLookup.TryGetValue(fontName, out var provider))
                {
                    provider = new GlyphProvider(fontName);
                    _fontFamilyLookup[fontName] = provider;
                }
                return provider;
            }
        }
        private static readonly object _lock = new object();

        private static readonly Dictionary<string, GlyphProvider> _fontFamilyLookup
            = new Dictionary<string, GlyphProvider>();

        private Dictionary<string, Glyph> _glyphLookup;

        #region J S O N    P R O P E R T I E S
        public string? Name { get; set; }
        public string? CssPrefixText { get; set; }
        public bool CssUseSuffix { get; set; }
        public bool Hinting { get; set; }
        public int UnitsPerEm { get; set; }
        public int Ascent { get; set; }
        public List<Glyph> Glyphs { get; set; }
        #endregion  J S O N    P R O P E R T I E S

        private GlyphProvider(string resourceName)
        {
            var json = LoadEmbeddedResource(resourceName, "config.json") ?? "{}";
            JsonConvert.PopulateObject(json, this);
            Glyphs ??= new();
            _glyphLookup = Glyphs.ToDictionary(_ => _.Css, _ => _) ?? new();
        }


        private string? LoadEmbeddedResource(string resourceName, string endsWith)
        {
            if (resourceName.GetResourcePath(endsWith, out var asm) is { } resourcePath && asm != null)
            {
                using (var stream = asm.GetManifestResourceStream(resourcePath) ?? throw new Exception())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }
        internal string CreateEnumPrototype()
        {
            var builder = new List<string>();
            builder.Add($"public enum Std{localLintTerm(string.IsNullOrWhiteSpace(Name) ? "Prototype" : Name)}Glyph");
            builder.Add($"{{");
            foreach (var name in _glyphLookup.Keys)
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
    public static class FontExtensions
    {
        public static string? GetResourcePath(this string fontFamily, string endsWith, out Assembly? asm)
        {
            string[]? manifestResourceNames;
            string? resourcePath;


            asm = typeof(GlyphProvider).Assembly;
            manifestResourceNames = asm.GetManifestResourceNames();

            resourcePath = manifestResourceNames
                .FirstOrDefault(_ =>
                _.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase) &&

                    // Try replacing hyphens with underscores FIRST.
                    _.Contains($".{fontFamily.Replace('-', '_')}.") ||
                    // Then try orig.
                    _.Contains($".{fontFamily}.")
                );
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                asm = Assembly.GetEntryAssembly();
                manifestResourceNames = asm?.GetManifestResourceNames();
                if (asm != null)
                {
                    resourcePath = manifestResourceNames
                        ?.FirstOrDefault(_ =>
                        _.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase) &&

                            // Try replacing hyphens with underscores FIRST.
                            _.Contains($".{fontFamily.Replace('-', '_')}.") ||
                            // Then try orig.
                            _.Contains($".{fontFamily}.")
                        );
                }
            }
            return resourcePath;
        }

        public static string ToGlyph(this string fontFamily, string fuzzyKey, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.FromFontConfigJson(fontFamily)[fuzzyKey, format];

        public static string ToGlyph(this string fontFamily, Enum stdGlyph, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.FromFontConfigJson(fontFamily)[stdGlyph, format];

        /// <summary>
        /// - Retrieves a standard attribute applied to an Enum member, or null if not found.
        /// - Throws if multiple attributes of type TAttr are applied to the same Enum member.
        /// - To retrieve intentional multiple attributes, call GetOnePageAttributes() instead.
        /// </summary>
        internal static TAttr? GetCustomAttribute<TAttr>(this Enum opid)
            where TAttr : Attribute
            => opid
                .GetType()
                .GetField(opid.ToString())
                ?.GetCustomAttributes<TAttr>()
                .SingleOrDefault();
    }
}