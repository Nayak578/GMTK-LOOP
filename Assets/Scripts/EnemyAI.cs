using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour {
    public NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPlayer, obstructionMask;

    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    public float attackRange = 2f;

    private Vector3 lastKnownPosition;
    private bool isSearching;
    private bool isWaitingAtLastPosition;
    public float searchDuration = 3f;
    private float searchTimer;

    public float rotationSpeed = 2f;
    public float rotationAngle = 45f;

    private Quaternion originalRotation;
    private float currentRotationTime;
    private bool rotationInitialized = false;

    private Transform target;
    private Transform realPlayer;
    public Vector3 StartPos;
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Start() {
        StartPos = transform.position;
    }
    private void Update() {
        FindTargets();

        if (target == null) {
            Patrol();
            return;
        }

        bool inFOV = CanSeeTarget(target);
        bool inAttackRange = Vector3.Distance(transform.position, GetFlatPosition(target)) <= attackRange;

        if (inFOV) {
            lastKnownPosition = GetFlatPosition(target);
            isSearching = false;
            isWaitingAtLastPosition = false;
            rotationInitialized = false;
            currentRotationTime = 0;

            if (inAttackRange)
                AttackTarget();
            else
                ChaseTarget();
        } else if (!inFOV && lastKnownPosition != Vector3.zero && !isSearching && !isWaitingAtLastPosition) {
            SearchLastKnownPosition();
        } else if (isSearching) {
            float distance = Vector3.Distance(transform.position, lastKnownPosition);
            if (distance < 0.5f) {
                if (!isWaitingAtLastPosition) {
                    agent.SetDestination(transform.position);
                    isWaitingAtLastPosition = true;
                    searchTimer = searchDuration;
                } else {
                    if (!rotationInitialized) {
                        originalRotation = transform.rotation;
                        rotationInitialized = true;
                    }

                    currentRotationTime += Time.deltaTime * rotationSpeed;
                    float angle = Mathf.Sin(currentRotationTime) * rotationAngle;
                    transform.rotation = originalRotation * Quaternion.Euler(0, angle, 0);

                    searchTimer -= Time.deltaTime;
                    if (searchTimer <= 0f) {
                        isSearching = false;
                        isWaitingAtLastPosition = false;
                        lastKnownPosition = Vector3.zero;

                        transform.rotation = originalRotation;
                        originalRotation = Quaternion.identity;
                        rotationInitialized = false;
                        currentRotationTime = 0;

                        ResumePatrol();
                    }
                }
            }
        } else {
            Patrol();
        }
    }

    private void FindTargets() {
        // Ensure real player is tracked
        if (realPlayer == null) {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
                realPlayer = found.transform;
        }

        // Track all echoes
        GameObject[] echoes = GameObject.FindGameObjectsWithTag("Echo");
        float minEchoDist = Mathf.Infinity;
        Transform closestEcho = null;

        foreach (GameObject echo in echoes) {
            float dist = Vector3.Distance(
                new Vector3(echo.transform.position.x, 0, echo.transform.position.z),
                new Vector3(transform.position.x, 0, transform.position.z)
            );

            if (dist < minEchoDist) {
                minEchoDist = dist;
                closestEcho = echo.transform;
            }
        }

        // Decide who to target
        bool echoInFOV = closestEcho != null && CanSeeTarget(closestEcho);
        bool playerInFOV = realPlayer != null && CanSeeTarget(realPlayer);

        if (echoInFOV) {
            target = closestEcho;
        } else if (playerInFOV) {
            target = realPlayer;
        } else {
            target = null;
        }
    }

    private Vector3 GetFlatPosition(Transform t) {
        return new Vector3(t.position.x, transform.position.y, t.position.z);
    }

    private bool CanSeeTarget(Transform t) {
        Vector3 flatTargetPos = GetFlatPosition(t);
        Vector3 dirToTarget = (flatTargetPos - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, flatTargetPos);

        if (distanceToTarget < viewRadius) {
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
            if (angleToTarget < viewAngle / 2f) {
                if (!Physics.Raycast(transform.position + Vector3.up, dirToTarget, distanceToTarget, obstructionMask))
                    return true;
            }
        }
        return false;
    }

    private void ChaseTarget() {
        agent.SetDestination(GetFlatPosition(target));
    }

    private void SearchLastKnownPosition() {
        isSearching = true;
        agent.SetDestination(lastKnownPosition);
    }

    private void Patrol() {
        if (patrolPoints.Length == 0 || isSearching || isWaitingAtLastPosition) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f) {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void ResumePatrol() {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void AttackTarget() {
        agent.SetDestination(transform.position);

        Vector3 lookDir = GetFlatPosition(target) - transform.position;
        if (lookDir != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (!alreadyAttacked) {
            // Call appropriate OnDeath() method
            if (target.CompareTag("Player")) {
                PlayerMovement pm = target.GetComponent<PlayerMovement>();
                if (pm != null)
                    pm.OnDeath();
            } else if (target.CompareTag("Echo")) {
                Ghost ghost = target.GetComponent<Ghost>();
                if (ghost != null)
                    ghost.OnDeath();
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }


    private void ResetAttack() {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void ResetAI() {
        agent.Warp(StartPos);
        lastKnownPosition = Vector3.zero;
        isSearching = false;
        isWaitingAtLastPosition = false;
        target = null;
        searchTimer = 0;
        rotationInitialized = false;
        currentRotationTime = 0;
        ResumePatrol();
    }

}
