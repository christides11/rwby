using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby
{
    public interface ITimeProvider
    {
        public int GetTimeInSeconds();
    }
}