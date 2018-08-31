using System.Collections.Generic;
using UnityEngine;


public class BackGround : MonoBehaviour {
	
    public GameObject player; // GameObjact player from which will move backgrounds
    public bool nineImage;
    public List<BackGroundLayer> paramBackGround; // All background layers
    [HideInInspector]
    public List<GameObject> parallaxBackgroundLayer;
    [HideInInspector]
    public List<GameObject> moveBackGroundLayer;
    [HideInInspector]
    public List<GameObject> allCreateSprite;
    [HideInInspector]
    public List<Vector2> bounds;
    [HideInInspector]
    public List<Vector2> starPositionBackGroundLayer;
    [HideInInspector]
    public bool haveBackGround;
    [HideInInspector]
    int countBounds = 0;



    private void Start()
    {
        if (haveBackGround) {
            for (int i = 0; i < parallaxBackgroundLayer.Count; i ++) {
                starPositionBackGroundLayer.Add(parallaxBackgroundLayer[i].transform.position);
            }
        }
    }

    // Use this for initialization. Create and fill all layers of backgrounds
    public void CreateBackGround() {
        
        if (!haveBackGround) {
            for (int i = 0; i < paramBackGround.Count; i++)
            {
                CreateParallaxBackGround(i);
                CreateMoveBackGround(i, parallaxBackgroundLayer[i]);
                for (int j = 0; j < 3; j++) {
                    if (nineImage) {
                        GameObject parentHorizontal = CreateHorizontalParent(j);
                        parentHorizontal.transform.parent = moveBackGroundLayer[i].transform;
                        for (int y = 0; y < 3; y++) {
                            Init(moveBackGroundLayer[i], i, y, j).transform.parent = parentHorizontal.transform;
                        }
                    }
                    else { Init(moveBackGroundLayer[i], i, j, 0); }
                    }
                }
            haveBackGround = true;
            }
        }

    // We move the background depending on the player's position
    void LateUpdate() {
        if (haveBackGround) {
            for (int i = 0; i < paramBackGround.Count; i++) {
                // Parallax background displacement
                parallaxBackgroundLayer[i].transform.position = new Vector2(starPositionBackGroundLayer[i].x + player.transform.position.x * paramBackGround[i].parallaxSpeedX,
                                                                      starPositionBackGroundLayer[i].y + player.transform.position.y * paramBackGround[i].parallaxSpeedY);
                // Constant background motion
                moveBackGroundLayer[i].transform.position = new Vector2(moveBackGroundLayer[i].transform.position.x + Time.deltaTime * paramBackGround[i].backGroundSpeedX,
                                                                        moveBackGroundLayer[i].transform.position.y + Time.deltaTime * paramBackGround[i].backGroundSpeedY);
            }
            for (int j = 0; j < allCreateSprite.Count; j++) {
                CheckPosition(allCreateSprite[j], j);
            }
        }
    }

    //We check the background position if it is too far from the player, we move it to the next position
    void CheckPosition(GameObject myObject, int j) {
        // Left or Right
        if (myObject.transform.position.x < player.transform.position.x - 1.5f * bounds[j].x)
        {
            myObject.transform.position = new Vector2(myObject.transform.position.x + bounds[j].x * 3, myObject.transform.position.y);
        } else if (myObject.transform.position.x > player.transform.position.x + bounds[j].x * 1.5f)
        {
            myObject.transform.position = new Vector2(myObject.transform.position.x - bounds[j].x * 3, myObject.transform.position.y);
        }
        // Down or Up
        if (nineImage) {
            if (myObject.transform.position.y < player.transform.position.y - 1.5f * bounds[j].y)
            {
                myObject.transform.position = new Vector2(myObject.transform.position.x, myObject.transform.position.y + bounds[j].y * 3);
            }
            else if (myObject.transform.position.y > player.transform.position.y + bounds[j].y * 1.5f)
            {
                myObject.transform.position = new Vector2(myObject.transform.position.x, myObject.transform.position.y - bounds[j].y * 3);
            }
        }
    }

    // Creates and renames if we create a parallax on both the X axis and the Y axis
    GameObject CreateHorizontalParent(int count) {
        GameObject newHorizontalparrent = new GameObject();
        switch (count) {
            case 0:
                newHorizontalparrent.name = "Bottom Horizontal Parent";
                break;
            case 1:
                newHorizontalparrent.name = "Middle Horizontal Parent";
                break;
            case 2:
                newHorizontalparrent.name = "Top Horizontal Parent";
                break;
        }
        return newHorizontalparrent;
    }
    // Create an object that will be the child of our main object
    void CreateParallaxBackGround(int i) {
        GameObject parallaxObject = new GameObject();
        parallaxBackgroundLayer.Add(parallaxObject);
        parallaxObject.transform.parent = gameObject.transform;
        parallaxObject.name = "Parallax BackGround Layer_" + i;
    }
    // Create an object that will be the child of our parallax object
    void CreateMoveBackGround(int i, GameObject parallaxParent) { 
        GameObject moveBackGround = new GameObject();
        moveBackGroundLayer.Add(moveBackGround);
        moveBackGround.transform.parent = parallaxParent.transform;
        moveBackGround.name = "Move BackGround Layer_" + i;
    }

    // Object initialization. Creates, places and adds a sprite 
    GameObject Init(GameObject newObject, int i, int j, int y) {
        GameObject newObjectLayer = new GameObject();
        allCreateSprite.Add(newObjectLayer);
        newObjectLayer.transform.parent = newObject.transform;
        newObjectLayer.AddComponent<SpriteRenderer>().sprite = paramBackGround[i].mySprite;
        newObjectLayer.GetComponent<SpriteRenderer>().sortingLayerName = paramBackGround[i].layerName;
        newObjectLayer.GetComponent<SpriteRenderer>().sortingOrder = paramBackGround[i].orderInLayer;
        Vector2 boundsVector = new Vector2(newObjectLayer.GetComponent<SpriteRenderer>().sprite.bounds.size.x, newObjectLayer.GetComponent<SpriteRenderer>().sprite.bounds.size.y);
        bounds.Add(boundsVector);
        int flag = 0;
        if (nineImage) { flag = 1; }
        countBounds++;
        newObjectLayer.transform.position = new Vector2(-bounds[countBounds-1].x + bounds[countBounds - 1].x * j, -bounds[countBounds - 1].y * flag + bounds[countBounds - 1].y * y);
        newObjectLayer.name = "BackGroundCount";
        return newObjectLayer;
    }

    // Delete BackGround and Clear all List
    public void DeleteBackGround()
    {
        for (int i = 0; i < parallaxBackgroundLayer.Count; i++) {
            DestroyImmediate(parallaxBackgroundLayer[i]);
        }
        parallaxBackgroundLayer.Clear();
        moveBackGroundLayer.Clear();
        allCreateSprite.Clear();
        bounds.Clear();
        countBounds = 0;
        haveBackGround = false;
    }
}
