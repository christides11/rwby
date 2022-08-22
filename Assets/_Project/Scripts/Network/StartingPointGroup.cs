using System;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public class StartingPointGroup : MonoBehaviour
    {
        public List<GameObject> points = new List<GameObject>();
        public bool forTeam;
        public string[] tags = Array.Empty<string>();
    }
}