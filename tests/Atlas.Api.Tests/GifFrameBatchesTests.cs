using ImageMagick;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Atlas.Api.Tests;

[TestClass]
public sealed class GifFrameBatchesTests
{
    [TestMethod]
    public void EncodedBatchesPreserveFrameOffsetsColorsAndTiming()
    {
        using var output = new MemoryStream();
        using var batch = new MemoryStream();
        for (var start = 0; start < 720; start += 24)
        {
            using var frames = new MagickImageCollection();
            for (var index = start; index < start + 24; index++)
            {
                var first = index == 0;
                var color = first ? MagickColors.White : start % 48 == 0 ? MagickColors.Red : MagickColors.Blue;
                var frame = new MagickImage(color, first ? 8u : 2u, first ? 6u : 2u)
                {
                    AnimationDelay = (uint)((index + 1) * 12000 / 720 - index * 12000 / 720),
                    GifDisposeMethod = GifDisposeMethod.None
                };
                if (first)
                    frame.AnimationIterations = 0;
                else
                    frame.Page = new MagickGeometry(2, 3, 8, 6);
                frames.Add(frame);
            }

            batch.SetLength(0);
            frames.Write(batch, MagickFormat.Gif);
            GifFrameBatches.Append(output, batch.GetBuffer().AsSpan(0, (int)batch.Length), start == 0);
        }

        output.WriteByte(0x3b);
        output.Position = 0;
        using var decoded = new MagickImageCollection(output);
        Assert.AreEqual(720, decoded.Count);
        Assert.AreEqual(12000L, decoded.Sum(frame => (long)frame.AnimationDelay));
        Assert.AreEqual(0u, decoded[0].AnimationIterations);
        decoded.Coalesce();
        using var pixels = decoded[24].GetPixels();
        Assert.AreEqual(MagickColors.White, pixels.GetPixel(0, 0).ToColor());
        Assert.AreEqual(MagickColors.Blue, pixels.GetPixel(2, 3).ToColor());
    }
}
