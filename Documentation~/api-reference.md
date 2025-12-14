# OMI Unity Plugin - API Reference

## Core Interfaces

### IOMIExtensionHandler\<TData\>

Base interface for all extension handlers.

```csharp
public interface IOMIExtensionHandler<TData> where TData : class
{
    /// <summary>
    /// The name of the extension this handler processes (e.g., "OMI_spawn_point").
    /// </summary>
    string ExtensionName { get; }

    /// <summary>
    /// Called during document-level import.
    /// </summary>
    void Import(TData data, OMIImportContext context);

    /// <summary>
    /// Called during document-level export.
    /// </summary>
    void Export(TData data, OMIExportContext context);
}
```

### IOMINodeExtensionHandler\<TData\>

For extensions that attach to individual nodes (most common).

```csharp
public interface IOMINodeExtensionHandler<TData> : IOMIExtensionHandler<TData> 
    where TData : class
{
    /// <summary>
    /// Called for each node with this extension during import.
    /// </summary>
    /// <param name="data">The parsed extension data.</param>
    /// <param name="nodeIndex">The glTF node index.</param>
    /// <param name="target">The Unity GameObject for this node.</param>
    /// <param name="context">The import context with document info.</param>
    void ImportNode(TData data, int nodeIndex, GameObject target, OMIImportContext context);

    /// <summary>
    /// Called for each node during export.
    /// </summary>
    /// <param name="source">The Unity GameObject being exported.</param>
    /// <param name="nodeIndex">The glTF node index being written.</param>
    /// <param name="context">The export context.</param>
    /// <returns>Extension data to write, or null to skip.</returns>
    TData ExportNode(GameObject source, int nodeIndex, OMIExportContext context);
}
```

### IOMIDocumentExtensionHandler\<TData\>

For extensions defined at the document level (e.g., environment sky).

```csharp
public interface IOMIDocumentExtensionHandler<TData> : IOMIExtensionHandler<TData> 
    where TData : class
{
    /// <summary>
    /// Called once during document import.
    /// </summary>
    void ImportDocument(TData data, OMIImportContext context);

    /// <summary>
    /// Called once during document export.
    /// </summary>
    /// <returns>Extension data for the document root, or null.</returns>
    TData ExportDocument(OMIExportContext context);
}
```

---

## Context Classes

### OMIImportContext

Provides access to import state and utilities.

```csharp
public class OMIImportContext
{
    /// <summary>
    /// The root GameObject being created.
    /// </summary>
    public GameObject Root { get; }

    /// <summary>
    /// Maps glTF node indices to Unity GameObjects.
    /// </summary>
    public IReadOnlyDictionary<int, GameObject> NodeToGameObject { get; }

    /// <summary>
    /// The raw glTF JSON for advanced parsing.
    /// </summary>
    public string RawJson { get; }

    /// <summary>
    /// Custom data dictionary for handler communication.
    /// Use for custom frameworks to store entity mappings.
    /// </summary>
    public Dictionary<string, object> CustomData { get; }

    /// <summary>
    /// Gets a GameObject by node index.
    /// </summary>
    public GameObject GetGameObject(int nodeIndex);

    /// <summary>
    /// Logs an import warning.
    /// </summary>
    public void LogWarning(string message);

    /// <summary>
    /// Logs an import error.
    /// </summary>
    public void LogError(string message);
}
```

### OMIExportContext

Provides access to export state and utilities.

```csharp
public class OMIExportContext
{
    /// <summary>
    /// The root GameObject being exported.
    /// </summary>
    public GameObject Root { get; }

    /// <summary>
    /// Maps Unity GameObjects to glTF node indices.
    /// </summary>
    public IReadOnlyDictionary<GameObject, int> GameObjectToNode { get; }

    /// <summary>
    /// Custom data dictionary for handler communication.
    /// </summary>
    public Dictionary<string, object> CustomData { get; }

    /// <summary>
    /// Gets the node index for a GameObject.
    /// </summary>
    public int GetNodeIndex(GameObject gameObject);

    /// <summary>
    /// Registers a texture for export.
    /// </summary>
    public int RegisterTexture(Texture2D texture);

    /// <summary>
    /// Registers a mesh for export.
    /// </summary>
    public int RegisterMesh(Mesh mesh);
}
```

---

## Extension Manager

### OMIExtensionManager

Central registry for extension handlers.

```csharp
public class OMIExtensionManager
{
    /// <summary>
    /// Registers a handler for an extension type.
    /// </summary>
    public void RegisterHandler<TData>(IOMIExtensionHandler<TData> handler) 
        where TData : class;

    /// <summary>
    /// Unregisters handlers for an extension by name.
    /// </summary>
    public void UnregisterHandler(string extensionName);

    /// <summary>
    /// Gets all registered handlers for an extension.
    /// </summary>
    public IEnumerable<IOMIExtensionHandler<TData>> GetHandlers<TData>() 
        where TData : class;

    /// <summary>
    /// Checks if a handler is registered for an extension.
    /// </summary>
    public bool HasHandler(string extensionName);
}
```

### OMIDefaultHandlers

Helper class to register all default handlers.

