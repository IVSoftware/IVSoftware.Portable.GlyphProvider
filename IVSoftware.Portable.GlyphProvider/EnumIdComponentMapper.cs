using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable
{
    public class EnumIdComponentMapper : IEnumIdComponentPA
    {
        private static readonly Dictionary<Type, Func<object, IEnumIdComponentPA>> _typeCache = new();

        public static IEnumIdComponentPA Map(object @this)
        {
            var platformType = @this.GetType();
#if DEBUG && true
            CodeGen(platformType);
#endif
            PropertyInfo? piNative;
            foreach (var piMap in typeof(EnumIdComponentMapper).GetProperties())
            {
                piNative = platformType.GetProperty(piMap.Name);
                if(piNative is null)
                {
                    switch (piMap.Name)
                    {
                        case nameof(EnumId):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(Text):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(TextColor):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(BackgroundColor):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(FontSize):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(Padding):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(WidthRequest):
                            throw new NotImplementedException("ToDo");
                            break;
                        case nameof(DisplayFormatOptions):
                            throw new NotImplementedException("ToDo");
                            break;
                    }
                }
            }
            throw new NotImplementedException("ToDo");
        }

        private static void CodeGen(Type platformType)
        {
            var builder = new List<string>();
            builder.Add($"switch (piMap.Name)");
            builder.Add($"{{");

            foreach (var piMap in typeof(EnumIdComponentMapper).GetProperties())
            {
                builder.Add($"\tcase nameof({piMap.Name}):");
                builder.Add($"\t\tthrow new NotImplementedException(\"ToDo\");");
                builder.Add($"\t\tbreak;");
            }
            builder.Add($"}}");

            var joined = string.Join(Environment.NewLine, builder);
            { } // <- break here to copy code block
        }

        private readonly EnumIdComponentMapper _impl;
        public Enum? EnumId => _impl.EnumId;


        T Get<T>([CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException("ToDo");
        }
        void Set<T>(T value, [CallerMemberName] string? caller = null)
        {
        }
        public string Text
        {
            get => _impl.Get<string>();
            set => _impl.Set(value);
        }
        public string TextColor
        {
            get => _impl.Get<string>();
            set => _impl.Set(value);
        }
        public string BackgroundColor
        {
            get => _impl.Get<string>();
            set => _impl.Set(value);
        }
        public double FontSize
        {
            get => _impl.Get<double>();
            set => _impl.Set(value);
        }
        public double WidthRequest
        {
            get => _impl.Get<double>();
            set => _impl.Set(value);
        }
        public UniformThickness Padding
        {
            get => _impl.Get<UniformThickness>();
            set => _impl.Set(value);
        }
        public DisplayFormatOptions DisplayFormatOptions
        {
            get => _impl.Get<DisplayFormatOptions>();
            set => _impl.Set(value);
        }
    }
}
