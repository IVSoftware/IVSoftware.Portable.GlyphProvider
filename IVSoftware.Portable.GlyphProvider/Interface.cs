using System.Collections;
using System.ComponentModel;

namespace IVSoftware.Portable
{
    public enum LayoutOrientation
    {
        Horizontal,

        Vertical,
    }
    public enum WidthTrackingMode
    {
        Normal,

        /// <summary>
        /// Make square buttons suitable for glyph icons.
        /// </summary>
        WidthTracksHeight,


        Auto,
    }
    public interface IGlyphButton { Enum? Id { get; } }

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
            WidthTrackingMode widthTrackingMode = WidthTrackingMode.Auto,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            bool overwriteRequests = false) where T : struct, Enum;
        LayoutOrientation Orientation { get; set; }
        WidthTrackingMode WidthTrackingMode { get; set; }

        int RowHeightRequest { get; set; }
        UniformThickness UniformThickness { get; set; }
        int UniformWidthRequest { get; set; }
        ActivatorTemplate ActivatorTemplate { get; set; }
        IDictionary Cache { get; set; }
    }
    public abstract class ActivatorTemplate
    {
        public abstract IGlyphButton Activate(Enum id);
    }
}
