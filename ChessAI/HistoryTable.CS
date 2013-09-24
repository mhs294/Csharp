using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class HistoryTable
{
  // default constructor
  public HistoryTable()
  {
    // initialize value
    whiteValue = new int[64][];
    blackValue = new int[64][];
    for (int i = 0; i < 64; i++)
    {
      whiteValue[i] = new int[64];
      blackValue[i] = new int[64];
      for (int j = 0; j < 64; j++)
      {
        whiteValue[i][j] = 0;
        blackValue[i][j] = 0;
      }
    }
  }

  // orders the moves List so that the move with the greatest history table value is at the front
  public void OrderMoves(ref List<ChessMove> moves, int color, int start)
  {
    // initialize variables
    int index = start, max = 0;

    // iterate though moves to find move with highest history table value
    for (int i = start; i < moves.Count; i++)
    {
      if (color == ChessBoard.WHITE)
      {
        if (this.whiteValue[moves[i].GetFromSq()][moves[i].GetToSq()] > max)
        {
          index = i;
          max = this.whiteValue[moves[i].GetFromSq()][moves[i].GetToSq()];
        }
      }
      else if (color == ChessBoard.BLACK)
      {
        if (this.blackValue[moves[i].GetFromSq()][moves[i].GetToSq()] > max)
        {
          index = i;
          max = this.blackValue[moves[i].GetFromSq()][moves[i].GetToSq()];
        }
      }
    }

    // if move with higher history table value was found, move it to front of moves List
    if (index > start)
    {
      ChessMove temp = moves[start];
      moves[start] = moves[index];
      moves[index] = temp;
    }
  }

  // 2D array of move values
  public int[][] whiteValue;
  public int[][] blackValue;
}
