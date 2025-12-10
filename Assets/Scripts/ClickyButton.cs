using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image buttonImage;
    public Sprite defaultImg, pressedImg;

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = pressedImg;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.sprite = defaultImg;
    }
}
