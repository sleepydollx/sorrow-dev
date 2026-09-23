using UnityEngine;

namespace GriefHorror.World
{
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Short line shown to the player when looking at this object, e.g. \"Two cups. You only need one now.\"")]
        [SerializeField] protected string prompt = "Look";

        public string Prompt => prompt;

        /// <summary>Called when the player interacts with this object.</summary>
        public abstract void Interact();
    }
}
