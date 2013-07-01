using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Check.Task;
using Check.Engine;
using Check.Define;
using Check.Engine;
using CheckTask = Check.Task.Task;
using Check.Utility;

namespace Check.UI.Forms
{
    /// <summary>
    /// ��ϣ�������¼��������У�CurrentTask����Ϊ���루���캯����task����������Ϊnull
    /// </summary>
    public partial class FrmTaskCheck : XtraForm
    {
        protected FrmTaskCheck()
        {
            InitializeComponent();
        }

        public FrmTaskCheck(CheckTask task, System.Collections.Generic.List<SchemaRuleEx> ruleInfoList)
        {
            InitializeComponent();

            this.CurrentTask = task;
            this.RuleInfoList = ruleInfoList;
        }

        public CheckTask CurrentTask
        {
            set
            {
                m_Task = value;
                if (m_Task == null)
                    return;

                this.m_TaskLog = m_Task.GetTaskFolder() + "\\������־_" + m_Task.Name + "_" + m_Task.ID + ".txt";
            }
        }

        public System.Collections.Generic.List<SchemaRuleEx> RuleInfoList
        {
            set
            {
                m_RuleInfoList = value;
            }
        }

        protected CheckTask m_Task;
        protected System.Collections.Generic.List<SchemaRuleEx> m_RuleInfoList;
        //Checker taskChecker = null;
        protected Thread m_Thread = null;
        protected StringWriter m_StringWriter = null;// new StringWriter();
        protected bool m_ExcuteComplete = false;
        protected string m_TaskLog = null;
        protected int m_RuleCount = 0;
        protected int m_ExcutedRuleCount = 0;
        protected int m_ErrorCount = 0;

        public void CheckTask()
        {

            ReadyForCheck();

            ThreadStart threadStart = delegate
            {
                if (m_Task == null)
                    return;

                Checker taskChecker = new Checker();

                taskChecker.BaseWorkspace = m_Task.BaseWorkspace;
                taskChecker.QueryConnection = m_Task.QueryConnection;
                taskChecker.QueryWorkspace = m_Task.QueryWorkspace;
                taskChecker.ResultPath = m_Task.GetResultDBPath();
                //TemplateRules templateRules = new TemplateRules(m_Task.SchemaID);
                taskChecker.RuleInfos = this.m_RuleInfoList; //templateRules.CurrentSchemaRules; //Check.Rule.Helper.RuleInfoHelper.GetRuleInfos(m_Task.SchemaID);
                taskChecker.SchemaID = m_Task.SchemaID;
                taskChecker.TopoDBPath = m_Task.GetResultDBPath();
                taskChecker.TopoTolerance = m_Task.TopoTolerance;

                AdaptCheckerEvents(taskChecker);

                taskChecker.Check();
            };

            m_Thread = new Thread(threadStart);
            m_Thread.Start();

            this.ShowDialog();
        }

        public void AdaptCheckerEvents(Checker taskChecker)
        {
            taskChecker.CheckingRuleChanged += new DealingRuleChangedHandler(CheckingRuleChanged);
            taskChecker.VerifyingRuleChanged += new DealingRuleChangedHandler(VerifyingRuleChanged);
            taskChecker.PretreatingRuleChanged += new DealingRuleChangedHandler(PretreatingRuleChanged);
            taskChecker.VerifyedComplete += new VerifyedCompleteHandler(VerifyedComplete);
            taskChecker.PretreatComplete += new CheckEventHandler(PretreatComplete);
            taskChecker.TopoRuleCheckBegin += new CheckEventHandler(TopoRuleCheckBegin);
            taskChecker.RuleChecked += new RuleCheckedHandler(RuleChecked);
            taskChecker.CheckComplete += new CheckEventHandler(CheckComplete);
            
            taskChecker.Messager = LogDeal;
        }

