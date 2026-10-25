using UnityEngine;

//CharacterUIData.cs
//one asset per character holding everything the UI needs to draw them
//gameplay numbers (damage, scaling) do NOT live here, only what the player sees on screen

//weapon types from the Playable Characters page
//same rule as Element: numbers are locked, only add new ones at the end
public enum WeaponType
{
    None = 0,
    Sword = 1,
    Bow = 2,
    Lance = 3,
    Dagger = 4,
    Catalyst = 5,
    Claymore = 6
}

[CreateAssetMenu(fileName = "NewCharacterUIData", menuName = "UI/Character UI Data")]
public class CharacterUIData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterName = "New Character";
    [SerializeField] private Element element = Element.None;
    [SerializeField] private WeaponType weapon = WeaponType.None;
    [SerializeField] private string role = "";              //e.g. "On-Field DPS"
    [SerializeField] private bool isBench = false;          //true for characters not fully designed yet

    [Header("Art")]
    [SerializeField] private Sprite portrait;               //large image (character screen, swap banner)
    [SerializeField] private Sprite partyIcon;              //small square image for the party slot

    [Header("Skill (E)")]
    [SerializeField] private string skillName = "";
    [SerializeField] private Sprite skillIcon;
    [SerializeField, Min(0f)] private float skillCooldown = 0f;   //seconds
    [SerializeField, Min(1)] private int skillCharges = 1;        //Thread 1 can raise this to 2
    [SerializeField, TextArea(2, 5)] private string skillDescription = "";

    [Header("Ultimate (Q)")]
    [SerializeField] private string ultimateName = "";
    [SerializeField] private Sprite ultimateIcon;
    [SerializeField, Min(0f)] private float ultimateEnergyCost = 0f;
    [SerializeField, Min(0f)] private float ultimateCooldown = 0f;   //seconds
    [SerializeField, TextArea(2, 5)] private string ultimateDescription = "";

    //---------- read-only access for HUD scripts ----------
    //"=>" is shorthand for a getter that just returns the field

    public string CharacterName => characterName;
    public Element Element => element;
    public WeaponType Weapon => weapon;
    public string Role => role;
    public bool IsBench => isBench;

    public Sprite Portrait => portrait;
    public Sprite PartyIcon => partyIcon != null ? partyIcon : portrait;  //fall back to portrait if no icon yet

    public string SkillName => skillName;
    public Sprite SkillIcon => skillIcon;
    public float SkillCooldown => skillCooldown;
    public int SkillCharges => skillCharges;
    public string SkillDescription => skillDescription;

    public string UltimateName => ultimateName;
    public Sprite UltimateIcon => ultimateIcon;
    public float UltimateEnergyCost => ultimateEnergyCost;
    public float UltimateCooldown => ultimateCooldown;
    public string UltimateDescription => ultimateDescription;

    //convenience: this character's element color from the shared ElementVisuals asset
    public Color GetElementColor(ElementVisuals visuals)
    {
        return visuals != null ? visuals.GetColor(element) : Color.white;
    }
}
