using System.ComponentModel;

namespace IVSoftware.Portable
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GlyphAttribute : Attribute
    {
        public GlyphAttribute() { }
        public GlyphAttribute(string key) => Key = key;
        public GlyphAttribute(Enum stdEnum) { }
        public string FontFamily
        {
            get =>
                string.IsNullOrEmpty(_fontFamily)
                ? "basics-icons"
                : _fontFamily;
            set
            {
                if (!Equals(_fontFamily, value))
                {
                    _fontFamily = value;
                }
            }
        }
        string _fontFamily = string.Empty;

        public string Key
        {
            get =>
                string.IsNullOrEmpty(_key)
                ? "help-circled-alt"
                : _key;
            set
            {
                if (!Equals(_key, value))
                {
                    _key = value;
                    _stdEnum = null;
                }
            }
        }
        string _key = string.Empty;

        public Enum? StdEnum
        {
            get => _stdEnum;
            set
            {
                if (!Equals(_stdEnum, value))
                {
                    if (value is not null)
                    {
                        _key =
                            value.GetCustomAttribute<DescriptionAttribute>()
                            ?.Description
                            ?? value.ToString();
                    }
                    _stdEnum = value;
                }
            }
        }
        Enum? _stdEnum = default;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CssNameAttribute : Attribute
    {
        public CssNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}
