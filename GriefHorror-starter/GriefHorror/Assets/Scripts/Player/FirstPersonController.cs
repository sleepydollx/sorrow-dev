using UnityEngine;
using GriefHorror.Systems;
using GriefHorror.World;

namespace GriefHorror.Player
{
    /// <summary>
    /// Basic first-person movement, mouse look, and interaction.
    ///
    /// Holding the run key increases speed but reports to the GriefMeter, which
    /// is the whole point: fleeing is fast and feels safe, and it is exactly
    /// what makes the game worse. Later you can add stamina, headbob, footstep
    /// audio, and a proper camera rig, but this is enough to walk the house and
    /// test the core loop.
    ///
    /// Uses Unity's built-in Input Manager (Input.GetAxis) so it runs with no
    /// extra package setup. If you move to the new Input System, this is the
    /// script to rewrite.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float runSpeed = 4.6f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look")]
        [Tooltip("The player camera, usually a child of this object at roughly eye height (y ~ 1.6).")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 85f;

        [Header("Interaction")]
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private CharacterController _controller;
        private float _verticalVelocity;
        private float _pitch;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleInteraction();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            _pitch = Mathf.Clamp(_pitch - mouseY, -maxLookAngle, maxLookAngle);
            if (cameraTransform != null)
                cameraTransform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            bool wantsToRun = Input.GetKey(KeyCode.LeftShift);
            bool isMoving = new Vector2(h, v).sqrMagnitude > 0.01f;
            bool isFleeing = wantsToRun && isMoving;

            float speed = isFleeing ? runSpeed : walkSpeed;

            // Running away is what feeds the grief.
            if (isFleeing && GriefMeter.Instance != null)
                GriefMeter.Instance.ReportFleeing();

            Vector3 move = (transform.right * h + transform.forward * v) * speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _controller.Move(move * Time.deltaTime);
        }

        private void HandleInteraction()
        {
            if (!Input.GetKeyDown(interactKey) || cameraTransform == null)
                return;

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                var interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }
    }
}
