using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolKits.Domain
{
    [Serializable]
    public class Setting
    {
        //ui素材根目录
        public SettingUI settingUI;
        public SettingModel settingModel;

        public Setting()
        {
            settingUI = new SettingUI();
            settingModel = new SettingModel();
        }
        
    }
}
