using IVSoftware.Portable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            if (Controls.Count != 0)
            {
                var rects = localCalcCenteredMetrics();
                SuspendLayout();
                for (int i = 0; i < Controls.Count; i++)
                {
                    Controls[i].Bounds = rects[i];
                }
                ResumeLayout();
            }
            Rectangle[] localCalcCenteredMetrics()
            {
                var controls = Controls.OfType<GlyphButton>().Where(c => c.Visible).ToList();
                if (controls.Count == 0) return Array.Empty<Rectangle>();

                var rects = new Rectangle[controls.Count];

                var contentWidth = controls.Sum(c => c.Width + c.Margin.Horizontal);
                var freeSpace = Width - Padding.Horizontal - contentWidth;
                var spacing = freeSpace / controls.Count;

                int x = Padding.Left + spacing / 2;
                int y = (ClientSize.Height - controls[0].Height) / 2;

                for (int i = 0; i < controls.Count; i++)
                {
                    var c = controls[i];
                    var h = Height - Padding.Vertical - c.Margin.Vertical;
                    x += c.Margin.Left;
                    rects[i] = new Rectangle(x, y, c.Width, h);
                    x += c.Width + c.Margin.Right + spacing;
                }
                return rects;
            }
        }

        public void Configure<T>() where T : struct, Enum
        {
            SuspendLayout();

            Controls.Clear();
            foreach (var value in Enum.GetValues<T>())
            {
                var btn = new GlyphButton
                {
                    BackColor = ColorTranslator.FromHtml("#444444"),
                    ForeColor = Color.WhiteSmoke,
                    Id = value,
                };
                btn.Height = Height - Padding.Vertical - btn.Margin.Vertical;
                Controls.Add(btn);
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
    }
}
