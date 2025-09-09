using Newtonsoft.Json;
using System.Reflection;

namespace IVSoftware.Portable
{
    public static class GlyphProviderExtensions
    {
        [Obsolete("The asm arg has not been reviewed.")]
        public static string ToGlyph(this Assembly asm, string fontFamily, string fuzzyKey, GlyphFormat format = GlyphFormat.Unicode)
            => GlyphProvider.FromFontConfigJson(asm, fontFamily)[asm, fuzzyKey, format];

        public static string ToGlyph(this Enum stdGlyph, GlyphFormat format = GlyphFormat.Unicode)
        => GlyphProvider.FromFontConfigJson(stdGlyph)[stdGlyph, format];

        public static string CreateEnumPrototype(this Assembly asm, string fontFamily)
            => GlyphProvider.CreateEnumPrototype(asm, fontFamily);

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

        /// <summary>
        /// Produces a key in the form "EnumType.Member".
        /// Useful when the member value alone might be insufficiently unique.
        /// </summary>
        internal static string ToFullKey(this Enum member) =>
            $"{member.GetType().Name}.{member}";

        internal static string ToAssemblyKey(this Enum member) =>
            $"{member.GetType().Assembly.GetName().Name}.{member.GetType().Name}";
    }
}
