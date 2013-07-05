using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Hy.Check.Define;

using System.Runtime.InteropServices;
using Hy.Check.Rule.Helper;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    public class RuleLength : BaseRule
    {
       
        public RuleLength()
        {
            m_structLengthPara.strName = "����м�ʼ����";
        }

        //�����ṹ��
        private RuleExpression.LENGTHPARA m_structLengthPara = new RuleExpression.LENGTHPARA();

        // Ŀ���ֶ������ַ���
        private string m_strTargetFields;


        public override string Name
        {
            get { return "����м�ʼ����"; }
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
            m_structLengthPara.strAlias = strResult[i++];
            m_structLengthPara.strScript = strResult[i++];
            m_structLengthPara.strFtName = strResult[i];

            m_structLengthPara.dbThreshold = pParameter.ReadDouble();

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
                m_structLengthPara.fieldArray = new List<string>();


                for (int j = 0; j < strResult1.Length; j++)
                {
                    m_structLengthPara.fieldArray.Add(strResult1[j]);
                }
            }

            //ȡ�ֶ�����
            int fieldNum = pParameter.ReadInt32();
            int fType = -1;

            if (fieldNum > 0)
            {
                m_structLengthPara.fieldTypeArray = new List<int>();
                for (int f = 0; f < fieldNum; f++)
                {
                    fType = pParameter.ReadInt32();
                    m_structLengthPara.fieldTypeArray.Add(fType);
                }
            }

        }

        public override bool Verify()
        {
            try
            {
                //���ݱ���ȡͼ����
               m_LayerName = base.GetLayerName(m_structLengthPara.strFtName);
                (m_BaseWorkspace as IFeatureWorkspace).OpenTable(m_LayerName);
            }
            catch
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ��в�����ͼ��" + m_structLengthPara.strFtName + "���޷�ִ������м���!");
            }

            return true;
        }

        private string m_LayerName = null;
        public override bool Check(ref List<Error> checkResult)
        {
            ITable ipTable = null;
            ICursor ipCursor = null;
            try
            {
              
                //����Ӧ��featureclass
                try
                {
                    ipTable = (m_BaseWorkspace as IFeatureWorkspace).OpenTable(m_LayerName);
                }
                catch
                {
                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��в�����ͼ��" + m_structLengthPara.strFtName + "���޷�ִ������м���!");
                    return false;
                }
                

                //ִ�в�ѯ����	       
                IQueryFilter pFilter = new QueryFilterClass();

                pFilter.WhereClause = ConstructClause();
                pFilter.SubFields = "OBJECTID,BSM,Shape_Length";

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
                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��ͼ��" + m_structLengthPara.strFtName + "�У�SQL���" + pFilter.WhereClause + "�޷�ִ��!");
                    SendMessage(enumMessageType.Exception,ex.ToString());
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
            catch(Exception exp)
            {
                SendMessage(enumMessageType.RuleError,string.Format("����ʧ�ܣ���Ϣ��{0}",exp.Message));
                SendMessage(enumMessageType.Exception,exp.ToString());
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
           

        private string ConstructErrorInfo()
        {
            string strInfo;
            strInfo = "�߳���С����ֵ" +COMMONCONST.dLengthThread + " ";
            string strFields = "";
            StandardLayer distLayer = LayerReader.GetLayerByName(m_structLengthPara.strFtName, SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID));
            if (m_structLengthPara.fieldArray != null)
            {
                for (int i = 0; i < m_structLengthPara.fieldArray.Count; i++)
                {
                    strFields = strFields + "|" + FieldReader.GetAliasName(m_structLengthPara.fieldArray[i], distLayer.ID);
                }
                strFields = strFields.Remove(0, 1);
                strFields = strFields + " �ֶζ�Ϊ��";
                strInfo = strInfo + ",���� " + strFields;
            }


            return strInfo;
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
                SendMessage(enumMessageType.RuleError, "��ǰ���Ķ�������Ҫ���ࣨû��OID���������м��");
                return null;
            }

            IFields pFields = ipRow.Fields;
            int nIndex = pFields.FindField("BSM");

            int nIndexShapeLength = pFields.FindField("Shape_Length");

            //string strErrInfo = ConstructErrorInfo();
            List<Error> pResAttr = new List<Error>();
            while (ipRow != null)
            {
                // ��ҽ����¼
                Error pResInfo = new Error();
                pResInfo.DefectLevel = this.m_DefectLevel;
                pResInfo.RuleID = this.InstanceID;

                int OID = ipRow.OID;

                if (nIndex >= 0)
                {
                    pResInfo.BSM = ipRow.get_Value(nIndex).ToString();

                    if (nIndexShapeLength > 0)
                    {
                        double dLength = Convert.ToDouble(ipRow.get_Value(nIndexShapeLength));
                        pResInfo.Description = string.Format("'{0}'���ڱ�ʶ��Ϊ'{1}'������Ϊ'{2}'����״���������ߡ���������С��ͼ����({3})��Ҫ��", m_structLengthPara.strFtName, pResInfo.BSM, dLength.ToString("f2"), COMMONCONST.dLengthThread);
                    }
                    else
                    {
                        pResInfo.Description = string.Format("'{0}'���ڱ�ʶ��Ϊ'{1}'����״���������ߡ���������С��ͼ����({2})��Ҫ��", m_structLengthPara.strFtName, pResInfo.BSM, COMMONCONST.dLengthThread);
                    }
                    
                }
                else
                {
                    pResInfo.Description = m_structLengthPara.strScript;   // ������Ϣ
                
                }
                pResInfo.OID = OID;
                pResInfo.LayerName = m_structLengthPara.strFtName;              
            
                
                pResAttr.Add(pResInfo);
                ipRow = pCursor.NextRow();
            }

            return pResAttr;
        }

        private string ConstructClause()
        {
            string strClause;
            //strClause = "abs(shape_length) < " + m_structLengthPara.dbThreshold + "";
            strClause = "abs(shape_length) < " + COMMONCONST.dLengthThread + "";

            string strMid = " AND ";

            if (m_structLengthPara.fieldTypeArray != null)
            {
                if (Convert.ToInt32(m_structLengthPara.fieldTypeArray[0]) != 5)
                {
                    strClause = strClause + strMid + m_structLengthPara.fieldArray[0] + " IS NULL ";
                }
                else
                {
                    strClause = strClause + strMid + " ( " + m_structLengthPara.fieldArray[0]
                                + " IS NULL OR " + m_structLengthPara.fieldArray[0] + " = '' )";
                }

                for (int i = 1; i < m_structLengthPara.fieldArray.Count; i++)
                {
                    if (Convert.ToInt32(m_structLengthPara.fieldTypeArray[i]) != 5)
                        strClause = strClause + strMid + m_structLengthPara.fieldArray[i] + " IS NULL ";
                    else
                    {
                        strClause = strClause + strMid + " ( " + m_structLengthPara.fieldArray[i]
                                    + " IS NULL OR " + m_structLengthPara.fieldArray[i] + " = '' )";
                    }
                }
            }

            return strClause;
        }



    }
}