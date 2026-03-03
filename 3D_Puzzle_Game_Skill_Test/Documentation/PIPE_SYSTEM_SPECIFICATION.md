# Pipe Connection & Level System Specification

## Port Mask System (Bit Flags)
```
Up    = 0b0001 = 1   (Direction 0)
Right = 0b0010 = 2   (Direction 1)  
Down  = 0b0100 = 4   (Direction 2)
Left  = 0b1000 = 8   (Direction 3)
```

## Direction System
```
Direction Index:  0=Up, 1=Right, 2=Down, 3=Left
Opposite:         Up<->Down (0<->2), Right<->Left (1<->3)
Grid Movement:    Up: z+1, Right: x+1, Down: z-1, Left: x-1
```

---

## PIPE TYPES & PORT MASKS

### 1. Source / Destination (Single Port)
- **Port Count:** 1
- **Unique Rotations:** 4
- **Rotatable:** No (fixed in level)

| Rotation | Port Mask | Ports | Visual |
|----------|-----------|-------|--------|
| 0        | 0b0001=1  | Up    | ^      |
| 90       | 0b0010=2  | Right | >      |
| 180      | 0b0100=4  | Down  | v      |
| 270      | 0b1000=8  | Left  | <      |

**Rule:** Port MUST point to a cell with a pipe that CAN connect back.

---

### 2. Straight Pipe
- **Port Count:** 2 (opposite sides)
- **Unique Rotations:** 2 (0=180, 90=270)
- **Rotatable:** Yes (unless Static)

| Rotation | Port Mask | Ports      | Visual |
|----------|-----------|------------|--------|
| 0/180    | 0b0101=5  | Up+Down    | ||     |
| 90/270   | 0b1010=10 | Left+Right | =      |

**Rule:** Both ports MUST connect to pipes (decoy excepted).

---

### 3. Corner Pipe (L-Pipe)
- **Port Count:** 2 (adjacent sides)
- **Unique Rotations:** 4
- **Rotatable:** Yes (unless Static)

| Rotation | Port Mask | Ports      | Visual |
|----------|-----------|------------|--------|
| 0        | 0b0011=3  | Up+Right   | L (bottom-left corner) |
| 90       | 0b0110=6  | Right+Down | r (top-left corner)    |
| 180      | 0b1100=12 | Down+Left  | 7 (top-right corner)   |
| 270      | 0b1001=9  | Left+Up    | J (bottom-right corner)|

**Rule:** Both ports MUST connect to pipes (decoy excepted).

---

### 4. T-Junction Pipe
- **Port Count:** 3
- **Unique Rotations:** 4
- **Rotatable:** Yes (unless Static)

| Rotation | Port Mask | Ports          | Missing | Visual |
|----------|-----------|----------------|---------|--------|
| 0        | 0b0111=7  | Up+Right+Down  | Left    | |-     |
| 90       | 0b1110=14 | Right+Down+Left| Up      | T      |
| 180      | 0b1101=13 | Down+Left+Up   | Right   | -|     |
| 270      | 0b1011=11 | Left+Up+Right  | Down    | _|_    |

**Rule:** ALL 3 ports MUST connect to pipes (decoy excepted).

---

