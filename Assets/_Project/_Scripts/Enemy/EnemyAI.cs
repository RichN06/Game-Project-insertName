using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Tweaks")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Movement Tweaks")]
    [SerializeField] private float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;

    private NavMeshAgent agent;
    private Transform activeTarget;
    private float targetScanCooldown = 0.2f;
    private float nextScanTime = 0f;

    private Animator animator;

    private AttackHandler attackHandler;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        attackHandler = GetComponent<AttackHandler>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackRange;

        FindActivePlayerTarget();
    }

    void Update()
    {
        // Optimization: Scanning 5 times a second saves massive performance.
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + targetScanCooldown;
            FindActivePlayerTarget();
        }

        if (activeTarget == null || !agent.isOnNavMesh) return;

        // Drive path calculations toward the active hero target position parameters [1]
        agent.SetDestination(activeTarget.position);

        // Calculate horizontal distance (ignoring height differences)
        float distanceToTarget = Vector3.Distance(transform.position, activeTarget.position);

        if (distanceToTarget <= attackRange)
        {
            // Close enough to engage: stop moving and attack
            agent.isStopped = true;
            
            // Check cooldown before attack
            if (Time.time >= nextAttackTime)
            {
                ExecuteEnemyAttack();
            }
        }
        else
        {
            // Too far: resume chasing
            agent.isStopped = false;
        }

        // Update animation parameters based on movement state
        if (animator != null)
        {
            // If the agent has remaining path distance and isn't stopped, play run animation
            bool isMoving = agent.remainingDistance > agent.stoppingDistance && !agent.isStopped;
            animator.SetFloat("Speed", isMoving ? 1f : 0f);
        }
    }

    private void FindActivePlayerTarget()
    {
        // Because your Team Swap Manager turns off inactive player objects,
        // we can find the active player instantly by filtering for the matching Tag name!
        GameObject activePlayer = GameObject.FindWithTag("Player");

        if (activePlayer != null)
        {
            activeTarget = activePlayer.transform;
        }
    }

    private void ExecuteEnemyAttack()
    {
        // Lock in the next allowed attack timestamp
        nextAttackTime = Time.time + attackCooldown;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Force the enemy's forward physics box to actively sweep for the player
        if (attackHandler != null)
        {
            attackHandler.ExecuteForwardHitboxCheck();
        }

        // This is a great anchor point for Section C to execute enemy damage frames later!
        Debug.Log("Enemy is actively attacking the player!");
    }
}
