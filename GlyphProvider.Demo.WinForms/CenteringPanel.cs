using IVSoftware.Portable;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IVSGlyphProvider.Demo.WinForms
{
    public enum WidthTrackingMode
    {
        Normal,

        /// <summary>
        /// Make square buttons suitable for glyph icons.
        /// </summary>
        WidthTracksHeight,


        Auto,
    }
    public class CenteringPanel : Panel
    {
        public CenteringPanel()
        {
            BackColor = Color.LightBlue;
            ControlAdded += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    e.Control.Anchor = 0;
                    e.Control.Dock = 0;
                    if(Orientation == Orientation.Vertical &&
                     control.Height > RowHeightRequest)
                    {
                        RowHeightRequest = control.Height;
                    }
                    control.VisibleChanged += localKickWDT;
                }
            };
            ControlRemoved += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    control.VisibleChanged -= localKickWDT;
                }
            };
            SizeChanged += (sender, e) =>
            {
                if (IsHandleCreated) WDTSettle.StartOrRestart();
            };
            void localKickWDT(object? sender, EventArgs e)
            {
                if (IsHandleCreated) WDTSettle.StartOrRestart();
            }
        }

        const int MIN_ITEM_WIDTH = 20;
        const int MIN_ITEM_HEIGHT = 25;
        const int DEFAULT_ROW_HEIGHT = 34;
        const int MIN_ROW_HEIGHT = 20;

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            var items = Controls
                .Cast<Control>()
                .Where(c => c.Visible)
                .ToArray();
            if (items.Length == 0 || !IsHandleCreated || Disposing ) return;
            var bounds = new Dictionary<Control, Rectangle>(items.Length);
            switch (Orientation)
            {
                case Orientation.Horizontal: localCalcCenteredMetricsH(); break;
                case Orientation.Vertical: localCalcCenteredMetricsV(); break;
                default: throw new NotImplementedException($"Bad case: {Orientation}");
            };

            SuspendLayout();
            foreach (var kvp in bounds)
            {
                kvp.Key.Bounds = kvp.Value;
            }

#if DEBUG
            if (bounds.Count > 1)
            {
                // Diagnostic: check actual gap between neighbors.
                // Even if math says 0, WinForms integer rounding (widthAlloc, division)
                // can leave a stray pixel column when painting. Use breakpoint to inspect.
                var space = items[1].Left - items[0].Right;
                { } // Breakpoint: inspect 'space' and control bounds
            }
