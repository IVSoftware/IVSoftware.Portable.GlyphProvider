using IVSoftware.Portable;
using System.Collections;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;

namespace IVSGlyphProvider.Demo.Maui
{
    public class CenteringPanel : ContentView , IConfigurableLayoutStack
    {
        public CenteringPanel() => InitializeComponent();
        private void InitializeComponent()
        {
        }

        public Grid Grid
        {
            get
            {
                if (_grid is null)
                {
                    _grid = new Grid();
                    Content = _grid;
                }
                return _grid;
            }
        }
        Grid? _grid = null;

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
                case LayoutOrientation.Horizontal: localConfigHorizontal(); break;
                case LayoutOrientation.Vertical: localConfigVertical(); break;
                default: throw new NotImplementedException($"Bad case: {Orientation}");
            }
            void localConfigHorizontal()
            {
                Grid.RowDefinitions.Add(new());
                elements.ForEach(_ => Grid.ColumnDefinitions.Add(new()));
                for (int col = 0; col < elements.Count; col++)
                {
                    var id = elements[col];
                    if (!Cache.TryGetValue(id, out var enumIdButton) || enumIdButton is null)
                    {
                        enumIdButton = ActivatorTemplate.Activate(id);
                        Cache[id] = enumIdButton;
                    }

                    if (enumIdButton is View view)
                    {
                        view.BackgroundColor = Color.FromArgb("#444444");
                        Grid.Add(view, col, 0);
                    }

                    if (enumIdButton is ITextColorComponent text)
                    {
                        text.TextColor =  Colors.WhiteSmoke.ToArgbHex();
                    }
                }
            }
            void localConfigVertical()
            {
                Grid.ColumnDefinitions.Add(new());
                elements.ForEach(_ => Grid.RowDefinitions.Add(new()));
            }
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

        public ActivatorTemplate ActivatorTemplate
        {
            get => _activatorTemplate;
            set
            {
                if (value is not null && !Equals(_activatorTemplate, value))
                {
                    _activatorTemplate = value;
                    OnPropertyChanged();
                }
            }
        }
        ActivatorTemplate _activatorTemplate = new ActivatorTemplate<EnumIdButton>();
        IDictionary IConfigurableLayoutStack.Cache
        {
            get => Cache;
            set
            {
                if (value is Dictionary<Enum, IEnumIdComponent> cache)
                {
                    Cache = cache;
                }
                else throw new InvalidCastException(
                    $"Expected a {typeof(Dictionary<Enum, IEnumIdComponent>).FullName}, " +
                    $"but received {value?.GetType().FullName ?? "<null>"}."
                );
            }
        }

        public Dictionary<Enum, IEnumIdComponent> Cache
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
        Dictionary<Enum, IEnumIdComponent> _cache = new();
    }
}
