using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace rwby
{
    public class HealthManager : NetworkBehaviour
    {
        [Networked] public int Health { get; set; }
    }
}