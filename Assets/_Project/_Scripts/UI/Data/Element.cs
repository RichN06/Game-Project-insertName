//Element.cs
//shared element + reaction definitions used by both combat code and UI code
//numbers are written out on purpose: Unity saves enums as numbers, so never renumber or reorder
//add new entries at the end with the next unused number

public enum Element
{
    None = 0,       //no element applied / not set yet
    Physical = 1,   //plain non-elemental hits (basic AttackHandler hits for now)
    Heat = 2,
    Cold = 3,
    Water = 4,
    Electric = 5,
    Toxin = 6,
    Erosion = 7,
    Gale = 8,
    Herbage = 9
}

public enum Reaction
{
    None = 0,

    //Heat reactions
    Melt = 1,           //Heat + Cold
    Vaporize = 2,       //Heat + Water
    Blast = 3,          //Heat + Electric
    Gas = 4,            //Heat + Toxin
    Petrify = 5,        //Heat + Erosion
    Burning = 6,        //Heat + Herbage

    //Cold reactions
    Freeze = 7,         //Cold + Water
    Superconduct = 8,   //Cold + Electric
    Frostbite = 9,      //Cold + Toxin
    Shatter = 10,       //Cold + Erosion
    Fragile = 11,       //Cold + Herbage

    //Water reactions
    ShockChain = 12,    //Water + Electric
    Mire = 13,          //Water + Toxin
    Abrasion = 14,      //Water + Erosion
    Overgrowth = 15,    //Water + Herbage

    //Electric reactions
    NeuroShock = 16,    //Electric + Toxin
    Plasma = 17,        //Electric + Erosion
    BioChain = 18,      //Electric + Herbage

    //Toxin reactions
    Corrosive = 19,     //Toxin + Erosion
    Poison = 20,        //Toxin + Herbage

    //Erosion reactions
    Decay = 21,         //Erosion + Herbage

    //Gale + any base element
    Swirl = 22
}

//single source of truth for "what happens when element A meets element B"
//static = no object needed, call it from anywhere: ElementReactions.GetReaction(Element.Heat, Element.Cold)
public static class ElementReactions
{
    //returns the reaction for two elements, or Reaction.None if they don't react
    //order doesn't matter: (Heat, Cold) and (Cold, Heat) both give Melt
    public static Reaction GetReaction(Element a, Element b)
    {
        //None and Physical never react, and an element can't react with itself
        if (!IsBaseElement(a) || !IsBaseElement(b) || a == b) return Reaction.None;

        //Gale swirls with any other base element
        if (a == Element.Gale || b == Element.Gale) return Reaction.Swirl;

        //sort the pair so we only have to list each combo once
        Element first = a < b ? a : b;
        Element second = a < b ? b : a;

        switch (first)
        {
            case Element.Heat:
                switch (second)
                {
                    case Element.Cold: return Reaction.Melt;
                    case Element.Water: return Reaction.Vaporize;
                    case Element.Electric: return Reaction.Blast;
                    case Element.Toxin: return Reaction.Gas;
                    case Element.Erosion: return Reaction.Petrify;
                    case Element.Herbage: return Reaction.Burning;
                }
                break;

            case Element.Cold:
                switch (second)
                {
                    case Element.Water: return Reaction.Freeze;
                    case Element.Electric: return Reaction.Superconduct;
                    case Element.Toxin: return Reaction.Frostbite;
                    case Element.Erosion: return Reaction.Shatter;
                    case Element.Herbage: return Reaction.Fragile;
                }
                break;

            case Element.Water:
                switch (second)
                {
                    case Element.Electric: return Reaction.ShockChain;
                    case Element.Toxin: return Reaction.Mire;
                    case Element.Erosion: return Reaction.Abrasion;
                    case Element.Herbage: return Reaction.Overgrowth;
                }
                break;

            case Element.Electric:
                switch (second)
                {
                    case Element.Toxin: return Reaction.NeuroShock;
                    case Element.Erosion: return Reaction.Plasma;
                    case Element.Herbage: return Reaction.BioChain;
                }
                break;

            case Element.Toxin:
                switch (second)
                {
                    case Element.Erosion: return Reaction.Corrosive;
                    case Element.Herbage: return Reaction.Poison;
                }
                break;

            case Element.Erosion:
                if (second == Element.Herbage) return Reaction.Decay;
                break;
        }

        return Reaction.None;
    }

    //true for the 8 elements from the design doc (not None, not Physical)
    public static bool IsBaseElement(Element e)
    {
        return e >= Element.Heat && e <= Element.Herbage;
    }
}
