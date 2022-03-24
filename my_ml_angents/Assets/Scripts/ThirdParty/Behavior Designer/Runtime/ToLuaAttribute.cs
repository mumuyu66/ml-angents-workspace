using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorDesigner.Runtime
{
    public enum ToLuaType
    {
        Normal,Function
    }
    public class ToLuaAttribute: Attribute
    {
        public ToLuaType type;
        public string luaName;
        public ToLuaAttribute(string luaName, ToLuaType type = ToLuaType.Normal)
        {
            this.luaName = luaName;
            this.type = type;
        }
       
    }
}
