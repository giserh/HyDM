using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Esri.DataManage.UI;
using Hy.Esri.DataManage.Standard;

namespace Hy.Esri.DataManage.Command
{
    public  class CommandStandardAdd : DMStandardBaseCommand
    {
        public CommandStandardAdd()
        {
            this.m_Caption = "�½����ݿ��׼";
        }

        FrmFeatureDatasetInfo m_FrmAdd;
        public override void OnClick()
        {
            if (m_FrmAdd == null || m_FrmAdd.IsDisposed)
            {
                m_FrmAdd = new FrmFeatureDatasetInfo();
                m_FrmAdd.Text = "�½����ݿ��׼";
            }

            StandardItem sItem = new StandardItem();
            sItem.Name = "�½����ݿ��׼";
            m_FrmAdd.StandardItem = sItem;
            if (m_FrmAdd.ShowDialog() == DialogResult.OK)
            {
                sItem = m_FrmAdd.StandardItem;
                sItem.Type = enumItemType.Standard;
                Environment.NhibernateHelper.SaveObject(sItem);
                Environment.NhibernateHelper.Flush();
                this.m_Manager.Refresh();
            }
        }
    }
}
