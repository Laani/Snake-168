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

    public static List<Game> games=new List<Game>();
    
    public static int numOfGames = 0;
    public static String[] playerNum = new String[2] { "one", "two" };

    private static bool loggedInSuccessfully = false;
    private static int heartbeat = 100;

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
                    bool addedPlayer = false;
                    
                    if (loggedInSuccessfully)
                    {
                        if ((games.Count==0)&& (addedPlayer==false)) //if no games are initialized
                        {
                            games.Add(new Game(handler));
                            times++;
                            Console.WriteLine(times.ToString());
                            addedPlayer = true;
                        }
                        else if (addedPlayer==false)//if there are initialized games
                        {
                            /*For all initialized games, look for ones that aren't full. 
                             Add player if a non-full game is found*/
                            for (int i = 0; i < games.Count; i++)
                            {
                                if (games[i].isGameNotFull())
                                {
                                    games[i].addPlayer(handler);
                                    addedPlayer = true;
                                    if (games[i].isGameFull())
                                    {
                                       //addMessageToAllPlayers(games[i], "sta<EOF>");
                                       SendToAllPlayers(games[i].players, "sta<EOF>");
                                    }
                                }
                            }

                            /*if there are no empty/nonfull games, create a new one for this player*/
                            if (addedPlayer == false)
                            {
                                games.Add(new Game(handler));

                                times++;
                                Console.WriteLine(times.ToString());

                                addedPlayer = true;
                            }
                        }
                        
                        




                        //else if (games[numOfGames].isGameNotFull()) //if the current game isn't full, then add player
                        //{
                        //    games[numOfGames].addPlayer(handler);
                        //    Send(handler, "two<EOF>");
                        //}
                        //if (games[numOfGames].isGameFull()) //if game is full, send a start message to all players of the current game, move index up by one
                        //{
                        //    List<Player> sending = games[numOfGames].playerHandlers();
                        //    Console.WriteLine("sending has " + sending.Count + " items");
                        //    Console.WriteLine(sending[0].playerHandler);
                        //    Console.WriteLine(sending[1].playerHandler);
                        //    SendToAllPlayers(sending, "sta<EOF>");
                        //    numOfGames++;
                        //}
                        loggedInSuccessfully = false;
                        addedPlayer = false;
                    }
                }
                else if (content.Substring(0,4)=="ackn")
                {
                    Console.WriteLine("ackn received");
                    for (int i =0;i<games.Count;i++)
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
                    Game thisGame=null;
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
                        sendLocation(thisGame, handler, head,"head");
                        //SendToOtherPlayers(handler, allPlayerInGame, head + "<EOF>"); //#locationsending
                     }
                    
                }
                else if (content.Substring(0, 4) == "tail")
                {
                    String tail = content.Substring(5, content.Length - 10);
                    Game thisGame=null;
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
                        sendLocation(thisGame, handler, tail ,"tail");
                        //SendToOtherPlayers(handler, allPlayerInGame, head + "<EOF>"); //#locationsending
                     }
                    
                }
                else if (content.Substring(0,4)=="quit")
                {
                    for (int i = 0; i < games.Count; i++)
                    {
                        if (games[i].playerInThisGame(handler))
                        {
                            List<Player> otherPlayers = games[i].getOtherPlayers(handler);
                            for (int m =0;m<otherPlayers.Count;m++)
                            {
                                otherPlayers[m].addMessage("log<EOF>");
                            }
                        }
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
                

            }
        }
    }

  ////  private static Game findGameWithHandler(Socket handler)
  //  {
  //      for (int i =0; i<games.Count;i++)
  //      {
  //          for (int m =0; m<games[i].players.Count;m++)
  //          {
  //              if (games[i].players[m].handler() == handler)
  //              {
  //                  return games[i];
  //              }
  //          }
  //      }

  //  }

    private static void sendLocation(Game game,Socket handler,String data,String type)
    {
        String header="";
        if (type=="head")
        {
            header = "h";
        }
        else if (type =="tail")
        {
            header = "t";
        }
        for (int i = 0; i < game.players.Count; i++)
        {
            if (game.players[i].handler() != handler)
            {
                game.players[i].addMessage("p"+(i+1).ToString()+header+" "+data+"<EOF>"); 
            }
        }
    }


    private static void addMessageToAllPlayers(Game game, String data)
    {
        for (int i =0; i < game.players.Count;i++)
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
            if (players[i].handler()==null)
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
        Console.WriteLine("Sent " + data + " to client " + handler.AddressFamily);
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
        SQLiteConnection m_dbConnection =
            new SQLiteConnection("Data Source=dbGame.db;Version=3;");
        m_dbConnection.Open();

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