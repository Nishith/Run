using UnityEngine;
using System.Collections;

/*
 * ExampleScript
 * 
 * By Bren Lynne
 * iam@brenlynne.com
 * 
 * An example of how to use the AdvancedRagdoll script
 * 
 */

public class ExampleScript : MonoBehaviour 
{
    // objects to destroy and convert to ragdolls
    public GameObject[] objectsToKill;
    // ragdolls
    public Transform[] ragdollsToCreate;
    // objects to create from ragdolls
    public Transform[] objectsToCreate;
    // animations to play on created objects
    public string[] objectAnimations;

    // how long to wait before destroying
    float killDelay = 5f;
    // how long to wait before creating
    float createDelay = 5f;

    // array of created ragdolls
    ArrayList ragdolls = new ArrayList();

	void Start () 
    {
        // error checking
        if (objectsToKill.Length == 0)
        {
            Debug.LogError("Example.Start() has no objectsToKill assigned!");
            enabled = false;
            return;
        }

        if (ragdollsToCreate.Length == 0)
        {
            Debug.LogError("Example.Start() has no ragdollsToCreate assigned!");
            enabled = false;
            return;
        }

        if (objectsToCreate.Length == 0)
        {
            Debug.LogError("Example.Start() has no objectsToCreate assigned!");
            enabled = false;
            return;
        }

        if (objectsToKill.Length != ragdollsToCreate.Length || objectsToKill.Length != objectsToCreate.Length || objectsToKill.Length != objectAnimations.Length)
        {
            Debug.LogError("Example.Start() objects count mismatch! objectsToKill = " + objectsToKill.Length + " ragdollsToCreate = " + ragdollsToCreate.Length + " objectsToCreate = " + objectsToCreate.Length + " objectAnimations = " + objectAnimations.Length);
            enabled = false;
            return;
        }

        // set animations to random start position
        for (int i = 0; i < objectsToKill.Length; ++i)
        {
            GameObject go = objectsToKill[i] as GameObject;
            go.animation[objectAnimations[i]].time = Random.Range(0, go.animation[objectAnimations[i]].length);
        }
    }
	
	void Update () 
    {
        if (killDelay > 0f)
        {
            killDelay -= Time.deltaTime;

            if (killDelay <= 0f)
            {
                // time to kill

                for (int i = 0; i < objectsToKill.Length; ++i)
                {
                    // instantiate ragdolls
                    Transform r = (Transform)Instantiate(ragdollsToCreate[i], objectsToKill[i].transform.position, objectsToKill[i].transform.rotation);

                    if (!r)
                    {
                        Debug.LogError("Example.Update() " + name + " could not Instantiate " + ragdollsToCreate[i].name);
                    }
                    else
                    {
                        // get AdvancedRagdoll script
                        AdvancedRagdoll ar = (AdvancedRagdoll)r.GetComponent("AdvancedRagdoll");

                        if (!ar)
                        {
                            Debug.LogError("Example.Update() " + name + " could not get AdvancedRagdoll from " + r.name);
                        }
                        else
                        {
                            // store ragdoll reference
                            ragdolls.Add(ar);
                            
                            // synch ragdoll to current object animation pose
                            ar.SynchRagdollIn(objectsToKill[i].transform);
                        }
                    }

                    // destroy object
                    Destroy(objectsToKill[i]);
                }
            }
        }
        else if (createDelay > 0f)
        {
            createDelay -= Time.deltaTime;

            if (createDelay <= 0f)
            {
                // time to create

                for (int i = 0; i < ragdolls.Count; ++i)
                {
                    // get AdvancedRagdoll script
                    AdvancedRagdoll ar = ragdolls[i] as AdvancedRagdoll;

                    if (!ar)
                    {
                        Debug.LogError("Example.Update() " + name + " could not get AdvancedRagdoll from ragdolls!");
                    }
                    else
                    {
                        // create new object at the ragdoll's pos/rot
                        Transform newObject = (Transform)Instantiate(objectsToCreate[i], ar.transform.position, ar.transform.rotation);

                        // synch object to ragdoll pose
                        ar.SynchRagdollOut(newObject);

                        // store new object reference
                        objectsToKill[i] = newObject.gameObject;

                        if (!newObject.animation)
                        {
                            Debug.LogError("Example.Update() " + name + " has no animation!");
                        }
                        else
                        {
                            // play animation at random start position
                            newObject.animation[objectAnimations[i]].time = Random.Range(0, newObject.animation[objectAnimations[i]].length);
                            newObject.animation.CrossFade(objectAnimations[i], 1f);
                        }

                        // destroy ragdoll
                        Destroy(ar.gameObject);
                    }
                }

                // clear ragdoll references
                ragdolls.Clear();

                // re-init timers
                killDelay = createDelay = 5f;
            }

        }
	}

    void OnGUI ()
    {
        if (killDelay > 0f)
        {
            GUILayout.Label("Ragdolling objects in " + (int)(killDelay + 1f) + " seconds...");
        }
        else if (createDelay > 0f)
        {
            GUILayout.Label("Restoring objects in " + (int)(createDelay+ 1f) + " seconds...");
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            if (GUI.Button(new Rect(0, Screen.height - 20, 100, 20), "Quit"))
                Application.Quit();
        }
    }
}
