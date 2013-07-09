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
            this.m_Caption = "�޸�";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && this.m_Manager.SelectedMetaStandard != null;
            }
        }

        public override void OnClick()
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("�ֶ���Ϣ���޸Ľ�ɾ����ǰ��׼�������ݣ���ȷ��Ҫ�޸���", "ɾ��ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Hy.Metadata.MetaStandardHelper.DeleteStandard(m_Manager.SelectedMetaStandard);
            }
        }
    }
}
