using IVSoftware.Portable;

namespace IVSGlyphProvider.Demo.Maui
{
    interface IPlatformEnumIdComponent 
        : IEnumIdComponent
        , IView
    {
        Color BackgroundColor { get; set; }
        string Text { get; set; }
        Color TextColor { get; set; }
        double FontSize { get; set; }
        Thickness Padding { get; set; }
        double WidthRequest { get; set; }
    }
    public class EnumIdButton
        : Button
        , IEnumIdComponent
        , IPlatformEnumIdComponent
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
