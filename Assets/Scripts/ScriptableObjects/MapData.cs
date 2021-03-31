using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapDataScriptableObject", order = 1)]
public class MapData : ScriptableObject
{
    public int height;
    public int width;
    public int[] heights;
}