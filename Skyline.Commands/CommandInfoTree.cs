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

        protected override Common.Operate.CommandDockable.enumDockPosition DockPosition
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
    //public sealed class CommandTocControl : SkylineBaseCommand,ITool
    //{
    //    public CommandTocControl()
    //    {
    //        this.m_Category = "��ͼ����";
    //        this.m_Caption = "ͼ��Ŀ¼";
    //        this.m_Message = "��ͼ��Ŀ¼";
    //        this.m_Tooltip = "����Դ򿪻�ر���άͼ��Ŀ¼"; 
    //    }

    //    private AxTerraExplorerX.AxTEInformationWindow m_InfoTree;
    //    DockPanel m_DockPanel;
    //    public override void OnClick()
    //    {

    //        IUIHook uiHook = m_Hook as IUIHook;
    //        if (uiHook != null && uiHook.LeftDockPanel != null)
    //        {
    //            bool panelCreateFlag = (m_DockPanel == null);
    //            if (panelCreateFlag)
    //            {
    //                DockPanel dockLeft = uiHook.LeftDockPanel as DockPanel;
    //                m_DockPanel = dockLeft.AddPanel();
    //                m_DockPanel.Text = "��άͼ��Ŀ¼";
    //                //m_DockPanel.Controls.Add(dockLeft.Container as Control);
    //                //m_DockPanel.DockTo(dockLeft);
    //            }

    //            if (m_DockPanel.Visibility == DockVisibility.Visible && m_InfoTree != null)
    //            {
    //                m_DockPanel.Visibility = DockVisibility.Hidden;
    //            }
    //            else
    //            {
    //                if (m_InfoTree == null)
    //                {
    //                    m_InfoTree = new AxTerraExplorerX.AxTEInformationWindow();
    //                    ((System.ComponentModel.ISupportInitialize)(m_InfoTree)).BeginInit();
    //                    m_DockPanel.Controls.Add(m_InfoTree);
    //                    m_InfoTree.Dock = System.Windows.Forms.DockStyle.Fill;
    //                    ((System.ComponentModel.ISupportInitialize)(m_InfoTree)).EndInit();
    //                }
    //                else
    //                {
    //                    m_DockPanel.Controls.Add(m_InfoTree);
    //                    m_InfoTree.Dock = System.Windows.Forms.DockStyle.Fill;
    //                }

    //                m_DockPanel.Visibility = DockVisibility.Visible;
    //                if (panelCreateFlag)
    //                {
    //                    m_DockPanel.Visibility = DockVisibility.Hidden;
    //                    m_DockPanel.Visibility = DockVisibility.Visible;
    //                }
    //                //m_DockPanel.BringToFront();
    //                m_DockPanel.Show();
    //                //dockLeft.Visibility = DockVisibility.Visible;
    //            }
    //        }
    //    }

    //    public override bool Enabled
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //    public override bool Checked
    //    {
    //        get
    //        {
    //            return m_InfoTree != null && m_InfoTree.Visible;
    //        }
    //    }

    //    public override string Message
    //    {
    //        get
    //        {
    //            this.m_Message = this.Checked ? "�ر�ͼ��Ŀ¼" : "��ͼ��Ŀ¼";
    //            return base.Message;
    //        }
    //    }

    //    public object Resource
    //    {
    //        get { return m_InfoTree; }
    //    }

    //    public bool Release()
    //    {
    //        return true;
    //    }
    //}
}
