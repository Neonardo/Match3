using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private int boardWidth;
    [SerializeField] private int boardHeight;
    [Range(2,6)] [SerializeField] private int numberOfColors;
    [SerializeField] private int seed = 0;

    [SerializeField] private GameView gameView;
    [SerializeField] private GameModel gameModel;

    private System.Random randomGenerator;

    private int[,] currentBoard;
    private Vector2Int newSwap;

    private void Start() {
        randomGenerator = new System.Random(seed);

        gameView.Initiallize(boardWidth, boardHeight);
        gameModel.Initialize(boardWidth, boardHeight, numberOfColors, ref randomGenerator);
    }

    public void SetCurrentBoard(int[,] board)
    {
        currentBoard = board;

        SendNewBoardToView(currentBoard);
    }

    public void SendNewBoardToView(int[,] board)
    {
        gameView.FillBoard(board);
    }

    public void SendNewSwapToModel(Vector2Int firstGem, Vector2Int secondGem)
    {
        bool ableToSwap = gameModel.TrySwapGems(firstGem, secondGem);
    }
}
