using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridView : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Button Room1x1Button;
    [SerializeField] private Button Room1x2Button;

    public event Action<RoomData> OnRoomSelected;

    public  List<RoomData> Rooms = new();
    private void Awake()
    {
        Room1x1Button.onClick.AddListener(()=> OnRoom1x1Selected());
        Room1x2Button.onClick.AddListener(() => OnRoom1x2Selected());
    }

    private void OnRoom1x1Selected()
    {
        OnRoomSelected?.Invoke(Rooms[0]);
    }

    private void OnRoom1x2Selected()
    {
        OnRoomSelected?.Invoke(Rooms[1]);
    }
}
