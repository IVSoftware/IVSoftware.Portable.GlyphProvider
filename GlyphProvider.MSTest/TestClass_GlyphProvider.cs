using IVSoftware.Portable;
using IVSoftware.WinOS.MSTest.Extensions;

namespace GlyphProvider.MSTest
{
    [TestClass]
    public sealed class TestClass_GlyphProvider
    {
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

			actual = "basics-icons".ToGlyph(StdBasicsIcons.Search, GlyphFormat.UnicodeDisplay);
            expected = @" 
U+E807";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting unicode formatted display value"
            );

			actual = "basics-icons".ToGlyph(StdBasicsIcons.Search, GlyphFormat.Xaml);
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

            actual = string.Join(Environment.NewLine, IVSoftware.Portable.GlyphProvider.ListFonts());
            expected = @" 
Assembly: IVSoftware.Portable.GlyphProvider
File: D:\Github\IVSoftware\TMP\IVSoftware.Portable.GlyphProvider\GlyphProvider.MSTest\bin\Debug\net8.0-windows\IVSoftware.Portable.GlyphProvider.dll
Resource: IVSoftware.Portable.Resources.Fonts.Basics.font.basics-icons.ttf";

            actual = string.Join(Environment.NewLine, IVSoftware.Portable.GlyphProvider.ListConfigs());
            expected = @" 
Assembly: IVSoftware.Portable.GlyphProvider
File: D:\Github\IVSoftware\TMP\IVSoftware.Portable.GlyphProvider\GlyphProvider.MSTest\bin\Debug\net8.0-windows\IVSoftware.Portable.GlyphProvider.dll
Resource: IVSoftware.Portable.Resources.Fonts.Basics.config.json
Family: basics-icons"
            ;
        }
    }
}
