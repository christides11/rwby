using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class SessionManagerBase : NetworkBehaviour
    {
        [Networked, Capacity(5)]
        public NetworkLinkedList<CustomSceneRef> currentLoadedScenes { get; } = MakeInitializer(new CustomSceneRef[] {new CustomSceneRef(0, 0, 0, 1)});

        [Networked] public byte teams { get; set; }
        [Networked] public int maxPlayersPerClient { get; set; }

        public ClientContentLoaderService clientContentLoaderService;
        public ClientMapLoaderService clientMapLoaderService;
        
        [HideInInspector] public GameManager gameManager;
        [HideInInspector] public ContentManager contentManager;

        public int sessionHandlerID = -1;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameManager = GameManager.singleton;
            contentManager = gameManager.contentManager;
            
            sessionHandlerID = GameManager.singleton.networkManager.GetSessionHandlerIDByRunner(Runner);
            GameManager.singleton.networkManager.sessions[sessionHandlerID].sessionManager = this;
        }

        public override void Spawned()
        {
            base.Spawned();
            maxPlayersPerClient = 4;
            teams = 1;
        }

        public override void Render()
        {
            base.Render();
            if (sessionHandlerID == -1)
            {
                sessionHandlerID = GameManager.singleton.networkManager.GetSessionHandlerIDByRunner(Runner);
                GameManager.singleton.networkManager.sessions[sessionHandlerID].sessionManager = this;
            }
        }

        public virtual void InitializeClient(ClientManager clientManager)
        {
            
        }

        public virtual void UpdateClientPlayerCount(ClientManager clientManager, uint oldAmount)
        {
            
        }

        public void SetMaxPlayersPerClient(int max)
        {
            if (max < 0 || max > 4) return;
            maxPlayersPerClient = max;
        }
        
        public void SetTeamCount(byte count)
        {
            teams = count;
        }
    }
}