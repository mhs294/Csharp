using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

///The class implementing gameplay logic.
class AI : BaseAI
{
  public override string username()
  {
    return "Lannister";
  }
  public override string password()
  {
    return "lordtyrion";
  }

  // returns the opposite color of the specified player
  public static int GetOtherPlayer(int player)
  {
    if (player == ChessBoard.WHITE)
      return ChessBoard.BLACK;
    else if (player == ChessBoard.BLACK)
      return ChessBoard.WHITE;

    // something went wrong...
    return -1;
  }
	
	// returns the index of the Piece in pieces that matches the described piece
	public int FindPiece(int file, int rank)
	{
		// iterate through pieces array until match is found
		for (int i = 0; i < pieces.Length; i++)
		{
				if (pieces[i].getFile() == file && pieces[i].getRank() == rank)
						return i;
		}
		
		// something went wrong...
		return -1;
	}

  // returns the index of the Piece in pieces that matches the described piece
  public int FindPiece(uint sq, uint thePiece)
  {
    // get Piece color
    int color = (int)((thePiece & 0x00000008) >> 3);

    // get Piece type
    int type = 0;
    switch ((int)(thePiece & 0x00000007))
    {
      case 1:
        type = (int)'P';
        break;
      case 2:
        type = (int)'K';
        break;
      case 3:
        type = (int)'R';
        break;
      case 4:
        type = (int)'N';
        break;
      case 5:
        type = (int)'B';
        break;
      case 6:
        type = (int)'Q';
        break;
    }

    // get Piece file and rank
    int file = (int)((sq % 8) + 1);
    int rank = (int)((sq / 8) + 1);

    // iterate through pieces until match is found
    for (int i = 0; i < pieces.Length; i++)
    {
      if (pieces[i].getOwner() == color && pieces[i].getType() == type && pieces[i].getFile() == file && pieces[i].getRank() == rank)
        return i;
    }

    // something went wrong...
    Console.WriteLine("ERROR - Piece not found. -1 returned.");
    return -1;
  }

