using System;
using UnityEngine;

namespace GriefHorror.Systems
{
    public class GriefMeter : MonoBehaviour
    {
        public static GriefMeter Instance { get; private set; }

        [Header("Tuning")]
        [Tooltip("How fast grief rises per second while the player is fleeing.")]
        [SerializeField] private float fleeRisePerSecond = 0.08f;

        [Tooltip("A small, constant background pressure. Standing still is not enough; the player has to actually face things. Set to 0 while you test movement if the room keeps darkening on its own.")]
        [SerializeField] private float ambientRisePerSecond = 0.005f;

        [Tooltip("How much grief a single confronted memory removes.")]
        [SerializeField] private float reliefPerConfrontation = 0.25f;

        /// <summary>Current grief, always clamped 0..1.</summary>
        public float Grief { get; private set; }

        /// <summary>Fired whenever grief changes. Argument is the new 0..1 value.</summary>
        public event Action<float> OnGriefChanged;

        private bool _isFleeingThisFrame;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            float delta = ambientRisePerSecond * Time.deltaTime;
            if (_isFleeingThisFrame)
                delta += fleeRisePerSecond * Time.deltaTime;

            // Reset each frame; the player controller re-reports fleeing every frame it runs.
            _isFleeingThisFrame = false;

            if (delta != 0f)
                SetGrief(Grief + delta);
        }

        /// <summary>Called by the player controller on every frame the player is running.</summary>
        public void ReportFleeing()
        {
            _isFleeingThisFrame = true;
        }

        /// <summary>Called when the player confronts a memory instead of running from it.</summary>
        public void GrantRelief()
        {
            SetGrief(Grief - reliefPerConfrontation);
        }

        private void SetGrief(float value)
        {
            float clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(clamped, Grief))
                return;

            Grief = clamped;
            OnGriefChanged?.Invoke(Grief);
        }
    }
}
