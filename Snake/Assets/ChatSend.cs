using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ChatSend : MonoBehaviour {
	dbLogin x;
	InputField chatbox;
	static Text chatboxText;
	// Use this for initialization
	void Start () {
		x = GameObject.Find ("dbLogin").GetComponent<dbLogin> ();
		chatbox = GameObject.Find ("ChatInput").GetComponent<InputField> ();
		chatbox.ActivateInputField ();
		chatboxText = GameObject.Find ("ChatBox").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.Return)) {

			GameObject messageSendGO = GameObject.Find ("ChatInput");
			InputField messageSendIF = messageSendGO.GetComponent<InputField> ();
			string message = messageSendIF.text;
			string send = "chat " + x.getUser () + ": " + message + "<EOF>";
			Debug.Log (send);
			x.SendToServer (send);
			messageSendIF.text="";
			messageSendIF.ActivateInputField();
		}
		chatboxText.text = dbLogin.chatbox;

	}
	public void Ready()
	{
		x.SendToServer ("read<EOF>");
	}
}
