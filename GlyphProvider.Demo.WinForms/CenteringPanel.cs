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
                    control.Layout += localTrackControl;
                }
            };
            ControlRemoved += (sender, e) =>
            {
                if (e.Control is { } control)
                {
                    control.Layout -= localTrackControl;
                }
            };
            SizeChanged += (sender, e) =>
            {
                if (IsHandleCreated) WDTSettle.StartOrRestart();
            };
            void localTrackControl(object? sender, EventArgs e) => WDTSettle.StartOrRestart();
        }

        public void Configure<T>() where T : struct, Enum
        {
            Visible = false;
            Controls.Clear();
            foreach (var value in Enum.GetValues<T>())
            {
                Controls.Add(new GlyphButton
                {
                    Visible = false,
                    BackColor = ColorTranslator.FromHtml("#444444"),
                    ForeColor = Color.WhiteSmoke,
                    Id = value,
                    Height = Height - Padding.Vertical - Margin.Vertical,
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
                    _wdtSettle.RanToCompletion += async (sender, e) =>
                    {
                        Debug.WriteLine(++_count);
                        SuspendLayout();
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
                        controls.ForEach(_ => _.IsInitialized = true);
                        ResumeLayout();
                        await Task.Delay(100);
                        Visible = true;
                    };
                }
                return _wdtSettle;
            }
        }
        WatchdogTimer? _wdtSettle = null;

        int _count = 0;
    }
}
