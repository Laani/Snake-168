using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine.UI;

public class dbLogin : MonoBehaviour {
	
	public static string username;
	public static string server;
	public static int playerNum;
	public static string p1Pos;
	public static string p2Pos;
	public static string opponent;
	public static int oscore=0;
	private static string myGameName="";
	static float p1x, p1y,p2x,p2y;
	private Socket client;

	private static Text openGames;
	private static Text playerList;
	private static string listOfGames = "Open Games: ";
	private static string listOfPlayers = "";
	private static bool stopped = false;
	private static bool gameStarted = false;
	private const int port = 11000;
	private static ManualResetEvent connectDone;
	private static String response;

	private static bool p1updated=false;
	private static bool p2updated = false;

	private static string errorMessage = "";
	// private static bool responseRead = true;

	private static bool goToGameRoom = false;
	private static bool gotMembers = false;
	private static string playerNames = "";
	private static bool someoneQuit = false;
	private static bool enterLobby = false;
	private static Text lobbyName;
	private static bool sendPass = false;
	private static string password = "";

	public string getUser() {
		return username;
	}

	public int getPlayerNum(){
		return playerNum;
	}

	public string getOpponent() {
		return opponent;
	}

	public int getOscore() {
		return oscore;
	}

	public void setGameName(string name)
	{
		myGameName = name;
	}

	public float getPos1(string type) {
//		string x = p1Pos.Substring (p1Pos.IndexOf (' ') + 1, p1Pos.IndexOf(',') - (p1Pos.IndexOf(' ') + 1));
//		string y = p1Pos.Substring(p1Pos.IndexOf (',') + 1, p1Pos.IndexOf('<') - (p1Pos.IndexOf(',') + 1));
		if (type == "x") {
			return p1x;
		} else
			return p1y;
	}

	public float getPos2(string type) {
//		string x = p2Pos.Substring (p2Pos.IndexOf (' ') + 1, p2Pos.IndexOf(',') - (p2Pos.IndexOf(' ') + 1));
//		string y = p2Pos.Substring(p2Pos.IndexOf (',') + 1, p2Pos.IndexOf('<') - (p2Pos.IndexOf(',') + 1));
		if (type == "x") {
			return p2x;
		} else
			return p2y;
	}

	public void SendToServer(String data)
	{
		Send (client, data);
	}

	public  string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
	
	public class StateObject {
		// Client socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 256;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();
	}

	void Awake () {
		DontDestroyOnLoad (transform.gameObject);
	}

	// Use this for initialization
	void Start () {
		username = null;
		
		client = null;

		connectDone = 
			new ManualResetEvent(false);
		response = String.Empty;
		GameObject serverGO = GameObject.Find ("Server");
		InputField serverIF = serverGO.GetComponent<InputField> ();
		serverIF.text = "127.0.0.1";
		Application.runInBackground = true;
		// private static bool responseRead = true;
	}
	
	public void Submit () {
		if (client == null) {
			// Connect to a remote device.
			try {
				// Establish the remote endpoint for the socket.
				// The name of the 
				// remote device is "host.contoso.com".
				GameObject serverGO = GameObject.Find ("Server");
				InputField serverIF = serverGO.GetComponent<InputField> ();
				server = serverIF.text;

				IPHostEntry ipHostInfo = Dns.GetHostEntry (server);
				IPAddress ipAddress = ipHostInfo.AddressList [0];
				IPEndPoint remoteEP = new IPEndPoint (ipAddress, port);
			
				// Create a TCP/IP socket.
				client = new Socket (AddressFamily.InterNetwork,
			                    SocketType.Stream, ProtocolType.Tcp);
			
				// Connect to the remote endpoint.
				client.BeginConnect (remoteEP, 
			                    new AsyncCallback (ConnectCallback), client);
				connectDone.WaitOne ();
			} catch (Exception e) {
				Debug.Log (e.ToString ());
			}
		}
		GameObject usernameGO = GameObject.Find ("Username");
		InputField usernameIF = usernameGO.GetComponent<InputField> ();
		username = usernameIF.text;
		
		GameObject passwordGO = GameObject.Find ("Password");
		InputField passwordIF = passwordGO.GetComponent<InputField> ();
		password = passwordIF.text;

		// Encrypt the password

		password = Md5Sum (password);
		
		// Send test data to the remote device.
		Send (client, "user " + username + "<EOF>");
		sendPass= true;

	}
			
