using UnityEngine;
using UnityEngine.AI;

namespace GriefHorror.Entity
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GriefEntityNavPursuit : MonoBehaviour
    {
        [Header("Speed Scaling")]
        [Tooltip("Agent speed when grief is at 0 — the presence is barely drifting.")]
        [SerializeField] private float minSpeed = 0.5f;

        [Tooltip("Agent speed when grief is at 1 — this is when running truly backfires.")]
        [SerializeField] private float maxSpeed = 4.5f;

        [Tooltip("How grief (0-1) maps to speed. Default is linear; curve lets you make the last stretch spike.")]
        [SerializeField] private AnimationCurve speedByGrief = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Pathing")]
        [Tooltip("How often the destination is recalculated. Cheaper than every frame, still responsive.")]
        [SerializeField] private float repathInterval = 0.25f;

        [Tooltip("Distance at which the presence stops pathing and switches to the embrace behavior.")]
        [SerializeField] private float embraceDistance = 1.2f;

        private NavMeshAgent agent;
        private Transform player;
        private float repathTimer;
        private bool hasEmbraced;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            FindPlayer();
        }

        // Dipisah jadi fungsi biar bisa dipanggil ulang kalau player respawn
        private void FindPlayer() 
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[GriefEntityNavPursuit] No GameObject tagged 'Player' found in scene.");
            }
        }

        private void Update()
        {
            // Kalau player null (misal baru mati/respawn), coba cari lagi
            if (player == null)
            {
                FindPlayer();
                return;
            }

            if (hasEmbraced) return;

            UpdateSpeed();
            UpdatePath();
            CheckEmbrace();
        }

        private void UpdateSpeed()
        {
            float grief = GriefMeter.Instance != null ? GriefMeter.Instance.CurrentGrief : 0f;
            float t = speedByGrief.Evaluate(Mathf.Clamp01(grief));
            agent.speed = Mathf.Lerp(minSpeed, maxSpeed, t);
        }

        private void UpdatePath()
        {
            repathTimer -= Time.deltaTime;
            if (repathTimer > 0f) return;

            repathTimer = repathInterval;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
        }

        private void CheckEmbrace()
        {
            // OPTIMASI: Pakai sqrMagnitude lebih ringan daripada Vector3.Distance
            float sqrDistance = (transform.position - player.position).sqrMagnitude;
            if (sqrDistance <= embraceDistance * embraceDistance)
            {
                hasEmbraced = true;
                agent.isStopped = true;

                // Placeholder — hook into the real OnEmbrace event from
                // GriefEntity/GriefMeter once wired up.
                Debug.Log("[GriefEntityNavPursuit] Embrace reached.");
            }
        }

        public void ResetPursuit()
        {
            hasEmbraced = false;
            repathTimer = 0f; // OPTIMASI: Paksa agen untuk langsung mencari rute baru
            
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }
}