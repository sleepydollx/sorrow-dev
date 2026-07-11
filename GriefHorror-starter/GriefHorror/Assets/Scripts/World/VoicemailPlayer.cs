using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GriefHorror.UI;

namespace GriefHorror.World
{
    [RequireComponent(typeof(AudioSource))]
    public class VoicemailPlayer : MonoBehaviour
    {
        [Serializable]
        public class SubtitleLine
        {
            [Tooltip("Seconds into the clip when this line appears.")]
            public float atTime;

            [TextArea(2, 4)]
            public string text;

            [Tooltip("How long the line stays on screen.")]
            public float duration = 4f;
        }

        [Header("Voicemail")]
        [SerializeField] private AudioClip clip;
        [SerializeField] private List<SubtitleLine> lines = new List<SubtitleLine>();

        [Header("Behaviour")]
        [Tooltip("If true, the voicemail can only be completed once.")]
        [SerializeField] private bool playOnlyOnce = true;

        [Tooltip("If true, interrupting (Stop / player leaving) resets progress — grief must be faced whole.")]
        [SerializeField] private bool mustHearToEnd = true;

        [Tooltip("Optional: beep played before the message, like a real machine.")]
        [SerializeField] private AudioClip beepClip;

        [Header("Events")]
        [Tooltip("Fires when the voicemail finishes playing to the end. Wire GameManager progress here.")]
        public UnityEvent onFinished;

        [Tooltip("Fires when playback is interrupted before the end.")]
        public UnityEvent onInterrupted;

        public bool IsPlaying { get; private set; }
        public bool HasBeenHeard { get; private set; }

        private AudioSource _source;
        private Coroutine _playRoutine;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 1f; // 3D by default — the voice lives in the room
        }

        // ---------- Public API ----------

        /// <summary>Start the voicemail. Safe to call from an Interactable's UnityEvent.</summary>
        public void Play()
        {
            if (IsPlaying)
                return;

            if (playOnlyOnce && HasBeenHeard)
            {
                // Already faced. A quiet acknowledgment instead of replaying.
                if (GameHUD.Instance != null)
                    GameHUD.Instance.ShowSubtitle("No new messages.", 2.5f);
                return;
            }

            if (clip == null)
            {
                Debug.LogWarning($"{name}: VoicemailPlayer has no clip assigned.", this);
                return;
            }

            _playRoutine = StartCoroutine(PlayRoutine());
        }

        /// <summary>Interrupt playback (e.g. player ran, or a chase started).</summary>
        public void Stop()
        {
            if (!IsPlaying)
                return;

            StopCoroutine(_playRoutine);
            _source.Stop();
            IsPlaying = false;

            if (!mustHearToEnd)
                HasBeenHeard = true;

            onInterrupted?.Invoke();
        }

        // ---------- Internals ----------

        private IEnumerator PlayRoutine()
        {
            IsPlaying = true;

            if (beepClip != null)
            {
                _source.PlayOneShot(beepClip);
                yield return new WaitForSeconds(beepClip.length);
            }

            _source.clip = clip;
            _source.Play();

            float elapsed = 0f;
            int nextLine = 0;

            while (elapsed < clip.length)
            {
                // Fire any subtitle lines whose time has come.
                while (nextLine < lines.Count && lines[nextLine].atTime <= elapsed)
                {
                    if (GameHUD.Instance != null)
                        GameHUD.Instance.ShowSubtitle(lines[nextLine].text, lines[nextLine].duration);
                    nextLine++;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            IsPlaying = false;
            HasBeenHeard = true;
            _playRoutine = null;

            // The truth was faced. Report it — wire GameManager here in the
            // Inspector (onFinished), or call it directly, e.g.:
            // GameManager.Instance.RegisterTruthFaced();
            onFinished?.Invoke();
        }
    }
}
