using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    [TypeConverter(typeof(UniformThicknessConverter))]
    public readonly struct UniformThickness
    {
        public int Left { get; }
        public int Top { get; }
        public int Right { get; }
        public int Bottom { get; }
        public int Horizontal => Left + Right;
        public int Vertical => Top + Bottom;

        public static UniformThickness Empty { get; } = new(0);

        public UniformThickness(int uniform) : this(uniform, uniform, uniform, uniform) { }
        public UniformThickness(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }
        public UniformThickness(int left, int top, int right, int bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }

        public override string ToString() => $"{Left},{Top},{Right},{Bottom}";
    }

    public class UniformThicknessConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                var parts = s.Split(',');
                switch (parts.Length)
                {
                    case 1:
                        if (int.TryParse(parts[0], out int uniform))
                            return new UniformThickness(uniform);
                        break;
                    case 2:
                        return new UniformThickness(
                            int.Parse(parts[0]), int.Parse(parts[1]));
                    case 4:
                        return new UniformThickness(
                            int.Parse(parts[0]),
                            int.Parse(parts[1]),
                            int.Parse(parts[2]),
                            int.Parse(parts[3]));
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is UniformThickness m)
            {
                return $"{m.Left},{m.Top},{m.Right},{m.Bottom}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}
