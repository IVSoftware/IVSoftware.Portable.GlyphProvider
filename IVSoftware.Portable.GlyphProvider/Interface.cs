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
    public interface IConfigurableLayoutStack : INotifyPropertyChanged
    {
        void Configure<T>(
            LayoutOrientation orientation = LayoutOrientation.Horizontal,
            WidthTrackingMode widthMode = WidthTrackingMode.Auto,
            int? rowHeightRequest = null,
            int? uniformWidthRequest = null,
            bool overwriteRequests = false) where T : struct, Enum;

        int RowHeightRequest { get; set; }
        UniformThickness UniformThickness { get; set; }
        int UniformWidthRequest { get; set; }
        ControlTemplate ControlTemplate { get; set; }
        LayoutOrientation Orientation { get; set; }

        Dictionary<Enum, object> Cache { get; set; }
    }
    public abstract class ControlTemplate
    {
        public abstract IGlyphButton Activate(Enum id);
    }
}
