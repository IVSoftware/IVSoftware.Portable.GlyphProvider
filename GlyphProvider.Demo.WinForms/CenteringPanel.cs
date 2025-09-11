using IVSoftware.Portable;
using System.Reflection;

namespace IVSGlyphProvider.Demo.WinForms
{
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
            if (Controls.Count == 0) return;

            var rects = localCalcCenteredMetrics();

            SuspendLayout();
            foreach (var kvp in rects)
            {
                kvp.Key.Bounds = kvp.Value;
            }
            ResumeLayout();

            Dictionary<GlyphButton, Rectangle> localCalcCenteredMetrics()
            {
                var controls = Controls
                    .OfType<GlyphButton>()
                    .Where(c => c.Visible)
                    .ToList();
                if (controls.Count == 0) return new Dictionary<GlyphButton, Rectangle>();

                var dict = new Dictionary<GlyphButton, Rectangle>(controls.Count);

                var contentWidth = controls.Sum(c => c.Width + c.Margin.Horizontal);
                var freeSpace = Width - Padding.Horizontal - contentWidth;
                var spacing = freeSpace / controls.Count;

                int x = Padding.Left + spacing / 2;
                int y = (ClientSize.Height - controls[0].Height) / 2;

                foreach (var c in controls)
                {
                    var h = Height - Padding.Vertical - c.Margin.Vertical;
                    x += c.Margin.Left;
                    dict[c] = new Rectangle(x, y, c.Width, h);
                    x += c.Width + c.Margin.Right + spacing;
                }
                return dict;
            }
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
    }
    public abstract class ControlTemplate
    {
        public abstract IIdButton Activate(Enum id);
    }

    public class ControlTemplate<T> : ControlTemplate
        where T : Control, IIdButton
    {
        public override IIdButton Activate(Enum id) => (IIdButton)Ctor.Value.Invoke([id]);

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
