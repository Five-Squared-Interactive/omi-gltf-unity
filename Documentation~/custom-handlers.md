# Custom Handlers Guide

This guide explains how to create custom handlers for OMI extensions, particularly useful for frameworks that wrap Unity objects.

## When to Use Custom Handlers

Custom handlers are useful when:

1. **You wrap Unity objects** - Your framework has its own entity/object system
2. **You need different behavior** - The default Unity components don't fit your needs
3. **You're building a platform** - Users of your platform load content at runtime
4. **You need preprocessing** - Data needs transformation before use

## Handler Types

### 1. Node Extension Handler

For extensions that attach to individual nodes (most common):

```csharp
public interface IOMINodeExtensionHandler<TData> : IOMIExtensionHandler<TData>
{
    void ImportNode(TData data, int nodeIndex, GameObject target, OMIImportContext context);
    TData ExportNode(GameObject source, int nodeIndex, OMIExportContext context);
}
```

**Use for:** spawn points, seats, links, physics bodies, vehicle parts

### 2. Document Extension Handler

For extensions defined at the document level:

```csharp
public interface IOMIDocumentExtensionHandler<TData> : IOMIExtensionHandler<TData>
{
    void ImportDocument(TData data, OMIImportContext context);
    TData ExportDocument(OMIExportContext context);
}
```

**Use for:** environment sky, audio emitter root, document-wide settings

## Creating a Custom Handler

### Step 1: Define Your Handler Class

```csharp
using OMI;
using OMI.Core;
using OMI.Extensions.SpawnPoint;
using UnityEngine;

public class MySpawnPointHandler : IOMINodeExtensionHandler<OMISpawnPointNode>
{
    public string ExtensionName => "OMI_spawn_point";

    // Document-level import (usually empty for node extensions)
    public void Import(OMISpawnPointNode data, OMIImportContext context)
    {
        // Called once per document, before nodes
    }

    // Document-level export
    public void Export(OMISpawnPointNode data, OMIExportContext context)
    {
        // Called once per document
    }

    // Per-node import - this is where the action happens
    public void ImportNode(OMISpawnPointNode data, int nodeIndex, 
        GameObject target, OMIImportContext context)
    {
        // Your custom logic here
    }

    // Per-node export
    public OMISpawnPointNode ExportNode(GameObject source, int nodeIndex, 
        OMIExportContext context)
    {
        // Return data to export, or null to skip
        return null;
    }
}
```

### Step 2: Implement Import Logic

**Option A: Use Unity Components (like default handlers)**

```csharp
public void ImportNode(OMISpawnPointNode data, int nodeIndex, 
    GameObject target, OMIImportContext context)
{
    var component = target.AddComponent<OMISpawnPointComponent>();
    component.title = data.Title;
    component.team = data.Team;
    component.group = data.Group;
}
```

**Option B: Use Custom Framework Objects**

```csharp
public void ImportNode(OMISpawnPointNode data, int nodeIndex, 
    GameObject target, OMIImportContext context)
{
    // Don't add Unity component - use our own system
    
    // Get our entity from custom data (set up earlier in pipeline)
    if (context.CustomData.TryGetValue($"entity_{nodeIndex}", out var obj))
    {
        var entity = obj as MyEntity;
        
        // Apply spawn point data to our entity
        entity.SpawnInfo = new SpawnPointInfo
        {
            Title = data.Title,
            Team = data.Team,
            Group = data.Group,
            Position = target.transform.position,
            Rotation = target.transform.rotation
        };
        
        // Register with spawn system
        SpawnManager.Instance.RegisterSpawn(entity);
    }
}
```

**Option C: Ignore GameObject Entirely**

```csharp
public void ImportNode(OMISpawnPointNode data, int nodeIndex, 
    GameObject target, OMIImportContext context)
{
    // We manage our own object graph, just need the data + index
    var worldMatrix = target.transform.localToWorldMatrix;
    
    MySceneGraph.AddSpawnPoint(new SpawnPointDef
    {
        NodeIndex = nodeIndex,
        Data = data,
        WorldTransform = worldMatrix
    });
}
```

### Step 3: Register Your Handler

```csharp
// Option 1: Replace default handler
var manager = new OMIExtensionManager();
OMIDefaultHandlers.RegisterAll(manager);
OMIDefaultHandlers.RegisterCustomHandler(manager, new MySpawnPointHandler());

// Option 2: Register only your handlers
var manager = new OMIExtensionManager();
manager.RegisterHandler<OMISpawnPointNode>(new MySpawnPointHandler());
manager.RegisterHandler<OMIPhysicsBodyNode>(new MyPhysicsBodyHandler());
// ... etc
```

## Using CustomData for Framework Integration

The `CustomData` dictionary allows handlers to communicate and store custom mappings:

### Setting Up Entity Mappings

