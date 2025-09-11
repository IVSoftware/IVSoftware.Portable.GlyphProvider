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
                var controls = Controls.OfType<GlyphButton>().ToList();

                var contentWidth = controls.Sum(_ => _.Width + _.Margin.Horizontal);
                var freeSpace =
                    Width - Padding.Horizontal - contentWidth;
                var spacing = freeSpace / (controls.Count);

                int x = Padding.Left + spacing / 2;
                int y = (ClientSize.Height - controls[0].Height) / 2;

                foreach (var control in controls)
                {
                    control.Height = Height - Padding.Vertical - control.Margin.Vertical;
                    x += control.Margin.Left;
                    control.Location = new Point(x, y);
                    x += control.Width + control.Margin.Right + spacing;
                }
            }
        }

        public void Configure<T>() where T : struct, Enum
        {
            Controls.Clear();
            foreach (var value in Enum.GetValues<T>())
            {
                var btn = new GlyphButton
                {
                    Visible = false,
                    BackColor = ColorTranslator.FromHtml("#444444"),
                    ForeColor = Color.WhiteSmoke,
                    Id = value,
                };
                btn.Height = this.Height - this.Padding.Vertical - btn.Margin.Vertical;
                Controls.Add(btn);
            }
            foreach (Control control in Controls)
            {
                control.Visible = true;
            }
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
