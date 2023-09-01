using UnityEngine;
using UnityEngine.UI;

public class GemUI : MonoBehaviour
{
    private int xPos, yPos;
    [SerializeField] private Image imageGem, imageHighlight;

    public delegate void Highlight(int x, int y, bool enabled);
    public static event Highlight OnHighlight;

    public bool Highlighted { get {return imageHighlight.gameObject.activeInHierarchy;} }
    public int PosX {get{return xPos;} private set{;}}
    public int PosY {get{return yPos;} private set{;}}

    private void Start()
    {
        imageHighlight.gameObject.SetActive(false);
    }

    public void ChangeColor(Color newColor)
    {
        imageGem.color = newColor;
    }

    public void SetPosition(int x, int y)
    {
        xPos = x;
        yPos = y;
    }

    public void TryHighlight()
    {
        if(!Highlighted)
        {
            imageHighlight.gameObject.SetActive(true);

        }
        else{
            imageHighlight.gameObject.SetActive(false);
        }

        if(OnHighlight != null)
        {
            OnHighlight(xPos, yPos, imageHighlight.gameObject.activeInHierarchy);
        }
    }
}
