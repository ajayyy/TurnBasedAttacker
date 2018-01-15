﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Class used by the animator to allow the animation to move based on local position and to be able to move in any direction needed
/// </summary>
public class AnimationScript : MonoBehaviour {

    //variables adjusted by animator
    public bool animating = false; //is this currently supposed to be animating
    public float offsetAmount = 0; // this amount is changed by the animator

    //variables set by Player class
    public float direction = 0; // direction in angles of where the object should move based on the offset
    public Vector3 target = Vector3.zero;
    public GameObject targetObject; //target gameobject if it exists
    public int type = 0; //0: one unit movement, 1: move to target
    public bool snapToGrid = true; //should it snap to the grid (true for game elements, not true for ui elements)

    //local variables
    Vector3 startPosition;
    bool started = false; //has animating already started or is it the first time

    void Start() {

    }

    void Update() {
        if (animating) {
            if (!started) {
                started = true;
                startPosition = transform.position;
            }

            switch (type) {
                case 0:
                    transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount;
                    break;
                case 1:
                    transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount * Vector3.Distance(target, startPosition);
                    break;
                case 2:
                    transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount * Vector3.Distance(target, startPosition);
                    break;
                case 3:
                    transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount * Vector3.Distance(target, startPosition);
                    break;
                case 4: //UI element
                    transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount * Vector3.Distance(target, startPosition);
                    break;
            }
        }
    }

    public void OnAnimationEnded() {
        //when the animation ends, set the started to false to make this the new position
        started = false;

        //round the positions incase it didn't reach a full number for some reason
        if(snapToGrid)
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

        if (type == 1) {
            gameObject.SetActive(false);
            targetObject.GetComponent<Animator>().SetTrigger("dead");

            Player playerScript = targetObject.GetComponent<Player>();

            foreach (GameObject playerObject in GameController.instance.players) {
                Player player = playerObject.GetComponent<Player>();
                if (player.playerNum == playerScript.playerNum) {
                    player.selected = false;
                    player.spriteRenderer.color = player.idleColor;

                    break;
                }
            }
        }

        if(type == 2) {
            gameObject.SetActive(false);

            GameObject newBlock = Instantiate(GameController.instance.block);
            newBlock.GetComponent<AnimationScript>().direction = direction - 180;
            newBlock.transform.position = new Vector3(MathHelper.cap(Mathf.RoundToInt(target.x), 11), MathHelper.cap(Mathf.RoundToInt(target.y), 11));
        }

        if (type == 3) {
            gameObject.SetActive(false);

            Player playerScript = targetObject.GetComponent<Player>();

            playerScript.stunned = true;
            playerScript.turnStunned = GameController.instance.turnNum;

            if(GameController.instance.turnPlayerNum > playerScript.playerNum) {
                playerScript.turnStunned++;
            }

            playerScript.stunnedColor = Instantiate(GameController.instance.stunColor);
            playerScript.stunnedColor.transform.position = targetObject.transform.position;

            if (playerScript.selected) {
                foreach (GameObject playerObject in GameController.instance.players) {
                    Player player = playerObject.GetComponent<Player>();
                    if (player.playerNum == playerScript.playerNum) {
                        player.selected = false;
                        player.spriteRenderer.color = player.idleColor;

                        break;
                    }
                }
            }

        }
    }

