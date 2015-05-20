using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LobbyScreen : MonoBehaviour {

	GameObject gameName;
	string game;

	dbLogin x;

	// Use this for initialization
	void Start()
	{
		gameName = GameObject.Find ("GameName");
		InputField gameNameIF = gameName.GetComponent<InputField> ();
		game = gameNameIF.text;

		x = GameObject.Find ("dbLogin").GetComponent<dbLogin>();
	}
	public void Refresh()
	{
		x.SendToServer ("list");
	}


	public void Host()
	{
		x.SendToServer("host " + game);
	}

	public void Join()
	{
		x.SendToServer ("join " + game);
	}
}
