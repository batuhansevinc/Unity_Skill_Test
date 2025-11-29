using UnityEngine;
using BufoGames.Constants;
using BufoGames.Controller;
using BufoGames.Data;
using BufoGames.Enums;

namespace BufoGames.Generation
{
    /// <summary>
    /// Spawns puzzle pieces (pipes, source, destination) from level data
    /// </summary>
    public class PieceSpawner
    {
        public void SpawnPieces(GameObject parent, LevelDataSO levelData, ThemeDataSO theme)
        {
            if (levelData.pieces == null || levelData.pieces.Count == 0)
            {
                Debug.LogWarning("PieceSpawner: No pieces to spawn!");
                return;
            }
            
            foreach (var pieceData in levelData.pieces)
            {
                SpawnPiece(parent, pieceData, theme);
            }
            
            Debug.Log($"PieceSpawner: Spawned {levelData.pieces.Count} pieces");
        }
        
        private void SpawnPiece(GameObject parent, PieceData pieceData, ThemeDataSO theme)
        {
            // We DON'T spawn a new piece GameObject!
            // Instead, we find the tile at this position and configure it
            // The tile's GroundObject will handle spawning the visual piece via its Initialize system
            
            Vector3 position = GetGridPosition(pieceData.x, pieceData.z);
            
            // Find tile at this position and configure its GroundObject
            FindAndConfigureTileGroundObject(position, pieceData, theme);
        }
        
        /// <summary>
        /// Find tile's GroundObject at position and configure it for this piece
        /// </summary>
        private void FindAndConfigureTileGroundObject(Vector3 position, PieceData pieceData, ThemeDataSO theme)
        {
            // Find all GroundObjects in scene
            var groundObjects = GameObject.FindObjectsByType<GroundObject>(FindObjectsSortMode.None);
            
            // Find the one at this position (tile)
            GroundObject tileGroundObject = null;
            float minDistance = 0.1f; // Tolerance for position matching
            
            foreach (var go in groundObjects)
            {
                float distance = Vector3.Distance(go.transform.position, position);
                if (distance < minDistance)
                {
                    tileGroundObject = go;
                    break;
                }
            }
            
            if (tileGroundObject == null)
            {
                Debug.LogWarning($"PieceSpawner: No GroundObject (tile) found at position {position} for {pieceData.pieceType}");
                return;
            }
            
            Debug.Log($"PieceSpawner: Found tile GroundObject at {position}, configuring for {pieceData.pieceType}");
            
            // Configure tile's GroundObject based on piece type
            ConfigureTileGroundObject(tileGroundObject.gameObject, pieceData, theme);
        }
        
        private GameObject GetPiecePrefab(PieceType type, ThemeDataSO theme)
        {
            // Use theme's GetPipePrefab method which handles all pipe types
            return theme.GetPipePrefab(type);
        }
        
        /// <summary>
        /// Configure tile's GroundObject for the piece type (tile will spawn its own visual via GroundObject system)
        /// </summary>
        private void ConfigureTileGroundObject(GameObject tileObject, PieceData pieceData, ThemeDataSO theme)
        {
            var groundObject = tileObject.GetComponent<GroundObject>();
            if (groundObject == null)
            {
                Debug.LogError($"PieceSpawner: No GroundObject component on tile at {tileObject.transform.position}");
                return;
            }
            
            // Ensure physics components on tile (only adds if missing)
            EnsurePhysicsComponents(tileObject);
            
            // Ensure SpawnedObjectController on tile (only adds if missing)
            var spawnedController = tileObject.GetComponent<SpawnedObjectController>();
            if (spawnedController == null)
            {
                spawnedController = tileObject.AddComponent<SpawnedObjectController>();
                Debug.Log($"PieceSpawner: Added SpawnedObjectController to tile at {tileObject.transform.position}");
            }
            else
            {
                Debug.Log($"PieceSpawner: SpawnedObjectController already exists on tile at {tileObject.transform.position}");
            }
            
            // Configure based on piece type
            switch (pieceData.pieceType)
            {
                case PieceType.Source:
                    ConfigureGroundObjectAsSource(tileObject, groundObject, spawnedController);
                    break;
                    
                case PieceType.Destination:
                    ConfigureGroundObjectAsDestination(tileObject, groundObject, spawnedController);
                    break;
                    
                default: // Pipes
                    ConfigureGroundObjectAsPipe(tileObject, groundObject, spawnedController, pieceData, theme);
                    break;
            }
        }
        