  //This function is called each time it is your turn
  //Return true to end your turn, return false to ask the server for updated information
  public override bool run()
  {
    // Print out the current board state
    Console.WriteLine("+---+---+---+---+---+---+---+---+");
    for(int rank=8; rank>0; rank--)
    {
      Console.Write("|");
      for(int file=1; file<=8; file++)
      {
        bool found = false;
        // Loops through all of the pieces
        for(int p=0; !found && p<pieces.Length; p++)
        {
          // determines if that piece is at the current rank and file
          if(pieces[p].getRank() == rank && pieces[p].getFile() == file)
          {
            found = true;
            // Checks if the piece is black
            if(pieces[p].getOwner() == 1)
            {
              Console.Write("*");
            }
            else
            {
              Console.Write(" ");
            }
            // prints the piece's type
            Console.Write((char)pieces[p].getType()+" ");
          }
        }
        if(!found)
        {
          Console.Write("   ");
        }
        Console.Write("|");
      }
      Console.WriteLine("\n+---+---+---+---+---+---+---+---+");
    }

    // Looks through information about the players
    for(int p=0; p<players.Length; p++)
    {
      Console.Write(players[p].getPlayerName());
      // if playerID is 0, you're white, if its 1, you're black
      if(players[p].getId() == myID)
      {
        Console.Write(" (ME)");

        // update timeRemaining
        timeRemaining = players[p].getTime();
      }
      Console.WriteLine(" time remaining: "+players[p].getTime());
    }

    // if there has been a move, print the most recent move
    if (moves.Length > 0)
    {
      Console.Write("Last Move Was: ");
      Console.WriteLine(files[moves[0].getFromFile() - 1] + "" + moves[0].getFromRank() + "-" + files[moves[0].getToFile() - 1] + "" + moves[0].getToRank());
    }

    /////////////////////////////////////
    // <-- END OF STOCK AI.cs CODE --> //
    /////////////////////////////////////

    // print current move number
    Console.WriteLine("\nMove " + turnNumber().ToString("D3") + "\n========\n");
		
		// add to GameState List and update ChessBoard
    if (moves.Length <= 1)
    {
      board = new ChessBoard(ref pieces, myID);
      states.Add(new GameState(null, null));
    }
    else
    {
      ChessMove lastMove = ChessMove.GetChessMove(moves[0].getFromFile(), moves[0].getToFile(), moves[0].getFromRank(), moves[0].getToRank(), 
         moves[0].getPromoteType(), states[states.Count - 1].enPassant);
      board = new ChessBoard(ref pieces, myID);
      states.Add(new GameState(states[states.Count - 1], lastMove));      
    }

    // display current score information for player
    Console.Write("Score for ");
    if (myID == ChessBoard.WHITE)
      Console.WriteLine("WHITE:\n");
    else if (myID == ChessBoard.BLACK)
      Console.WriteLine("BLACK:\n");
    int material = Score.GetMaterialScore(myID);
    int position = Score.GetPositionScore(myID);
    // int mobility = Score.GetPositionScore(myID);
    int pawn_structure = Score.GetPawnStructureScore(myID);
    int king_safety = Score.GetKingSafetyScore(myID);
    Console.WriteLine("Net Material = " + material);
    Console.WriteLine("Net Position = " + position);
    //Console.WriteLine("Net Mobility = " + mobility);
    Console.WriteLine("Net Pawn Structure = " + pawn_structure);
    Console.WriteLine("Net King Safety = " + king_safety + "\n");
    Console.WriteLine("Overall Score = " + (material + position + /*mobility +*/ pawn_structure + king_safety) + "\n");
		
		// if playing as human, get move from console prompt
		while (HUMAN_PLAYER)
		{
      // get legal moves for this position
      List<ChessMove> legalMoves = MoveGen.GenerateMoves(myID, false);

			// prompt user for move
			Console.Write("Enter a move ([from] [to] <promotion type>): ");
			string[] humanMove = Console.ReadLine().Split(' ');
				
			// get origin square
			int humanFromFile = 0, humanFromRank = 0;
			for (int i = 0; i < 8; i++)
			{
				if (humanMove[0][0] == files[i])
				{
					humanFromFile = i + 1;
					break;
				}
			}
			humanFromRank = (int)Char.GetNumericValue(humanMove[0][1]);
				
			// get destination square
			int humanToFile = 0, humanToRank = 0;
			for (int i = 0; i < 8; i++)
			{
				if (humanMove[1][0] == files[i])
				{
					humanToFile = i + 1;
					break;
				}
			}
			humanToRank = (int)Char.GetNumericValue(humanMove[1][1]);				
				
			// if promotion type is specified, get the promotion piece from move
			int humanPromote = 0;
			if (humanMove.Length > 2)
				humanPromote = (int)humanMove[2][0];
				
			// check for legality of human move
			bool isLegal = false;
			for (int i = 0; i < legalMoves.Count; i++)
			{
				ChessMove m = legalMoves[i];
				if ((ChessMove.GetFile(m.GetFromSq()) + 1) == (uint)humanFromFile && 
						(ChessMove.GetRank(m.GetFromSq()) + 1) == (uint)humanFromRank &&
						(ChessMove.GetFile(m.GetToSq()) + 1) == (uint)humanToFile && 
						(ChessMove.GetRank(m.GetToSq()) + 1) == (uint)humanToRank)
				{
					isLegal = true;
					break;
				}
			}
				
			// if move is legal, make move
			if (isLegal)
			{
				// get Piece associated with move
				Piece humanPiece = pieces[FindPiece(humanFromFile, humanFromRank)];
				
				// make move
				humanPiece.move(humanToFile, humanToRank, humanPromote);
				return true;
			}
			else if (!isLegal)
				Console.WriteLine("ILLEGAL MOVE. Please input a legal move.\n");
		}

    // reset TIME_EXPIRED and timer
    TIME_EXPIRED = false;
    timer.Reset();

    // reset history table
    history = new HistoryTable();

    // run ABMiniMax
    int moveScore = 0, n = 0, toFile = -1, toRank = -1;
    uint fromSq = 0, toSq = 0, thePiece = 0;
    depth = 0;
    List<ChessMove> completeBestMoves = new List<ChessMove>(0);    
    Search.UpdateTimePerMove(moves.Length);
    timer.Start();
    while (!TIME_EXPIRED)
    { 
      depth += 1;
      nodes = 0;
      Search.MAX_DEPTH = depth;
      Search.NULLMOVE_ALLOWED = true;
      Search.FOLLOW_PV = true;
      Search.PV = new List<ChessMove>(0);      
      int score = Search.PVABMiniMax(0, Search.SMALL_NUM, Search.LARGE_NUM);
      if (score != Score.TIME_EXPIRED_SCORE)
      {
        moveScore = score;
        completeBestMoves = new List<ChessMove>(Search.PV);
      }        

      // select random move from bestMoves List
      if (completeBestMoves.Count > 0)
      {
        n = generator.Next(0, completeBestMoves.Count - 1);

        // get bestMove info
        fromSq = completeBestMoves[n].GetFromSq();
        thePiece = completeBestMoves[n].GetPiece();
        toSq = completeBestMoves[n].GetToSq();
        toFile = (int)((toSq % 8) + 1);
        toRank = (int)((toSq / 8) + 1);

        // print bestMove info
        Console.WriteLine("Best Move: " + completeBestMoves[n].GetMoveString() + ", Score: " + moveScore + ", Depth: " + depth + " (t = " +
		    (timer.ElapsedMilliseconds / 1000.0).ToString("F3") + "s, nodes = " + nodes + ")");
      }

      // if checkmate is found, stop searching
      if (score == Score.CHECKMATE_WIN_SCORE) break;
    }
    timer.Stop();

    // output number of best moves
    Console.WriteLine("completeBestMoves = " + completeBestMoves.Count);

    // make bestMove
    pieces[FindPiece(fromSq, thePiece)].move(toFile, toRank, completeBestMoves[n].GetPromoteType());    

    // update ChessBoard and GameState List
    completeBestMoves[n].DoMove(Search.MAKE);

    return true;
  }

