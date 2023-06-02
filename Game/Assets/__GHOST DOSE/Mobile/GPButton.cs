using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GPButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public bool buttonPressed, buttonReleased;

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonReleased = true;
        buttonPressed = false;
    }
    public void LateUpdate()
    {
       buttonReleased = false;
    }
}