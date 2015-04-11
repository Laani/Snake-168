using UnityEngine;
using System.Collections;

public class SpawnFood : MonoBehaviour {

	public GameObject foodPrefab;
	public Transform borderTop;
	public Transform borderBottom;
	public Transform borderLeft;
	public Transform borderRight;

	// Use this for initialization
	void Start () {
		// Spawn food every 4 seconds, starting in 3
		InvokeRepeating("Spawn", 3, 4);
	}
	
	// Spawn one piece of food
	void Spawn() {
		// x position between left & right border
		int x = (int)Random.Range(borderLeft.position.x+1,
		                          borderRight.position.x-1);
		
		// y position between top & bottom border
		int y = (int)Random.Range(borderBottom.position.y+1,
		                          borderTop.position.y-1);
		
		// Instantiate the food at (x, y)
		Instantiate(foodPrefab,
		            new Vector2(x, y),
		            Quaternion.identity); // default rotation
	}
	

}
