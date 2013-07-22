using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;
using Hy.Metadata.UI;
using Hy.Esri.DataManage.Standard;

namespace Hy.Esri.DataManage.Command
{
    public class CommandStandardFlushToDB : DMStandardBaseCommand
    {

        public CommandStandardFlushToDB()
        {
            this.m_Caption = "����ʵ��";
            this.m_Message = "�����ݿ��׼������ʵ�����ݿ���";
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && m_Manager.SelectedItem != null && m_Manager.SelectedItem.Type == enumItemType.Standard;
            }
        }

        public override void OnClick()
        {
            Hy.Esri.DataManage.Standard.Helper.Creator creator = new Standard.Helper.Creator();
            creator.fws = Environment.GisConnection as ESRI.ArcGIS.Geodatabase.IWorkspace;
            creator.StandardItem = m_Manager.SelectedItem;
            creator.OnMessage += base.SendMessage;
            creator.CreateToWorkspace();
        }
    }
}
