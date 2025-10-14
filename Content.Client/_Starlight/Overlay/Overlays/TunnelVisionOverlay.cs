using Robust.Client.Graphics;

namespace Content.Client._Starlight.Overlay;

public sealed class TunnelVisionOverlay : BaseVisionOverlay
{
    public TunnelVisionOverlay(ShaderPrototype shader) : base(shader) { ZIndex = (int?)OverlayZIndexes.TunnelVision; }
}
