using System.Reflection;
using PdfSharp.Fonts;

namespace CarRental.Reporting.Worker.Pdf;

internal sealed class EmbeddedFontResolver : IFontResolver
{
    private const string RegularFace = "Vera";
    private const string BoldFace = "VeraBold";

    public byte[]? GetFont(string faceName) => faceName switch
    {
        RegularFace => ReadResource("Assets.Vera.ttf"),
        BoldFace => ReadResource("Assets.VeraBd.ttf"),
        _ => null
    };

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        new(isBold ? BoldFace : RegularFace);

    private static byte[] ReadResource(string suffix)
    {
        var assembly = typeof(EmbeddedFontResolver).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith(suffix, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded font resource '{suffix}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
