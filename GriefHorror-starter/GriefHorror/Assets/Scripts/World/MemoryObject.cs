using System;
using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.World
{
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

            Debug.Log($"[Memory] {memoryLine}");

            OnConfronted?.Invoke();
        }
    }
}
