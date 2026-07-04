using System;
using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.World
{
    /// <summary>
    /// A single memory the player is meant to face instead of avoid: a photo,
    /// a half-painted wall, the second coffee cup, the unfinished suitcase.
    ///
    /// Confronting it is the counter-move to running. It eases the grief and
    /// advances the story (one more piece of the voicemail becomes hearable).
    /// A memory can only be confronted once.
    /// </summary>
    public class MemoryObject : Interactable
    {
        [Header("Memory")]
        [TextArea]
        [Tooltip("The line or beat this memory holds. Shown / spoken when confronted.")]
        [SerializeField] private string memoryLine = "You left this half-finished. You keep meaning to come back to it.";

        [Tooltip("If true, confronting this memory advances the voicemail / story by one step.")]
        [SerializeField] private bool advancesStory = true;

        public bool Confronted { get; private set; }

        /// <summary>Fired when this memory is confronted for the first time. Hook visuals/audio here.</summary>
        public event Action OnConfronted;

        public override void Interact()
        {
            if (Confronted)
                return;

            Confronted = true;

            // Facing it, instead of fleeing, is what heals.
            if (GriefMeter.Instance != null)
                GriefMeter.Instance.GrantRelief();

            if (advancesStory && GameManager.Instance != null)
                GameManager.Instance.RegisterTruthFaced();

            // For now, the memory speaks to the console. Replace with subtitle UI / voice audio.
            Debug.Log($"[Memory] {memoryLine}");

            OnConfronted?.Invoke();
        }
    }
}