```csharp
// In your loader, before OMI processing:
public async Task<MyScene> LoadSceneAsync(string url)
{
    var loader = new OMIGltfLoader();
    await loader.LoadAsync(url);
    
    // Create our entities for each node
    var root = await loader.InstantiateAsync();
    
    // Map our entities in CustomData
    foreach (var kvp in loader.ImportContext.NodeToGameObject)
    {
        int nodeIndex = kvp.Key;
        GameObject go = kvp.Value;
        
        var entity = new MyEntity(go);
        loader.ImportContext.CustomData[$"entity_{nodeIndex}"] = entity;
    }
    
    // Now process OMI extensions - handlers can access entities
    await loader.ProcessExtensionsAsync();
    
    return new MyScene(root);
}
```

### Accessing in Handlers

```csharp
public void ImportNode(OMIPhysicsBodyNode data, int nodeIndex, 
    GameObject target, OMIImportContext context)
{
    // Get our entity
    var entity = context.CustomData[$"entity_{nodeIndex}"] as MyEntity;
    
    // Get physics shape from another handler's data
    if (context.CustomData.TryGetValue($"shape_{nodeIndex}", out var shapeObj))
    {
        var shape = shapeObj as MyCollisionShape;
        entity.SetPhysics(data, shape);
    }
}
```

## Complete Example: WebVerse-Style Integration

```csharp
public class WebVerseOMIIntegration
{
    private OMIExtensionManager _manager;
    private WorldEngine _worldEngine;

    public WebVerseOMIIntegration(WorldEngine engine)
    {
        _worldEngine = engine;
        _manager = new OMIExtensionManager();
        
        // Register our custom handlers
        _manager.RegisterHandler<OMISpawnPointNode>(new WVSpawnPointHandler(engine));
        _manager.RegisterHandler<OMIPhysicsBodyNode>(new WVPhysicsBodyHandler(engine));
        _manager.RegisterHandler<OMISeatNode>(new WVSeatHandler(engine));
        _manager.RegisterHandler<OMIVehicleBodyData>(new WVVehicleHandler(engine));
        // ... etc
    }

    public async Task<WorldEntity> LoadGLTFAsync(string url, WorldEntity parent)
    {
        using var loader = new OMIGltfLoader(_manager);
        
        if (!await loader.LoadAsync(url))
            return null;

        var root = await loader.InstantiateAsync(parent?.Transform);
        
        // Create WorldEntities for each node
        var rootEntity = CreateEntityHierarchy(root, loader.ImportContext);
        
        // Process OMI extensions with our handlers
        await loader.ProcessExtensionsAsync();
        
        return rootEntity;
    }

    private WorldEntity CreateEntityHierarchy(GameObject root, OMIImportContext context)
    {
        var rootEntity = _worldEngine.CreateEntity(root);
        
        foreach (var kvp in context.NodeToGameObject)
        {
            var entity = _worldEngine.CreateEntity(kvp.Value);
            context.CustomData[$"entity_{kvp.Key}"] = entity;
        }
        
        return rootEntity;
    }
}

// Custom spawn point handler
public class WVSpawnPointHandler : IOMINodeExtensionHandler<OMISpawnPointNode>
{
    private readonly WorldEngine _engine;
    
    public WVSpawnPointHandler(WorldEngine engine) => _engine = engine;
    
    public string ExtensionName => "OMI_spawn_point";
    
    public void Import(OMISpawnPointNode data, OMIImportContext context) { }
    public void Export(OMISpawnPointNode data, OMIExportContext context) { }

    public void ImportNode(OMISpawnPointNode data, int nodeIndex, 
        GameObject target, OMIImportContext context)
    {
        var entity = context.CustomData[$"entity_{nodeIndex}"] as WorldEntity;
        
        // Add to WebVerse spawn system
        _engine.SpawnSystem.RegisterSpawnPoint(new SpawnPoint
        {
            Entity = entity,
            Team = data.Team,
            Group = data.Group,
            Title = data.Title
        });
        
        // Tag the entity
        entity.AddTag("spawn_point");
    }

    public OMISpawnPointNode ExportNode(GameObject source, int nodeIndex, 
        OMIExportContext context)
    {
        var entity = context.CustomData[$"entity_{nodeIndex}"] as WorldEntity;
        if (entity == null || !entity.HasTag("spawn_point"))
            return null;

        var spawn = _engine.SpawnSystem.GetSpawnPoint(entity);
        return new OMISpawnPointNode
        {
            Title = spawn.Title,
            Team = spawn.Team,
            Group = spawn.Group
        };
    }
}
```

## Best Practices

1. **Don't modify the GameObject if you don't need it** - Use `nodeIndex` and `CustomData`
2. **Store minimal data** - Don't duplicate what's in the glTF
3. **Use meaningful keys** - `entity_{nodeIndex}`, `physics_{nodeIndex}`, etc.
4. **Clean up CustomData** - Remove entries you no longer need
5. **Handle missing data gracefully** - Check for null/missing CustomData entries
6. **Document your keys** - Other handlers may need to access them
