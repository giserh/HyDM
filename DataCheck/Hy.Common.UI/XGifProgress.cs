using System.Threading;
using System.Windows.Forms;

namespace Hy.Common.UI
{
    /// <summary>
    /// ��������
    /// </summary>
    public class XGifProgress
    {
        private frmProgress _progressForm = null;


        /// <summary>
        /// ���췽��
        /// </summary>
        public XGifProgress()
        {
            _progressForm = new frmProgress();
        }

        /// <summary>
        /// ��ʾ��
        /// </summary>
        protected internal frmProgress ProgressForm
        {
            get { return _progressForm; }
            set { _progressForm = value; }
        }

        /// <summary>
        /// ��ʾ������ʾ
        /// </summary>
        /// <param name="toolstip">��ʾ����</param>
        public void ShowHint(string toolstip)
        {
            Application.DoEvents();
            ShowHint(null, toolstip);
        }


        /// <summary>
        /// ��ʾ������ʾ
        /// </summary>
        /// <param name="owner">˭���õ�?</param>
        /// <param name="toolstip">��ʾ����</param>
        public void ShowHint(Control owner, string toolstip)
        {
            //if (ProgressForm.Visible == true)
            //{
            //    return;
            //}

            ProgressForm.Owner = (Form) owner;
            if (owner != null)
            {
                owner.UseWaitCursor = true;
                //this.ProgressForm.Parent = owner;
            }
            m_ToolStip = toolstip;
            ThreadStart start = new ThreadStart(ShowHintInthread);
            new Thread(start).Start();
        }
        private string m_ToolStip;
        private delegate void NoneHandler();
        private delegate void ShowStringHandler(string strContent);
        private void ShowHintInthread()
        {
            if (ProgressForm.InvokeRequired)
            {
                ProgressForm.Invoke(new NoneHandler(ProgressForm.ShowGifProgress));
                object[] objStip = { m_ToolStip };
                ProgressForm.Invoke(new ShowStringHandler(ProgressForm.ShowDoing), objStip);
                ProgressForm.Invoke(new NoneHandler(ProgressForm.ShowProgress));
            }
            else
            {
                ProgressForm.ShowGifProgress();
                ProgressForm.ShowDoing(m_ToolStip);
                ProgressForm.ShowProgress();
            }
        }

        /// <summary>
        /// �رս�����ʾ
        /// </summary>
        public void Hide()
        {
            if (ProgressForm.Visible == false)
            {
                return;
            }

            //ProgressForm.Dispose();

            ThreadStart start = new ThreadStart(ProgressForm.Hide);
            ProgressForm.Invoke(start);
            //ProgressForm.BeginInvoke(start);
           

            //this.ProgressForm = new FrmSimpleProgress();
        }
    }
}