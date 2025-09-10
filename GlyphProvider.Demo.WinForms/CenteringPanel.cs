using IVSoftware.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSGlyphProvider.Demo.WinForms
{
    public class CenteringPanel : Panel
    {
        public CenteringPanel()
        {
            ControlAdded += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    control.VisibleChanged += localTrackControl;
                    control.SizeChanged += localTrackControl;
                }
            };
            ControlRemoved += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    control.VisibleChanged -= localTrackControl;
                    control.SizeChanged -= localTrackControl;
                }
            };
            void localTrackControl(object? sender, EventArgs e) => WDTSettle.StartOrRestart();
        }

        public void Configure<T>() where T : struct, Enum
        {
            Controls.Clear();
            foreach (var value in Enum.GetValues<T>())
            {
                Controls.Add(new GlyphButton
                {
                    BackColor = ColorTranslator.FromHtml("#444444"),
                    ForeColor = Color.WhiteSmoke,
                    Id = value,
                });
            }
        }
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            WDTSettle.StartOrRestart();
        }
        public WatchdogTimer WDTSettle
        {
            get
            {
                if (_wdtSettle is null)
                {
                    _wdtSettle = new WatchdogTimer { Interval = TimeSpan.FromSeconds(0.1) };
                    _wdtSettle.RanToCompletion += (sender, e) =>
                    {
                        var controls = Controls.Cast<Control>().ToArray();

                        var contentWidth = controls.Sum(_ => _.Width + _.Margin.Horizontal);
                        var freeSpace =
                            Width - Padding.Horizontal - contentWidth;
                        var spacing = freeSpace / (controls.Length);

                        int x = Padding.Left + spacing / 2;
                        int y = (ClientSize.Height - controls[0].Height) / 2;

                        foreach (var control in controls)
                        {
                            control.Height = Height - Padding.Vertical - control.Margin.Vertical;
                            x += control.Margin.Left;
                            control.Location = new Point(x, y);
                            x += control.Width + control.Margin.Right + spacing;
                        }
                    };
                }
                return _wdtSettle;
            }
        }
        WatchdogTimer? _wdtSettle = null;

    }
}
