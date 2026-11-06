using System;
using UnityEngine;
using GriefHorror.Systems;

namespace GriefHorror.Entity
{
    public class GriefEntity : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 0.6f;
        [SerializeField] private float griefSpeedBonus = 2.2f;
        [SerializeField] private float embraceDistance = 1.2f;
        [SerializeField] private float turnSpeed = 5f;

        [Header("After an embrace")]
        [SerializeField] private float embracePause = 3f;
        [Tooltip("Player must get this many times embraceDistance away before it can embrace again.")]
        [SerializeField] private float rearmMultiplier = 2f;

        public event Action OnEmbrace;

        private float _pauseTimer;
        private bool _embraced;
        private bool _warnedNoMeter;

        private void Awake() => TryFindTarget();

        private void TryFindTarget()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        private void Update()
        {
            if (target == null)
            {
                TryFindTarget(); // player may spawn after Awake
                if (target == null) return;
            }

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                return;
            }

            Vector3 targetOnPlane = new Vector3(target.position.x, transform.position.y, target.position.z);
            float planarDist = Vector3.Distance(transform.position, targetOnPlane);

            // One embrace per approach: wait for the player to move away before re-arming.
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
                grief = Mathf.Clamp01(GriefMeter.Instance.Grief); // assumes 0..1; divide first if yours is 0..100
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