  //This function is called once, before your first turn
  public override void init()
  {
    // initialize myID
    myID = playerID();

    // initialize states List
    states = new List<GameState>(0);

    // initialize generator
    generator = new Random();

    // initialize timer
    timer = new Stopwatch();

    // initialize scoring arrays
    Score.InitializeScoringArrays();

    // initialize IMPROVED_HEURISTIC (change to run either side against improved heuristic)
    if (myID == ChessBoard.WHITE)
      IMPROVED_HEURISTIC = true;
    else if (myID == ChessBoard.BLACK)
      IMPROVED_HEURISTIC = false;
			
		// initialize HUMAN_PLAYER (change to run either side as a human player)
		if (myID == ChessBoard.WHITE)
			HUMAN_PLAYER = false;
		else if (myID == ChessBoard.BLACK)
			HUMAN_PLAYER = false;
  }

  //This function is called once, after your last turn
  public override void end() 
  {
  
  } 

  public AI(IntPtr c) : base(c)
  {}

  private int myID;                                                           // playerID of this program instance
  public static char[] files = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };    // character representations of files
  public static char[] pieceChars = { ' ', 'P', 'K', 'R', 'N', 'B', 'Q', ' ',
                                ' ', 'p', 'k', 'r', 'n', 'b', 'q' };          // character representations of pieces
  public static bool TIME_EXPIRED;                                            // true if time per move heuristic is exceeded, false otherwise
  public static bool IMPROVED_HEURISTIC;                                      // true if AI uses improved evaluation heuristic, false otherwise
   public static bool HUMAN_PLAYER;																			      // true if moves are entered by human, false otherwise
  public static int depth;                                                    // current depth limit for search function
  public static int nodes;                                                    // number of nodes evaluated by search function
  public static float timeRemaining;                                          // time left on player's clock
  public static Stopwatch timer;                                              // timer for time-limited search
  public static ChessBoard board;                                             // state of the chess board
  public static List<GameState> states;                                       // List of all previous game states
  public static HistoryTable history;                                         // history table for improved move ordering
  private Random generator;                                                   // generator for random numbers
}

