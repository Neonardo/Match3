using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    [SerializeField] GameController controllerReference;
    
    private int[,] gemsGrid;
    private int width, height;
    private int numberOfColors;
    private System.Random randomGenerator;
    private Queue<Vector2Int[]> horizontalMatchesFound;
    private Queue<Vector2Int[]> verticalMatchesFound;

    private Queue<Vector2Int[]> sectorsToRefill;

    private const float boardRefreshCooldown = 0.2f; // board refresh in seconds

    private enum State{
        Uninitialized,
        WaitngForInput,
        SwapingGems,
        ChekingBoardState,
        ScoreCombinations,
        RefillingEmptySlots,
    }

    private State currentState = State.Uninitialized;
    private float timer;

    private void Update()
    {
        if(timer > 0f)
        {
            timer -= Time.deltaTime;
            return;
        }

        switch(currentState)
        {
            case State.Uninitialized:
            case State.WaitngForInput:
            case State.SwapingGems:
                break;
            case State.ChekingBoardState:
                CheckBoardState();
                if( horizontalMatchesFound.Count == 0 &&
                    verticalMatchesFound.Count == 0)
                {
                    ChangeState(State.WaitngForInput);
                }
                else{
                    ChangeState(State.ScoreCombinations);
                }
                break;
            case State.ScoreCombinations:
                if(horizontalMatchesFound.Count > 0)
                {
                    DeleteFirstMatchInRow();
                }
                else if(verticalMatchesFound.Count > 0)
                {
                    DeleteFirstMatchInColumn();
                }
                else{
                    ChangeState(State.RefillingEmptySlots);
                }
                break;
            case State.RefillingEmptySlots:
                if(sectorsToRefill.Count > 0)
                {
                    Vector2Int[] sector = sectorsToRefill.Dequeue();
                    FillEmptySector(sector[0], sector[1]);
                    timer = boardRefreshCooldown;
                }
                else{
                    ChangeState(State.ChekingBoardState);
                }
                break;
            default:
                Debug.LogError("No current state, shouldn't occur");
                break;
        }
    }

    public void Initialize(int width, int height, int numberOfColors, ref System.Random randomGenerator)
    {
        horizontalMatchesFound = new Queue<Vector2Int[]>(2);
        verticalMatchesFound = new Queue<Vector2Int[]>(2);
        sectorsToRefill = new Queue<Vector2Int[]>(2);

        this.width = width;
        this.height = height;
        this.numberOfColors = numberOfColors;
        this.randomGenerator = randomGenerator;

        gemsGrid = new int[width,height];

        CreateInitialBoard();
    }

    private void ChangeState(State newState)
    {
        if(currentState == newState)
        {
            Debug.LogError("Cant change to the same state");
            return;
        }

        // allow game view to change with every change of game state
        controllerReference.SendNewBoardToView(gemsGrid);
        currentState = newState;
        // we set our timer to allow for non-instantaneous movement of the gems
        timer = boardRefreshCooldown;
    }

    private void CreateInitialBoard()
    {
        for(int y=0; y<height; ++y)
        {
            for(int x=0; x<width; ++x)
            {
                gemsGrid[x,y] = randomGenerator.Next() % (numberOfColors);
            }
        }

        ChangeState(State.ChekingBoardState);
    }

    public bool TrySwapGems(Vector2Int firstGemPosition, Vector2Int secondGemPosition)
    {
        // don't allow swaping in incorrect states
        if(currentState != State.WaitngForInput) return false;

        
        // check if valid move
        if( AreGemsSwapable(firstGemPosition, secondGemPosition) )
        {
            ChangeState(State.SwapingGems);

            SwapGems(firstGemPosition, secondGemPosition);
            CheckBoardState();

            if(horizontalMatchesFound.Count > 0 || verticalMatchesFound.Count > 0)
            {
                ChangeState(State.ChekingBoardState);
                return true;
            }
            else{
                SwapGems(firstGemPosition, secondGemPosition);
                ChangeState(State.WaitngForInput);
                return false;
            }
        }
        else{
            return false;
        }
    }

    private void SwapGems(Vector2Int firstGemPosition, Vector2Int secondGemPosition)
    {
        int firstGemValue = gemsGrid[firstGemPosition.x, firstGemPosition.y];
        int secondGemValue = gemsGrid[secondGemPosition.x, secondGemPosition.y];

        gemsGrid[firstGemPosition.x, firstGemPosition.y] = secondGemValue;
        gemsGrid[secondGemPosition.x, secondGemPosition.y] = firstGemValue;
    }

    private void CheckBoardState()
    {
        //check rows
        for(int i=0; i<height; ++i)
        {
            CheckRow(i);
        }

        //check columns
        for(int i=0; i<width; ++i)
        {
            CheckColumn(i);
        }
    }

    private void CheckRow(int index)
    { 
        int firstGemIndex = 0;
        int lastGemIndex = -1;
        int currentColor = gemsGrid[0,index];
        int consecutiveCount = 1;

        for(int i=1; i<width; ++i)
        {
            if(gemsGrid[i,index] == currentColor)
            {
                consecutiveCount++;
                lastGemIndex = i;

                if(i == width-1)
                {
                    if(consecutiveCount >= 3)
                    {
                        // we found combination at the end of the row so we add it to the queue
                        horizontalMatchesFound.Enqueue( new Vector2Int[]
                            {new Vector2Int(firstGemIndex, index), new Vector2Int(lastGemIndex,index)} );
                    }
                }
            }
            else
            {
                if(consecutiveCount >= 3)
                {
                    // we place last matching combination in queue  before reseting gem count
                    horizontalMatchesFound.Enqueue( new Vector2Int[]
                        {new Vector2Int(firstGemIndex, index), new Vector2Int(lastGemIndex,index)} );
                }

                firstGemIndex = i;
                lastGemIndex = -1;
                consecutiveCount = 1;
                currentColor = gemsGrid[i,index];
            }
        }
    }

    private void CheckColumn(int index)
    {
        int firstGemIndex = 0;
        int lastGemIndex = -1;
        int currentColor = gemsGrid[index,0];
        int consecutiveCount = 1;

        for(int i=1; i<height; ++i)
        {
            if(gemsGrid[index,i] == currentColor)
            {
                consecutiveCount++;
                lastGemIndex = i;

                if(i == height-1)
                {
                    if(consecutiveCount >= 3)
                    {
                        // we found combination at the end of the column so we add it to the queue
                        verticalMatchesFound.Enqueue( new Vector2Int[]
                            {new Vector2Int(index, firstGemIndex), new Vector2Int(index, lastGemIndex)} );
                    }
                }
            }
            else
            {
                if(consecutiveCount >= 3)
                {
                    // we place last matching combination in queue  before reseting gem count
                    verticalMatchesFound.Enqueue( new Vector2Int[]
                        {new Vector2Int(index, firstGemIndex), new Vector2Int(index, lastGemIndex)} );
                }

                firstGemIndex = i;
                lastGemIndex = -1;
                consecutiveCount = 1;
                currentColor = gemsGrid[index,i];
            }
        }
    }

    private void FillEmptySector(Vector2Int firstGem, Vector2Int lastGem)
    {
        //check in which way we will be filling missing gems
        //we fill horizontaly
        if(firstGem.x-lastGem.x != 0)
        {
            for(int i=firstGem.x; i<lastGem.x+1; ++i)
            {
                FillEmptyGem(new Vector2Int(i,firstGem.y));
            }
        }
        // we fill verticly
        else{
            for(int i=firstGem.y; i<lastGem.y+1; ++i)
            {
                FillEmptyGem(new Vector2Int(firstGem.x, i));
            }
        }

    }

    private void FillEmptyGem(Vector2Int gemPosition)
    {
        if(gemsGrid[gemPosition.x, gemPosition.y] != -1)
        {
            Debug.Log("Space is not empty");
            return;
        }

        // we search positions above our gem to find what can fall to this place
        for(int i = gemPosition.y; i >= 0; --i)
        {
            if(gemsGrid[gemPosition.x, i] != -1)
            {
                gemsGrid[gemPosition.x,gemPosition.y] = gemsGrid[gemPosition.x,i];
                gemsGrid[gemPosition.x,i] = -1;
                break;
            }
        }

        //if we didn't find gems to slot above, we create new one
        if(gemsGrid[gemPosition.x, gemPosition.y] == -1)
        {
            gemsGrid[gemPosition.x, gemPosition.y] = randomGenerator.Next() % numberOfColors;
        }
        
        // we recursively search for newly created gaps
        for(int i = gemPosition.y-1; i >= 0; --i)
        {
            if(gemsGrid[gemPosition.x, i] == -1)
            {
                FillEmptyGem(new Vector2Int(gemPosition.x, i));
                break;
            }
        }
    }

    private bool AreGemsSwapable(Vector2Int firstGemPosition, Vector2Int secondGemPosition)
    {
        // we check if one of the gems is a direct neighbour of the second
        return ( (firstGemPosition + Vector2Int.right) == secondGemPosition ||
            (firstGemPosition - Vector2Int.right) == secondGemPosition ||
            (firstGemPosition + Vector2Int.up) == secondGemPosition ||
            (firstGemPosition - Vector2Int.up) == secondGemPosition);
    }

    private void DeleteFirstMatchInRow()
    {
        if(horizontalMatchesFound.Count == 0) return;

        Vector2Int[] match = horizontalMatchesFound.Dequeue();

        int gemsToDestroyCount = (match[1].x - match[0].x) + 1;
        int rowIndex = match[0].y;

        for(int i = match[0].x; i < (match[0].x + gemsToDestroyCount); ++i)
        {
            gemsGrid[i, rowIndex] = -1; // we mark destroyed gem with -1 value
        }

        sectorsToRefill.Enqueue(match);
    }

    private void DeleteFirstMatchInColumn()
    {
        if(verticalMatchesFound.Count == 0) return;

        Vector2Int[] match = verticalMatchesFound.Dequeue();

        int gemsToDestroyCount = (match[1].y - match[0].y) + 1;
        int columnIndex = match[0].x;

        for(int i = match[0].y; i < (match[0].y + gemsToDestroyCount); ++i)
        {
            gemsGrid[columnIndex, i] = -1; // we mark destroyed gem with -1 value
        }
        
        sectorsToRefill.Enqueue(match);
    }



/// Debug methods for printing various data to console
    private void DebugPrintBoard()
    {
        string debug = "";
        for(int y=0; y<height; ++y)
        {
            for(int x=0; x<width; ++x)
            {
                debug += gemsGrid[x, y] + " ";
            }
            debug += "\n";
        }

        Debug.Log(debug);
    }

    private void DebugRowsToScore()
    {
        string debug = "";
        Vector2Int[][] matches = horizontalMatchesFound.ToArray();
        for(int i=0; i<horizontalMatchesFound.Count; ++i)
        {
            debug += matches[i][0] + "-" + matches[i][1] + "\n"; 
        }

        Debug.Log(debug);
    }

    private void DebugColumnsToScore()
    {
        string debug = "";
        Vector2Int[][] matches = verticalMatchesFound.ToArray();
        for(int i=0; i<verticalMatchesFound.Count; ++i)
        {
            debug += matches[i][0] + "-" + matches[i][1] + "\n"; 
        }
    }
}
