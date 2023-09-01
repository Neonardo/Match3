using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    [SerializeField] private GameController controllerReference;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<Color> colors;
    [SerializeField] private RectTransform boardTransform;
    [SerializeField] private float gemImageWidth, gemImageHeight;
    [SerializeField] private GameObject gemPrefab;

    private int columns, rows;
    private GemUI[,] board;

    int currentlyHiglightedGemsCount;
    GemUI[] highlightedGems;
    
    public void Initiallize(int height, int width)
    {
        columns = width;
        rows = height;

        currentlyHiglightedGemsCount = 0;
        GemUI.OnHighlight += HandleNewHighlight;
        highlightedGems = new GemUI[2];

        CreateBoard();
    }

    public void FillBoard(int[,] newSetup)
    {
        for(int y=0; y < columns; ++y)
        {
            for(int x=0; x < rows; ++x)
            {
                if(newSetup[x,y] != -1)
                {
                    board[x,y].ChangeColor(GetColor(newSetup[x,y]));
                }
                else
                {
                    // if we want to show already scored/destroyed gems, we turn them to clear color
                    board[x,y].ChangeColor(Color.clear);
                }
            }
        }
    }
    public void HandleNewHighlight(int x, int y, bool enabled)
    {
        switch(currentlyHiglightedGemsCount)
        {
            case 0:
                if(!enabled) break;

                currentlyHiglightedGemsCount++;
                highlightedGems[0] = board[x,y];
                break;
            case 1:
                if(enabled)
                {
                    currentlyHiglightedGemsCount++;
                    highlightedGems[1] = board[x,y];
                    controllerReference.SendNewSwapToModel(
                        new Vector2Int(highlightedGems[0].PosX, highlightedGems[0].PosY),
                        new Vector2Int(highlightedGems[1].PosX, highlightedGems[1].PosY));

                    highlightedGems[0].TryHighlight();
                    highlightedGems[1].TryHighlight();
                    highlightedGems[0] = null;
                    highlightedGems[1] = null;
                    currentlyHiglightedGemsCount = 0;
                    break;
                }
                else{
                    currentlyHiglightedGemsCount--;
                    highlightedGems[0] = null;
                    break;
                }

            default:
                break;
        }
    }

    private Color GetColor(int index)
    {
        return colors[index];
    }
    private void CreateBoard()
    {
        board = new GemUI[rows, columns];

        // we compare the size of the board to size of gem's image to better fit it on screen
        Vector3 globalScale = new Vector3(
            (boardTransform.sizeDelta.x / (gemImageWidth * (float)rows)),
            (boardTransform.sizeDelta.y / (gemImageHeight * (float)columns)),
            0f);

        float targetScale = Mathf.Min(globalScale.x, globalScale.y);
        if(targetScale > 1f) targetScale = 1f;

        for(int y=0; y < columns; ++y)
        {
            for(int x=0; x < rows; ++x)
            {
                GameObject newGem = Instantiate(gemPrefab, Vector3.zero, Quaternion.identity);
                newGem.transform.parent = boardTransform;
                newGem.transform.name = x.ToString() + ":" + y.ToString();
                float gemScale = newGem.transform.localScale.x;
                newGem.transform.localScale = new Vector3(targetScale,targetScale,0f);
                newGem.transform.localPosition = new Vector3(
                    (x * gemImageWidth * gemScale), 
                    (-y * gemImageHeight * gemScale),
                    0f);
                board[x,y] = newGem.GetComponent<GemUI>();
                newGem.GetComponent<GemUI>().SetPosition(x,y);
            }
        }

        CenterCamera();
        ResizeCamera();
    }

    private void CenterCamera()
    {
        int rowCenter = Mathf.FloorToInt(rows/2);
        int columnCenter = Mathf.FloorToInt((columns)/2);

        float horizontalOffset = 0f;
        float verticalOffset = 0f;

        if(rows % 2 == 0)
        {
            horizontalOffset = -(gemImageWidth/2f);
        }
        if(columns % 2 == 0)
        {
            verticalOffset = (gemImageHeight/2f);
        }

        Vector3 centerPosition = new Vector3(
            board[rowCenter, columnCenter].transform.position.x + horizontalOffset,
            board[rowCenter,columnCenter].transform.position.y + verticalOffset,
            -10f);

        mainCamera.transform.position = centerPosition;
    }

    private void ResizeCamera()
    {
        // about 20 units for one gem
        int size = Mathf.Max(rows,columns);

        mainCamera.orthographicSize = 20f * size * 2.5f;
    }
}
