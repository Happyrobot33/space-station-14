using System.Numerics;
using Content.Client.Parallax.Managers;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.ViewVariables;

namespace Content.Client.Parallax;

/// <summary>
///     Renders the parallax background as a UI control.
/// </summary>
public sealed class ParallaxControl : Control
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IParallaxManager _parallaxManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [ViewVariables(VVAccess.ReadWrite)] public Vector2 Offset { get; set; }

    public ParallaxControl()
    {
        IoCManager.InjectDependencies(this);

        //Offset = new Vector2(_random.Next(0, 1000), _random.Next(0, 1000));
        RectClipContent = true;
        _parallaxManager.LoadParallaxByName("Default");
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        foreach (var layer in _parallaxManager.GetParallaxLayers("Default"))
        {
            var tex = layer.Texture;
            Vector2 texSizeTemp = new Vector2(
                (float)tex.Size.X * Size.X,
                (float)tex.Size.Y * Size.X);
            texSizeTemp *= layer.Config.Scale / 1080f;

            Vector2i texSize = new Vector2i(
                (int)texSizeTemp.X,
                (int)texSizeTemp.Y);

            var ourSize = PixelSize;

            //var currentTime = (float)_timing.RealTime.TotalSeconds;
            //var offset = Offset + new Vector2(currentTime * 100f, currentTime * 0f);
            var offset = Vector2.Zero;

            if (layer.Config.Tiled)
            {
                // Multiply offset by slowness to match normal parallax
                var scaledOffset = (offset * layer.Config.Slowness).Floored();

                // Then modulo the scaled offset by the size to prevent drawing a bunch of offscreen tiles for really small images.
                scaledOffset.X %= texSize.X;
                scaledOffset.Y %= texSize.Y;

                // Note: scaledOffset must never be below 0 or there will be visual issues.
                // It could be allowed to be >= texSize on a given axis but that would be wasteful.

                for (var x = -scaledOffset.X; x < ourSize.X; x += texSize.X)
                {
                    for (var y = -scaledOffset.Y; y < ourSize.Y; y += texSize.Y)
                    {
                        handle.DrawTextureRect(tex, UIBox2.FromDimensions(new Vector2(x, y), texSize));
                    }
                }
            }
            else
            {
                var origin = ((ourSize - texSize) / 2) + layer.Config.ControlHomePosition;
                handle.DrawTextureRect(tex, UIBox2.FromDimensions(origin, texSize));
            }
        }
    }
}

