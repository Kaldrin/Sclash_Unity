﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using TMPro;
using Photon.Pun;
using Photon.Realtime;

using Steamworks;



// Script that handles main network stuff
[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public class ConnectManager : MonoBehaviourPunCallbacks, IConnectionCallbacks
{
    public static ConnectManager Instance;
    AudioManager audioManager;



    bool isConnecting = false;
    string gameVersion = "3.1.5";
    public bool connectedToMaster = false;
    public bool enableMultiplayer;
    public GameObject localPlayer;
    public int localPlayerNum;
    GameObject[] spawners;
    bool twoPlayersInRoom = false;

    [SerializeField] private Animator RightPanel;
    [SerializeField] private Animator LeftPanel;
    [SerializeField] private GameObject multiplayerMenu;

    [SerializeField] GameObject[] spawnlist;
    [SerializeField] TMP_InputField[] parametersInputs = null;
    int localPlayerSide = 1;


    string[] newRoomParameters = { "rn", "pc", "rc" };
    ExitGames.Client.Photon.Hashtable customRoomProperties;
    readonly string DefaultRoomName = "ROOM NAME";


    // RESTART
    bool localWantToRestart = false;
    bool remoteWantsToRestart = false;



    [Header("ONLINE MENU")]
    [SerializeField] GameObject connectionWindow = null;
    [SerializeField] GameObject serverListBrowser = null;
    [SerializeField] MenuBrowser multiplayerBrowser = null;
    [SerializeField] GameObject backBrowser = null;
    [SerializeField] GameObject rightPart = null;
    [SerializeField] GameObject leftPart = null;
    [SerializeField] GameObject screenTitle = null;
    [SerializeField] GameObject backIndicator = null;


    [Header("OTHER MENUS")]
    [SerializeField] MenuBrowser pauseBrowser = null;
    [SerializeField] MenuBrowser winBrowser = null;



    [Header("CHARACTER CHANGE")]
    [SerializeField] List<GameObject> characterChangeDisplays = new List<GameObject>(2);



    [Header("ONLINE MESSAGES")]
    [SerializeField] GameObject waitingForPlayerMessage = null;
    [SerializeField] GameObject playerDisconnectedMessage = null;
    [SerializeField] GameObject timeoutWindow = null;
    [SerializeField] GameObject serverErrorMessage = null;
    [SerializeField] GameObject restartSentMessage = null;
    [SerializeField] GameObject restartReceivedMessage = null;
    [SerializeField] GameObject cantRestartMessage = null;
    [SerializeField] GameObject restartAcceptedMessage = null;

    #region Events
    public delegate void Disconnected();
    public static event Disconnected PlayerDisconnected;
    public delegate void Connected();
    public static event Connected PlayerConnected;
    public delegate void Joined();
    public static event Joined PlayerJoined;

    #endregion

    #region BASE FUNCTIONS
    void Awake()                                                                                // AWAKE
    {
        Instance = this;
        audioManager = FindObjectOfType<AudioManager>();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NetworkingClient.EnableLobbyStatistics = true;
        gameVersion = Application.version;

        // SET UP MESSAGES
        SetUpMessages();
    }

    void Start()                                                                                // START
    {
        if (enableMultiplayer)
            Connect();


        spawners = GameManager.Instance.playerSpawns;
    }


    void Update()                                                                               // UPDATE
    {
        if (enabled && isActiveAndEnabled)
            if (enableMultiplayer && !PhotonNetwork.IsConnected && isConnecting)
                Connect();
    }
    #endregion








    public void Connect()
    {
        Debug.Log("Connecting ...");

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            isConnecting = true;
        }


        // CONNEXION WAIT
        ConnexionWaitDisplay(true);
    }


    public void JoinRoom()
    {
        if (!connectedToMaster)
        {
            if (!enableMultiplayer)
                GameManager.Instance.StartMatch();
            else
                Debug.LogWarning("Can't join room if you're not connected");
            return;
        }


        PhotonNetwork.JoinRandomRoom();
    }


    public void JoinSelectedRoom(TextMeshProUGUI roomName)
    {
        InitMultiplayerMatch(false);

        if (roomName.text != DefaultRoomName)
        {
            Debug.Log($"Join {roomName.text} room");
            PhotonNetwork.JoinRoom(roomName.text);
        }
        else
        {
            Debug.Log("Join random room");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public void SetMultiplayer(bool multiplayer)
    {
        enableMultiplayer = multiplayer;
    }










    #region Init room
    /// <summary>
    /// Initialise tous les paramètres pour se connecter au mode en ligne.
    /// </summary>
    /// <param name="isJoining"></param>


    public void InitMultiplayerMatch(bool isJoining = true)
    {
        // DELETE EXISTING PLAYERS IN SCENE //
        foreach (GameObject p in GameManager.Instance.playersList)
            Destroy(p);

        GameManager.Instance.playersList.Clear();


        //  JOIN OR CREATE ROOM  //
        if (isJoining)
            JoinRoom();



        twoPlayersInRoom = false;
    }


    IEnumerator JoinRoomCoroutine()
    {
        // INSTANTIATE PLAYER //
        ///Get player spawns
        Vector3 spawnPos = spawners[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.position;
        GameObject newPlayer = null;
        try
        {
            newPlayer = PhotonNetwork.Instantiate("Prefabs/PlayerNetwork", spawnPos, Quaternion.identity);
        }
        catch
        {
            Debug.Log("Error");

            // MENU
            if (backIndicator != null)
                backIndicator.SetActive(true);
            if (serverErrorMessage != null)
                serverErrorMessage.SetActive(true);
            if (winBrowser != null)
                winBrowser.enabled = false;
            if (pauseBrowser != null)
                pauseBrowser.enabled = false;
        }


        if (SteamManager.Initialized)
            newPlayer.name = SteamFriends.GetPersonaName();
        else
            newPlayer.name = "Player" + PhotonNetwork.CurrentRoom.PlayerCount;


        CameraManager.Instance.FindPlayers();


        Debug.LogFormat("Spawning on spawner {0} : {1}", spawners[PhotonNetwork.CurrentRoom.PlayerCount - 1], spawnPos);


        Player stats = newPlayer.GetComponent<Player>();
        PlayerAnimations playerAnimations = newPlayer.GetComponent<PlayerAnimations>();


        ///Manage Input
        InputManager.Instance.playerInputs = new InputManager.PlayerInputs[1];
        stats.playerNum = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        stats.ResetAllPlayerValuesForNextMatch();




        // CHARACTER CHANGE
        if (stats.playerNum > 0)
        {
            if (characterChangeDisplays[0] != null)
                characterChangeDisplays[0].SetActive(false);
            if (characterChangeDisplays[1] != null)
                characterChangeDisplays[1].SetActive(true);
        }
        else if (stats.playerNum == 0)
        {
            if (characterChangeDisplays[0] != null)
                characterChangeDisplays[0].SetActive(true);
            if (characterChangeDisplays[1] != null)
                characterChangeDisplays[1].SetActive(false);
        }




        ///Animation setup
        //playerAnimations.spriteRenderer.color = GameManager.Instance.playersColors[stats.playerNum];
        //splayerAnimations.legsSpriteRenderer.color = GameManager.Instance.playersColors[stats.playerNum];

        playerAnimations.spriteRenderer.sortingOrder = 10 * stats.playerNum;
        playerAnimations.legsSpriteRenderer.sortingOrder = 10 * stats.playerNum;
        //playerAnimations.legsMask.GetComponent<SpriteMask>().frontSortingOrder = 10 * stats.playerNum + 2;
        //playerAnimations.legsMask.GetComponent<SpriteMask>().backSortingOrder = 10 * stats.playerNum - 2;


        ///FX Setup
        ParticleSystem attackSignParticles = stats.attackRangeFX.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule attackSignParticlesMain = attackSignParticles.main;
        attackSignParticlesMain.startColor = GameManager.Instance.attackSignColors[stats.playerNum];


        if (!GameManager.Instance.playersList.Contains(newPlayer))
            GameManager.Instance.playersList.Add(newPlayer);






        stats.characterNameDisplay.text = GameManager.Instance.charactersData.charactersList[0].name;
        stats.characterNameDisplay.color = GameManager.Instance.playersColors[stats.playerNum];
        stats.characterIdentificationArrow.color = GameManager.Instance.playersColors[stats.playerNum];

        yield return new WaitForEndOfFrame();


        // INIT MATCH //
        stats.SwitchState(Player.STATE.sneathed);


        /// Wait 0.1 seconds
        yield return new WaitForSeconds(1f);


        // SLIDE ANIM
        WaitScene();


        GameManager.Instance.SwitchState(GameManager.GAMESTATE.game);
        AudioManager.Instance.SwitchAudioState(AudioManager.AUDIOSTATE.beforeBattle);
        CameraManager.Instance.SwitchState(CameraManager.CAMERASTATE.battle);


        // AUDIO
        audioManager.matchBeginsRandomSoundSource.Play();



        // STAGE
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncMap", RpcTarget.AllBuffered, MapLoader.Instance.currentMapIndex);
            photonView.RPC("SyncRoundCount", RpcTarget.AllBuffered, GameManager.Instance.scoreToWin);
        }

        //audioManager.ActivateWind();


        /// Wait 0.5 seconds
        yield return new WaitForSeconds(0.5f);





        Debug.Log("PUN : OnJoinedRoom() called by PUN. Player is now in a room");
        Debug.LogFormat("Players in room : {0}", PhotonNetwork.CurrentRoom.PlayerCount);


        if (PhotonNetwork.CurrentRoom.PlayerCount < 2 && waitingForPlayerMessage != null)
            waitingForPlayerMessage.SetActive(true);







        // RESET MENU
        multiplayerBrowser.enabled = true;


        yield return new WaitForSeconds(GameManager.Instance.timeBeforeBattleCameraActivationWhenGameStarts);



        CameraManager.Instance.actualXSmoothMovementsMultiplier = CameraManager.Instance.battleXSmoothMovementsMultiplier;
        CameraManager.Instance.actualZoomSpeed = CameraManager.Instance.battleZoomSpeed;
        CameraManager.Instance.actualZoomSmoothDuration = CameraManager.Instance.battleZoomSmoothDuration;
    }
    #endregion





    #region Monobehaviour Callbacks

    public override void OnConnectedToMaster()
    {
        PlayerConnected.Invoke();

        isConnecting = false;
        connectedToMaster = true;
        Debug.Log("PUN : OnConnectedToMaster() was called by PUN");
        PhotonNetwork.JoinLobby();



        twoPlayersInRoom = false;


        // CONNEXION WINDOW
        ConnexionWaitDisplay(false);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("PUN : OnJoinedLobby() was called by PUN");
        //InitMultiplayerMatch();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ServerListManager.Instance.roomInfosList = roomList;
        ServerListManager.Instance.DisplayServerList();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("PUN : Failed to join a room");
        Debug.Log("PUN : CreateRoom called");



        // Room name
        string randRoomName = "Matchmaking " + Time.time.ToString();



        if (SteamManager.Initialized)
        {
            randRoomName = SteamFriends.GetPersonaName();
            randRoomName += "'s room";
        }


        RoomOptions options = new RoomOptions();
        options.CustomRoomPropertiesForLobby = newRoomParameters;


        customRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            {"rn", randRoomName },
            {"pc", "2" },
            {"rc", "5"}
        };

        options.CustomRoomProperties = customRoomProperties;


        if (parametersInputs.Length > 0)
        {
            if (parametersInputs[1].text != "")
                options.MaxPlayers = byte.Parse(parametersInputs[1].text);
            else
                options.MaxPlayers = 2;
        }


        localPlayerSide = 0;
        PhotonNetwork.CreateRoom(randRoomName, options);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room successfully created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed : " + message);
    }


    public void LeaveLobby()
    {
        Debug.Log("Leave Lobby called");
        PhotonNetwork.Disconnect();
        localPlayerSide = 1;

        twoPlayersInRoom = false;



        // CONNEXION WINDOW
        ConnexionWaitDisplay(false);
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        connectedToMaster = false;
        Debug.LogWarning(cause);

        SetMultiplayer(false);
        PlayerDisconnected.Invoke();


        twoPlayersInRoom = false;

        localPlayerSide = 1;


        // CHARACTER CHANGERS
        if (characterChangeDisplays != null && characterChangeDisplays.Count > 0)
            for (int i = 0; i < characterChangeDisplays.Count; i++)
                if (characterChangeDisplays[i] != null && !characterChangeDisplays[i].activeInHierarchy)
                    characterChangeDisplays[i].SetActive(true);



        // MESSAGE
        if (waitingForPlayerMessage != null)
            waitingForPlayerMessage.SetActive(false);
        if (playerDisconnectedMessage != null)
            playerDisconnectedMessage.SetActive(false);



        // TIME OUT DISPLAY
        if (cause == DisconnectCause.ClientTimeout)
            if (timeoutWindow != null)
                timeoutWindow.SetActive(true);
            else
                Debug.Log("Can't find timeout window, ignoring");
    }


    public override void OnJoinedRoom()
    {
        StartCoroutine(JoinRoomCoroutine());
    }






    [PunRPC]
    void GetAllPlayers()
    {
        Debug.Log("Get all players in room");


        if (PhotonNetwork.CurrentRoom.PlayerCount != GameManager.Instance.playersList.Count)
        {
            GameManager.Instance.playersList.Clear();


            Debug.Log("Update playerlists");


            Player p2 = null;
            Player[] playersStats = FindObjectsOfType<Player>();


            for (int i = 0; i < playersStats.Length; i++)
            {
                GameManager.Instance.playersList.Add(playersStats[i].gameObject);


                //Instantiate 2nd player
                p2 = playersStats[i];
                PlayerAnimations p2Anim = playersStats[i].gameObject.GetComponent<PlayerAnimations>();

                //p2Anim.spriteRenderer.color = GameManager.Instance.playersColors[p2.playerNum];
                //p2Anim.legsSpriteRenderer.color = GameManager.Instance.playersColors[p2.playerNum];

                p2Anim.spriteRenderer.sortingOrder = 10 * p2.playerNum;
                p2Anim.legsSpriteRenderer.sortingOrder = 10 * p2.playerNum;
                //p2Anim.legsMask.GetComponent<SpriteMask>().frontSortingOrder = 10 * p2.playerNum + 2;
                //p2Anim.legsMask.GetComponent<SpriteMask>().backSortingOrder = 10 * p2.playerNum - 2;


                ///FX Setup
                ParticleSystem attackSignParticles = p2.attackRangeFX.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule attackSignParticlesMain = attackSignParticles.main;
                attackSignParticlesMain.startColor = GameManager.Instance.attackSignColors[p2.playerNum];
            }


            if (PhotonNetwork.CurrentRoom.PlayerCount != GameManager.Instance.playersList.Count)
                Invoke("WaitForInstantiation", 0.5f);
            else
            {
                for (int i = 0; i < GameManager.Instance.playersList.Count; i++)
                {
                    Debug.LogFormat("Player list n°{0} : {1}",
                        i,
                        GameManager.Instance.playersList[i]
                    );


                    if (GameManager.Instance.playersList[i] == null)
                    {
                        Invoke("WaitForInstantiation", 0.5f);
                        return;
                    }
                }


                foreach (Player p in playersStats)
                    p.ManageOrientation();


                Debug.Log("All players are here");

                if (waitingForPlayerMessage != null)
                    waitingForPlayerMessage.SetActive(false);

                CameraManager.Instance.FindPlayers();
            }

            PlayerJoined.Invoke();
        }
    }






    void WaitForInstantiation()
    {
        photonView.RPC("GetAllPlayers", RpcTarget.AllViaServer);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("PUN : OnLeftRoom() was called by PUN");
        PhotonNetwork.Disconnect();
        CameraManager.Instance.FindPlayers();
        GameManager.Instance.playersList = new List<GameObject>(2);
        localPlayerSide = 1;

        GameManager.Instance.Start();
        InputManager.Instance.playerInputs = new InputManager.PlayerInputs[2];
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }


        Debug.LogFormat("{0} joined the room", other.NickName);
        Debug.LogFormat("Players in room : {0}", PhotonNetwork.CurrentRoom.PlayerCount);


        GameObject otherPlayer = GameManager.Instance.GetOtherPlayer(localPlayer);


        if (otherPlayer == null)
        {
            photonView.RPC("GetAllPlayers", RpcTarget.AllViaServer);
            return;
        }


        GameManager.Instance.playersList.Add(otherPlayer);


        if (PhotonNetwork.IsMasterClient)
        {
            foreach (GameObject p in GameManager.Instance.playersList)
            {
                int _tempPNum = p.GetComponent<Player>().playerNum;

                p.transform.position = GameManager.Instance.playerSpawns[_tempPNum].transform.position;
                p.transform.rotation = GameManager.Instance.playerSpawns[_tempPNum].transform.rotation;

                p.GetComponent<PlayerAnimations>().ResetAnimsForNextRound();
                p.GetComponent<Player>().SwitchState(Player.STATE.normal);

                p.GetComponent<PhotonView>().RPC("ResetPos", RpcTarget.AllViaServer);
            }
        }
        PlayerJoined.Invoke();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (GameManager.Instance.gameState == GameManager.GAMESTATE.game)
        {
            //GameManager.Instance.
            Debug.LogWarning("Player Left in the middle of the game");

            //SceneManage.Instance.Restart();
            GameManager.Instance.APlayerLeft();


            if (characterChangeDisplays != null && characterChangeDisplays.Count > 0)
                for (int i = 0; i < characterChangeDisplays.Count; i++)
                    if (characterChangeDisplays[i] != null)
                    {
                        characterChangeDisplays[i].SetActive(true);
                        if (characterChangeDisplays[i].GetComponent<Animator>())
                            characterChangeDisplays[i].GetComponent<Animator>().SetBool("On", false);
                        //characterChangeDisplays[i].SetActive(false);

                    }
        }
        else
            Debug.Log("Player left room");


        // MESSAGE
        if (playerDisconnectedMessage != null)
            playerDisconnectedMessage.SetActive(true);
        if (backIndicator != null)
            backIndicator.SetActive(true);
        if (winBrowser != null)
            winBrowser.enabled = false;
        if (pauseBrowser != null)
            pauseBrowser.enabled = false;

        base.OnPlayerLeftRoom(otherPlayer);
    }


    public void CreateCustomRoom()
    {
        InitMultiplayerMatch(false);
        localPlayerSide = 0;

        RoomOptions options = new RoomOptions();
        options.CustomRoomPropertiesForLobby = newRoomParameters;

        foreach (TMP_InputField input in parametersInputs)
        {
            if (input.text == "")
                input.text = input.placeholder.GetComponent<TextMeshProUGUI>().text;
        }

        Debug.LogFormat("{0} {1} {2}", parametersInputs[0].text, parametersInputs[1].text, parametersInputs[2].text);

        customRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            {"rn", parametersInputs[0].text },
            {"pc", parametersInputs[1].text },
            {"rc", parametersInputs[2].text }
        };


        options.CustomRoomProperties = customRoomProperties;
        GameManager.Instance.scoreToWin = int.Parse(parametersInputs[2].text);


        if (parametersInputs[1].text != "")
            options.MaxPlayers = byte.Parse(parametersInputs[1].text);
        else
            options.MaxPlayers = 2;


        PhotonNetwork.CreateRoom(parametersInputs[0].text, options);
    }


    public void WaitSceneLoading()
    {
        //Invoke("WaitScene", 0.5f);
    }


    void WaitScene()
    {
        if (RightPanel == null)
        {
            RightPanel = GameObject.Find("RightPanel").GetComponent<Animator>();
            Debug.LogWarning("Panel not referenced in script", gameObject);
        }
        if (LeftPanel == null)
        {
            LeftPanel = GameObject.Find("LeftPanel").GetComponent<Animator>();
            Debug.LogWarning("Panel not referenced in script", gameObject);
        }

        RightPanel.Play("SlideOut");
        LeftPanel.Play("SlideOut");


        // DISABLE MENU
        if (rightPart != null)
            rightPart.SetActive(false);
        if (leftPart != null)
            leftPart.SetActive(false);
        if (screenTitle != null)
            screenTitle.SetActive(false);


        Invoke("DisableMenu", 0.25f);
    }


    void DisableMenu()
    {
        if (multiplayerMenu == null)
        {
            multiplayerMenu = GameObject.Find("MultiplayerMenu");
            Debug.LogWarning("Multiplayer menu not referenced in the script", gameObject);
        }

        multiplayerMenu.SetActive(false);
    }

    void ResetGame()
    {
        Debug.Log("PUN : OnLeftRoom() was called by PUN");
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        CameraManager.Instance.FindPlayers();
        GameManager.Instance.playersList = new List<GameObject>(2);

        GameManager.Instance.Start();

        InputManager.Instance.playerInputs = new InputManager.PlayerInputs[2];
    }


    [PunRPC]
    // SYNC STAGE OF THE JOINING PLAYER
    void SyncMap(int targetMapIndex)
    {
        Debug.Log("SyncMap called");
        MapLoader.Instance.SetMap(targetMapIndex, false);
    }


    [PunRPC]
    void SyncRoundCount(int targetScoreWin)
    {
        GameManager.Instance.scoreToWin = targetScoreWin;
        GameManager.Instance.maxScoreTextDisplay.text = targetScoreWin.ToString();
    }
    #endregion






    // MENUS
    void ConnexionWaitDisplay(bool state = false)
    {
        if (connectionWindow != null)
            connectionWindow.SetActive(state);
        else
            Debug.Log("Can't find connection window, ignoring");

        if (serverListBrowser != null)
            serverListBrowser.SetActive(!state);
        else
            Debug.Log("Can't find server list browser, ignoring");

        if (multiplayerBrowser != null)
            multiplayerBrowser.gameObject.SetActive(!state);
        else
            Debug.Log("Can't find multiplayer browser, ignoring");

        if (backBrowser != null)
            backBrowser.SetActive(state);
        else
            Debug.Log("Can't find multiplayer browser, ignoring");

        if (timeoutWindow != null)
            timeoutWindow.SetActive(false);
        else
            Debug.Log("Can't find timeout window, ignoring");
    }

    void SetUpMessages()
    {
        if (waitingForPlayerMessage != null)
            waitingForPlayerMessage.SetActive(false);
        if (playerDisconnectedMessage != null)
            playerDisconnectedMessage.SetActive(false);
        if (timeoutWindow != null)
            timeoutWindow.SetActive(false);
        if (restartSentMessage != null)
            restartSentMessage.SetActive(false);
        if (restartReceivedMessage != null)
            restartReceivedMessage.SetActive(false);
        if (cantRestartMessage != null)
            cantRestartMessage.SetActive(false);
        if (restartAcceptedMessage != null)
            restartAcceptedMessage.SetActive(false);
    }











    #region RESTART
    public void RestartCall()
    {
        if (GameManager.Instance != null && GameManager.Instance.playersList.Count < 2)
        {
            // MESSAGE
            if (cantRestartMessage != null)
                cantRestartMessage.SetActive(true);
        }
        else
        {
            if (!localWantToRestart)
                localWantToRestart = true;


            if (!remoteWantsToRestart)
            {
                // MESSAGE
                if (restartSentMessage != null)
                    restartSentMessage.SetActive(true);
            }

            // SEND
            Debug.Log("Sent restart request");
            photonView.RPC("RestartRequestReceived", RpcTarget.Others);
        }
    }


    [PunRPC]
    public void RestartRequestReceived()
    {
        if (GameManager.Instance != null && GameManager.Instance.playersList.Count > 1)
        {
            if (!remoteWantsToRestart)
                remoteWantsToRestart = true;

            Debug.Log("Restart request received");
            if (localWantToRestart)
            {
                // CONFIRM SEND
                RestartOnlineDuel();
                photonView.RPC("RestartOnlineDuel", RpcTarget.Others);
            }
            else
            {
                // MESSAGE
                if (restartReceivedMessage != null)
                    restartReceivedMessage.SetActive(true);
            }
        }
    }


    [PunRPC]
    public void RestartOnlineDuel()
    {
        // Initiator starts restart process
        // Receives the remote confirmation
        // Restarts locally and  calls the remote restart

        if (restartAcceptedMessage != null)
            restartAcceptedMessage.SetActive(true);

        Debug.Log("Restart confirmed");


        // PAUSE
        MenuManager.Instance.PauseOff();
        MenuManager.Instance.winScreen.SetActive(false);


        Invoke("FXRestartOnlineDuel", 2f);
        Invoke("ActuallyRestartOnlineDuel", 3.5f);
    }


    void FXRestartOnlineDuel()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.roundTransitionLeavesFX.Play();
    }


    void ActuallyRestartOnlineDuel()
    {
        Debug.Log("Restart!");
        // SCORE
        if (GameManager.Instance != null)
        {
            GameManager.Instance.score = new Vector2(0, 0);
            for (int i = 0; i < GameManager.Instance.scoresDisplays.Count; i++)
                GameManager.Instance.scoresDisplays[i].text = "0";
        }



        // PLAYERS
        if (GameManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.playersList.Count; i++)
            {
                // Reset
                Player playerScript = GameManager.Instance.playersList[i].GetComponent<Player>();
                playerScript.ResetAllPlayerValuesForNextMatch();
                playerScript.SwitchState(Player.STATE.sneathed);


                // Position
                if (playerScript.GetComponent<PhotonView>() && playerScript.GetComponent<PhotonView>().IsMine)
                    playerScript.transform.position = spawners[localPlayerSide].transform.position;

                Debug.Log(localPlayerSide);
            }
            GameManager.Instance.allPlayersHaveDrawn = false;
        }

        localWantToRestart = false;
        remoteWantsToRestart = false;
        




        // AUDIO
        if (AudioManager.Instance != null)
            AudioManager.Instance.SwitchAudioState(AudioManager.AUDIOSTATE.beforeBattle);


        // FILTER
        if (GameManager.Instance != null)
            if (GameManager.Instance.gameState == GameManager.GAMESTATE.finished)
                GameManager.Instance.TriggerMatchEndFilterEffect(false);


        // STAGE
        if (GameManager.Instance != null && MapLoader.Instance != null)
        {
            if (GameManager.Instance.gameState == GameManager.GAMESTATE.finished)
            {
                if (localPlayerSide == 0)
                {
                    int nextStageIndex = GameManager.Instance.CalculateNextStageIndex();
                    MapLoader.Instance.SetMap(nextStageIndex, false);
                    photonView.RPC("RestartSyncStage", RpcTarget.Others, nextStageIndex);
                }
            }
            else
                MapLoader.Instance.SetMap(MapLoader.Instance.currentMapIndex, false);
        }
    }


    [PunRPC]
    void RestartSyncStage(int stageIndex)
    {
        MapLoader.Instance.SetMap(stageIndex, false);
    }
    #endregion
}