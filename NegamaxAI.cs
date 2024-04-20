using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NegamaxAI : AIScript {
    uint maxDepth = 4;

    private float[,] spaceValues = {
        // Corners are super valuable
        // Spaces right next to corners are dangerous (see below)  
        // Edges are pretty decent
        // Rest get less valuable towards the center
        {999,   0, 100, 100, 100, 100,   0, 999},
        {  0,   0,   6,   6,   6,   6,   0,   0},
        {100,   6,   2,   2,   2,   2,   6, 100},
        {100,   6,   2,   1,   1,   2,   6, 100},
        {100,   6,   2,   1,   1,   2,   6, 100},
        {100,   6,   2,   2,   2,   2,   6, 100},
        {  0,   0,   6,   6,   6,   6,   0,   0},
        {999,   0, 100, 100, 100, 100,   0, 999}
    };

    protected int countSpaces(BoardSpace[][] currentBoard, BoardSpace spaceType) {
        return currentBoard.SelectMany(row => row).Count(space => space.Equals(spaceType));
    }

    protected BoardSpace[][] copyBoard(BoardSpace[][] oldBoard) {
        return oldBoard.Select(a => a.ToArray()).ToArray();
    }

    protected float staticEvaluationFunction(BoardSpace[][] currentBoard, uint turnNumber) {
        float blackMod = turnNumber % 2 == 0 ? 1 : -1;
        float whiteMod = turnNumber % 2 == 1 ? 1 : -1;

        float rawSpaces = 0.0f;
        for (int y = 0; y < 8; y++) {
            for (int x = 0; x < 8; x++) {
                rawSpaces += (currentBoard[y][x] == BoardSpace.BLACK ? 1.0f : 0.0f) * blackMod * spaceValues[x,y];
                rawSpaces += (currentBoard[y][x] == BoardSpace.WHITE ? 1.0f : 0.0f) * whiteMod * spaceValues[x,y];
            }
        }

        float opponentMoves = BoardScript.GetValidMoves(currentBoard, turnNumber + 1).Count;
        
        return rawSpaces * 1.0f + opponentMoves * -10.0f;
    } 

    //main recursive negamax function
    private KeyValuePair<float, KeyValuePair<int, int>>
        NegamaxFunction(BoardSpace[][] currentBoard, uint currentDepth) {
        uint turnNumber = (uint)currentBoard.Sum(row => row.Sum(tile => tile == BoardSpace.EMPTY ? 0 : 1));
        List<KeyValuePair<int, int>> currentValidMoves = BoardScript.GetValidMoves(currentBoard, turnNumber);

        //if game is over we are done recursing
        if (currentValidMoves.Count == 0 || currentDepth == maxDepth) {
            KeyValuePair<int, int> finalMove = new KeyValuePair<int, int>(-1, -1);
            return new KeyValuePair<float, KeyValuePair<int, int>>(staticEvaluationFunction(currentBoard, turnNumber), finalMove);
        }

        KeyValuePair<int, int> bestMove;
        float bestScore = Mathf.NegativeInfinity;

        //loop through all possible moves
        foreach (KeyValuePair<int, int> move in currentValidMoves) {
            BoardSpace[][] newBoard = copyBoard(currentBoard);
            SimulateMove(ref newBoard, move.Value, move.Key, turnNumber);

            //recurse
            KeyValuePair<float, KeyValuePair<int, int>> recursionResult = NegamaxFunction(newBoard, currentDepth + 1);

            float currentScore = -1 * recursionResult.Key;

            //if you found a new better score update score and move
            if (currentScore > bestScore) {
                bestScore = currentScore;
                bestMove = move;
            }
        }

        return new KeyValuePair<float, KeyValuePair<int, int>>(bestScore, bestMove);
    }

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves,
        BoardSpace[][] currentBoard) {
        return NegamaxFunction(currentBoard, 0).Value;
    }

//from 
    protected void SimulateMove(ref BoardSpace[][] currentBoard, int x, int y, uint turnNumber) {
        if (turnNumber % 2 == 0) {
            currentBoard[y][x] = BoardSpace.BLACK;
        }
        else {
            currentBoard[y][x] = BoardSpace.WHITE;
        }

        List<KeyValuePair<int, int>> changedSpaces =
            BoardScript.GetPointsChangedFromMove(currentBoard, turnNumber, x, y);
        foreach (KeyValuePair<int, int> space in changedSpaces) {
            if (turnNumber % 2 == 0) {
                currentBoard[space.Key][space.Value] = BoardSpace.BLACK;
            }
            else {
                currentBoard[space.Key][space.Value] = BoardSpace.WHITE;
            }
        }

        ++turnNumber;
    }
}