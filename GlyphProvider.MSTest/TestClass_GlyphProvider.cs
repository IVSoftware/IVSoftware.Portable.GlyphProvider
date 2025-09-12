using IVSGlyphProvider.Demo.WinForms;
using IVSoftware.Portable;
using IVSoftware.WinOS.MSTest.Extensions;
using System.ComponentModel;

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
                    Assert.AreEqual(4, uut.ItemMargin.Vertical);
                    Assert.AreEqual(29, uut.PreferredRowHeight);
                    // Getter no longer notifies
                    Assert.AreEqual(0, opcCount);

                    localClear();
                    uut.ItemMargin = Padding.Empty;
                    Assert.AreEqual(1, opcCount);
                    Assert.AreEqual(0, uut.ItemMargin.Vertical);
                    Assert.AreEqual(25, uut.PreferredRowHeight);
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
                    uut.PreferredRowHeight = 10;
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
                    Orientation = CenteringOrientation.Vertical,
                };
                try
                {
                    uut.PropertyChanged += localOPC;
                    Assert.AreEqual(25, uut.ItemHeightRequest);
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
#if false
        [TestMethod]
        public void Test_Prototype()
        {
            string actual, expected;

            actual = "basics-icons".CreateEnumPrototype();
            expected = @" 
public enum StdBasicsIcons
{
	[Description(""add"")]
	Add,

	[Description(""delete"")]
	Delete,

	[Description(""edit"")]
	Edit,

	[Description(""ellipsis-horizontal"")]
	EllipsisHorizontal,

	[Description(""ellipsis-vertical"")]
	EllipsisVertical,

	[Description(""filter"")]
	Filter,

	[Description(""menu"")]
	Menu,

	[Description(""search"")]
	Search,

	[Description(""settings"")]
	Settings,

	[Description(""checked"")]
	Checked,

	[Description(""unchecked"")]
	Unchecked,

	[Description(""eye"")]
	Eye,

	[Description(""eye-off"")]
	EyeOff,

	[Description(""help-circled"")]
	HelpCircled,

	[Description(""help-circled-alt"")]
	HelpCircledAlt,

	[Description(""doc-empty"")]
	DocEmpty,

	[Description(""doc"")]
	Doc,

	[Description(""doc-new"")]
	DocNew,

}"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting enum gen with some minor manual nits"
            );
        }


        [TestMethod]
        public void Test_ToGlyphFromEnum()
        {
            string actual, expected;

			actual = "basics-icons".ToGlyph(IconBasics.Search, GlyphFormat.UnicodeDisplay);
            expected = @" 
U+E807";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting unicode formatted display value"
            );

			actual = "basics-icons".ToGlyph(IconBasics.Search, GlyphFormat.Xaml);
            expected = @" 
&#xE807;";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xaml formatted display value"
            );

        }

        [TestMethod]
        public void Test_ToGlyphFromFuzzyString()
        {
            string actual, expected;

            actual = "basics-icons".ToGlyph("search", GlyphFormat.UnicodeDisplay);
            expected = @" 
U+E807";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting unicode formatted display value"
            );

            actual = "basics-icons".ToGlyph("search", GlyphFormat.Xaml);
            expected = @" 
&#xE807;";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xaml formatted display value"
            );
        }

        [TestMethod]
        public void Test_Reports()
        {
            string actual, expected;

            actual = string.Join(Environment.NewLine, IVSoftware.Portable.GlyphProvider.ListDomainFontResources());
            expected = @" 
Assembly: IVSoftware.Portable.GlyphProvider
File: D:\Github\IVSoftware\TMP\IVSoftware.Portable.GlyphProvider\GlyphProvider.MSTest\bin\Debug\net8.0-windows\IVSoftware.Portable.GlyphProvider.dll
Resource: IVSoftware.Portable.Resources.Fonts.Basics.font.basics-icons.ttf";

            actual = string.Join(Environment.NewLine, IVSoftware.Portable.GlyphProvider.ListDomainFontConfigs());
            expected = @" 
Assembly: IVSoftware.Portable.GlyphProvider
File: D:\Github\IVSoftware\TMP\IVSoftware.Portable.GlyphProvider\GlyphProvider.MSTest\bin\Debug\net8.0-windows\IVSoftware.Portable.GlyphProvider.dll
Resource: IVSoftware.Portable.Resources.Fonts.Basics.config.json
Family: basics-icons"
            ;
        }
#endif
    }
}
