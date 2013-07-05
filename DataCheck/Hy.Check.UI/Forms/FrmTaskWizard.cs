using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using Hy.Check.Task;
using Hy.Check.Task.DataImport;
using Common.Utility.Esri;
using Hy.Check.Define;
using CheckTask = Hy.Check.Task.Task;
using Hy.Check.Utility;

namespace Hy.Check.UI.Forms
{
    public partial class FrmTaskWizard : DevExpress.XtraEditors.XtraForm
    {
        public FrmTaskWizard()
        {
            InitializeComponent();
        }


        private enumDataType m_DataType;
        private Dictionary<string ,string> m_DicSchema = null;
        private Dictionary<string, ISpatialReference> m_DicSpatialReference;
        private bool BolFinished = false;

        private int[] m_MapScales =
        {
            500,
            1000,
            2000,
            5000,
            10000,
            25000,
            50000,
            100000,
            250000,
            500000,
            1000000
        };

        private void Init()
        {
            // ��׼�ͷ���
            Dictionary<int, string> dicStandard = SysDbHelper.GetStandardInfo();
            if (dicStandard == null)
            {
                MessageBoxApi.ShowWaringMessageBox("��ǰϵͳ�����Ҳ����ʼ��׼,�޷���������!");
                
                this.Close();
                this.DialogResult = DialogResult.Cancel;
                return;
            }
            Dictionary<int, string>.ValueCollection.Enumerator enName = dicStandard.Values.GetEnumerator();
            while(enName.MoveNext())
            {
                cmbStandard.Properties.Items.Add(enName.Current);
            }
            if (cmbStandard.Properties.Items.Count > 0)
            {
                cmbStandard.SelectedIndex = 0;
            }

            // ����ʱ��
            dateTime.DateTime = DateTime.Now;
        }

        private void txtfolder_Click(object sender, EventArgs e)
        {
            bool cancel = true;
            switch (m_DataType)
            {
                case enumDataType.VCT:
                    dlgSourceFile.Filter = "VCT�ļ�|*.VCT";
                    dlgSourceFile.FileName = txtDatasource.Text;
                    if (dlgSourceFile.ShowDialog() == DialogResult.OK)
                    {
                        txtDatasource.Text = dlgSourceFile.FileName;
                        cancel = false;
                    }
                    break;

                case enumDataType.FileGDB:
                case enumDataType.SHP:
                    dlgSourceFolder.SelectedPath = txtDatasource.Text;
                    if (dlgSourceFolder.ShowDialog() == DialogResult.OK)
                    {
                        txtDatasource.Text = dlgSourceFolder.SelectedPath;
                        cancel = false;
                    }
                    break;

                case enumDataType.PGDB:
                    dlgSourceFile.Filter = "PGDB(MDB)�ļ�|*.MDB";
                    dlgSourceFile.FileName = txtDatasource.Text;
                    if (dlgSourceFile.ShowDialog() == DialogResult.OK)
                    {
                        txtDatasource.Text = dlgSourceFile.FileName;
                        cancel = false;
                    }
                    break;

                default:
                    break;
            }
            if (string.IsNullOrEmpty(txtDatasource.Text))
            {
                dxErrorProvider.SetError(txtDatasource, "����Դ·������Ϊ��");
            }
            else
            {
                dxErrorProvider.SetError(txtDatasource, null);
            }

            if (cancel)
                return;

            if (m_DataType != enumDataType.VCT)
            {
                try
                {
                    // �ռ�ο��б�
                    m_DicSpatialReference = new Dictionary<string, ISpatialReference>();
                    cmbSpatialRefLayer.Properties.Items.Clear();
                    IWorkspaceFactory wsFactory = null;
                    switch (m_DataType)
                    {
                        case enumDataType.FileGDB:
                            wsFactory = new FileGDBWorkspaceFactoryClass();
                            break;
                        case enumDataType.SHP:
                            wsFactory = new ShapefileWorkspaceFactoryClass();
                            break;

                        case enumDataType.PGDB:
                            wsFactory = new AccessWorkspaceFactoryClass();
                            break;
                        default:
                            break;
                    }
                    IWorkspace wsSource = wsFactory.OpenFromFile(txtDatasource.Text, 0);
                    IEnumDataset enDataset = wsSource.get_Datasets(esriDatasetType.esriDTAny);
                    IDataset ds = enDataset.Next();

                    while (ds != null)
                    {
                        IGeoDataset geoDs = ds as IGeoDataset;
                        if (geoDs != null)
                        {
                            cmbSpatialRefLayer.Properties.Items.Add(ds.Name);
                            m_DicSpatialReference.Add(ds.Name, geoDs.SpatialReference);
                        }
                        ds = enDataset.Next();
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wsSource);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wsFactory);
                    wsSource = null;
                    wsFactory = null;
                    GC.Collect();

                }
                catch
                {
                    MessageBoxApi.ShowErrorMessageBox("ѡ�������Դ����ȷ��������Դ��û�пռ����ݣ�");
                    txtDatasource.Text = "";
                }

                if (cmbSpatialRefLayer.Properties.Items.Count == 0)
                {
                    MessageBoxApi.ShowErrorMessageBox("ѡ�������Դ����ȷ��������Դ��û�пռ����ݣ�");
                    //dxErrorProvider.SetError(txtDatasource, "��ǰ����Դ��û�пռ�����", DevExpress.XtraEditors.DXErrorProvider.ErrorType.Warning);
                    txtDatasource.Text = "";
                }
                else
                {
                    dxErrorProvider.SetError(txtDatasource, null);
                    cmbSpatialRefLayer.SelectedIndex = 0;
                }
            }
        }