		// Release the socket.
		/*try {
			client.Shutdown(SocketShutdown.Both);
		}
		catch (SocketException e) {
			Debug.Log ("Socket closed remotely");
		}
		client.Close();*/
	
	// Update is called once per frame
	void Update () {

		if (sendPass) {
			Send (client, "pass " + password + "<EOF>");
			sendPass = false;
		}
			
		if (client != null && !stopped) {
			Receive (client);
		}
		if (stopped) {
			//Debug.Log("opening load page");
			Application.LoadLevelAsync("Lobby");
			stopped = false;
		}
		if ((gameStarted) && (Application.loadedLevel != 2)) {
			Application.LoadLevelAsync("Main");
			gameStarted = false;
		}
		if (p1updated)
		{
			GameObject player1Obj = GameObject.Find("Player1");
			if (player1Obj==null)
			{
				Debug.Log("player1 object not found.");
			}
			player1Obj.transform.position= new Vector3(p1x,p1y,0);
			p1updated = false;
		}
		if (p2updated) {
			GameObject player2Obj = GameObject.Find("Player2");
			player2Obj.transform.position=new Vector3 (p2x,p2y,0);
			p2updated=false;
		}

		if (someoneQuit && (Application.loadedLevelName != "Lobby")){
			Application.LoadLevelAsync("Lobby");
			someoneQuit = false;
			
		}	
		if (((goToGameRoom) && (Application.loadedLevelName != "LobbyEnter")) || (enterLobby)){
			
			Application.LoadLevelAsync("LobbyEnter");
			Send (client, "lobb<EOF>");
			if (enterLobby) {
				enterLobby = false;
			}
			if (goToGameRoom) {
				goToGameRoom = false;
			}
			//Application.LoadLevelAsync("Game Room"); // Wrong name? - william
		}
		if (Application.loadedLevelName=="Lobby") {

			openGames = GameObject.Find("Open Game List").GetComponent<Text>();
			openGames.text = listOfGames;
		}
//		if (Application.loadedLevelName=="LobbyEnter"){
//			//someoneQuit = false; // If someone quit, this will be true and sent to someoneQuit
//
//
//			playerList = GameObject.Find("Players").GetComponent<Text>();
//			playerList.text = listOfPlayers;
//		}		
		if ((gotMembers) && (Application.loadedLevelName=="LobbyEnter") ){
			lobbyName=GameObject.Find("GameName").GetComponent<Text>();
			lobbyName.text = myGameName;
			Text membersList = GameObject.Find ("Players").GetComponent<Text>();
			membersList.text = playerNames;
			gotMembers = false;
		}

		if (errorMessage != "") {
			GameObject error = GameObject.Find("Error");
			Text x = error.GetComponent<Text>();
			x.text = errorMessage;
			errorMessage="";
		}

		/*if (!responseRead) {
			Debug.Log ("Response received: " + response);
			responseRead = true;
		}*/
	}

	private static void ConnectCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket client = (Socket) ar.AsyncState;
			
			// Complete the connection.
			client.EndConnect(ar);
			
			Debug.Log("Socket connected to " +
			                  client.RemoteEndPoint.ToString());
			
