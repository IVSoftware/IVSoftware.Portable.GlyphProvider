using IVSoftware.Portable;

namespace IVSGlyphProvider.Demo.WinForms
{
    public class EnumIdButton 
        : Button
        , IEnumIdComponent
    {
        public EnumIdButton(Enum id)
        {
            UseCompatibleTextRendering = true;
            EnumId = id;
        }
        public Enum? EnumId
        {
            get => _id;
            init
            {
                if (!Equals(_id, value))
                {
                    _id = value;
                    if( _id is not null && 
                        _id.GetGlyphAttribute() is { } glyph &&
                        glyph.StdEnum is IconBasics icon)
                    {
                        Font = MainForm.IconBasicsFont;
                        Text = icon.ToGlyph();
                    }
                }
            }
        }
        Enum? _id = default;

        public string TextColor
        {
            get => _textColor;
            set
            {
                if (!Equals(_textColor, value))
                {
                    _textColor = value;
                    OnTextColorChanged();
                }
            }
        }
        string _textColor = string.Empty;

        protected virtual void OnTextColorChanged() 
            => ForeColor = ColorTranslator.FromHtml(TextColor);


        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                if (!Equals(base.ForeColor, value))
                {
                    base.ForeColor = value;
                    TextColor = ColorTranslator.ToHtml(ForeColor);
                }
            }
        }
    }
}
