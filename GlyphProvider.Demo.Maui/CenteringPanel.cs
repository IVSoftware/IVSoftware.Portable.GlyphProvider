using IVSoftware.Portable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSGlyphProvider.Demo.Maui
{
    public class CenteringPanel : ContentView , IConfigurableLayoutStack
    {
        public int RowHeightRequest
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public UniformThickness UniformThickness
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public int UniformWidthRequest { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IVSoftware.Portable.ControlTemplate ControlTemplate
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public LayoutOrientation Orientation
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        IDictionary IConfigurableLayoutStack.Cache
        {
            get => Cache;
            set
            {
                if (value is Dictionary<Enum, IGlyphButton> cache)
                {
                    Cache = cache;
                }
                else throw new InvalidCastException(
                    $"Expected a {typeof(Dictionary<Enum, IGlyphButton>).FullName}, " +
                    $"but received {value?.GetType().FullName ?? "<null>"}."
                );
            }
        }

        public Dictionary<Enum, IGlyphButton> Cache
        {
            get => _cache;
            set
            {
                if (!Equals(_cache, value))
                {
                    _cache = value;
                    OnPropertyChanged();
                }
            }
        }
        Dictionary<Enum, IGlyphButton> _cache = new();

        public void Configure<T>(
            LayoutOrientation orientation = LayoutOrientation.Horizontal,
            WidthTrackingMode widthMode = WidthTrackingMode.Auto, 
            int? rowHeightRequest = null, 
            int? uniformWidthRequest = null, 
            bool overwriteRequests = false) where T : struct, Enum
        {
        }
    }
}
