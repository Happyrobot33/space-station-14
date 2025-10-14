using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;

namespace Content.Client._Starlight.Overlay;

public sealed class TunnelVisionOverlay : BaseVisionOverlay
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly SharedEyeSystem _sharedEyeSystem = default!;
    
    public TunnelVisionOverlay(ShaderPrototype shader) : base(shader) { ZIndex = (int?)OverlayZIndexes.TunnelVision; }

    //add logic to beforedraw
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        base.BeforeDraw(args);

        //get angle between player and mouse
        var mousePos = _inputManager.MouseScreenPosition.Position;

        //get rotation relative to 0.5, 0.5
        var screenCenter = new Vector2(args.Viewport.Size.X / 2, args.Viewport.Size.Y / 2);
        var dir = (mousePos - screenCenter).Normalized();
        var angle = MathF.Atan2(dir.Y, dir.X);

        //pass the angle as radians to the shader
        _shader.SetParameter("angle", angle);

        return true;
    }
}