```csharp
public static class OMIDefaultHandlers
{
    /// <summary>
    /// Registers all default Unity handlers.
    /// </summary>
    public static void RegisterAll(OMIExtensionManager manager);

    /// <summary>
    /// Register individual handlers:
    /// </summary>
    public static void RegisterPhysicsShapeHandler(OMIExtensionManager manager);
    public static void RegisterPhysicsBodyHandler(OMIExtensionManager manager);
    public static void RegisterPhysicsJointHandler(OMIExtensionManager manager);
    public static void RegisterPhysicsGravityHandler(OMIExtensionManager manager);
    public static void RegisterSpawnPointHandler(OMIExtensionManager manager);
    public static void RegisterSeatHandler(OMIExtensionManager manager);
    public static void RegisterLinkHandler(OMIExtensionManager manager);
    public static void RegisterPersonalityHandler(OMIExtensionManager manager);
    public static void RegisterAudioEmitterHandler(OMIExtensionManager manager);
    public static void RegisterVehicleBodyHandler(OMIExtensionManager manager);
    public static void RegisterVehicleWheelHandler(OMIExtensionManager manager);
    public static void RegisterVehicleThrusterHandler(OMIExtensionManager manager);
    public static void RegisterVehicleHoverThrusterHandler(OMIExtensionManager manager);
    public static void RegisterEnvironmentSkyHandler(OMIExtensionManager manager);

    /// <summary>
    /// Replace a default handler with a custom one.
    /// </summary>
    public static void RegisterCustomHandler<TData>(
        OMIExtensionManager manager, 
        IOMIExtensionHandler<TData> handler) where TData : class;
}
```

---

## Validation

### OMIValidator

Static validation utilities.

```csharp
public static class OMIValidator
{
    /// <summary>
    /// Validation result structure.
    /// </summary>
    public struct ValidationResult
    {
        public bool IsValid;
        public List<string> Errors;
        public List<string> Warnings;
        
        public void AddError(string error);
        public void AddWarning(string warning);
        public void Merge(ValidationResult other);
        public void LogAll();
    }

    // Physics validators
    public static ValidationResult ValidatePhysicsShape(OMIPhysicsShape data);
    public static ValidationResult ValidatePhysicsBody(OMIPhysicsBodyNode data);
    public static ValidationResult ValidatePhysicsJoint(OMIPhysicsJointData data, int maxNodeIndex = int.MaxValue);
    public static ValidationResult ValidatePhysicsGravity(OMIPhysicsGravityData data);

    // Interaction validators
    public static ValidationResult ValidateSpawnPoint(OMISpawnPointNode data);
    public static ValidationResult ValidateSeat(OMISeatNode data);
    public static ValidationResult ValidateLink(OMILinkNode data);

    // Audio validators
    public static ValidationResult ValidateAudioEmitter(KHRAudioEmitterData data);

    // Vehicle validators
    public static ValidationResult ValidateVehicleBody(OMIVehicleBodyData data);
    public static ValidationResult ValidateVehicleWheel(OMIVehicleWheelData data);
    public static ValidationResult ValidateVehicleThruster(OMIVehicleThrusterData data);
    public static ValidationResult ValidateVehicleHoverThruster(OMIVehicleHoverThrusterData data);

    // Environment validators
    public static ValidationResult ValidateEnvironmentSky(OMIEnvironmentSkySkyData data);

    // Character validators
    public static ValidationResult ValidatePersonality(OMIPersonalityData data);
}
```

---

## Integration

### OMIGltfLoader

Main class for loading glTF files with OMI extensions.

```csharp
public class OMIGltfLoader : IDisposable
{
    /// <summary>
    /// The extension manager used by this loader.
    /// </summary>
    public OMIExtensionManager ExtensionManager { get; }

    /// <summary>
    /// Loads a glTF from a URL or file path.
    /// </summary>
    public async Task<bool> LoadAsync(string url);

    /// <summary>
    /// Loads a glTF from a byte array.
    /// </summary>
    public async Task<bool> LoadAsync(byte[] data);

    /// <summary>
    /// Instantiates the loaded glTF as a GameObject.
    /// </summary>
    public async Task<GameObject> InstantiateAsync(Transform parent = null);

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose();
}
```

### OMIGltfExporter

Main class for exporting GameObjects to glTF.

```csharp
public class OMIGltfExporter
{
    /// <summary>
    /// The extension manager used by this exporter.
    /// </summary>
    public OMIExtensionManager ExtensionManager { get; }

    /// <summary>
    /// Exports a GameObject to a glTF file.
    /// </summary>
    public async Task<bool> ExportAsync(GameObject root, string path);

    /// <summary>
    /// Exports a GameObject with custom settings.
    /// </summary>
    public async Task<bool> ExportAsync(GameObject root, string path, OMIExportSettings settings);

    /// <summary>
    /// Exports a GameObject to a byte array.
    /// </summary>
    public async Task<byte[]> ExportToBufferAsync(GameObject root, OMIExportSettings settings);
}
```

### OMIExportSettings

Configuration for export operations.

```csharp
public class OMIExportSettings
{
    /// <summary>
    /// Output format (GLTF or GLB).
    /// </summary>
    public OMIExportFormat Format { get; set; } = OMIExportFormat.GLTF;

    /// <summary>
    /// Whether to include inactive GameObjects.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Copyright string for the asset.
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    /// Generator string for the asset.
    /// </summary>
    public string Generator { get; set; } = "OMI Unity Plugin";
}
```
