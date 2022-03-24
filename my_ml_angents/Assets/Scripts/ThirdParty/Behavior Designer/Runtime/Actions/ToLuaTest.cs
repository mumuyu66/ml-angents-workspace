using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class ToLuaTest : Action
    {
        [ToLua(nameof(testString))]
        public string testString;

        [ToLua(nameof(testInt))]
        public int testInt;

        [ToLua(nameof(testFloat))]
        public float testFloat;

        [ToLua(nameof(testVector2))]
        public Vector2 testVector2;
    }
}
