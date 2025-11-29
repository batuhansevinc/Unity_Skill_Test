using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    public UnityEvent OnClick;
    
    [SerializeField] private float clickCooldown = 0.5f;
    private float lastClickTime = -999f;
    private bool isProcessing = false;

    public void OnObjectClicked()
    {
        // Prevent duplicate clicks during cooldown
        if (isProcessing)
        {
            Debug.Log($"ClickableObject: Click ignored - already processing on {gameObject.name}");
            return;
        }
        
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick < clickCooldown)
        {
            Debug.Log($"ClickableObject: Click ignored - cooldown active ({timeSinceLastClick:F2}s < {clickCooldown}s) on {gameObject.name}");
            return;
        }
        
        // Process click
        lastClickTime = Time.time;
        isProcessing = true;
        
        Debug.Log($"ClickableObject: Click accepted on {gameObject.name}");
        OnClick.Invoke();
        
        // Reset processing flag after cooldown
        Invoke(nameof(ResetProcessing), clickCooldown);
    }
    
    private void ResetProcessing()
    {
        isProcessing = false;
    }
}