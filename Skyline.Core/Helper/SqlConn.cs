using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Windows.Forms;

namespace Skyline.Core.Helper
{
    public class SqlConn : IDisposable
    {
        static string url = System.Windows.Forms.Application.StartupPath + @"\data\guangdong.mdb";

        private static string CON_STRING = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + url;
        //System.Environment.CurrentDirectory + @"\data\guangdong.mdb";


        private static OleDbConnection dbConn;


        public static OleDbConnection getOleConn()
        {
            dbConn = new OleDbConnection(CON_STRING);
            dbConn.Open();
            return dbConn;
        }

        #region IDisposable ��Ա

        //��using���ǿ��Բ��õ������Dispose����
        public void Dispose()
        {
            dbConn.Dispose();
            dbConn.Close();
        }

        #endregion

    }
}




//�����������ļ��ĵ�ַ�ڳ�����д����
//�����ҳ�һ���Ƚ�����scheme��