        public virtual void ReadyForCheck()
        {
            COMMONCONST.TOPOTOLORANCE = m_Task.TopoTolerance;
            COMMONCONST.dAreaThread = m_Task.MapScale * 0.04;
            COMMONCONST.dLengthThread = m_Task.MapScale * 0.2 / 10000;

            m_ExcuteComplete = false;
            this.m_StringWriter = new StringWriter();
            this.m_ErrorCount = 0;
            this.m_RuleCount = 0;
            this.m_Time = 0;

            m_StringWriter.WriteLine(DateTime.Now.ToString() + "������" + m_Task.Name + "����鿪ʼ\r\n\r\n");

            this.btnViewLog.Visible = false;
            this.btnClose.Text = "ȡ��";
            this.Text = "����ִ���Զ����...";
            this.lblErrorCount.Text = "0";
            this.lblExcutedRuleCount.Text = "0";
            this.lblRuleCount.Text = "���ڼ��㡭";
            this.lblTime.Text = "0��";
            this.lblOperate.Text = "����������顭";
            this.progressBarControl1.Position = 0;
            timer1.Start();
        }


        private void LogDeal(enumMessageType msgType, string strMsg)
        {
            switch (msgType)
            {
                case enumMessageType.VerifyError:
                case enumMessageType.PretreatmentError:
                case enumMessageType.RuleError:
                    m_StringWriter.WriteLine(DateTime.Now.ToString() + "��" + strMsg);
                    break;

                case enumMessageType.Exception:
                    Common.Utility.Log.OperationalLogManager.AppendMessage(strMsg);
                    break;

                default:
                    break;
            }
        }

