using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rwby.ui
{
    public interface IMenuHandler
    {
        public bool Forward(int menu, bool autoClose = true);
        public bool Back();
        public IList GetHistory();
        public IMenu GetCurrentMenu();
    }
}