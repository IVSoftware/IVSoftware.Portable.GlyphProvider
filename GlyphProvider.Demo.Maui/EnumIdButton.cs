using IVSoftware.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSGlyphProvider.Demo.Maui
{
    public class EnumIdButton
        : Button
        , IEnumIdComponent
        , ITextColorComponent
    {
        public EnumIdButton(Enum id)
        {
            EnumId = id;
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

        string ITextColorComponent.TextColor
        { 
            get => TextColor.ToArgbHex();
            set => TextColor = Color.FromArgb(value);
        }

        Enum? _enumId = default;
    }
}