        /// <summary>
        /// Configure tile's GroundObject as Source and spawn source visual
        /// </summary>
        private void ConfigureGroundObjectAsSource(GameObject tileObject, GroundObject groundObject, 
            SpawnedObjectController spawnedController)
        {
            // Set tag
            tileObject.tag = LevelConstants.SOURCE_TAG;
            
            // Configure SpawnedObjectController
            spawnedController.IsResourceObject = true;
            
            // Spawn source visual manually (GroundObject doesn't handle Source type)
            SpawnSourceVisual(groundObject);
            
            Debug.Log($"PieceSpawner: Configured tile as SOURCE at {tileObject.transform.position}");
        }
        
        /// <summary>
        /// Spawn source visual prefab on tile
        /// </summary>
        private void SpawnSourceVisual(GroundObject groundObject)
        {
            if (groundObject.instantiateTransform == null)
            {
                Debug.LogError("PieceSpawner: No instantiateTransform on GroundObject for Source!");
                return;
            }
            
            // We need to get the source prefab from theme - but we don't have it here!
            // This is a design issue - we'll use GroundObject's ObjectType.A as workaround
            groundObject.SelectedObjectType = ObjectType.A; // Use ObjectType.A for Source
            
            Debug.Log("PieceSpawner: Source visual will be spawned by GroundObject.Start()");
        }
        
        /// <summary>
        /// Configure tile's GroundObject as Destination and spawn destination visual
        /// </summary>
        private void ConfigureGroundObjectAsDestination(GameObject tileObject, GroundObject groundObject,
            SpawnedObjectController spawnedController)
        {
            // Set tag
            tileObject.tag = LevelConstants.DESTINATION_TAG;
            
            // Configure SpawnedObjectController
            spawnedController.IsResourceObject = false;
            
            // Spawn destination visual manually (GroundObject doesn't handle Destination type)
            SpawnDestinationVisual(groundObject);
            
            Debug.Log($"PieceSpawner: Configured tile as DESTINATION at {tileObject.transform.position}");
        }
        
        /// <summary>
        /// Spawn destination visual prefab on tile
        /// </summary>
        private void SpawnDestinationVisual(GroundObject groundObject)
        {
            if (groundObject.instantiateTransform == null)
            {
                Debug.LogError("PieceSpawner: No instantiateTransform on GroundObject for Destination!");
                return;
            }
            
            // Use ObjectType.B for Destination
            groundObject.SelectedObjectType = ObjectType.B; // Use ObjectType.B for Destination
            
            Debug.Log("PieceSpawner: Destination visual will be spawned by GroundObject.Start()");
        }
        
