using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;

using Hy.Check.Define;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    /// <summary>
    /// ����м�ʼ����
    /// </summary>
    public class RuleArea :BaseRule 
    {

        //�������ṹ��
        public RuleExpression.AreaParameter m_structAreaPara;

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
            m_structAreaPara = new RuleExpression.AreaParameter();

            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();

            //�����ַ���
            int nSize = sizeof(int);
            Byte[] bb = new byte[nCount1];
            pParameter.Read(bb, 0, nCount1);
            string para_str = Encoding.Default.GetString(bb);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_structAreaPara.strAlias = strResult[i++];
            m_structAreaPara.strScript = strResult[i++];
            m_structAreaPara.strFtName = strResult[i++];

            m_structAreaPara.dbThreshold = pParameter.ReadDouble();

            int fieldLength = pParameter.ReadInt32();

            //�����ֶ���
            m_structAreaPara.fieldArray = new List<string>();
            if (fieldLength > 0)
            {
                Byte[] bb1 = new byte[fieldLength];
                pParameter.Read(bb1, 0, fieldLength);
                string strFields = Encoding.Default.GetString(bb1);

                strFields.Trim();

                string[] strResult1 = strFields.Split('|');


                for (int j = 0; j < strResult1.Length; j++)
                {
                    m_structAreaPara.fieldArray.Add(strResult1[j]);
                }
            }

            //ȡ�ֶ�����
            m_structAreaPara.fieldTypeArray = new ArrayList();
            int fieldNum = pParameter.ReadInt32();
            int fType = -1;
            if (fieldNum > 0)
            {
                for (int f = 0; f < fieldNum; f++)
                {
                    fType = pParameter.ReadInt32();
                    m_structAreaPara.fieldTypeArray.Add(fType);
                }
            }
        }

        public override bool Verify()
        {
            if (m_structAreaPara == null)
            {
                SendMessage(enumMessageType.VerifyError, "������ȡ���ɹ�");
                return false;
            }

            try
            {
                m_LayerName = base.GetLayerName(m_structAreaPara.strFtName);
                (m_BaseWorkspace as IFeatureWorkspace).OpenFeatureClass(m_LayerName);
            }
            catch
            {
                SendMessage(enumMessageType.VerifyError,string.Format("ͼ��{0}������",m_structAreaPara.strFtName));
                return false;
            }

            return true;
        }

        private string m_LayerName;
        public override bool Check(ref List<Error> checkResult)
        {
            ITable ipTable = null;
            ICursor ipCursor = null;
            try
            {
                //���ݱ���ȡͼ����
                m_LayerName = base.GetLayerName(m_structAreaPara.strFtName);// m_structAreaPara.strFtName; //LayerReader.GetAliasName(m_structAreaPara.strFtName, this.m_SchemaID);

                //����Ӧ��featureclass
                IFeatureWorkspace ipFtWS;
                ipFtWS = (IFeatureWorkspace)this.m_BaseWorkspace;
                if (ipFtWS != null)
                {
                    try
                    {
                        ipTable = ipFtWS.OpenTable(m_LayerName);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��в�����ͼ��" + m_structAreaPara.strFtName + "���޷�ִ������м���!");
                        return false;
                    }
                }
                else
                {
                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��в�����ͼ��" + m_structAreaPara.strFtName + "���޷�ִ������м���!");
                    return false;
                }

                //ִ�в�ѯ����	       
                IQueryFilter pFilter = new QueryFilterClass();
                pFilter.WhereClause = ConstructClause();
                pFilter.SubFields = "OBJECTID,BSM,shape_area";

                //���Ų�ѯ
                try
                {
                    ipCursor = ipTable.Search(pFilter, true);
                }
                catch (Exception ex)
                {
                    SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ��ͼ��" + m_structAreaPara.strFtName + "�У�SQL���" + pFilter.WhereClause + "�޷�ִ��!");
                    return false;
                }
                //�����Ϊ�գ�����ʽ����ȷ
                if (ipCursor == null)
                {
                    return false;
                }
               checkResult= GetResult(ipCursor);
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

            int nIndexShapeArea = pFields.FindField("Shape_area");

            List<Error> errorList = new List<Error>();
            while (ipRow != null)
            {
                // ��ҽ����¼
                Error error = new Error();
                error.DefectLevel = this.m_DefectLevel;
                error.RuleID = this.InstanceID;

                error.OID = ipRow.OID;

                if (nIndex >= 0)
                {
                    error.BSM = ipRow.get_Value(nIndex).ToString();

                    double dArea = Convert.ToDouble(ipRow.get_Value(nIndexShapeArea));
                    error.Description = string.Format("'{0}'�ڱ�ʶ��Ϊ'{1}'�����Ϊ{2}��ͼ������Ƭ����Ρ�������ͼ����С��ͼ���({3})��Ҫ��", m_structAreaPara.strFtName, error.BSM, dArea.ToString("f2"), COMMONCONST.dAreaThread);
                }
                else
                {
                    // ������Ϣ
                    error.Description = m_structAreaPara.strScript;
                }

                error.LayerName = m_structAreaPara.strFtName;


                errorList.Add(error);

                ipRow = pCursor.NextRow();
            }

            return errorList;
        }

        private string ConstructClause()
        {
            string strClause;
            try
            {
                
                //strClause = "abs(shape_Area) <" + m_structAreaPara.dbThreshold + "";
                strClause = "abs(shape_Area) <" + COMMONCONST.dAreaThread+ "";
                
                string strMid = " and ";

                if (m_structAreaPara.fieldTypeArray.Count > 0)
                {
                    if (Convert.ToInt32(m_structAreaPara.fieldTypeArray[0]) != 5)
                    {
                        strClause = strClause + strMid + m_structAreaPara.fieldArray[0] + " IS NULL ";
                    }
                    else
                    {
                        strClause = strClause + strMid + " (" + m_structAreaPara.fieldArray[0]
                                    + " is null or " + m_structAreaPara.fieldArray[0] + " = '')";
                    }
                    for (int i = 1; i < m_structAreaPara.fieldArray.Count; i++)
                    {
                        if (Convert.ToInt32(m_structAreaPara.fieldTypeArray[i]) != 5)
                            strClause = strClause + strMid + m_structAreaPara.fieldArray[i] + " IS NULL ";
                        else
                        {
                            strClause = strClause + strMid + " ( " + m_structAreaPara.fieldArray[i]
                                        + " IS NULL OR " + m_structAreaPara.fieldArray[i] + " = '' )";
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return strClause;

        }

    }
}