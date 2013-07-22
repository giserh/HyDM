using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Esri.DataManage.Command
{
    public class CommandStandardRefresh : DMStandardBaseCommand
    {
        
        public CommandStandardRefresh()
        {
            this.m_Caption = "ˢ��";
        }

        public override void OnClick()
        {
            this.m_Manager.Refresh();
        }
    }
}
