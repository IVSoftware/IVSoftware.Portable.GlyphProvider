using IVSoftware.Portable;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IVSGlyphProvider.Demo.WinForms
{
    public enum CenteringOrientation
    {
        /// <summary>
        /// The width of CenteringPanel is a hard limit.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Provisioning first attempts to adjust the height
        /// of CenteringPanel, which then becomes a hard limit.
        /// </summary>
        Vertical,
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
        const int MIN_ROW_HEIGHT = 25;

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
                case CenteringOrientation.Horizontal: localCalcCenteredMetricsH(); break;
                case CenteringOrientation.Vertical: localCalcCenteredMetricsV(); break;
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
                int itemY = Math.Max(Padding.Top, ItemMargin.Top);

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
                        ItemHeightRequest))
                    .ToArray();

                // After margins are subtracted, this is
                // the maximum width for the control itself.
                int maxItemWidth = widthAlloc - (ItemMargin.Horizontal / 2);
                int netItemWidth = Math.Min(maxItemWidth, ItemWidthRequest);

                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var cell = clipBounds[i];

                    int height = ItemHeightRequest;

                    int x = cell.X + (cell.Width - netItemWidth) / 2;
                    int y = cell.Y + (cell.Height - ItemHeightRequest) / 2;
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
                Height = items.Length * PreferredRowHeight;

                var clipBounds =
                    Enumerable.Range(0, items.Length)
                    .Select(_ => new Rectangle(
                        Math.Max(Padding.Left, ItemMargin.Left),
                        _ * PreferredRowHeight,
                        Width - ItemMargin.Horizontal,
                        PreferredRowHeight))
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

        public void Configure<T>() where T : struct, Enum
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

        public WatchdogTimer WDTSettle
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
        public CenteringOrientation Orientation
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
        CenteringOrientation _centeringMode = CenteringOrientation.Horizontal;

        public Padding ItemMargin
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

        public int ItemWidthRequest
        {
            get
            {
                if (_itemWidthRequest is null)
                {
                    switch (Orientation)
                    {
                        case CenteringOrientation.Horizontal: 
                            return ItemHeightRequest;
                        case CenteringOrientation.Vertical:
                            return Width - Math.Max(Padding.Horizontal, ItemMargin.Horizontal);
                        default: throw new NotImplementedException($"Bad case: {Orientation}");
                    };
                }
                else return _itemWidthRequest.Value;
            }
            set
            {
                if (!Equals(_itemWidthRequest, value))
                {
                    _itemWidthRequest = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int? _itemWidthRequest = null;

        /// <summary>
        /// Gets or sets the effective content height.
        /// In horizontal mode this is derived from the panel height minus collapsed padding.
        /// In vertical mode this is derived from PreferredRowHeight minus collapsed padding.
        /// The getter may adjust the backing field and raise PropertyChanged to maintain invariants.
        /// </summary>
        public int ItemHeightRequest
        {
            get
            {
                if (_itemHeightRequest is null)
                {
                    switch (Orientation)
                    {
                        case CenteringOrientation.Horizontal:
                            return Height - Math.Max(Padding.Vertical, ItemMargin.Vertical);
                        case CenteringOrientation.Vertical:
                            return _itemHeightRequest ?? PreferredRowHeight - Math.Max(Padding.Vertical, ItemMargin.Vertical);
                        default: throw new NotImplementedException($"Bad case: {Orientation}");
                    };
                }
                else return _itemHeightRequest.Value;
            }
            set
            {
                if (!Equals(_itemHeightRequest, value))
                {
                    _itemHeightRequest = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int? _itemHeightRequest = null;

        /// <summary>
        /// Gets or sets the preferred row height.
        /// Enforces both a minimum row height and a minimum usable content height.
        /// The getter self-normalizes silently to guarantee invariants.
        /// The setter clamps values, triggers a layout pass, and raises PropertyChanged if changed.
        /// </summary>
        public int PreferredRowHeight
        {
            get
            {
                var previewContentHeight = _preferredRowHeight - Math.Max(Padding.Vertical, ItemMargin.Vertical);
                var adj = MIN_ITEM_HEIGHT - previewContentHeight;
                if(adj != 0)
                {
                    _preferredRowHeight += adj;
                    // However, INPC is not necessary or desireable here.
                }
                return _preferredRowHeight;
            }
            set
            {
                value = Math.Max(value, MIN_ROW_HEIGHT);
                // To tell whether this is a change, we need
                // to compare it to the adjusted height getter that adds the padding.
                var previewGetContentHeight = value - Math.Max(Padding.Vertical, ItemMargin.Vertical);

                if (!Equals(PreferredRowHeight, previewGetContentHeight))
                {
                    _preferredRowHeight = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int _preferredRowHeight = MIN_ROW_HEIGHT;
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
