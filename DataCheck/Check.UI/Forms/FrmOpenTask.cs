using System;
using System.Data;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using Check.Task;
using Common.UI;
using Check.Utility;
using CheckTask = Check.Task.Task;

namespace Check.UI.Forms
{
    public partial class FrmOPenTask : XtraForm
    {
        private CheckTask m_SytemTask;
        /// <summary>
        /// ϵͳ����
        /// </summary>
        public CheckTask SystemTask
        {
            set
            {
                m_SytemTask = value;
            }
        }

        public FrmOPenTask()
        {
            InitializeComponent();

            Init();
        }

        public CheckTask SelectedTask
        {
            get
            {
                return m_SelectedTask;
            }
            set
            {
            }
        }

        private List<CheckTask> m_AllTasks;
        /// <summary>
        /// ���������б�
        /// ʹ�ýṹ���ķ���--��CheckTask��TaskHelper�ṩ�ķ����������ڴ�����ֱ�Ӹ����ݿ⼰�ֶδ򽻵�
        /// </summary>
        private void Init()
        {
            // ���������б� 
            m_AllTasks = TaskHelper.GetAllTasks();
            if(m_AllTasks==null)
                return;

            DataTable tTask = ConstructTaskTable();
            int count = m_AllTasks.Count;
            for (int i = 0; i < count; i++)
            {
                CheckTask task=m_AllTasks[i];
                DataRow rowTask = tTask.NewRow();
                rowTask[0] = task.Name;
                rowTask[1] = task.Path;
                rowTask[2] = task.SourcePath;
                rowTask[3] = task.DatasourceType.ToString();

                tTask.Rows.Add(rowTask);
            }

            this.gridControlTasks.DataSource = tTask;
            this.gridControlTasks.RefreshDataSource();
        }
        private DataTable ConstructTaskTable()
        {
            DataTable tStruct = new DataTable();
            tStruct.Columns.Add("��������");
            tStruct.Columns.Add("����·��");
            tStruct.Columns.Add("����Դ·��");
            tStruct.Columns.Add("����Դ��������");

            return tStruct;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (m_SytemTask!=null && m_SelectedTask.ID == m_SytemTask.ID)
            {
                XtraMessageBox.Show("��ѡ������Ϊ��ǰ�򿪵����񣬲���ɾ��");
                return;
            }

            if (XtraMessageBox.Show("ȷʵҪɾ����������", "ɾ��ȷ��", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                try
                {
                    string strFolder = m_SelectedTask.Path + "\\" + m_SelectedTask.Name;
                    if (Directory.Exists(strFolder))
                    {
                        m_SelectedTask.Release();
                        Directory.Delete(strFolder, true);
                    }
                    if (!TaskHelper.DeleteTask(m_SelectedTask.ID))
                    {
                        XtraMessageBox.Show("ɾ��ʱ��������");
                        return;
                    }
                    m_AllTasks.Remove(m_SelectedTask);
                    gridViewTasks.DeleteRow(gridViewTasks.FocusedRowHandle);
                    this.gridViewTasks_FocusedRowChanged(gridViewTasks, null);
                }
                catch
                {
                    XtraMessageBox.Show("ɾ��ʱ��������");
                }
            }
        }

        public void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d);    //ֱ��ɾ�����е��ļ�  
                    else
                        DeleteFolder(d);       //�ݹ�ɾ�����ļ���  
                }
                Directory.Delete(dir);    //ɾ���ѿ��ļ���  
            }
        }
    
        private void btnOpenTask_Click(object sender, EventArgs e)
        {
            if (OpenTask())
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool OpenTask()
        {
            try
            {
                if (gridViewTasks.RowCount < 1)
                {
                    XtraMessageBox.Show("��ǰ�����б�Ϊ�գ�", "��ʾ");
                    return false;
                }

                int nRowIndex = gridViewTasks.FocusedRowHandle;
                if (nRowIndex < 0)
                {
                    XtraMessageBox.Show("δѡ��������ѡ����Ҫ�򿪵�����", "��ʾ");
                    return false;
                }


                if (m_SytemTask != null)
                {
                    if (m_SytemTask.Name == m_SelectedTask.Name && m_SytemTask.Path == m_SelectedTask.Path)
                    {
                        XtraMessageBox.Show("�����Ѵ�!", "��ʾ");
                        return false;
                    }
                }

                if (!Directory.Exists(m_SelectedTask.Path+"\\"+m_SelectedTask.Name))
                {
                    XtraMessageBox.Show("����Ŀ¼������!�޷���");
                    return false;
                }
              
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnOpenFromFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.Description = "��ѡ���ʼ�����";

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string strFolderPath = folderDialog.SelectedPath;
                    string strConfigFile = strFolderPath + "\\" + COMMONCONST.File_Name_SystemConfig;
                    CheckTask task = TaskHelper.FromTaskConfig(strConfigFile);
                    // �޸�task��pathΪ�ϼ�Ŀ¼������Ϊ��ǰ�ļ�������
                    DirectoryInfo dirInfo = new DirectoryInfo(strFolderPath);
                    task.Name = dirInfo.Name;
                    task.Path = dirInfo.Parent.FullName;

                    m_SelectedTask = task;
                    if (m_SytemTask != null && m_SytemTask.Name == m_SelectedTask.Name && m_SytemTask.Path==m_SelectedTask.Path)
                    {
                        XtraMessageBox.Show("�����Ѵ�");
                        return;
                    }
                    this.DialogResult = DialogResult.OK;
                }
                catch
                {
                    XtraMessageBox.Show("���ļ�������ʧ�ܣ�", "��ʾ");
                }
            }
        }

        private CheckTask m_SelectedTask = null;

        private void gridViewTasks_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int[] handleSelected = gridViewTasks.GetSelectedRows();
            if (handleSelected == null || handleSelected.Length == 0)
            {
                btnOpenTask.Enabled = false;
                btnDeleteTask.Enabled = false;
            }
            else
            {
                btnOpenTask.Enabled = true;
                btnDeleteTask.Enabled = true;
            }

           int taskIndex= gridViewTasks.GetDataSourceRowIndex(handleSelected[0]);
           m_SelectedTask = m_AllTasks[taskIndex];
        }

        private void gridControlTasks_DoubleClick(object sender, EventArgs e)
        {
            if (OpenTask())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}