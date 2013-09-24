using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// stores a move with detailed information
class ChessMove
{
  // default constructor
  public ChessMove()
  {
    move = NULL_MOVE;
  }

  // copy constructor
  public ChessMove(ChessMove cm)
  {
    this.move = cm.move;
  }

  // standard constructor
  public ChessMove(uint fromSq, uint toSq, uint pieceMoved, uint pieceCaptured, uint promotionType)
  {
    // perform bit-shifts to align mask elements
    toSq <<= 6;
    pieceMoved <<= 12;
    pieceCaptured <<= 16;
    promotionType <<= 20;

    // merge mask elements to form move mask
    this.move = fromSq | toSq | pieceMoved | pieceCaptured | promotionType;
  }

  // modifies the ChessBoard and GameState List to reflect the specified move (undo move if unmake = true)
  public void DoMove(bool unmake)
  {
    // initialize variables
    uint fromSq = GetFromSq();
    uint toSq = GetToSq();
    uint piece = GetPiece();
    uint capture = GetCapture();
    uint promote = GetPromote();
    UInt64 fromBitBoard = ChessBoard.SQUARES[fromSq];
    UInt64 fromToBitBoard = fromBitBoard | ChessBoard.SQUARES[toSq];

    // set destination/origin square to empty based on unmake flag
    if (unmake)
      AI.board.theBoard[toSq] = ChessBoard.EMPTY;
    else
      AI.board.theBoard[fromSq] = ChessBoard.EMPTY;

    // set destination/origin square to piece based on unmake flag
    if (unmake)
      AI.board.theBoard[fromSq] = piece;
    else
      AI.board.theBoard[toSq] = piece;

    // determine piece type and color
    uint type = piece & 0x00000007;
    switch (type)
    {
      case ChessMove.PAWN:        

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_PAWN)
        {
          // update white_pawn and white_pieces BitBoards
          AI.board.white_pawn ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;          

          // check for a white en passant capture
          if (capture == ChessBoard.B_PAWN && promote == ChessBoard.W_PAWN)
          {
            // update black_pawn, black_pieces, and occupied_squares BitBoards
            AI.board.black_pawn ^= ChessBoard.SQUARES[toSq - 8];
            AI.board.black_pieces ^= ChessBoard.SQUARES[toSq - 8];
            AI.board.occupied_squares ^= fromToBitBoard | ChessBoard.SQUARES[toSq - 8];

            // set en passant square to black pawn/empty based on unmake flag
            if (unmake)
              AI.board.theBoard[toSq - 8] = ChessBoard.B_PAWN;
            else
              AI.board.theBoard[toSq - 8] = ChessBoard.EMPTY;
          }
        }
        else if (piece == ChessBoard.B_PAWN)
        {
          // update black_pawn and black_pieces BitBoards
          AI.board.black_pawn ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;

          // check for a black en passant capture
          if (capture == ChessBoard.W_PAWN && promote == ChessBoard.B_PAWN)
          {
            // update black_pawn, black_pieces, and occupied_squares BitBoards
            AI.board.white_pawn ^= ChessBoard.SQUARES[toSq + 8];
            AI.board.white_pieces ^= ChessBoard.SQUARES[toSq + 8];
            AI.board.occupied_squares ^= fromToBitBoard | ChessBoard.SQUARES[toSq + 8];

            // set en passant square to white pawn/empty based on unmake flag
            if (unmake)
              AI.board.theBoard[toSq + 8] = ChessBoard.W_PAWN;
            else
              AI.board.theBoard[toSq + 8] = ChessBoard.EMPTY;
          }
        }        

        // check for a pawn promotion
        if (promote != 0 && promote != ChessBoard.W_PAWN && promote != ChessBoard.W_KING && 
            promote != ChessBoard.B_PAWN && promote != ChessBoard.B_KING)
        {
          // update BitBoards for promotion
          DoPromotion(promote, toSq);

          // set destination square to promotion piece type
          AI.board.theBoard[toSq] = promote;
        }
        break;

      case ChessMove.KING: 

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_KING)
        {
          // update white_king and white_pieces BitBoards
          AI.board.white_king ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;

          // check for castling move
          if (promote == ChessBoard.W_KING)
          {
            // set rookFromSq and rookToSq
            int rookFromSq = -1, rookToSq = -1;
            if (GetFile(toSq) == 6) // 0-0
            {              
              rookFromSq = 7;
              rookToSq = 5;              
            }
            else if (GetFile(toSq) == 2) // 0-0-0
            {
              rookFromSq = 0;
              rookToSq = 3;
            }

            // update white_rook, white_pieces, and occupied_squares BitBoards
            AI.board.white_rook ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];
            AI.board.white_pieces ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];
            AI.board.occupied_squares ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];

            // move rook on theBoard based on unmake flag
            if (unmake)
            {
              AI.board.theBoard[rookFromSq] = ChessBoard.W_ROOK;
              AI.board.theBoard[rookToSq] = ChessBoard.EMPTY;
            }
            else
            {
              AI.board.theBoard[rookFromSq] = ChessBoard.EMPTY;
              AI.board.theBoard[rookToSq] = ChessBoard.W_ROOK;
            }
          }
        }
        else if (piece == ChessBoard.B_KING)
        {
          // update black_king and black_pieces BitBoards
          AI.board.black_king ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;

          // check for castling move
          if (promote == ChessBoard.B_KING)
          {
            // set rookFromSq and rookToSq
            int rookFromSq = -1, rookToSq = -1;
            if (GetFile(toSq) == 6) // 0-0
            {
              rookFromSq = 63;
              rookToSq = 61;
            }
            else if (GetFile(toSq) == 2) // 0-0-0
            {
              rookFromSq = 56;
              rookToSq = 59;
            }

            // update black_rook, black_pieces, and occupied_squares BitBoards
            AI.board.black_rook ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];
            AI.board.black_pieces ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];
            AI.board.occupied_squares ^= ChessBoard.SQUARES[rookFromSq] | ChessBoard.SQUARES[rookToSq];

            // move rook on theBoard based on unmake flag
            if (unmake)
            {
              AI.board.theBoard[rookFromSq] = ChessBoard.B_ROOK;
              AI.board.theBoard[rookToSq] = ChessBoard.EMPTY;
            }
            else
            {
              AI.board.theBoard[rookFromSq] = ChessBoard.EMPTY;
              AI.board.theBoard[rookToSq] = ChessBoard.B_ROOK;
            }
          }
        }
        break;

      case ChessMove.ROOK:

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_ROOK)
        {
          // update white_rook and white_pieces BitBoards
          AI.board.white_rook ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;
        }
        else if (piece == ChessBoard.B_ROOK)
        {
          // update black_rook and black_pieces BitBoards
          AI.board.black_rook ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;
        }
        break;

      case ChessMove.KNIGHT:

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_KNIGHT)
        {
          // update white_knight and white_pieces BitBoards
          AI.board.white_knight ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;
        }
        else if (piece == ChessBoard.B_KNIGHT)
        {
          // update black_knight and black_pieces BitBoards
          AI.board.black_knight ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;
        }
        break;

      case ChessMove.BISHOP:

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_BISHOP)
        {
          // update white_bishop and white_pieces BitBoards
          AI.board.white_bishop ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;
        }
        else if (piece == ChessBoard.B_BISHOP)
        {
          // update black_bishop and black_pieces BitBoards
          AI.board.black_bishop ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;
        }
        break;

      case ChessMove.QUEEN:

        // modify ChessBoard data based on piece color
        if (piece == ChessBoard.W_QUEEN)
        {
          // update white_queen and white_pieces BitBoards
          AI.board.white_queen ^= fromToBitBoard;
          AI.board.white_pieces ^= fromToBitBoard;
        }
        else if (piece == ChessBoard.B_QUEEN)
        {
          // update black_queen and black_pieces BitBoards
          AI.board.black_queen ^= fromToBitBoard;
          AI.board.black_pieces ^= fromToBitBoard;
        }
        break;
    }

    // check for a normal piece capture
    if (capture != 0 && promote != ChessBoard.W_PAWN && promote != ChessBoard.B_PAWN)
    {
      // update BitBoards for capture
      DoCapture(capture, toSq, unmake);

      // update occupied_squares BitBoard
      AI.board.occupied_squares ^= fromBitBoard;
    }
    else
    {
      // update occupied_squares BitBoard
      AI.board.occupied_squares ^= fromToBitBoard;
    }

    // add/remove GameState to/from AI.states List based on unmake flag
    if (unmake)
      AI.states.RemoveAt(AI.states.Count - 1);
    else
      AI.states.Add(new GameState(AI.states[AI.states.Count - 1], this));
  }

  // modifies the ChessBoard to reflect a piece capture
  public void DoCapture(uint capture, uint toSq, bool unmake)
  {
    // initialize variables
    uint piece = capture & 0x00000007;
    UInt64 toBitMap = ChessBoard.SQUARES[toSq];

    // make capture based on piece type and color
    switch (piece)
    {
      case ChessMove.PAWN:
        if (capture == ChessBoard.W_PAWN)
        {
          AI.board.white_pieces ^= toBitMap;
          AI.board.white_pawn ^= toBitMap;          
        }
        else if (capture == ChessBoard.B_PAWN)
        {
          AI.board.black_pieces ^= toBitMap;
          AI.board.black_pawn ^= toBitMap;
        }
        break;

      case ChessMove.QUEEN:
        if (capture == ChessBoard.W_QUEEN)
        {
          AI.board.white_pieces ^= toBitMap;
          AI.board.white_queen ^= toBitMap;
        }
        else if (capture == ChessBoard.B_QUEEN)
        {
          AI.board.black_pieces ^= toBitMap;
          AI.board.black_queen ^= toBitMap;
        }
        break;

      case ChessMove.ROOK:
        if (capture == ChessBoard.W_ROOK)
        {
          AI.board.white_pieces ^= toBitMap;
          AI.board.white_rook ^= toBitMap;
        }
        else if (capture == ChessBoard.B_ROOK)
        {
          AI.board.black_pieces ^= toBitMap;
          AI.board.black_rook ^= toBitMap;
        }
        break;

      case ChessMove.BISHOP:
        if (capture == ChessBoard.W_BISHOP)
        {
          AI.board.white_pieces ^= toBitMap;
          AI.board.white_bishop ^= toBitMap;
        }
        else if (capture == ChessBoard.B_BISHOP)
        {
          AI.board.black_pieces ^= toBitMap;
          AI.board.black_bishop ^= toBitMap;
        }
        break;

      case ChessMove.KNIGHT:
        if (capture == ChessBoard.W_KNIGHT)
        {
          AI.board.white_pieces ^= toBitMap;
          AI.board.white_knight ^= toBitMap;
        }
        else if (capture == ChessBoard.B_KNIGHT)
        {
          AI.board.black_pieces ^= toBitMap;
          AI.board.black_knight ^= toBitMap;
        }
        break;
    }

    // if unmake is true, put piece back on theBoard
    if (unmake)
      AI.board.theBoard[toSq] = capture;
  }

  // modifies the ChessBoard to reflect a pawn promotion (undo if unmake = true)
  public void DoPromotion(uint promote, uint toSq)
  {
    // initialize variables
    uint piece = promote & 0x00000007;
    UInt64 toBitMap = ChessBoard.SQUARES[toSq];

    // make promotion based on piece type and color
    switch (piece)
    {
      case ChessMove.QUEEN:
        if (promote == ChessBoard.W_QUEEN)
        {
          AI.board.white_pawn ^= toBitMap;
          AI.board.white_queen ^= toBitMap;
        }
        else if (promote == ChessBoard.B_QUEEN)
        {
          AI.board.black_pawn ^= toBitMap;
          AI.board.black_queen ^= toBitMap;
        }
        break;

      case ChessMove.ROOK:
        if (promote == ChessBoard.W_ROOK)
        {
          AI.board.white_pawn ^= toBitMap;
          AI.board.white_rook ^= toBitMap;
        }
        else if (promote == ChessBoard.B_ROOK)
        {
          AI.board.black_pawn ^= toBitMap;
          AI.board.black_rook ^= toBitMap;
        }
        break;

      case ChessMove.BISHOP:
        if (promote == ChessBoard.W_BISHOP)
        {
          AI.board.white_pawn ^= toBitMap;
          AI.board.white_bishop ^= toBitMap;
        }
        else if (promote == ChessBoard.B_BISHOP)
        {
          AI.board.black_pawn ^= toBitMap;
          AI.board.black_bishop ^= toBitMap;
        }
        break;

      case ChessMove.KNIGHT:
        if (promote == ChessBoard.W_KNIGHT)
        {
          AI.board.white_pawn ^= toBitMap;
          AI.board.white_knight ^= toBitMap;
        }
        else if (promote == ChessBoard.B_KNIGHT)
        {
          AI.board.black_pawn ^= toBitMap;
          AI.board.black_knight ^= toBitMap;
        }
        break;
    }
  }

  // returns a ChessMove constructed from the input parameters
  public static ChessMove GetChessMove(int fromFile, int toFile, int fromRank, int toRank, int promoteTo, int enPassant)
  {
    // get fromSq and toSq
    uint fromSq = (uint)(8 * (fromRank - 1)) + (uint)(fromFile - 1);
    uint toSq = (uint)(8 * (toRank - 1)) + (uint)(toFile - 1);

    // get pieceMoved
    uint pieceMoved = AI.board.theBoard[fromSq];    

    // get promotionType
    uint promotionType = ChessBoard.EMPTY;
    switch ((char)promoteTo)
    {
      case 'Q':
        if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.WHITE)
          promotionType = ChessBoard.W_QUEEN;
        else if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.BLACK)
          promotionType = ChessBoard.B_QUEEN;
        break;

      case 'R':
        if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.WHITE)
          promotionType = ChessBoard.W_ROOK;
        else if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.BLACK)
          promotionType = ChessBoard.B_ROOK;
        break;

      case 'B':
        if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.WHITE)
          promotionType = ChessBoard.W_BISHOP;
        else if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.BLACK)
          promotionType = ChessBoard.B_BISHOP;
        break;

      case 'N':
        if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.WHITE)
          promotionType = ChessBoard.W_KNIGHT;
        else if (((pieceMoved & 0x00000008) >> 3) == ChessBoard.BLACK)
          promotionType = ChessBoard.B_KNIGHT;
        break;
    }

    // get pieceCaptured
    uint pieceCaptured = AI.board.theBoard[toSq];

    // check for en passant and update pieceCaptured/promotionType if needed
    if (pieceMoved == ChessBoard.W_PAWN && toSq == (uint)enPassant)
    {
      pieceCaptured = ChessBoard.B_PAWN;
      promotionType = ChessBoard.W_PAWN;
    }
    else if (pieceMoved == ChessBoard.B_PAWN && toSq == (uint)enPassant)
    {
      pieceCaptured = ChessBoard.W_PAWN;
      promotionType = ChessBoard.B_PAWN;
    }

    // check for castling and set promotion type if needed
    if (pieceMoved == ChessBoard.W_KING && fromSq == 4)
    {
      if (toSq == 6 || toSq == 2)
        promotionType = ChessBoard.W_KING;
    }
    else if (pieceMoved == ChessBoard.B_KING && fromSq == 60)
    {
      if (toSq == 62 || toSq == 58)
        promotionType = ChessBoard.B_KING;
    }

    // construct ChessMove and return it
    return new ChessMove(fromSq, toSq, pieceMoved, pieceCaptured, promotionType);
  }

  // returns the fromSq value from the move bitmask
  public uint GetFromSq()
  {
    return move & 0x0000003F;
  }

  // returns the toSq value from the move bitmask
  public uint GetToSq()
  {
    return (move & 0x00000FC0) >> 6;
  }

  // returns the piece value from the move bitmask
  public uint GetPiece()
  {
    return (move & 0x0000F000) >> 12;
  }

  // returns the capture value from the move bitmask
  public uint GetCapture()
  {
    return (move & 0x000F0000) >> 16;
  }

  // returns the promote value from the move bitmask
  public uint GetPromote()
  {
    return (move & 0x00F00000) >> 20;
  }

  // returns the promoteTo value (for the AI) from the promote value
  public int GetPromoteType()
  {
    // initialize promoteTo
    uint promoteTo = GetPromote() & 0x00000007;

    // return integer value based on promoteTo
    switch (promoteTo)
    {
      case ChessMove.QUEEN:
        return (int)'Q';
      case ChessMove.ROOK:
        return (int)'R';
      case ChessMove.BISHOP:
        return (int)'B';
      case ChessMove.KNIGHT:
        return (int)'N';
    }

    // no promoteTo value for move
    return 0;
  }

  // returns the file of the specified square
  public static uint GetFile(uint sq)
  {
    return sq % 8;
  }

  // returns the rank of the specified square
  public static uint GetRank(uint sq)
  {
    return sq / 8;
  }

  // returns the move in algebraic notation as a string
  public string GetMoveString()
  {
    // initialize variables
    string move_string = String.Empty;

    // build string from move data
    move_string += AI.pieceChars[this.GetPiece()].ToString();
    move_string += AI.files[this.GetFromSq() % 8].ToString();
    move_string += ((this.GetFromSq() / 8) + 1).ToString();
    move_string += "-";
    move_string += AI.files[GetFile(this.GetToSq())].ToString();
    move_string += (GetRank(this.GetToSq()) + 1).ToString();

    return move_string;
  }

  // returns the target square for en passant capture, -1 if no square exists
  public int GetEnPassant()
  {
    // check for enpassant and set target square
    if ((move & 0x00007000) == 0x00001000) // last piece moved was a pawn
    {
      if ((move & 0x00008000) == 0x00000000) // white pawn
      {
        if ((((move & 0x00000FC0) >> 6) - (move & 0x0000003F)) == 16) // move was a double pawn push
        {
          return (int)((move & 0x0000003F) + 8);
        }
      }
      else if ((move & 0x00008000) == 0x00008000) // black pawn
      {
        if (((move & 0x0000003F) - ((move & 0x00000FC0) >> 6)) == 16) // move was a double pawn push
        {
          return (int)((move & 0x0000003F) - 8);
        }
      }
    }

    // no en passant target square exists
    return -1;
  }

  // returns true if 0-0 is legal for white, false otherwise
  public bool GetWCastle00(bool previousFlag)
  {
    // 0-0 was not legal for white last time
    if (!previousFlag) return false;

    // white king has just moved
    if ((move & 0x0000F000) == 0x00002000) 
      return false;

    // white rook on h1 has just moved
    if ((move & 0x0000F000) == 0x00003000 && (move & 0x0000003F) == 7)
      return false;

    // white rook on h1 has just been captured
    if ((move & 0x000F0000) == 0x0003000 && ((move & 0x00000FC0) >> 6) == 7)
      return false;

    // 0-0 is legal for white
    return true;
  }

  // returns true if 0-0-0 is legal for white, false otherwise
  public bool GetWCastle000(bool previousFlag)
  {
    // 0-0-0 was not legal for white last time
    if (!previousFlag) return false;

    // white king has just moved
    if ((move & 0x0000F000) == 0x00002000)
      return false;

    // white rook on a1 has just moved
    if ((move & 0x0000F000) == 0x00003000 && (move & 0x0000003F) == 0)
      return false;

    // white rook on a1 has just been captured
    if ((move & 0x000F0000) == 0x0003000 && ((move & 0x00000FC0) >> 6) == 0)
      return false;

    // 0-0-0 is legal for white
    return true;
  }

  // returns true if 0-0 is legal for black, false otherwise
  public bool GetBCastle00(bool previousFlag)
  {
    // 0-0 was not legal for black last time
    if (!previousFlag) return false;

    // black king has just moved
    if ((move & 0x0000F000) == 0x0000A000)
      return false;

    // black rook on h8 has just moved
    if ((move & 0x0000F000) == 0x0000B000 && (move & 0x0000003F) == 63)
      return false;

    // black rook on h8 has just been captured
    if ((move & 0x000F0000) == 0x000B000 && ((move & 0x00000FC0) >> 6) == 63)
      return false;

    // 0-0 is legal for black
    return true;
  }

  // returns true if 0-0-0 is legal for black, false otherwise
  public bool GetBCastle000(bool previousFlag)
  {
    // 0-0-0 was not legal for black last time
    if (!previousFlag) return false;

    // black king has just moved
    if ((move & 0x0000F000) == 0x0000A000)
      return false;

    // black rook on a8 has just moved
    if ((move & 0x0000F000) == 0x0000B000 && (move & 0x0000003F) == 56)
      return false;

    // black rook on a8 has just been captured
    if ((move & 0x000F0000) == 0x000B000 && ((move & 0x00000FC0) >> 6) == 56)
      return false;

    // 0-0-0 is legal for black
    return true;
  }

  // returns true if move increments the 100-move draw count, false otherwise
  public uint GetHundredDrawUpdate(uint current)
  {    
    if ((move & 0x00007000) == 0x00001000) // last piece moved was pawn
    {
      return 0;
    }
    else if ((move & 0x00070000) != 0x00000000) // last move was a capturing move
    {
      return 0;
    }
    
    // move increments 100-move draw count
    return current + 1;
  } 

  // clears the value stored in move
  public void Clear()
  {
    move = NULL_MOVE;
  }

  //------------------------------------------------------//
  //                                                      //
  //   (unused)   prom   capt   piec    toSq     frSq     //
  //   00000000   0000   0000   0000   000000   000000    //
  //                                                      //
  // Piece values:                                        //
  // 1 = Pawn                                             //
  // 2 = King                                             //
  // 3 = Rook                                             //
  // 4 = Knight                                           //
  // 5 = Bishop                                           //
  // 6 = Queen                                            //
  // (Color is determined by MSB --> 0 = W, 1 = B         //
  //                                                      //
  // prom = promotion piece (P = en_passant, K = castle)  //
  // capt = captured piece                                //
  // piec = moved piece                                   //
  // toSq = destination square of move                    //
  // frSq = origin square of move                         //
  //                                                      //
  //------------------------------------------------------//

  public uint move; // stores the move data in a bitmask	

  // move static readonlyants
  public const uint NULL_MOVE = 0x00000000;

  // piece type constants
  public const uint EMPTY = 0x00000000;
  public const uint PAWN = 0x00000001;
  public const uint KING = 0x00000002;
  public const uint ROOK = 0x00000003;
  public const uint KNIGHT = 0x00000004;
  public const uint BISHOP = 0x00000005;
  public const uint QUEEN = 0x00000006;
}
