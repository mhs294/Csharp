using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class SEE
{
  // checks to see if capture move is profitable; if not, eliminate move from generated moves array
  public static void EvaluateCapture(ref List<ChessMove> moves, int player)
  {
    // get standing pat score of capturing move
    ChessMove theMove = moves[moves.Count - 1];
    int see_score = StaticExchangeEvaluate(theMove, player);

    // if standing pat score is less than the minimum required score, eliminate move from moves list
    if (see_score < MIN_CAPTURE_SCORE)
      moves.RemoveAt(moves.Count - 1);
    else if (see_score >= MIN_CAPTURE_SCORE)
    {
      if (AI.board.myColor == ChessBoard.WHITE)
        AI.history.whiteValue[theMove.GetFromSq()][theMove.GetToSq()] += see_score;
      else if (AI.board.myColor == ChessBoard.BLACK)
        AI.history.blackValue[theMove.GetFromSq()][theMove.GetToSq()] += see_score;
    }
  }

  // returns standing pat (static exchange evalutation) score of specified capturing move
  public static int StaticExchangeEvaluate(ChessMove move, int player)
  {
    // variables to track loss values
    int white_loss = 0, black_loss = 0, next_loss = 0;
    int wKing_loss = 0, bKing_loss = 0;

    // flags to check if either king is participating in attack
    bool wKingAttack = false, bKingAttack = false;

    // get current occupied BitBoard
    UInt64 occupied = AI.board.occupied_squares; 

    // get target square of attack
    uint target = move.GetToSq();

    // add piece on target square to color's material loss
    if (ChessBoard.GetPieceColor(move.GetCapture()) == ChessBoard.WHITE)
      white_loss += Score.GetPieceValue(move.GetCapture());
    else if (ChessBoard.GetPieceColor(move.GetCapture()) == ChessBoard.BLACK)
      black_loss += Score.GetPieceValue(move.GetCapture());

    // set next loss value equal to value of piece making initial capture
    next_loss = Score.GetPieceValue(move.GetPiece());

    // remove piece making capture from occupied BitBoard
    occupied ^= ChessBoard.SQUARES[move.GetFromSq()];

    // keep track of next player to move
    int playerToMove = AI.GetOtherPlayer(player);

    // loop until no more pieces are attacking the target square
    while (true)
    {
      // if either king is participating in an attack illegally, undo last capture, break loop
      if (wKingAttack && playerToMove == ChessBoard.WHITE)
      {
        black_loss -= wKing_loss;
        break;
      }
      else if (bKingAttack && playerToMove == ChessBoard.BLACK)
      {
        white_loss -= bKing_loss;
        break;
      }

      // reset attackers BitBoard
      UInt64 attackers = occupied;

      // check for attacking pieces of player to move
      if (playerToMove == ChessBoard.WHITE)
      {
        //--------------------//
        //  WHITE PAWNS (NP)  //
        //--------------------//

        // check for non-promoting white pawns
        if (ChessMove.GetRank(target) != 7)
        {
          // get all non-promoting white pawns attacking target square
          attackers = occupied & (ChessBoard.PAWN_CAPTURES[target] >> 16) & AI.board.white_pawn;

          // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
          if (attackers != ChessBoard.EMPTY_BOARD)
          {
            // update loss values
            black_loss += next_loss;
            next_loss = Score.GetPieceValue(ChessBoard.W_PAWN);

            // update occupied BitBoard
            occupied ^= ChessBoard.GetFirstBit(attackers);

            // switch player to move
            playerToMove = AI.GetOtherPlayer(playerToMove);

            // restart loop
            continue;
          }
        }

        //--------------------//
        //  WHITE KNIGHTS     //
        //--------------------//

        // get all white knights attacking target square
        attackers = occupied & (MoveGen.GetKnightMoves(target, occupied)) & AI.board.white_knight;

        // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          black_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.W_KNIGHT);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  WHITE BISHOPS     //
        //--------------------//

        // get all white bishops attacking target square
        attackers = occupied & (MoveGen.GetBishopMoves(target, occupied, occupied)) & AI.board.white_bishop;

        // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          black_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.W_BISHOP);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  WHITE ROOKS       //
        //--------------------//

        // get all white rooks attacking target square
        attackers = occupied & (MoveGen.GetRookMoves(target, occupied, occupied)) & AI.board.white_rook;

        // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          black_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.W_ROOK);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  WHITE PAWNS (P)   //
        //--------------------//

        // check for promoting white pawns
        if (ChessMove.GetRank(target) == 7)
        {
          // get all promoting white pawns attacking target square
          attackers = occupied & (ChessBoard.PAWN_CAPTURES[target - 16]) & AI.board.white_pawn;

          // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
          if (attackers != ChessBoard.EMPTY_BOARD)
          {
            // update loss values
            black_loss += next_loss;
            next_loss = Score.GetPieceValue(ChessBoard.W_QUEEN) - Score.GetPieceValue(ChessBoard.W_PAWN);

            // update occupied BitBoard
            occupied ^= ChessBoard.GetFirstBit(attackers);

            // switch player to move
            playerToMove = AI.GetOtherPlayer(playerToMove);

            // restart loop
            continue;
          }
        }

        //--------------------//
        //  WHITE QUEENS      //
        //--------------------//

        // get all white queens attacking target square
        attackers = occupied & (MoveGen.GetQueenMoves(target, occupied, occupied)) & AI.board.white_queen;

        // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          black_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.W_QUEEN);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  WHITE KING        //
        //--------------------//

        // get white king attacking target square
        attackers = occupied & (MoveGen.GetKingMoves(target, occupied)) & AI.board.white_king;

        // if attacking piece exists, update black loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          black_loss += next_loss;
          wKing_loss = next_loss;

          // set white king attack flag
          wKingAttack = true;

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }
      }
      else if (playerToMove == ChessBoard.BLACK)
      {
        //--------------------//
        //  BLACK PAWNS (NP)  //
        //--------------------//

        // check for non-promoting black pawns
        if (ChessMove.GetRank(target) != 0)
        {
          // get all non-promoting white pawns attacking target square
          attackers = occupied & (ChessBoard.PAWN_CAPTURES[target]) & AI.board.black_pawn;

          // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
          if (attackers != ChessBoard.EMPTY_BOARD)
          {
            // update loss values
            white_loss += next_loss;
            next_loss = Score.GetPieceValue(ChessBoard.B_PAWN);

            // update occupied BitBoard
            occupied ^= ChessBoard.GetFirstBit(attackers);

            // switch player to move
            playerToMove = AI.GetOtherPlayer(playerToMove);

            // restart loop
            continue;
          }
        }

        //--------------------//
        //  BLACK KNIGHTS     //
        //--------------------//

        // get all black knights attacking target square
        attackers = occupied & (MoveGen.GetKnightMoves(target, occupied)) & AI.board.black_knight;

        // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          white_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.B_KNIGHT);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  BLACK BISHOPS     //
        //--------------------//

        // get all black bishops attacking target square
        attackers = occupied & (MoveGen.GetBishopMoves(target, occupied, occupied)) & AI.board.black_bishop;

        // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          white_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.B_BISHOP);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  BLACK ROOKS       //
        //--------------------//

        // get all black rooks attacking target square
        attackers = occupied & (MoveGen.GetRookMoves(target, occupied, occupied)) & AI.board.black_rook;

        // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          white_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.B_ROOK);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  BLACK PAWNS (P)   //
        //--------------------//

        // check for promoting black pawns
        if (ChessMove.GetRank(target) == 0)
        {
          // get all promoting black pawns attacking target square
          attackers = occupied & (ChessBoard.PAWN_CAPTURES[target]) & AI.board.black_pawn;

          // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
          if (attackers != ChessBoard.EMPTY_BOARD)
          {
            // update loss values
            white_loss += next_loss;
            next_loss = Score.GetPieceValue(ChessBoard.B_QUEEN) - Score.GetPieceValue(ChessBoard.B_PAWN);

            // update occupied BitBoard
            occupied ^= ChessBoard.GetFirstBit(attackers);

            // switch player to move
            playerToMove = AI.GetOtherPlayer(playerToMove);

            // restart loop
            continue;
          }
        }

        //--------------------//
        //  BLACK QUEENS      //
        //--------------------//

        // get all black queens attacking target square
        attackers = occupied & (MoveGen.GetQueenMoves(target, occupied, occupied)) & AI.board.black_queen;

        // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          white_loss += next_loss;
          next_loss = Score.GetPieceValue(ChessBoard.B_QUEEN);

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }

        //--------------------//
        //  BLACK KING        //
        //--------------------//

        // get black king attacking target square
        attackers = occupied & (MoveGen.GetKingMoves(target, occupied)) & AI.board.black_king;

        // if attacking piece exists, update white loss, next loss, and occupied BitBoard; repeat loop
        if (attackers != ChessBoard.EMPTY_BOARD)
        {
          // update loss values
          white_loss += next_loss;
          bKing_loss = next_loss;

          // set white king attack flag
          bKingAttack = true;

          // update occupied BitBoard
          occupied ^= ChessBoard.GetFirstBit(attackers);

          // switch player to move
          playerToMove = AI.GetOtherPlayer(playerToMove);

          // restart loop
          continue;
        }
      }

      // no more attacking pieces - break loop
      break;
    }
    
    // return net standing pat score based on player color
    if (player == ChessBoard.WHITE)
      return black_loss - white_loss;
    else if (player == ChessBoard.BLACK)
      return white_loss - black_loss;

    // something went wrong...
    return -1;
  }

  public const int MIN_CAPTURE_SCORE = 1;    // minimum standing pat score required for "good" capture
}
