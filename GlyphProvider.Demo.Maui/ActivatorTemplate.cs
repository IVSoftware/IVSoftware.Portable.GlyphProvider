using IVSoftware.Portable;
using System.Reflection;

namespace IVSGlyphProvider.Demo.Maui
{
    public class ActivatorTemplate<T> : ActivatorTemplate
        where T : View, IEnumIdComponent
    {
        public override IEnumIdComponent Activate(Enum id) => (IEnumIdComponent)Ctor.Value.Invoke([id]);

        private static readonly Lazy<ConstructorInfo> Ctor =
            new Lazy<ConstructorInfo>(() =>
            {
                var ctor = typeof(T).GetConstructor([typeof(Enum)]);
                if (ctor is null)
                    throw new InvalidOperationException($"{typeof(T).Name} must expose a public constructor (Enum).");
                return ctor;
            });
    }
}
