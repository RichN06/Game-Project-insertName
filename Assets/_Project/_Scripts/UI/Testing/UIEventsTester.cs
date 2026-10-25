using UnityEngine;
using UnityEngine.InputSystem;

//UIEventsTester.cs
//fires FAKE UIEvents from the keyboard so the UI can be tested without real combat
//use it in an empty test scene only (its number keys overlap the real swap keys)
//1-4 swap | E skill | Q ult | H damage | R random reaction | S stun break | T attack tell
public class UIEventsTester : MonoBehaviour
{
    [Header("Fake Party (drag 4 CharacterUIData assets here)")]
    [SerializeField] private CharacterUIData[] party = new CharacterUIData[4];

    [Header("Fake Values")]
    [SerializeField] private float maxHealth = 1000f;
    [SerializeField] private float damagePerHit = 120f;
    [SerializeField] private float swapCooldown = 1.0f;

    private int activeSlot = 0;
    private float[] health;
    private float[] energy;
    private float nextSwapTime = 0f;

    private void Start()
    {
        //fill every slot to full so the UI starts in a sensible state
        health = new float[party.Length];
        energy = new float[party.Length];
        for (int i = 0; i < party.Length; i++)
        {
            health[i] = maxHealth;
            energy[i] = 0f;
        }

        UIEvents.RaisePartySet(party);
        UIEvents.RaiseActiveCharacterChanged(activeSlot, -1);
        for (int i = 0; i < party.Length; i++)
        {
            UIEvents.RaiseCharacterHealthChanged(i, health[i], maxHealth);
            UIEvents.RaiseEnergyChanged(i, energy[i], GetUltCost(i));
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;     //no keyboard connected

        if (kb.digit1Key.wasPressedThisFrame) TrySwap(0);
        if (kb.digit2Key.wasPressedThisFrame) TrySwap(1);
        if (kb.digit3Key.wasPressedThisFrame) TrySwap(2);
        if (kb.digit4Key.wasPressedThisFrame) TrySwap(3);

        if (kb.eKey.wasPressedThisFrame) FakeSkill();
        if (kb.qKey.wasPressedThisFrame) FakeUltimate();
        if (kb.hKey.wasPressedThisFrame) FakeHit();
        if (kb.rKey.wasPressedThisFrame) FakeReaction();
        if (kb.sKey.wasPressedThisFrame) UIEvents.RaiseStunBreak(transform);
        if (kb.tKey.wasPressedThisFrame) UIEvents.RaiseAttackTell(transform, (AttackTell)Random.Range(0, 3));
    }

    private void TrySwap(int slot)
    {
        if (slot >= party.Length || slot == activeSlot || party[slot] == null) return;
        if (Time.time < nextSwapTime) return;   //still on swap cooldown

        int previous = activeSlot;
        activeSlot = slot;
        nextSwapTime = Time.time + swapCooldown;

        UIEvents.RaiseActiveCharacterChanged(activeSlot, previous);
        UIEvents.RaiseSwapCooldownStarted(swapCooldown);
    }

    private void FakeSkill()
    {
        CharacterUIData data = party[activeSlot];
        if (data == null) return;

        UIEvents.RaiseSkillUsed(activeSlot, data.SkillCooldown, data.SkillCharges - 1);

        //skills generate a bit of energy, like the real game will
        energy[activeSlot] = Mathf.Min(energy[activeSlot] + 15f, GetUltCost(activeSlot));
        UIEvents.RaiseEnergyChanged(activeSlot, energy[activeSlot], GetUltCost(activeSlot));
    }

    private void FakeUltimate()
    {
        CharacterUIData data = party[activeSlot];
        if (data == null) return;
        if (energy[activeSlot] < GetUltCost(activeSlot))
        {
            Debug.Log("Not enough energy for ultimate (press E to build some)");
            return;
        }

        energy[activeSlot] = 0f;
        UIEvents.RaiseUltimateUsed(activeSlot, data.UltimateCooldown);
        UIEvents.RaiseEnergyChanged(activeSlot, 0f, GetUltCost(activeSlot));
    }

    private void FakeHit()
    {
        //the active character takes a hit AND deals a hit, so both kinds of event fire
        health[activeSlot] = Mathf.Max(0f, health[activeSlot] - damagePerHit);
        UIEvents.RaiseCharacterHealthChanged(activeSlot, health[activeSlot], maxHealth);

        Element element = party[activeSlot] != null ? party[activeSlot].Element : Element.Physical;
        bool isCrit = Random.value < 0.3f;
        UIEvents.RaiseDamageDealt(transform.position, isCrit ? damagePerHit * 2f : damagePerHit, element, isCrit);
        UIEvents.RaiseEnemyElementChanged(transform, element);
    }

    private void FakeReaction()
    {
        //pick two random base elements and ask the real table what they make
        Element a = (Element)Random.Range((int)Element.Heat, (int)Element.Herbage + 1);
        Element b = (Element)Random.Range((int)Element.Heat, (int)Element.Herbage + 1);
        Reaction reaction = ElementReactions.GetReaction(a, b);

        if (reaction == Reaction.None)
        {
            Debug.Log($"{a} + {b} = no reaction (press R again)");
            return;
        }
        UIEvents.RaiseReactionTriggered(transform.position, reaction);
    }

    private float GetUltCost(int slot)
    {
        CharacterUIData data = party[slot];
        return data != null && data.UltimateEnergyCost > 0f ? data.UltimateEnergyCost : 60f;
    }
}
