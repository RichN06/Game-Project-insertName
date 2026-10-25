using System;
using System.Collections.Generic;
using UnityEngine;

//ElementVisuals.cs
//one asset that holds the UI look (name, color, icon) for every element and reaction
//HUD scripts ask this asset "what color is Heat?" instead of hardcoding colors everywhere
[CreateAssetMenu(fileName = "ElementVisuals", menuName = "UI/Element Visuals")]
public class ElementVisuals : ScriptableObject
{
    [Serializable]
    public class ElementEntry
    {
        public Element element;
        public string displayName;
        public Color color = Color.white;
        public Sprite icon;     //leave empty until icons are drawn
    }

    [Serializable]
    public class ReactionEntry
    {
        public Reaction reaction;
        public string displayName;  //text shown on the popup, e.g. "Shock-Chain"
        public Color color = Color.white;
    }

    [SerializeField] private List<ElementEntry> elements = new List<ElementEntry>();
    [SerializeField] private List<ReactionEntry> reactions = new List<ReactionEntry>();

    //fast lookups built from the lists above (not saved, rebuilt automatically)
    private Dictionary<Element, ElementEntry> elementLookup;
    private Dictionary<Reaction, ReactionEntry> reactionLookup;

    //---------- public getters HUD scripts will call ----------

    public Color GetColor(Element element)
    {
        ElementEntry entry = FindEntry(element);
        return entry != null ? entry.color : Color.white;
    }

    public Sprite GetIcon(Element element)
    {
        ElementEntry entry = FindEntry(element);
        return entry != null ? entry.icon : null;
    }

    public string GetName(Element element)
    {
        ElementEntry entry = FindEntry(element);
        return entry != null && !string.IsNullOrEmpty(entry.displayName) ? entry.displayName : element.ToString();
    }

    public Color GetColor(Reaction reaction)
    {
        ReactionEntry entry = FindEntry(reaction);
        return entry != null ? entry.color : Color.white;
    }

    public string GetName(Reaction reaction)
    {
        ReactionEntry entry = FindEntry(reaction);
        return entry != null && !string.IsNullOrEmpty(entry.displayName) ? entry.displayName : reaction.ToString();
    }

    //---------- lookup plumbing ----------

    private ElementEntry FindEntry(Element element)
    {
        if (elementLookup == null) BuildLookups();
        elementLookup.TryGetValue(element, out ElementEntry entry);
        return entry;
    }

    private ReactionEntry FindEntry(Reaction reaction)
    {
        if (reactionLookup == null) BuildLookups();
        reactionLookup.TryGetValue(reaction, out ReactionEntry entry);
        return entry;
    }

    private void BuildLookups()
    {
        elementLookup = new Dictionary<Element, ElementEntry>();
        foreach (ElementEntry entry in elements)
        {
            elementLookup[entry.element] = entry;   //if listed twice, the last one wins
        }

        reactionLookup = new Dictionary<Reaction, ReactionEntry>();
        foreach (ReactionEntry entry in reactions)
        {
            reactionLookup[entry.reaction] = entry;
        }
    }

    //runs whenever you edit the asset in the Inspector, so lookups never go stale
    private void OnValidate()
    {
        BuildLookups();
    }

    //---------- auto-fill so you don't type 31 entries by hand ----------

    //Unity calls Reset() when the asset is first created (and from the ⋮ menu > Reset)
    private void Reset()
    {
        FillDefaults();
    }

    //right-click the asset's Inspector header > Fill Defaults to refill at any time
    [ContextMenu("Fill Defaults")]
    private void FillDefaults()
    {
        elements = new List<ElementEntry>
        {
            NewElement(Element.Physical, "#D0D0D0"),
            NewElement(Element.Heat,     "#FF6A2B"),
            NewElement(Element.Cold,     "#9FE6FF"),
            NewElement(Element.Water,    "#3A8DFF"),
            NewElement(Element.Electric, "#FFD84A"),
            NewElement(Element.Toxin,    "#A64DFF"),
            NewElement(Element.Erosion,  "#C08A4A"),
            NewElement(Element.Gale,     "#7EE8C8"),
            NewElement(Element.Herbage,  "#4CC46A"),
        };
        BuildLookups();

        reactions = new List<ReactionEntry>();
        foreach (Reaction reaction in Enum.GetValues(typeof(Reaction)))
        {
            if (reaction == Reaction.None) continue;
            reactions.Add(new ReactionEntry
            {
                reaction = reaction,
                displayName = DefaultReactionName(reaction),
                color = DefaultReactionColor(reaction)
            });
        }
        BuildLookups();
    }

    private static ElementEntry NewElement(Element element, string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return new ElementEntry { element = element, displayName = element.ToString(), color = color };
    }

    //the doc spells a few reactions with hyphens; enum names can't have hyphens
    private static string DefaultReactionName(Reaction reaction)
    {
        switch (reaction)
        {
            case Reaction.ShockChain: return "Shock-Chain";
            case Reaction.NeuroShock: return "Neuro-Shock";
            case Reaction.BioChain: return "Bio-Chain";
            default: return reaction.ToString();
        }
    }

    //default reaction color = halfway blend of the two element colors that cause it
    private Color DefaultReactionColor(Reaction reaction)
    {
        if (reaction == Reaction.Swirl) return GetColor(Element.Gale);

        //search every element pair until we find the one that makes this reaction
        for (Element a = Element.Heat; a <= Element.Herbage; a++)
        {
            for (Element b = a + 1; b <= Element.Herbage; b++)
            {
                if (ElementReactions.GetReaction(a, b) == reaction)
                {
                    return Color.Lerp(GetColor(a), GetColor(b), 0.5f);
                }
            }
        }
        return Color.white;
    }
}
