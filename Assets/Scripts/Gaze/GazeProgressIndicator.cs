using UnityEngine;
using UnityEngine.UI;

public class GazeProgressIndicator : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetProgress(float t)
    {
        if (fillImage != null) fillImage.fillAmount = t;
    }


    public void Hide() { SetProgress(0f); }
    public void Show() { }
}
