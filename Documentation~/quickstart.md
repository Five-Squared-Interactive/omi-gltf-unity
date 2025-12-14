# Quick Start Guide

Get up and running with the OMI Unity Plugin in minutes.

## Installation

### Via Unity Package Manager

1. Open **Window > Package Manager**
2. Click **+** → **Add package from git URL...**
3. Enter: `https://github.com/Five-Squared-Interactive/omi-gltf-unity.git`
4. Click **Add**

### Dependencies

The package automatically installs:
- glTFast (glTF import/export)
- Newtonsoft.Json (JSON parsing)

## Basic Usage

### Loading a glTF File

**Option 1: Via Component (No Code)**

1. Create an empty GameObject
2. Add Component: **OMI > glTF Loader**
3. Set the **URL** field
4. Enable **Load On Start**
5. Press Play

**Option 2: Via Code**

```csharp
using OMI.Integration;
using UnityEngine;

public class LoadExample : MonoBehaviour
{
    async void Start()
    {
        using var loader = new OMIGltfLoader();
        
        if (await loader.LoadAsync("https://example.com/model.glb"))
        {
            var root = await loader.InstantiateAsync(transform);
            Debug.Log($"Loaded: {root.name}");
        }
    }
}
```

### Exporting a glTF File

**Option 1: Via Menu**

1. Select the root GameObject in Hierarchy
2. Menu: **OMI > Export Selected to glTF**
3. Choose save location

**Option 2: Via Code**

```csharp
using OMI.Integration;

public async void ExportScene()
{
    var exporter = new OMIGltfExporter();
    await exporter.ExportAsync(gameObject, "output.gltf");
}
```

## Adding OMI Components

### Spawn Point

1. Select a GameObject
2. Add Component: **OMI > OMI Spawn Point**
3. Configure:
   - **Title**: Display name
   - **Team**: Team filter (optional)
   - **Group**: Group filter (optional)

### Physics Shape

1. Select a GameObject with a Collider
2. Add Component: **OMI > OMI Physics Shape**
3. The shape type is auto-detected from the Collider

### Physics Body

1. Select a GameObject (with OMI Physics Shape)
2. Add Component: **OMI > OMI Physics Body**
3. Configure:
   - **Motion Type**: Static, Kinematic, or Dynamic
   - **Mass**: For dynamic bodies
   - **Is Trigger**: For trigger volumes

### Seat

1. Create child GameObjects for: Back, Knee, Foot positions
2. Add Component: **OMI > OMI Seat** to parent
3. Assign the position references
4. Adjust **Knee Angle** (default: 90°)

### Link

1. Select a GameObject
2. Add Component: **OMI > OMI Link**
3. Set the **URI** (required)
4. Optionally set **Title** and **Description**

### Environment Sky

1. Create an empty GameObject
2. Add Component: **OMI > OMI Environment Sky**
3. Choose sky type:
   - **Plain**: Solid color
   - **Gradient**: Top/horizon/bottom colors
   - **Panorama**: HDR/panoramic texture
   - **Physical**: Atmospheric scattering (HDRP)

### Vehicle Body

1. Select your vehicle root GameObject
2. Add Component: **OMI > OMI Vehicle Body**
3. Configure:
   - **Gyro Stabilize**: Auto-leveling (0-1)
   - **Dampening**: Drag values
   - **Pilot Seat**: Reference to driver seat

### Vehicle Wheel

1. Select wheel GameObjects
2. Add Component: **OMI > OMI Vehicle Wheel**
3. Configure:
   - **Radius**: Wheel size
   - **Powered**: Is this a drive wheel?
   - **Max Steer Angle**: For steering wheels
   - **Suspension**: Stiffness, damping, travel

## Common Patterns

### Loading with Progress

```csharp
using OMI.Integration;
using UnityEngine;
using UnityEngine.UI;

public class LoadWithProgress : MonoBehaviour
{
    public Slider progressBar;
    public string url;

    async void Start()
    {
        using var loader = new OMIGltfLoader();
        
        // Subscribe to progress
        loader.OnProgress += (progress) => 
        {
            progressBar.value = progress;
        };
        
        if (await loader.LoadAsync(url))
        {
            await loader.InstantiateAsync(transform);
        }
    }
}
```

### Accessing Loaded Data

```csharp
using OMI.Integration;
using OMI.Extensions.SpawnPoint;
using UnityEngine;

public class AccessData : MonoBehaviour
{
    async void Start()
    {
        using var loader = new OMIGltfLoader();
        await loader.LoadAsync("level.glb");
        var root = await loader.InstantiateAsync();
        
        // Find all spawn points
        var spawnPoints = root.GetComponentsInChildren<OMISpawnPointComponent>();
        foreach (var spawn in spawnPoints)
        {
            Debug.Log($"Spawn: {spawn.title} at {spawn.transform.position}");
        }
    }
}
```

### Conditional Export

```csharp
using OMI.Integration;
using UnityEngine;

public class ConditionalExport : MonoBehaviour
{
    public async void ExportVisibleOnly()
    {
        var settings = new OMIExportSettings
        {
            Format = OMIExportFormat.GLB,
            IncludeInactive = false  // Skip inactive objects
        };
        
        var exporter = new OMIGltfExporter();
        await exporter.ExportAsync(gameObject, "visible_only.glb", settings);
    }
}
```

## Validation

Check your data before export:

```csharp
using OMI;
using OMI.Extensions.PhysicsBody;

public void ValidateBeforeExport()
{
    var bodyComponent = GetComponent<OMIPhysicsBodyComponent>();
    var data = bodyComponent.ToData();
    
    var result = OMIValidator.ValidatePhysicsBody(data);
    
    if (!result.IsValid)
    {
        result.LogAll();  // Logs errors and warnings
        return;
    }
    
    // Safe to export
}
```

## Troubleshooting

### "Extension not recognized"

Make sure default handlers are registered:
```csharp
var manager = new OMIExtensionManager();
OMIDefaultHandlers.RegisterAll(manager);
```

### "Missing reference" errors

OMI components reference other objects by glTF node index. After loading, these are resolved to GameObjects. If references are missing, check that the target objects exist in the glTF.

### Physics not working

1. Ensure `OMI_physics_shape` is on the same or child object
2. Check that motion type is correct (`dynamic` for moving objects)
3. Verify mass is positive for dynamic bodies

### Sky not appearing

1. Check that your render pipeline supports the sky type
2. Physical sky requires HDRP
3. Panorama requires proper texture format

## Next Steps

- [API Reference](api-reference.md) - Detailed API documentation
- [Extension Reference](extensions.md) - All supported extensions
- [Custom Handlers](custom-handlers.md) - Build your own handlers
