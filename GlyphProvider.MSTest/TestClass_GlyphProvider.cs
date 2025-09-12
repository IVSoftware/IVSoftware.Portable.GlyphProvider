using IVSGlyphProvider.Demo.WinForms;
using IVSoftware.Portable;
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

            subtestHorizontal();
            subtestVertical();
            void subtestHorizontal()
            {
                var uut = new CenteringPanel();
                try
                {
                    uut.PropertyChanged += localOPC;
                    Assert.AreEqual(4, uut.UniformSpacing.Vertical);
                    Assert.AreEqual(29, uut.RowHeightRequest);
                    // Getter no longer notifies
                    Assert.AreEqual(0, opcCount);

                    localClear();
                    uut.UniformSpacing = UniformThickness.Empty;
                    Assert.AreEqual(1, opcCount);
                    Assert.AreEqual(0, uut.UniformSpacing.Vertical);
                    Assert.AreEqual(25, uut.RowHeightRequest);
                    actual = string.Join(" ", builder);
                    expected = @" 
ItemMargin"
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

            void subtestVertical()
            {
                var uut = new CenteringPanel
                {
                    Orientation = LayoutOrientation.Vertical,
                };
                try
                {
                    uut.PropertyChanged += localOPC;

                    Debug.Assert(DateTime.Now.Date == new DateTime(2025, 9, 12).Date, "Don't forget disabled");
                    // Assert.AreEqual(25, uut.ContentHeightRequest);
                    Assert.AreEqual(2, opcCount);
                    actual = string.Join(" ", builder);
                    actual.ToClipboardExpected();
                    expected = @" 
PreferredRowHeight ContentHeightRequest";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting property changed to match."
                    );
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
            void localClear()
            {
                opcCount = 0;
                builder.Clear();
            }
            #endregion L o c a l F x
        }
    }
}
