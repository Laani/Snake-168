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
	static float x, y;
	private Socket client;

	private static bool stopped = false;
	private static bool gameStarted = false;
	private const int port = 11000;
	private static ManualResetEvent connectDone;
	private static String response;

	private static bool updated=false;

	// private static bool responseRead = true;

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

	public float getPos1(string type) {
		string x = p1Pos.Substring (p1Pos.IndexOf (' ') + 1, p1Pos.IndexOf(',') - (p1Pos.IndexOf(' ') + 1));
		string y = p1Pos.Substring(p1Pos.IndexOf (',') + 1, p1Pos.IndexOf('<') - (p1Pos.IndexOf(',') + 1));
		if (type == "x") {
			return float.Parse (x);
		} else
			return float.Parse (y);
	}

	public float getPos2(string type) {
		string x = p2Pos.Substring (p2Pos.IndexOf (' ') + 1, p2Pos.IndexOf(',') - (p2Pos.IndexOf(' ') + 1));
		string y = p2Pos.Substring(p2Pos.IndexOf (',') + 1, p2Pos.IndexOf('<') - (p2Pos.IndexOf(',') + 1));
		if (type == "x") {
			return float.Parse (x);
		} else
			return float.Parse (y);
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
		string password = passwordIF.text;

		// Encrypt the password

		password = Md5Sum (password);
		
		// Send test data to the remote device.
		Send (client, "user " + username + "<EOF>");
		Send (client, "pass " + password + "<EOF>");
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

		if (client != null && !stopped) {
			Receive (client);
		}
		if (stopped) {
			Debug.Log("opening load page");
			Application.LoadLevelAsync(4);
			stopped = false;
		}
		if ((gameStarted)&& (Application.loadedLevel!=2)) {
			Application.LoadLevelAsync(2);
		}
		if (updated)
		{
			GameObject player1Obj = GameObject.Find("Player1");
			player1Obj.transform.position= new Vector3(x,y,0);
			updated = false;
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
				Debug.Log ("<EOF> found in "+response+" is "+(response.IndexOf("<EOF>")>-1).ToString());

				if (response.IndexOf("<EOF>") > -1) 
				{
					Debug.Log (response);
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
						/*try {
							client.Shutdown(SocketShutdown.Both);
						}
						catch (SocketException e) {
							Debug.Log ("Socket closed remotely");
						}
						client.Close();*/
						stopped = true;
					}
					else if (response.Substring(0,3) == "sta")
					{
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
						p1Pos = response.Substring (4, response.Length - 9);
						String p1posx = p1Pos.Substring(0,p1Pos.IndexOf(","));
						String p1posy = p1Pos.Substring(p1Pos.IndexOf(",")+1);
						Debug.Log (p1posx);
						Debug.Log (p1posy);
						//Snake snakeManager = GameObject.Find("Snake").GetComponent<Snake>();

						dbLogin.x = float.Parse(p1posx);
						dbLogin.y = float.Parse(p1posy);

						updated = true;
						Send (client, "ackn<EOF>");

					}else if (response.Substring(0, 3) == "p2h")
					{
						Debug.Log ("p2h read");
						p2Pos = response.Substring (4, response.Length - 9);
						Debug.Log (p2Pos);

						//Snake snakeManager = GameObject.Find("Snake").GetComponent<Snake>();
//						GameObject player2Obj = GameObject.Find("Player2");
//						
//						float player2ObjX = player2Obj.transform.position.x;
//						float player2ObjY = player2Obj.transform.position.y;
						Send (client, "ackn<EOF>");


					} else if (response.Substring(0, 3) == "opp") {
						opponent = response.Substring(4, response.Length - 9);
						Debug.Log ("opponent's name received: " + opponent);
						Send (client, "ackn<EOF>");
					} else if (response.Substring(0, 3) == "sco") {
						oscore = int.Parse(response.Substring (4, response.Length - 9));
						Debug.Log ("opponent's score: " + oscore);
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
			Debug.Log("Sent " + bytesSent + " bytes to the server.");
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	void OnApplicationQuit(){
		Send (client, "quit<EOF>");
	}

}