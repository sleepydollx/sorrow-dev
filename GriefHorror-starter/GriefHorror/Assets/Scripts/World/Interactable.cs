using UnityEngine;

namespace GriefHorror.World
{
    /// <summary>
    /// Base class for anything the player can look at and press E on: a photo,
    /// a coffee cup, a door, the focal object of a memory. Put a collider on the
    /// same object (or a child) so the interaction raycast can hit it.
    ///
    /// Subclass it and override Interact() to define what happens.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Short line shown to the player when looking at this object, e.g. \"Two cups. You only need one now.\"")]
        [SerializeField] protected string prompt = "Look";

        public string Prompt => prompt;

        /// <summary>Called when the player interacts with this object.</summary>
        public abstract void Interact();
    }
}
