using System;
using UnityEngine;

namespace GriefHorror.Systems
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Story")]
        [Tooltip("How many memories must be confronted before the voicemail is whole.")]
        [SerializeField] private int truthsToFaceForEnding = 5;

        public int TruthsFaced { get; private set; }

        public float VoicemailProgress =>
            truthsToFaceForEnding <= 0 ? 1f : Mathf.Clamp01((float)TruthsFaced / truthsToFaceForEnding);

        /// Fired each time a truth is faced. Argument is the new voicemail progress 0..1.
        public event Action<float> OnVoicemailProgressed;

        /// Fired once, when the last truth is faced and the message is finally whole.
        public event Action OnStoryComplete;

        private bool _completed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// Called by a MemoryObject when the player confronts it.
        public void RegisterTruthFaced()
        {
            if (_completed)
                return;

            TruthsFaced++;
            Debug.Log($"[Voicemail] Another few seconds become hearable. Progress: {VoicemailProgress:P0}");
            OnVoicemailProgressed?.Invoke(VoicemailProgress);

            if (TruthsFaced >= truthsToFaceForEnding)
            {
                _completed = true;
                Debug.Log("[Voicemail] It plays all the way through now. It was never an accusation.");
                OnStoryComplete?.Invoke();
            }
        }
    }
}