        protected virtual void CheckComplete(Checker sender)
        {
            //Task״̬����[�����ݿ�]
            m_Task.UpdateStateForExcuted();
            m_Task.CreateTaskConfig();
            TaskHelper.UpdateTaskState(m_Task.ID, m_Task.State);

            // ֹͣ��ʱ��д�����־
            m_StringWriter.WriteLine(DateTime.Now.ToString() + "��������");
            m_StringWriter.WriteLine(string.Format("��������:{0}���ɹ���:{1}",this.m_RuleCount, sender.SucceedCount));
            m_StringWriter.WriteLine("�ܴ�����:" + m_ErrorCount.ToString());
            StreamWriter streamWriter = new StreamWriter(m_TaskLog);
            streamWriter.Write(m_StringWriter.ToString());
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();

            timer1.Stop();
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "������" });
                ThreadStart uiChanged = delegate
                {
                    this.Text = "������";
                    this.btnViewLog.Visible = true;
                    this.btnClose.Text = "��˼����";
                };
                this.Invoke(uiChanged);
            }
            else
            {
                SetOperateText("������");
                this.Text = "������";
                this.btnViewLog.Visible = true;
                this.btnClose.Text = "��˼����";
                Application.DoEvents();
            }
            this.m_ExcuteComplete = true;
        }
        protected virtual void RuleChecked(Checker sender, ICheckRule checkedRule, int errorCount)
        {
            m_ErrorCount += errorCount;
            this.m_ExcutedRuleCount++;
            if (this.InvokeRequired)
            {
                ThreadStart errCountChanged = delegate
                {
                    lblErrorCount.Text = m_ErrorCount.ToString();
                    this.lblExcutedRuleCount.Text = this.m_ExcutedRuleCount.ToString();

                    progressBarControl1.Position = m_ExcutedRuleCount;
                    progressBarControl1.Update();
                };
                this.Invoke(errCountChanged);
            }
            else
            {
                    lblErrorCount.Text = m_ErrorCount.ToString();
                    this.lblExcutedRuleCount.Text = this.m_ExcutedRuleCount.ToString();

                    progressBarControl1.Position = m_ExcutedRuleCount;
                    progressBarControl1.Update();
                    Application.DoEvents();
            }
        }
        protected virtual void TopoRuleCheckBegin(Checker sender)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "���ڽ������˼�顭" });
            }
            else
            {
                SetOperateText("���ڽ������˼�顭");
                Application.DoEvents();
            } 
        }
        protected virtual void PretreatComplete(Checker sender)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "Ԥ�������" });
            }
            else
            {
                SetOperateText("Ԥ�������");
                Application.DoEvents();
            }
        }
        protected virtual void VerifyedComplete(Checker sender, int availCount)
        {
            this.m_RuleCount = availCount;
            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetAvaliateText), new object[] { availCount.ToString() });
            }
            else
            {
                SetAvaliateText(availCount.ToString());
                Application.DoEvents();
            }
        }
        protected virtual void CheckingRuleChanged(Checker sender, ICheckRule dealingRule)
        {
            if (dealingRule == null)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "����ִ�С�" + dealingRule.InstanceName + "����顭" });
            }
            else
            {
                SetOperateText("����ִ�С�" + dealingRule.InstanceName + "����顭");
                Application.DoEvents();
            }
        }
        protected virtual void PretreatingRuleChanged(Checker sender, ICheckRule dealingRule)
        {
            if (dealingRule == null)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "����ִ��Ԥ����" });
            }
            else
            {
                SetOperateText("����ִ��Ԥ����");
                Application.DoEvents();
            }
        }
        protected virtual void VerifyingRuleChanged(Checker sender, ICheckRule dealingRule)
        {
            if (dealingRule == null)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new SetTextHandler(SetOperateText), new object[] { "������֤��" + dealingRule.InstanceName + "������" });
            }
            else
            {
                SetOperateText("������֤��" + dealingRule.InstanceName + "������");
                Application.DoEvents();
            }
        }

        protected delegate void SetTextHandler(string strText);
        protected void SetOperateText(string strText)
        {
            lblOperate.Text = strText;
        }
        private void SetAvaliateText(string strText)
        {
            lblRuleCount.Text = strText;
            progressBarControl1.Properties.Minimum = 0;
            progressBarControl1.Properties.Maximum = m_RuleCount;
        }



        private void progressBarControl1_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            double v = Convert.ToDouble(e.Value);
            double d = Math.Floor(v);
            e.DisplayText = d.ToString("n0");
        }
        protected virtual void btnViewLog_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.Environment.SystemDirectory + "\\notepad.exe", m_TaskLog);

            //Form frmLog = new Form();
            //RichTextBox txtLog = new RichTextBox();
            //txtLog.Dock = DockStyle.Fill;
            //txtLog.Text = m_StringWriter.ToString();
            //txtLog.ReadOnly = true;
            //frmLog.Controls.Add(txtLog);

            //frmLog.Show();
        }
        protected virtual void btnClose_Click(object sender, EventArgs e)
        {
            if (!m_ExcuteComplete)
            {
                if (XtraMessageBox.Show("��ȷ��Ҫȡ�������", "ȡ��ȷ��", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                if (m_Thread != null)
                {
                    try
                    {
                        m_Thread.Abort();
                    }
                    catch
                    {
                    }
                }

                //if (taskChecker != null)
                //    taskChecker.Release();

                this.DialogResult = DialogResult.Abort;
            }
            this.Close();
        }
        private int m_Time = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.m_Time++;
            string strTime = null;
            if (m_Time > 3600)
            {
                strTime = string.Format("{0}Сʱ{1}��{2}��", m_Time / 3600, (m_Time % 3600) / 60, m_Time % 60);
            }
            else if (m_Time > 60)
            {
                strTime = string.Format("{0}��{1}��", m_Time / 60, m_Time % 60);                
            }
            else
            {
                strTime = string.Format("{0}��", m_Time);
            }
            lblTime.Text = strTime;
            Application.DoEvents();

        }
        private void FrmTaskCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_Thread == null)
                return;

            if (!m_ExcuteComplete)
            {
                if (XtraMessageBox.Show("��ȷ��Ҫȡ�������", "ȡ��ȷ��", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    m_Thread.Abort();
                }
                catch
                {
                }

                GC.Collect();
                if (this.m_Task != null)
                    this.m_Task.Release();

                this.DialogResult = DialogResult.Abort;
            }

            this.m_ErrorCount = 0;
            this.m_RuleCount = 0;
            if (this.m_StringWriter != null)
            {
                this.m_StringWriter.Close();
                this.m_StringWriter.Dispose();
            }
            this.m_Time = 0;
        }
    }
}