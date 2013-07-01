using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Check.Define;

using System.Runtime.InteropServices;
using Check.Rule.Helper;
using Check.Utility;

namespace Check.Rule
{
    /// <summary>
    /// ���Կ�ֵ�ʼ����
    /// </summary>
    public class RuleBlankVal : BaseRule
    {
        

        //�ʼ�����ṹ��
        public RuleExpression.BLANKVALPARA m_structBlankPara;


        public override string Name
        {
            get { return "���Կ�ֵ�ʼ����"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            m_structBlankPara = new RuleExpression.BLANKVALPARA();
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
            m_structBlankPara.strAlias = strResult[i++];
            m_structBlankPara.strScript = strResult[i++];
            m_structBlankPara.strFtName = strResult[i++];

            m_structBlankPara.iType = pParameter.ReadInt32();
            int fieldLength = pParameter.ReadInt32();

            //�����ֶ���
            if (fieldLength > 0)
            {
                Byte[] bb1 = new byte[fieldLength];
                pParameter.Read(bb1, 0, fieldLength);
                string strFields = Encoding.Default.GetString(bb1);

                strFields.Trim();

                //�����ֶ���
                string[] strResult1 = strFields.Split('|');

                m_structBlankPara.fieldArray = new List<string>();

                for (int j = 0; j < strResult1.Length; j++)
                {
                    m_structBlankPara.fieldArray.Add(strResult1[j]);
                }
            }

            int fieldNum = pParameter.ReadInt32();
            m_structBlankPara.fieldTypeArray = new ArrayList();

            int fType = -1;

            for (int f = 0; f < fieldNum; f++)
            {
                fType = pParameter.ReadInt32();
                m_structBlankPara.fieldTypeArray.Add(fType);
            }
        }

        public override bool Verify()
        {
            if (this.m_structBlankPara == null)
            {
                SendMessage(enumMessageType.VerifyError, "������ȡ���ɹ�");
                return false;
            }

            //���ݱ���ȡͼ����
            m_LayerName = this.GetLayerName(m_structBlankPara.strFtName);
            //����Ӧ��featureclass

            try
            {
                (this.m_BaseWorkspace as IFeatureWorkspace).OpenTable(m_LayerName);
            }
            catch
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ��в�����ͼ��" + m_LayerName + "���޷�ִ�����Կ�ֵ���!");
                return false;
            }
            return true;
        }
        string m_LayerName = null;
        public override bool Check(ref List<Error> checkResult)
        {

            ITable ipTable = null;
            ICursor ipCursor = null;
            try
            {

                //����Ӧ��featureclass
                IFeatureWorkspace ipFtWS;
                ipFtWS = (IFeatureWorkspace)this.m_BaseWorkspace;
                if (ipFtWS != null)
                {
                    try
                    {
                        ipTable = ipFtWS.OpenTable(m_LayerName);
                    }
                    catch
                    {
                        SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��в�����ͼ��" + m_LayerName + "���޷�ִ�����Կ�ֵ���!");
                        return false;
                    }
                }
                else
                {
                    SendMessage(enumMessageType.RuleError, "Base���Ȼ����FeatureWorkspace!");
                    return false;
                }

                //ִ�в�ѯ����
                IQueryFilter pFilter = new QueryFilterClass();
                pFilter.WhereClause = ConstructClause();
                pFilter.SubFields = "OBJECTID,BSM";

                //���Ų�ѯ
                try
                {
                    ipCursor = ipTable.Search(pFilter, true);
                    if (ipCursor == null)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��ͼ��" + m_LayerName + "�У�SQL���" + pFilter.WhereClause + "�޷�ִ��!");
                    return false;
                }

                //�����Ϊ�գ�����ʽ����ȷ
                if (ipCursor == null)
                {
                    return false;
                }
                checkResult = GetResult(ipCursor);

                return checkResult != null;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
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

        

        

        private string ConstructClause()
        {
            string strMid;
            if (m_structBlankPara.iType == 0)
            {
                strMid = " AND ";
            }
            else
            {
                strMid = " OR ";
            }
            string strClause;
            if (Convert.ToInt32(m_structBlankPara.fieldTypeArray[0]) != 5)
            {
                strClause = m_structBlankPara.fieldArray[0] + " IS NULL ";
            }
            else
            {
                strClause = "( " + m_structBlankPara.fieldArray[0] + " IS NULL OR " +
                            m_structBlankPara.fieldArray[0] + " = '' ) ";
            }
            for (int i = 1; i < m_structBlankPara.fieldArray.Count; i++)
            {
                if (Convert.ToInt32(m_structBlankPara.fieldTypeArray[i]) != 5)
                    strClause = strClause + strMid + m_structBlankPara.fieldArray[i] + " IS NULL ";
                else
                {
                    strClause = strClause + strMid + " ( " + m_structBlankPara.fieldArray[i]
                                + " IS NULL OR " + m_structBlankPara.fieldArray[i] + " = '' )";
                }
            }
            return strClause;
        }

        private List<Error> GetResult(ICursor pCursor)
        {
            IRow ipRow;
            ipRow = pCursor.NextRow();
            if (ipRow == null)
            {
                return new List<Error>();
            }

            // �ж��Ƿ���OID��
            bool bHasOID;
            bHasOID = ipRow.HasOID;

            if (!bHasOID)
            {
                SendMessage(enumMessageType.RuleError, "��ǰ���Ķ���û��OID�ֶΣ������м��");
                return null;
            }

            string strErrMsg;
            if (m_structBlankPara.iType == 0)
            {
                strErrMsg = "Ŀ���ֶ�ֵ��Ϊ��";
            }
            else
            {
                strErrMsg = "����Ŀ���ֶ�ֵΪ��";
            }

            string strErrInfo = ConstructErrorInfo();
            List<Error> pResAttr = new List<Error>();

            IFields pFields = ipRow.Fields;
            int nIndex = pFields.FindField("BSM");

            while (ipRow != null)
            {
                // ��ҽ����¼
                Error pResInfo = new Error();
                pResInfo.DefectLevel = this.m_DefectLevel;
                pResInfo.RuleID = this.InstanceID;

                int OID;
                OID = ipRow.OID;
                pResInfo.OID = OID;

                if (nIndex >= 0)
                {
                    pResInfo.BSM = ipRow.get_Value(nIndex).ToString();
                }

                pResInfo.LayerName = m_structBlankPara.strFtName;

                // ������Ϣ
                if (m_structBlankPara.strScript.Trim() != "" && m_structBlankPara.strScript != null)
                {
                    pResInfo.Description = m_structBlankPara.strScript;
                }
                else
                {
                    pResInfo.Description = strErrInfo;
                }
                
                pResAttr.Add(pResInfo);

                ipRow = pCursor.NextRow();
            }
            return pResAttr;
        }

        private string ConstructErrorInfo()
        {
            string strInfo = "";
            string strFields = "";
            StandardLayer distLayer = LayerReader.GetLayerByName(m_structBlankPara.strFtName, SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID));
            if (m_structBlankPara.fieldArray.Count > 0)
            {
                for (int i = 1; i < m_structBlankPara.fieldArray.Count; i++)
                {
                    strFields = strFields + "|" + FieldReader.GetAliasName(m_structBlankPara.fieldArray[i], distLayer.ID);
                }
                strFields = strFields.Remove(0, 1);
            }
            if (m_structBlankPara.iType == 0)
            {
                strInfo = "�ֶ� " + strFields + " ��Ϊ��";
            }
            else if (m_structBlankPara.iType == 1)
            {
                strInfo = "�ֶ� " + strFields + " ����һ������Ϊ��";
            }
            return strInfo;
        }


    }
}