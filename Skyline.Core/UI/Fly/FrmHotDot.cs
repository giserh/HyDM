using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TerraExplorerX;

namespace Skyline.Core.UI
{
    public partial class FrmHotDot : FrmBase
    {
        private Form _frmMain;
        TreeNode tn;
        public FrmHotDot(Form FrmMain)
        {
            _frmMain = FrmMain;

            if (base.BeginForm(FrmMain))
            {
                InitializeComponent();
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// ��½�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmHotDot_Load(object sender, EventArgs e)
        {
            base.FrmName = "�ȵ�����";
            this.InitTree();
        }

        /// <summary>
        /// ��ʼ����״�ṹ
        /// </summary>
        public void InitTree()
        {
            int groupID = Program.TE.FindItem("�ȵ�");
            if (groupID==0)
            {
              //  groupID = Program.TE.CreateGroup("�ȵ�", 0);
                return;
            }
            int childId = Program.TE.GetNextItem(groupID, ItemCode.CHILD);
            while (childId != 0)
            {
                try
                {
                    ITerrainLocation5 itl = (ITerrainLocation5)Program.TE.GetObjectEx(childId, "ITerrainLocation5");
                    //������ýӿ������������ص�Ҳ��һ���ӿ�
                    //yon

                    TreeNode tn = new TreeNode(itl.Description);
                    tn.Tag = itl;
                    tn.ImageIndex = 0;
                    tn.SelectedImageIndex = 0;
                    this.tree_hotDot.Nodes.Add(tn);

                    childId = Program.TE.GetNextItem(childId, ItemCode.NEXT);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// �趨�ȵ����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.txt_name.Text == ""
                || this.txt_name.Text.Equals(null))
            //this.txt_name.Text.Equals(null)���û�п�����
            //���ң���������ô�����ģ�
            {
                MessageBox.Show("�������뵱ǰ��ע������ƣ�Ȼ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                int groupId = Program.TE.FindItem("�ȵ�");
                if (groupId == 0)
                {
                    groupId = Program.TE.CreateGroup("�ȵ�", 0);

                }
                GetPositionInfo gp = new GetPositionInfo();
                gp.GetPosition();

                ITerrainLocation5 itl = Program.TE.IObjectManager51_CreateLocation(gp.Longitude, gp.Latitude, gp.Height, gp.Yaw, gp.CameraDeltaPitch, 90, HeightStyleCode.HSC_DEFAULT, groupId, this.txt_name.Text.ToString());
                this.tree_hotDot.Nodes.Clear();
                this.InitTree();
            }
            catch (Exception)
            {
            
            }
        
        }
        /// <summary>
        /// ���ڵ����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void tree_hotDot_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                tn = tree_hotDot.GetNodeAt(e.X, e.Y);
                if (tn != null)
                {
                    tree_hotDot.SelectedNode = tn;
                    tn.ContextMenuStrip = contextMenuStrip1;
                }
            }
        }

        private void ɾ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ITerrainLocation5 itdo = (ITerrainLocation5)tn.Tag;
            Program.TE.DeleteItem(itdo.InfoTreeItemID);
            tree_hotDot.Nodes.Remove(tn);
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tree_hotDot.LabelEdit = true;
            tn.BeginEdit();
        }

        private void tree_hotDot_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string objectId = ((ITerrainLocation5)e.Node.Tag).ID;
            Program.TE.FlyToObject(objectId, ActionCode.AC_FLYTO);
        }
        private void tree_hotDot_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            ITerrainLocation5 itdo = (ITerrainLocation5)tn.Tag;
            //itdo.Text = e.Label;
            itdo.Description = e.Label;
            tree_hotDot.LabelEdit = false;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (this.txt_name.Text == ""
                || this.txt_name.Text.Equals(null))
            {
                MessageBox.Show("�������뵱ǰ��ע������ƣ�Ȼ��������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int groupId = Program.TE.FindItem("�ȵ�");
            if (groupId == 0)
            {
                groupId = Program.TE.CreateGroup("�ȵ�", 0);

            }
            try
            {
                GetPositionInfo gp = new GetPositionInfo();
                gp.GetPosition();

                ITerrainLocation5 itl = Program.TE.IObjectManager51_CreateLocation(gp.Longitude, gp.Latitude, gp.Height, gp.Yaw, gp.CameraDeltaPitch, 90, HeightStyleCode.HSC_DEFAULT, groupId, this.txt_name.Text.ToString());
                this.tree_hotDot.Nodes.Clear();
                this.InitTree();
            }
            catch (Exception)
            {
                
               
            }
            
        }

        private void FrmHotDot_FormClosing(object sender, FormClosingEventArgs e)
        {
            _frmMain.RemoveOwnedForm(this);
        }


    }
}