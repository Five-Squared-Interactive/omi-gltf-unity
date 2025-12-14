# Package Development Guide

This document provides information for developers who want to contribute to or understand the structure of the OMI glTF Unity package.

## Package Structure

This repository follows the Unity Package Manager (UPM) structure for Git URL installation:

```
/
├── package.json              # Package manifest (required)
├── README.md                # Package documentation
├── LICENSE.md               # License file
├── CHANGELOG.md             # Version history
├── CONTRIBUTING.md          # This file
├── Runtime/                 # Runtime scripts (required)
│   ├── Core/               # Core interfaces and managers
│   │   ├── IOMIExtensionHandler.cs
│   │   ├── OMIExtensionManager.cs
│   │   ├── OMIImportContext.cs
│   │   ├── OMIExportContext.cs
│   │   ├── OMISettings.cs
│   │   ├── OMIValidator.cs
│   │   └── OMIDefaultHandlers.cs
│   ├── Extensions/         # OMI extension implementations
│   │   ├── PhysicsShape/
│   │   ├── PhysicsBody/
│   │   ├── PhysicsJoint/
│   │   ├── SpawnPoint/
│   │   ├── Seat/
│   │   └── Link/
│   ├── DefaultHandlers/    # Default Unity component handlers
│   ├── Integration/        # glTFast integration
│   └── OMI.asmdef         # Assembly definition
└── Editor/                 # Editor scripts (optional)
    ├── OMISettingsEditor.cs
    ├── OMIComponentInspectors.cs
    └── OMI.Editor.asmdef

# Excluded from package (via .gitignore)
Assets/                     # Test Unity project assets
ProjectSettings/            # Test Unity project settings
Packages/                   # Test Unity project packages
```

## Key Files

### package.json
The package manifest defines metadata, dependencies, and version information. This is required for Unity Package Manager to recognize the repository as a valid package.

**Important fields:**
- `name`: Must follow reverse domain notation (e.g., `com.fivesquared.omi-gltf`)
- `version`: Semantic versioning (e.g., `0.1.0`)
- `unity`: Minimum Unity version (e.g., `2021.3`)
- `dependencies`: Other UPM packages this package depends on

### Runtime/ and Editor/
These directories contain the actual package code:
- `Runtime/`: Code that runs in builds and the editor
- `Editor/`: Editor-only code (custom inspectors, tools, etc.)

Each directory should have an `.asmdef` (Assembly Definition) file to define the assembly.

### .gitignore
Configured to exclude Unity project files that shouldn't be part of the package:
- `/Assets/` - Test project assets
- `/ProjectSettings/` - Project settings
- `/Packages/` - Other packages

## Development Workflow

### Setting Up for Development

1. Clone the repository:
   ```bash
   git clone https://github.com/Five-Squared-Interactive/omi-gltf-unity.git
   cd omi-gltf-unity
   ```

2. Open in Unity (the repository root contains both the package and a test project)

3. The package code is in `Runtime/` and `Editor/` directories

4. Test assets and scenes are in `Assets/` (not included in the package)

### Making Changes

1. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make changes to files in `Runtime/` or `Editor/`

3. Test your changes using the test project in `Assets/`

4. Update `CHANGELOG.md` with your changes

5. Commit and push your changes

### Testing the Package

To test the package in another Unity project:

1. Add the package using your branch:
   ```
   https://github.com/Five-Squared-Interactive/omi-gltf-unity.git#feature/your-feature-name
   ```

2. Verify that the package installs correctly and functions as expected

### Versioning

This package follows [Semantic Versioning](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality additions
- **PATCH** version for backwards-compatible bug fixes

Update `package.json` version and create a git tag when releasing:
```bash
git tag -a v0.1.0 -m "Release version 0.1.0"
git push origin v0.1.0
```

## Dependencies

This package depends on:
- `com.unity.cloud.gltfast` - For glTF loading and exporting
- `com.unity.nuget.newtonsoft-json` - For JSON serialization

These are automatically installed by Unity Package Manager when the package is added.

## Code Style

- Follow Unity C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Keep methods focused and small
- Use async/await for asynchronous operations

## Assembly Definitions

The package uses Assembly Definitions (`.asmdef`) to:
- Explicitly define dependencies
- Improve compilation times
- Separate Runtime and Editor code

Do not modify these unless adding new dependencies.

## Contributing

For general contribution guidelines, see the [OMI Group's contribution guidelines](https://github.com/omigroup/gltf-extensions/blob/main/CONTRIBUTING.md).

## Resources

- [Unity Package Layout](https://docs.unity3d.com/Manual/cus-layout.html)
- [Git URL Installation](https://docs.unity3d.com/Manual/upm-git.html)
- [OMI Extensions](https://github.com/omigroup/gltf-extensions)
- [glTFast Documentation](https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@latest)