#endif
            ResumeLayout();

            #region L o c a l F x
            void localCalcCenteredMetricsH()
            {
                // CSS style collapsed height (intuitive)
                int itemY = Math.Max(Padding.Top, ContentMargin.Top);

                // Back out the padding of this container itself.
                int widthAlloc = (int)Math.Floor((double)(Width - Padding.Horizontal) / items.Length);

                // What happens here is *not* a collapse, to wit:
                // - If this.Padding is 50 then the entire array is shifted left.
                // - Horizontal padding in content often doesn't play because
                //   of the "extra room" left by iconized, centered buttons.
                // - However, if compressed enough, overlapping content padding will be collapsed
                //   between the overlapping controls, effecively breating "min spacing" between them.
                var clipBounds =
                    Enumerable.Range(0, items.Length)
                    .Select(_ => new Rectangle(
                        _ * widthAlloc,
                        itemY, 
                        widthAlloc,
                        ContentHeightRequest))
                    .ToArray();

                // After margins are subtracted, this is
                // the maximum width for the control itself.
                int maxItemWidth = widthAlloc - (ContentMargin.Horizontal / 2);
                int netItemWidth = Math.Min(maxItemWidth, ContentWidthRequest);

                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var cell = clipBounds[i];

                    int height = ContentHeightRequest;

                    int x = cell.X + (cell.Width - netItemWidth) / 2;
                    int y = cell.Y + (cell.Height - ContentHeightRequest) / 2;
                    bounds[item] = new Rectangle(x, y, netItemWidth, height);
                }

                if(netItemWidth < MIN_ITEM_WIDTH)
                {
                    BeginInvoke(() => // Because we're inside of SuspendLayout
                    throw new InvalidOperationException("Minimum width violation. Either add fewer items or increase the container minimum width."));
                }
            }

            void localCalcCenteredMetricsV()
            {
                Height = items.Length * RowHeightRequest;

                var clipBounds =
                    Enumerable.Range(0, items.Length)
                    .Select(_ => new Rectangle(
                        Math.Max(Padding.Left, ContentMargin.Left),
                        _ * RowHeightRequest,
                        Width - ContentMargin.Horizontal,
                        RowHeightRequest))
                    .ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var cell = clipBounds[i];

                    //int height = ItemHeightRequest;

                    int x = cell.X ;
                    int y = cell.Y;
                    bounds[item] = new Rectangle(x, y, cell.Width, cell.Height);
                }
                return;
                BeginInvoke(() => // Because we're inside of SuspendLayout
                throw new NotImplementedException("ToDo"));
            }
            #endregion L o c a l F x
        }

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
            Orientation orientation = Orientation.Horizontal,
             WidthTrackingMode widthMode = WidthTrackingMode.Auto,
             int? rowHeightRequest = null,
             int? uniformWidthRequest = null,
             bool overwriteRequests = false
        ) where T : struct, Enum
        {
            if (ControlTemplate is null)
                throw new InvalidOperationException("ControlTemplate must be set before calling Configure<T>().");

            SuspendLayout();
            Controls.Clear();
            foreach (var id in Enum.GetValues<T>())
            {
                var idButton = ControlTemplate.Activate(id);

                if (idButton is Control ctl)
                {
                    ctl.BackColor = ColorTranslator.FromHtml("#444444");
                    ctl.ForeColor = Color.WhiteSmoke;
                    ctl.Height = Height - Padding.Vertical - ctl.Margin.Vertical;
                    ctl.Visible = true;

                    Controls.Add(ctl);
                }
            }
            ResumeLayout(performLayout: true);
        }

        private WatchdogTimer WDTSettle
        {
            get
            {
                if (_wdtSettle is null)
                {
                    _wdtSettle = new WatchdogTimer { Interval = TimeSpan.FromSeconds(0.1) };
                    _wdtSettle.RanToCompletion += (sender, e) => PerformLayout(); ;
                }
                return _wdtSettle;
            }
        }
        WatchdogTimer? _wdtSettle = null;

        public ControlTemplate ControlTemplate { get; set; } = new ControlTemplate<GlyphButton>();

        #region L A Y O U T    T R I G G E R S
        public Orientation Orientation
        {
            get => _centeringMode;
            set
            {
                if (!Equals(_centeringMode, value))
                {
                    _centeringMode = value;
                    PerformLayout();   // Request new layout, not just paint
                    OnPropertyChanged();
                }
            }
        }
        Orientation _centeringMode = Orientation.Horizontal;

        public Padding ContentMargin
        {
            get => _itemMargin;
            set
            {
                if (!Equals(_itemMargin, value))
                {
                    _itemMargin = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        Padding _itemMargin = new(2);

        /// <summary>
        /// Sets a uniform width for content.
        /// </summary>
        public int ContentWidthRequest
        {
            get
            {
                if (_contentWidthRequest is null)
                {
                    switch (Orientation)
                    {
                        case Orientation.Horizontal: 
                            return ContentHeightRequest;
                        case Orientation.Vertical:
                            return Width - Math.Max(Padding.Horizontal, ContentMargin.Horizontal);
                        default: throw new NotImplementedException($"Bad case: {Orientation}");
                    };
                }
                else return _contentWidthRequest.Value;
            }
            set
            {
                if (!Equals(_contentWidthRequest, value))
                {
                    _contentWidthRequest = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int? _contentWidthRequest = null;

        /// <summary>
        /// Gets or sets the effective content height.
        /// In horizontal mode this is derived from the panel height minus collapsed padding.
        /// In vertical mode this is derived from PreferredRowHeight minus collapsed padding.
        /// The getter may adjust the backing field and raise PropertyChanged to maintain invariants.
        /// </summary>
        public int ContentHeightRequest => RowHeightRequest - Math.Max(Padding.Vertical, ContentMargin.Vertical);

        /// <summary>
        /// Gets or sets the preferred row height.
        /// Enforces both a minimum row height and a minimum usable content height.
        /// The getter self-normalizes silently to guarantee invariants.
        /// The setter clamps values, triggers a layout pass, and raises PropertyChanged if changed.
        /// </summary>
        public int RowHeightRequest
        {
            get => _rowHeightRequest;
            set
            {
                if (!Equals(_rowHeightRequest, value))
                {
                    _rowHeightRequest = value;
                    OnPropertyChanged();
                }
            }
        }
        int _rowHeightRequest = MIN_ROW_HEIGHT;
        #endregion L A Y O U T    T R I G G E R S

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;

    }
    public abstract class ControlTemplate
    {
        public abstract IGlyphButton Activate(Enum id);
    }

    public class ControlTemplate<T> : ControlTemplate
        where T : Control, IGlyphButton
    {
        public override IGlyphButton Activate(Enum id) => (IGlyphButton)Ctor.Value.Invoke([id]);

        private static readonly Lazy<ConstructorInfo> Ctor =
            new Lazy<ConstructorInfo>(() =>
            {
                var ctor = typeof(T).GetConstructor([typeof(Enum)]);
                if (ctor is null)
                    throw new InvalidOperationException($"{typeof(T).Name} must expose a public constructor (Enum).");
                return ctor;
            });
    }
}
