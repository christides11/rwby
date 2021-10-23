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
            public BoxCollectionDefinition hurtboxDefinition;
        }

        protected Dictionary<int, BoxCollectionDefinition> hurtboxDictionary = new Dictionary<int, BoxCollectionDefinition>();
        [SerializeField] protected HurtboxEntry[] hurtboxDefinitions = new HurtboxEntry[0];

        public void OnEnable()
        {
            if (hurtboxDictionary == null)
            {
                hurtboxDictionary = new Dictionary<int, BoxCollectionDefinition>();
            }
            hurtboxDictionary.Clear();
            for (int i = 0; i < hurtboxDefinitions.Length; i++)
            {
                if (hurtboxDictionary.ContainsKey(hurtboxDefinitions[i].identifier))
                {
                    Debug.LogError($"{name} HurtboxCollection has a duplicate entry for {hurtboxDefinitions[i].identifier}.");
                    continue;
                }
                hurtboxDictionary.Add(hurtboxDefinitions[i].identifier, hurtboxDefinitions[i].hurtboxDefinition);
            }
        }

        public BoxCollectionDefinition GetHurtbox(int identifier)
        {
            if (hurtboxDictionary.TryGetValue(identifier, out BoxCollectionDefinition hurtbox))
            {
                return hurtbox;
            }
            return null;
        }

        public bool TryGetHurtbox(int identifier, out BoxCollectionDefinition hurtbox)
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