using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;

namespace Hy.Check.Demo.Helper
{
    public class CmdDevExpressAdapter
    {
        private RibbonControl ribbonctrl;
        private CommandManager m_cmdManager = new CommandManager();
        /// <summary>
        /// ����������
        /// </summary>
        public CommandManager CmdManager
        {
            get
            {
                return m_cmdManager;
            }
            set
            {
                m_cmdManager = value;
            }
        }

        public AxToolbarControl ToolbarControl
        {
            set { m_cmdManager.ToolbarControl = value; }
        }

        public RibbonControl RibbonCtrl
        {
            set
            {
                //barManager1 = value;
                //barManager1.ItemClick += new ItemClickEventHandler(barManager1_ItemClick);
                ribbonctrl = value;
            }
        }

        public bool InitCommand(int viewtype)
        {
            try
            {
                if (viewtype == 0)
                    m_cmdManager.LoadDefaultMapCommand();
                else if (viewtype == 1)
                    m_cmdManager.LoadDefaultPageCommand();

                UpdateToolbar();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// ����ⲿ����
        /// </summary>
        /// <param name="cmdprogid"></param>
        /// <returns></returns>
        public bool AddCommand(string cmdprogid)
        {
            return m_cmdManager.AddCommand(cmdprogid);
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="cmdprogids"></param>
        /// <returns></returns>
        public bool AddCommands(string[] cmdprogids)
        {
            foreach (string progid in cmdprogids)
            {
                AddCommand(progid);
            }
            return true;
        }

        /// <summary>
        /// ���¹�����ͼ���״̬������ʼʱ
        /// </summary>
        public void UpdateToolbar()
        {
            Bitmap bitmap = null;
            string progID = "";
            UID pUid = new UIDClass();
            ICommandPool pCmdPool = m_cmdManager.ToolbarControl.CommandPool;

            for (int i = 0; i < ribbonctrl.Items.Count; i++)
            {
                BarItem baritem = ribbonctrl.Items[i];
                if (baritem == null || baritem.Tag == null || baritem.Tag.ToString().Equals("")) continue;
                progID = baritem.Tag.ToString();
                try
                {
                    pUid.Value = m_cmdManager.GetUIDFromStr(progID);
                    ICommand pCmd = pCmdPool.FindByUID(pUid);
                    if (pCmd != null)
                    {
                        if (!baritem.IsImageExist && pCmd.Bitmap != 0)
                            //if (!baritem.IsImageExist)
                        {
                            bitmap = Bitmap.FromHbitmap((IntPtr) pCmd.Bitmap);
                            bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
                            baritem.Glyph = bitmap;
                        }
                        try
                        {
                            //��ʾ��Ϣ
                            if (baritem.Hint.Length == 0)
                                baritem.Hint = pCmd.Tooltip;
                            //����״̬
                            baritem.Enabled = pCmd.Enabled;
                        }
                        catch 
                        {
                        }
                    }
                    baritem.ItemClick += barButtonItem_ItemClick;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("װ�ع���"+progID+"ʱ����:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// ���¹��߿���״̬
        /// </summary>
        public void RefreshButtonState()
        {
            try
            {
                string progID = "";
                UID pUid = new UIDClass();
                ICommandPool pCmdPool = m_cmdManager.ToolbarControl.CommandPool;

                for (int i = 0; i < ribbonctrl.Items.Count; i++)
                {
                    BarItem baritem = ribbonctrl.Items[i];
                    if (baritem == null || baritem.Tag == null) continue;
                    progID = baritem.Tag.ToString();
                    try
                    {
                        pUid.Value = m_cmdManager.GetUIDFromStr(progID); //progID;
                        ICommand pCmd = pCmdPool.FindByUID(pUid);
                        if (pCmd != null)
                        {
                            baritem.Enabled = pCmd.Enabled;
                        }
                    }
                    catch 
                    {
                    }
                }
            }
            catch 
            {
            }
        }

        /// <summary>
        /// ���߰�ť�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.Item.Tag == null) return;
                string progID = e.Item.Tag.ToString();
                if (m_cmdManager.ExecuteCommand(progID))
                {
                    RefreshButtonState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("����{0}����:{1},��ջ:{2}", e.ToString(), ex.Message, ex.StackTrace));
            }
        }

        public bool ExecuteCommand(string progID)
        {
            return m_cmdManager.ExecuteCommand(progID);
        }

        public bool saveCmdToFile(string strcmdfile)
        {
            return m_cmdManager.SaveToFile(strcmdfile);
        }
    }
}