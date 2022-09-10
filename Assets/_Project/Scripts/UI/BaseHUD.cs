using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace rwby
{
    public class BaseHUD : SimulationBehaviour
    {
        public Canvas canvas;
        public ClientManager client;
        public int playerIndex;
        public CameraSwitcher cameraSwitcher;
        public FighterManager playerFighter;

        public List<HUDElement> hudElements = new List<HUDElement>();

        public virtual void SetClient(ClientManager client, int playerIndex)
        {
            this.client = client;
            this.playerIndex = playerIndex;
        }

        public virtual void AddHUDElement(HUDElement hudElement)
        {
            hudElement.InitializeElement(this);
            hudElements.Add(hudElement);
        }

        public override void Render()
        {
            base.Render();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].UpdateElement(this);
            }
        }
    }
}