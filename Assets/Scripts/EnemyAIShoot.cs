using UnityEngine;

public class EnemyAISniper : MonoBehaviour
{
    public Transform player;
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
    public float playerAimHeight = 1.5f; // Adjust height where laser hits player


    private bool isTrackingPlayer = false;

    // Laser variables
    private LineRenderer laserLine;
    private Vector3 currentLaserTarget;
    public float laserSmoothSpeed = 10f;

    private void Start()
    {
        // Initialize Line Renderer
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = true;
        laserLine.positionCount = 2;
        laserLine.startWidth = 0.05f;
        laserLine.endWidth = 0.05f;

        // Set laser material
        laserLine.material = new Material(Shader.Find("Unlit/Color"));
        laserLine.material.color = Color.red;

        currentLaserTarget = transform.position + transform.forward * viewRadius;
    }

    private void Update()
    {
        timeSinceStart += Time.deltaTime;

        bool playerInFOV = CanSeePlayer();
        bool playerInRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (playerInFOV && playerInRange)
        {
            isTrackingPlayer = true;
            isSearchingLastPosition = false;
            AttackPlayer();
        }
        else if (isTrackingPlayer && !playerInFOV)
        {
            lastKnownPosition = player.position;
            isTrackingPlayer = false;
            isSearchingLastPosition = true;
            searchTimer = searchDuration;
        }

        if (isSearchingLastPosition)
        {
            SearchLastKnownPosition();
        }
        else if (!isTrackingPlayer)
        {
            PatrolRotation();
        }

        // Laser Logic (Always On)
        Vector3 laserStart = transform.position + Vector3.up * 1.5f;
        Vector3 laserDirection;

        if (playerInFOV)
        {
            Vector3 targetPoint = player.position + Vector3.up * playerAimHeight;
            laserDirection = (targetPoint - laserStart).normalized;
        }
        else
        {
            laserDirection = transform.forward;
        }

        RaycastHit hit;
        if (Physics.Raycast(laserStart, laserDirection, out hit, viewRadius))
        {
            Vector3 target = hit.point;
            currentLaserTarget = Vector3.Lerp(currentLaserTarget, target, Time.deltaTime * laserSmoothSpeed);
        }
        else
        {
            Vector3 fallback = laserStart + laserDirection * viewRadius;
            currentLaserTarget = Vector3.Lerp(currentLaserTarget, fallback, Time.deltaTime * laserSmoothSpeed);
        }

        laserLine.SetPosition(0, laserStart);
        laserLine.SetPosition(1, currentLaserTarget);
    }

    private bool CanSeePlayer()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 1.5f; // sniper's eye level
        Vector3 targetPosition = player.position + Vector3.up * playerAimHeight;
        Vector3 dirToPlayer = (targetPosition - eyePosition).normalized;
        float distanceToPlayer = Vector3.Distance(eyePosition, targetPosition);


        if (distanceToPlayer < viewRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleToPlayer < viewAngle / 2f)
            {
                if (!Physics.Raycast(eyePosition, dirToPlayer, distanceToPlayer, obstructionMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PatrolRotation()
    {
        float angle = Mathf.PingPong((timeSinceStart + patrolTimeOffset) * rotationSpeed, maxRotationAngle * 2f) - maxRotationAngle;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void AttackPlayer()
    {
        Vector3 lookAtTarget = player.position + Vector3.up * playerAimHeight;
        transform.LookAt(lookAtTarget);

        if (!alreadyAttacked)
        {
            Debug.Log("Sniper fires at player!");
            // Your shooting logic goes here
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void SearchLastKnownPosition()
    {
        Vector3 lookAtTarget = new Vector3(lastKnownPosition.x, transform.position.y, lastKnownPosition.z);
        transform.LookAt(lookAtTarget);

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            isSearchingLastPosition = false;

            float currentYAngle = transform.eulerAngles.y;
            float pingPongTime = (currentYAngle + maxRotationAngle) / rotationSpeed;
            patrolTimeOffset = pingPongTime - timeSinceStart;

            Debug.Log("Player not found. Resuming patrol from last seen direction.");
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
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
}
