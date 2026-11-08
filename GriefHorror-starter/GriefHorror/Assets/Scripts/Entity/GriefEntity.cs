using System;
using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.Entity
{
    public class GriefEntity : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The player to approach. If left empty, the object tagged 'Player' is used.")]
        [SerializeField] private Transform target;

        [Header("Movement")]
        [Tooltip("Base approach speed at zero grief.")]
        [SerializeField] private float baseSpeed = 0.6f;
        [Tooltip("Extra speed added at maximum grief. Higher grief, faster approach.")]
        [SerializeField] private float griefSpeedBonus = 2.2f;
        [Tooltip("How close counts as reaching the player.")]
        [SerializeField] private float embraceDistance = 1.2f;
        [SerializeField] private float turnSpeed = 5f;

        [Header("After an embrace")]
        [Tooltip("Seconds the presence waits before approaching again.")]
        [SerializeField] private float embracePause = 3f;
        [Tooltip("Player must get this many times embraceDistance away before it can embrace again.")]
        [SerializeField] private float rearmMultiplier = 2f;

        /// Fired when the presence reaches the player. Hook the memory beat here.</summary>
        public event Action OnEmbrace;

        private float _pauseTimer;
        private bool _embraced;
        private bool _warnedNoMeter;

        private void Awake()
        {
            TryFindTarget();
        }

        private void TryFindTarget()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        private void Update()
        {
            // Player may spawn after Awake, so keep looking until found.
            if (target == null)
            {
                TryFindTarget();
                if (target == null)
                    return;
            }

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                return;
            }

            Vector3 targetOnPlane = new Vector3(target.position.x, transform.position.y, target.position.z);
            float planarDist = Vector3.Distance(transform.position, targetOnPlane);

            // One embrace per approach: wait until the player moves away before re-arming.
            if (_embraced)
            {
                if (planarDist > embraceDistance * rearmMultiplier)
                    _embraced = false;
                else
                    return;
            }

            if (planarDist <= embraceDistance)
            {
                Embrace();
                return;
            }

            float grief = 0f;
            if (GriefMeter.Instance != null)
            {
                grief = Mathf.Clamp01(GriefMeter.Instance.Grief); // assumes 0..1
            }
            else if (!_warnedNoMeter)
            {
                Debug.LogWarning("[GriefEntity] GriefMeter.Instance is null; using 0 grief.", this);
                _warnedNoMeter = true;
            }

            float speed = baseSpeed + griefSpeedBonus * grief;
            transform.position = Vector3.MoveTowards(transform.position, targetOnPlane, speed * Time.deltaTime);

            Vector3 look = targetOnPlane - transform.position;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), turnSpeed * Time.deltaTime);
        }

        private void Embrace()
        {
            _embraced = true;
            _pauseTimer = embracePause;
            Debug.Log("[Presence] It reaches you. An open hand, not a blow.");
            OnEmbrace?.Invoke();
        }
    }
}