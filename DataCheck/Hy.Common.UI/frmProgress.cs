using System;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace Hy.Common.UI
{
    public partial class frmProgress : XtraForm
    {
        public frmProgress()
        {
            InitializeComponent();
        }

        //public delegate void RefreshGif();

        private void frmProgress_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// ��ʼ��������
        /// </summary>
        /// <param name="lMin">��Сֵ</param>
        /// <param name="lMax">���ֵ</param>
        /// <param name="lStep">����</param>
        public void ShowProgress(int lMin, int lMax, int lStep)
        {
            progressBarControl1.Visible = true;
            marqueeProgressBarControl1.Visible = false;
            progressBarControl1.Properties.PercentView = true;
            
            
            progressBarControl1.Properties.Minimum = lMin;
            progressBarControl1.Properties.Maximum = lMax;
            progressBarControl1.Properties.Step = lStep;
            progressBarControl1.Position = lMin;
            //progressBarControl1.Update();
            Show();
        }

        /// <summary>
        /// ��ʾ���ȶ���
        /// </summary>
        public void ShowGifProgress()
        {
            progressBarControl1.Visible = false;
            marqueeProgressBarControl1.Visible = true;
            marqueeProgressBarControl1.Properties.ProgressKind = ProgressKind.Horizontal;
            marqueeProgressBarControl1.Update();
        }

        //private delegate void _RefreshGif();
        //public void RefreshGif()
        //{
        //    _RefreshGif rf = delegate
        //        {
        //            marqueeProgressBarControl1.Update();
        //        };
        //    BeginInvoke(rf);
        //}
        
        /// <summary>
        /// ��ʾ������
        /// </summary>
        /// <param name="sWhat">��ʾ����</param>
        public void ShowDoing(string sWhat)
        {
            labelControl1.Text = sWhat;
            labelControl1.Update();
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void Step()
        {
            if (progressBarControl1.Visible)
            {
                progressBarControl1.PerformStep();
                progressBarControl1.Update();
                Application.DoEvents();
            }
        }

        /// <summary>
        /// �ֶ����ý�����ֵ
        /// </summary>
        /// <param name="intValue">������ֵ</param>
        public void SetValue(int intValue)
        {
            if (progressBarControl1.Visible)
            {
                progressBarControl1.Position = intValue;
                progressBarControl1.Update();
            }
        }

        /// <summary>
        /// ��ʾ������ʾ
        /// </summary>
        protected internal void ShowProgress()
        {
            try
            {
                if (Visible == false)
                {
                    ShowDialog();
                }                
            }
            catch(Exception ex)
            {}
        }

        private void progressBarControl1_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            double v = Convert.ToDouble(e.Value);
            e.DisplayText = v.ToString("n0");
        }
    }
}