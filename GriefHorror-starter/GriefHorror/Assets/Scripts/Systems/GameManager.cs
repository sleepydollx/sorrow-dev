using System;
using UnityEngine;

namespace GriefHorror.Systems
{
    /// <summary>
    /// Tracks the one thread that ties the whole game together: the voicemail
    /// the player has never been able to listen to all the way through.
    ///
    /// Each memory the player confronts unlocks one more piece of it. When every
    /// truth has been faced, the message can finally be heard in full — and it
    /// turns out not to be an accusation at all. This manager just counts the
    /// progress and announces milestones; the actual audio, subtitles, and
    /// ending scene hook onto its events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Story")]
        [Tooltip("How many memories must be confronted before the voicemail is whole.")]
        [SerializeField] private int truthsToFaceForEnding = 5;

        /// <summary>Number of memories confronted so far.</summary>
        public int TruthsFaced { get; private set; }

        /// <summary>Voicemail completeness, 0..1.</summary>
        public float VoicemailProgress =>
            truthsToFaceForEnding <= 0 ? 1f : Mathf.Clamp01((float)TruthsFaced / truthsToFaceForEnding);

        /// <summary>Fired each time a truth is faced. Argument is the new voicemail progress 0..1.</summary>
        public event Action<float> OnVoicemailProgressed;

        /// <summary>Fired once, when the last truth is faced and the message is finally whole.</summary>
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

        /// <summary>Called by a MemoryObject when the player confronts it.</summary>
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
