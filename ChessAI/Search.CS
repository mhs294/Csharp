using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

class Search
{
  // executes time-limited alpha/beta pruning depth-limited MiniMax search
  public static int PVABMiniMax(int depth, int alpha, int beta)
  {
    // initialize principal variant flag
    bool pvfound = false;

		// update evaluated node count
		AI.nodes++;
	
    // check to see if we have time left
    if (AI.timer.ElapsedMilliseconds > TIME_PER_MOVE)
    {
      AI.TIME_EXPIRED = true;
      return Score.TIME_EXPIRED_SCORE;
    }

    // initialize variables
    List<ChessMove> moves = new List<ChessMove>(0);
    ChessMove bestAction = new ChessMove();
    int value = SMALL_NUM, bestValue = SMALL_NUM, color = -1;
    if (depth % 2 != 0)
      bestValue = LARGE_NUM;

    // check for draws
    if (Score.IsDrawByHundredMoves()) // 100 move draw; update history table
      return Score.DRAW_SCORE;
    if (Score.IsDrawByRepetition()) // repitition draw
      return Score.DRAW_SCORE;
    if (Score.IsDrawByInsufficientMaterial()) // insufficient material draw
      return Score.DRAW_SCORE;

    // set color of current player
    if (depth % 2 == 0)
      color = AI.board.myColor;
    else
      color = AI.board.oppColor;

    // if depth limit has been reached, return quiescence search value for this node
    if (depth >= MAX_DEPTH)
    {
      // set follow pv flag
      FOLLOW_PV = false;

      // go one node deeper if player to move is currently in check
      if (MoveGen.IsKingAttacked(color))
      {
        MAX_DEPTH++;
        value = PVABMiniMax(depth, alpha, beta);
        MAX_DEPTH--;
        return value;
      }

      // return quiescence search value
      return QSearch(alpha, beta, color);
    }

    // if allowed, try a null move to get an early prune
    if (NULLMOVE_ALLOWED && !FOLLOW_PV)
    {
      // if game could be in zugzwang for player to move or player to move is in check, do not attempt null move
      if (((color == ChessBoard.WHITE && Score.GetMaterialScore(color) > Score.NULLMOVE_LIMIT_SCORE) ||
          (color == ChessBoard.BLACK && Score.GetMaterialScore(color) > Score.NULLMOVE_LIMIT_SCORE)) &&
          !MoveGen.IsKingAttacked(color))
      {
        // set allow null move flag
        NULLMOVE_ALLOWED = false;

        // if we are next player to move, run zero-window search for alpha; else use beta
        if (color == AI.board.oppColor)
          value = PVABMiniMax(depth + NULLMOVE_DEPTH_GAP, alpha, alpha + 1);
        else if (color == AI.board.myColor)
          value = PVABMiniMax(depth + NULLMOVE_DEPTH_GAP, beta, beta + 1);

        // reset allow null move flag
        NULLMOVE_ALLOWED = true;

        // if alpha/beta was not improved, prune
        if (color == AI.board.myColor && value >= beta)
          return value;
        else if (color == AI.board.oppColor && value <= alpha)
          return value;
      }
    }
    NULLMOVE_ALLOWED = true;

    // generate moves for player based on depth
    moves = MoveGen.GenerateMoves(color, false);

    // iterate through generated moves
    for (int i = 0; i < moves.Count; i++)
    {
      // order remaining moves according to history table
      AI.history.OrderMoves(ref moves, color, i);

      // make current move
      moves[i].DoMove(MAKE);

      // if move is illegal, unmake move; continue otherwise
      if (!MoveGen.IsKingAttacked(color))
      {
        // if principal variant flag is set, run zero-window search; else, run normal search
        if (pvfound)
        {
          // if we are next player to move, use alpha for zero-window search; else, use beta
          if (depth % 2 != 0)
            value = PVABMiniMax(depth + 1, alpha, alpha + 1);
          else if (depth % 2 == 0)
            value = PVABMiniMax(depth + 1, beta, beta + 1);

          // if value returned falls within alpha/beta window, run normal search with normal alpha/beta window
          if (value > alpha && value < beta)
            value = PVABMiniMax(depth + 1, alpha, beta);
        }
        else
          value = PVABMiniMax(depth + 1, alpha, beta);

        // unmake current move
        moves[i].DoMove(UNMAKE);

				// check to see if search found checkmate
				if (value == Score.CHECKMATE_WIN_SCORE && depth == 0)
				{
          // update PV
          PV.Clear();
          PV.Add(moves[i]);

          // return checkmate score
	  			return Score.CHECKMATE_WIN_SCORE;
				}

        // check to see if time has expired
        if (value == Score.TIME_EXPIRED_SCORE)
          return Score.TIME_EXPIRED_SCORE;        

        // evaluate minimax search value
        if (depth % 2 == 0) // maximize
        {
          if (value >= beta) // fail-high, prune; update history table
          {
            if (color == ChessBoard.WHITE)
              AI.history.whiteValue[moves[i].GetFromSq()][moves[i].GetToSq()] += (MAX_DEPTH - depth) * (MAX_DEPTH - depth);
            else if (color == ChessBoard.BLACK)
              AI.history.blackValue[moves[i].GetFromSq()][moves[i].GetToSq()] += (MAX_DEPTH - depth) * (MAX_DEPTH - depth);
            return value + 1;
          }
          if (value > alpha) // set new alpha, set principal variant flag
          {
            alpha = value;
            pvfound = true;
          }
	  			if (value >= bestValue) // set new best action, best value
          {
            // if alpha improves at root, update PV
            if (depth == 0 && value > bestValue)
            {
              PV.Clear();
              PV.Add(moves[i]);
            }
            else if (depth == 0 && value == bestValue)
              PV.Add(moves[i]);

            bestAction = moves[i];
            bestValue = value;
	  			}          
        }
        else if (depth % 2 == 1) // minimize
        {
          if (value <= alpha) // fail-low, prune; update history table
          {
            if (color == ChessBoard.WHITE)
              AI.history.whiteValue[moves[i].GetFromSq()][moves[i].GetToSq()] += (MAX_DEPTH - depth) * (MAX_DEPTH - depth);
            else if (color == ChessBoard.BLACK)
              AI.history.blackValue[moves[i].GetFromSq()][moves[i].GetToSq()] += (MAX_DEPTH - depth) * (MAX_DEPTH - depth);
            return value - 1;
          }
          if (value < beta) // set new beta, set principal variant flag
          {
            beta = value;
            pvfound = true;
          }
          if (value < bestValue) // set new best action, best value
          {
            bestAction = moves[i];
            bestValue = value;
          }
        }
      }
      else
        moves[i].DoMove(UNMAKE);
    }

    // no legal moves for this state
    if (value == SMALL_NUM)
    {
      // if in check, checkmate; else stalemate
      if (MoveGen.IsKingAttacked(color))
      {
        if (color == AI.board.myColor) // we are in checkmate
          return Score.CHECKMATE_LOSE_SCORE;
        else if (color == AI.board.oppColor) // opp is in checkmate
          return Score.CHECKMATE_WIN_SCORE;
      }
      else
        return Score.DRAW_SCORE;
    }

    // return best value from current depth; update history table
    if (color == ChessBoard.WHITE)
      AI.history.whiteValue[bestAction.GetFromSq()][bestAction.GetToSq()] += ((MAX_DEPTH - depth) * (MAX_DEPTH - depth) + 1);
    else if (color == ChessBoard.BLACK)
      AI.history.blackValue[bestAction.GetFromSq()][bestAction.GetToSq()] += ((MAX_DEPTH - depth) * (MAX_DEPTH - depth) + 1);
    return bestValue;
  }

