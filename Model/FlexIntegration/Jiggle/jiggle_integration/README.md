# JiggleBone Integration Package

Drop the **Runtime/Scripts** folder into your Unity project alongside uSource.

## Files

- **JiggleBoneBehaviour.cs**  
  A MonoBehaviour that simulates a simple spring-damper on each bone.

- **JiggleIntegrator.cs**  
  Static helper that reads `mstudiojigglebone_t[]` from your `MDLFile` and wires up `JiggleBoneBehaviour` per bone.

## Usage

In your `MDLFile.BuildModel` (after you call `ApplyFlexAssets`), add:

```csharp
JiggleIntegrator.Setup(ModelObject, this);
```

That’s it! Your imported models will now have jiggle-bone physics driven by the original Source parameters.

## Optional Tweaks

- **fadeToBone**: lerps between `zmin` and `zmax` influences; set per-instance or via code.
- Expose additional fields (gravity, angle limits, etc.) in `JiggleBoneBehaviour` to match more advanced Source jiggle features.
