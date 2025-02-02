﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using VRTK;
using UnityEngine.SceneManagement;

public class BucketList : MonoBehaviour
{
    [Header("Bucket")]
    [Tooltip("Bucket")]
    public GameObject bucket;
    public GameObject checkliste;
    public float colorChangetimer = 1f;
    public float errorTimer = 5f;
    public float errorTimertotal = 5f;
    [Tooltip("Return Speed")]
    public float speed = 1f;
    public float fadingTime = 2f;
    public float beginingDelay = 1f;
    
    [Header("UI Checklist")]
    public Image UIDefault;
    public TextMeshProUGUI checkListHeader;

    [Tooltip("List Elements")]
    public int max_size = 7;
    public GameObject[] checkedObjects;
    public GameObject[] bucketListContent;
    private int textCounter = 0;
    public Color defaultColor;
    public Color errorColor;
    public Color grey;
    public Color white;
    public string invalidText;
    public string checkListHeaderText;
    public VerticalLayoutGroup backgroundTransparent;
    public VerticalLayoutGroup backgroundDefault;
    private Hashtable listItems = new Hashtable();

    // To change color by Coroutine Calls
    protected bool coroutineCalled = false;
    protected Collider bucketCollider;
    protected GameObject[] allGameObjects;
    protected IEnumerator moveCoroutine;
    protected IEnumerator delayCoroutine;
    protected GameObject[] checkListObjects;

    public AudioSource correctObject;
    public AudioSource IncorrectObject;


    [Tooltip("Player Score Points")]
    //public int playerScore = 10;
    // Score Objects
    ScoringSystem scoringSystem;

    public GameObject score10;
    public GameObject score25;
    public GameObject scorePlus;
    public GameObject scoreMinus;
    GameObject activeCameraRig;
    public int scoreValue;



    // Controller 
    protected Game_Manager controller = Game_Manager.Instance;

    public void Start()
    {
        //Objekte die im Eimer erkannt werden sollen einem Array zuweisen (in diesem Fall ALLE GameObjekte die aktiv in der Szene sind zur Demonstration).
        allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        FetchAllPositions();

        //Den Collider(MeshCollider) des Eimers einer Variable zuweisen.
        bucketCollider = GetComponent<Collider>();
        errorTimer = errorTimertotal;

        checkListObjects = new GameObject[max_size];
        // Fill List with random gameobjects
        SelectRandomObjects();
        // Create UI
        CreateDefaultUIText();

        scoringSystem = FindObjectOfType<ScoringSystem>();
        Invoke("GetCameraRig", 0.05f);
    }

