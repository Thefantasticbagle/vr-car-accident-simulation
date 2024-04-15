using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Contains information about a given step in the protocol.
 */
public struct ProtocolItem
{
    public int         intentedOrder;
    public int         completedOrder;
    public string      description;

    public ProtocolItem(int intentedOrder, string description)
    {
        this.intentedOrder = intentedOrder;
        this.completedOrder = -1;
        this.description = description;
    }
}

/**
 *  Manages the scene state (aka protocol).
 */
public static class SceneState
{
    /*
    allSteps = [
    { index = 0
        intentedOrder = 1,
        completedOrder = null,
        description = "help the guy"
    },
    { index = 1
        intentedOrder = 1,
        completedOrder = null,
        description = "help the guy"
    }]
    */
    public static Dictionary<string, ProtocolItem>  allItems;
    public static List<string>                      unfinishedItems;
    public static List<string>                      finishedItems = new List<string> { };

    public static void CompleteItem(string itemName)
    {
        finishedItems.Add(itemName);
        if (unfinishedItems.Contains(itemName))
            unfinishedItems.Remove(itemName);
    }
}
