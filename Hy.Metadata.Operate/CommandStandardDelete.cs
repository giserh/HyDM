using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;
using DevExpress.XtraEditors;

namespace Hy.Metadata.Operate
{
    public class CommandStandardDelete : StandardBaseCommand
    {

        public CommandStandardDelete()
        {
            this.m_Category = "Ԫ����";
            this.m_Caption = "ɾ��Ԫ���ݱ�׼";
            this.m_Message = "ɾ��Ԫ���ݱ�׼����";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && this.m_Manager.CurrentMetaStandard != null;
            }
        }

        public override void OnClick()
        {
            if (XtraMessageBox.Show("ɾ����׼Ҳ��ɾ����ǰ��׼�������ݣ���ȷ��Ҫ�޸���", "ɾ��ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (!MetaStandardHelper.DeleteStandard(m_Manager.CurrentMetaStandard))
                {
                    XtraMessageBox.Show(MetaStandardHelper.ErrorMessage);
                }
                else
                {
                    m_Manager.Refresh();
                }
            }
        }
    }
}
