using IVSoftware.Portable;

namespace IVSGlyphProvider.Demo.Maui
{
    public class EnumIdButton
        : Button
        , IEnumIdComponent
        , IView
    {
        public EnumIdButton(Enum id)
        {
            EnumId = id;
            PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Padding):
                        break;
                }
            };
        }
        public Enum EnumId
        {
            get => _enumId ?? (Enum)(object)0;
            init
            {
                if (!Equals(_enumId, value))
                {
                    _enumId = value;
                    if (_enumId is not null &&
                        _enumId.GetGlyphAttribute() is { } glyph &&
                        glyph.StdEnum is IconBasics icon)
                    {
                        FontFamily = nameof(IconBasics);
                        Text = icon.ToGlyph();
                    }
                }
            }
        }
        Enum? _enumId = default;
    }
}
