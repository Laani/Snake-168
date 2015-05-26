using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Data.SQLite;
using Snake_Server;

// State object for reading client data asynchronously
public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener
{
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    public static List<Socket> clients = new List<Socket>();

    public static String username = String.Empty;
    public static String password = String.Empty;

    public static List<Game> games = new List<Game>();

    public static int numOfGames = 0;
    public static String[] playerNum = new String[2] { "one", "two" };

    private static bool loggedInSuccessfully = false;
    private static int heartbeat = 100;
    public static List<Player> players = new List<Player>();

    public static SQLiteConnection m_dbConnection;

    static int times = 0;
    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        //Listen to external IP address
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Listen to any IP Address
        IPEndPoint any = new IPEndPoint(IPAddress.Any, 11000);

        // Bind the socket to the local endpoint and listen for incoming connections.
        try
        {
            listener.Bind(any);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection..");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);
                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        m_dbConnection =
            new SQLiteConnection("Data Source=dbGame.db;Version=3;");
        m_dbConnection.Open();

        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;

        // Games have bidirectional communication (as opposed to request/response)
        // So I need to store all clients sockets so I can send them messages later
        // TODO: store in meaningful way,such as Dictionary<string,Socket>
        clients.Add(handler);

        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        //games.Add(new Game());

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the 
                // client. Display it on the console.
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Console.WriteLine("\n\n");

                if (content.Substring(0, 4) == "user")
                {
                    Console.WriteLine("Got a username");
                    username = content.Substring(5, content.Length - 10);
                }
                else if (content.Substring(0, 4) == "pass")
                {
                    Console.WriteLine("Got a password");
                    password = content.Substring(5, content.Length - 10);

                    DbLogin(username, password, handler);
                    players.Add(new Player(handler, username));
                }
                else if (content.Substring(0, 4) == "ackn")
                {
                    Console.WriteLine("ackn received");
                    for (int i = 0; i < games.Count; i++)
                    {
                        if (games[i].playerInThisGame(handler))
                        {
                            String message = games[i].getNextMessage(handler);
                            if (message != "")
                            {
                                Send(handler, message);
                            }
                        }
                    }
                }
                else if (content.Substring(0, 4) == "head")
                {
                    String head = content.Substring(5, content.Length - 10);
                    Game thisGame = null;
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            if (games[i].players[m].handler() == handler)
                            {
                                thisGame = games[i];
                            }
                        }
                    }


                    if (thisGame != null)
                    {
                        sendLocation(thisGame, handler, head, "head");
                        //SendToOtherPlayers(handler, allPlayerInGame, head + "<EOF>"); //#locationsending
                    }

                }
                else if (content.Substring(0, 4) == "tail")
                {
                    String tail = content.Substring(5, content.Length - 10);
                    Game thisGame = null;
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            if (games[i].players[m].handler() == handler)
                            {
                                thisGame = games[i];
                            }
                        }
                    }


                    if (thisGame != null)
                    {
                        sendLocation(thisGame, handler, tail, "tail");
                        //SendToOtherPlayers(handler, allPlayerInGame, head + "<EOF>"); //#locationsending
                    }

                }

                else if (content.Substring(0, 4) == "list")
                {
                    Console.WriteLine("client asked for game list");
                    string gameNames = "ope ";
                    for (int i = 0; i < games.Count; i++)
                    {
                        gameNames += games[i].getGameName();
                        if (i != games.Count - 1)
                        {
                            gameNames += ", ";
                        }

                    }
                    gameNames += "<EOF>";
                    Send(handler, gameNames);
                }

                else if (content.Substring(0, 4) == "1sco")
                {
                    Game thisGame = null;
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            if (games[i].players[m].handler() == handler)
                            {
                                thisGame = games[i];
                            }
                        }
                    }

                    if (thisGame != null)
                    {
                        thisGame.players[0].addScore();
                    }
                }
                else if (content.Substring(0, 4) == "2sco")
                {
                    Game thisGame = null;
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            if (games[i].players[m].handler() == handler)
                            {
                                thisGame = games[i];
                            }
                        }
                    }

                    if (thisGame != null)
                    {
                        thisGame.players[1].addScore();
                    }
                }

                else if (content.Substring(0, 4) == "food")
                {
                    //add score to correct player base on handler
                    //update score to all players

                }
                //make a player per game send food pellet location to server.
                //server echoes the location to all players
                else if (content.Substring(0, 4) == "host")
                {

                    string gameName = content.Substring(5, content.Length - 10);
                    Console.WriteLine(gameName);
                    host(handler, gameName);

                }
                else if (content.Substring(0, 4) == "join")
                {
                    string gameName = content.Substring(5, content.Length - 10);
                    join(handler, gameName);
                }
                else if (content.Substring(0, 4) == "lobb") //getting all players in lobby
                {
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            if (games[i].players[m].handler() == handler)
                            {
                                string members = lobbyPlayers(games[i], handler);
                                Send(handler, members + "<EOF>");
                                
                            }
                        }
                    }
                }
                //else if (content.Substring(0,4)=="name")
                //{
                //    int gameNum = findGameNumWithHandler(handler);
                //    if (gameNum!=-1)
                //    {
                //        string gamePlayers = lobbyPlayers(games[gameNum], handler);
                //    }
                //}
                else if (content.Substring(0, 4) == "quit")
                {
                    // Remove the game from the server list
                    int gameNum = -1;
                    for (int i = 0; i < games.Count; i++)
                    {
                        if (games[i].playerInThisGame(handler))
                        {
                            gameNum = i;
                            List<Player> otherPlayers = games[i].getOtherPlayers(handler);
                            for (int m = 0; m < otherPlayers.Count; m++)
                            {
                                Send(otherPlayers[m].handler(), "qui<EOF>");
                            }
                        }
                    }

                    if (gameNum != -1)
                    {
                        games.Remove(games[gameNum]);

                        // Remove the game from the database of active games
                        string query = "SELECT gameid FROM tb_games WHERE gameid = " + gameNum;
                        SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                        SQLiteDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            query = "DELETE FROM tb_games WHERE gameid = " + gameNum;
                            SQLiteCommand command2 = new SQLiteCommand(query, m_dbConnection);
                            command2.ExecuteNonQuery();
                        }
                        reader.Close();
                    }
                }

                // Echo the data back to the client.
                // Send(handler, "[ECHO] " + content + "<EOF>");

                // Setup a new state object
                StateObject newstate = new StateObject();
                newstate.workSocket = handler;

                // Call BeginReceive with a new state object
                handler.BeginReceive(newstate.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), newstate);
            }
            else
            {
                // Not all data received. Get more.
                // handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                // new AsyncCallback(ReadCallback), state);
                if (heartbeat == 0)
                {
                    heartbeat = 100;
                    for (int i = 0; i < games.Count; i++)
                    {
                        for (int m = 0; m < games[i].players.Count; m++)
                        {
                            Send(games[i].players[m].handler(), "hear");
                        }
                    }
                }

            }
            heartbeat--;
        }
    }

    private static int findGameNumWithHandler(Socket handler)
    {
        for (int i = 0; i < games.Count; i++)
        {
            for (int m = 0; m < games[i].players.Count; m++)
            {
                if (games[i].players[m].handler() == handler)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static string lobbyPlayers(Game game, Socket handler)
    {
        string playerString = "mem ";
        for (int i = 0; i < game.players.Count; i++)
        {
            playerString += game.players[i].getPlayerName() + " ";
            Console.WriteLine(game.players[i].getPlayerNum());
            if (game.players[i].getPlayerNum() == 0)
            {
                playerString += "(host) ";
            }
            if (game.players[i].handler() == handler)
            {
                playerString += "(you)";
                playerString += "\n";
            }
            else
            {
                playerString += "\n";
            }
        }
        
        return playerString;
    }

    private static void sendLocation(Game game, Socket handler, String data, String type)
    {
        String header = "";
        if (type == "head")
        {
            header = "h";
        }
        else if (type == "tail")
        {
            header = "t";
        }
        int from = 0;
        for (int i = 0; i < game.players.Count; i++)
        {
            if (game.players[i].handler() == handler)
            {
                from = game.players[i].getPlayerNum() + 1;
            }
        }

        for (int i = 0; i < game.players.Count; i++)
        {
            if (game.players[i].handler() == handler)
            {
                Console.WriteLine("this is the handler that sent the head data: " + (game.players[i].getPlayerNum() + 1));
            }
            else if (game.players[i].handler() != handler)
            {
                Console.WriteLine("This player (not the sender) number is: " + (game.players[i].getPlayerNum() + 1));
                String message = "p" + from.ToString() + header + " " + data + "<EOF>";
                Console.WriteLine(message);
                
                string lobbyplayers = lobbyPlayers(game, handler);
                game.players[i].addMessage(lobbyplayers);

                Send(game.players[i].handler(), message);
                //game.players[i].addMessage("p"+(i+1).ToString()+header+" "+data+"<EOF>"); 
            }
        }
    }

    private static void host(Socket handler, String gameName)
    //opens database connection, checks if game name exists, send message to client saying game already exists, 
    //otherwise, add a new game to the database
    {
        // Check if the game exists
        bool exists = false;
        for (int i = 0; i < games.Count; i++)
        {
            if (games[i].getGameName() == gameName)
            {
                Send(handler, "err Game name already in use!<EOF>"); // doesn't make game cause game name exists
                return;
                exists = true;
            }
            List<Player> players = games[i].players;
            for (int m = 0; m < players.Count; m++)
            {
                if (players[i].handler() == handler)
                {
                    Send(handler, "err You are already hosting a game!<EOF>"); //changed 'in a game' to 'hosting' - William
                    exists = true;
                }
            }
        }
        // If the game doesn't exist, create one!
        if (!exists)
        {
            string player1 = "";
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].handler() == handler)
                {
                    games.Add(new Game(handler, players[i], gameName));
                    player1 = players[i].getPlayerName();
                }
            }
            string gameNames = "ope ";
            for (int i = 0; i < games.Count; i++)
            {
                Console.WriteLine(games[i].getGameName());
                gameNames += games[i].getGameName();
                if (i != games.Count - 1)
                {
                    gameNames += ", ";
                }

            }
            gameNames += "<EOF>";
            Console.WriteLine("game names: " + gameNames);
            Send(handler, gameNames);

            // Also make sure to add it to the database with the current player as player1

            try
            {
                string query = "INSERT INTO tb_games (gameid, gameName, player1) VALUES (" + (games.Count - 1) + ", '" + gameName + "', '" + player1 + "')";
                SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
    private static void join(Socket handler, String gameName)
    //opens database connection, check if gamename exists, add player to game if game exists, else send message to 
    //client that the game doesn't exist. 
    {
        string playerNames = "";
        //If there are games, for the amount of players, if you are the handler, put yourself into the game.
        for (int i = 0; i < games.Count; i++)
        {
            for (int m = 0; m < games[i].players.Count; m++)
            {
                if (games[i].players[m].handler() == handler)
                {
                    playerNames += username;
                    Send(handler, "hos Entering your game<EOF>"); // Changed to if you join as host u send a host code - William
                    return;
                }
            }
        }

        // If there is this game name and game is not full. add player to that game.
        for (int i = 0; i < games.Count; i++)
        {
            string player1 = "";
            string player2 = username;

            if (games[i].getGameName() == gameName)
            {
                if (games[i].isGameNotFull())
                {
                    games[i].addPlayer(handler, username);
                    playerNames += username;

                    try
                    {
                        string query = "UPDATE tb_games SET player2 = '" + username + "' WHERE gameid = " + i;
                        SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                        command.ExecuteNonQuery();
                        query = "SELECT player1 FROM tb_games WHERE gameid = " + i;
                        command = new SQLiteCommand(query, m_dbConnection);
                        SQLiteDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            player1 = "" + reader["player1"];
                        }
                        reader.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    
                    Send(handler, "joi<EOF>");

                    // Now there are two players -- tell the clients to start the game!
                    SendToAllPlayers(games[i].players, "sta " + player1 + "," + player2 + "<EOF>");

                    return;
                }
                
            }
        }
        Send(handler, "err Game name does not exist.<EOF>");


        //        if ((games.Count==0)&& (addedPlayer==false)) //if no games are initialized
        //        {
        //            games.Add(new Game(handler,username));
        //            times++;
        //            Console.WriteLine(times.ToString());
        //            addedPlayer = true;
        //        }
        //        else if (addedPlayer==false)//if there are initialized games
        //        {
        //            /*For all initialized games, look for ones that aren't full. 
        //             Add player if a non-full game is found*/
        //            for (int i = 0; i < games.Count; i++)
        //            {
        //                if (games[i].isGameNotFull())
        //                {
        //                    games[i].addPlayer(handler,username);
        //                    addedPlayer = true;
        //                    if (games[i].isGameFull())
        //                    {
        //                       //addMessageToAllPlayers(games[i], "sta<EOF>");
        //                       SendToAllPlayers(games[i].players, "sta<EOF>");
        //                       for (int j = 0; j < games[i].players.Count; j++)
        //                       {
        //                           if (games[i].players[j].handler() != handler)
        //                           {
        //                               String message = "opp " + games[i].players[j].getPlayerName() + "<EOF>";
        //                               Console.WriteLine(message);
        //                               games[i].players[i].addMessage( message);
        //                           }
        //                       }
        //                    }
        //                }
        //            }

        //            /*if there are no empty/nonfull games, create a new one for this player*/
        //            if (addedPlayer == false)
        //            {
        //                games.Add(new Game(handler,username));

        //                times++;
        //                Console.WriteLine(times.ToString());

        //                addedPlayer = true;
        //            }
        //        }


        //        loggedInSuccessfully = false;
        //        addedPlayer = false;
        //    }
        //}
    }

    private static void addMessageToAllPlayers(Game game, String data)
    {
        for (int i = 0; i < game.players.Count; i++)
        {
            game.players[i].addMessage(data);
        }
    }

    private static List<Player> findGameWithHandler(Socket handler)
    {
        for (int i = 0; i < games.Count; i++)
        {
            if (games[i].playerInThisGame(handler))
            {
                return games[i].players;
            }
        }
        return null;
    }


    private static void SendToAllPlayers(List<Player> players, String data)
    {

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].handler() == null)
            {
                Console.WriteLine("handler is null");
            }
            else
            {
                Send(players[i].handler(), data);
            }
        }
    }

    private static void SendToOtherPlayers(Socket handler, List<Player> sendingTo, String data)
    {
        //Player[] sending = games[numOfGames].playerHandlers();
        for (int i = 0; i < sendingTo.Count; i++)
        {
            if (sendingTo[i].handler() != handler)
            {
                Send(sendingTo[i].handler(), data);
            }

        }
    }


    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
        for (int i = 0; i < games.Count; i++)
        {
            for (int m = 0; m < games[i].players.Count; m++)
            {
                if (handler == games[i].players[m].handler())
                {
                    Console.WriteLine("Sent " + data + " to client " + (games[i].players[m].getPlayerNum() + 1));
                }
            }
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void DbLogin(String username, String password, Socket handler)
    {
        string query = "SELECT username, password FROM tb_users WHERE username = '" + username + "'";
        SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine(username + " exists.");
            string correctPass = reader.GetString(1);
            if (correctPass == password)
            {
                Console.WriteLine(username + " has logged in successfully.");
                loggedInSuccessfully = true;
                Send(handler, "log<EOF>");
            }
            else
            {
                Console.WriteLine(username + " has used the wrong password. Please try again.");
                Send(handler, "wro<EOF>");
            }
        }
        else
        {
            query = "INSERT INTO tb_users (username, password) VALUES ('" + username + "', '" + password + "')";
            SQLiteCommand command2 = new SQLiteCommand(query, m_dbConnection);
            command2.ExecuteNonQuery();
            Console.WriteLine(username + " has been registered.");
            Send(handler, "reg<EOF>");
        }

        reader.Close();
    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}