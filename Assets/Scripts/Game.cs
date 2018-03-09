﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
/**
* Game contains static functions and constant expressions such as:
* default board size
* default number of camps per board
* default number of players per camp
* @author Harry Hollands
*/
public class Game : NetworkBehaviour
{
    public const uint NUMBER_OBSTACLES = 13;
    public const uint NUMBER_CAMPS = 5;
    public const uint PLAYERS_PER_CAMP = 5;
    public int currentPlayer = 1;

    // These are edited in Unity Component settings; 5 is just the default.
	public uint tileWidth = 5, tileHeight = 5;

    private Board board;

    //Handles spawning request? yes use this me
    public GameObject SpawnDice(Vector3 position, NetworkHash128 assetId)
    {
        return (GameObject)Instantiate(Resources.Load("Prefabs/Dice"), position, Quaternion.identity);
    }
    //handles unspawn request? maybe useful??
    public void UnSpawn(GameObject spawned)
    {
        Destroy(spawned);
    }

    [SyncVar]
    NetworkInstanceId diceID;

    [SyncVar]
    GameObject globalDice = null;

    IEnumerator WaitForDice()
    {
        yield return new WaitUntil(()=> this.globalDice != null);
    }

    IEnumerator WaitForServer()
    {
        yield return new WaitForSeconds(4);
    }
    void Start()
	{
        /*
        new TestBoard(50, 50, 5, 5); // Perform Board Unit Test
        new TestTile();
        new TestCamp();
        new TestPlayer();
        */
        Debug.Log(this.globalDice);
        if (isServer)
        {
            Dice dice = Dice.Create(new Vector3(10, 10, 10), new Vector3(), new Vector3(1, 1, 1));
            ClientScene.RegisterPrefab(dice.gameObject);
            diceID = dice.gameObject.GetComponent<NetworkIdentity>().netId;
            //Debug.Log(diceID);
            NetworkServer.Spawn(dice.gameObject);
            this.globalDice = dice.gameObject;
            //Debug.Log(this.globalDice);
            Debug.Log(this.globalDice.GetComponent<Dice>());
        }
        else
        {
            //Debug.Log(this.globalDice);
            WaitForDice();
            Dice dice = this.globalDice.AddComponent<Dice>();
            //Debug.Log(this.globalDice);
        }

        // Create a normal Board with Input attached. Both Board and InputController are attached to the root GameObject (this).
        if (!isServer)
        {
            Debug.Log("Waiting");
            WaitForServer();
        }
        Debug.Log(this.globalDice);
        Debug.Log(this.globalDice.GetComponent<Dice>());
        this.board = Board.Create(this.gameObject, tileWidth, tileHeight, this.globalDice.GetComponent<Dice>());
		this.board.gameObject.AddComponent<InputController>();   
        /*if(isServer)
        {
            NetworkServer.Spawn(this.board.gameObject);
        }*/
    }

    private void Update()
    {
        /*if (Input.GetKeyDown("r"))
        {
           this.board.GetDice.Roll(Camera.main.transform.position + new Vector3(0, 20, 0));
        }*/
        //To me tomorrow. This is important. 
        if(Input.GetKeyDown("r"))
        {
            GameObject dicePrefab = ((GameObject)Instantiate(Resources.Load("Prefabs/Dice")));
            NetworkHash128 assetId = dicePrefab.GetComponent<NetworkIdentity>().assetId;//Gets netID!!!
            //            ClientScene.RegisterSpawnHandler(assetId, SpawnDice, UnSpawnDice);  
            ClientScene.RegisterPrefab(dicePrefab);
            NetworkServer.Spawn(dicePrefab);

            //I offfer this sacrifice to the tiny snake god and pray for his help
            //All praise his shiny scales <3
        }
        if(Input.GetKeyDown("d"))
        {
            //UnSpawn(My parents disappointment); //ahaha a joke because,.. funny
        }
    }

    public void DiceRoll()
    {
        if (isServer)
        {
            Debug.Log("Server Rolling");
            Player last = this.board.gameObject.GetComponent<InputController>().LastClickedPlayer;
            if (last != null)
                this.board.GetDice.Roll(last.transform.position + new Vector3(0, 20, 0));
            else
                this.board.GetDice.Roll(Camera.main.transform.position + new Vector3(0, 20, 0));
        }
        else
        {
            Debug.Log("Sending Cmd");
            Player last = this.board.gameObject.GetComponent<InputController>().LastClickedPlayer;
            if (last != null)
                this.board.GetDice.Roll(last.transform.position + new Vector3(0, 20, 0));
            else
                this.board.GetDice.Roll(Camera.main.transform.position + new Vector3(0, 20, 0));
        }
    }
    /*[Command]
    public void CmdDiceRoll()
    {
        Player last = this.board.gameObject.GetComponent<InputController>().LastClickedPlayer;
        if (last != null)
            this.board.GetDice.Roll(last.transform.position + new Vector3(0, 20, 0));
        else
            this.board.GetDice.Roll(Camera.main.transform.position + new Vector3(0, 20, 0));
    }
    */
    void OnDestroy()
	{

	}

    /**
    * Returns the vertex of the GameObject parameter terrain (in world-space) positively furthest away from the origin (in model-space)
    * @author Harry Hollands
    * @param gameObject used to calculate how big the world is
    * @return a vector containing the furthest point from the origin used by the world
    */
	public static Vector3 MaxWorldSpace(GameObject gameObject)
	{
		Vector3 boundMax = gameObject.GetComponent<Terrain>().terrainData.bounds.max;
		return boundMax + gameObject.transform.position;
	}

    /**
    * Returns the vertex of the GameObject parameter terrain (in world-space) negatively furthest away from the origin (in model-space)
    * @author Harry Hollands
    * @param gameObject used to calculate the minimum of the world space
    * @return a vector containing the closest point to the origin used by the world
    */
	public static Vector3 MinWorldSpace(GameObject gameObject)
	{
		Vector3 boundMax = gameObject.GetComponent<Terrain>().terrainData.bounds.min;
		return boundMax + gameObject.transform.position;
	}

    /**
    * Returns the actual y-coordinate of the terrain-data in world-space.
    * @author Harry Hollands
    * @param gameObject the gameObject to retreive the y coordinate from
    * @param positionWorldSpace the x and z coordinate of the gameObjects desired y coordinate
    * @return returns the y coordinate
    */
	public static float InterpolateYWorldSpace(GameObject gameObject, Vector3 positionWorldSpace)
	{
		return gameObject.GetComponent<Terrain>().SampleHeight(positionWorldSpace);
	}

    public void BoardNextTurn()
    {
        this.board.NextTurn();
    }

}
