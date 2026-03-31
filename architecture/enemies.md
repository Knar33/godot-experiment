# Enemy Architecture

## Separation System (Boid Algorithm)

All enemies apply a separation steering force each physics frame that pushes them away from nearby neighbors. This prevents overlapping clumps while preserving each enemy type's existing AI movement.

### Core Type: `SeparationState` (pure C#, `GodotExperiment.Enemies`)

`SeparationState` computes a separation steering vector given a list of neighbor positions and distances. It lives in `src/GodotExperiment.Core/Enemies/SeparationState.cs` with no Godot dependency, fully unit-testable.

#### Constructor

```csharp
SeparationState(float detectionRadius, float separationWeight)
```

- `detectionRadius` — maximum distance at which another enemy is considered a neighbor (units).
- `separationWeight` — multiplier on the final separation force. Higher = stronger push.

#### Method: `ComputeSeparationForce`

```csharp
Vector2 ComputeSeparationForce(Vector2 myPosition, ReadOnlySpan<Vector2> neighborPositions)
```

Uses flat 2D positions (X/Z plane — all enemies move on the arena floor).

Algorithm:
1. For each neighbor within `detectionRadius`:
   - Compute the vector **away** from the neighbor: `myPosition - neighborPosition`
   - Normalize it, then scale by `(1 - distance / detectionRadius)` (linear falloff — full strength at overlap, zero at detection edge). If distance < `epsilon` (0.001), use a random unit vector at full strength to break symmetry.
   - Accumulate into a running sum.
2. If any neighbors were found, divide the accumulated vector by the neighbor count (average), then multiply by `separationWeight`.
3. Return the resulting 2D force vector. Returns zero if no neighbors are within range.

The linear falloff ensures smooth, gradual separation — enemies can partially overlap and push apart over time rather than snapping rigidly. The averaged output means single distant neighbors produce a gentle nudge while dense clusters build up meaningful pressure.

### Per-Type Configuration

| Enemy Type | Detection Radius | Separation Weight | Notes |
|------------|-----------------|-------------------|-------|
| Crawler    | 2.0             | 4.0               | Small radius; they're a pack, just not a pile |
| Spitter    | 3.0             | 5.0               | Keeps ranged enemies spread for better zoning |
| Charger    | 3.5             | 5.0               | Separates during approach; ignored mid-charge |
| Drone      | 2.5             | 4.0               | 3D separation (uses Y as well) for aerial spread |
| Bloater    | 4.0             | 6.0               | Large body; needs wide berth |
| Shade      | 3.0             | 4.0               | Separates during flanking approach |
| Sentinel   | 5.0             | 7.0               | Very large; must not overlap anything |
| Burrower   | 3.0             | 5.0               | Only while surfaced; ignored while underground |
| Howler     | 3.5             | 5.0               | Standard medium enemy spacing |
| Titan      | 6.0             | 8.0               | Massive; widest separation radius |

These values are configured as `[Export]` properties on each enemy scene, allowing tuning without code changes.

### Godot Integration: `BaseEnemy` Changes

`BaseEnemy` gains two new exported properties:

```csharp
[Export] public float SeparationRadius { get; set; } = 2.5f;
[Export] public float SeparationWeight { get; set; } = 4.0f;
```

And a `SeparationState` instance composed in `_Ready()`.

#### Neighbor Query

Each physics frame, `BaseEnemy` queries for neighbors using the `"enemy"` group:

```csharp
var enemies = GetTree().GetNodesInGroup("enemy");
```

For each enemy in the group (excluding self), if the flat distance is within `SeparationRadius`, its position is included in the neighbor list passed to `SeparationState.ComputeSeparationForce()`.

#### Velocity Blending

The separation force is blended into the enemy's movement in `MoveTowardPlayer()`:

```csharp
Vector3 toPlayer = (playerPos - myPos).Normalized();
Vector2 separationForce = _separation.ComputeSeparationForce(myPos2D, neighbors);
Vector3 separation3D = new(separationForce.X, 0, separationForce.Y);

Vector3 finalDirection = (toPlayer + separation3D).Normalized();
Velocity = finalDirection * MoveSpeed;
MoveAndSlide();
```

The separation vector is added to the player-seeking direction before normalization. Because `separationWeight` scales the separation magnitude, it naturally competes with the unit-length player direction — higher weights mean separation wins over seeking when enemies are very close.

#### Override Exemptions

Subclasses can disable separation during specific states:

- **Charger**: `MoveTowardPlayer()` override skips separation during charge and recovery phases.
- **Burrower**: Skips separation while underground.
- **Drone**: Uses 3D separation (includes Y component) since drones fly above the ground plane.

A protected property `SeparationEnabled` (default `true`) lets subclasses toggle it per-frame.

### Performance: Spatial Optimization

With 30-50 enemies, the naive O(n²) neighbor query is acceptable (~2500 distance checks per frame). If enemy counts scale beyond 60-80, the system should be upgraded to a flat-grid spatial hash:

- Arena divided into cells of size `maxSeparationRadius` (6 units, matching the Titan).
- Each frame, enemies register into their grid cell.
- Neighbor queries only check the home cell and 8 adjacent cells (9-cell neighborhood).
- Expected cost drops to O(n·k) where k is average neighbors per cell.

The initial implementation uses the simple group query. Spatial hashing is a follow-up optimization task if profiling shows a bottleneck.

### Tests

`tests/GodotExperiment.Tests/SeparationStateTests.cs` covers:

- No neighbors returns zero vector
- Single neighbor produces force directly away
- Multiple neighbors produces averaged separation direction
- Neighbors beyond detection radius are ignored
- Force magnitude scales inversely with distance (closer = stronger)
- Overlapping position (zero distance) produces a non-zero force (symmetry break)
- Separation weight scales the output magnitude
- Large neighbor counts produce stable, normalized output
