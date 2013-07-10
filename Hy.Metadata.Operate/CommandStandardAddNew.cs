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
            this.m_Caption = "���Ԫ���ݱ�׼";
            this.m_Message = "���Ԫ���ݱ�׼����";
        }

        FrmStandardProperty m_FrmAdd;
        public override void OnClick()
        {
            MetaStandard newStandard=new MetaStandard();
            newStandard.Name="�½���׼";
            newStandard.Creator = Environment.Application.UserName;
            newStandard.CreateTime = DateTime.Now;

            if (m_FrmAdd == null || m_FrmAdd.IsDisposed)
            {
                m_FrmAdd = new FrmStandardProperty();
                m_FrmAdd.ViewMode = FrmStandardProperty.enumPropertyViewMode.New;
                m_FrmAdd.Text = "�½�Ԫ���ݱ�׼";
            }
            m_FrmAdd.CurrentStandard = newStandard;
            if (m_FrmAdd.ShowDialog(base.m_Hook.UIHook.MainForm) == DialogResult.OK)
            {
                MetaStandardHelper.SaveStandard(m_FrmAdd.CurrentStandard);
            }

            this.m_Manager.Refresh();
        }
    }
}
