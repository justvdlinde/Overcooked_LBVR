using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.SceneManagement;

[System.Serializable]
public class GameLevel : ScriptableObject
{
    public string Title => title;
	[SerializeField] private string title = "New Level";

    public SceneField Scene => scene;
    [SerializeField] private SceneField scene = null;

    public Vector2 Dimensions => dimensions;
    [SerializeField] private Vector2 dimensions = new Vector2(5, 5);

    public Vector2 DimensionsInFeet => dimensionsInFeet;
    [SerializeField, ReadOnly] private Vector2 dimensionsInFeet = new Vector2();
}
