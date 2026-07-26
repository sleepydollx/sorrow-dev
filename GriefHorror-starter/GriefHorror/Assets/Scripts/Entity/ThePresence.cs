using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace GriefHorror.Entity
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ThePresence : MonoBehaviour
    {
        public enum State { Dormant, Pursuing, Reaching, Dissipating }

        [Header("Target")]
        [SerializeField] private Transform player;
        [Tooltip("Must match (or sit just under) the player's WALK speed. It never moves faster than this.")]
        [SerializeField] private float walkSpeed = 2.0f;

        [Header("Encounter distances")]
        [Tooltip("Closer than this, it stops walking and reaches out instead.")]
        [SerializeField] private float reachDistance = 2.2f;
        [Tooltip("Farther than this, it loses the thread and dissipates on its own.")]
        [SerializeField] private float loseDistance = 25f;

        [Header("Witnessing")]
        [Tooltip("Seconds the player must remain still, at reach distance, for the encounter to complete.")]
        [SerializeField] private float witnessTime = 6f;
        [Tooltip("Player speed below this counts as 'standing still'.")]
        [SerializeField] private float stillThreshold = 0.4f;
        [Tooltip("Player speed above this counts as 'fleeing'.")]
        [SerializeField] private float fleeThreshold = 3.5f;

        [Header("Presentation")]
        [Tooltip("Optional: Animator with 'Walking' (bool) and 'Reach' (trigger) parameters.")]
        [SerializeField] private Animator animator;
        [SerializeField] private float dissipateSeconds = 3f;

        [Header("Events")]
        public UnityEvent onPursuitStarted;
        [Tooltip("Invoked roughly once per second while the player is actively running away.")]
        public UnityEvent onPlayerFleeing;
        [Tooltip("The player stood still and let it reach them. The encounter is complete.")]
        public UnityEvent onWitnessed;
        public UnityEvent onDissipated;

        public State CurrentState { get; private set; } = State.Dormant;

        private NavMeshAgent _agent;
        private Vector3 _lastPlayerPos;
        private float _playerSpeed;
        private float _witnessTimer;
        private float _fleeTickTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = walkSpeed;
            _agent.acceleration = 4f;      // it does not lunge
            _agent.angularSpeed = 120f;    // it turns like a person, not a turret
            _agent.autoBraking = true;
            _agent.stoppingDistance = reachDistance * 0.9f;
            _agent.enabled = false;        // dormant until Manifest() is called
        }

        private void Start()
        {
            if (player == null)
            {
                var found = GameObject.FindGameObjectWithTag("Player");
                if (found != null) player = found.transform;
            }
            if (player != null)
                _lastPlayerPos = player.position;
        }

        private void Update()
        {
            if (player == null || CurrentState == State.Dormant || CurrentState == State.Dissipating)
                return;

            TrackPlayerSpeed();

            switch (CurrentState)
            {
                case State.Pursuing:  TickPursuing();  break;
                case State.Reaching:  TickReaching();  break;
            }
        }

        // Public API

        public void Manifest(Vector3 position)
        {
            transform.position = position;
            _agent.enabled = true;
            _agent.Warp(position);
            SetState(State.Pursuing);
            onPursuitStarted?.Invoke();
        }

        public void Manifest() => Manifest(transform.position);

        public void Dissipate()
        {
            if (CurrentState == State.Dormant || CurrentState == State.Dissipating)
                return;
            StartCoroutine(DissipateRoutine(witnessed: false));
        }

        // State Ticks

        private void TickPursuing()
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist > loseDistance)
            {
                // Ran far enough. It stops following — for now. That is not victory.
                StartCoroutine(DissipateRoutine(witnessed: false));
                return;
            }

            if (dist <= reachDistance)
            {
                SetState(State.Reaching);
                return;
            }

            _agent.SetDestination(player.position);
            if (animator != null)
                animator.SetBool("Walking", _agent.velocity.sqrMagnitude > 0.01f);

            // Fleeing feeds the grief, about once per second.
            if (_playerSpeed >= fleeThreshold)
            {
                _fleeTickTimer -= Time.deltaTime;
                if (_fleeTickTimer <= 0f)
                {
                    onPlayerFleeing?.Invoke();
                    _fleeTickTimer = 1f;
                }
            }
            else
            {
                _fleeTickTimer = 0f;
            }
        }

        private void TickReaching()
        {
            float dist = Vector3.Distance(transform.position, player.position);

            // Player pulled away again — resume the slow walk. The timer resets:
            // witnessing cannot be done in installments.
            if (dist > reachDistance * 1.5f)
            {
                _witnessTimer = 0f;
                SetState(State.Pursuing);
                return;
            }

            // Face the player. The hand stays open.
            Vector3 look = player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 2f);

            if (_playerSpeed <= stillThreshold)
            {
                _witnessTimer += Time.deltaTime;
                if (_witnessTimer >= witnessTime)
                    StartCoroutine(DissipateRoutine(witnessed: true));
            }
            else
            {
                // Fidgeting is allowed; the timer just pauses.
                _witnessTimer = Mathf.Max(0f, _witnessTimer - Time.deltaTime * 0.5f);
            }
        }

        // Internals

        private void TrackPlayerSpeed()
        {
            _playerSpeed = (player.position - _lastPlayerPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPlayerPos = player.position;
        }

        private void SetState(State next)
        {
            CurrentState = next;

            if (next == State.Reaching)
            {
                _agent.ResetPath();
                _witnessTimer = 0f;
                if (animator != null)
                {
                    animator.SetBool("Walking", false);
                    animator.SetTrigger("Reach");
                }
            }
        }

        private IEnumerator DissipateRoutine(bool witnessed)
        {
            SetStateInternalDissipating();

            if (witnessed)
                onWitnessed?.Invoke();

            // Hand the fade to your VFX via onDissipated, or scale-out as a fallback.
            onDissipated?.Invoke();

            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < dissipateSeconds)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / dissipateSeconds);
                yield return null;
            }

            transform.localScale = startScale;
            _agent.enabled = false;
            gameObject.SetActive(false);
            CurrentState = State.Dormant;
        }

        private void SetStateInternalDissipating()
        {
            CurrentState = State.Dissipating;
            _agent.ResetPath();
            if (animator != null)
                animator.SetBool("Walking", false);
        }
    }
}
