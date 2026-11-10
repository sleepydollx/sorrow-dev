using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Hiding Coordinates")]
    public Transform hidePosition;
    public Transform exitPosition;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    // Static boolean so GriefEntityNavPursuit.cs can easily read this state globally
    public static bool isPlayerHiding = false;

    private bool isPlayerInRange = false;
    private bool isCurrentlyHiding = false;
    
    private GameObject playerObject;
    private CharacterController playerController;

    private void Update()
    {
        // Listen for interaction key only when the player is in range
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleHideState();
        }
    }

    private void ToggleHideState()
    {
        isCurrentlyHiding = !isCurrentlyHiding;
        isPlayerHiding = isCurrentlyHiding; // Update global state for the enemy AI

        if (isCurrentlyHiding)
        {
            // Disable CharacterController to prevent teleportation bugs
            if (playerController != null) playerController.enabled = false;
            
            // Snap player to the inside of the hiding spot
            playerObject.transform.position = hidePosition.position;
            playerObject.transform.rotation = hidePosition.rotation;
            
            Debug.Log("[HIDING] Player entered hiding spot.");
        }
        else
        {
            // Snap player back outside
            playerObject.transform.position = exitPosition.position;
            playerObject.transform.rotation = exitPosition.rotation;

            // Re-enable CharacterController so the player can walk again
            if (playerController != null) playerController.enabled = true;
            
            Debug.Log("[HIDING] Player exited hiding spot.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isCurrentlyHiding)
        {
            isPlayerInRange = true;
            playerObject = other.gameObject;
            
            // Cache the controller component
            playerController = playerObject.GetComponent<CharacterController>();
            
            Debug.Log("[HIDING] Press 'E' to hide.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }
}