			// Signal that the connection has been made.
			connectDone.Set();
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	private static void Receive(Socket client) {
		try {
			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = client;
			
			// Begin receiving the data from the remote device.
			client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
			                    new AsyncCallback(ReceiveCallback), state);
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	private static void ReceiveCallback( IAsyncResult ar ) {
		try {
			// Retrieve the state object and the client socket 
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket client = state.workSocket;
			
			// Read data from the remote device.
			int bytesRead = client.EndReceive(ar);
			
			if (bytesRead > 0) {
				// There might be more data, so store the data received so far.
				state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));

				response = state.sb.ToString();
				Debug.Log("response before parsing: "+response);
				//Debug.Log ("<EOF> found in "+response+" is "+(response.IndexOf("<EOF>")>-1).ToString());

				if (response.IndexOf("<EOF>") > -1) 
				{
					//Debug.Log (response);
					Debug.Log ("Response received: " + response.Substring(0, response.Length - 5));

					if (response.Substring(0, 3) == "log") 
					{
						Debug.Log (username + " has logged in successfully.");
						Send (client, "ackn<EOF>");
						Debug.Log ("Sent ackn to server.");
						/*try {
							client.Shutdown(SocketShutdown.Both);
						}
						catch (SocketException e) {
							Debug.Log ("Socket closed remotely");
						}
						client.Close(); */
						stopped = true;


					} else if (response.Substring (0,3) == "wro") {
						Debug.Log("You have entered the wrong password for " + username + ". Please try again.");
					} else if (response.Substring(0, 3) == "reg") {
						Debug.Log(username + " has been registered.");
						stopped = true;
					}
					else if (response.Substring(0,3) == "sta")
					{
						string responseCut = response.Substring(4);
						responseCut = responseCut.Replace ("<EOF>", "");
						if (playerNum == 1) {
							username = responseCut.Substring (0, responseCut.IndexOf(","));
							opponent = responseCut.Substring (responseCut.IndexOf (",") + 1);
						}
						else if (playerNum == 2) {
							opponent = responseCut.Substring (0, responseCut.IndexOf(","));
							username = responseCut.Substring (responseCut.IndexOf (",") + 1);
						}
						Debug.Log ("start game with username: " + username + " | opponent: " + opponent);
						gameStarted = true;
						Send (client, "ackn<EOF>");
						//Application.LoadLevel("Main");
					}else if (response.Substring(0,3) == "one")
					{
						Debug.Log ("you are player one");
						playerNum=1;
						Send (client, "ackn<EOF>");
						//Application.LoadLevel("Main");
					}else if (response.Substring(0,3) == "two")
					{
						Debug.Log ("you are player two");
						playerNum=2;
						Send (client, "ackn<EOF>");
						//Application.LoadLevel("Main");
					}else if (response.Substring(0, 3) == "p1h")
					{

						Debug.Log ("p1h read");
						Debug.Log(response);
						p1Pos = response.Substring (4, 5);
						String p1posx = p1Pos.Substring(0,p1Pos.IndexOf(","));
						String p1posy = p1Pos.Substring(p1Pos.IndexOf(",")+1);
						Debug.Log (p1posx);
						Debug.Log (p1posy);
						//Snake snakeManager = GameObject.Find("Snake").GetComponent<Snake>();

						dbLogin.p1x = float.Parse(p1posx);
						dbLogin.p1y = float.Parse(p1posy);

						p1updated = true;
						Send (client, "ackn<EOF>");

					}else if (response.Substring(0, 3) == "p2h")
					{
						Debug.Log ("p2h read");
						p2Pos = response.Substring (4, 5);
						//Debug.Log ("p2h"+p2Pos);
						String p2posx = p2Pos.Substring(0,p2Pos.IndexOf(","));
						String p2posy = p2Pos.Substring(p2Pos.IndexOf(",")+1);
						Debug.Log("player 2 location:");
						Debug.Log(p2posx);
						Debug.Log(p2posy);
						dbLogin.p2x = float.Parse(p2posx);
						dbLogin.p2y = float.Parse(p2posy);
						
						p2updated = true;
						//Snake snakeManager = GameObject.Find("Snake").GetComponent<Snake>();
//						GameObject player2Obj = GameObject.Find("Player2");
//						
//						float player2ObjX = player2Obj.transform.position.x;
//						float player2ObjY = player2Obj.transform.position.y;
						Send (client, "ackn<EOF>");


					} //else if (response.Substring(0, 3) == "opp") {
					//	opponent = response.Substring(4, response.Length - 9);
					//	Debug.Log ("opponent's name received: " + opponent);
					//	Send (client, "ackn<EOF>"); }
					else if (response.Substring(0, 3) == "sco") {
						oscore = int.Parse(response.Substring (4, response.Length - 9));
						Debug.Log ("opponent's score: " + oscore);
						Send (client, "ackn<EOF>");
					}
					else if (response.Substring(0,3) == "ope")
					{
						listOfGames = "Open Games: "+ response.Substring(4,response.Length-9);
						//enterLobby = true;
						//Send (client, "lobb<EOF>");
						Send (client, "ackn<EOF>");
						Debug.Log ("Sent ackn to server.");
					}
					else if (response.Substring(0,3) == "joi")
					{
						goToGameRoom = true;
						Send (client, "lobb<EOF>");
						Send (client, "ackn<EOF>");
						Debug.Log ("Sent ackn to server.");
					}
					else if (response.Substring(0,3) == "pla")
					{
						listOfPlayers += response.Substring(4, response.Length-9);
						Send (client, "ackn<EOF>");
						Debug.Log ("Sent ackn to server.");
					}
					else if (response.Substring(0,3) == "mem")
					{
						gotMembers = true;
						int index = response.IndexOf ("<EOF>");
						playerNames = response.Substring(0, index);
						playerNames = playerNames.Replace ("mem ","");
						Debug.Log(playerNames);
						Send (client, "ackn<EOF>");
						Debug.Log ("Sent ackn to server.");

						// After "mem" is finished, check if ">sta " is in the original message
						if (response.Contains (">sta "))
						{
							// Create a staSection which separates the mem part
							string staSection = response.Substring (index);
							// Trim it so that it ends at the first <EOF> (assuming there might be other crap behind it)
							staSection = staSection.Substring (0, staSection.IndexOf ("<EOF>"));

							// Same implementation as the "sta" section above. Copy-pasted.
							string responseCut = staSection.Substring(4);
							responseCut = responseCut.Replace ("<EOF>", "");
							if (playerNum == 1) {
								username = responseCut.Substring (0, responseCut.IndexOf(","));
								opponent = responseCut.Substring (responseCut.IndexOf (",") + 1);
							}
							else if (playerNum == 2) {
								opponent = responseCut.Substring (0, responseCut.IndexOf(","));
								username = responseCut.Substring (responseCut.IndexOf (",") + 1);
							}
							Debug.Log ("start game with username: " + username + " | opponent: " + opponent);
							gameStarted = true;
							Send (client, "ackn<EOF>");
						}



					}

					else if (response.Substring(0,3)=="err")
					{
						errorMessage = response.Substring(4,response.Length-9);
					}
					else if (response.Substring(0,3) == "qui")
					{
						Debug.Log("opponent disconnected. gg no re");
						//Application.LoadLevelAsync("Lobby"); //This doesn't work here, have to be called in update()
						someoneQuit = true;
						goToGameRoom = false;
						Send (client, "ackn<EOF>");
					}


					//state.sb = new StringBuilder(); // Victor told me to  comment this out - William
				}


				StateObject newstate = new StateObject();
				newstate.workSocket = client;

				// Get the rest of the data.
				if (client != null) {
					client.BeginReceive(newstate.buffer,0,StateObject.BufferSize,0,
				                    new AsyncCallback(ReceiveCallback), newstate);
				}
			} /*else {
				// All the data has arrived; put it in response.
				if (state.sb.Length > 1) {
					responseRead = false;
					response = state.sb.ToString();
				}
			}*/
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}
	
	public static void Send(Socket client, String data) {
		// Convert the string data to byte data using ASCII encoding.
		Debug.Log ("sending " + data + " to server");
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
		// Begin sending the data to the remote device.
		client.BeginSend(byteData, 0, byteData.Length, 0,
		                 new AsyncCallback(SendCallback), client);
	}
	
	private static void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket client = (Socket) ar.AsyncState;

			// Complete sending the data to the remote device.
			int bytesSent = client.EndSend(ar);
			//Debug.Log("Sent " + bytesSent + " bytes to the server.");
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	void OnApplicationQuit(){
		Send (client, "quit<EOF>");
	}

}