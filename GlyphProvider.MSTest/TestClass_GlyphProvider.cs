using IVSGlyphProvider.Demo.WinForms;
using IVSoftware.Portable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.ComponentModel;
using System.Diagnostics;

namespace IVSGlyphProvider.MSTest
{
    [TestClass]
    public sealed class TestClass_GlyphProvider
    {
        [TestMethod]
        public void Test_CenteringContainerMetrics()
        {
            string actual, expected;

            int opcCount = 0;
            var builder = new List<string?>();
            CenteringPanel uut;

            subtestHorizontal();
            subtestVertical();
            void subtestHorizontal()
            {
                uut = new CenteringPanel();
                try
                {
                    uut.PropertyChanged += localOPC;

                    // The default is (2)
                    Assert.AreEqual(4, uut.UniformSpacing.Vertical);

                    // This is (currently) DEFAULT_ROW_HEIGHT, a value
                    // that anticipates the default UniformSpacing
                    // but does not attempt to calculate it.
                    Assert.AreEqual(34, uut.RowHeightRequest);

                    // Get only with RT adjustment for margins: 34 - 4
                    Assert.AreEqual(30, uut.UniformHeightRequest);

                    // We jettisoned the idea that getters could notify.
                    Assert.AreEqual(0, opcCount);

                    localClear(25);
                    Assert.AreEqual(25, uut.RowHeightRequest);
                    Assert.AreEqual(21, uut.UniformHeightRequest);


                    uut.UniformSpacing = UniformThickness.Empty;
                    Assert.AreEqual(1, opcCount);
                    Assert.AreEqual(0, uut.UniformSpacing.Vertical);
                    Assert.AreEqual(25, uut.RowHeightRequest);
                    actual = string.Join(" ", builder);

                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
UniformSpacing"
                    ;

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting property changed to match."
                    );

                    localClear();
                    uut.RowHeightRequest = 10;
                    Assert.AreEqual(0, opcCount, "Expecting no change.");
                }
                finally
                {
                    uut.PropertyChanged -= localOPC;
                }
            }

            // So far, tests nothing.
            void subtestVertical()
            {
                localClear();
                uut = new CenteringPanel
                {
                    Orientation = LayoutOrientation.Vertical,
                };
                try
                {
                    uut.PropertyChanged += localOPC;

                    Assert.AreEqual(30, uut.UniformHeightRequest);
                    Assert.AreEqual(0, opcCount);
                }
                finally
                {
                    uut.PropertyChanged -= localOPC;
                }
            }

            #region L o c a l F x	
            void localOPC(object? sender, PropertyChangedEventArgs e)
            {
                builder.Add(e.PropertyName);
                opcCount++;
            }
            void localClear(int? rowHeightRequest = null)
            {
                uut.RowHeightRequest = rowHeightRequest ?? -1;
                opcCount = 0;
                builder.Clear();
            }
            #endregion L o c a l F x
        }

        [TestMethod]
        public void Test_Mapper()
        {
            var cp = new CenteringPanel();
            const string EXPECTED_TEXT = "Click me";
            var uut = new LookNoInterface
            {
                Text = EXPECTED_TEXT,
                BackColor = Color.LightBlue,
                ForeColor = ColorTranslator.FromHtml("222222"),
                Width = 201,
                Height = 31,
            };
            IEnumIdComponentPA pac = cp.Add(uut);
            { } // <- Break here to observe


            Assert.AreEqual(EXPECTED_TEXT, pac.Text);
            { }
        }
        [DebuggerDisplay("{DebuggerDisplay}")]
        class LookNoInterface : System.Windows.Forms.Button
        {
            public Enum EnumId { get; } = LookNoInterfaceId.Create.Auto();
            private string DebuggerDisplay => $"{EnumId.ToFullKey()} {Text}";
        }
        enum LookNoInterfaceId
        {
            Create,
        }
    }
    internal static class UnitTestExtensions
    {
        public static Enum Auto<T>(this T @this) where T : Enum
        {
            T auto = (T)Enum.ToObject(typeof(T), _autoId++);
#if DEBUG
            var cMe = auto.ToFullKey();
#endif
            return (T)auto;
        }
        public static uint _autoId = 1000;
    }
}
