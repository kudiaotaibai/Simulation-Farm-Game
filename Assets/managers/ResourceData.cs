using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Data", menuName = "Game/Resource Data")]
public class ResourceData : ScriptableObject
{
    public string resourceName;
    public Sprite icon;
}