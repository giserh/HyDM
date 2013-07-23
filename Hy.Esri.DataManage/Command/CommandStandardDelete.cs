using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;
using DevExpress.XtraEditors;

namespace Hy.Esri.DataManage.Command
{
    public class CommandStandardDelete : DMStandardBaseCommand
    {

        public CommandStandardDelete()
        {
            this.m_Caption = "ɾ��";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && m_Manager.SelectedItem!=null;
            }
        }

        public override void OnClick()
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("��ȷ��Ҫɾ����", "ɾ��ȷ��", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            if (Standard.Helper.StandardHelper.DeleteStandard(m_Manager.SelectedItem))
            {
                XtraMessageBox.Show("ɾ���ɹ�"); 
                this.m_Manager.Refresh();
            }
            else
            {
                XtraMessageBox.Show("ɾ��������������"); 
            }
        }
    }
}
