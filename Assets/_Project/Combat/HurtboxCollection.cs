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

        protected Dictionary<int, StateHurtboxDefinition> collboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
        [SerializeField] protected HurtboxEntry[] collboxDefinitions = new HurtboxEntry[0];

        public void OnEnable()
        {
            if (hurtboxDictionary == null)
            {
                hurtboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
            }
            if (collboxDictionary == null)
            {
                collboxDictionary = new Dictionary<int, StateHurtboxDefinition>();
            }
            hurtboxDictionary.Clear();
            collboxDictionary.Clear();
            for (int i = 0; i < hurtboxDefinitions.Length; i++)
            {
                if (hurtboxDictionary.ContainsKey(hurtboxDefinitions[i].identifier))
                {
                    Debug.LogError($"{name} HurtboxCollection has a duplicate entry for {hurtboxDefinitions[i].identifier}.");
                    continue;
                }
                hurtboxDictionary.Add(hurtboxDefinitions[i].identifier, hurtboxDefinitions[i].hurtboxDefinition);
            }
            for (int i = 0; i < collboxDefinitions.Length; i++)
            {
                if (collboxDictionary.ContainsKey(collboxDefinitions[i].identifier))
                {
                    Debug.LogError($"{name} CollboxCollection has a duplicate entry for {collboxDefinitions[i].identifier}.");
                    continue;
                }
                collboxDictionary.Add(collboxDefinitions[i].identifier, collboxDefinitions[i].hurtboxDefinition);
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

        public StateHurtboxDefinition GetCollbox(int identifier)
        {
            if (collboxDictionary.TryGetValue(identifier, out StateHurtboxDefinition pushbox))
            {
                return pushbox;
            }
            return null;
        }

        public bool TryGetHurtbox(int identifier, out StateHurtboxDefinition hurtbox)
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