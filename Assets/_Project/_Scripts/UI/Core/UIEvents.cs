using System;
using UnityEngine;

//UIEvents.cs
//static message board between gameplay and UI
//gameplay calls UIEvents.RaiseSomething(...), UI scripts subscribe to UIEvents.OnSomething
//party slots use the same index as CharacterSwapManager's teamCharacters array (0-3)

//Foresight tell colors from the Dodge / Parry / Foresight page
public enum AttackTell
{
    Yellow = 0,     //heavy, parryable
    Red = 1,        //unparryable, must dodge
    Blue = 2        //Foresight Vulnerability window
}

public static class UIEvents
{
    //========== PARTY ==========

    //(party) the 4 characters on the team, in slot order
    public static event Action<CharacterUIData[]> OnPartySet;
    public static void RaisePartySet(CharacterUIData[] party) => OnPartySet?.Invoke(party);

    //(newSlot, previousSlot) previousSlot is -1 on the very first set
    public static event Action<int, int> OnActiveCharacterChanged;
    public static void RaiseActiveCharacterChanged(int newSlot, int previousSlot) => OnActiveCharacterChanged?.Invoke(newSlot, previousSlot);

    //(duration) swap is locked for this many seconds (1.0s base per the doc)
    public static event Action<float> OnSwapCooldownStarted;
    public static void RaiseSwapCooldownStarted(float duration) => OnSwapCooldownStarted?.Invoke(duration);

    //========== CHARACTER RESOURCES ==========

    //(slot, current, max)
    public static event Action<int, float, float> OnCharacterHealthChanged;
    public static void RaiseCharacterHealthChanged(int slot, float current, float max) => OnCharacterHealthChanged?.Invoke(slot, current, max);

    //(slot, current, max)
    public static event Action<int, float, float> OnEnergyChanged;
    public static void RaiseEnergyChanged(int slot, float current, float max) => OnEnergyChanged?.Invoke(slot, current, max);

    //(current, max) stamina is shared, not per character
    public static event Action<float, float> OnStaminaChanged;
    public static void RaiseStaminaChanged(float current, float max) => OnStaminaChanged?.Invoke(current, max);

    //========== ABILITIES ==========

    //(slot, cooldownSeconds, chargesLeft) UI runs its own countdown from here
    public static event Action<int, float, int> OnSkillUsed;
    public static void RaiseSkillUsed(int slot, float cooldown, int chargesLeft) => OnSkillUsed?.Invoke(slot, cooldown, chargesLeft);

    //(slot, cooldownSeconds)
    public static event Action<int, float> OnUltimateUsed;
    public static void RaiseUltimateUsed(int slot, float cooldown) => OnUltimateUsed?.Invoke(slot, cooldown);

    //========== COMBAT FEEDBACK ==========

    //(worldPosition, amount, element, isCrit) for floating damage numbers
    public static event Action<Vector3, float, Element, bool> OnDamageDealt;
    public static void RaiseDamageDealt(Vector3 position, float amount, Element element, bool isCrit) => OnDamageDealt?.Invoke(position, amount, element, isCrit);

    //(worldPosition, reaction) for "Melt!" style popups
    public static event Action<Vector3, Reaction> OnReactionTriggered;
    public static void RaiseReactionTriggered(Vector3 position, Reaction reaction) => OnReactionTriggered?.Invoke(position, reaction);

    //(enemy, element) element icon above an enemy, Element.None clears it
    public static event Action<Transform, Element> OnEnemyElementChanged;
    public static void RaiseEnemyElementChanged(Transform enemy, Element element) => OnEnemyElementChanged?.Invoke(enemy, element);

    //(enemy, current, max) stun meter under the enemy HP bar
    public static event Action<Transform, float, float> OnStunChanged;
    public static void RaiseStunChanged(Transform enemy, float current, float max) => OnStunChanged?.Invoke(enemy, current, max);

    //(enemy) stun meter hit 100%
    public static event Action<Transform> OnStunBreak;
    public static void RaiseStunBreak(Transform enemy) => OnStunBreak?.Invoke(enemy);

    //(enemy, tell) yellow / red / blue flash before an attack
    public static event Action<Transform, AttackTell> OnAttackTell;
    public static void RaiseAttackTell(Transform enemy, AttackTell tell) => OnAttackTell?.Invoke(enemy, tell);

    //========== SAFETY ==========

    //static events survive between play sessions when Domain Reload is turned off,
    //which would leave listeners from the LAST play session still subscribed
    //this runs before every play session and wipes all subscribers clean
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAll()
    {
        OnPartySet = null;
        OnActiveCharacterChanged = null;
        OnSwapCooldownStarted = null;
        OnCharacterHealthChanged = null;
        OnEnergyChanged = null;
        OnStaminaChanged = null;
        OnSkillUsed = null;
        OnUltimateUsed = null;
        OnDamageDealt = null;
        OnReactionTriggered = null;
        OnEnemyElementChanged = null;
        OnStunChanged = null;
        OnStunBreak = null;
        OnAttackTell = null;
    }
}
