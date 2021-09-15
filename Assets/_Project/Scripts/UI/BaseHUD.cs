using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class BaseHUD : MonoBehaviour
    {

        protected ClientManager client;

        public virtual void SetClient(ClientManager client)
        {
            this.client = client;
        }

        public virtual void Update()
        {

        }
    }
}