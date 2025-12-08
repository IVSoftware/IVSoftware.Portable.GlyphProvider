using IVSoftware.Portable;
using IVSoftware.Portable.Demo;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices.Marshalling;
using Color = Microsoft.Maui.Graphics.Color;

namespace IVSGlyphProvider.Demo.Maui
{
    using static IVSoftware.Portable.Demo.Constants;

    public class CenteringPanel : ContentView
    {
        public CenteringPanel() => InitializeComponent();
        private void InitializeComponent() { } // N O O P

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
#if false
new CenteringPanel().InteropConfigure<T>(
                orientation,
                displayFormatOptions,
                rowHeightRequest,
                uniformWidthRequest,
                uniformFontSize,
                uniformBackgroundColor,
                uniformTextColor,
                overwriteDefaults
            );
#else
#endif
            // You must do this first, before we perform auto on anything.
            if (overwriteDefaults) localOverwriteDefaults();
            localSetEphemeralComponentColors();
            Grid.Children.Clear();
            Grid.RowDefinitions.Clear();
            Grid.ColumnDefinitions.Clear();

            var elements = Enum.GetValues<T>();
            var components = new List<IView>();
            localStageComponents();
            switch (orientation)
            {
                case LayoutOrientation.Horizontal: localConfigHorizontal(); break;
                case LayoutOrientation.Vertical: localConfigVertical(); break;
                default: throw new NotImplementedException($"Bad case: {orientation}");
            }

            #region L o c a l F x
            void localSetEphemeralComponentColors()
            {
                Debug.Assert(DateTime.Now.Date == new DateTime(2025, 12, 04).Date, "Don't forget disabled");
                // uniformBackgroundColor ??= BackgroundColor.ToArgbHex();
                uniformBackgroundColor ??= "#DDDDDD";
                uniformTextColor ??= "#222222";
            }

            // Find or create component.
            void localStageComponents()
            {
                if (displayFormatOptions == DisplayFormatOptions.Auto)
                {
                    Debug.Assert(DateTime.Now.Date == new DateTime(2025, 12, 04).Date, "Don't forget disabled");
                    displayFormatOptions = orientation switch
                    {
                        LayoutOrientation.Vertical => DisplayFormatOptions.ShowMember, // TODO: Icon + Member
                        _ => DisplayFormatOptions.ShowSquareIcon,
                    };
                }
#if DEBUG
                else
                {   /* G T K */
                }
#endif
                for (int col = 0; col < elements.Length; col++)
                {
                    var id = elements[col];
                    // [Remember]
                    // - EUD can preload the cache
                    // - OR reference an external system-wide Cache that is preloaded.
                    if (!Cache.TryGetValue(id, out var enumIdButton) || enumIdButton is null)
                    {
                        enumIdButton = ActivatorTemplate.Activate(id);
                        Cache[id] = enumIdButton;
                    }
                    // Migration path
                    if (enumIdButton.As<IEnumIdComponentInterop>() is { } eicpa1)
                    {
                        switch (displayFormatOptions)
                        {
                            case DisplayFormatOptions.ShowSquareIcon:
                                eicpa1.WidthRequest = UniformHeightRequest;
                                break;
                            case DisplayFormatOptions.ShowIcon:
                            case DisplayFormatOptions.ShowMember:
                            case DisplayFormatOptions.ShowSquareIconAndMember:
                                eicpa1.WidthRequest = UniformWidthRequest;
                                break;
                            case DisplayFormatOptions.Auto:
                                throw new InvalidOperationException("S.B. Already handled.");
                            default:
                                throw new NotImplementedException($"Bad case: {displayFormatOptions}");
                        }
                        eicpa1.BackgroundColor = uniformBackgroundColor;
                        eicpa1.TextColor = uniformTextColor;
                        eicpa1.FontSize = UniformFontSize;
                        eicpa1.Padding = 0;
                        if (enumIdButton is IView view)
                        {
                            components.Add(view);
#if DEBUG
                            if(enumIdButton is Button btn)
                            {
                                // band aid
                                Debug.Assert(DateTime.Now.Date == new DateTime(2025, 12, 04).Date, "Don't forget disabled");
                                Debug.WriteLine($"250920.B");
                                Debug.WriteLine($"WidthRequest: {btn.WidthRequest}");
                                btn.WidthRequest = UniformHeightRequest;
                            }
#endif
                        }
                        else throw new InvalidOperationException("Expecting IView");
                    }
                    else if (enumIdButton.As<PlatformComponentInterop>() is { } eicpa0)
                    {
                        switch (displayFormatOptions)
                        {
                            case DisplayFormatOptions.ShowSquareIcon:
                                eicpa0.WidthRequest = UniformHeightRequest;
                                break;
                            case DisplayFormatOptions.ShowIcon:
                            case DisplayFormatOptions.ShowMember:
                            case DisplayFormatOptions.ShowSquareIconAndMember:
                                eicpa0.WidthRequest = UniformWidthRequest;
                                break;
                            case DisplayFormatOptions.Auto:
                                throw new InvalidOperationException("S.B. Already handled.");
                            default:
                                throw new NotImplementedException($"Bad case: {displayFormatOptions}");
                        }
                        eicpa0.BackgroundColor = uniformBackgroundColor;
                        eicpa0.TextColor = uniformTextColor;
                        eicpa0.FontSize = UniformFontSize;
                        eicpa0.Padding = 0;
                        if (enumIdButton is IView view)
                        {
                            components.Add(view);
                        }
                        else throw new InvalidOperationException("Expecting IView");
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
                components.ForEach(_ => Grid.RowDefinitions.Add(new()));
                HeightRequest = components.Count * RowHeightRequest;
                for (int row = 0; row < components.Count; row++)
                {
                    var component = components[row];
                    Grid.Add(component, 0, row);
                    if (component.As<IEnumIdComponentInterop>() is { } eicpa1)
                    {
                        if (displayFormatOptions.HasFlag(DisplayFormatOptions.ShowMember))
                        {
                            eicpa1.Text = eicpa1.EnumId?.ToString() ?? "?";
                        }
                    }
                    else if (component.WithCompatibility() is { } eicpa0)
                    {
                        if (displayFormatOptions.HasFlag(DisplayFormatOptions.ShowMember))
                        {
                            eicpa0.Text = eicpa0.EnumId?.ToString() ?? "?";
                        }
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
                    UniformFontSizeNE = uniformFontSize.Value;
            }
            #endregion L o c a l F x       
        }
        public IPlatformComponentInterop Add(object o)
        {
            var w = PlatformComponentInterop.FindOrCreate(o);
            throw new NotImplementedException("ToDo");
            return w;
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

        public PortableThickness UniformSpacing
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
        PortableThickness _uniformSpacing = new(2);
        public int UniformHeightRequest => RowHeightRequest - Math.Max((int)Padding.VerticalThickness, UniformSpacing.Vertical);

        public float UniformFontSize
        {
            get => UniformFontSizeNE;
            set
            {
                if (value >= MIN_FONT_SIZE && !Equals(UniformFontSizeNE, value))
                {
                    UniformFontSizeNE = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// NEED REVIEW - The OverwriteDefault needs non-eventing
        /// access to this property but I don't recall why.
        /// </summary>
        public float UniformFontSizeNE { get; set; } = DEFAULT_FONT_SIZE;

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

        /// <summary>
        /// Components that are created using the
        /// activator  template are cached here.
        /// </summary>
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
    }
}
