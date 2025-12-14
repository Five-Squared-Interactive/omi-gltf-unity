# Extension Reference

This document provides detailed information about each supported OMI extension.

---

## Physics Extensions

### OMI_physics_shape

Defines collision shapes for physics simulation.

**Namespace:** `OMI.Extensions.PhysicsShape`

#### Shape Types

| Type | Description | Unity Mapping |
|------|-------------|---------------|
| `box` | Axis-aligned box | `BoxCollider` |
| `sphere` | Sphere | `SphereCollider` |
| `capsule` | Capsule (Y-axis aligned) | `CapsuleCollider` |
| `cylinder` | Cylinder | `MeshCollider` (approximated) |
| `convex` | Convex hull from mesh | `MeshCollider` (convex) |
| `trimesh` | Triangle mesh | `MeshCollider` |

#### Data Classes

```csharp
// Main shape data
public class OMIPhysicsShapeData
{
    public string Type { get; set; }
    public OMIPhysicsShapeBoxData Box { get; set; }
    public OMIPhysicsShapeSphereData Sphere { get; set; }
    public OMIPhysicsShapeCapsuleData Capsule { get; set; }
    public OMIPhysicsShapeCylinderData Cylinder { get; set; }
    public OMIPhysicsShapeConvexData Convex { get; set; }
    public OMIPhysicsShapeTrimeshData Trimesh { get; set; }
}

// Box shape: default size is [1, 1, 1]
public class OMIPhysicsShapeBoxData
{
    public float[] Size { get; set; }  // [width, height, depth]
}

// Sphere shape: default radius is 1.0
public class OMIPhysicsShapeSphereData
{
    public float? Radius { get; set; }
}

// Capsule shape: default radius 0.5, height 2.0
public class OMIPhysicsShapeCapsuleData
{
    public float? Radius { get; set; }
    public float? Height { get; set; }
}
```

#### Unity Component

`OMIPhysicsShapeComponent` - Attached to GameObjects with collision shapes.

---

### OMI_physics_body

Defines rigid body physics properties.

**Namespace:** `OMI.Extensions.PhysicsBody`

#### Motion Types

| Type | Description | Unity Mapping |
|------|-------------|---------------|
| `static` | Non-moving collision | No Rigidbody |
| `kinematic` | Scripted movement | `Rigidbody.isKinematic = true` |
| `dynamic` | Physics-driven | `Rigidbody` |
| (trigger) | Trigger volume | `Collider.isTrigger = true` |

#### Data Classes

```csharp
public class OMIPhysicsBodyData
{
    public OMIPhysicsBodyMotionData Motion { get; set; }
    public OMIPhysicsBodyTriggerData Trigger { get; set; }
}

public class OMIPhysicsBodyMotionData
{
    public string Type { get; set; }           // "static", "kinematic", "dynamic"
    public float? Mass { get; set; }           // Default: 1.0
    public float[] LinearVelocity { get; set; }
    public float[] AngularVelocity { get; set; }
    public float[] CenterOfMass { get; set; }
    public float[] InertiaDiagonal { get; set; }
    public float[] InertiaOrientation { get; set; }  // Quaternion
}
```

---

### OMI_physics_joint

Defines physics constraints between bodies.

**Namespace:** `OMI.Extensions.PhysicsJoint`

#### Joint Types (by constraint configuration)

| Configuration | Description | Unity Mapping |
|---------------|-------------|---------------|
| No constraints | Fixed joint | `FixedJoint` |
| 1 angular axis | Hinge | `HingeJoint` |
| 1 linear axis | Slider | `ConfigurableJoint` |
| 3 angular axes | Ball socket | `ConfigurableJoint` |
| Multiple axes | 6DOF | `ConfigurableJoint` |

#### Data Classes

```csharp
public class OMIPhysicsJointData
{
    public int ConnectedNode { get; set; }
    public OMIPhysicsJointConstraint[] Constraints { get; set; }
}

public class OMIPhysicsJointConstraint
{
    public int[] LinearAxes { get; set; }   // [0]=X, [1]=Y, [2]=Z
    public int[] AngularAxes { get; set; }  // [0]=X, [1]=Y, [2]=Z
    public float? LowerLimit { get; set; }  // Default: -infinity
    public float? UpperLimit { get; set; }  // Default: +infinity
    public float? Stiffness { get; set; }   // Default: infinity (rigid)
    public float? Damping { get; set; }     // Default: 1.0
}
```

