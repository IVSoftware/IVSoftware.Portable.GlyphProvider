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

        /// <summary>
        /// Produces a key in the form "AssemblyName.EnumType".
        /// This goes beyond using just GetType().Name:
        /// - Within an AppDomain, type names are unique, so that alone is enough
        ///   when you already hold an enum member.
        /// - But when lookup is string-driven (camel, kebab, underscore),
        ///   or the type isn’t guaranteed to be an enum, the assembly name
        ///   provides a stronger anchor across packages and contexts.
        /// </summary>
        /// <remarks>
        /// - This key identifies the font family dictionary for a given enum type.
        /// - If called with an enum member, only the declaring type is used at this step.
        /// - If called with a Type directly, the result is the same, without needing a member.
        /// - Once the dictionary is located, the member itself (when present) is used as
        ///   the final key to retrieve the glyph.
        /// </remarks>
        internal static string ToGlyphProviderKey(this Enum member) =>
            member.GetType().ToGlyphProviderKey();        
        internal static string ToGlyphProviderKey(this Type type) =>
            $"{type.Assembly.GetName().Name}.{type.Name}";
    }
}
