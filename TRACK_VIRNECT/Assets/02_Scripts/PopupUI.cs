using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopupUI : MonoBehaviour, IPointerDownHandler
{
    public Button _closeButton;
    public event Action OnFocus;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnFocus();
    }
}