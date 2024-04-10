using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Contains information about a given step in the protocol.
 */
public struct Step
{
    string      name;
    int         intentedOrder;
    int         completedOrder;
    string      description;

    public Step(string name, int intentedOrder, string description)
    {
        this.name = name;
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
    public static List<Step> allSteps;
    public static List<int>  unfinishedItems;
    public static List<int>  finishedItems;
}
