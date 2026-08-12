using System.Collections;
using UnityEngine;

namespace GriefHorror.Systems
{
    public class VoicemailAudioSystem : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("One clip per memory faced, in order. Length should match GameManager's Truths To Face For Ending.")]
        [SerializeField] private AudioClip[] voicemailSegments;

        [Tooltip("Optional short static/tape-hiss stinger played right before each new segment.")]
        [SerializeField] private AudioClip segmentIntroStinger;

        [Header("Timing")]
        [Tooltip("Silence between the stinger and the segment itself.")]
        [SerializeField] private float stingerToSegmentDelay = 0.4f;

        private int lastPlayedSegmentIndex = -1;
        private Coroutine playbackRoutine;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVoicemailProgressed += HandleVoicemailProgressed;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnVoicemailProgressed -= HandleVoicemailProgressed;
            }
        }

        private void HandleVoicemailProgressed(int segmentsUnlocked)
        {
            // segmentsUnlocked is 1-based (1 = first memory faced, etc.)
            int index = segmentsUnlocked - 1;

            if (index < 0 || index >= voicemailSegments.Length) return;
            if (index <= lastPlayedSegmentIndex) return; // already played, avoid re-trigger on reload/replay

            lastPlayedSegmentIndex = index;

            if (playbackRoutine != null)
            {
                StopCoroutine(playbackRoutine);
            }
            playbackRoutine = StartCoroutine(PlaySegment(index));
        }

        private IEnumerator PlaySegment(int index)
        {
            AudioClip clip = voicemailSegments[index];
            bool isFinalSegment = index == voicemailSegments.Length - 1;

            if (segmentIntroStinger != null)
            {
                audioSource.PlayOneShot(segmentIntroStinger);
                yield return new WaitForSeconds(segmentIntroStinger.length + stingerToSegmentDelay);
            }

            if (clip == null)
            {
                Debug.LogWarning($"[VoicemailAudioSystem] Missing clip for segment {index}.");
                yield break;
            }

            audioSource.clip = clip;
            audioSource.Play();

            // Placeholder log — swap for subtitle UI trigger later, matching
            // the rest of the codebase's Debug.Log-as-scaffolding convention.
            Debug.Log(isFinalSegment
                ? "[Voicemail] Final segment playing — the full message."
                : $"[Voicemail] Segment {index + 1}/{voicemailSegments.Length} playing.");

            yield return new WaitForSeconds(clip.length);

            if (isFinalSegment)
            {
                Debug.Log("[Voicemail] Full message heard. Not an accusation.");
            }
        }

        /// <summary>Replays the most recently unlocked segment (e.g. for a "listen again" interaction).</summary>
        public void ReplayLastSegment()
        {
            if (lastPlayedSegmentIndex < 0) return;

            if (playbackRoutine != null)
            {
                StopCoroutine(playbackRoutine);
            }
            playbackRoutine = StartCoroutine(PlaySegment(lastPlayedSegmentIndex));
        }
    }
}
