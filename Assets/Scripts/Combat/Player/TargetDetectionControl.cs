using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetectionControl : MonoBehaviour
{
    public static TargetDetectionControl instance;

    [Header("Components")]
    public PlayerControl playerControl;

    [Space]
    [Header("Target Detection")]
    public LayerMask whatIsEnemy;
    public bool canChangeTarget = true;

    [Tooltip("Detection Range: Player range for detecting potential targets.")]
    [Range(0f, 15f)] public float detectionRange = 10f;

    [Tooltip("Auto-target closest enemy when no input")]
    public bool autoTargetClosest = true;

    [Tooltip("Angle threshold for directional targeting (degrees)")]
    [Range(0f, 180f)] public float targetingAngle = 45f;

    [Tooltip("How often to update target detection (seconds)")]
    [Range(0.1f, 1f)] public float updateInterval = 0.2f;

    [Space]
    [Header("Debug")]
    public bool debug;

    private Transform currentTarget;
    private float lastUpdateTime;
    private List<Transform> nearbyEnemies = new List<Transform>();

    void Awake()
    {
        instance = this;
    }

    void FixedUpdate()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateTarget();
            lastUpdateTime = Time.time;
        }
    }

    #region Target Handling

    void UpdateTarget()
    {
        if (!canChangeTarget) return;

        // Update nearby enemies list
        UpdateNearbyEnemies();

        // Check if current target is still valid
        if (currentTarget != null && !IsTargetValid(currentTarget))
        {
            ClearTarget();
        }

        // Find new target if we don't have one
        if (currentTarget == null)
        {
            Transform newTarget = FindBestTarget();
            if (newTarget != null)
            {
                SetTarget(newTarget);
            }
        }
    }

    void UpdateNearbyEnemies()
    {
        nearbyEnemies.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, whatIsEnemy);

        foreach (Collider hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && enemy.IsDead == false)
            {
                nearbyEnemies.Add(enemy.transform);
            }
        }
    }

    Transform FindBestTarget()
    {
        if (nearbyEnemies.Count == 0) return null;

        Vector3 inputDirection = GetInputDirection();
        bool hasInput = inputDirection != Vector3.zero;

        Transform bestTarget = null;
        float bestScore = 0f;

        foreach (Transform enemy in nearbyEnemies)
        {
            float score = CalculateTargetScore(enemy, inputDirection, hasInput);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    float CalculateTargetScore(Transform enemy, Vector3 inputDirection, bool hasInput)
    {
        Vector3 toEnemy = enemy.position - transform.position;
        float distance = toEnemy.magnitude;

        // Base score inversely proportional to distance
        float score = 1f - (distance / detectionRange);

        if (hasInput)
        {
            // When player has input, prioritize enemies in input direction
            toEnemy.y = 0;
            toEnemy.Normalize();

            float angle = Vector3.Angle(inputDirection, toEnemy);
            if (angle <= targetingAngle)
            {
                // Boost score for enemies in the input direction
                float angleScore = 1f - (angle / targetingAngle);
                score += angleScore * 2f; // Double weight for directional targeting
            }
            else
            {
                score *= 0.1f; // Heavily penalize enemies outside targeting angle
            }
        }
        else if (autoTargetClosest)
        {
            // When no input, slightly prefer enemies in front
            Vector3 forward = transform.forward;
            forward.y = 0;
            toEnemy.y = 0;
            toEnemy.Normalize();

            float dot = Vector3.Dot(forward, toEnemy);
            if (dot > 0) score += dot * 0.5f;
        }

        return score;
    }

    bool IsTargetValid(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;
        if (target.GetComponent<EnemyBase>() == null || target.GetComponent<EnemyBase>().IsDead) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= detectionRange;
    }

    Vector3 GetInputDirection()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (input.magnitude < 0.1f) return Vector3.zero;

        // Transform input to world space relative to camera
        Vector3 worldInput = Camera.main.transform.TransformDirection(input);
        worldInput.y = 0;
        return worldInput.normalized;
    }

    void SetTarget(Transform target)
    {
        if (currentTarget == target) return;

        currentTarget = target;
        if (playerControl != null)
            playerControl.ChangeTarget(currentTarget);

        if (debug) Debug.Log($"Target locked: {target.name}");
    }

    void ClearTarget()
    {
        if (debug && currentTarget != null)
            Debug.Log($"Target lost: {currentTarget.name}");

        currentTarget = null;
        if (playerControl != null)
            playerControl.ChangeTarget(null);
    }

    #endregion

    #region Public Methods

    public void ForceRetarget()
    {
        ClearTarget();
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    public List<Transform> GetNearbyEnemies()
    {
        return new List<Transform>(nearbyEnemies);
    }


    //get closest enemy
    public Transform GetClosestEnemy()
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (Transform enemy in nearbyEnemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        }

        // Input direction
        Vector3 inputDir = GetInputDirection();
        if (inputDir != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, inputDir * 3f);

            // Targeting cone
            Gizmos.color = Color.cyan;
            Vector3 leftBound = Quaternion.AngleAxis(-targetingAngle, Vector3.up) * inputDir;
            Vector3 rightBound = Quaternion.AngleAxis(targetingAngle, Vector3.up) * inputDir;
            Gizmos.DrawRay(transform.position, leftBound * detectionRange);
            Gizmos.DrawRay(transform.position, rightBound * detectionRange);
        }

        // Nearby enemies
        Gizmos.color = Color.green;
        foreach (Transform enemy in nearbyEnemies)
        {
            if (enemy != currentTarget)
            {
                Gizmos.DrawWireSphere(enemy.position, 0.3f);
            }
        }
    }
}