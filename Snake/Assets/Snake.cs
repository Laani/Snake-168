using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine.UI;




public class Snake : MonoBehaviour {
	//List for tails
	List<Transform> tail = 	new List<Transform>();

	// Bool for just ate something
	bool ate = false;

	//Bool for if game is over or not
	bool over = false;	
	// Tail Prefab
	public GameObject tailPrefab;

	//GameOver Object
	public GameObject gameOver;

	//Wall Objects
	public GameObject topWall, bottomWall, rightWall, leftWall;

	dbLogin networkObject;
	//Score
	private int score=0;

	//Player score objects
	public Text player1Score, player2Score;

	//Direction for snake head
	Vector2 dir;

	// Use this for initialization
	void Start () {
		if (this.name == "Player1") {
			dir = Vector2.right;
		} else if (this.name == "Player2") {
			dir = -Vector2.right;
		}
		InvokeRepeating("Move", 0.1f, 0.1f);  

		//networkObject = GameObject.Find ("dblogin").GetComponent<dbLogin>();


	}



	// Update is called once per frame
	void Update () {
		if (over) {

			Time.timeScale = 0;
			// Find and acquire the Manager component within the scene, and call the GameOver method.
            //FindObjectOfType<Manager>().GameOver();

            FindObjectOfType<loadOnClick>().LoadScene(3);

		} else {
			gameOver.SetActive(false);
		}
		if (this.name == "Player1") {
			// Move in a new Direction?
			if (Input.GetKey (KeyCode.D)) {
				if (dir != -Vector2.right) {
					dir = Vector2.right;
				}
			} else if (Input.GetKey (KeyCode.S)) {
				if (dir != Vector2.up) {
					dir = -Vector2.up;    // '-up' means 'down'
				}
			} else if (Input.GetKey (KeyCode.A)) {
				if (dir != Vector2.right) {
					dir = -Vector2.right; // '-right' means 'left'
				}
			} else if (Input.GetKey (KeyCode.W)) {
				if (dir != -Vector2.up) {
					dir = Vector2.up;
				}
			}

			//Set score for Player 1
			dbLogin loginManager = GameObject.Find("dbLogin").GetComponent<dbLogin>();
			string username = loginManager.getUser ();

			player1Score.text = username + ": " + score;

		} else if (this.name == "Player2") {
			if (Input.GetKey (KeyCode.RightArrow)) {
				if (dir != -Vector2.right) {
					dir = Vector2.right;
				}
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				if (dir != Vector2.up) {
					dir = -Vector2.up;    // '-up' means 'down'
				}
			} else if (Input.GetKey (KeyCode.LeftArrow)) {
				if (dir != Vector2.right) {
					dir = -Vector2.right; // '-right' means 'left'
				}
			} else if (Input.GetKey (KeyCode.UpArrow)) {
				if (dir != -Vector2.up) {
					dir = Vector2.up;
				}
			}
			//Set score for player 2
			player2Score.text = "Player 2: " + score;
			
		}


	}

	string HeadData()
	{

	}


	void Move() {
		// Save current position (gap will be here)
		Vector2 v = transform.position;
		
		// Move head into new direction (now there is a gap)
		transform.Translate(dir);
		
		// Ate something? Then insert new Element into gap
		if (ate) {
			// Load Prefab into the world
			GameObject g =(GameObject)Instantiate(tailPrefab,
			                                      v,
			                                      Quaternion.identity);
			
			// Keep track of it in our tail list
			tail.Insert(0,g.transform);

			
			// Reset the flag
			ate = false;
		}
		// Do we have a Tail?
		else if (tail.Count > 0) {

			// Move last Tail Element to where the Head was
			tail.Last().position = v;
			
			// Add to front of list, remove from the back
			tail.Insert(0, tail.Last());
			tail.RemoveAt(tail.Count-1);

		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
		// Food?

		if (coll.name.StartsWith ("foodPrefab")) {
			// Get longer in next Move call
			ate = true;

			//increment score. score is kept individually by snake head. 
			score++;

			// Remove the Food
			Destroy (coll.gameObject);
		}
		// Collided with Tail or Border
		else if ((coll.name.StartsWith ("TailPrefab"))||(coll.name.StartsWith("Player")) ){
		

			over = true;
		} else if ((coll.name.StartsWith ("Top")) || (coll.name.StartsWith ("Bottom")) || 
			(coll.name.StartsWith ("Left")) || (coll.name.StartsWith ("Right"))) {

			over = true;

		}




	}

}
