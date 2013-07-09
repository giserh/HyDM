using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Define;
using DevExpress.XtraBars.Docking;
using System.Windows.Forms;

namespace Skyline.Commands
{
    public class CommandTocControl : Common.Operate.CommandDockable
    {
        public CommandTocControl()
        {
            this.m_Category = "��ͼ����";
            this.m_Caption = "ͼ��Ŀ¼";
            this.m_Message = "����άͼ��Ŀ¼";
            this.m_Tooltip = "����Դ򿪻�ر���άͼ��Ŀ¼";
        }
        private AxTerraExplorerX.AxTEInformationWindow m_InfoTree;
        protected override Control CreateControl()
        {
            return new AxTerraExplorerX.AxTEInformationWindow();
        }

        protected override enumDockPosition DockPosition
        {
            get
            {
                return enumDockPosition.Left;
            }
        }

        protected override void Init()
        {
        }

        public override string Message
        {
            get
            {
                this.m_Message = this.Checked ? "�ر���άͼ��Ŀ¼" : "����άͼ��Ŀ¼";
                return base.Message;
            }
        }
    }
}
