using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    [Header("Simple Combat Parameters")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float hitboxRange = 2f;      // How far in front of you the box reaches
    [SerializeField] private float hitboxWidth = 1.5f;    // The side-to-side width of the box
    [SerializeField] private float hitboxHeight = 2.0f;   // The vertical thickness of the box
    [SerializeField] private LayerMask targetLayer;       // Players choose "Enemy", Enemies choose "Player"

    private PlayerInputBridge inputBridge;

    void Start()
    {
        // Try to fetch input bridge if this is a player character instance
        inputBridge = GetComponent<PlayerInputBridge>();
    }

    private void OnEnable()
    {
        if (inputBridge == null) inputBridge = GetComponent<PlayerInputBridge>();
        if (inputBridge != null) inputBridge.OnAttackTriggered += ExecuteForwardHitboxCheck;
    }

    private void OnDisable()
    {
        if (inputBridge != null) inputBridge.OnAttackTriggered -= ExecuteForwardHitboxCheck;
    }

    // NOTE FOR AI: Your EnemyAI script can call this public function directly when it gets in range!
    public void ExecuteForwardHitboxCheck()
    {
        // 1. Calculate the center position of the box directly in front of the character's face
        Vector3 boxCenter = transform.position + (transform.forward * (hitboxRange / 2f)) + (Vector3.up * (hitboxHeight / 2f));
        
        // 2. Define the half-extents (size calculations) for the Physics Box Check
        Vector3 boxHalfExtents = new Vector3(hitboxWidth / 2f, hitboxHeight / 2f, hitboxRange / 2f);

        // 3. Cast an instantaneous invisible spatial calculation box to capture any overlapping colliders
        Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation, targetLayer);

        foreach (Collider victim in hits)
        {
            // Avoid friendly fire self-harm checks
            if (victim.gameObject == this.gameObject) continue;

            // 4. Check if the target implements our IDamageable contract interface
            IDamageable damageTarget = victim.GetComponent<IDamageable>();
            if (damageTarget != null)
            {
                Vector3 hitDirection = (victim.transform.position - transform.position).normalized;
                damageTarget.TakeDamage(attackDamage, hitDirection);
                Debug.Log($"{gameObject.name} successfully struck {victim.name} using forward hitbox!");
            }
        }
    }

    // Draws the invisible hitbox inside your Unity Scene Editor view so you can see its exact size
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        
        // Match the rotation gizmo to match player heading vectors
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        
        Vector3 visualCenter = (Vector3.forward * (hitboxRange / 2f)) + (Vector3.up * (hitboxHeight / 2f));
        Vector3 visualSize = new Vector3(hitboxWidth, hitboxHeight, hitboxRange);
        
        Gizmos.DrawWireCube(visualCenter, visualSize);
        Gizmos.matrix = originalMatrix;
    }
}
