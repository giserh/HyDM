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
    public partial class FrmCustomPath : FrmBase
    {
        private bool lock_onLbuttonDown = false;
        private bool lock_onFrame = false;
        private bool lock_onRbuttonDown = false;
        private bool lock_clickForLbtn = true;
        private bool lock_moveForOnframe = true;

        /// <summary>
        /// ������̬������
        /// </summary>
        private bool lock_btn_create = true;
        /// <summary>
        /// �ж��Ƿ񱣴涯̬����
        /// </summary>
        private bool lock_deleteObj = true;
        /// <summary>
        /// ����·�ߵ��߶���
        /// </summary>
        private ITerrainPolyline5 polyLine;

        private ITerrainDynamicObject5 dynamicObj;

        private IList<double[]> list;

        private int clickIndex = 0;

        private string _pathName;
        private double _pathSpeend;

        public double PathSpeend
        {
            get { return _pathSpeend; }
            set { _pathSpeend = value; }
        }
	

        public string PathName
        {
            get { return _pathName; }
            set { _pathName = value; }
        }
	



        public FrmCustomPath(Form frmMain)
        {
            base.BeginForm(frmMain);
            InitializeComponent();
        }

        /// <summary>
        /// ��½�¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmCustomPath_Load(object sender, EventArgs e)
        {
            base.FrmName = "�Զ���·��";

            Program.TE.OnLButtonDown += new TerraExplorerX._ITerraExplorerEvents5_OnLButtonDownEventHandler(TE_OnLButtonDown);
            Program.TE.OnFrame += new TerraExplorerX._ITerraExplorerEvents5_OnFrameEventHandler(TE_OnFrame);
            Program.TE.OnRButtonDown += new TerraExplorerX._ITerraExplorerEvents5_OnRButtonDownEventHandler(TE_OnRButtonDown);
        }


        /// <summary>
        /// TE�Ҽ��¼�
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pbHandled"></param>
        void TE_OnRButtonDown(int Flags, int X, int Y, ref object pbHandled)
        {
            pbHandled = true;
            if (this.lock_onRbuttonDown)
            {
                lock_onLbuttonDown = false;
                lock_onFrame = false;
                lock_onRbuttonDown = false;
                lock_clickForLbtn = true;
                lock_moveForOnframe = true;
            }

            int groupID = 0;
            try
            {
                groupID = Program.IInfoTree.FindItem("fly");
                if (groupID == 0)
                {
                    Program.IInfoTree.CreateGroup("fly", 0);
                    groupID = Program.IInfoTree.FindItem("fly");
                }
            }
            catch (Exception)
            {
                
            }

            //��������
            this.dynamicObj = Program.TE.IObjectManager51_CreateDynamicObject(DynamicMotionStyle.MOTION_GROUND_VEHICLE, DynamicObjectType.DYNAMIC_VIRTUAL,"",100,HeightStyleCode.HSC_DEFAULT,groupID,this.PathName);

            for (int i = 0; i < this.list.Count; i++)
            {
                this.dynamicObj.AddWaypoint(this.list[i][0], 0, this.list[i][1],this.PathSpeend, i);
            }
            dynamicObj.Acceleration = this.PathSpeend;
            dynamicObj.CircularRoute = 0;
            
            Program.TE.FlyToObject(dynamicObj.ID, ActionCode.AC_WAYPOINT_REACHED);
            simpleButton2.Enabled = true;

        }

        /// <summary>
        /// TEˢ���¼�
        /// </summary>
        void TE_OnFrame()
        {
            if (this.lock_onFrame)
            {
                object longitudeObj, latitudeObj, flagsObj;
                double longitude, latitude, height;
                Program.TE.GetMouseInfo(out flagsObj, out longitudeObj, out latitudeObj);
                Program.TE.ScreenToTerrain(int.Parse(longitudeObj.ToString()), int.Parse(latitudeObj.ToString()), out longitude, out latitude, out height);

                if (this.lock_moveForOnframe)
                {
                    this.polyLine.AddVertex(longitude, height, latitude, 0);
                    this.lock_moveForOnframe = false;

                }
                else
                {
                    this.polyLine.ModifyVertex(this.clickIndex, longitude, height, latitude, 0);
                }
            }
        }

        /// <summary>
        /// TE����¼�
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="pbHandled"></param>
        void TE_OnLButtonDown(int Flags, int X, int Y, ref object pbHandled)
        {
            try
            {
                this.clickIndex += 1;
                if (this.lock_onLbuttonDown)
                {
                    double longitude, latitude, height;


                    if (this.lock_clickForLbtn)
                    {
                        Program.TE.ScreenToTerrain(X, Y, out longitude, out latitude, out height);

                        double[] temp = new double[2];
                        temp[0] = longitude;
                        temp[1] = latitude;
                        this.list.Add(temp);

                        this.polyLine.AddVertex(longitude, 0, latitude, 0);

                        this.lock_onFrame = true;

                        this.lock_clickForLbtn = false;
                    }
                    else
                    {
                        this.lock_moveForOnframe = true;
                        Program.TE.ScreenToTerrain(X, Y, out longitude, out latitude, out height);

                        double[] temp = new double[2];
                        temp[0] = longitude;
                        temp[1] = latitude;
                        this.list.Add(temp);

                        this.polyLine.ModifyVertex(this.clickIndex, longitude, height, latitude, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                
                
            }
           
        }

        /// <summary>
        /// �ر��¼�
        /// ����TE�¼� 
        /// �����Դ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmCustomPath_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.TE.OnLButtonDown -= new TerraExplorerX._ITerraExplorerEvents5_OnLButtonDownEventHandler(TE_OnLButtonDown);
            Program.TE.OnFrame -= new TerraExplorerX._ITerraExplorerEvents5_OnFrameEventHandler(TE_OnFrame);
            Program.TE.OnRButtonDown -= new TerraExplorerX._ITerraExplorerEvents5_OnRButtonDownEventHandler(TE_OnRButtonDown);

            if (this.lock_deleteObj)
            {
                try
                {
                    Program.TE.DeleteItem(this.dynamicObj.InfoTreeItemID);
     
                }
                catch (Exception)
                {
                }
                    
            }
            try
            {
                Program.TE.DeleteItem(this.polyLine.InfoTreeItemID);
            }
            catch (Exception)
            {
 
            }
               
   
            this.Dispose();
            this.Close();
        }

        #region -------------------------��ȡ����-------------------------

        private bool GetProperty()
        {
            if (!this.GetName())
            {
                return false;   
            }
            if (!this.GetSpeend())
            {
                return false;
            }
            return true;
        }

        private bool GetName()
        {
            if (this.txt_name.Text == ""
                || this.txt_name.Text.Equals(null))
            {
                MessageBox.Show("���Ʋ���Ϊ�գ����������ƺ�ʼ����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                this.PathName = this.txt_name.Text.ToString().Trim();
                return true;
            }
        }

        private bool GetSpeend()
        {
            if (this.spinEdit1.Value == null||this.spinEdit1.Value ==0)
            {
                MessageBox.Show("�ٶȲ���Ϊ�գ�����������ٶȺ�ʼ����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            else
            {
                try
                {
                    this.PathSpeend = Convert.ToDouble(this.spinEdit1.Value);

                    if (this.PathSpeend <= 0)
                    {
                        MessageBox.Show("����ٶȸ�ʽ����ȷ�����������0������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    return true;
                }
                catch (Exception)
                {

                    MessageBox.Show("����ٶȸ�ʽ����ȷ�����������0������", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
        }

        #endregion -------------------------��ȡ����-------------------------

       
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            simpleButton2.Enabled = false;
            if (this.lock_btn_create)
            {
                try
                {
                    if (!this.GetProperty())
                    {
                        return;
                    }
                    this.polyLine = Program.TE.IObjectManager51_CreatePolyline(null, 0x0000ff, HeightStyleCode.HS_ON_TERRAIN, 0, "newline");

                    this.list = new List<double[]>();

                    this.lock_onLbuttonDown = true;
                    this.lock_onRbuttonDown = true;

                    this.lock_btn_create = false;
                }
                catch (Exception)
                {
                    
                  
                }
                
            }
            else
            {
                MessageBox.Show("���·���ѿ�ʼ�����������ظ����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.lock_deleteObj = false;
            this.Close();
        }   
    }
}

