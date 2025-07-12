using UnityEngine;

public class NPCController : NetworkedEntity
{
    public string npcName = "NPC";

    void Start()
    {
        var floating = GetComponent<FloatingName>();
        if (floating != null)
            floating.entityName = npcName;
    }
}
