//
// Vyas Dhimant : 101199558 
// Multiplayer Systems 
// Assignment 1
//
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;


public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public GameObject playerGO; //Player.
    public string myAddress;
    private string defMessage;

    public List<Cube> currentPlayerList;
    public List<string> newPlayerList;
    public List<string> droppedPlayerList;

    public float cubeX;
    public float cubeY;
    public float cubeZ;



    private void SetCube()
    {
        myAddress = "init";
        cubeX = 1; cubeY = 1; cubeZ = 1; ///Lets Set the cube to a fix position.
    }



    // Start is called before the first frame update
    void Start()
    {
        udp = new UdpClient();

        udp.Connect("localhost", 12345); //udp.Connect("3.22.203.136",12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);

        InvokeRepeating("SendPosition", 1, 0.01f);  //Need to send all the position things at a fast pace so 0.01 repeatrate.

    }

    void OnDestroy(){
        udp.Dispose();
    }

    [Serializable] 
    public class Position
    {
        public Vector3 position;
    }

    [Serializable]
    public class Player
    {
        [Serializable]
        public struct receivedPosition
        {
            public float x;
            public float y;
            public float z;
        }
        public string id;
        public receivedPosition position;
    }

    [Serializable] 
    public class NewPlayer
    {
        public Player player;     //New Player Class.
    }

    [Serializable]
    public class ListOfPlayers
    {
        public Player[] players;

    }

    [Serializable]
    public class ListOfdroppedPlayerList  //To delete the players
    {
        public string address;
    }

    public enum commands{
        NEW_CLIENT,  //0
        UPDATE,       //1
        PLAYER_DISCONNECTED,//2
        LIST_OF_PLAYERS,//3
    };

    [Serializable]
    public class Message
    {
        public commands cmd;
    }


    [Serializable]
    public class GameState
    {
        public Player[] players;
    }

    public Message latestMessage;
    public GameState ActiveGameState;

    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);

        Debug.Log("Got this: " + returnData);
        defMessage = "Got this: " + returnData;   //saving it in a variable so that easily can be used later.


        latestMessage= JsonUtility.FromJson<Message>(returnData);

        try{
            switch (latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    Debug.Log("New Player"); //Lets LOG
                    Debug.Log(defMessage);  //Used what we made in a variable.
                    NewPlayer p = JsonUtility.FromJson<NewPlayer>(returnData);
                    if (myAddress == "init")
                    {
                        myAddress = p.player.id;  //Assigning the id to my new spanned Player.
                    }
                    newPlayerList.Add(p.player.id); //Spawn Player with this ID.
                    break;

                case commands.UPDATE:
                    ActiveGameState = JsonUtility.FromJson<GameState>(returnData);  
                    break;

                case commands.PLAYER_DISCONNECTED:
                    ListOfdroppedPlayerList drop = JsonUtility.FromJson<ListOfdroppedPlayerList>(returnData);
                    Debug.Log("Player Gone.");
                    Debug.Log(defMessage);
                    droppedPlayerList.Add(drop.address);  //Adding to the drop list so that in next update can be deleted.
                    //Drop the player
                    break;
                case commands.LIST_OF_PLAYERS:
                    ListOfPlayers gamePlayers = JsonUtility.FromJson<ListOfPlayers>(returnData);
                    for (int i = 0; i < gamePlayers.players.Length; i++)
                    {
                        if (gamePlayers.players[i].id != myAddress)
                        {
                            newPlayerList.Add(gamePlayers.players[i].id);  //Adding to the add list so that in next update can be added.
                        }
                    }
                    break;
                default:
                    Debug.Log("Error: " + returnData);
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers()
    {
        //Spawn the Cube
        for (int i = 0; i < newPlayerList.Count; i++)
        {
            GameObject NewCube = Instantiate(playerGO, Vector3.zero, Quaternion.identity);  // Getting the game object which is a cube player prefab.
            NewCube.GetComponent<Cube>().Address = newPlayerList[i];
            currentPlayerList.Add(NewCube.GetComponent<Cube>());  //Adding to the list.
        }
        newPlayerList.Clear();
        newPlayerList.TrimExcess();
    }

    void UpdatePlayers()
    {
        for (int i = 0; i < ActiveGameState.players.Length; i++)
        {
            if (ActiveGameState.players[i].id != myAddress)
            {
                for (int j = 0; j < currentPlayerList.Count; j++)
                {
                    if (currentPlayerList[j].Address == ActiveGameState.players[i].id)
                    { 
                        float x = ActiveGameState.players[i].position.x;   //
                        float y = ActiveGameState.players[i].position.y;  // Lets set the positions.
                        float z = ActiveGameState.players[i].position.z;  //
                        currentPlayerList[j].transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }
    }

    void deletePlayers( string address)
    {
        for (int i = 0; i < currentPlayerList.Count; i++)
        {
            if (currentPlayerList[i].Address == address)
            {
                currentPlayerList[i].gameObject.SendMessage("deleteCube");  
            }
        }
    }

    void deleteDropped()
    {
        for (int i = 0; i < droppedPlayerList.Count; i++)
        {
            deletePlayers(droppedPlayerList[i]);        // was it in the drop list? Lets Delete that.
        }
        droppedPlayerList.Clear();
        droppedPlayerList.TrimExcess();
    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }
    void SendPosition()
    {
        Position cube = new Position();   // Getting the cube from my Cube Script and setting its positon according to my cube set function.
        cube.position.x = cubeX;
        cube.position.y = cubeY;
        cube.position.z = cubeZ;
        Byte[] sendPosition = Encoding.ASCII.GetBytes(JsonUtility.ToJson(cube));
        udp.Send(sendPosition, sendPosition.Length);

    }
    void Update(){
        SpawnPlayers();  
        UpdatePlayers();
        deleteDropped();
    }
}
