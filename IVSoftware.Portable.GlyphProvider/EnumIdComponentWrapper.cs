using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable
{
    public class EnumIdComponentMapper : IEnumIdComponentPA
    {
        public EnumIdComponentMapper(object @this)
        {
            _impl =
                @this as IEnumIdComponentPA
                ?? localCreateMap();

            IEnumIdComponentPA localCreateMap()
            {
                throw new NotImplementedException("ToDo");
            }
        }
        IEnumIdComponentPA _impl;

        public Enum? EnumId => _impl.EnumId;

        public string Text
        {
            get => _impl.Text; 
            set => _impl.Text = value;
        }
        public string TextColor 
        {
            get => _impl.TextColor;
            set => _impl.TextColor = value;
        }
        public string BackgroundColor 
        {
            get => _impl.BackgroundColor; 
            set => _impl.BackgroundColor = value;
        }
        public double FontSize
        { 
            get => _impl.FontSize;
            set => _impl.FontSize = value;
        }
        public UniformThickness Padding 
        {
            get => _impl.Padding; 
            set => _impl.Padding = value;
        }
        public double WidthRequest
        {
            get => _impl.WidthRequest;
            set => _impl.WidthRequest = value; 
        }
        public DisplayFormatOptions DisplayFormatOptions
        {
            get => _impl.DisplayFormatOptions;
            set => _impl.DisplayFormatOptions = value; 
        }
    }
}
