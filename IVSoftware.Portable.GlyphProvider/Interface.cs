using System.Collections;
using System.ComponentModel;

namespace IVSoftware.Portable
{
    public enum LayoutOrientation
    {
        Horizontal,

        Vertical,
    }

    [Flags]
    public enum LayoutOptions
    {
        /// <summary>
        /// Icon, when visible, has square dimensions (width tracks height). 
        /// Does not itself make the icon visible.  
        /// </summary>
        IconWidthTracksHeight = 1,

        /// <summary>
        /// Show icon (without constraining its shape).
        /// </summary>
        ShowIcon = 2,

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
        ShowMember = 5,
    }


    public interface IEnumIdComponent { Enum? EnumId { get; } }

    /// <summary>
    /// ARCHETYPE: Obfuscate an internal grid with an outer Panel-ContentView.
    /// </summary>
    public interface IConfigurableLayoutStack : INotifyPropertyChanged
    {
        /// <summary>
        /// Configures a toolbar based on enum values of type T.
        /// - Defaults to a horizontal layout where buttons are square (width tracks height).
        /// - If no row height is specified, the first added element sets the height
        ///   after collapsing container padding and element margin.
        /// - If no uniform width is specified, the behavior depends on orientation
        ///   when widthMode is Auto: for horizontal, width tracks height;
        ///   for vertical, uniform height is set by the first element.
        /// - Once established, row height and element width remain fixed unless
        ///   overwriteRequests is true.
        /// </summary>
        void Configure<T>(
            LayoutOrientation orientation = LayoutOrientation.Horizontal,
            LayoutOptions widthTrackingMode = LayoutOptions.WithTracksHeightForHorizontal,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            float? uniformFontSize = null,
            bool overwriteRequests = false) where T : struct, Enum;
        LayoutOrientation Orientation { get; set; }
        LayoutOptions WidthTrackingMode { get; set; }
        int RowHeightRequest { get; set; }
        UniformThickness UniformSpacing { get; set; }
        int UniformWidthRequest { get; set; }
        ActivatorTemplate ActivatorTemplate { get; set; }
        IDictionary Cache { get; set; }
    }
    public abstract class ActivatorTemplate
    {
        public abstract IEnumIdComponent Activate(Enum id);
    }
}
