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

        [Header("After an embrace")]
        [Tooltip("Seconds the presence waits before approaching again.")]
        [SerializeField] private float embracePause = 3f;

        /// <summary>Fired when the presence reaches the player. Hook the memory beat here.</summary>
        public event Action OnEmbrace;

        private float _pauseTimer;

        private void Awake()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
        }

        private void Update()
        {
            if (target == null)
                return;

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                return;
            }

            float grief = GriefMeter.Instance != null ? GriefMeter.Instance.Grief : 0f;
            float speed = baseSpeed + griefSpeedBonus * grief;

            Vector3 targetOnPlane = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetOnPlane, speed * Time.deltaTime);

            Vector3 look = targetOnPlane - transform.position;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 5f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetOnPlane) <= embraceDistance)
                Embrace();
        }

        private void Embrace()
        {
            Debug.Log("[Presence] It reaches you. An open hand, not a blow.");
            OnEmbrace?.Invoke();
            _pauseTimer = embracePause;
        }
    }
}
