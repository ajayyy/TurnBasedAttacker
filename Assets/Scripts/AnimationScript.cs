using System.Collections;
using System.Collections.Generic;
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
            }
        }
    }

    public void OnAnimationEnded() {
        //when the animation ends, set the started to false to make this the new position
        started = false;

        //round the positions incase it didn't reach a full number for some reason
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
    }

    public void OnDeadAnimationEnded() {

        Player playerScript = GetComponent<Player>();

        //select another player owned by this player
        foreach (GameObject playerObject in GameController.instance.players) {
            Player player = playerObject.GetComponent<Player>();
            if (player.playerNum == playerScript.playerNum && player != playerScript) {
                player.selected = true;
                player.spriteRenderer.color = player.highlightColor;

                break;
            }
        }

        GameController.instance.players.Remove(gameObject);

        Destroy(gameObject);

    }
}
