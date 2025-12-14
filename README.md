# OMI Unity Plugin

A Unity plugin for importing and exporting glTF files with [OMI (Open Metaverse Interoperability)](https://github.com/omigroup/gltf-extensions) extensions.

## Features

- **Interface-driven architecture**: Easily customize how OMI extensions map to your project's objects
- **Default Unity handlers**: Built-in support for converting OMI extensions to Unity components
- **glTFast integration**: Seamless integration with the [glTFast](https://github.com/atteneder/glTFast) library
- **Import & Export**: Full support for both importing and exporting OMI extension data
- **Async operations**: Non-blocking import/export operations
- **Validation**: Built-in validation utilities for extension data

## Supported Extensions

### Physics
- `OMI_physics_shape` - Collision shapes (box, sphere, capsule, cylinder, convex, trimesh)
- `OMI_physics_body` - Rigid body physics (static, kinematic, dynamic)
- `OMI_physics_joint` - Physics constraints and joints

### Interaction
- `OMI_spawn_point` - Spawn points for players/objects
- `OMI_seat` - Seating positions with IK data
- `OMI_link` - Hyperlinks and navigation targets

### Audio (Coming Soon)
- `KHR_audio_emitter` - 3D audio emitters
- `OMI_audio_ogg_vorbis` - Ogg Vorbis audio codec
- `OMI_audio_opus` - Opus audio codec

### Vehicles (Coming Soon)
- `OMI_vehicle_body` - Vehicle physics body
- `OMI_vehicle_wheel` - Vehicle wheels
- `OMI_vehicle_thruster` - Directional thrusters
- `OMI_vehicle_hover_thruster` - Hovering thrusters

## Installation

### Via Unity Package Manager (Git URL)

1. Open Unity Package Manager (Window > Package Manager)
2. Click the `+` button in the top-left corner
3. Select "Add package from git URL..."
4. Enter the following URL:
   ```
   https://github.com/Five-Squared-Interactive/omi-gltf-unity.git
   ```
5. Click "Add"

The package manager will automatically install the required dependencies (glTFast and Newtonsoft.Json).

### Via Git URL in manifest.json

Alternatively, you can add the package directly to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.fivesquared.omi-gltf": "https://github.com/Five-Squared-Interactive/omi-gltf-unity.git",
    ...
  }
}
```

### Specific Version/Branch

To install a specific version or branch, append `#` followed by the branch name, tag, or commit hash:

```
https://github.com/Five-Squared-Interactive/omi-gltf-unity.git#v0.1.0
https://github.com/Five-Squared-Interactive/omi-gltf-unity.git#main
https://github.com/Five-Squared-Interactive/omi-gltf-unity.git#develop
```

### Manual Installation (Alternative)

If you prefer not to use Git URL installation:

1. Install glTFast via Unity Package Manager (required dependency)
2. Download or clone this repository
3. Copy the `Runtime` and `Editor` folders to your project's `Assets/OMI` directory

## Quick Start

### Loading a glTF with OMI Extensions

```csharp
using OMI.Integration;
using UnityEngine;

public class LoadExample : MonoBehaviour
{
    public string gltfUrl = "https://example.com/model.glb";

    async void Start()
    {
        using var loader = new OMIGltfLoader();
        
        if (await loader.LoadAsync(gltfUrl))
        {
            var root = await loader.InstantiateAsync(transform);
            Debug.Log($"Loaded: {root.name}");
        }
    }
}
```

### Using the Loader Component

Add an `OMI > glTF Loader` component to any GameObject, set the URL, and enable "Load On Start".

### Custom Handlers

Implement custom handlers to control how OMI extensions are processed:

```csharp
using OMI;
using OMI.Extensions.SpawnPoint;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MySpawnPointHandler : IOMINodeExtensionHandler<OMISpawnPointNode>
{
    public string ExtensionName => "OMI_spawn_point";
    public int Priority => 100;

    public Task OnImportAsync(OMISpawnPointNode data, OMIImportContext context, 
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task<OMISpawnPointNode> OnExportAsync(OMIExportContext context, 
        CancellationToken ct = default)
    {
        return Task.FromResult<OMISpawnPointNode>(null);
    }

    public Task OnNodeImportAsync(OMISpawnPointNode data, int nodeIndex, 
        GameObject target, OMIImportContext context, CancellationToken ct = default)
    {
        // Your custom logic here
        Debug.Log($"Custom spawn point: {data.Title} on {target.name}");
        return Task.CompletedTask;
    }

    public Task<OMISpawnPointNode> OnNodeExportAsync(GameObject source, 
        OMIExportContext context, CancellationToken ct = default)
    {
        // Your custom export logic
        return Task.FromResult<OMISpawnPointNode>(null);
    }
}
```

Register your handler:

```csharp
var manager = new OMIExtensionManager();
manager.RegisterHandler<OMISpawnPointNode>(new MySpawnPointHandler());
```

## Project Structure

```
OMI/
├── Runtime/
│   ├── Core/
│   │   ├── IOMIExtensionHandler.cs    # Handler interfaces
│   │   ├── OMIExtensionManager.cs     # Handler registry
│   │   ├── OMIImportContext.cs        # Import context
│   │   ├── OMIExportContext.cs        # Export context
│   │   ├── OMISettings.cs             # Settings classes
│   │   └── OMIValidator.cs            # Validation utilities
│   ├── Extensions/
│   │   ├── PhysicsShape/              # OMI_physics_shape
│   │   ├── PhysicsBody/               # OMI_physics_body
│   │   ├── PhysicsJoint/              # OMI_physics_joint
│   │   ├── SpawnPoint/                # OMI_spawn_point
│   │   ├── Seat/                      # OMI_seat
│   │   └── Link/                      # OMI_link
│   ├── DefaultHandlers/               # Built-in Unity handlers
│   └── Integration/                   # glTFast integration
└── Editor/
    ├── OMISettingsEditor.cs           # Settings inspector
    └── OMIComponentInspectors.cs      # Component inspectors
```

## Requirements

- Unity 2021.3 or later
- glTFast 6.0 or later
- Newtonsoft.Json (via glTFast)

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please see the OMI Group's [contribution guidelines](https://github.com/omigroup/gltf-extensions/blob/main/CONTRIBUTING.md).

## Links

- [OMI Group](https://omigroup.org/)
- [OMI glTF Extensions](https://github.com/omigroup/gltf-extensions)
- [glTFast](https://github.com/atteneder/glTFast)
