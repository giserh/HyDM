using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public class CommandStandardProperty : StandardBaseCommand
    {

        public CommandStandardProperty()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "����";
            this.m_Message = "�鿴Ԫ���ݱ�׼����";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && this.m_Manager.CurrentMetaStandard != null;
            }
        }
        FrmStandardProperty m_FrmProperty;
        public override void OnClick()
        {

            if (m_FrmProperty == null || m_FrmProperty.IsDisposed)
            {
                m_FrmProperty = new FrmStandardProperty();
                m_FrmProperty.ViewMode = FrmStandardProperty.enumPropertyViewMode.View;
            }
            m_FrmProperty.CurrentStandard = m_Manager.CurrentMetaStandard;
            m_FrmProperty.Text = string.Format("�鿴Ԫ���ݱ�׼[{0}]�Ķ���", m_Manager.CurrentMetaStandard.Name);
            m_FrmProperty.ShowDialog(base.m_Hook.UIHook.MainForm);
            

        }
    }
}
