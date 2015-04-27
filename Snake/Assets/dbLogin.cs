using UnityEngine;
using System.Collections;
using System.Data;
using System;
using Mono.Data.Sqlite;
using UnityEngine.UI;

public class dbLogin : MonoBehaviour {
	IDbConnection dbconn;
	IDbCommand dbcmd;
	
	// Use this for initialization
	void Start () {
		
	}
	
	public void Submit () {
		string conn= "URI=file:" + Application.dataPath + "/dbGame.db";
		dbconn = (IDbConnection) new SqliteConnection(conn);
		dbconn.Open(); //Open connection to the database.
		
		dbcmd = dbconn.CreateCommand();
		
		GameObject usernameGO = GameObject.Find ("Username");
		InputField usernameIF = usernameGO.GetComponent<InputField> ();
		string username = usernameIF.text;
		
		GameObject passwordGO = GameObject.Find ("Password");
		InputField passwordIF = passwordGO.GetComponent<InputField> ();
		string password = passwordIF.text;
		
		string query = "SELECT username, password FROM tb_users WHERE username = '" + username + "'";
		dbcmd.CommandText = query;
		IDataReader reader = dbcmd.ExecuteReader();
		
		if (reader.Read ()) {
			Debug.Log (username + " exists.");
			string correctPass = reader.GetString (1);
			if (correctPass == password) {
				Debug.Log(username + " has logged in successfully.");
			} else {
				Debug.Log(username + " has used the wrong password.");
			}
		} else {
			query = "INSERT INTO tb_users (username, password) VALUES ('" + username + "', '" + password + "')";
			IDbCommand dbcmd2 = dbconn.CreateCommand();
			dbcmd2.CommandText = query;
			IDataReader reader2 = dbcmd2.ExecuteReader();
			Debug.Log(username + " has been registered.");
			reader2.Close();
		}
		
		reader.Close ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}