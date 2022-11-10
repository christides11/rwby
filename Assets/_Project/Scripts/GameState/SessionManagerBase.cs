using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    [OrderBefore(typeof(ClientManager), typeof(GameModeBase))]
    public class SessionManagerBase : NetworkBehaviour, IContentLoad
    {
        public IEnumerable<ModGUIDContentReference> loadedContent
        {
            get { return BuildLoadedContentList(); }
        }

        [Networked, Capacity(5)]
        public NetworkLinkedList<CustomSceneRef> currentLoadedScenes { get; } = MakeInitializer(new CustomSceneRef[]
        {
            new CustomSceneRef(new ContentGUID(8), 0, 1)
        });

        [Networked, Capacity(8)] public NetworkLinkedList<TeamDefinition> teamDefinitions => default;
        [Networked] public byte maxPlayersPerClient { get; set; }

        public ClientContentLoaderService clientContentLoaderService;
        public ClientContentUnLoaderService clientContentUnloaderService;
        
        [HideInInspector] public GameManager gameManager;
        [HideInInspector] public ContentManager contentManager;

        public int sessionHandlerID = -1;

        protected virtual void Awake()
        {
            gameManager = GameManager.singleton;
            contentManager = gameManager.contentManager;
        }

        public override void Spawned()
        {
            base.Spawned();
            maxPlayersPerClient = 4;
            
            sessionHandlerID = GameManager.singleton.networkManager.GetSessionHandlerIDByRunner(Runner);
            GameManager.singleton.networkManager.sessions[sessionHandlerID].sessionManager = this;
            DontDestroyOnLoad(gameObject);
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
            maxPlayersPerClient = (byte)max;
        }

        public void SetTeamDefinitions(TeamDefinition[] teamDefinitions)
        {
            this.teamDefinitions.Clear();
            for (int i = 0; i < teamDefinitions.Length; i++)
            {
                this.teamDefinitions.Add(teamDefinitions[i]);
            }
        }

        protected virtual HashSet<ModGUIDContentReference> BuildLoadedContentList()
        {
            HashSet<ModGUIDContentReference> references = new HashSet<ModGUIDContentReference>();
            
            for (int i = 0; i < currentLoadedScenes.Count; i++)
            {
                /*
                references.Add(new ModObjectGUIDReference()
                {
                    modIdentifier = new ModIdentifierTuple(currentLoadedScenes[i].source, currentLoadedScenes[i].modIdentifier),
                    objectIdentifier = currentLoadedScenes[i].mapIdentifier
                });*/
            }
            
            return references;
        }
    }
}