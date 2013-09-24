using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GameState
{
  // default constructor
  public GameState(GameState previousState, ChessMove lastMove)
  {
    if (previousState == null) // set GameState values for start of game
    {
      // initialize castling flags
      wCanCastle00 = true;
      wCanCastle000 = true;
      bCanCastle00 = true;
      bCanCastle000 = true;

      // check to see if we have first move
      if (lastMove == null)
      {
        // set default values
        enPassant = -1;
        hundredMoveDraw = 0;
        theLastMove = new ChessMove();
      }
      else
      {
        // get en passant target square
        enPassant = lastMove.GetEnPassant();

        // check for pawn advancement, pawn promotion, or capture since last GameState
        hundredMoveDraw = lastMove.GetHundredDrawUpdate(0);

        // update theLastMove
        theLastMove = lastMove;
      }
    }
    else // set GameState values for non-starting game position
    {
      // update castling flags
      wCanCastle00 = lastMove.GetWCastle00(previousState.wCanCastle00);
      wCanCastle000 = lastMove.GetWCastle000(previousState.wCanCastle000);
      bCanCastle00 = lastMove.GetBCastle00(previousState.bCanCastle00);
      bCanCastle000 = lastMove.GetBCastle000(previousState.bCanCastle000);

      // get en passant target square
      enPassant = lastMove.GetEnPassant();

      // check for pawn advancement, pawn promotion, or capture since last GameState
      hundredMoveDraw = lastMove.GetHundredDrawUpdate(previousState.hundredMoveDraw);

      // update theLastMove
      theLastMove = lastMove;
    }
   
    // set state FEN
    state_fen = AI.board.GetFENString();
  }

  // global game state information
  public ChessMove theLastMove;   // last move made before this GameState
  public bool wCanCastle00;       // true if 0-0 is legal for white, false otherwise
  public bool wCanCastle000;      // true if 0-0-0 is legal for white, false otherwise
  public bool bCanCastle00;       // true if 0-0 is legal for black, false otherwise
  public bool bCanCastle000;      // true if 0-0-0 is legal for black, false otherwise
  public int enPassant;           // target square for legal en passant move (-1 if no legal square)
  public uint hundredMoveDraw;    // number of moves since last capture or pawn movement
  public string state_fen;        // FEN represenation of current game board

  // yet to be implemented
  //public UInt64 state_hash;       // Zobrist hash of current game board
}
