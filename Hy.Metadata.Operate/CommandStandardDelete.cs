using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;

namespace Hy.Metadata.Operate
{
    public class CommandStandardDelete : StandardBaseCommand
    {

        public CommandStandardDelete()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "Ԫ����";
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
            if (DevExpress.XtraEditors.XtraMessageBox.Show("ɾ����׼Ҳ��ɾ����ǰ��׼�������ݣ���ȷ��Ҫ�޸���", "ɾ��ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.m_Manager.SetEditStandard(this.m_Manager.SelectedMetaStandard);
            }
        }
    }
}
