using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InputFieldHandler : MonoBehaviour {
	InputField usernameIF, passwordIF, ipIF;
	// Use this for initialization
	void Start () {
		usernameIF = GameObject.Find ("Username").GetComponent<InputField> ();
		passwordIF = GameObject.Find ("Password").GetComponent<InputField> ();
		ipIF = GameObject.Find ("Server").GetComponent<InputField> ();
		usernameIF.ActivateInputField ();
	}
	
	// Update is called once per frame
	void Update () {


		if (Input.GetKeyDown (KeyCode.Tab)) {
			if (usernameIF.IsActive())
			{
				usernameIF.DeactivateInputField();
				passwordIF.ActivateInputField();
			}
			else if (passwordIF.IsActive())
			{
				passwordIF.DeactivateInputField();
				usernameIF.ActivateInputField();
			}

		}
	}
}
