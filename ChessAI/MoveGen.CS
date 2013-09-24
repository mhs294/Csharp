using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MoveGen
{
  // returns a List of all pseudo-legal ChessMoves for a given player
  public static List<ChessMove> GenerateMoves(int player, bool unquietOnly)
  {
    // initialize moves List
    List<ChessMove> moves = new List<ChessMove>(0);
    UInt64 currentPiece = 0, targetBitBoard = 0, tempMove = 0;
    GameState state = AI.states[AI.states.Count - 1];
    uint piece = ChessBoard.EMPTY;

    // iterate through piece BitBoards and generate moves
    if (player == ChessBoard.WHITE) // white to move
    {
      // set targetBitBoard to squares white can move to
      if (!unquietOnly)
        targetBitBoard = AI.board.empty_squares | AI.board.black_pieces;     
      else if (unquietOnly)
        targetBitBoard = AI.board.black_pieces;        

      //--------------------//
      //  WHITE PAWNS       //
      //--------------------//

      // set piece and currentPiece BitBoard
      currentPiece = AI.board.white_pawn;
      piece = ChessBoard.W_PAWN;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = ChessBoard.PAWN_ADVANCES[fromSq] & AI.board.empty_squares;

        // if generating only unquiet moves, set target bit board to only promoting squares
        if (unquietOnly)
          tempMove &= 0xFF00000000000000;
        else
        {
          // check for double push
          if (GetRank(fromSq) == 1 && tempMove != 0)
            tempMove |= ((ChessBoard.PAWN_ADVANCES[fromSq] << 8) & AI.board.empty_squares);
        }

        // add capture moves
        tempMove |= ChessBoard.PAWN_CAPTURES[fromSq] & AI.board.black_pieces;

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // if promotion is possible, add all possible promotion moves; else add default move
          if (GetRank(toSq) == 7)
          {
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.W_QUEEN));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.W_ROOK));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.W_BISHOP));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.W_KNIGHT));
          }
          else
          {
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

            // if move is a capturing move, run standing pat evaluation
            if (capt != 0)
              SEE.EvaluateCapture(ref moves, player);
          }

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // check for en passant capture
        if (state.enPassant != -1)
        {
          if ((ChessBoard.PAWN_CAPTURES[fromSq] & ChessBoard.SQUARES[state.enPassant]) != 0)
            moves.Add(new ChessMove(fromSq, (uint)state.enPassant, piece, ChessBoard.B_PAWN, ChessBoard.W_PAWN));
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  WHITE KNIGHTS     //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.W_KNIGHT;
      currentPiece = AI.board.white_knight;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetKnightMoves(fromSq, targetBitBoard);
        
        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  WHITE BISHOPS     //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.W_BISHOP;
      currentPiece = AI.board.white_bishop;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetBishopMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  WHITE ROOKS       //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.W_ROOK;
      currentPiece = AI.board.white_rook;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetRookMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  WHITE QUEENS      //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.W_QUEEN;
      currentPiece = AI.board.white_queen;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetQueenMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  WHITE KING        //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.W_KING;
      currentPiece = AI.board.white_king;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetKingMoves(fromSq, targetBitBoard);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // if not generating only unquiet moves, check for castling moves
        if (!unquietOnly)
        {
          // check for 0-0 (king's side castling)
          if (state.wCanCastle00 && (ChessBoard.FG_MASK & AI.board.occupied_squares) == 0)
          {
            if (!IsAttacked(ChessBoard.EG_MASK, ChessBoard.BLACK))
            {
              moves.Add(new ChessMove(4, 6, piece, ChessBoard.EMPTY, ChessBoard.W_KING));
            }
          }

          // check for 0-0-0 (queen's side castling)
          if (state.wCanCastle000 && (ChessBoard.BD_MASK & AI.board.occupied_squares) == 0)
          {
            if (!IsAttacked(ChessBoard.CE_MASK, ChessBoard.BLACK))
            {
              moves.Add(new ChessMove(4, 2, piece, ChessBoard.EMPTY, ChessBoard.W_KING));
            }
          }
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }
    }
    else if (player == ChessBoard.BLACK) // black to move
    {
      // set targetBitBoard to squares black can move to
      if (unquietOnly)
        targetBitBoard = AI.board.white_pieces;
      else if (!unquietOnly)
        targetBitBoard = AI.board.empty_squares | AI.board.white_pieces;

      //--------------------//
      //  BLACK PAWNS       //
      //--------------------//

      // set currentPiece BitBoard
      currentPiece = AI.board.black_pawn;
      piece = ChessBoard.B_PAWN;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // add standard moves
        tempMove = (ChessBoard.PAWN_ADVANCES[fromSq] >> 16) & AI.board.empty_squares;

        // if generating only unquiet moves, set target bit board to only promoting squares
        if (unquietOnly)
          tempMove &= 0x00000000000000FF;
        else
        {
          // check for double push
          if (GetRank(fromSq) == 6 && tempMove != 0)
            tempMove |= ((ChessBoard.PAWN_ADVANCES[fromSq] >> 24) & AI.board.empty_squares);
        }

        // add capture moves
        tempMove |= (ChessBoard.PAWN_CAPTURES[fromSq] >> 16) & AI.board.white_pieces;

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // if promotion is possible, generate all possible promotion moves
          if (GetRank(toSq) == 0)
          {
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.B_QUEEN));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.B_ROOK));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.B_BISHOP));
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.B_KNIGHT));
          }
          else
          {
            moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

            // if move is a capturing move, run standing pat evaluation
            if (capt != 0)
              SEE.EvaluateCapture(ref moves, player);
          }

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // check for en passant capture
        if (state.enPassant != -1)
        {
          if (((ChessBoard.PAWN_CAPTURES[fromSq] >> 16) & ChessBoard.SQUARES[state.enPassant]) != 0)
            moves.Add(new ChessMove(fromSq, (uint)state.enPassant, piece, ChessBoard.W_PAWN, ChessBoard.B_PAWN));
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  BLACK KNIGHTS     //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.B_KNIGHT;
      currentPiece = AI.board.black_knight;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetKnightMoves(fromSq, targetBitBoard);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  BLACK BISHOPS     //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.B_BISHOP;
      currentPiece = AI.board.black_bishop;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetBishopMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  BLACK ROOKS       //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.B_ROOK;
      currentPiece = AI.board.black_rook;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetRookMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  BLACK QUEENS      //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.B_QUEEN;
      currentPiece = AI.board.black_queen;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetQueenMoves(fromSq, targetBitBoard, AI.board.occupied_squares);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }

      //--------------------//
      //  BLACK KING        //
      //--------------------//

      // set piece and currentPiece BitBoard
      piece = ChessBoard.B_KING;
      currentPiece = AI.board.black_king;

      // loop until all pieces have been checked
      while (currentPiece != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(currentPiece);

        // create standard moves BitBoard
        tempMove = GetKingMoves(fromSq, targetBitBoard);

        // loop until all destinations for currentPiece have been checked
        while (tempMove != 0)
        {
          // get destination square
          uint toSq = GetNextSquare(tempMove);

          // get capture piece type
          uint capt = AI.board.theBoard[toSq];

          // add move
          moves.Add(new ChessMove(fromSq, toSq, piece, capt, ChessBoard.EMPTY));

          // if move is a capturing move, run standing pat evaluation
          if (capt != 0)
            SEE.EvaluateCapture(ref moves, player);

          // remove toSq from tempMove BitBoard
          tempMove ^= ChessBoard.SQUARES[toSq];
        }

        // if not generating only unquiet moves, check for castling moves
        if (!unquietOnly)
        {
          // check for 0-0 (king's side castling)
          if (state.bCanCastle00 && (ChessBoard.FG_MASK & AI.board.occupied_squares) == 0)
          {
            if (!IsAttacked(ChessBoard.EG_MASK, ChessBoard.WHITE))
            {
              moves.Add(new ChessMove(60, 62, piece, ChessBoard.EMPTY, ChessBoard.B_KING));
            }
          }

          // check for 0-0-0 (queen's side castling)
          if (state.bCanCastle000 && (ChessBoard.BD_MASK & AI.board.occupied_squares) == 0)
          {
            if (!IsAttacked(ChessBoard.CE_MASK, ChessBoard.WHITE))
            {
              moves.Add(new ChessMove(60, 58, piece, ChessBoard.EMPTY, ChessBoard.B_KING));
            }
          }
        }

        // remove fromSq from currentPiece BitBoard
        currentPiece ^= ChessBoard.SQUARES[fromSq];
      }    
    }

    // return moves List
    return moves;
  }

  // returns true if any of the squares represented by the bitmask are attacked, false otherwise
  public static bool IsAttacked(UInt64 targetBitBoard, int attackPlayer)
  {
    if (attackPlayer == ChessBoard.WHITE) // check if targets are attacked by white
    {
      // loop until all targets have been checked
      while (targetBitBoard != 0)
      {
        // get target square
        uint toSq = GetNextSquare(targetBitBoard);

        // check for pawn attacks
        if ((toSq / 8) > 1 && ((ChessBoard.PAWN_CAPTURES[toSq % 8] << (int)(((toSq / 8) - 2) * 8)) & AI.board.white_pawn) != 0) return true;

        // check for king attakcs
        if ((ChessBoard.KING_MOVES[toSq] & AI.board.white_king) != 0) return true;

        // check for knight attacks
        if ((ChessBoard.KNIGHT_MOVES[toSq] & AI.board.white_knight) != 0) return true;

        // check for bishop attacks
        UInt64 bishops = AI.board.white_bishop;
        while (bishops != 0)
        {
          // get origin square
          uint fromSq = GetNextSquare(bishops);

          // check for bishop attack from origin square to target square
          UInt64 bishopTargets = AI.board.empty_squares | AI.board.black_pieces;
          if ((GetBishopMoves(fromSq, bishopTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

          // remove fromSq from bishops BitBoard
          bishops ^= ChessBoard.SQUARES[fromSq];
        }

        // check for rook attacks
        UInt64 rooks = AI.board.white_rook;
        while (rooks != 0)
        {
          // get origin square
          uint fromSq = GetNextSquare(rooks);

          // check for rook attack from origin square to target square
          UInt64 rookTargets = AI.board.empty_squares | AI.board.black_pieces;
          if ((GetRookMoves(fromSq, rookTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

          // remove fromSq from rooks BitBoard
          rooks ^= ChessBoard.SQUARES[fromSq];
        }

        // check for queen attacks
        UInt64 queens = AI.board.white_queen;
        while (queens != 0)
        {
          // get origin square
          uint fromSq = GetNextSquare(queens);

          // check for queen attack from origin square to target square
          UInt64 queenTargets = AI.board.empty_squares | AI.board.black_pieces;
          if ((GetQueenMoves(fromSq, queenTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

          // remove fromSq from queens BitBoard
          queens ^= ChessBoard.SQUARES[fromSq];
        }

        // remove toSq from target BitBoard
        targetBitBoard ^= ChessBoard.SQUARES[toSq];
      }
    }
    else if (attackPlayer == ChessBoard.BLACK) // check if targets are attacked by black
    {
      // get target square
      uint toSq = GetNextSquare(targetBitBoard);

      // check for pawn attacks
      if ((toSq / 8) < 6 && ((ChessBoard.PAWN_CAPTURES[toSq]) & AI.board.black_pawn) != 0) return true;

      // check for king attakcs
      if ((ChessBoard.KING_MOVES[toSq] & AI.board.black_king) != 0) return true;

      // check for knight attacks
      if ((ChessBoard.KNIGHT_MOVES[toSq] & AI.board.black_knight) != 0) return true;

      // check for bishop attacks
      UInt64 bishops = AI.board.black_bishop;
      while (bishops != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(bishops);

        // check for bishop attack from origin square to target square
        UInt64 bishopTargets = AI.board.empty_squares | AI.board.white_pieces;
        if ((GetBishopMoves(fromSq, bishopTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

        // remove fromSq from bishops BitBoard
        bishops ^= ChessBoard.SQUARES[fromSq];
      }

      // check for rook attacks
      UInt64 rooks = AI.board.black_rook;
      while (rooks != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(rooks);

        // check for rook attack from origin square to target square
        UInt64 rookTargets = AI.board.empty_squares | AI.board.white_pieces;
        if ((GetRookMoves(fromSq, rookTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

        // remove fromSq from rooks BitBoard
        rooks ^= ChessBoard.SQUARES[fromSq];
      }

      // check for queen attacks
      UInt64 queens = AI.board.black_queen;
      while (queens != 0)
      {
        // get origin square
        uint fromSq = GetNextSquare(queens);

        // check for queen attack from origin square to target square
        UInt64 queenTargets = AI.board.empty_squares | AI.board.white_pieces;
        if ((GetQueenMoves(fromSq, queenTargets, AI.board.occupied_squares) & targetBitBoard) != 0) return true;

        // remove fromSq from queens BitBoard
        queens ^= ChessBoard.SQUARES[fromSq];
      }

      // remove toSq from target BitBoard
      targetBitBoard ^= ChessBoard.SQUARES[toSq];
    }

    // targets are not under attack
    return false;
  }

  // returns true if the specified king is under attack, false otherwise
  public static bool IsKingAttacked(int player)
  {
    // initalize variables
    int attackPlayer = AI.GetOtherPlayer(player);
    UInt64 targetBitBoard = 0;
    if (player == ChessBoard.WHITE)
      targetBitBoard = AI.board.white_king;
    else if (player == ChessBoard.BLACK)
      targetBitBoard = AI.board.black_king;

    // return IsAttacked on specified king's BitBoard
    return IsAttacked(targetBitBoard, attackPlayer);
  }

  // returns a BitBoard containing all legal moves for a Knight
  public static UInt64 GetKnightMoves(uint sq, UInt64 targetBitBoard)
  {
    // all possible squares not occupied by same-color pieces
    return ChessBoard.KNIGHT_MOVES[sq] & targetBitBoard;
  }

  // returns a BitBoard containing all legal moves for a Bishop
  public static UInt64 GetBishopMoves(uint sq, UInt64 targetBitBoard, UInt64 occupiedBitBoard)
  {
    // generate NE moves
    UInt64 ur_moves = ChessBoard.UP_RIGHT_MOVES[sq] & occupiedBitBoard;
    ur_moves = (ur_moves << 9) | (ur_moves << 18) | (ur_moves << 27) | (ur_moves << 36) | (ur_moves << 45) | (ur_moves << 54);
    ur_moves &= ChessBoard.UP_RIGHT_MOVES[sq];
    ur_moves = targetBitBoard & (ur_moves ^ ChessBoard.UP_RIGHT_MOVES[sq]);

    // generate NW moves
    UInt64 ul_moves = ChessBoard.UP_LEFT_MOVES[sq] & occupiedBitBoard;
    ul_moves = (ul_moves << 7) | (ul_moves << 14) | (ul_moves << 21) | (ul_moves << 28) | (ul_moves << 35) | (ul_moves << 42);
    ul_moves &= ChessBoard.UP_LEFT_MOVES[sq];
    ul_moves = targetBitBoard & (ul_moves ^ ChessBoard.UP_LEFT_MOVES[sq]);

    // generate SW moves
    UInt64 dl_moves = ChessBoard.DOWN_LEFT_MOVES[sq] & occupiedBitBoard;
    dl_moves = (dl_moves >> 9) | (dl_moves >> 18) | (dl_moves >> 27) | (dl_moves >> 36) | (dl_moves >> 45) | (dl_moves >> 54);
    dl_moves &= ChessBoard.DOWN_LEFT_MOVES[sq];
    dl_moves = targetBitBoard & (dl_moves ^ ChessBoard.DOWN_LEFT_MOVES[sq]);

    // generate SE moves
    UInt64 dr_moves = ChessBoard.DOWN_RIGHT_MOVES[sq] & occupiedBitBoard;
    dr_moves = (dr_moves >> 7) | (dr_moves >> 14) | (dr_moves >> 21) | (dr_moves >> 28) | (dr_moves >> 35) | (dr_moves >> 42);
    dr_moves &= ChessBoard.DOWN_RIGHT_MOVES[sq];
    dr_moves = targetBitBoard & (dr_moves ^ ChessBoard.DOWN_RIGHT_MOVES[sq]);

    // combine to form bishop moves
    return ur_moves | ul_moves | dl_moves | dr_moves;
  }

  // returns a BitBoard containing all legal moves for a Rook
  public static UInt64 GetRookMoves(uint sq, UInt64 targetBitBoard, UInt64 occupiedBitBoard)
  {
    // generate E moves
    UInt64 r_moves = ChessBoard.RIGHT_MOVES[sq] & occupiedBitBoard;
    r_moves = (r_moves << 1) | (r_moves << 2) | (r_moves << 3) | (r_moves << 4) | (r_moves << 5) | (r_moves << 6);
    r_moves &= ChessBoard.RIGHT_MOVES[sq];
    r_moves = targetBitBoard & (r_moves ^ ChessBoard.RIGHT_MOVES[sq]);

    // generate W moves
    UInt64 l_moves = ChessBoard.LEFT_MOVES[sq] & occupiedBitBoard;
    l_moves = (l_moves >> 1) | (l_moves >> 2) | (l_moves >> 3) | (l_moves >> 4) | (l_moves >> 5) | (l_moves >> 6);
    l_moves &= ChessBoard.LEFT_MOVES[sq];
    l_moves = targetBitBoard & (l_moves ^ ChessBoard.LEFT_MOVES[sq]);

    // generate N moves
    UInt64 u_moves = ChessBoard.UP_MOVES[sq] & occupiedBitBoard;
    u_moves = (u_moves << 8) | (u_moves << 16) | (u_moves << 24) | (u_moves << 32) | (u_moves << 40) | (u_moves << 48);
    u_moves &= ChessBoard.UP_MOVES[sq];
    u_moves = targetBitBoard & (u_moves ^ ChessBoard.UP_MOVES[sq]);

    // generate S moves
    UInt64 d_moves = ChessBoard.DOWN_MOVES[sq] & occupiedBitBoard;
    d_moves = (d_moves >> 8) | (d_moves >> 16) | (d_moves >> 24) | (d_moves >> 32) | (d_moves >> 40) | (d_moves >> 48);
    d_moves &= ChessBoard.DOWN_MOVES[sq];
    d_moves = targetBitBoard & (d_moves ^ ChessBoard.DOWN_MOVES[sq]);

    // combine to form rook moves
    return r_moves | l_moves | u_moves | d_moves;
  }

  // returns a BitBoard containing all legal moves for a Queen
  public static UInt64 GetQueenMoves(uint sq, UInt64 targetBitBoard, UInt64 occupiedBitBoard)
  {
    // queen = bishop + rook
    return GetBishopMoves(sq, targetBitBoard, occupiedBitBoard) | GetRookMoves(sq, targetBitBoard, occupiedBitBoard);
  }

  // returns a BitBoard containing all legal moves for a King
  public static UInt64 GetKingMoves(uint sq, UInt64 targetBitBoard)
  {
    // all possible squares not occupied by same-color pieces
    return ChessBoard.KING_MOVES[sq] & targetBitBoard;
  }

  // returns the square number of the next positive bit in bitmask
  public static uint GetNextSquare(UInt64 bitmask)
  {
    for (int i = 0; i < 64; i++)
    {
      if (((bitmask >> i) & 0x0000000000000001) == 0x0000000000000001)
        return (uint)i;
    }

    // something went wrong...
    Console.WriteLine("ERROR - Next Square = uint.MaxValue");
    return uint.MaxValue;
  }

  // returns the rank containing the square
  public static uint GetRank(uint square)
  {
    return square / 8;
  }

  // returns the file containing the square
  public static uint GetFile(uint square)
  {
    return square % 8;
  }
}
