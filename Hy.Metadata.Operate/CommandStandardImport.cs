using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public  class CommandStandardImport : StandardBaseCommand
    {

        public CommandStandardImport()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "����Ԫ����";
            this.m_Message = "���뵱ǰ��׼�µ�Ԫ����";
        }


        public override void OnClick()
        {
            FrmMetadataImport frmImport = new FrmMetadataImport();
            frmImport.CurrentStandard = base.m_Manager.CurrentMetaStandard;
            frmImport.ShowDialog(base.m_Hook.UIHook.MainForm);
        }
    }
}
