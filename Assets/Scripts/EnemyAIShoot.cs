using UnityEngine;
using System.Collections.Generic;

public class EnemyAISniper : MonoBehaviour {
    public LayerMask obstructionMask;

    [Header("Attack Settings")]
    public float attackRange = 35f;
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    [Header("FOV Settings")]
    public float viewRadius = 40f;
    [Range(0, 360)] public float viewAngle = 20f;

    [Header("Rotation Patrol")]
    public float rotationSpeed = 30f;
    public float maxRotationAngle = 45f;
    private float patrolTimeOffset;
    private float timeSinceStart;

    [Header("Search Settings")]
    public float searchDuration = 3f;
    private float searchTimer;
    private bool isSearchingLastPosition = false;
    private Vector3 lastKnownPosition;

    [Header("Targeting")]
    public float playerAimHeight = 1.5f;

    private bool isTrackingPlayer = false;

    // Laser variables
    private LineRenderer laserLine;
    private Vector3 currentLaserTarget;
    public float laserSmoothSpeed = 10f;

    private Transform currentTarget; // NEW: dynamic target reference

    private void Start() {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = true;
        laserLine.positionCount = 2;
        laserLine.startWidth = 0.05f;
        laserLine.endWidth = 0.05f;

        laserLine.material = new Material(Shader.Find("Unlit/Color"));
        laserLine.material.color = Color.red;

        currentLaserTarget = transform.position + transform.forward * viewRadius;
    }

    private void Update() {
        timeSinceStart += Time.deltaTime;

        currentTarget = FindVisibleTarget();

        bool targetInFOV = currentTarget != null;
        bool targetInRange = targetInFOV && Vector3.Distance(transform.position, currentTarget.position) <= attackRange;

        if (targetInFOV && targetInRange) {
            isTrackingPlayer = true;
            isSearchingLastPosition = false;
            AttackTarget();
        } else if (isTrackingPlayer && !targetInFOV) {
            lastKnownPosition = currentTarget ? currentTarget.position : lastKnownPosition;
            isTrackingPlayer = false;
            isSearchingLastPosition = true;
            searchTimer = searchDuration;
        }

        if (isSearchingLastPosition) {
            SearchLastKnownPosition();
        } else if (!isTrackingPlayer) {
            PatrolRotation();
        }

        // LASER
        Vector3 laserStart = transform.position + Vector3.up * 1.5f;
        Vector3 laserDirection;

        if (targetInFOV) {
            Vector3 targetPoint = currentTarget.position + Vector3.up * playerAimHeight;
            laserDirection = (targetPoint - laserStart).normalized;
        } else {
            laserDirection = transform.forward;
        }

        RaycastHit hit;
        if (Physics.Raycast(laserStart, laserDirection, out hit, viewRadius)) {
            currentLaserTarget = Vector3.Lerp(currentLaserTarget, hit.point, Time.deltaTime * laserSmoothSpeed);
        } else {
            Vector3 fallback = laserStart + laserDirection * viewRadius;
            currentLaserTarget = Vector3.Lerp(currentLaserTarget, fallback, Time.deltaTime * laserSmoothSpeed);
        }

        laserLine.SetPosition(0, laserStart);
        laserLine.SetPosition(1, currentLaserTarget);
    }

    // NEW: Find best target (prioritize Echo if both found)
    private Transform FindVisibleTarget() {
        List<Transform> potentialTargets = new List<Transform>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] echoes = GameObject.FindGameObjectsWithTag("Echo");

        foreach (GameObject go in echoes)
            if (CanSee(go.transform))
                return go.transform; // Prioritize Echo

        foreach (GameObject go in players)
            if (CanSee(go.transform))
                potentialTargets.Add(go.transform);

        // If Echo not found, return closest Player (if any)
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform t in potentialTargets) {
            float dist = Vector3.Distance(transform.position, t.position);
            if (dist < minDist) {
                minDist = dist;
                closest = t;
            }
        }
        return closest;
    }

    private bool CanSee(Transform target) {
        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * playerAimHeight;
        Vector3 dir = (targetPos - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetPos);

        if (dist < viewRadius) {
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle < viewAngle / 2f) {
                if (!Physics.Raycast(eyePos, dir, dist, obstructionMask))
                    return true;
            }
        }
        return false;
    }

    private void AttackTarget() {
        if (!currentTarget) return;

        Vector3 lookAt = currentTarget.position + Vector3.up * playerAimHeight;
        transform.LookAt(lookAt);

        if (!alreadyAttacked) {
            Debug.Log($"Sniper fires at {currentTarget.tag}!");
            if (currentTarget.CompareTag("Player"))
                currentTarget.GetComponent<PlayerMovement>()?.OnDeath(); // Call OnDeath on Player
            else if (currentTarget.CompareTag("Echo"))
                currentTarget.GetComponent<Ghost>()?.OnDeath(); // Call OnDeath on Ghost

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void PatrolRotation() {
        float angle = Mathf.PingPong((timeSinceStart + patrolTimeOffset) * rotationSpeed, maxRotationAngle * 2f) - maxRotationAngle;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void SearchLastKnownPosition() {
        Vector3 lookAtTarget = new Vector3(lastKnownPosition.x, transform.position.y, lastKnownPosition.z);
        transform.LookAt(lookAtTarget);

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f) {
            isSearchingLastPosition = false;

            float currentYAngle = transform.eulerAngles.y;
            float pingPongTime = (currentYAngle + maxRotationAngle) / rotationSpeed;
            patrolTimeOffset = pingPongTime - timeSinceStart;

            Debug.Log("Player not found. Resuming patrol.");
        }
    }

    private void ResetAttack() {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ResetAI() {
        transform.rotation = Quaternion.identity;

        alreadyAttacked = false;
        isTrackingPlayer = false;
        isSearchingLastPosition = false;
        lastKnownPosition = Vector3.zero;
        searchTimer = 0f;
        patrolTimeOffset = 0f;
        timeSinceStart = 0f;
        currentLaserTarget = transform.position + transform.forward * viewRadius;

        if (laserLine != null) {
            laserLine.SetPosition(0, transform.position + Vector3.up * 1.5f);
            laserLine.SetPosition(1, currentLaserTarget);
        }
    }
}
