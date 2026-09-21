using UnityEngine;

public interface IDamageable
{
    /*
    Every script that implements this interface MUST include this method.
    This allows the AttackHandler to deal damage to anything without 
    needing to know exactly what component it is hitting.
    */
    void TakeDamage(float amount, Vector3 hitDirection);
}
