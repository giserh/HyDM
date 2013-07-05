using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using Hy.Check.Define;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    public class RuleSQLExpression : BaseRule
    {
        private string strField1 = "";
        private string strField2 = "";
        //���ݱ���ȡͼ����
        private string layerName = "";

        //��ѯ�����ṹ��
        private SQLPARA m_structSqlPara = new SQLPARA();

        public RuleSQLExpression()
        {
            //��ղ�ѯ����
            m_structSqlPara.strSQLName = "���Ա��ʽ(SQL)�ʼ����";
            m_structSqlPara.strFtName = "";
            m_structSqlPara.strWhereClause = "";
        }

        // ��ȡ�����
        private List<Error> GetResult(ICursor pCursor)
        {
            IRow ipRow;
            ipRow = pCursor.NextRow();
            if (ipRow == null)
            {
                return null;
            }
            // �ж��Ƿ���OID��
            bool bHasOID;
            bHasOID = ipRow.HasOID;

            if (!bHasOID)
            {
                return null;
            }
            IFields pFields = ipRow.Fields;
            int nIndex = pFields.FindField("BSM");

            int nIndexJSMJ = pFields.FindField(strField1);
            int nIndexJSMJDIST = pFields.FindField(strField2);

            //string strErrInfo = ConstructErrorInfo();
            List<Error> pResAttr = new List<Error>();

            while (ipRow != null)
            {
                int OID;
                OID = ipRow.OID;

                Error err = new Error();
                err.DefectLevel = this.m_DefectLevel;
                err.RuleID = this.InstanceID;

                err.OID = OID;
                err.LayerName = m_structSqlPara.strFtName; // Ŀ��ͼ��              
                if (nIndex >= 0)
                {
                    err.BSM =ipRow.get_Value(nIndex).ToString();
                }

                // ������Ϣ
                if (m_structSqlPara.strScript.Contains("����������㲻��ȷ"))
                {
                    double dJSMJ = Convert.ToDouble(ipRow.get_Value(nIndexJSMJ));
                    double dJSMJDIST = Convert.ToDouble(ipRow.get_Value(nIndexJSMJDIST));
                    double dPlus = Math.Abs(dJSMJ-dJSMJDIST);
                    //pResInfo.strErrInfo = "���ݿ��������Ϊ"+dJSMJ.ToString("f2")+"ƽ���ף��ʼ���������������Ϊ"+dJSMJDIST.ToString("f2")+"ƽ���ף��������"+dPlus.ToString("f2")+"ƽ����";
                    //pResInfo.strErrInfo = string.Format(Helper.ErrMsgFormat.ERR_4401, m_structSqlPara.strFtName, pResInfo.BSM, "���ݿ��������", dJSMJ.ToString("f2") + "ƽ����", dJSMJDIST.ToString("f2") + "ƽ����", dPlus.ToString("f2") + "ƽ����");
                    err.Description = string.Format("���ݿ���'{0}'���б�ʶ��Ϊ'{1}'��{2}({3}ƽ����)�������������({4}ƽ����)��һ�£����߲�ֵΪ{5}ƽ����",
                        m_structSqlPara.strFtName, err.BSM, "���ݿ��������", dJSMJ.ToString("f2"), dJSMJDIST.ToString("f2"), dPlus.ToString("f2"));
                }              
                else
                {
                    err.Description = m_structSqlPara.strScript;
                    //pResInfo.strErrInfo = string.Format("'{0}'ͼ���ʶ��Ϊ'{1}'��ͼ�ߵ��������ֵ����ȷ��Ӧ����0", pResInfo.strTargetLayer, pResInfo.BSM);
                }

                pResAttr.Add(err);
                ipRow = pCursor.NextRow();
            }
            return pResAttr;
        }

        public override string Name
        {
            get
            {
                return m_structSqlPara.strAlias;
            }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            if (objParamters == null || objParamters.Length == 0) return;

            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));

            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();

            //�����ַ���
            Byte[] bb = new byte[nCount1];
            pParameter.Read(bb, 0, nCount1);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();

            string[] strResult = para_str.Split('|');
            int i = 0;
            m_structSqlPara.strAlias = strResult[i++];
            m_structSqlPara.strScript = strResult[i++];
            m_structSqlPara.strFtName = strResult[i++];
            m_structSqlPara.strWhereClause = strResult[i];
            return ;
        }

        public override bool Verify()
        {
            try
            {
                if (string.IsNullOrEmpty(m_structSqlPara.strFtName) ||
                    string.IsNullOrEmpty(m_structSqlPara.strWhereClause))
                {
                    //XtraMessageBox.Show("���Ŀ�������ʽ�����ڣ��޷�ִ�м�飡");
                    SendMessage(enumMessageType.VerifyError, "��ȡ�������ʧ�ܣ�");
                    return false;
                }

                //����Ӧ��featureclass
                if (base.m_QueryWorkspace is IFeatureWorkspace)
                {
                    IFeatureWorkspace ipFtWS = (IFeatureWorkspace)base.m_QueryWorkspace;
                    if(!(ipFtWS  as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable,layerName))
                    {
                        string strLog = string.Format("��ǰ���ݿ��в�����ͼ�� {0}���޷�ִ�����Ա��ʽ(SQL)���!", layerName);
                        SendMessage(enumMessageType.VerifyError, strLog);
                        return false;
                    }
                    //��ȡͼ�����
                    layerName = base.GetLayerName(m_structSqlPara.strAlias);
                }
                else
                {
                    SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ��޷��򿪣��޷�ִ�����Ա��ʽ(SQL)���!");
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
            ITable ipTable = null;
            ICursor ipCursor = null;
            checkResult = new List<Error>();
            try
            {
                if (m_structSqlPara.strFtName == "��״����" && 
                    m_structSqlPara.strAlias == "SQL_��״������")
                {
                    int nKDThread = Convert.ToInt32(COMMONCONST.nMapScale * 0.002);
                    m_structSqlPara.strWhereClause =string.Format("[KD] >= {0} or [KD] < 0", nKDThread);
                    m_structSqlPara.strScript =string.Format("��״�������ֶ�ֵ���ڵ���{0} ����С��0", nKDThread);
                }

                //ִ�в�ѯ����	       
                IQueryFilter pFilter = new QueryFilterClass();

                pFilter.WhereClause = m_structSqlPara.strWhereClause;

                if (m_structSqlPara.strScript.Contains("����������㲻��ȷ"))
                {
                    string strTemp = m_structSqlPara.strWhereClause;
                    int n1 = strTemp.IndexOf('[');
                    int n2 = strTemp.IndexOf(']');

                    int n3 = strTemp.LastIndexOf('[');
                    int n4 = strTemp.LastIndexOf(']');

                    strField1 = strTemp.Substring(n1 + 1, n2 - n1 - 1);

                    strField2 = strTemp.Substring(n3 + 1, n4 - n3 - 1);
                    pFilter.SubFields = "OBJECTID,BSM," + strField1 + "," + strField2;
                }
                else
                {
                    pFilter.SubFields = "OBJECTID,BSM";
                }

                //���Ų�ѯ
                try
                {
                    ipCursor = ipTable.Search(pFilter, true);
                }
                catch (Exception ex)
                {
                    SendMessage(enumMessageType.RuleError, string.Format("Sql���{0}�޷���{1}ͼ����ִ��", pFilter.WhereClause, layerName));
                    SendMessage(enumMessageType.Exception, string.Format("Sql���{0}�޷���{1}ͼ����ִ�У�ԭ��:{2}", pFilter.WhereClause, layerName, ex.Message));
                    return false;
                }

                //�����Ϊ�գ�����ʽ����ȷ
                if (ipCursor == null)
                {

                    return false;
                }

                checkResult = GetResult(ipCursor);

                if (checkResult == null)
                    return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (ipCursor != null)
                {
                    Marshal.ReleaseComObject(ipCursor);
                    ipCursor = null;
                }
                if (ipTable != null)
                {
                    Marshal.ReleaseComObject(ipTable);
                    ipTable = null;
                }
            }
            return true;
        }
    }

    //SQL��ѯ�������ṹ��
    public struct SQLPARA
    {
        public string strSQLName; //�ʼ�������
        public string strAlias; //��ѯ�������
        public string strScript; //����
        public string strFtName; //Ҫ������,�Ƿ���Ҫ
        public string strWhereClause; //��ѯ����
    } ;
}