using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "HurtboxCollection", menuName = "rwby/HurtboxCollection")]
    public class HurtboxCollection : ScriptableObject
    {
        [System.Serializable]
        public class HurtboxEntry
        {
            public int identifier;
            public StateHurtboxDefinition hurtboxDefinition;
        }

        protected Dictionary<int, StateHurtboxDefinition> hurtboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
        [SerializeField] protected HurtboxEntry[] hurtboxDefinitions = new HurtboxEntry[0];

        protected Dictionary<int, StateHurtboxDefinition> pushboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
        [SerializeField] protected HurtboxEntry[] pushboxDefinitions = new HurtboxEntry[0];

        public void OnEnable()
        {
            if (hurtboxDictionary == null)
            {
                hurtboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
            }
            if (pushboxDictionary == null)
            {
                pushboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
            }
            hurtboxDictionary.Clear();
            pushboxDictionary.Clear();
            for (int i = 0; i < hurtboxDefinitions.Length; i++)
            {
                if (hurtboxDictionary.ContainsKey(hurtboxDefinitions[i].identifier))
                {
                    Debug.LogError($"{name} HurtboxCollection has a duplicate entry for {hurtboxDefinitions[i].identifier}.");
                    continue;
                }
                hurtboxDictionary.Add(hurtboxDefinitions[i].identifier, hurtboxDefinitions[i].hurtboxDefinition);
            }
            for (int i = 0; i < pushboxDefinitions.Length; i++)
            {
                if (pushboxDictionary.ContainsKey(pushboxDefinitions[i].identifier))
                {
                    Debug.LogError($"{name} PushboxCollection has a duplicate entry for {pushboxDefinitions[i].identifier}.");
                    continue;
                }
                pushboxDictionary.Add(pushboxDefinitions[i].identifier, pushboxDefinitions[i].hurtboxDefinition);
            }
        }

        public StateHurtboxDefinition GetHurtbox(int identifier)
        {
            if (hurtboxDictionary.TryGetValue(identifier, out StateHurtboxDefinition hurtbox))
            {
                return hurtbox;
            }
            return null;
        }

        public StateHurtboxDefinition GetPushbox(int identifier)
        {
            if (pushboxDictionary.TryGetValue(identifier, out StateHurtboxDefinition pushbox))
            {
                return pushbox;
            }
            return null;
        }

        public bool TryGetAnimation(int identifier, out StateHurtboxDefinition hurtbox)
        {
            hurtbox = null;
            if (hurtboxDictionary.TryGetValue(identifier, out hurtbox))
            {
                return true;
            }
            return false;
        }
    }
}