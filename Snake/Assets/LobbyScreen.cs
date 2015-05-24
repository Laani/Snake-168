﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LobbyScreen : MonoBehaviour {

	GameObject gameName;
	string game;
	private InputField gameNameIF;
	dbLogin x;

	// Use this for initialization
	void Start()
	{
		gameName = GameObject.Find ("GameName");
		gameNameIF = gameName.GetComponent<InputField> ();


		x = GameObject.Find ("dbLogin").GetComponent<dbLogin>();
	}
	public void Refresh()
	{
		x.SendToServer ("list<EOF>");
	}


	public void Host()
	{
		game = gameNameIF.text;
		x.SendToServer("host " + game+"<EOF>");
	}

	public void Join()
	{
		x.SendToServer ("join " + game+"<EOF>");
	}
}