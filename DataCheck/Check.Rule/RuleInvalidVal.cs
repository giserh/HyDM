using System;
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
    public class RuleInvalidVal : BaseRule
    {
        //�����ṹ��
        private RuleExpression.INVALIDPARA m_structInvalidPara = new RuleExpression.INVALIDPARA();
        private int m_StandarID;
        private string m_layerName;
        public RuleInvalidVal()
        {
            //��ղ�ѯ����
            m_structInvalidPara.strInvalidName = "�Ƿ��ַ��ʼ����";
            m_structInvalidPara.strFtName = "";
            m_structInvalidPara.charSetArray = null;
            m_structInvalidPara.strAlias = "";
            m_structInvalidPara.fieldArray = null;
        }

        public override string Name
        {
            get { return "�Ƿ��ַ��ʼ����"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
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
            m_structInvalidPara.strAlias = strResult[i++];
            m_structInvalidPara.strScript = strResult[i++];
            m_structInvalidPara.strFtName = strResult[i++];

            int fieldLength = pParameter.ReadInt32();

            m_structInvalidPara.fieldArray = new List<string>();

            //�����ֶ���
            if (fieldLength > 0)
            {
                Byte[] bb1 = new byte[fieldLength];
                pParameter.Read(bb1, 0, fieldLength);
                string strFields = Encoding.Default.GetString(bb1);

                strFields.Trim();

                //�����ֶ���
                string[] strResult1 = strFields.Split('|');


                for (int j = 0; j < strResult1.Length; j++)
                {
                    m_structInvalidPara.fieldArray.Add(strResult1[j]);
                }
            }

            //�Ƿ��ַ�
            m_structInvalidPara.charSetArray = new List<string>();
            int fieldNum = pParameter.ReadInt32();
            if (fieldNum > 0)
            {
                Byte[] bb2 = new byte[fieldNum];
                pParameter.Read(bb2, 0, fieldNum);
                string strChars = Encoding.Default.GetString(bb2);

                strChars.Trim();

                //�����ֶ���
                string[] strResult2 = strChars.Split('|');


                for (int k = 0; k < strResult2.Length; k++)
                {
                    m_structInvalidPara.charSetArray.Add(strResult2[k]);
                }
            }

        }

        public override bool Verify()
        {

            m_StandarID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            //���ݱ���ȡͼ����
            m_layerName = LayerReader.GetNameByAliasName(m_structInvalidPara.strFtName, m_StandarID);

            if (m_QueryWorkspace == null)
            {
                return false;
            }

            if (!(m_QueryWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable, m_layerName))
            {
                string strLog = "��ǰ�������ݿ��в�����ͼ��" + m_layerName + "���޷�ִ�зǷ��ַ����!";
                SendMessage(enumMessageType.RuleError, strLog);
                return false;
            }
            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
            //����Ӧ��featureclass
            ITable ipTable=null;
            ICursor ipCursor=null;
            try
            {
                IFeatureWorkspace ipFtWS;
                ipFtWS = (IFeatureWorkspace) m_QueryWorkspace;
                ipTable = ipFtWS.OpenTable(m_layerName);

                //ִ�в�ѯ����	       
                IQueryFilter pFilter = new QueryFilterClass();

                pFilter.WhereClause = ConstructClause();
                pFilter.SubFields = "OBJECTID,BSM";

                //���Ų�ѯ
                try
                {
                    ipCursor = ipTable.Search(pFilter, false);
                    if (ipCursor == null)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {

                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��ͼ��" + m_layerName + "�У�SQL���" + pFilter.WhereClause + "�޷�ִ��!");
                    return false;
                }
                //�����Ϊ�գ�����ʽ����ȷ
                if (ipCursor == null)
                {
                    return false;
                }
                checkResult = GetResult(ipCursor);

                return checkResult!=null;
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
        }
  

        private string ConstructClause()
        {
            string strClause;

            string strFiled = m_structInvalidPara.fieldArray[0];
            string invalidChar = m_structInvalidPara.charSetArray[0];
            //���Ϊ*?#�����ַ��������⴦��,��[]
            if (invalidChar.CompareTo("*") == 0 || invalidChar.CompareTo("#") == 0
                || invalidChar.CompareTo("?") == 0)
            {
                strClause = strFiled + " LIKE " + "\'*[" + invalidChar + "]*\'";
            }
            else
            {
                strClause = strFiled + " LIKE " + "\'*" + invalidChar + "*\'";
            }

            for (int j = 1; j < m_structInvalidPara.charSetArray.Count; j++)
            {
                invalidChar = m_structInvalidPara.charSetArray[j];
                if (invalidChar.CompareTo("*") == 0 || invalidChar.CompareTo("#") == 0
                    || invalidChar.CompareTo("?") == 0)
                {
                    strClause = strClause + " OR " + strFiled +
                                " LIKE " + "\'*[" + invalidChar + "]*\'";
                }
                else
                {
                    strClause = strClause + " OR " + strFiled +
                                " LIKE " + "\'*" + invalidChar + "*\'";
                }
            }

            for (int i = 1; i < m_structInvalidPara.fieldArray.Count; i++)
            {
                for (int j = 0; j < m_structInvalidPara.charSetArray.Count; j++)
                {
                    invalidChar = m_structInvalidPara.charSetArray[j];
                    if (invalidChar.CompareTo("*") == 0 || invalidChar.CompareTo("#") == 0
                        || invalidChar.CompareTo("?") == 0)
                    {
                        strClause = strClause + " OR " + m_structInvalidPara.fieldArray[i] +
                                    " LIKE " + "\'*[" + invalidChar + "]*\'";
                    }
                    else
                    {
                        strClause = strClause + " OR " + m_structInvalidPara.fieldArray[i] +
                                    " LIKE " + "\'*" + invalidChar + "*\'";
                    }
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
                return null;
            }
            // �ж��Ƿ���OID��
            bool bHasOID;
            bHasOID = ipRow.HasOID;

            if (!bHasOID)
            {
                return null;
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

                pResInfo.OID = ipRow.OID;

                if (nIndex >= 0)
                {
                    pResInfo.BSM = ipRow.get_Value(nIndex).ToString();
                }

                pResInfo.LayerName = m_structInvalidPara.strFtName;

                // ������Ϣ
                if (m_structInvalidPara.strScript.Trim()!=""&& m_structInvalidPara.strScript != null)
                {
                    pResInfo.Description = m_structInvalidPara.strScript;
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
            string strInfo;
            string strFields = "";
            StandardLayer lyr=LayerReader.GetLayerByName(m_structInvalidPara.strFtName,this.m_StandarID);
            if(lyr==null)
            {
                return "";
            }
            int lyrID=lyr.ID;
            if (m_structInvalidPara.fieldArray.Count > 0)
            {
                for (int i = 0; i < m_structInvalidPara.fieldArray.Count; i++)
                {
                    string strTFields = FieldReader.GetNameByAliasName(m_structInvalidPara.fieldArray[i], lyrID);
                    strFields = strFields + "|" + strTFields;
                }
            }
            string strCharset;
            strCharset = m_structInvalidPara.charSetArray[0];
            for (int j = 1; j < m_structInvalidPara.charSetArray.Count; j++)
            {
                strCharset += "," + m_structInvalidPara.charSetArray[j];
            }
            strInfo = "�ֶ� " + strFields + " ֵ�к��в�����Ҫ����ַ�(" + strCharset + ")�е�һ������";

            return strInfo;
        }



      
    }
}