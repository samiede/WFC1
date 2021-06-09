using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[CreateAssetMenu(fileName = "Default Vars", menuName = "ScriptableObjects/Global Vars", order = 1)]
public class GlobalVars : ScriptableObject
{
    [Range(0, 1)]
    public float animationDelay;
}
