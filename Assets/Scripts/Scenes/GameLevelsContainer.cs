using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class GameLevelsContainer : ScriptableObject
{
    [SerializeField, InspectableSO] public List<GameLevel> GameLevels = null;
}