### 5. Cross Pipe (+)
- **Port Count:** 4
- **Unique Rotations:** 1 (all rotations identical)
- **Rotatable:** No (rotation doesn't matter)

| Rotation | Port Mask | Ports              | Visual |
|----------|-----------|-------------------|--------|
| Any      | 0b1111=15 | Up+Right+Down+Left| +      |

**Rule:** ALL 4 ports MUST connect to pipes (decoy excepted).

---

## STATIC VARIANTS

Static pipes have same port logic but:
- **Cannot be rotated by player**
- **Must be validated at generation time with FIXED rotation**
- **All ports must have valid connections in current rotation**

| Type               | Static Variant        |
|--------------------|----------------------|
| StraightPipe       | StaticStraightPipe   |
| CornerPipe         | StaticCornerPipe     |
| TJunctionPipe      | StaticTJunctionPipe  |
| CrossPipe          | StaticCrossPipe      |

---

## CONNECTION RULES

### Rule 1: Bidirectional Connection
For a valid connection between Cell A and Cell B:
- A must have a port facing B
- B must have a port facing A

```
Example: A at (0,0), B at (1,0) - B is Right of A
A needs: Right port (mask & 2)
B needs: Left port (mask & 8)
```

### Rule 2: Non-Decoy Pipes - All Ports Connected
Path pipes (non-decoy) must have ALL their ports connected:
- Straight: 2/2 ports connected
- Corner: 2/2 ports connected
- T-Junction: 3/3 ports connected
- Cross: 4/4 ports connected

### Rule 3: Boundary Validation
Static pipes and Source/Destination:
- Port pointing outside grid = INVALID
- Port pointing to empty cell = INVALID

### Rule 4: Decoy Pipes (Exception)
Decoy pipes may have unconnected ports. They are NOT part of solution path.

---

## GENERATION ALGORITHM

### Phase 1: Place Endpoints
1. Place Source on edge cell with rotation facing INWARD (not outside grid)
2. Place N Destinations with minimum distance from Source, rotation facing toward path

### Phase 2: Build Paths (A*)
1. Find path from Source to each Destination
2. For multi-destination: paths share common segments from Source

### Phase 3: Determine Pipe Types
For each path cell, count required directions:
```
1 direction  -> Part of Source/Dest port (already handled)
2 opposite   -> Straight (Up+Down or Left+Right)
2 adjacent   -> Corner
3 directions -> T-Junction
4 directions -> Cross
```

### Phase 4: Validate All Ports Have Connections
For non-decoy pipes, ensure EVERY port connects:
```
foreach pipe in pathPipes:
    portMask = GetPortMask(pipe)
    foreach direction where (portMask & dirMask) != 0:
        neighbor = GetNeighborInDirection(direction)
        if neighbor == null: 
            MUST change pipe type or add connecting pipe
        if !neighbor.HasPortFacing(opposite(direction)): 
            MUST adjust neighbor or change pipe type
```

### Phase 5: Convert to Static
Only convert if ALL conditions met:
- All ports point to valid cells (not boundary, not empty)
- All neighbors WILL connect back in solved state

### Phase 6: Scramble
Rotate non-static rotatable pipes to non-solved position.

---

## VALIDATION ALGORITHM

### Step 1: Basic Requirements
- Exactly 1 Source exists
- At least 1 Destination exists
- At least 1 pipe exists

### Step 2: Boundary Check (Static & Endpoints)
```
foreach piece where IsStatic or IsSource or IsDestination:
    portMask = GetPortMask(piece)
    foreach direction where (portMask & dirMask) != 0:
        nx = piece.x + dx[direction]
        nz = piece.z + dz[direction]
        
        if OutsideGrid(nx, nz): FAIL - "port facing outside"
        
        neighbor = GetPieceAt(nx, nz)
        if neighbor == null: FAIL - "port facing empty cell"
        
        if !CanNeighborConnectBack(neighbor, oppositeDir): FAIL
```

### Step 3: All-Ports-Connected Check (Non-Decoy)
```
foreach pipe where !IsDecoy:
    portMask = GetPortMask(pipe, solvedRotation)
    foreach direction where (portMask & dirMask) != 0:
        neighbor = GetNeighborInDirection(direction)
        neighborMask = GetPortMask(neighbor, neighborSolvedRotation)
        if (neighborMask & oppositeDirMask) == 0: FAIL
```

### Step 4: Solvability Check (Backtracking)
```
TrySolve(rotatableIndex):
    if rotatableIndex >= rotatableCount:
        return IsAllDestinationsConnected()
    
    piece = rotatables[rotatableIndex]
    for rotation in GetUniqueRotations(piece.type):
        piece.rotation = rotation
        if IsValidPartialState(piece):
            if TrySolve(rotatableIndex + 1):
                return true
    return false
```

### Step 5: BFS Connection Check
```
IsAllDestinationsConnected():
    visited = BFS from Source following connected ports
    return all Destinations in visited
```

---

## PORT MASK CALCULATION CODE

```csharp
int GetPortMask(PieceType type, int rotationDegrees)
{
    int rotStep = (rotationDegrees / 90) % 4;
    
    return type switch
    {
        // Single port - rotates to each direction
        Source or Destination => 
            new[] { 0b0001, 0b0010, 0b0100, 0b1000 }[rotStep],
        
        // Straight - alternates between vertical/horizontal
        StraightPipe or StaticStraightPipe => 
            new[] { 0b0101, 0b1010, 0b0101, 0b1010 }[rotStep],
        
        // Corner - rotates through 4 L-shapes
        CornerPipe or StaticCornerPipe => 
            new[] { 0b0011, 0b0110, 0b1100, 0b1001 }[rotStep],
        
        // T-Junction - rotates through 4 T-shapes
        TJunctionPipe or StaticTJunctionPipe => 
            new[] { 0b0111, 0b1110, 0b1101, 0b1011 }[rotStep],
        
        // Cross - always all 4 directions
        CrossPipe or StaticCrossPipe => 0b1111,
        
        _ => 0
    };
}
```

---

## DIRECTION HELPERS

```csharp
// Direction deltas (for grid navigation)
int[] DirDx = { 0, 1, 0, -1 };  // Up, Right, Down, Left
int[] DirDz = { 1, 0, -1, 0 };  // Up: z+1, Right: x+1, Down: z-1, Left: x-1

// Direction bit masks
int[] DirMask = { 1, 2, 4, 8 };  // Up=1, Right=2, Down=4, Left=8

// Opposite direction indices
int[] OppositeDir = { 2, 3, 0, 1 };  // Up<->Down, Right<->Left

// Check if two adjacent cells are connected
bool AreConnected(PieceData a, PieceData b, int dirFromAtoB)
{
    int aMask = GetPortMask(a.pieceType, a.rotation);
    int bMask = GetPortMask(b.pieceType, b.rotation);
    
    bool aHasPort = (aMask & DirMask[dirFromAtoB]) != 0;
    bool bHasPort = (bMask & DirMask[OppositeDir[dirFromAtoB]]) != 0;
    
    return aHasPort && bHasPort;
}
```

---

## KNOWN ISSUES TO FIX

### Issue 1: T-Junction All Ports Must Connect
Generator places T-Junction but doesn't verify all 3 ports have neighbors.
**Fix:** After determining T-Junction, check all 3 port directions have valid neighbors.
If not, use different pipe type (Corner or Straight) that matches available connections.

### Issue 2: Source/Destination Facing Outside
When Source placed at grid edge, rotation may point outside.
**Fix:** Calculate valid inward direction before placing. If at corner, pick one of two inward directions.

### Issue 3: Static Pipe Boundary Check
Static T-Junction placed at edge with port facing outside = unsolvable.
**Fix:** Before converting to static, verify ALL ports point to valid cells with connectable neighbors.

### Issue 4: Multi-Destination Path Merging
Paths to multiple destinations may not properly share segments.
**Fix:** Build tree structure from Source, branch only at cells with 3+ connections.

---

## VISUAL REFERENCE

### Grid Coordinate System
```
Z (Height/Row)
^
|  (0,2) (1,2) (2,2)   <- Row 2 (top)
|  (0,1) (1,1) (2,1)   <- Row 1
|  (0,0) (1,0) (2,0)   <- Row 0 (bottom)
+----------------------> X (Width/Column)
     Col0  Col1  Col2
```

### Port Direction Visual
```
       Up (dir=0, mask=1)
              ^
              |
Left (3,8) <--+--> Right (1,2)
              |
              v
       Down (dir=2, mask=4)
```

### Example: Valid T-Junction Placement
```
Position (1,1) with T-Junction at 0 rotation (Up+Right+Down, missing Left):

Grid:
      (0,2)  [P]   (2,2)
        |     ^
       [P]<--[T]-->[P]
        |     v
      (0,0)  [P]   (2,0)

T at (1,1) has ports: Up(1,2), Right(2,1), Down(1,0)
All 3 neighbors must have ports facing T:
- (1,2) needs Down port
- (2,1) needs Left port  
- (1,0) needs Up port
```

### Example: Invalid T-Junction Placement
```
Position (1,2) - top row, T-Junction at 0 rotation:

Grid:
      (0,2)  [T]   (2,2)   <- T has Up port but (1,3) is outside!
              ^
              |
      (0,1)  [P]   (2,1)

This is INVALID because Up port points outside grid.
```