  // executes quiescence search
  public static int QSearch(int alpha, int beta, int player)
  {
    // initialize variables
    int value = SMALL_NUM, bestValue = SMALL_NUM;
    List<ChessMove> moves = new List<ChessMove>(0);
    if (player != AI.board.myColor)
      bestValue = LARGE_NUM;

    // check to see if we have time left
    if (AI.timer.ElapsedMilliseconds > TIME_PER_MOVE)
    {
      AI.TIME_EXPIRED = true;
      return Score.TIME_EXPIRED_SCORE;
    }

    // check for draw by insufficient material
    if (Score.IsDrawByInsufficientMaterial())
      return Score.DRAW_SCORE;

    // update evaluated node count
    AI.nodes++;
    
    // generate all capturing and promoting moves (all unquiet moves)
    moves = MoveGen.GenerateMoves(player, true);

    // iterate through generated moves
    for (int i = 0; i < moves.Count; i++)
    {
      // make current move
      moves[i].DoMove(MAKE);

      // if current move is illegal, unmake move; continue otherwise
      if (!MoveGen.IsKingAttacked(player))
      {
        // get value of next quiescence search recursion
        value = QSearch(alpha, beta, AI.GetOtherPlayer(player));

        // check to see if time has expired
        if (value == Score.TIME_EXPIRED_SCORE)
          return Score.TIME_EXPIRED_SCORE;

        // unmake current move
        moves[i].DoMove(UNMAKE);

        // evaluate quiescence search value
        if (player == AI.board.myColor) // maximize
        {
          if (value >= beta) // fail-high, prune
            return value + 1;
          if (value > alpha) // set new alpha
            alpha = value;
          if (value > bestValue) // set new best action, best value
            bestValue = value;
        }
        else if (player == AI.board.oppColor) // minimize
        {
          if (value <= alpha) // fail-low, prune
            return value - 1;
          if (value < beta) // set new beta
            beta = value;
          if (value < bestValue) // set new best action, best value
            bestValue = value;
        }
      }
      else
        moves[i].DoMove(UNMAKE);
    }

    // if no capturing or promoting moves for this state, set best value to node's heuristic value
    if (value == SMALL_NUM)
      bestValue = Score.Evaluate(AI.board.myColor);

    // return best value from the current depth
    return bestValue;
  }

  // sets the search time limit based on the current game state
  public static void UpdateTimePerMove(int moves)
  {
    // if less than 5 moves have been made, we are in the opening (3s per move)
    if (moves < 5)
      TIME_PER_MOVE = 3000;
    else
    {
      // if we are in the endgame, think at least 10 seconds; else, think at least 6 seconds
      if (Score.IsEndGame(Score.GetMaterialScore(ChessBoard.WHITE), Score.GetMaterialScore(ChessBoard.BLACK)))
        TIME_PER_MOVE = 6000 + (int)(AI.timeRemaining * 3.5); // 6s + 0.35% of remaining time
      else
	      TIME_PER_MOVE = 5000 + (int)(AI.timeRemaining * 3.5); // 4s + 0.35% of remaining time
    }
  }

  // search variables
  public static List<ChessMove> PV;         // principal variation
  public static int MAX_DEPTH;              // search depth limit
  public static long TIME_PER_MOVE;         // time limit for search function
  public static bool FOLLOW_PV;             // true if search is following principal variant, false otherwise
  public static bool NULLMOVE_ALLOWED;      // true if search can make null move, false otherwise

  // search constants
  public const bool MAKE = false;
  public const bool UNMAKE = true;
  public const int SMALL_NUM = -999999;
  public const int LARGE_NUM = 999999;
  public const int NULLMOVE_DEPTH_GAP = 5;
}
