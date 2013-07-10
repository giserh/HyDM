using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public class CommandStandardRefresh : StandardBaseCommand
    {
        
        public CommandStandardRefresh()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "ˢ��";
            this.m_Message = "ˢ��Ԫ���ݱ�׼������";
        }


        public override void OnClick()
        {
            this.m_Manager.Refresh();
        }
    }
}