---

### OMI_physics_gravity

Defines custom gravity zones.

**Namespace:** `OMI.Extensions.PhysicsGravity`

#### Gravity Types

| Type | Description |
|------|-------------|
| `directional` | Uniform gravity in a direction |
| `point` | Gravity toward a point (planetary) |

#### Data Classes

```csharp
public class OMIPhysicsGravityData
{
    public string Type { get; set; }
    public OMIPhysicsGravityDirectionalData Directional { get; set; }
    public OMIPhysicsGravityPointData Point { get; set; }
}

public class OMIPhysicsGravityDirectionalData
{
    public float[] Gravity { get; set; }  // Default: [0, -9.81, 0]
}

public class OMIPhysicsGravityPointData
{
    public float? UnitDistance { get; set; }  // Distance where gravity = 9.81
}
```

---

## Interaction Extensions

### OMI_spawn_point

Defines spawn locations for players or objects.

**Namespace:** `OMI.Extensions.SpawnPoint`

#### Data Classes

```csharp
public class OMISpawnPointData
{
    public string Title { get; set; }  // Display name
    public string Team { get; set; }   // Team filter
    public string Group { get; set; }  // Group filter
}
```

#### Unity Component

`OMISpawnPointComponent` - Marks spawn locations with optional team/group.

---

### OMI_seat

Defines seating positions with IK hints.

**Namespace:** `OMI.Extensions.Seat`

#### Data Classes

```csharp
public class OMISeatData
{
    public int? Back { get; set; }    // Node index for back position
    public int? Foot { get; set; }    // Node index for foot position
    public int? Knee { get; set; }    // Node index for knee position
    public float? Angle { get; set; } // Knee angle in radians (default: π/2)
}
```

---

### OMI_link

Defines hyperlinks and navigation targets.

**Namespace:** `OMI.Extensions.Link`

#### Data Classes

```csharp
public class OMILinkData
{
    public string Uri { get; set; }         // Required - the link target
    public string Title { get; set; }       // Optional display name
    public string Description { get; set; } // Optional description
}
```

---

## Audio Extensions

### KHR_audio_emitter

Defines 3D audio sources.

**Namespace:** `OMI.Extensions.Audio`

#### Emitter Types

| Type | Description | Unity Mapping |
|------|-------------|---------------|
| `global` | Non-spatialized | 2D `AudioSource` |
| `positional` | 3D spatialized | 3D `AudioSource` |

#### Distance Models

| Model | Description |
|-------|-------------|
| `linear` | Linear rolloff |
| `inverse` | Inverse distance (default) |
| `exponential` | Exponential rolloff |

#### Data Classes

```csharp
public class KHRAudioEmitterRoot
{
    public KHRAudioData[] Audio { get; set; }
    public KHRAudioSourceData[] Sources { get; set; }
    public KHRAudioEmitterData[] Emitters { get; set; }
}

public class KHRAudioEmitterData
{
    public string Type { get; set; }        // "global" or "positional"
    public float? Gain { get; set; }        // Default: 1.0
    public int[] Sources { get; set; }      // Audio source indices
    public KHRAudioPositionalData Positional { get; set; }
}

public class KHRAudioPositionalData
{
    public float? ConeInnerAngle { get; set; }  // Default: 2π (omnidirectional)
    public float? ConeOuterAngle { get; set; }  // Default: 2π
    public float? ConeOuterGain { get; set; }   // Default: 0
    public string DistanceModel { get; set; }   // Default: "inverse"
    public float? MaxDistance { get; set; }     // Default: infinity
    public float? RefDistance { get; set; }     // Default: 1.0
    public float? RolloffFactor { get; set; }   // Default: 1.0
}
```

---

## Vehicle Extensions

### OMI_vehicle_body

Defines the main vehicle physics body.

