using UnityEngine;

[System.Serializable]
public class DoorData
{
    public DoorSideEnum Side;
    public GameObject Plug;
    /// <summary>
    /// »ндекс идЄт от 0 и т.д. под возростающей. —читаетс€ справо налево и снизу вверх 
    /// </summary>
    public int Index; 
}