        /// <summary>
        /// Configure tile's GroundObject as Pipe (GroundObject will spawn its own visual)
        /// </summary>
        private void ConfigureGroundObjectAsPipe(GameObject tileObject, GroundObject groundObject,
            SpawnedObjectController spawnedController, PieceData pieceData, ThemeDataSO theme)
        {
            // Set tag
            tileObject.tag = LevelConstants.PIPE_TAG;
            
            // Configure GroundObject - it will spawn the pipe visual via its Initialize system
            groundObject.SelectedObjectType = ObjectType.Pipe;
            
            // Set pipe type (enum, not SO!) - this is used for connection validation
            groundObject.PipeType = pieceData.pieceType;
            
            // Set theme - GroundObject will use this to get pipe prefabs
            groundObject.SetTheme(theme);
            
            // Set initial rotation BEFORE initialization
            if (groundObject.instantiateTransform != null)
            {
                groundObject.instantiateTransform.eulerAngles = new Vector3(0, pieceData.rotation, 0);
            }
            else
            {
                Debug.LogWarning($"PieceSpawner: No instantiateTransform on GroundObject at {tileObject.transform.position}");
            }
            
            // Add ClickableObject for input handling
            AddClickableComponent(tileObject, groundObject);
            
            Debug.Log($"PieceSpawner: Configured tile as PIPE ({pieceData.pieceType}) at {tileObject.transform.position}");
        }
        
        /// <summary>
        /// Ensure physics components (BoxCollider + Rigidbody) exist on tile
        /// </summary>
        private void EnsurePhysicsComponents(GameObject instance)
        {
            // Ensure BoxCollider
            var boxCollider = instance.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = instance.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
                boxCollider.isTrigger = true;
                Debug.Log($"PieceSpawner: Added BoxCollider to {instance.name}");
            }
            else
            {
                // Component already exists, just ensure correct settings
                if (!boxCollider.isTrigger)
                {
                    boxCollider.isTrigger = true;
                    Debug.Log($"PieceSpawner: Set BoxCollider.isTrigger=true on {instance.name}");
                }
            }
            
            // Ensure Rigidbody
            var rigidbody = instance.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                Debug.Log($"PieceSpawner: Added Rigidbody to {instance.name}");
            }
            else
            {
                // Component already exists, just ensure correct settings
                if (!rigidbody.isKinematic || rigidbody.useGravity)
                {
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;
                    Debug.Log($"PieceSpawner: Updated Rigidbody settings on {instance.name}");
                }
            }
        }
        
        /// <summary>
        /// Add ClickableObject component and hook up rotation
        /// </summary>
        private void AddClickableComponent(GameObject instance, GroundObject groundObject)
        {
            // Ensure collider exists for raycast
            EnsureCollider(instance);
            
            // Check if ClickableObject already exists
            var clickable = instance.GetComponent<ClickableObject>();
            if (clickable == null)
            {
                clickable = instance.AddComponent<ClickableObject>();
                Debug.Log($"PieceSpawner: Added ClickableObject to {instance.name}");
            }
            
            // Hook up the rotation event (clear previous and add new)
            clickable.OnClick.RemoveAllListeners();
            clickable.OnClick.AddListener(() => groundObject.RotateObject());
        }
        
        /// <summary>
        /// Ensure object has collider and rigidbody for physics interactions
        /// </summary>
        private void EnsureCollider(GameObject instance)
        {
            // Ensure BoxCollider exists
            var boxCollider = instance.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                // Check if any child has a collider
                var childCollider = instance.GetComponentInChildren<BoxCollider>();
                if (childCollider == null)
                {
                    // Add a box collider
                    boxCollider = instance.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
                    boxCollider.isTrigger = true;
                    Debug.Log($"PieceSpawner: Added BoxCollider to {instance.name}");
                }
            }
            else
            {
                // Component already exists, just ensure correct settings
                if (!boxCollider.isTrigger)
                {
                    boxCollider.isTrigger = true;
                }
            }
            
            // Ensure Rigidbody exists
            var rigidbody = instance.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = instance.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                Debug.Log($"PieceSpawner: Added Rigidbody to {instance.name}");
            }
            else
            {
                // Component already exists, just ensure correct settings
                if (!rigidbody.isKinematic || rigidbody.useGravity)
                {
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;
                }
            }
        }
        
        private Vector3 GetGridPosition(int x, int z)
        {
            return new Vector3(
                x * LevelConstants.X_INTERVAL,
                0,
                z * LevelConstants.Z_INTERVAL
            );
        }
    }
}

