using UnityEngine;
using BufoGames.Pieces;

/// <summary>
/// Handles mouse/touch input for piece rotation
/// Directly calls Rotate() on PipeController, SourceController, or DestinationController
/// </summary>
public class InputManager : MonoBehaviour
{
    private Camera _mainCamera;
    private bool _inputEnabled = true;

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (!_inputEnabled) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                
                // Try to find rotatable piece on hit object or parent
                if (TryRotatePiece(hitObject)) return;
                
                if (hitObject.transform.parent != null)
                {
                    TryRotatePiece(hitObject.transform.parent.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Try to rotate any rotatable piece (Pipe, Source, or Destination)
    /// </summary>
    private bool TryRotatePiece(GameObject obj)
    {
        // Try PipeController
        PipeController pipe = obj.GetComponent<PipeController>();
        if (pipe != null)
        {
            pipe.Rotate();
            return true;
        }
        
        // Try SourceController
        SourceController source = obj.GetComponent<SourceController>();
        if (source != null)
        {
            source.Rotate();
            return true;
        }
        
        // Try DestinationController
        DestinationController dest = obj.GetComponent<DestinationController>();
        if (dest != null)
        {
            dest.Rotate();
            return true;
        }
        
        return false;
    }
}