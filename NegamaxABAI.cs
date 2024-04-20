using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NegamaxABAI : NegamaxAI {
    private const uint MaxDepth = 5;

    //main recursive negamax function
    private KeyValuePair<float, KeyValuePair<int, int>>
        NegamaxABFunction(BoardSpace[][] currentBoard, uint currentDepth, float alpha, float beta) {
        uint turnNumber = (uint)currentBoard.Sum(row => row.Sum(tile => tile == BoardSpace.EMPTY ? 0 : 1));
        List<KeyValuePair<int, int>> currentValidMoves = BoardScript.GetValidMoves(currentBoard, turnNumber);

        //if game is over we are done recursing
        if (currentValidMoves.Count == 0 || currentDepth == MaxDepth) {
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
            KeyValuePair<float, KeyValuePair<int, int>> recursionResult =
                NegamaxABFunction(newBoard, currentDepth + 1, -1 * beta, -1 * alpha);

            float currentScore = -1 * recursionResult.Key;

            //if you found a new better score update score and move
            if (currentScore > bestScore) {
                bestScore = currentScore;
                bestMove = move;
            }

            if (currentScore > alpha) {
                alpha = currentScore;
            }

            if (alpha >= beta) {
                break;
            }
        }

        return new KeyValuePair<float, KeyValuePair<int, int>>(bestScore, bestMove);
    }

    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves,
        BoardSpace[][] currentBoard) {
        return NegamaxABFunction(currentBoard, 0, Mathf.NegativeInfinity, Mathf.Infinity).Value;
    }
}