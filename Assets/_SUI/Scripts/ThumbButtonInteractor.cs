using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class ThumbButtonInteractor : ButtonInteractorBase
{
    [Header("Thumb Button Settings")]
    public float rotationSpeed = 2.0f;
    public int numberOfPositions = 3;
    
    [Header("References")]
    public Transform rotationPoint; // The point around which the cylinder rotates
    
    [Header("Menu Items")]
    [Tooltip("The text names for each menu position")]
    public string[] menuItemNames = new string[] { "Item 1", "Item 2", "Item 3" };
    
    [Header("UI References")]
    [Tooltip("TextMeshPro component to display the current selection")]
    public TextMeshPro displayText;
    public Color textHighlightColor = Color.yellow;
    public Color textNormalColor = Color.white;
    
    // Rotation settings
    private float minRotation = 0f;
    private float maxRotation = 180f;
    private float[] snapPositions;
    private int currentPosition = 0;
    private int previousPosition = 0;
    
    // Interaction state
    private bool isDragging = false;
    private float dragStartY;
    private float startRotationX;
    private float targetRotation;
    private float currentRotationX;
    
    protected override void Start()
    {
        base.Start();
        
        // Initialize the cylinder
        if (rotationPoint == null)
        {
            rotationPoint = transform;
            Debug.LogWarning("No rotation point assigned, using this transform instead.");
        }
        
        // Set initial rotation
        currentRotationX = transform.localRotation.eulerAngles.x;
        SnapToPosition(0, false);
        
        // Update text display
        UpdateDisplayText();
    }
    
    protected override void InitializeButton()
    {
        // Calculate snap positions based on the number of positions
        InitializeSnapPositions();
        
        // If menu items array doesn't match the number of positions, resize it
        if (menuItemNames.Length != numberOfPositions)
        {
            string[] oldNames = menuItemNames;
            menuItemNames = new string[numberOfPositions];
            
            // Copy existing names
            for (int i = 0; i < numberOfPositions; i++)
            {
                if (i < oldNames.Length)
                {
                    menuItemNames[i] = oldNames[i];
                }
                else
                {
                    menuItemNames[i] = $"Item {i+1}";
                }
            }
            
            Debug.LogWarning($"Menu item names array adjusted to match numberOfPositions ({numberOfPositions})");
        }
    }
    
    void InitializeSnapPositions()
    {
        snapPositions = new float[numberOfPositions];
        float step = (maxRotation - minRotation) / (numberOfPositions - 1);
        
        // Reverse the order of snap positions so higher rotations correspond to lower positions
        for (int i = 0; i < numberOfPositions; i++)
        {
            // This reverses the mapping - higher rotation value = lower menu position
            snapPositions[i] = maxRotation - (step * i);
        }
    }
    
    protected override bool CheckRaycastHit(RaycastHit hit)
    {
        return hit.transform == transform;
    }
    
    protected override void OnLeftClickPressed(Vector2 mousePosition)
    {
        isDragging = true;
        dragStartY = mousePosition.y;
        startRotationX = currentRotationX;
        
        // Stop any ongoing animations (except press animation)
        if (animationCoroutine != null && !isPressed)
        {
            StopAnimation();
        }
    }
    
    protected override void OnLeftClickDrag(Vector2 mousePosition)
    {
        if (isDragging)
        {
            // Calculate screen delta from start position
            float dragDeltaY = mousePosition.y - dragStartY;
            
            // Convert screen movement to rotation (scaled by sensitivity)
            float rotationDelta = dragDeltaY * rotationSpeed;
            float newRotation = Mathf.Clamp(startRotationX + rotationDelta, minRotation, maxRotation);
            
            // Apply rotation
            currentRotationX = newRotation;
            transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
            
            // Find the nearest position during dragging for dynamic updates
            int nearestPos = FindNearestPosition(currentRotationX);
            
            // Check if the nearest position has changed while dragging
            if (nearestPos != currentPosition)
            {
                // Record previous position
                previousPosition = currentPosition;
                // Update current position
                currentPosition = nearestPos;
                
                // Play sound when changing positions
                PlayInteractionSound();
                
                // Update text display immediately
                UpdateDisplayText();
                
                Debug.Log($"Dragging to position: {currentPosition} - {GetCurrentItemName()}");
            }
        }
    }
    
    protected override void OnLeftClickReleased(Vector2 mousePosition)
    {
        if (isDragging)
        {
            isDragging = false;
            int nearestPosition = FindNearestPosition(currentRotationX);
            SnapToPosition(nearestPosition, true);
        }
    }
    
    protected override void OnRightClickPressed(Vector2 mousePosition)
    {
        Debug.Log($"Right-clicked on position: {currentPosition} - {GetCurrentItemName()}");
    }
    
    // We don't need to override HandleInput anymore since the base class handles it
    
    int FindNearestPosition(float currentRotation)
    {
        int nearest = 0;
        float minDistance = float.MaxValue;
        
        for (int i = 0; i < snapPositions.Length; i++)
        {
            float distance = Mathf.Abs(currentRotation - snapPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = i;
            }
        }
        
        return nearest;
    }
    
    void SnapToPosition(int position, bool animate)
    {
        position = Mathf.Clamp(position, 0, snapPositions.Length - 1);
        targetRotation = snapPositions[position];
        
        // Record the previous position to detect changes
        previousPosition = currentPosition;
        
        // Only update current position if it changed (we might already be at this position from dragging)
        bool positionChanged = (currentPosition != position);
        currentPosition = position;
        
        if (animate)
        {
            StopAnimation();
            animationCoroutine = StartCoroutine(AnimateRotation());
        }
        else
        {
            // Immediately set rotation
            currentRotationX = targetRotation;
            transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
        }
        
        // Check if position changed and update UI
        if (positionChanged)
        {
            PlayInteractionSound();
            Debug.Log($"Snapped to position: {currentPosition} - {GetCurrentItemName()}");
            
            // Update text display
            UpdateDisplayText();
        }
    }
    
    IEnumerator AnimateRotation()
    {
        // Use the base class's animation coroutine
        float startRotation = currentRotationX;
        
        return AnimateCoroutine(
            // Update action
            (t) => {
                currentRotationX = Mathf.Lerp(startRotation, targetRotation, t);
                transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
            }
        );
    }
    
    // New method to update the TextMeshPro display
    void UpdateDisplayText()
    {
        if (displayText != null)
        {
            // Create the displayed text
            string displayString = "";
            
            // Add each menu item in normal order (top to bottom)
            for (int i = 0; i < menuItemNames.Length; i++)
            {
                // Highlight the selected item with color
                if (i == currentPosition)
                {
                    displayString += $"<color=#{ColorUtility.ToHtmlStringRGB(textHighlightColor)}>{menuItemNames[i]}</color>";
                }
                else
                {
                    displayString += $"<color=#{ColorUtility.ToHtmlStringRGB(textNormalColor)}>{menuItemNames[i]}</color>";
                }
                
                // Add line break if not the last item
                if (i < menuItemNames.Length - 1)
                {
                    displayString += "\n";
                }
            }
            
            // Set the text
            displayText.text = displayString;
        }
    }
    
    // Get the name of the currently selected item
    public string GetCurrentItemName()
    {
        if (currentPosition >= 0 && currentPosition < menuItemNames.Length)
        {
            // Using the position directly (we've already reversed the rotation mapping)
            return menuItemNames[currentPosition];
        }
        return "Unknown";
    }
    
    // Public methods to access the current selection
    public int GetCurrentPosition()
    {
        return currentPosition;
    }
    
    // Programmatically set the position
    public void SetPosition(int position, bool animate = true)
    {
        SnapToPosition(position, animate);
    }
    
    // Add a public method to set menu item names
    public void SetMenuItemName(int index, string name)
    {
        if (index >= 0 && index < menuItemNames.Length)
        {
            menuItemNames[index] = name;
            UpdateDisplayText();
        }
    }
}
