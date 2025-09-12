using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.Demo
{
    public enum ToolBarEmpty { }
    public enum ToolbarButtons
    {
        [Glyph(typeof(IconBasics), nameof(IconBasics.HelpCircled))]
        Help,

        [Glyph(typeof(IconBasics), nameof(IconBasics.Add))]
        Add,

        [Glyph(typeof(IconBasics), nameof(IconBasics.Edit))]
        Edit,

        [Glyph(typeof(IconBasics), nameof(IconBasics.Delete))]
        Delete,

        [Glyph(typeof(IconBasics), nameof(IconBasics.EllipsisVertical))]
        Menu,
    }
}