        private void rgpDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtDatasource.Text = "";
            dxErrorProvider.SetError(txtDatasource, null);
            m_DataType = (enumDataType)rgpDataType.SelectedIndex;
            if (m_DataType == enumDataType.VCT)
            {
                cmbSpatialRefLayer.Enabled = false;
                checkBoxUseSourceDirectly.Enabled = false;
            }
            else
            {
                cmbSpatialRefLayer.Enabled = true;
                checkBoxUseSourceDirectly.Enabled = true;
            }
        }

        private void txtTaskPath_Click(object sender, EventArgs e)
        {
            dlgSourceFolder.SelectedPath = txtTaskPath.Text;
            if (dlgSourceFolder.ShowDialog() == DialogResult.OK)
            {
                txtTaskPath.Text = dlgSourceFolder.SelectedPath;
            }


            if (string.IsNullOrEmpty(txtTaskPath.Text))
            {
                dxErrorProvider.SetError(txtTaskPath, "������·������Ϊ�գ�");
            }
            else
            {
                dxErrorProvider.SetError(txtTaskPath, null);
            }
        }

        private void cmbStandard_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbSchema.Properties.Items.Clear();

            if (cmbStandard.SelectedIndex < 0)
                return;

            m_DicSchema = SysDbHelper.GetSchemasInfo(cmbStandard.SelectedItem as string);
            if (m_DicSchema == null)
            {
                dxErrorProvider.SetError(cmbStandard, "��ǰ��׼��û�����ü�鷽��");
                return;
            }
            Dictionary<string, string>.Enumerator enSchema = m_DicSchema.GetEnumerator();
            while (enSchema.MoveNext())
            {
                cmbSchema.Properties.Items.Add(enSchema.Current);
            }
            if (cmbSchema.Properties.Items.Count > 0)
            {
                cmbSchema.SelectedIndex = 0;
            }
        }

        private void txtTaskPath_EditValueChanged(object sender, EventArgs e)
        {
            dxErrorProvider.SetError(txtTaskPath, null);
            if (string.IsNullOrEmpty(txtTaskPath.Text))
            {
                dxErrorProvider.SetError(txtTaskPath, "������·������Ϊ�գ�");
                return ;
            }
            else
            {
                dxErrorProvider.SetError(txtTaskName, null);
            }

            // ��֤������+·��
            CancelEventArgs eTaskName = new CancelEventArgs();
            txtTaskName_Validating(sender, eTaskName);
        }

        private void txtTaskName_Validating(object sender, CancelEventArgs e)
        {
            if (txtTaskName.Text != null)
                txtTaskName.Text = txtTaskName.Text.Trim();

            string strTaskName = txtTaskName.Text;
            if (!TaskHelper.TestTaskName(strTaskName))
            {
                lblNameError.Text = "�������Ʋ��Ϸ�";
                //e.Cancel = true;
                return;
            }

            string strError = "";
            if (TaskHelper.TaskExists(strTaskName, txtTaskPath.Text, ref strError))
            {
                lblNameError.Text = strError;
                //e.Cancel = true;
                return;
            }
        }

        private void txtTaskName_EditValueChanged(object sender, EventArgs e)
        {
            string strTaskName = txtTaskName.Text;
            if (string.IsNullOrEmpty(strTaskName))
            {
                lblNameError.Text = "����������Ϊ��";
                return;
            }
            dxErrorProvider.SetError(txtTaskName, null);
            lblNameError.Text = "";
        }

        private void txtTopoTolerance_Validating(object sender, CancelEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtTopoTolerance.Text, "^[1-9]d*.d*|0.d*[1-9]d*$"))
            {
                dxErrorProvider.SetError(txtTopoTolerance, "��������ֵΪ������");
                //e.Cancel = true;
            }
            else
            {
                dxErrorProvider.SetError(txtTopoTolerance, null);
            }
        }

        /// <summary>
        /// ��֤����Դҳ��
        /// @remark �ռ�ο��б��������������
        /// </summary>
        /// <returns></returns>
        private bool ValidateDatasouce()
        {
            if (string.IsNullOrEmpty(txtDatasource.Text))
            {
                dxErrorProvider.SetError(txtDatasource, "����Դ·������Ϊ�գ�");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ����·����������֤
        /// </summary>
        /// <returns></returns>
        private bool ValidatePathAndName()
        {
            bool flag = true;
            if (string.IsNullOrEmpty(txtTaskPath.Text))
            {
                dxErrorProvider.SetError(txtTaskPath, "������·������Ϊ�գ�");
                flag = false;
            }

            lblNameError.Text = "";
            if (string.IsNullOrEmpty(txtTaskName.Text))
            {
                dxErrorProvider.SetError(txtTaskName, "�������Ʋ���Ϊ��");
                lblNameError.Text = "�������Ʋ���Ϊ��";
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// ��Ҫ������֤
        /// </summary>
        /// <returns></returns>
        private bool ValidatePrimaryArgs()
        {
            bool flag = true;
            if (cmbStandard.SelectedIndex < 0)
            {
                dxErrorProvider.SetError(cmbStandard, "����ѡ�����׼");
                flag = false;
            }
            else if (cmbSchema.Properties.Items.Count == 0)
            {
                dxErrorProvider.SetError(cmbStandard, "��ǰ�����׼��û�����ü�鷽��");
                flag = false;
            }

            if (cmbSchema.SelectedIndex < 0)
            {
                dxErrorProvider.SetError(cmbSchema, "����ѡ���鷽��");
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// ���Ӳ�����֤
        /// @remark ��ʱû����֤����
        /// </summary>
        /// <returns></returns>
        private bool ValidateAppendArgs()
        {
            return true;
        }

        private void ShowInformation()
        {
            StringBuilder strInfo = new StringBuilder("��ǰ������Ϣ��\r\n");
            strInfo.Append("\r\n����Դ��\t\t"); strInfo.Append(txtDatasource.Text);
            strInfo.Append("\r\n�������ͣ�\t"); strInfo.Append(m_DataType.ToString());
            strInfo.Append("\r\nֱ�Ӽ������Դ��\t"); strInfo.Append(checkBoxUseSourceDirectly.Checked ? "��" : "��");
            strInfo.Append("\r\n��׼��\t\t"); strInfo.Append(cmbStandard.Text);
            strInfo.Append("\r\n������\t\t"); strInfo.Append(cmbSchema.Text);
            strInfo.Append("\r\nTopo���ޣ�\t"); strInfo.Append(txtTopoTolerance.Text);
            strInfo.Append("\r\n�����ߣ�\t\t"); strInfo.Append(cmbScale.Text);
            strInfo.Append("\r\n�ռ�ο�ͼ�㣺\t"); strInfo.Append(cmbSpatialRefLayer.Text);
            strInfo.Append("\r\n��λ��\t\t"); strInfo.Append(txtInstitute.Text);
            strInfo.Append("\r\n�����ˣ�\t\t"); strInfo.Append(txtPerson.Text);
            strInfo.Append("\r\n����ʱ�䣺\t"); strInfo.Append(dateTime.Text);
            strInfo.Append("\r\n��ע��\t\t"); strInfo.Append(txtRemark.Text);

            txtInformation.Text = strInfo.ToString();

        }

        private void wizardControl_CancelClick(object sender, CancelEventArgs e)
        {
            if (MessageBoxApi.ShowQuestionMessageBox("�Ƿ�رմ���������?") ==DialogResult.Yes)
            {
                BolFinished = true;
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void wizardControl_NextClick(object sender, DevExpress.XtraWizard.WizardCommandButtonClickEventArgs e)
        {
            switch (wizardControl.SelectedPageIndex)
            {
                case 0:
                    e.Handled = !ValidateDatasouce();
                    break;

                case 1:
                    e.Handled = !ValidatePathAndName();
                    break;

                case 2:
                    e.Handled = !ValidatePrimaryArgs();
                    break;

                case 3:
                    e.Handled = !ValidateAppendArgs();
                    if (!e.Handled)
                        ShowInformation();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// ����Task���󣬽��д���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wizardControl_FinishClick(object sender, CancelEventArgs e)
        { 
            this.m_Task = GenerateTask();
            IDataImport dataImporter = null;
            switch (this.m_DataType)
            {
                case enumDataType.FileGDB:
                    dataImporter = new FileGDBDataImport();
                    break;

                case enumDataType.PGDB:
                    dataImporter = new MDBDataImport();
                    break;

                case enumDataType.SHP:
                    dataImporter = new SHPDataImport();
                    break;

                case enumDataType.VCT:
                    VCTDataImport vctDataImport=new VCTDataImport();
                   // vctDataImport.ConvertStepping += new VCTConvertStepHandler(vctDataImport_ConvertStepping);
                    dataImporter = vctDataImport;
                    break;
            }       
            this.m_Task.DataImporter = dataImporter;
            this.m_Task.Messager = MessageHandler;
//#if !DEBUG 

            this.Hide();
//#endif

            #region ��̨�̵߳��뷽ʽ

            //Common.UI.frmProgress frmProgress = new Common.UI.frmProgress();
            //dataImporter.ImportingObjectChanged += new ImportingObjectChangedHandler(frmProgress.ShowDoing);
            //System.Threading.Thread newThread = new System.Threading.Thread(new System.Threading.ThreadStart(RunTaskCreate));
            //newThread.Start();

            //frmProgress.ShowGifProgress();
            //frmProgress.ShowDialog();

            #endregion


            // �߳���ʾ���ȷ�ʽ

            dataImporter.ImportingObjectChanged += new ImportingObjectChangedHandler(dataImporter_ImportingObjectChanged);

            try
            {
                bool isSucceed = m_Task.Create();


                isSucceed = isSucceed && m_Task.CreateMXD();


                if (isSucceed)
                {
                    //MessageBoxApi.ShowFinishedMessageBox("���񴴽��ɹ�!");
                    BolFinished = true;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBoxApi.ShowFinishedMessageBox("���񴴽�ʧ��!");
                    this.DialogResult = DialogResult.Cancel;
                }
            }
            catch (Exception exp)
            {
                MessageHandler(enumMessageType.Exception, exp.ToString());
            }
            finally
            {
                m_GifProgress.Hide();
                //m_Progress.Hide();
            }


        }

        //Common.UI.XProgress m_Progress = new Common.UI.XProgress();
        //private int m_ImportedCount = 0;
        //void vctDataImport_ConvertStepping(int totalCount)
        //{
        //    m_ImportedCount++;
        //    m_Progress.ShowProgress(0, totalCount, m_ImportedCount, this);
        //}


        Common.UI.XGifProgress m_GifProgress=new Common.UI.XGifProgress();

        /// <summary>
        /// ��ʾ������
        /// </summary>
        /// <param name="strOjbectName"></param>
        void dataImporter_ImportingObjectChanged(string strOjbectName)
        {
            m_GifProgress.ShowHint(strOjbectName);
            //if (m_FrmProgress == null)
            //{
            //    m_FrmProgress = new Common.UI.frmProgress();
            //    m_FrmProgress.UseWaitCursor = true;

            //    System.Threading.ThreadStart start = new System.Threading.ThreadStart(m_FrmProgress.ShowGifProgress);
            //    new System.Threading.Thread(start).Start();

            //}
            //if (m_FrmProgress.Visible != true)
            //    m_FrmProgress.Visible = true;

            //m_FrmProgress.ShowDoing(strOjbectName);
            //m_FrmProgress.ShowGifProgress();

        }
        /// <summary>
        /// �쳣�Ͳ�����Ϣ����
        /// </summary>
        void MessageHandler(enumMessageType msgType, string strMsg)
        {
            Common.Utility.Log.OperationalLogManager.AppendMessage(strMsg);
        }

        private CheckTask GenerateTask()
        {
            CheckTask task = new CheckTask();
            task.DatasourceType = m_DataType;
            task.SourcePath = this.txtDatasource.Text;
            task.UseSourceDirectly = checkBoxUseSourceDirectly.Checked;
            task.Name = this.txtTaskName.Text;
            task.Path = this.txtTaskPath.Text;
            task.StandardName = cmbStandard.SelectedItem as string;
            task.SchemaID = (( KeyValuePair<string, string>)cmbSchema.SelectedItem ).Key;
            task.TopoTolerance = double.Parse(txtTopoTolerance.Text);
            task.MapScale = m_MapScales[cmbScale.SelectedIndex];
            if (m_DicSpatialReference != null)
            {
                task.SpatialReference = m_DicSpatialReference[cmbSpatialRefLayer.SelectedItem as string];
            }

            task.Institution = txtInstitute.Text;
            task.Creator = txtPerson.Text;
            task.CreateTime = dateTime.DateTime.ToString();
            task.Remark = txtRemark.Text;

            return task;
        }

        private CheckTask m_Task;
        //private void RunTaskCreate()
        //{
        //    string strMsg=null;
        //    bool isSucceed = m_Task.Create(ref strMsg);
        //    isSucceed = isSucceed && m_Task.CreateMXD();
        //    if (isSucceed)
        //    {
        //        XtraMessageBox.Show("���񴴽��ɹ�");
        //    }
        //    else
        //    {
        //        XtraMessageBox.Show("���񴴽�ʧ��");
        //    }
        //}

        public CheckTask GetCurrentTask()
        {
            if (m_Task == null)
            {
                throw new Exception("�������ô���Task����������򵼺�ű�����");
            }
            return m_Task;
        }

        private void FrmTaskWizard_Load(object sender, EventArgs e)
        {
            Init(); 
        }

        private void FrmTaskWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BolFinished)
            {
                return;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}