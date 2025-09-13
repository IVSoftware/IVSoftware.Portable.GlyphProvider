using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable
{
    public enum LayoutOrientation
    {
        Horizontal,

        Vertical,
    }

    [Flags]
    public enum DisplayFormatOptions
    {
        /// <summary>
        /// Icon, when visible, has square dimensions (width tracks height). 
        /// Does not itself make the icon visible.  
        /// </summary>
        IconWidthTracksHeight = 0x1,

        /// <summary>
        /// Show icon (without constraining its shape).
        /// </summary>
        ShowIcon = 0x2,

        /// <summary>
        /// Show icon, always with square dimensions.
        /// Equivalent to ShowIcon | IconWidthTracksHeight.
        /// </summary>
        ShowSquareIcon = ShowIcon | IconWidthTracksHeight,

        /// <summary>
        /// Show text content, using the enum member name or its [Description] attribute.  
        /// Does not itself make the icon visible.  
        /// Includes IconWidthTracksHeight so that if an icon *is* shown, it remains square.
        /// </summary>
        ShowMember = 0x4 | IconWidthTracksHeight, // Text only; enforces square if icon is shown

        /// <summary>
        /// Show both icon and text.  
        /// Icon is always square (ShowSquareIcon).  
        /// Text comes from the enum member name or its [Description] attribute (ShowMember).
        /// </summary>
        ShowSquareIconAndMember = ShowSquareIcon | ShowMember,

        /// <summary>
        /// Obviates all other flags.
        /// Applies ShowSquareIcon for LayoutOrientation.Horizontal.
        /// Applies ShowSquareIconAndMember for LayoutOrientation.Vertical.
        /// </summary>
        Auto = 0x8,
    }

    [Flags]
    public enum ServiceTier
    {
        Identity = 0x1,

        /// <summary>
        /// Text + Font Size
        /// </summary>
        Text     = 0x2,

        /// <summary>
        /// BackColor | BackgroundColor expressed as ARGB string in model.
        /// </summary>
        Colors   = 0x4,

        /// <summary>
        /// Supports {[*], Text, [*] Text} display formats
        /// </summary>
        Display  = 0x8,
    }
    public interface IEnumIdComponent
    {
        Enum? EnumId { get; }
    }
    public interface IEnumIdComponentPA
    {
        Enum? EnumId { get; }
        string Text { get; set; }
        string TextColor { get; set; }
        string BackgroundColor { get; set; }
        double FontSize { get; set; }

        [TypeConverter(typeof(UniformThicknessConverter))]
        UniformThickness Padding { get; set; }
        double WidthRequest { get; set; }

        DisplayFormatOptions DisplayFormatOptions { get; set; }
    }

    /// <summary>
    /// ARCHETYPE: Obfuscate an internal grid with an outer Panel-ContentView.
    /// </summary>
    public interface IConfigurableLayoutStack : INotifyPropertyChanged
    {
        /// <summary>
        /// Configures a toolbar based on enum values of type T.
        /// Uses existing parameters unless supplied, in which case
        /// supplied values have the option of becoming new defaults
        /// based on the overwrite bool.
        /// </summary>
        void Configure<T>(
            LayoutOrientation orientation = LayoutOrientation.Horizontal,
            DisplayFormatOptions displayFormatOptions = DisplayFormatOptions.ShowSquareIcon,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            float? uniformFontSize = null,
            string? uniformBackgroundColor = null,
            string? uniformTextColor = null,
            bool overwriteRequests = false) where T : struct, Enum;

        /// <summary>
        /// Gets or sets the layout orientation used by the stack.
        /// In horizontal mode, the height request is = RowHeightRequest.
        /// But in vertical mode, height request attempts to allocate
        /// N × RowHeightRequest where N is the number of items.
        /// </summary>
        LayoutOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the requested row height for child elements.
        /// Applies inward pressure on contained component heights.
        /// In vertical mode, applies outward pressure on container height request.
        /// </summary>
        int RowHeightRequest { get; set; }

        /// <summary>
        /// The common height of contained components, obtained by CSS-style
        /// collapsing of container padding with component margin.
        /// </summary>
        int UniformHeightRequest { get; }

        /// <summary>
        /// Gets or sets the display format applied when the orientation
        /// is horizontal where the typical default is ShowSquareIcon.
        /// </summary>
        DisplayFormatOptions HorizontalDisplayFormatOptions { get; set; }

        /// <summary>
        /// Gets or sets the display format applied when the orientation
        /// is vertical where the typical default is ShowSquareIconAndMember.
        /// </summary>
        DisplayFormatOptions VerticalDisplayFormatOptions { get; set; }

        /// <summary>
        /// Combination Padding-Margin quantity with ITypeConverter.
        /// Uniform in the sense that it applies consistently to all child components.
        /// </summary>
        UniformThickness UniformSpacing { get; set; }

        /// <summary>
        /// Width used when component display is not constrained to square.
        /// A special value of -1 will assign a width that is 1/3 of the container width.
        /// </summary>
        int UniformWidthRequest { get; set; }

        /// <summary>
        /// Hex color for child component background color.
        /// If the term numerically evaluates to 0, the container background color will be used.
        /// </summary>
        string? UniformBackgroundColor { get; set; }

        /// <summary>
        /// Hex color for child components text color.
        /// If the term numerically evaluates to 0, a contrasting forecolor will be heuristically determined.
        /// </summary>
        string? UniformTextColor { get; set; }

        /// <summary>
        /// When present, the container is able to autonomously create child components.
        /// </summary>
        ActivatorTemplate ActivatorTemplate { get; set; }

        /// <summary>
        /// Recycler for components by id, whether created externally or autonomously.
        /// </summary>
        IDictionary Cache { get; set; }
    }
    public abstract class ActivatorTemplate
    {
        public abstract IEnumIdComponent Activate(Enum id);
    }
}
