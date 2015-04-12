using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolKits.Domain
{
    [Serializable]
    public class SettingUI
    {
        public string asset_root = "";
        public string packTag = "";
        public bool mipmap = false;

        public override string ToString()
        {
            return "Root[" + asset_root + "]Mipmap[" + mipmap + "]";
        }
    }
}
