using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSGlyphProvider.Demo.WinForms
{
    public class GlyphButton : Button
    {
        public Enum? Id
        {
            get => _id;
            set
            {
                if (!Equals(_id, value))
                {
                    _id = value;
                    if(_id is not null)
                    {

                    }
                }
            }
        }
        Enum? _id = default;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = Height;
        }
    }
}
