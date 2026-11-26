using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Atlas.Api.Tests")]

namespace Atlas.Api;

internal static class GifFrameBatches
{
    // Keep the first batch's screen and loop metadata. Later batches contribute only frames
    // and their timing. Preserve each batch's palette as a local palette when needed.
    internal static void Append(Stream output, ReadOnlySpan<byte> gif, bool firstBatch)
    {
        if (!gif.StartsWith("GIF89a"u8) && !gif.StartsWith("GIF87a"u8))
            throw new InvalidDataException("Expected an encoded GIF batch.");

        var screenFlags = gif[10];
        var paletteSize = screenFlags & 7;
        var palette = (screenFlags & 0x80) != 0 ? gif.Slice(13, 3 * (2 << paletteSize)) : default;
        var offset = 13 + palette.Length;
        if (firstBatch)
            output.Write(gif[..offset]);

        while (gif[offset] != 0x3b)
        {
            var start = offset;
            switch (gif[offset++])
            {
                case 0x21: // Extension block
                    var label = gif[offset++];
                    offset = SkipSubBlocks(gif, offset);
                    if (firstBatch || label == 0xf9) // Graphic control: delay and disposal
                        output.Write(gif[start..offset]);
                    break;
                case 0x2c: // Image descriptor
                    var flags = gif[offset + 8];
                    offset += 9;
                    if ((flags & 0x80) != 0)
                    {
                        offset += 3 * (2 << (flags & 7));
                        output.Write(gif[start..offset]);
                    }
                    else if (firstBatch)
                    {
                        output.Write(gif[start..offset]);
                    }
                    else
                    {
                        if (palette.IsEmpty)
                            throw new InvalidDataException("GIF frame has no palette.");
                        output.Write(gif[start..(offset - 1)]);
                        output.WriteByte((byte)((flags & 0x78) | 0x80 | paletteSize));
                        output.Write(palette);
                    }

                    var imageDataStart = offset;
                    offset = SkipSubBlocks(gif, offset + 1); // Skip the LZW minimum code size.
                    output.Write(gif[imageDataStart..offset]);
                    break;
                default:
                    throw new InvalidDataException("Unexpected GIF block in encoded batch.");
            }
        }
    }

    private static int SkipSubBlocks(ReadOnlySpan<byte> gif, int offset)
    {
        while (gif[offset] != 0)
            offset += 1 + gif[offset];
        return offset + 1;
    }
}
