using IVSoftware.Portable;

namespace IVSGlyphProvider.Demo.WinForms
{
    public class GlyphButton : Button, IGlyphButton
    {
        public GlyphButton(Enum id)
        {
            UseCompatibleTextRendering = true;
            Id = id;
        }
        public Enum? Id
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
    }
}
