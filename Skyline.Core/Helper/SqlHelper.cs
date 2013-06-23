using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Skyline.Core.Helper
{
    public class SqlHelper
    {
        /// <summary>
        /// ���ݿ�����
        /// </summary>
        private OleDbConnection oledbConn;
        /// <summary>
        /// ���ݿ���������
        /// </summary>
        private OleDbCommand oledbCom;
        /// <summary>
        /// ���ݿ���������
        /// </summary>
        private OleDbDataAdapter oledbDap;
        /// <summary>
        /// ��ѯ������ڴ����ݱ�
        /// </summary>
        private DataSet ds;


        /// <summary>
        /// ��ѯȫ����Ϣ
        /// </summary>
        /// <param name="tablename">����</param>
        /// <param name="conditions">����</param>
        /// <returns></returns>
        public DataSet selectAll(string tablename, string conditions)
        {
            using (oledbConn = SqlConn.getOleConn())
            {
                try
                {
                    oledbCom = new OleDbCommand("select * from " + tablename + " where " + conditions, oledbConn);
                    oledbDap = new OleDbDataAdapter(oledbCom);
                    ds = new DataSet();
                    oledbDap.Fill(ds, tablename);
                }
                catch (Exception e)
                { 
                    System.Windows.Forms.MessageBox.Show(e.Message.ToString());
                }
                return ds;
            }
            
            
        }
        /// <summary>
        /// ��ѯȫ����Ϣ
        /// </summary>
        /// <param name="tablename">����</param>
        /// <param name="conditions">����</param>
        /// <returns></returns>
        public DataSet selectSQL(string tablename, string SQL)
        {
            using (oledbConn = SqlConn.getOleConn())
            {
                try
                {
                    oledbCom = new OleDbCommand(SQL, oledbConn);
                    oledbDap = new OleDbDataAdapter(oledbCom);
                    ds = new DataSet();
                    oledbDap.Fill(ds, tablename);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message.ToString());
                }
                return ds;
            }


        }
        /// <summary>
        /// ��ѯȫ����Ϣ
        /// </summary>
        /// <param name="tablename">����</param>
        /// <param name="conditions">����</param>
        /// <returns></returns>
        public DataSet selectTableAll(string tablename)
        {
            using (oledbConn = SqlConn.getOleConn())
            {
                try
                {
                    oledbCom = new OleDbCommand("select * from " + tablename + " ", oledbConn);
                    oledbDap = new OleDbDataAdapter(oledbCom);
                    ds = new DataSet();
                    oledbDap.Fill(ds, tablename);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message.ToString());
                }
                return ds;
            }


        }
        /// <summary>
        /// ���뷽��
        /// </summary>
        /// <param name="commStr">һ��������insert���sql��</param>
        /// <returns></returns>
        public bool Insert(string commStr)
        {
            using (oledbConn = SqlConn.getOleConn())
            {
                try
                {
                    oledbCom = new OleDbCommand(commStr, oledbConn);
                    if (oledbCom.ExecuteNonQuery() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                     System.Windows.Forms.MessageBox.Show(e.Message.ToString());
                     return false;
                }
            }
        }

        /// <summary>
        /// �жϽ����Ƿ����
        /// </summary>
        /// <param name="objectid">Object����</param>
        /// <returns></returns>
        public bool JudgeObjectID(string objectid)
        {
            oledbConn = SqlConn.getOleConn();
            string sql = "select * from builderObject where buildObjectID = '" + objectid + "'";
            oledbCom = new OleDbCommand(sql, oledbConn);
            if (oledbCom.ExecuteScalar() == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
