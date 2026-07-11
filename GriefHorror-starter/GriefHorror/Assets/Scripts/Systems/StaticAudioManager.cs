using UnityEngine;

namespace GriefHorror.Systems
{
    public class StaticAudioManager : MonoBehaviour
    {
        public static StaticAudioManager Instance { get; private set; }

        [Header("Clips (looping)")]
        [Tooltip("High hiss, like snow on an old TV. The main voice of the static.")]
        [SerializeField] private AudioClip hissLoop;
        [Tooltip("Low rumble/drone underneath. Optional but recommended.")]
        [SerializeField] private AudioClip rumbleLoop;

        [Header("Levels")]
        [Tooltip("Hiss volume at grief = 0. A faint bed so silence never feels safe.")]
        [SerializeField, Range(0f, 1f)] private float hissFloor = 0.03f;
        [Tooltip("Hiss volume at grief = 1.")]
        [SerializeField, Range(0f, 1f)] private float hissCeiling = 0.65f;
        [Tooltip("Rumble volume at grief = 1 (it fades in only past mid grief).")]
        [SerializeField, Range(0f, 1f)] private float rumbleCeiling = 0.5f;

        [Header("Feel")]
        [Tooltip("Seconds for volume to chase the target. Slow = dread, fast = panic.")]
        [SerializeField] private float smoothTime = 2.5f;
        [Tooltip("At high grief the hiss pitch wavers slightly, like a signal failing.")]
        [SerializeField, Range(0f, 0.2f)] private float pitchWobble = 0.06f;

        public float CurrentGrief { get; private set; }

        private AudioSource _hiss;
        private AudioSource _rumble;

        private float _hissVel, _rumbleVel;
        private float _duckMultiplier = 1f;
        private float _duckTarget = 1f;
        private float _duckSpeed = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _hiss = CreateLoop("HissLoop", hissLoop);
            _rumble = CreateLoop("RumbleLoop", rumbleLoop);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            // Duck multiplier eases toward its target (unscaled: keeps moving
            // even if something pauses timescale without pausing audio).
            _duckMultiplier = Mathf.MoveTowards(
                _duckMultiplier, _duckTarget, _duckSpeed * Time.unscaledDeltaTime);

            float hissTarget = Mathf.Lerp(hissFloor, hissCeiling, CurrentGrief) * _duckMultiplier;

            // Rumble only wakes up past mid grief — the dread has a second stage.
            float rumbleAmount = Mathf.InverseLerp(0.5f, 1f, CurrentGrief);
            float rumbleTarget = rumbleCeiling * rumbleAmount * _duckMultiplier;

            if (_hiss != null)
            {
                _hiss.volume = Mathf.SmoothDamp(_hiss.volume, hissTarget, ref _hissVel, smoothTime);

                // Signal-failing wobble, only audible when grief is high.
                float wobble = Mathf.Sin(Time.unscaledTime * 1.7f) * pitchWobble * CurrentGrief;
                _hiss.pitch = 1f + wobble;
            }

            if (_rumble != null)
                _rumble.volume = Mathf.SmoothDamp(_rumble.volume, rumbleTarget, ref _rumbleVel, smoothTime);
        }

        // ---------- Public API ----------

        /// <summary>Feed the current grief value (0..1). Call from GriefMeter.</summary>
        public void SetGrief(float normalized)
        {
            CurrentGrief = Mathf.Clamp01(normalized);
        }
        public void Duck(float toFraction = 0.15f, float overSeconds = 1.5f)
        {
            _duckTarget = Mathf.Clamp01(toFraction);
            _duckSpeed = Mathf.Abs(_duckMultiplier - _duckTarget) / Mathf.Max(overSeconds, 0.01f);
        }

        /// <summary>Bring the static back to its grief-driven level.</summary>
        public void Restore(float overSeconds = 2.5f)
        {
            _duckTarget = 1f;
            _duckSpeed = Mathf.Abs(_duckMultiplier - _duckTarget) / Mathf.Max(overSeconds, 0.01f);
        }

        // ---------- Internals ----------

        private AudioSource CreateLoop(string name, AudioClip clip)
        {
            if (clip == null)
                return null;

            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;   // the static is not in the room — it's in your head
            source.volume = 0f;
            source.Play();
            return source;
        }
    }
}
