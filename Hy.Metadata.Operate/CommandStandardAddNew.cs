using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public  class CommandStandardAddNew : StandardBaseCommand
    {
        public CommandStandardAddNew()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "���";
        }


        public override void OnClick()
        {
            MetaStandard standard= this.m_Manager.NewStandard();
            this.m_Manager.SetEditStandard(standard);
        }
    }
}
