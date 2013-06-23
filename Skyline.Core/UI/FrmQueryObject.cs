using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using System.IO;
using System.Configuration;
using TerraExplorerX;

namespace Skyline.Core.UI
{
    public partial class FrmQueryObject:FrmBase
    {
        private string ModelID = ConfigurationManager.AppSettings["ModelID"];
        private IWorkspace pWorkspace;
        private IFeatureClass tFeatureClass;
        private Form _frmMain;
         /// <summary>
        /// ����ʱ ��������������
        /// </summary>
        /// <param name="FrmMain"></param>
        public FrmQueryObject(Form FrmMain)
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
        /// ���������¼� 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmQueryObject_Load(object sender, EventArgs e)
        {
            base.FrmName = "���Բ�ѯ";
            try
            {
                IFeatureWorkspace pFeatureWorkspace = Program.pWorkspace as IFeatureWorkspace;
                this.tFeatureClass = pFeatureWorkspace.OpenFeatureClass(ConfigurationManager.AppSettings["ModleTable"]);
            }
            catch (Exception ex)
            {
                
                //throw;
            }
         
        }

        /// <summary>
        /// ����ر��¼� �ͷ���Դ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmQueryObject_FormClosing(object sender, FormClosingEventArgs e)
        {
            _frmMain.RemoveOwnedForm(this);
            this.Dispose();
            this.Close();
        }

        /// <summary>
        /// ������¼�
        /// ����һ�ε��ʱ ȥ����ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_selectWhere_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.txt_selectWhere.Text == "������Ҫ��ѯ����������")
            {
                this.txt_selectWhere.Text = "";
            }
        }

        private void txt_selectWhere_Leave(object sender, EventArgs e)
        {
            if (this.txt_selectWhere.Text == "" || this.txt_selectWhere.Equals(null))
            {
                this.txt_selectWhere.Text = "������Ҫ��ѯ����������";
            }
        }

        /// <summary>
        /// ��ѯ��ť����¼�
        /// ִ�в�ѯ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_selectWhere_Click(object sender, EventArgs e)
        {
            try
            {
                BuilderObjectBiz bob = new BuilderObjectBiz();
                List<File3dattribute> list = bob.GetModelByName(this.txt_selectWhere.Text.ToString().Trim());

                this.InitTree(list);
            }
            catch (Exception)
            {
                
             
            }
         
        }

        public void InitTree(List<File3dattribute> list)
        {
            try
            {
                this.tree_dataSet.Nodes.Clear();

                for (int i = 0; i < list.Count; i++)
                {
                    File3dattribute bo = list[i];
                    TreeNode tn = new TreeNode(bo.Mc);
                    tn.Tag = bo;
                    tn.ImageIndex = 1;
                    this.tree_dataSet.Nodes.Add(tn);
                }
            }
            catch (Exception)
            {
                
              
            }
          
            
            
        }

   

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                BuilderObjectBiz bob = new BuilderObjectBiz();
                List<File3dattribute> list = bob.GetModelByName(this.txt_selectWhere.Text.ToString().Trim());
                this.InitTree(list);
            }
            catch (Exception)
            {
              
            }
            
        }

        private void tree_dataSet_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                File3dattribute bo = (File3dattribute)e.Node.Tag;
                QueryFilter pQuery = new QueryFilterClass();

                pQuery.WhereClause = ModelID + " = " + bo.Objectid.ToString();
                IFeatureCursor featureCursor = this.tFeatureClass.Search(pQuery, true);
                IFeature esriFeature = featureCursor.NextFeature();
                ESRI.ArcGIS.Geometry.IPoint pPoint = esriFeature.Shape as ESRI.ArcGIS.Geometry.IPoint;
                IPosition61 _Position6 = Program.pCreator6.CreatePosition(pPoint.X, pPoint.Y, 100, AltitudeTypeCode.ATC_TERRAIN_RELATIVE, 0, -89, 0, 100);
                Program.pNavigate6.FlyTo(_Position6, ActionCode.AC_FLYTO);
                
            }
            catch (Exception ex)
            {
                
                //throw;
            }
           
        }

    }
}