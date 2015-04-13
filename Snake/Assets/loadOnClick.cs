using UnityEngine;
using System.Collections;

public class loadOnClick : MonoBehaviour {

	public void LoadScene(int level)
	{
		// Will take in an integer that says what scene it's taking in
		Application.LoadLevel(level);
		
	}
}
