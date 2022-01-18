using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class BaseHUD : MonoBehaviour
    {

        protected ClientManager client;
        protected int playerIndex;

        public virtual void SetClient(ClientManager client, int playerIndex)
        {
            this.client = client;
            this.playerIndex = playerIndex;
        }

        public virtual void Update()
        {

        }
    }
}