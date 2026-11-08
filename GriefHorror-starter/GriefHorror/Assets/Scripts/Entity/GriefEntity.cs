using System;
using UnityEngine;
using UnityEngine.AI;
using GriefHorror.Systems;

namespace GriefHorror.Entity
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GriefEntity : MonoBehaviour
    {
        private enum State { Idle, Chase, Investigate, Search }

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 0.6f;
        [SerializeField] private float griefSpeedBonus = 2.2f;
        [Range(0.1f, 1f)]
        [SerializeField] private float searchSpeedMultiplier = 0.6f;
        [SerializeField] private float embraceDistance = 1.2f;
        [SerializeField] private float repathInterval = 0.25f;

        [Header("Sight")]
        [SerializeField] private float sightRange = 15f;
        [SerializeField] private float fieldOfView = 120f;
        [SerializeField] private float eyeHeight = 1.6f;
        [Tooltip("Layers that can block line of sight (walls, doors...).")]
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("Memory")]
        [Tooltip("Seconds it remembers the last known position at zero grief.")]
        [SerializeField] private float minMemoryTime = 6f;
        [Tooltip("Seconds it remembers at maximum grief.")]
        [SerializeField] private float maxMemoryTime = 20f;

        [Header("Search")]
        [SerializeField] private int searchPoints = 3;
        [SerializeField] private float searchRadius = 6f;
        [SerializeField] private float searchWait = 2f;

        [Header("After an embrace")]
        [SerializeField] private float embracePause = 3f;
        [SerializeField] private float rearmMultiplier = 2f;

        public event Action OnEmbrace;

        private NavMeshAgent _agent;
        private State _state = State.Idle;

        // Memory
        private bool _hasMemory;
        private Vector3 _lastKnownPos;
        private float _lastSeenTime;

        private float _pauseTimer;
        private float _repathTimer;
        private float _waitTimer;
        private int _searchesLeft;
        private bool _embraced;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = embraceDistance * 0.9f;
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
            if (target == null)
            {
                TryFindTarget();
                if (target == null) return;
            }

            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                return;
            }

            float dist = Vector3.Distance(transform.position, target.position);

            if (_embraced)
            {
                if (dist > embraceDistance * rearmMultiplier)
                {
                    _embraced = false;
                    _agent.isStopped = false;
                }
                else return;
            }

            float grief = GriefMeter.Instance != null ? Mathf.Clamp01(GriefMeter.Instance.Grief) : 0f;
            float fullSpeed = baseSpeed + griefSpeedBonus * grief;

            bool sees = CanSeeTarget();

            // Update memory
            if (sees)
            {
                _hasMemory = true;
                _lastKnownPos = target.position;
                _lastSeenTime = Time.time;
                _state = State.Chase;
            }
            else if (_state == State.Chase)
            {
                _state = State.Investigate; // lost sight: go to the last known spot
                _agent.SetDestination(_lastKnownPos);
            }

            // Forget after a while (longer when grief is high)
            if (_hasMemory && !sees)
            {
                float memoryTime = Mathf.Lerp(minMemoryTime, maxMemoryTime, grief);
                if (Time.time - _lastSeenTime > memoryTime)
                {
                    Forget();
                    return;
                }
            }

            switch (_state)
            {
                case State.Chase:
                    if (dist <= embraceDistance)
                    {
                        Embrace();
                        return;
                    }
                    _agent.speed = fullSpeed;
                    _repathTimer -= Time.deltaTime;
                    if (_repathTimer <= 0f)
                    {
                        _repathTimer = repathInterval;
                        _agent.SetDestination(target.position);
                    }
                    break;

                case State.Investigate:
                    _agent.speed = fullSpeed * searchSpeedMultiplier;
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.3f)
                    {
                        _state = State.Search;
                        _searchesLeft = searchPoints;
                        _waitTimer = searchWait;
                    }
                    break;

                case State.Search:
                    _agent.speed = fullSpeed * searchSpeedMultiplier;
                    TickSearch();
                    break;

                case State.Idle:
                    break;
            }
        }

        private void TickSearch()
        {
            if (_agent.pathPending) return;
            if (_agent.remainingDistance > _agent.stoppingDistance + 0.3f) return; // still walking

            _waitTimer -= Time.deltaTime; // arrived: look around
            if (_waitTimer > 0f) return;

            if (_searchesLeft <= 0)
            {
                Forget();
                return;
            }

            _searchesLeft--;
            _waitTimer = searchWait;

            Vector2 offset = UnityEngine.Random.insideUnitCircle * searchRadius;
            Vector3 candidate = _lastKnownPos + new Vector3(offset.x, 0f, offset.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }

        private bool CanSeeTarget()
        {
            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 aim = target.position + Vector3.up * 1f;
            Vector3 toTarget = aim - eye;

            if (toTarget.magnitude > sightRange) return false;
            if (Vector3.Angle(transform.forward, toTarget) > fieldOfView * 0.5f) return false;

            if (Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, sightRange,
                    obstructionMask, QueryTriggerInteraction.Ignore))
            {
                return hit.transform == target || hit.transform.IsChildOf(target);
            }
            return false;
        }

        private void Forget()
        {
            _hasMemory = false;
            _state = State.Idle;
            if (_agent.isOnNavMesh) _agent.ResetPath();
        }

        private void Embrace()
        {
            _embraced = true;
            _pauseTimer = embracePause;
            _agent.isStopped = true;
            _agent.ResetPath();
            Debug.Log("[Presence] It reaches you. An open hand, not a blow.");
            OnEmbrace?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_hasMemory) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastKnownPos, 0.4f);
            Gizmos.DrawWireSphere(_lastKnownPos, searchRadius);
        }
    }
}