using IVSoftware.Portable;
using System.Collections;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;

namespace IVSGlyphProvider.Demo.Maui
{
    public class CenteringPanel : ContentView , IConfigurableLayoutStack
    {
        const int DEFAULT_UNIFORM_WIDTH = -1;
        const int MIN_UNIFORM_WIDTH = 20;
        const int DEFAULT_ROW_HEIGHT = 34;
        const int MIN_ROW_HEIGHT = 20;
        const int DEFAULT_FONT_SIZE = 11;
        const int MIN_FONT_SIZE = 7;
        public CenteringPanel() => InitializeComponent();
        private void InitializeComponent() { } // N O O P

        public Grid Grid
        {
            get
            {
                if (_grid is null)
                {
                    _grid = new Grid();
                    Content = _grid;
                    _grid.PropertyChanged += (sender, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(Height):
                                HeightRequest = _grid.Height;
                                break;
                        }
                    };
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
            DisplayFormatOptions displayFormatOptions = DisplayFormatOptions.Auto,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            float? uniformFontSize = null,
            string? uniformBackgroundColor = null,
            string? uniformTextColor = null,
            bool overwriteDefaults = false) where T : struct, Enum
        {
            if (overwriteDefaults) localOverwriteDefaults();
            if(displayFormatOptions == DisplayFormatOptions.Auto)
            {
                displayFormatOptions = orientation switch
                {
                    LayoutOrientation.Vertical => DisplayFormatOptions.ShowSquareIconAndMember,
                    _ => DisplayFormatOptions.ShowSquareIcon,
                };
            }

            Grid.Children.Clear();
            Grid.RowDefinitions.Clear();
            Grid.ColumnDefinitions.Clear();

            var elements = Enum.GetValues<T>().ToList();
            var components = new List<IView>();
            localStageComponentss();
            switch (orientation)
            {
                case LayoutOrientation.Horizontal: localConfigHorizontal(); break;
                case LayoutOrientation.Vertical: localConfigVertical(); break;
                default: throw new NotImplementedException($"Bad case: {orientation}");
            }

            #region L o c a l F x
            void localStageComponentss()
            {
                for (int col = 0; col < elements.Count; col++)
                {
                    var id = elements[col];
                    if (!Cache.TryGetValue(id, out var enumIdButton) || enumIdButton is null)
                    {
                        enumIdButton = ActivatorTemplate.Activate(id);
                        Cache[id] = enumIdButton;
                    }
                    if (enumIdButton is IPlatformEnumIdComponent view)
                    {
                        if (displayFormatOptions.HasFlag(DisplayFormatOptions.IconWidthTracksHeight))
                        {
                            view.WidthRequest = UniformHeightRequest;
                        }
                        view.BackgroundColor = Color.FromArgb("#444444");
                        view.TextColor = Colors.WhiteSmoke;
                        view.FontSize = UniformFontSize;
                        view.Padding = 0;
                        components.Add(view);
                    }
                }
            }
            void localConfigHorizontal()
            {
                Grid.RowDefinitions.Add(new());
                components.ForEach(_ => Grid.ColumnDefinitions.Add(new()));
                HeightRequest = elements.Any() ? RowHeightRequest : 0;
                for (int col = 0; col < components.Count; col++)
                {
                    Grid.Add(components[col], col, 0);
                }
            }
            void localConfigVertical()
            {
                Grid.ColumnDefinitions.Add(new());
                elements.ForEach(_ => Grid.RowDefinitions.Add(new()));
                HeightRequest = elements.Count * RowHeightRequest;
                for (int row = 0; row < elements.Count; row++)
                {
                    var id = elements[row];
                    if (!Cache.TryGetValue(id, out var enumIdButton) || enumIdButton is null)
                    {
                        enumIdButton = ActivatorTemplate.Activate(id);
                        Cache[id] = enumIdButton;
                    }

                    if (enumIdButton is IPlatformEnumIdComponent view)
                    {
                        view.WidthRequest = UniformWidthRequest;
                        view.BackgroundColor = Color.FromArgb("#444444");
                        view.TextColor = Colors.WhiteSmoke;
                        view.FontSize = UniformFontSize;
                        view.Padding = 0;
                        Grid.Add(view, 0, row);
                    }
                }
            }
            void localOverwriteDefaults()
            {
                Orientation = orientation;
                switch (orientation)
                {
                    case LayoutOrientation.Horizontal:
                        HorizontalDisplayFormatOptions = displayFormatOptions;
                        break;
                    case LayoutOrientation.Vertical:
                        VerticalDisplayFormatOptions = displayFormatOptions;
                        break;
                    default:
                        throw new NotImplementedException($"Bad case: {orientation}");
                }

                if (rowHeightRequest.HasValue)
                    RowHeightRequest = rowHeightRequest.Value;

                if (uniformWidthRequest.HasValue)
                    UniformWidthRequest = uniformWidthRequest.Value;

                if (uniformFontSize.HasValue)
                    _uniformFontSize = uniformFontSize.Value;
            }
            #endregion L o c a l F x       
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

        public DisplayFormatOptions HorizontalDisplayFormatOptions
        {
            get => _horizontalDisplayFormatOptions;
            set
            {
                if (value > DisplayFormatOptions.IconWidthTracksHeight && 
                    !Equals(_horizontalDisplayFormatOptions, value))
                {
                    _horizontalDisplayFormatOptions = value;
                    OnPropertyChanged();
                }
            }
        }
        DisplayFormatOptions _horizontalDisplayFormatOptions = DisplayFormatOptions.ShowSquareIcon;

        public DisplayFormatOptions VerticalDisplayFormatOptions
        {
            get => _verticalDisplayFormatOptions;
            set
            {
                if (value > DisplayFormatOptions.IconWidthTracksHeight && 
                    !Equals(_verticalDisplayFormatOptions, value))
                {
                    _verticalDisplayFormatOptions = value;
                    OnPropertyChanged();
                }
            }
        }
        DisplayFormatOptions _verticalDisplayFormatOptions = DisplayFormatOptions.ShowSquareIconAndMember;

        public int RowHeightRequest
        {
            get => _rowHeightRequest;
            set
            {
                if (value > MIN_ROW_HEIGHT && !Equals(_rowHeightRequest, value))
                {
                    _rowHeightRequest = value;
                    OnPropertyChanged();
                }
            }
        }
        int _rowHeightRequest = DEFAULT_ROW_HEIGHT;

        public UniformThickness UniformSpacing
        {
            get => _uniformSpacing;
            set
            {
                if (!Equals(_uniformSpacing, value))
                {
                    _uniformSpacing = value;
                    OnPropertyChanged();
                }
            }
        }
        UniformThickness _uniformSpacing = new(2);
        public int UniformHeightRequest => RowHeightRequest - Math.Max((int)Padding.VerticalThickness, UniformSpacing.Vertical);

        public float UniformFontSize
        {
            get => _uniformFontSize;
            set
            {
                if (value >= MIN_FONT_SIZE && !Equals(_uniformFontSize, value))
                {
                    _uniformFontSize = value;
                    OnPropertyChanged();
                }
            }
        }
        float _uniformFontSize = DEFAULT_FONT_SIZE;

        public int UniformWidthRequest
        {
            get =>
                _uniformWidthRequest == -1
                ? (int)Width / 3
                : _uniformWidthRequest;
            set
            {
                if ((value == -1 || value >= MIN_UNIFORM_WIDTH)
                    && !Equals(_uniformWidthRequest, value))
                {
                    _uniformWidthRequest = value;
                    OnPropertyChanged();
                }
            }
        }
        int _uniformWidthRequest = DEFAULT_UNIFORM_WIDTH;

        public string? UniformBackgroundColor
        {
            get => _uniformBackgroundColor;
            set
            {
                if (!Equals(_uniformBackgroundColor, value))
                {
                    _uniformBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }
        string? _uniformBackgroundColor = "#FF0000";

        public string? UniformTextColor
        {
            get => _uniformTextColor;
            set
            {
                if (!Equals(_uniformTextColor, value))
                {
                    _uniformTextColor = value;
                    OnPropertyChanged();
                }
            }
        }
        string? _uniformTextColor = null;



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
