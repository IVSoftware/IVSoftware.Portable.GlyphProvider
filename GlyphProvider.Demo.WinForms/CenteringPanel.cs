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
                    control.VisibleChanged += localTrackControl;
                }
            };
            ControlRemoved += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    control.VisibleChanged -= localTrackControl;
                }
            };
            SizeChanged += (sender, e) =>
            {
                if (IsHandleCreated) WDTSettle.StartOrRestart();
            };
            void localTrackControl(object? sender, EventArgs e)
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
                int contentHeight = Height - Math.Max(Padding.Vertical, ContentMargin.Vertical);
                int contentY = Math.Max(Padding.Top, ContentMargin.Top);

                int widthAlloc = (int)Math.Floor((double)(Width - Padding.Horizontal) / content.Length);
                // What happens here is *not* a collapse, to wit:
                // - If this.Padding is 50 then the entire array is shifted left.
                // - Horizontal padding in content often doesn't play because
                //   of the "extra room" left by iconized, centered buttons.
                // - However, if compressed enough, overlapping content padding will be collapsed
                //   between the overlapping controls, effecively breating "min spacing" between them.
                var freeSpace = Width - Padding.Horizontal - (ContentMargin.Horizontal * content.Length);
                var centeringCells = Enumerable.Repeat(new Rectangle(0, contentY, widthAlloc, contentHeight), content.Length).ToArray();
                { }

                var spacing = freeSpace / content.Length;
                int x = Padding.Left + spacing / 2;

                foreach (var control in content)
                {
                    // CenteringContainer is the authority on height
                    control.Height = contentHeight;
                    x += ContentMargin.Left;
                    bounds[control] = new Rectangle(x, contentY, control.Width, control.Height);
                    x += control.Width + ContentMargin.Right + spacing;
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
                }
            }
        }
        Padding _contentMargin = new(3);

        public int? ContentWidthRequest
        {
            get
            {
                if (_contentWidthRequest is null)
                {
                    switch (CenteringMode)
                    {
                        case CenteringMode.Horizontal: return ContentHeightRequest;
                        case CenteringMode.Vertical: break;
                        default: throw new NotImplementedException($"Bad case: {CenteringMode}");
                    };
                    throw new NotImplementedException("ToDo");
                }
                else return _contentWidthRequest;
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

        public int? ContentHeightRequest
        {
            get
            {
                if (_contentHeightRequest is null)
                {
                    switch (CenteringMode)
                    {
                        case CenteringMode.Horizontal: return Height - (int)Math.Max((double)Padding.Vertical, ContentMargin.Vertical);
                        case CenteringMode.Vertical: break;
                        default: throw new NotImplementedException($"Bad case: {CenteringMode}");
                    };
                    throw new NotImplementedException("ToDo");
                }
                else return _contentHeightRequest;
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
