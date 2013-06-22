
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Skyline.Core.UI
{
    public partial class FrmBase : DevExpress.XtraEditors.XtraForm
    {
        private string _frmName;

        /// <summary>
        /// ���õ�ǰ�ļ��Ĺ���˵�� ��"��齨ģ"
        /// </summary>
        public string FrmName
        {
            get { return _frmName; }
            set { _frmName = value; }
        }

        public FrmBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ÿ�δ��幹��ǰִ�д˷��� 
        /// ����Ϊ��֤�������Ψһ��
        /// </summary>
        /// <param name="frmMain">�˴���� ����OwnedForm�������</param>
        public bool BeginForm(Form frmMain)
        {
            if (frmMain.OwnedForms.Length >0)
            {
                FrmBase temp = null;
                try
                {
                    temp = (FrmBase)frmMain.OwnedForms[0];
                
                }
                
                catch (Exception)
                {
                    return false;
                }
    
                if (MessageBox.Show("��ǰ���ڲ���" + temp.FrmName + "���Ƿ�رգ�", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //temp.Dispose();
                    temp.Close();
                    return true;
                }
                else
                {
                    // MessageBox.Show("���е���FormBae�е�BeginForm�������������������Ҫ�˳�");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// �ر���Ϣ����
        /// </summary>
        public void CloseForm()
        { 
            
        }
    }
}