    public void Update()
    {
        //Überprüfung aller GameObjekte im Array.
        //Befindet sich die Postion(in Unity immer der Mittelpunkt der geometrischen Form) eines GameObjekts innerhalb der Collider-Grenzen (collider.bounds) und das GameObjekt ist nicht der Eimer selbst(der Eimer befindet sich natürlich immer in den eigenen Collidergrenzen),
        // ...so wird dieses Objekt der Bucketliste hinzugefügt, falls es nicht schon in dieser vorhanden ist (Vermeidung von Redundanz).
        //Ist dies nicht der Fall, wird das Objekt wieder aus der BucketListe gelöscht.

        //Die Überprüfung funktioniert auch mit renderer.bounds, falls man es nicht über den Collider abfragen möchte oder der Eimer keinen Collider hätte.
        //Bei einer Box könnte der Collider bspw. die Objekte vom Hinzufügen blockieren, da keine Objekte durch einen Collider hindurch in einen Eimer geworfen werden können.
        //Da hier der Meshcollider aber direkt um das Eimerobjekt, um den Mesh, anliegt, ist das kein Hinderniss.

        foreach (GameObject gameObj in allGameObjects)
        {
            if (gameObj != null)
            {
                Vector3 position = gameObj.transform.position;

                if (bucketCollider.bounds.Contains(position))
                {
                    if (gameObj != bucket && gameObj != checkliste && !gameObj.transform.root.CompareTag("Player") && gameObj.layer != 11)
                    {
                        CheckGameObject(gameObj);
                    }
                }

                else
                {
                    if (controller.GetBucketObjects().Contains(gameObj))
                    {
                        controller.Remove(gameObj);
                    }
                }
            }

        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bucket.SetActive(false);
    }

    public void FetchAllPositions()
    {
        foreach (GameObject obj in allGameObjects)
        {
            // Target objects
            controller.AddPositions(obj.GetHashCode(), obj.transform.position);
        }
    }

    public void CheckGameObject(GameObject gameObj)
    {
            // Gameobject Tag und gelistete Tags müssen übereinstimmen
            if (isFound(gameObj) && !controller.GetBucketObjects().Contains(gameObj))
            {
                gameObj.GetComponent<VRTK_InteractableObject>().isGrabbable = false;
                controller.AddToBucketList(gameObj);
                controller.ResetMaterial(gameObj);
                //controller.SetPlayerScore(playerScore);
                //SetDefaultUIText(gameObj);
                EnableUICheck(gameObj);
                correctObject.Play();
                scoreValue = 25;

                //------Scoring ----------------------
                scoringSystem.AddLocalScore((sbyte)scoreValue);

                GameObject scoreText = null;

                //Spawn Text 25_Cube
                scoreText = Instantiate(score25, gameObject.transform.position, gameObject.transform.rotation);
        

                GameObject vorzeichen = Instantiate(scorePlus, gameObject.transform.position + new Vector3(0.5f, 0, 0), gameObject.transform.rotation);

                vorzeichen.transform.parent = scoreText.transform;

                //Vector3 dir = scoreText.transform.position - activeCameraRig.transform.position;
                scoreText.transform.LookAt(activeCameraRig.transform);

            }
            else if (!isFound(gameObj) && !controller.GetBucketObjects().Contains(gameObj))
            {
                // Set Gameobject back to it's original position
                Vector3 position = controller.FindOriginalPos(gameObj);
                delayCoroutine = DelayAndMove(gameObj, gameObj.transform.position, position, speed);
                StartCoroutine(delayCoroutine);
            

            if (!coroutineCalled)
            {
                StartCoroutine("FlashColor");
                //controller.ReducePlayerScore(playerScore);
                IncorrectObject.Play();

                //------Scoring ----------------------
                scoreValue = 10;

                scoringSystem.SubtractLocalScore((sbyte)scoreValue);
                GameObject scoreText = null;

                scoreText = Instantiate(score10, gameObject.transform.position, gameObject.transform.rotation);

                GameObject vorzeichen = Instantiate(scoreMinus, gameObject.transform.position + new Vector3(0.5f, 0, 0), gameObject.transform.rotation);

                vorzeichen.transform.parent = scoreText.transform;

                scoreText.transform.LookAt(activeCameraRig.transform);

            }
            else if (coroutineCalled)
                {
                    DisableDefaultUI();
                }

            }
    }

    private bool isFound(GameObject obj)
    {
        for (int i = 0; i < checkListObjects.Length; i++)
        {
            if (checkListObjects[i].tag == obj.tag && !controller.GetBucketObjects().Contains(obj))
            {
                return true;
            };
        }       
        return false;
    }

    /// <summary>
    /// Add random elements to list.
    /// </summary>
    private void SelectRandomObjects()
    {
        List<int> check = new List<int>();
        int i = 0;
        // Select random number       
        for (int j = 0; j < bucketListContent.Length; j++)
        {
            while (i < checkListObjects.Length)
            {
                int index = UnityEngine.Random.Range(j, bucketListContent.Length - 1);

                if (!check.Contains(index))
                {
                    // Select object  
                    GameObject obj = bucketListContent[index];
                    checkListObjects[i] = obj;
                    check.Add(index);
                    i++;
                }                
            }        
        }       
    }
    
    /// <summary>
    /// Adjust the color and flash ups
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashColor()
    {
        float step = (fadingTime / 0.5f) * Time.fixedDeltaTime;
        float t = 0;
        while (t <= 1.0f)
        {
            coroutineCalled = true;
            t += step;
            coroutineCalled = true;
            EnableErrorUI();
            yield return new WaitForSeconds(0.3f);
            DisableErrorUI();
            yield return new WaitForSeconds(0.3f);
        }
        DisableErrorUI();
        EnableDefaultUI();
        coroutineCalled = false;
    }

    private IEnumerator DelayAndMove(GameObject objectToMove, Vector3 a, Vector3 b, float speed)
    {
        yield return new WaitForSeconds(beginingDelay);
        // Make invisible
        // Check if all is contained
        if (objectToMove.GetComponent<Renderer>() != null && objectToMove.GetComponent<Collider>() != null)
        {

            objectToMove.GetComponent<Renderer>().enabled = false;
            objectToMove.GetComponent<Collider>().enabled = false;
            moveCoroutine = MoveFromTo(objectToMove, a, b, speed);
            // After the delay do..
            StartCoroutine(moveCoroutine);
        }
    }

    public IEnumerator MoveFromTo(GameObject objectToMove, Vector3 a, Vector3 b, float speed)
    {
        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        // Move out of bucket by finding the centre of bucket and then move up straight
        Vector3 pos = new Vector3(
            this.GetComponent<Renderer>().bounds.center.x,
            this.GetComponent<Renderer>().bounds.center.y + 2.00f,
            this.GetComponent<Renderer>().bounds.center.z);
        objectToMove.transform.position = pos;
        // Move gradiently back 
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            objectToMove.transform.position = Vector3.Lerp(pos, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        objectToMove.transform.position = b;
        // Set visible
        objectToMove.GetComponent<Renderer>().enabled = true;
        objectToMove.GetComponent<Collider>().enabled = true;
    }

    /// <summary>
    /// UI Canvas
    /// </summary>
    // UI Checklist activation / deactivation & Itemtext setup - Dynamically adjusts depending on BucketContent

    private void EnableErrorUI()
    {
        UIDefault.color = errorColor;
        UIDefault.enabled = true;
        checkListHeader.text = invalidText;
        checkListHeader.enabled = true;
    }

    private void DisableErrorUI()
    {
        UIDefault.enabled = false;
        checkListHeader.enabled = false;
    }

    private void EnableUICheck(GameObject obj)
    {
        for (int i = 0; i < checkListObjects.Length; i++)
        {
            if (obj.tag == checkListObjects[i].tag)
            {
                checkedObjects[i].transform.GetChild(0).GetComponent<RawImage>().color = white;
                ForceCanvasUpdate();
            }         
        }            
    }

    private void SetDefaultUIText(GameObject gameObj)
    {
        if (!listItems.ContainsKey(gameObj))
        {
            listItems.Add(gameObj, checkedObjects[textCounter]);
            GameObject checkedObject = listItems[gameObj] as GameObject;
            checkedObject.GetComponent<TextMeshProUGUI>().text = gameObj.name.ToString();
            checkedObject.SetActive(true);
            textCounter++;
            ForceCanvasUpdate();
        }
    }

    private void CreateDefaultUIText()
    {
        for (int i = 0; i < checkListObjects.Length; i++)
        {
            checkedObjects[i].GetComponent<TextMeshProUGUI>().text = checkListObjects[i].tag;
            checkedObjects[i].SetActive(true);
            checkedObjects[i].transform.GetChild(0).GetComponent<RawImage>().color = grey;
            ForceCanvasUpdate();
        }
    }

    private void EnableDefaultUI()
    {
        checkListHeader.text = checkListHeaderText;
        UIDefault.color = defaultColor;
        UIDefault.enabled = true;
        checkListHeader.enabled = true;

        for (int i = 0; i < textCounter; i++)
        {
            checkedObjects[i].SetActive(true);
        }

        ForceCanvasUpdate();
    }

    private void DisableDefaultUI()
    {
        for (int i = 0; i < textCounter; i++)
        {
            checkedObjects[i].SetActive(false);
        }
        ForceCanvasUpdate();
    }

    private void ForceCanvasUpdate()
    {
        Canvas.ForceUpdateCanvases();
        backgroundTransparent.enabled = false;
        backgroundDefault.enabled = false;
        backgroundTransparent.enabled = true;
        backgroundDefault.enabled = true;
    }

    void GetCameraRig()
    {
        activeCameraRig = GameObject.Find("CenterEyeAnchor");
    }
}