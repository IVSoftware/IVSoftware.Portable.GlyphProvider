using IVSoftware.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSGlyphProvider.Demo.WinForms
{
    public class GlyphButton : Button
    {
        public GlyphButton() => UseCompatibleTextRendering = true;
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value && IsInitialized);
        }
        public Enum? Id
        {
            get => _id;
            set
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

        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                if (!Equals(_isInitialized, value))
                {
                    _isInitialized = value;
                    if(_isInitialized) Show();
                }
            }
        }
        bool _isInitialized = default;

        Enum? _id = default;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = Height;
        }
    }
}
