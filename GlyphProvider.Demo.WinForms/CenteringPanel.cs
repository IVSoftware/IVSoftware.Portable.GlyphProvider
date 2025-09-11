using IVSoftware.Portable;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IVSGlyphProvider.Demo.WinForms
{
    public enum CenteringMode
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

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            var content = Controls
                .Cast<Control>()
                .Where(c => c.Visible)
                .ToArray();
            if (content.Length == 0 || !IsHandleCreated || Disposing ) return;
            var bounds = new Dictionary<Control, Rectangle>(content.Length);
            switch (CenteringMode)
            {
                case CenteringMode.Horizontal: localCalcCenteredMetricsH(); break;
                case CenteringMode.Vertical: localCalcCenteredMetricsV(); break;
                default: throw new NotImplementedException($"Bad case: {CenteringMode}");
            };

            SuspendLayout();
            foreach (var kvp in bounds)
            {
                kvp.Key.Bounds = kvp.Value;
            }
            ResumeLayout();

            #region L o c a l F x
            void localCalcCenteredMetricsH()
            {
                var contentWidth = 
                    (int)Math.Ceiling(
                        (double)content.Sum(_ => _.Width) + ContentMargin.Horizontal * content.Length);

                // CSS style collapsed height (intuitive)
                int contentY = Math.Max(Padding.Top, ContentMargin.Top);
                int widthAlloc = (int)Math.Floor((double)(Width - Padding.Horizontal) / content.Length);
                // What happens here is *not* a collapse, to wit:
                // - If this.Padding is 50 then the entire array is shifted left.
                // - Horizontal padding in content often doesn't play because
                //   of the "extra room" left by iconized, centered buttons.
                // - However, if compressed enough, overlapping content padding will be collapsed
                //   between the overlapping controls, effecively breating "min spacing" between them.
                var centeringCells = 
                    Enumerable.Range(0, content.Length)
                    .Select(_ => new Rectangle(_ * widthAlloc, contentY, widthAlloc, ContentHeightRequest))
                    .ToArray();
                { }
                for (int i = 0; i < content.Length; i++)
                {
                    var control = content[i];
                    var cell = centeringCells[i];
                    var cellFreeSpace = cell.Width - control.Width;
                    control.Bounds = new Rectangle(cell.X + cellFreeSpace / 2, cell.Y, control.Width, cell.Height);
                }
            }

            void localCalcCenteredMetricsV()
            {
                throw new NotImplementedException("ToDo");
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
        public CenteringMode CenteringMode
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
        CenteringMode _centeringMode = CenteringMode.Horizontal;

        public Padding ContentMargin
        {
            get => _contentMargin;
            set
            {
                if (!Equals(_contentMargin, value))
                {
                    _contentMargin = value;
                    PerformLayout();
                    OnPropertyChanged();
                    _ = PreferredRowHeight;
                }
            }
        }
        Padding _contentMargin = new(3);

        public int ContentWidthRequest
        {
            get
            {
                if (_contentWidthRequest is null)
                {
                    switch (CenteringMode)
                    {
                        case CenteringMode.Horizontal: 
                            return ContentHeightRequest;
                        case CenteringMode.Vertical:
                            return Width - Math.Max(Padding.Horizontal, ContentMargin.Horizontal);
                        default: throw new NotImplementedException($"Bad case: {CenteringMode}");
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
        public int ContentHeightRequest
        {
            get
            {
                if (_contentHeightRequest is null)
                {
                    switch (CenteringMode)
                    {
                        case CenteringMode.Horizontal:
                            return Height - Math.Max(Padding.Vertical, ContentMargin.Vertical);
                        case CenteringMode.Vertical:
                            return _contentHeightRequest ?? PreferredRowHeight - Math.Max(Padding.Vertical, ContentMargin.Vertical);
                        default: throw new NotImplementedException($"Bad case: {CenteringMode}");
                    };
                }
                else return _contentHeightRequest.Value;
            }
            set
            {
                if (!Equals(_contentHeightRequest, value))
                {
                    _contentHeightRequest = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int? _contentHeightRequest = null;

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
                var previewContentHeight = _preferredRowHeight - Math.Max(Padding.Vertical, ContentMargin.Vertical);
                var adj = MIN_CONTENT_HEIGHT - previewContentHeight;
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
                var previewContentHeight = value - Math.Max(Padding.Vertical, ContentMargin.Vertical);
                var adj = MIN_CONTENT_HEIGHT - previewContentHeight;
                value += adj;
                if (!Equals(PreferredRowHeight, value))
                {
                    _preferredRowHeight = value;
                    PerformLayout();
                    OnPropertyChanged();
                }
            }
        }
        int _preferredRowHeight = MIN_ROW_HEIGHT;

        const int MIN_ROW_HEIGHT = 25;
        const int MIN_CONTENT_HEIGHT = 25;
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
