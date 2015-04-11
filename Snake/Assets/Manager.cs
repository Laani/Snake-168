using UnityEngine;
using System.Collections;
using System.Timers;
public class Manager : MonoBehaviour {

		// Player Prefab
        public GameObject player1, player2;

        // Title
        private GameObject title, gameOver, pressStart, player1Score, player2Score;
		

        void Start ()
        {
                // Search for the Title game object, and save it
                title = GameObject.Find ("Title");
				gameOver = GameObject.Find ("Over");
				
				gameOver.SetActive (false);
                Time.timeScale=0;
			
				player1Score = GameObject.Find ("Player1Score");
				player2Score = GameObject.Find ("Player2Score");

        }

        void Update ()
        {
                // When not playing, check if the X key is being pressed.
                if (IsPlaying () == false && Input.GetKeyDown (KeyCode.Space)) {
                        GameStart ();
                        Time.timeScale=1;

                }
				
        }
		
		

        void GameStart ()
        {
                // When it’s time to start the game, hide the title and make the player
                title.SetActive (false);
                //Instantiate (player, player.transform.position, player.transform.rotation);
        }

        public void GameOver ()
        {
                // When the game ends, show the title.
                
				gameOver.SetActive (true);

				if (Input.GetKeyDown(KeyCode.Space)){

					Application.LoadLevel (0);
				}
			
				
        }

        public bool IsPlaying ()
        {
                // Determine whether the game is being played by the visibility of the title.
                return title.activeSelf == false;
        }
}
