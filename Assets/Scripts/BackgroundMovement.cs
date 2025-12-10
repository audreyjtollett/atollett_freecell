using UnityEngine;
using UnityEngine.UI;

public class BackgroundMovement : MonoBehaviour
{
    public RawImage background;
    public float scrollSpeed = 0.5f;
    public float offset;

    // Update is called once per frame
    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        background.uvRect = new Rect(offset, 0f, background.uvRect.width, background.uvRect.height);
    }
}
