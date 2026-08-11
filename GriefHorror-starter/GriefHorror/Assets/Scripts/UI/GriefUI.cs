using UnityEngine;
using UnityEngine.UI;

namespace GriefHorror.UI
{
    
    public class GriefUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Full-screen Image used as the vignette overlay. Should be black, alpha 0 at rest.")]
        [SerializeField] private Image vignette;

        [Tooltip("Text/label shown when an Interactable is in view range.")]
        [SerializeField] private GameObject interactPrompt;

        [Tooltip("Camera used for the interact raycast. Defaults to Camera.main if left empty.")]
        [SerializeField] private Camera playerCamera;

        [Header("Vignette Tuning")]
        [Tooltip("Max alpha of the vignette at grief = 1.")]
        [SerializeField] [Range(0f, 1f)] private float maxVignetteAlpha = 0.65f;

        [Tooltip("How quickly the vignette eases toward the target alpha (higher = snappier).")]
        [SerializeField] private float vignetteLerpSpeed = 2f;

        [Header("Interact Prompt")]
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactMask = ~0;

        private float currentVignetteAlpha;
        private Transform lastLookedAt;

        private void OnEnable()
        {
            if (GriefMeter.Instance != null)
            {
                GriefMeter.Instance.OnGriefChanged += HandleGriefChanged;
                // Prime the vignette with whatever the current value already is,
                // in case this UI is enabled after grief has already shifted.
                HandleGriefChanged(GriefMeter.Instance.CurrentGrief);
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (GriefMeter.Instance != null)
            {
                GriefMeter.Instance.OnGriefChanged -= HandleGriefChanged;
            }
        }

        private void Update()
        {
            UpdateVignette();
            UpdateInteractPrompt();
        }

        private void HandleGriefChanged(float griefValue)
        {
            // Just cache the target — the actual alpha eases toward it in Update
            // so the screen doesn't flash instantly when grief ticks.
            targetVignetteAlpha = Mathf.Clamp01(griefValue) * maxVignetteAlpha;
        }

        private float targetVignetteAlpha;

        private void UpdateVignette()
        {
            if (vignette == null) return;

            currentVignetteAlpha = Mathf.Lerp(
                currentVignetteAlpha,
                targetVignetteAlpha,
                Time.deltaTime * vignetteLerpSpeed
            );

            Color c = vignette.color;
            c.a = currentVignetteAlpha;
            vignette.color = c;
        }

        private void UpdateInteractPrompt()
        {
            if (interactPrompt == null || playerCamera == null) return;

            bool foundInteractable = false;

            if (Physics.Raycast(
                    playerCamera.transform.position,
                    playerCamera.transform.forward,
                    out RaycastHit hit,
                    interactRange,
                    interactMask))
            {
                var interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null && interactable.CanInteract)
                {
                    foundInteractable = true;
                }
            }

            if (foundInteractable != interactPrompt.activeSelf)
            {
                interactPrompt.SetActive(foundInteractable);
            }
        }
    }
}
