using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Score
{
  // returns the score for the player with the current AI.board state
  public static int Evaluate(int player)
  {
    // initialize variables
    int score = 0;
    int material = GetMaterialScore(player);
    int position = GetPositionScore(player);
    //int mobility = GetMobilityScore(player);

    // add net material score
    score += material;

    // add net position score
    score += position;

    // add net mobility score
    //score += mobility;

    // run improved heuristic calculations
    if (AI.IMPROVED_HEURISTIC)
    {
      // initialize variables
      int pawn_structure = GetPawnStructureScore(player);
      int king_safety = 0;
      if (!IsEndGame(GetMaterialScore(ChessBoard.WHITE), GetMaterialScore(ChessBoard.BLACK)))
        king_safety = GetKingSafetyScore(player);

      // add net pawn structure score
      score += pawn_structure;

      // add net king safety score
      score += king_safety;
    }   

    // return net score
    return score;
  }

  // returns the material score for the specified player
  public static int GetMaterialScore(int player)
  {
    // initialize variables
    int white_material = 0, black_material = 0, net_material = 0;
    int wPawns = ChessBoard.BitCount(AI.board.white_pawn);
    int wRooks = ChessBoard.BitCount(AI.board.white_rook);
    int wKnights = ChessBoard.BitCount(AI.board.white_knight);
    int wBishops = ChessBoard.BitCount(AI.board.white_bishop);
    int wQueens = ChessBoard.BitCount(AI.board.white_queen);
    int wTotal = wPawns + wRooks + wKnights + wBishops + wQueens;
    int bPawns = ChessBoard.BitCount(AI.board.black_pawn);
    int bRooks = ChessBoard.BitCount(AI.board.black_rook);
    int bKnights = ChessBoard.BitCount(AI.board.black_knight);
    int bBishops = ChessBoard.BitCount(AI.board.black_bishop);
    int bQueens = ChessBoard.BitCount(AI.board.black_queen);
    int bTotal = bPawns + bRooks + bKnights + bBishops + bQueens;
    UInt64 light_squares = 0x55AA55AA55AA55AA;
    UInt64 dark_squares = 0xAA55AA55AA55AA55;

    // calculate material score for both players
    white_material += PAWN_SCORE * wPawns;
    white_material += ROOK_SCORE * wRooks;
    white_material += KNIGHT_SCORE * wKnights;
    white_material += BISHOP_SCORE * wBishops;
    white_material += QUEEN_SCORE * wQueens;    
    black_material += PAWN_SCORE * bPawns;
    black_material += ROOK_SCORE * bRooks;
    black_material += KNIGHT_SCORE * bKnights;
    black_material += BISHOP_SCORE * bBishops;
    black_material += QUEEN_SCORE * bQueens;
    
    // add bonus for bishop pair
    if ((AI.board.white_bishop & light_squares) != ChessBoard.EMPTY && (AI.board.white_bishop & dark_squares) != ChessBoard.EMPTY)
      white_material += BONUS_BISHOP_PAIR;
    if ((AI.board.black_bishop & light_squares) != ChessBoard.EMPTY && (AI.board.black_bishop & dark_squares) != ChessBoard.EMPTY)
      black_material += BONUS_BISHOP_PAIR;

    // if one side is ahead on material, give bonus for pieces remaining on board
    if (player == ChessBoard.WHITE && (white_material + wTotal) > (black_material + bTotal))
    {
      white_material += (45 + (3 * wTotal));
      black_material += (6 * bTotal);
    }
    else if (player == ChessBoard.BLACK && (black_material + bTotal) > (white_material + wTotal))
    {
      black_material += (45 + (3 * bTotal));
      white_material += (6 * wTotal);
    }

    // calculate net material score
    if (player == ChessBoard.WHITE)
      net_material = white_material - black_material;
    else if (player == ChessBoard.BLACK)
      net_material = black_material - white_material;

    // return material score
    return net_material;
  }

  // returns the position score for the specified color
  public static int GetPositionScore(int player)
  {
    // initialize variables
    int white_position = 0, black_position = 0, net_position = 0;
    uint white_king_sq = MoveGen.GetNextSquare(AI.board.white_king);
    uint black_king_sq = MoveGen.GetNextSquare(AI.board.black_king);

    // loop through game AI.board squares and calculate position/distance scores for each piece
    for (int i = 0; i < 64; i++)
    {
      if (AI.board.theBoard[i] == ChessBoard.EMPTY) continue; // square is empty
      if (ChessBoard.GetPieceColor(AI.board.theBoard[i]) == ChessBoard.WHITE)
      {
        switch (AI.board.theBoard[i])
        {
          case ChessBoard.W_PAWN:
            white_position += PAWN_POSITION[i];
            white_position += PAWN_OPPONENT_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)), 
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            white_position += PAWN_OWN_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            break;
          case ChessBoard.W_ROOK:
            white_position += ROOK_POSITION[i];
            white_position += ROOK_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            break;
          case ChessBoard.W_KNIGHT:
            white_position += KNIGHT_POSITION[i];
            white_position += KNIGHT_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            break;
          case ChessBoard.W_BISHOP:
            white_position += BISHOP_POSITION[i];
            white_position += BISHOP_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            break;
          case ChessBoard.W_QUEEN:
            white_position += QUEEN_POSITION[i];
            white_position += QUEEN_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            break;
          case ChessBoard.W_KING:
            if (IsEndGame(GetMaterialScore(ChessBoard.WHITE), GetMaterialScore(ChessBoard.BLACK)))
              white_position += KING_ENDGAME_POSITION[i];
            else
              white_position += KING_POSITION[i];
            break;
        }
      }
      else if (ChessBoard.GetPieceColor(AI.board.theBoard[i]) == ChessBoard.BLACK)
      {
        switch (AI.board.theBoard[i])
        {
          case ChessBoard.B_PAWN:
            black_position += PAWN_POSITION[MIRROR[i]];
            black_position += PAWN_OPPONENT_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)), 
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            black_position += PAWN_OWN_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(black_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(black_king_sq)))];
            break;
          case ChessBoard.B_ROOK:
            black_position += ROOK_POSITION[MIRROR[i]];
            black_position += ROOK_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            break;
          case ChessBoard.B_KNIGHT:
            black_position += KNIGHT_POSITION[MIRROR[i]];
            black_position += KNIGHT_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            break;
          case ChessBoard.B_BISHOP:
            black_position += BISHOP_POSITION[MIRROR[i]];
            black_position += BISHOP_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            break;
          case ChessBoard.B_QUEEN:
            black_position += QUEEN_POSITION[MIRROR[i]];
            black_position += QUEEN_DISTANCE[Math.Min(Math.Abs((int)ChessMove.GetFile((uint)i) - (int)ChessMove.GetFile(white_king_sq)),
              Math.Abs((int)ChessMove.GetRank((uint)i) - (int)ChessMove.GetRank(white_king_sq)))];
            break;
          case ChessBoard.B_KING:
            if (IsEndGame(GetMaterialScore(ChessBoard.WHITE), GetMaterialScore(ChessBoard.BLACK)))
              black_position += KING_ENDGAME_POSITION[MIRROR[i]];
            else
              black_position += KING_POSITION[MIRROR[i]];
            break;
        }
      }
    }

    // calculate net position score
    if (player == ChessBoard.WHITE)
      net_position += white_position - black_position;
    else if (player == ChessBoard.BLACK)
      net_position += black_position - white_position;

    // return position score
    return net_position;
  }

  // returns the mobility score for the specified color
  public static int GetMobilityScore(int player)
  {
    // initialize variables
    int white_mobility = 0, black_mobility = 0, net_mobility = 0;

    // calculate mobility score for white
    List<ChessMove> white_moves = MoveGen.GenerateMoves(ChessBoard.WHITE, false);
    for (int i = 0; i < white_moves.Count; i++)
    {
      // add default value to white mobility score
      white_mobility += MOBILITY_SCORE;

      // if move is a capture, give a bonus to white mobility score
      if (white_moves[i].GetCapture() != ChessBoard.EMPTY)
        white_mobility += THREAT_SCORE;      
    }

    // calculate mobility score for black
    List<ChessMove> black_moves = MoveGen.GenerateMoves(ChessBoard.BLACK, false);
    for (int i = 0; i < black_moves.Count; i++)
    {
      // add default value to black mobility score
      black_mobility += MOBILITY_SCORE;

      // if move is a capture, give a bonus to black mobility score
      if (black_moves[i].GetCapture() != ChessBoard.EMPTY)
        black_mobility += THREAT_SCORE;
    }

    // calculate net mobility score
    if (player == ChessBoard.WHITE)
      net_mobility += white_mobility - black_mobility;
    else if (player == ChessBoard.BLACK)
      net_mobility += black_mobility - white_mobility;
 
    // return mobility score;
    return net_mobility;
  }

  // returns the pawn structure score for the specified color
  public static int GetPawnStructureScore(int player)
  {
    // initialize variables
    int white_pawn_structure = 0, black_pawn_structure = 0, net_pawn_structure = 0;
    UInt64 white_pawns = AI.board.white_pawn, black_pawns = AI.board.black_pawn;

    // calculate white pawn structure score
    while (white_pawns != ChessBoard.EMPTY_BOARD)
    {
      // get square of next pawn
      uint square = MoveGen.GetNextSquare(white_pawns);

      // check to see if pawn is a passed pawn
      if ((W_PASSED_PAWN[square] & AI.board.black_pawn) == ChessBoard.EMPTY_BOARD)
      {
        white_pawn_structure += BONUS_PASSED_PAWN;

        // check to see if rook is behind a passed pawn
        if ((MoveGen.GetRookMoves(square, AI.board.occupied_squares, AI.board.occupied_squares) ^ AI.board.white_rook) != ChessBoard.EMPTY_BOARD)
          white_pawn_structure += BONUS_ROOK_BEHIND_PASSED_PAWN;
      }

      // check to see if pawn is a doubled pawn
      if (((ChessBoard.UP_MOVES[square] | ChessBoard.DOWN_MOVES[square]) ^ ChessBoard.SQUARES[square]) != 0)
        white_pawn_structure += PENALTY_DOUBLED_PAWN;

      // check to see if pawn is an isolated pawn (if not isolated, check to see if pawn is a backward pawn)
      if ((W_ISOLATED_PAWN[square] & AI.board.white_pawn) == ChessBoard.EMPTY_BOARD)
        white_pawn_structure += PENALTY_ISOLATED_PAWN;
      else
      {
        if (((ChessBoard.PAWN_CAPTURES[square + 8] & AI.board.black_pawn) != ChessBoard.EMPTY_BOARD) &&
            (W_BACKWARD_PAWN[square] & AI.board.white_pawn) == ChessBoard.EMPTY_BOARD)
          white_pawn_structure += PENALTY_BACKWARD_PAWN;
      }

      // remove current pawn from white pawns BitBoard
      white_pawns ^= ChessBoard.SQUARES[square];
    }

    // calculate black pawn structure score
    while (black_pawns != ChessBoard.EMPTY_BOARD)
    {
      // get square of next pawn
      uint square = MoveGen.GetNextSquare(black_pawns);

      // check to see if pawn is a passed pawn
      if ((B_PASSED_PAWN[square] & AI.board.white_pawn) == ChessBoard.EMPTY_BOARD)
      {
        black_pawn_structure += BONUS_PASSED_PAWN;

        // check to see if rook is behind a passed pawn
        if ((MoveGen.GetRookMoves(square, AI.board.occupied_squares, AI.board.occupied_squares) ^ AI.board.black_rook) != ChessBoard.EMPTY_BOARD)
          black_pawn_structure += BONUS_ROOK_BEHIND_PASSED_PAWN;
      }

      // check to see if pawn is a doubled pawn
      if (((ChessBoard.UP_MOVES[square] | ChessBoard.DOWN_MOVES[square]) ^ ChessBoard.SQUARES[square]) != 0)
        black_pawn_structure += PENALTY_DOUBLED_PAWN;

      // check to see if pawn is an isolated pawn (if not isolated, check to see if pawn is a backward pawn)
      if ((B_ISOLATED_PAWN[square] & AI.board.black_pawn) == ChessBoard.EMPTY_BOARD)
        black_pawn_structure += PENALTY_ISOLATED_PAWN;
      else
      {
        if (((ChessBoard.PAWN_CAPTURES[square - 8] & AI.board.white_pawn) != ChessBoard.EMPTY_BOARD) &&
            (B_BACKWARD_PAWN[square] & AI.board.black_pawn) == ChessBoard.EMPTY_BOARD)
          black_pawn_structure += PENALTY_BACKWARD_PAWN;
      }

      // remove current pawn from black pawns BitBoard
      black_pawns ^= ChessBoard.SQUARES[square];
    }

    // calculate net pawn structure score
    if (player == ChessBoard.WHITE)
      net_pawn_structure += white_pawn_structure - black_pawn_structure;
    else if (player == ChessBoard.BLACK)
      net_pawn_structure += black_pawn_structure - white_pawn_structure;

    // return pawn structure score
    return net_pawn_structure;
  }

  // returns the king safety score for the specified color
  public static int GetKingSafetyScore(int player)
  {
    // initialize variables
    int white_king_safety = 0, black_king_safety = 0, net_king_safety = 0;

    // calculate white king safety score
    uint white_king_sq = MoveGen.GetNextSquare(AI.board.white_king);
    white_king_safety += BONUS_KINGSAFETY_STRONG * (ChessBoard.BitCount(W_KINGSAFETY_STRONG[white_king_sq] & AI.board.white_pawn));
    white_king_safety += BONUS_KINGSAFETY_WEAK * (ChessBoard.BitCount(W_KINGSAFETY_WEAK[white_king_sq] & AI.board.white_pawn));

    // calculate black king safety score
    uint black_king_sq = MoveGen.GetNextSquare(AI.board.black_king);
    black_king_safety += BONUS_KINGSAFETY_STRONG * (ChessBoard.BitCount(B_KINGSAFETY_STRONG[black_king_sq] & AI.board.black_pawn));
    black_king_safety += BONUS_KINGSAFETY_WEAK * (ChessBoard.BitCount(B_KINGSAFETY_WEAK[black_king_sq] & AI.board.black_pawn));

    // give penalty for being in check in end game
    if (IsEndGame(GetMaterialScore(ChessBoard.WHITE), GetMaterialScore(ChessBoard.BLACK)))
    {
      if (MoveGen.IsKingAttacked(ChessBoard.WHITE))
        white_king_safety += PENALTY_KING_CHECKED;
      if (MoveGen.IsKingAttacked(ChessBoard.BLACK))
        black_king_safety += PENALTY_KING_CHECKED;
    }

    // calculate net king safety score
    if (player == ChessBoard.WHITE)
      net_king_safety += white_king_safety - black_king_safety;
    else if (player == ChessBoard.BLACK)
      net_king_safety += black_king_safety - white_king_safety;

    // return king safety score
    return net_king_safety;
  }

  // returns the material value of the specified piece
  public static int GetPieceValue(uint piece)
  {
    switch (piece)
    {
      case ChessBoard.W_PAWN:
        return PAWN_SCORE;
      case ChessBoard.B_PAWN:
        return PAWN_SCORE;
      case ChessBoard.W_KNIGHT:
        return KNIGHT_SCORE;
      case ChessBoard.B_KNIGHT:
        return KNIGHT_SCORE;
      case ChessBoard.W_BISHOP:
        return BISHOP_SCORE;
      case ChessBoard.B_BISHOP:
        return BISHOP_SCORE;
      case ChessBoard.W_ROOK:
        return ROOK_SCORE;
      case ChessBoard.B_ROOK:
        return ROOK_SCORE;
      case ChessBoard.W_QUEEN:
        return QUEEN_SCORE;
      case ChessBoard.B_QUEEN:
        return QUEEN_SCORE;
      case ChessBoard.W_KING:
        return KING_SCORE;
      case ChessBoard.B_KING:
        return KING_SCORE;
      default:
        return -1; // something went wrong
    }
  }

  // returns true if we are in end game, false otherwise
  public static bool IsEndGame(int white_material, int black_material)
  {
    return white_material <= 1500 && black_material <= 1500;
  }

  // returns true if last GameState results in draw by 100 non-pawn/capture moves, false otherwise
  public static bool IsDrawByHundredMoves()
  {
    return AI.states[AI.states.Count - 1].hundredMoveDraw >= 100;
  }

  // returns true if last GameState results in draw by repetition, false otherwise
  public static bool IsDrawByRepetition()
  {
    // initialize count
    int count = AI.states.Count;

    // not enough moves have been made yet
    if (count < 8)
      return false;

    // check last 8 GameStates for repetition draw    
    return (AI.states[count - 8].state_fen.Equals(AI.states[count - 4].state_fen) &&
      AI.states[count - 7].state_fen.Equals(AI.states[count - 3].state_fen) &&
      AI.states[count - 6].state_fen.Equals(AI.states[count - 2].state_fen) &&
      AI.states[count - 5].state_fen.Equals(AI.states[count - 1].state_fen));
  }

  // returns true if last GameState results in a draw by insufficient material, false otherwise
  public static bool IsDrawByInsufficientMaterial()
  {
    // initialize piece counts
    int w_knight_count = ChessBoard.BitCount(AI.board.white_knight);
    int w_bishop_count = ChessBoard.BitCount(AI.board.white_bishop);
    int b_knight_count = ChessBoard.BitCount(AI.board.black_knight);
    int b_bishop_count = ChessBoard.BitCount(AI.board.black_bishop);
    int w_piece_count = ChessBoard.BitCount(AI.board.white_pieces);
    int b_piece_count = ChessBoard.BitCount(AI.board.black_pieces);

    // white king v. black king
    if (w_piece_count == 1 && b_piece_count == 1)
      return true;

    // white king v. black king and knight or bishop
    if (w_piece_count == 1 && b_piece_count == 2 && (b_knight_count == 1 || b_bishop_count == 1)) 
      return true;

    // white king and knight or bishop v. black king
    if (b_piece_count == 1 && w_piece_count == 2 && (w_knight_count == 1 || w_bishop_count == 1)) 
      return true;

    // white king and bishop(s) v. black king and bishop(s), bishops all on same color squares
    if ((w_piece_count - w_bishop_count) == 1 && (b_piece_count - b_bishop_count) == 1)
    {
      // initialize light/dark squares Biboards
      UInt64 light_squares = 0x55AA55AA55AA55AA;
      UInt64 dark_squares = 0xAA55AA55AA55AA55;

      // bishops are all on light squares
      if ((AI.board.white_bishop & dark_squares) == 0 && (AI.board.black_bishop & dark_squares) == 0)
        return true;

      // bishops are all on dark squares
      if ((AI.board.white_bishop & light_squares) == 0 && (AI.board.black_bishop & light_squares) == 0)
        return true;
    }

    // there is still sufficient material to checkmate
    return false;
  }

  // initializes non-constant scoring arrays
  public static void InitializeScoringArrays()
  {
    // initialize scoring arrays and set values to 0
    W_PASSED_PAWN = new UInt64[64];
    B_PASSED_PAWN = new UInt64[64];
    W_ISOLATED_PAWN = new UInt64[64];
    B_ISOLATED_PAWN = new UInt64[64];
    W_BACKWARD_PAWN = new UInt64[64];
    B_BACKWARD_PAWN = new UInt64[64];
    W_KINGSAFETY_STRONG = new UInt64[64];
    B_KINGSAFETY_STRONG = new UInt64[64];
    W_KINGSAFETY_WEAK = new UInt64[64];
    B_KINGSAFETY_WEAK = new UInt64[64];
    for (int i = 0; i < 64; i++)
    {
      W_PASSED_PAWN[i] = 0;
      B_PASSED_PAWN[i] = 0;
      W_ISOLATED_PAWN[i] = 0;
      B_ISOLATED_PAWN[i] = 0;
      W_BACKWARD_PAWN[i] = 0;
      B_BACKWARD_PAWN[i] = 0;
      W_KINGSAFETY_STRONG[i] = 0;
      B_KINGSAFETY_STRONG[i] = 0;
      W_KINGSAFETY_WEAK[i] = 0;
      B_KINGSAFETY_WEAK[i] = 0;
    }

    // set values for pawn structure position arrays
    for (int i = 0; i < 64; i++)
    {
      // set values for passed pawn position arrays
      for (int rank = (int)ChessMove.GetRank((uint)i) + 1; rank < 8; rank++)
      {
        // white passed pawns
        if (ChessMove.GetFile((uint)i) > 0) 
          W_PASSED_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
        W_PASSED_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
        if (ChessMove.GetFile((uint)i) < 7)
          W_PASSED_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
      }

      // set values for isolated pawn position arrays
      for (int rank = 2; rank < 8; rank++)
      {
        // white isolated pawns
        if (ChessMove.GetFile((uint)i) > 0)
          W_ISOLATED_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
        if (ChessMove.GetFile((uint)i) < 7)
          W_ISOLATED_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
      }

      // set values for backward pawn position array
      for (int rank = 2; rank <= (int)ChessMove.GetRank((uint)i) + 1; rank++)
      {
        // white backward pawns
        if (ChessMove.GetFile((uint)i) > 0)
          W_BACKWARD_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
        if (ChessMove.GetFile((uint)i) < 7)
          W_BACKWARD_PAWN[i] ^= ChessBoard.SQUARES[((rank - 1) * 8) + (int)ChessMove.GetFile((uint)i)];
      }
    }

    // set values for king safety position arrays
    for (int i = 0; i < 24; i++)
    {
      // white king safety strong/weak
      W_KINGSAFETY_STRONG[i] ^= ChessBoard.SQUARES[i + 8];
      W_KINGSAFETY_WEAK[i] ^= ChessBoard.SQUARES[i + 16];
      if (ChessMove.GetFile((uint)i) > 0)
      {
        W_KINGSAFETY_STRONG[i] ^= ChessBoard.SQUARES[i + 7];
        W_KINGSAFETY_WEAK[i] ^= ChessBoard.SQUARES[i + 15];
      }
      if (ChessMove.GetFile((uint)i) < 7)
      {
        W_KINGSAFETY_STRONG[i] ^= ChessBoard.SQUARES[i + 9];
        W_KINGSAFETY_WEAK[i] ^= ChessBoard.SQUARES[i + 17];
      }
      if (ChessMove.GetFile((uint)i) == 0)
      {
        W_KINGSAFETY_STRONG[i] ^= ChessBoard.SQUARES[i + 10];
        W_KINGSAFETY_WEAK[i] ^= ChessBoard.SQUARES[i + 18];
      }
      if (ChessMove.GetFile((uint)i) == 7)
      {
        W_KINGSAFETY_STRONG[i] ^= ChessBoard.SQUARES[i + 6];
        W_KINGSAFETY_WEAK[i] ^= ChessBoard.SQUARES[i + 14];
      }
    }

    // set values for black position arrays
    for (int i = 0; i < 64; i++)
    {
      for (int square = 0; square < 64; square++)
      {
        // black passed pawns (mirror of white passed pawns)
        if ((W_PASSED_PAWN[i] & ChessBoard.SQUARES[square]) != 0)
          B_PASSED_PAWN[MIRROR[i]] |= ChessBoard.SQUARES[MIRROR[square]];

        // black isolated pawns (mirror of white isolated pawns)
        if ((W_ISOLATED_PAWN[i] & ChessBoard.SQUARES[square]) != 0)
          B_ISOLATED_PAWN[MIRROR[i]] |= ChessBoard.SQUARES[MIRROR[square]];

        // black backward pawns (mirror of white backward pawns)
        if ((W_BACKWARD_PAWN[i] & ChessBoard.SQUARES[square]) != 0)
          B_BACKWARD_PAWN[MIRROR[i]] |= ChessBoard.SQUARES[MIRROR[square]];

        // black king safety strong (mirror of white king safety strong)
        if ((W_KINGSAFETY_STRONG[i] & ChessBoard.SQUARES[square]) != 0)
          B_KINGSAFETY_STRONG[MIRROR[i]] |= ChessBoard.SQUARES[MIRROR[square]];

        // black king safety weak (mirror of white king safety weak)
        if ((W_KINGSAFETY_WEAK[i] & ChessBoard.SQUARES[square]) != 0)
          B_KINGSAFETY_WEAK[MIRROR[i]] |= ChessBoard.SQUARES[MIRROR[square]];
      }
    }
  }

  // game state scoring constants
  public const int DRAW_SCORE = -250;
  public const int CHECKMATE_WIN_SCORE = 100000;
  public const int CHECKMATE_LOSE_SCORE = -100000;
  public const int TIME_EXPIRED_SCORE = 6666666;

  // material scoring constants
  public const int PAWN_SCORE = 100;
  public const int KNIGHT_SCORE = 300;
  public const int BISHOP_SCORE = 300;
  public const int ROOK_SCORE = 500;
  public const int QUEEN_SCORE = 900;
  public const int KING_SCORE = 100000;

  // mobility scoring constants
  public const int MOBILITY_SCORE = 1;
  public const int THREAT_SCORE = 3;

  // pawn structure scoring constants
  public const int PENALTY_DOUBLED_PAWN = -10;
  public const int PENALTY_ISOLATED_PAWN = -20;
  public const int PENALTY_BACKWARD_PAWN = -8;
  public const int BONUS_PASSED_PAWN = 20;
  public const int BONUS_ROOK_BEHIND_PASSED_PAWN = 20;

  // material bonus scoring constants
  public const int BONUS_BISHOP_PAIR = 20;

  // king safety scoring constants
  public const int BONUS_KINGSAFETY_STRONG = 9;
  public const int BONUS_KINGSAFETY_WEAK = 4;
  public const int PENALTY_KING_CHECKED = -30;

  // score limit for allowing null moves
  public const int NULLMOVE_LIMIT_SCORE = KNIGHT_SCORE - 1;

  // position arrays
  public static UInt64[] W_PASSED_PAWN;
  public static UInt64[] B_PASSED_PAWN;
  public static UInt64[] W_ISOLATED_PAWN;
  public static UInt64[] B_ISOLATED_PAWN;
  public static UInt64[] W_BACKWARD_PAWN;
  public static UInt64[] B_BACKWARD_PAWN;
  public static UInt64[] W_KINGSAFETY_STRONG;
  public static UInt64[] B_KINGSAFETY_STRONG;
  public static UInt64[] W_KINGSAFETY_WEAK;
  public static UInt64[] B_KINGSAFETY_WEAK;

  // distance scoring arrays
  public static readonly int[] PAWN_OWN_DISTANCE = { 0, 8, 4, 2, 0, 0, 0, 0 };
  public static readonly int[] PAWN_OPPONENT_DISTANCE = { 0, 2, 1, 0, 0, 0, 0, 0 };
  public static readonly int[] KNIGHT_DISTANCE = { 0, 4, 4, 0, 0, 0, 0, 0 };
  public static readonly int[] BISHOP_DISTANCE = { 0, 5, 4, 3, 2, 1, 0, 0 };
  public static readonly int[] ROOK_DISTANCE = { 0, 7, 5, 4, 3, 0, 0, 0 };
  public static readonly int[] QUEEN_DISTANCE = { 0, 10, 8, 5, 4, 0, 0, 0 };

  // pawn position scoring array
  public static readonly int[] PAWN_POSITION = {
    0,   0,   0,   0,   0,   0,   0,   0,
    0,   0,   0, -40, -40,   0,   0,   0,
    1,   2,   3, -10, -10,   3,   2,   1,
    2,   4,   6,   8,   8,   6,   4,   2,
    3,   6,   9,  12,  12,   9,   6,   3,
    4,   8,  12,  16,  16,  12,   8,   4,
    5,  10,  15,  20,  20,  15,  10,   5,
    0,   0,   0,   0,   0,   0,   0,   0  
  };

  // knight position scoring array
  public static readonly int[] KNIGHT_POSITION = {
    -10, -30, -10, -10, -10, -10, -30, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10, -10, -10, -10, -10, -10, -10, -10
  };

  // bishop position scoring array
  public static readonly int[] BISHOP_POSITION = {
    -10, -10, -20, -10, -10, -20, -10, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10, -10, -10, -10, -10, -10, -10, -10
  };

  // rook position scoring array
  public static readonly int[] ROOK_POSITION = {
    -10,  0,  0, 10, 10,  0,  0, -10,
      0,  0,  0,  0,  0,  0,  0,   0,
      0,  0,  0,  0,  0,  0,  0,   0,
      0,  0,  0,  0,  0,  0,  0,   0,
      0,  0,  0,  0,  0,  0,  0,   0,
      0,  0,  0,  0,  0,  0,  0,   0,
     15, 15, 15, 15, 15, 15, 15,  15,
      0,  0,  0,  0,  0,  0,  0,   0,
  };

  // queen position scoring array
  public static readonly int[] QUEEN_POSITION = {
    -10, -10, -20, -10, -10, -20, -10, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,  10,  10,   5,   0, -10,
    -10,   0,   5,   5,   5,   5,   0, -10,
    -10,   0,   0,   0,   0,   0,   0, -10,
    -10, -10, -10, -10, -10, -10, -10, -10
  };

  // king position scoring array
  public static readonly int[] KING_POSITION = {
      0,  20,  40, -40,   0, -40,  40,  20,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -60, -60, -60, -60, -60, -60, -60, -60,
    -60, -60, -60, -60, -60, -60, -60, -60,
    -60, -60, -60, -60, -60, -60, -60, -60,
    -60, -60, -60, -60, -60, -60, -60, -60,
    -60, -60, -60, -60, -60, -60, -60, -60,
    -60, -60, -60, -60, -60, -60, -60, -60     
  };

  // king endgame position scoring array
  public static readonly int[] KING_ENDGAME_POSITION = {
      0,  10,  20,  30,  30,  20,  10,   0,
     10,  20,  30,  40,  40,  30,  20,  10,
     20,  30,  40,  50,  50,  40,  30,  20,
     30,  40,  50,  60,  60,  50,  40,  30,
     30,  40,  50,  60,  60,  50,  40,  30,
     20,  30,  40,  50,  50,  40,  30,  20,
     10,  20,  30,  40,  40,  30,  20,  10,
      0,  10,  20,  30,  30,  20,  10,   0
  };

	// mirror array (for black moves)
	public static readonly int[] MIRROR = {
		56,	57,	58,	59,	60,	61,	62,	63,
		48,	49,	50,	51,	52,	53,	54,	55,
		40,	41,	42,	43,	44,	45,	46,	47,
		32,	33,	34,	35,	36,	37,	38,	39,
		24,	25,	26,	27,	28,	29,	30,	31,
		16,	17,	18,	19,	20,	21,	22,	23,
		 8,	 9,	10,	11,	12,	13,	14,	15,
		 0,	 1,	 2,	 3,	 4,	 5,	 6,	 7	
	};
}
