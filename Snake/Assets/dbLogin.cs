using UnityEngine;
using System.Collections;
using System.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine.UI;

public class dbLogin : MonoBehaviour {
	
	public static string username;
	
	private Socket client;

	private static bool stopped = false;
	
	private const int port = 11000;
	private static ManualResetEvent connectDone;
	private static String response;
	// private static bool responseRead = true;

	public string getUser() {
		return username;
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
		// private static bool responseRead = true;
	}
	
	public void Submit () {
		if (client == null) {
			// Connect to a remote device.
			try {
				// Establish the remote endpoint for the socket.
				// The name of the 
				// remote device is "host.contoso.com".
				IPHostEntry ipHostInfo = Dns.GetHostEntry (Dns.GetHostName ());
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
			Application.LoadLevelAsync("Main");
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
				if (response.IndexOf("<EOF") > -1) {
					Debug.Log ("Response received: " + response.Substring(0, response.Length - 5));
					if (response.Substring(0, 3) == "log") {
						Debug.Log (username + " has logged in successfully.");
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
	
	private static void Send(Socket client, String data) {
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
}