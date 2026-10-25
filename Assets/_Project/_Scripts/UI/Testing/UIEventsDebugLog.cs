using UnityEngine;

//UIEventsDebugLog.cs
//listens to EVERY UIEvents event and prints it to the Console in the element's color
//proves events are flowing before any real HUD exists; delete or disable once the HUD works
public class UIEventsDebugLog : MonoBehaviour
{
    [SerializeField] private ElementVisuals elementVisuals;   //drag the ElementVisuals asset here

    private CharacterUIData[] party;

    //subscribe when this object turns on
    private void OnEnable()
    {
        UIEvents.OnPartySet += HandlePartySet;
        UIEvents.OnActiveCharacterChanged += HandleActiveCharacterChanged;
        UIEvents.OnSwapCooldownStarted += HandleSwapCooldown;
        UIEvents.OnCharacterHealthChanged += HandleHealth;
        UIEvents.OnEnergyChanged += HandleEnergy;
        UIEvents.OnStaminaChanged += HandleStamina;
        UIEvents.OnSkillUsed += HandleSkill;
        UIEvents.OnUltimateUsed += HandleUltimate;
        UIEvents.OnDamageDealt += HandleDamage;
        UIEvents.OnReactionTriggered += HandleReaction;
        UIEvents.OnEnemyElementChanged += HandleEnemyElement;
        UIEvents.OnStunChanged += HandleStun;
        UIEvents.OnStunBreak += HandleStunBreak;
        UIEvents.OnAttackTell += HandleAttackTell;
    }

    //ALWAYS unsubscribe when turning off, or the static event keeps calling a destroyed object
    private void OnDisable()
    {
        UIEvents.OnPartySet -= HandlePartySet;
        UIEvents.OnActiveCharacterChanged -= HandleActiveCharacterChanged;
        UIEvents.OnSwapCooldownStarted -= HandleSwapCooldown;
        UIEvents.OnCharacterHealthChanged -= HandleHealth;
        UIEvents.OnEnergyChanged -= HandleEnergy;
        UIEvents.OnStaminaChanged -= HandleStamina;
        UIEvents.OnSkillUsed -= HandleSkill;
        UIEvents.OnUltimateUsed -= HandleUltimate;
        UIEvents.OnDamageDealt -= HandleDamage;
        UIEvents.OnReactionTriggered -= HandleReaction;
        UIEvents.OnEnemyElementChanged -= HandleEnemyElement;
        UIEvents.OnStunChanged -= HandleStun;
        UIEvents.OnStunBreak -= HandleStunBreak;
        UIEvents.OnAttackTell -= HandleAttackTell;
    }

    //---------- handlers (each one matches its event's parameters exactly) ----------

    private void HandlePartySet(CharacterUIData[] newParty)
    {
        party = newParty;
        string names = "";
        for (int i = 0; i < party.Length; i++)
        {
            names += $"[{i + 1}] {(party[i] != null ? Paint(party[i].CharacterName, party[i].Element) : "empty")}  ";
        }
        Debug.Log($"<b>Party set:</b> {names}");
    }

    private void HandleActiveCharacterChanged(int newSlot, int previousSlot)
    {
        Debug.Log($"<b>Swap:</b> slot {previousSlot + 1} -> slot {newSlot + 1} ({SlotName(newSlot)})");
    }

    private void HandleSwapCooldown(float duration) => Debug.Log($"Swap locked for {duration:0.0}s");

    private void HandleHealth(int slot, float current, float max) => Debug.Log($"{SlotName(slot)} HP {current:0}/{max:0}");

    private void HandleEnergy(int slot, float current, float max) => Debug.Log($"{SlotName(slot)} energy {current:0}/{max:0}");

    private void HandleStamina(float current, float max) => Debug.Log($"Stamina {current:0}/{max:0}");

    private void HandleSkill(int slot, float cooldown, int chargesLeft)
    {
        CharacterUIData data = GetData(slot);
        string skill = data != null ? data.SkillName : "Skill";
        Debug.Log($"{SlotName(slot)} used <b>{skill}</b> (E) | cooldown {cooldown:0.0}s | charges left {chargesLeft}");
    }

    private void HandleUltimate(int slot, float cooldown)
    {
        CharacterUIData data = GetData(slot);
        string ult = data != null ? data.UltimateName : "Ultimate";
        Debug.Log($"{SlotName(slot)} used <b>{ult}</b> (Q) | cooldown {cooldown:0.0}s");
    }

    private void HandleDamage(Vector3 position, float amount, Element element, bool isCrit)
    {
        string crit = isCrit ? " <b>CRIT!</b>" : "";
        Debug.Log($"Damage {Paint(amount.ToString("0"), element)} ({element}){crit}");
    }

    private void HandleReaction(Vector3 position, Reaction reaction)
    {
        string name = elementVisuals != null ? elementVisuals.GetName(reaction) : reaction.ToString();
        Color color = elementVisuals != null ? elementVisuals.GetColor(reaction) : Color.white;
        Debug.Log($"<b>Reaction:</b> <color=#{ColorUtility.ToHtmlStringRGB(color)}>{name}!</color>");
    }

    private void HandleEnemyElement(Transform enemy, Element element) => Debug.Log($"{enemy.name} now has {Paint(element.ToString(), element)} applied");

    private void HandleStun(Transform enemy, float current, float max) => Debug.Log($"{enemy.name} stun {current:0}/{max:0}");

    private void HandleStunBreak(Transform enemy) => Debug.Log($"<b>STUN BREAK</b> on {enemy.name}");

    private void HandleAttackTell(Transform enemy, AttackTell tell)
    {
        string hex = tell == AttackTell.Yellow ? "FFD84A" : tell == AttackTell.Red ? "FF4A4A" : "4AA8FF";
        Debug.Log($"<color=#{hex}><b>{tell} flash</b></color> from {enemy.name}");
    }

    //---------- helpers ----------

    private CharacterUIData GetData(int slot)
    {
        if (party == null || slot < 0 || slot >= party.Length) return null;
        return party[slot];
    }

    private string SlotName(int slot)
    {
        CharacterUIData data = GetData(slot);
        return data != null ? Paint(data.CharacterName, data.Element) : $"Slot {slot + 1}";
    }

    //wraps text in a rich-text color tag using the element's color
    private string Paint(string text, Element element)
    {
        if (elementVisuals == null) return text;
        string hex = ColorUtility.ToHtmlStringRGB(elementVisuals.GetColor(element));
        return $"<color=#{hex}>{text}</color>";
    }
}
