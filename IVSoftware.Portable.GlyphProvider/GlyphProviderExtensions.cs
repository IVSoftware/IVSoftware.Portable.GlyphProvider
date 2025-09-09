using Newtonsoft.Json;
using System.Reflection;

namespace IVSoftware.Portable
{
    public static class GlyphProviderExtensions
    {
        public static string ToGlyph(this string fontFamily, string fuzzyKey, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.FromFontConfigJson(fontFamily)[fuzzyKey, format];

        public static string ToGlyph(this string fontFamily, Enum stdGlyph, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.FromFontConfigJson(fontFamily)[stdGlyph, format];

        public static string CreateEnumPrototype(this string fontFamily)
            => GlyphProvider.CreateEnumPrototype(fontFamily);

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
