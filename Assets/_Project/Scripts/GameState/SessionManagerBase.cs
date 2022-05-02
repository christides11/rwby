using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class SessionManagerBase : NetworkBehaviour
    {
        [Networked, Capacity(5)] public NetworkLinkedList<CustomSceneRef> currentLoadedScenes => default;

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
        }

        public override void Render()
        {
            base.Render();
            if (sessionHandlerID == -1)
            {
                sessionHandlerID = GameManager.singleton.networkManager.GetSessionIDByRunner(Runner);
                GameManager.singleton.networkManager.sessions[sessionHandlerID].sessionManager = this;
            }
        }
    }
}