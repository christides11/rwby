using HnSF.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    [CreateAssetMenu(fileName = "BoxCollectionDefinition", menuName = "rwby/boxcollectiondefinition")]
    public class BoxCollectionDefinition : HnSF.Combat.BoxCollectionDefinition
    {
        [SerializeReference] public List<HurtboxGroup> collboxGroups = new List<HurtboxGroup>();
        [SerializeReference] public List<HurtboxGroup> throwableboxGroups = new List<HurtboxGroup>();

        [SerializeField] public int hurtboxCount = 0;
        [SerializeField] public int collboxCount = 0;
        [SerializeField] public int throwableboxCount = 0;

        public void OnValidate()
        {
            hurtboxCount = 0;
            collboxCount = 0;
            throwableboxCount = 0;
            for (int j = 0; j < hurtboxGroups.Count; j++)
            {
                hurtboxCount += hurtboxGroups[j].boxes.Count;
            }
            for(int i = 0; i < collboxGroups.Count; i++)
            {
                collboxCount += collboxGroups[i].boxes.Count;
            }
            for(int g = 0; g < throwableboxGroups.Count; g++)
            {
                throwableboxCount += throwableboxGroups[g].boxes.Count;
            }
        }
    }
}