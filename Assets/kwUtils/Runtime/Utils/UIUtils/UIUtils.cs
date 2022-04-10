using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KaizerWaldCode.Utils
{
    public static class UIUtils
    {
        public static Button InitButton(in VisualElement root, string bName, Action action = null, bool enable = true)
        {
            Button b = root.Q<Button>(bName);
            b.SetEnabled(enable);
            b.clickable.clicked += action;
            return b;
        }
        
        public static void OnQuit()
        {
            Application.Quit();
        }
    }
}