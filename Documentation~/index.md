# OMI Unity Plugin Documentation

Welcome to the OMI Unity Plugin documentation. This plugin enables Unity to import and export glTF files with [OMI (Open Metaverse Interoperability)](https://github.com/omigroup/gltf-extensions) extensions.

## Documentation

| Document | Description |
|----------|-------------|
| [Quick Start](quickstart.md) | Get up and running in minutes |
| [API Reference](api-reference.md) | Complete API documentation |
| [Extension Reference](extensions.md) | Details on all supported extensions |
| [Custom Handlers](custom-handlers.md) | Build handlers for your framework |

## Supported Extensions

### Physics
- ✅ `OMI_physics_shape` - Collision shapes
- ✅ `OMI_physics_body` - Rigid body physics
- ✅ `OMI_physics_joint` - Physics constraints
- ✅ `OMI_physics_gravity` - Custom gravity zones

### Interaction
- ✅ `OMI_spawn_point` - Spawn locations
- ✅ `OMI_seat` - Seating with IK
- ✅ `OMI_link` - Hyperlinks

### Audio
- ✅ `KHR_audio_emitter` - 3D audio
- ✅ `OMI_audio_ogg_vorbis` - Ogg Vorbis codec
- ✅ `OMI_audio_opus` - Opus codec

### Vehicles
- ✅ `OMI_vehicle_body` - Vehicle physics
- ✅ `OMI_vehicle_wheel` - Wheels with suspension
- ✅ `OMI_vehicle_thruster` - Directional thrusters
- ✅ `OMI_vehicle_hover_thruster` - Hover thrusters

### Environment
- ✅ `OMI_environment_sky` - Skybox configuration

### AI/Character
- ✅ `OMI_personality` - AI personality traits

## Quick Example

```csharp
using OMI.Integration;
using UnityEngine;

public class Example : MonoBehaviour
{
    async void Start()
    {
        // Load a glTF with OMI extensions
        using var loader = new OMIGltfLoader();
        await loader.LoadAsync("https://example.com/world.glb");
        await loader.InstantiateAsync(transform);
    }
}
```

## Requirements

- Unity 2021.3+
- glTFast 6.0+

## Links

- [GitHub Repository](https://github.com/Five-Squared-Interactive/omi-gltf-unity)
- [OMI Group](https://omigroup.org/)
- [OMI glTF Extensions Spec](https://github.com/omigroup/gltf-extensions)