    public void OnDeadAnimationEnded() {

        Player playerScript = GetComponent<Player>();

        if(playerScript != null) {
            //select another player owned by this player
            foreach (GameObject playerObject in GameController.instance.players) {
                Player player = playerObject.GetComponent<Player>();
                if (player.playerNum == playerScript.playerNum && player != playerScript) {
                    player.selected = true;
                    player.spriteRenderer.color = player.highlightColor;

                    break;
                }
            }

			//remove player from list and lower that players unit count
            GameController.instance.players.Remove(gameObject);
			GameController.instance.playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text = int.Parse(GameController.instance.playerStatusList[playerScript.playerNum].GetComponentInChildren<Text>().text) - 1 + "";

			//if it's 0, then make it an X and skip their turn if it is next
			if (int.Parse (GameController.instance.playerStatusList [playerScript.playerNum].GetComponentInChildren<Text> ().text) == 0) {
				//add it to the dead list to know how many have died so far and when
				GameController.instance.playersDead++;
				GameController.instance.playersDeadList.Add (playerScript);

				GameController.instance.playerStatusList [playerScript.playerNum].GetComponentInChildren<Text>().enabled = false;

				if (GameController.instance.turnPlayerNum == playerScript.playerNum) {
					GameController.instance.NextTurn ();
				}

				foreach (Transform child in GameController.instance.playerStatusList [playerScript.playerNum].transform) {
					if (child.gameObject.name == "X") {
						child.gameObject.SetActive (true);
						break;
					}
				}

				if (GameController.instance.playersDead >= GameController.instance.personAmount - 1) {
					//the only units left in the players array will be the winner's units
//					GameController.instance.winner.GetComponent<Text> ().text = "Player " + (GameController.instance.players[0].GetComponent<Player>().playerNum + 1) + " Wins";
//					GameController.instance.winner.SetActive (true);

					//reverse the list so the first item is the player who is second place
					GameController.instance.playersDeadList.Reverse();

					//players left to add to the scoreboard
					int playersLeft = GameController.instance.personAmount;

					for (int i = 0; i < 1; i++) { //in a for loop to keep consistency
						GameObject winner = Instantiate (GameController.instance.winners [0]);
						winner.transform.SetParent(GameController.instance.winnerTextsHolder.transform);
						winner.transform.localPosition = new Vector3 (0, 278);
						winner.GetComponent<Text> ().text = "Player " + (GameController.instance.players[0].GetComponent<Player>().playerNum + 1) + " Wins";
						winner.GetComponent<Text> ().color = GameController.instance.players[0].GetComponent<Player>().idleColor;
						winner.SetActive (true);
					}

					for (int i = 0; i < 2; i++) {
						GameObject winner = Instantiate (GameController.instance.winners [1]);
						winner.transform.SetParent(GameController.instance.winnerTextsHolder.transform);
						winner.transform.localPosition = new Vector3 (0, 212 - (i*60));
						winner.transform.localScale = new Vector3(1, 1, 1);
						winner.GetComponent<Text> ().text = (i+2) + ". Player " + (GameController.instance.playersDeadList[i].playerNum + 1);
						winner.GetComponent<Text> ().color = GameController.instance.playersDeadList[i].idleColor;
						winner.SetActive (true);
					}

					for (int i = 0; i < 3; i++) {
						GameObject winner = Instantiate (GameController.instance.winners [2]);
						winner.transform.SetParent(GameController.instance.winnerTextsHolder.transform);
						winner.transform.localPosition = new Vector3 (0, 97 - (i*45));
						winner.transform.localScale = new Vector3(1, 1, 1);
						winner.GetComponent<Text> ().text = (i+2+2) + ". Player " + (GameController.instance.playersDeadList[i+2].playerNum + 1);
						winner.GetComponent<Text> ().color = GameController.instance.playersDeadList[i+2].idleColor;
						winner.SetActive (true);
					}

					for (int i = 0; i < 5; i++) {
						GameObject winner = Instantiate (GameController.instance.winners [3]);
						winner.transform.SetParent(GameController.instance.winnerTextsHolder.transform);
						winner.transform.localPosition = new Vector3 (0, -34 - (i*35));
						winner.transform.localScale = new Vector3(1, 1, 1);
						winner.GetComponent<Text> ().text = (i+2+2+3) + ". Player " + (GameController.instance.playersDeadList[i+2+3].playerNum + 1);
						winner.GetComponent<Text> ().color = GameController.instance.playersDeadList[i+2+3].idleColor;
						winner.SetActive (true);
					}

					for (int i = 0; i < 5; i++) {
						GameObject winner = Instantiate (GameController.instance.winners [4]);
						winner.transform.SetParent(GameController.instance.winnerTextsHolder.transform);
						winner.transform.localPosition = new Vector3 (0, -205 - (i*28));
						winner.transform.localScale = new Vector3(1, 1, 1);
						winner.GetComponent<Text> ().text = (i+2+2+3+5) + ". Player " + (GameController.instance.playersDeadList[i+2+3+5].playerNum + 1);
						winner.GetComponent<Text> ().color = GameController.instance.playersDeadList[i+2+3+5].idleColor;
						winner.SetActive (true);
					}

					GameController.instance.gameOver = true;
				}

			}

        } //not player if null

        if(GetComponent<Pickup>() != null) { //must be a pickup
            GameController.instance.pickups.Remove(gameObject);
        }

        Destroy(gameObject);

    }
}
