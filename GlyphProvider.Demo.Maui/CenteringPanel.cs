using IVSoftware.Portable;
using System.Collections;

namespace IVSGlyphProvider.Demo.Maui
{
    public class CenteringPanel : ContentView , IConfigurableLayoutStack
    {
        public CenteringPanel() => InitializeComponent();
        private void InitializeComponent()
        {
        }

        private readonly Grid Grid = new();

        /// <summary>
        /// Configures a toolbar based on enum values of type T.
        /// - Defaults to a horizontal layout where buttons are square (width tracks height).
        /// - If no row height is specified, the first added element sets the height
        ///   after collapsing container padding and element margin.
        /// - If no uniform width is specified, the behavior depends on orientation
        ///   when widthMode is Auto: for horizontal, width tracks height;
        ///   for vertical, uniform height is set by the first element.
        /// - Once established, row height and element width remain fixed unless
        ///   overwriteRequests is true.
        /// </summary>
        public void Configure<T>(
            LayoutOrientation orientation = LayoutOrientation.Horizontal,
            WidthTrackingMode widthTrackingMode = WidthTrackingMode.Auto,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            bool overwriteRequests = false) where T : struct, Enum
        {
            Orientation = orientation;
            WidthTrackingMode = widthTrackingMode;
            Grid.Children.Clear();
            Grid.RowDefinitions.Clear();
            Grid.ColumnDefinitions.Clear();

            var elements = Enum.GetValues<T>().ToList();
            switch (Orientation)
            {
                case LayoutOrientation.Horizontal:
                    Grid.RowDefinitions.Add(new());
                    elements.ForEach(_ =>Grid.ColumnDefinitions.Add(new()));
                    break;
                case LayoutOrientation.Vertical:
                    Grid.ColumnDefinitions.Add(new());
                    elements.ForEach(_ => Grid.RowDefinitions.Add(new()));
                    break;
                default:
                    throw new NotImplementedException($"Bad case: {Orientation}");
            }
            elements.ForEach(_ => 
            {
                if(Cache.TryGetValue(_, out var glyphButton) && glyphButton is not null)
                {

                }
                else
                {

                }
            });
        }

        public LayoutOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (!Equals(_orientation, value))
                {
                    _orientation = value;
                    OnPropertyChanged();
                }
            }
        }
        LayoutOrientation _orientation = default;

        public WidthTrackingMode WidthTrackingMode
        {
            get => _widthTrackingMode;
            set
            {
                if (!Equals(_widthTrackingMode, value))
                {
                    _widthTrackingMode = value;
                    OnPropertyChanged();
                }
            }
        }
        WidthTrackingMode _widthTrackingMode = default;


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
        public IVSoftware.Portable.ActivatorTemplate ActivatorTemplate
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
    }
}
