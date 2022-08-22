using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RespawnPointGroup : MonoBehaviour
{
    [FormerlySerializedAs("spawnPoints")] public List<GameObject> points = new List<GameObject>();
    public string[] tags = Array.Empty<string>();
}
