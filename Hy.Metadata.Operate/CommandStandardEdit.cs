using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public class CommandStandardEdit : StandardBaseCommand
    {

        public CommandStandardEdit()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "�޸�Ԫ���ݱ�׼";
            this.m_Message = "�޸�Ԫ���ݱ�׼����";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && this.m_Manager.CurrentMetaStandard != null;
            }
        }

        FrmStandardProperty m_FrmEdit;
        public override void OnClick()
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("��׼���޸Ľ����´˱�׼�����ݱ��ؽ�����ȷ��Ҫ�޸���", "�޸�ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (m_FrmEdit == null || m_FrmEdit.IsDisposed)
                {
                    m_FrmEdit = new FrmStandardProperty();
                    m_FrmEdit.ViewMode = FrmStandardProperty.enumPropertyViewMode.Edit;
                }
                m_FrmEdit.CurrentStandard = m_Manager.CurrentMetaStandard;
                m_FrmEdit.Text = string.Format("Ԫ���ݱ�׼[{0}]�޸�", m_Manager.CurrentMetaStandard.Name);
                if (m_FrmEdit.ShowDialog(base.m_Hook.UIHook.MainForm) == DialogResult.OK)
                {
                    MetaStandardHelper.SaveStandard(m_FrmEdit.CurrentStandard);
                }

                this.m_Manager.Refresh();
            }
        }
    }
}