**Namespace:** `OMI.Extensions.Vehicle`

#### Data Classes

```csharp
public class OMIVehicleBodyData
{
    public float? GyroStabilize { get; set; }     // 0-1, default: 0
    public float? LinearDampening { get; set; }   // Default: 0
    public float? AngularDampening { get; set; }  // Default: 0
    public int? PilotSeat { get; set; }           // Node index
    public int[] Seats { get; set; }              // Passenger seat indices
}
```

---

### OMI_vehicle_wheel

Defines vehicle wheels with suspension.

**Namespace:** `OMI.Extensions.Vehicle`

#### Data Classes

```csharp
public class OMIVehicleWheelData
{
    public float? Radius { get; set; }             // Default: 0.25
    public float? MaxSteerAngle { get; set; }      // Radians, default: 0
    public bool? Powered { get; set; }             // Default: false
    public float? MaxForce { get; set; }           // Default: 1000
    public float? SuspensionStiffness { get; set; } // Default: 20000
    public float? SuspensionDamping { get; set; }   // Default: 3000
    public float? SuspensionTravel { get; set; }    // Default: 0.15
}
```

---

### OMI_vehicle_thruster

Defines directional thrusters.

**Namespace:** `OMI.Extensions.Vehicle`

#### Data Classes

```csharp
public class OMIVehicleThrusterData
{
    public float? MaxForce { get; set; }       // Default: 1000
    public float? MaxGimbal { get; set; }      // Radians, default: 0
    public float? CurrentThrottle { get; set; } // 0-1
    public float[] CurrentGimbal { get; set; }  // [pitch, yaw]
}
```

---

### OMI_vehicle_hover_thruster

Defines hover thrusters for levitation.

**Namespace:** `OMI.Extensions.Vehicle`

#### Data Classes

```csharp
public class OMIVehicleHoverThrusterData
{
    public float? MaxForce { get; set; }    // Default: 1000
    public float? HoverHeight { get; set; } // Default: 1.0
    public float? HoverDamping { get; set; } // Default: 0.3
}
```

---

## Environment Extensions

### OMI_environment_sky

Defines skybox configuration.

**Namespace:** `OMI.Extensions.EnvironmentSky`

#### Sky Types

| Type | Description | Unity Support |
|------|-------------|---------------|
| `gradient` | Color gradient | Procedural skybox |
| `panorama` | Texture-based | Panoramic/Cubemap skybox |
| `physical` | Atmospheric scattering | HDRP only (fallback available) |
| `plain` | Solid color | Camera clear color |

#### Data Classes

```csharp
public class OMIEnvironmentSkySkyData
{
    public string Type { get; set; }
    public float[] AmbientLightColor { get; set; }   // Default: [1,1,1]
    public float? AmbientSkyContribution { get; set; } // Default: 1.0
    public OMIEnvironmentSkyGradientData Gradient { get; set; }
    public OMIEnvironmentSkyPanoramaData Panorama { get; set; }
    public OMIEnvironmentSkyPhysicalData Physical { get; set; }
    public OMIEnvironmentSkyPlainData Plain { get; set; }
}

public class OMIEnvironmentSkyGradientData
{
    public float[] TopColor { get; set; }     // Default: [0.3, 0.5, 1.0]
    public float[] HorizonColor { get; set; } // Default: [0.8, 0.8, 0.9]
    public float[] BottomColor { get; set; }  // Default: [0.3, 0.3, 0.3]
    public float? TopCurve { get; set; }      // Default: 0.2
    public float? BottomCurve { get; set; }   // Default: 0.2
    public float? SunAngleMax { get; set; }   // Radians, default: 0.5
    public float? SunCurve { get; set; }      // Default: 0.15
}
```

---

## AI/Character Extensions

### OMI_personality

Defines AI agent personality and behavior.

**Namespace:** `OMI.Extensions.Personality`

#### Data Classes

```csharp
public class OMIPersonalityData
{
    public string Agent { get; set; }         // AI agent identifier
    public string DefaultMessage { get; set; } // Initial/greeting message
    public string[] Traits { get; set; }       // Personality traits
}
```
