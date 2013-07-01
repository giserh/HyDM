using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Check.Define;

using Check.Rule.Helper;
using Check.Utility;

namespace Check.Rule
{
    public class RuleFrequency : BaseRule
    {

        public RuleFrequency()
        {
            m_structFrePara.nMaxTime = 0;
            m_structFrePara.nMinTime = 0;
            m_structFrePara.nType = -1;
            m_structFrePara.strAlias = "";
            m_structFrePara.strFtName = "";
            m_structFrePara.strName = "����Ƶ�ȣ�Ψһ�ԣ��ʼ����";
            m_structFrePara.strScript = "";
        }

        public override string Name
        {
            get { return "����Ƶ�ȣ�Ψһ�ԣ��ʼ����"; }
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
            //int nSize = sizeof(int) * 4;
            Byte[] bb = new byte[nCount1];
            pParameter.Read(bb, 0, nCount1);

            m_structFrePara.nType = pParameter.ReadInt32();
            m_structFrePara.nMinTime = pParameter.ReadInt32();
            m_structFrePara.nMaxTime = pParameter.ReadInt32();

            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_structFrePara.strAlias = strResult[i++];
            m_structFrePara.strScript = strResult[i++];
            m_structFrePara.strFtName = strResult[i++];

            m_structFrePara.arrayFields = new List<string>();

            for (int j = i; j < strResult.Length; j++)
            {
                m_structFrePara.arrayFields.Add(strResult[j]);
            }

        }

        public override bool Verify()
        {
            int standardID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            lyr = LayerReader.GetLayerByAliasName(m_structFrePara.strFtName, standardID);
            if (lyr == null)
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ڵı�׼���Ҳ�����Ϊ��" + m_structFrePara.strFtName + "����ͼ��");
                return false;
            }

            return true;
        }
        private StandardLayer lyr = null;
        public override bool Check(ref List<Error> checkResult)
        {
            try
            {
                //���ݱ���ȡͼ����
                string layerName = lyr.Name;
                List<Error> m_pResAttr = new List<Error>();

                if (m_structFrePara.strAlias.Equals("Ƶ��_BSMȫ��Ψһ�Լ��"))
                {
                    // 2012-07-03 ��ʱ��ʵ��
                    //long longCount = 0;
                    //CCommonCheck.g_pBsmResAttr.GetResultCount(ref longCount);
                    //if (longCount == 0)
                    //{
                    //    CCommonCheck.GetBsmErrorForXml(m_TaskPath);
                    //}
                    //CCommonCheck.g_pBsmResAttr.SetFcAlias(ref Xstand);
                    //ppResult = CCommonCheck.g_pBsmResAttr as ICheckResult;
                }
                else
                {
                    //ͨ��ADO����⣬�����������m_arrResult
                    if (m_QueryConnection == null) return false;
                    string strField = m_structFrePara.arrayFields[0];

                    // �ֶα���ת��Ϊ��ʵ����
                    for (int j = 0; j < m_structFrePara.arrayFields.Count; j++)
                    {
                        strField = FieldReader.GetNameByAliasName(m_structFrePara.arrayFields[j], lyr.ID);
                        m_structFrePara.arrayFields[j] = strField;
                    }

                    string strSql, strGroup = "";
                    string strNullWhere = "";
                    for (int i = 0; i < m_structFrePara.arrayFields.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(m_structFrePara.arrayFields[i]))
                        {
                            strGroup += m_structFrePara.arrayFields[i] + ",";
                            strNullWhere += "((" + m_structFrePara.arrayFields[i] + " is not null) or " + m_structFrePara.arrayFields[i] + " <>'') and ";

                        }
                    }

                    if (strGroup == "")
                    {
                        return false;
                    }
                    strGroup = strGroup.Substring(0, strGroup.Length - 1);

                    strNullWhere = strNullWhere.Substring(0, strNullWhere.Length - 4);

                    strSql = "Select TOTAL," + strGroup + " from (Select count(*) as TOTAL," + strGroup + " From " + layerName + " GROUP BY " + strGroup + ") where ( TOTAL>=2 and " + strNullWhere + ")";

                    //�򿪼�¼����������	         
                    DataTable ipRecordset = new DataTable();
                    ipRecordset = Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql);
                    if (ipRecordset == null)
                    {
                        return false;
                    }

                    int index = 0;
                    List<string> listWhereClause = new List<string>();

                    foreach (DataRow dr in ipRecordset.Rows) //���������
                    {

                        string strFieldsValue = "";

                        for (int i = 0; i < m_structFrePara.arrayFields.Count; i++)
                        {
                            string strValue = "";
                            if (m_structFrePara.arrayFields[i] != "")
                            {
                                object varValue = dr[m_structFrePara.arrayFields[i]];

                                Type t = varValue.GetType();
                                TypeCode typeCode = Type.GetTypeCode(t);

                                switch (typeCode)
                                {
                                    case TypeCode.Int32:
                                    case TypeCode.Int64:
                                    case TypeCode.Double:
                                        {
                                            if (varValue == null || Convert.ToString(varValue) == "")
                                            {
                                                strValue = "(" + m_structFrePara.arrayFields[i] + " is null) and";

                                            }
                                            else
                                            {
                                                strValue = "(" + m_structFrePara.arrayFields[i] + " = " + varValue.ToString() +
                                                           ") and";
                                            }

                                            break;
                                        }
                                    case TypeCode.String:
                                    case TypeCode.Char:
                                        {
                                            if (varValue == null || Convert.ToString(varValue) == "")
                                            {
                                                strValue = "(" + m_structFrePara.arrayFields[i] + " is null or " +
                                                           m_structFrePara.arrayFields[i] + " = '' ) and ";

                                            }
                                            else
                                            {
                                                strValue = "(" + m_structFrePara.arrayFields[i] + " = '" + varValue.ToString() +
                                                           "') and ";
                                            }
                                            break;
                                        }
                                }
                                strFieldsValue += strValue;

                            }
                        }
                        strFieldsValue = strFieldsValue.Substring(0, strFieldsValue.Length - 4);
                        listWhereClause.Add(strFieldsValue);

                        index++;
                    }

                    ipRecordset.Dispose();

                    if (listWhereClause.Count == 0)
                        return true;

                    // Ŀ���ֶ��ַ���
                    string strTargetField = GetTargetField();
                    string strSql1 = "Select " + strGroup + ",BSM,ObjectID From " + layerName;
                    DataTable ipRecordsetRes = new DataTable();
                    ipRecordsetRes = Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql1);
                    if (ipRecordsetRes == null)
                    {
                        return false;
                    }

                    DataRow[] listRow = null;
                    for (int i = 0; i < listWhereClause.Count; i++)
                    {
                        string strWhere = listWhereClause[i];
                        listRow = ipRecordsetRes.Select(strWhere);
                        if (listRow != null && listRow.Length > 0)
                        {
                            string strListBSM = "";  //�ظ���BSM�ַ���
                            int nIndex = 0;
                            int nOID = 0;
                            int BSM = 0;
                            foreach (DataRow dr in listRow) //���������
                            {
                                nIndex++;
                                if (nIndex >= 2)
                                {
                                    strListBSM += dr["BSM"].ToString() + "|";
                                }
                                else
                                {
                                    BSM = Convert.ToInt32(dr["BSM"]);
                                    nOID = Convert.ToInt32(dr["ObjectID"]);
                                }
                            }
                            strListBSM = strListBSM.Substring(0, strListBSM.Length - 1);

                            // ��ӽ����¼
                            Error pResInfo = new Error();
                            pResInfo.DefectLevel = this.m_DefectLevel;
                            pResInfo.RuleID = this.InstanceID;

                            // OID
                            pResInfo.OID = nOID;
                            pResInfo.BSM = BSM.ToString();
                            pResInfo.ReferBSM = strListBSM;

                            // Ŀ��ͼ��
                            pResInfo.LayerName = m_structFrePara.strFtName;
                            // Ŀ���ֶ�
                            pResInfo.ReferLayerName = strTargetField;
                            // ������Ϣ
                            pResInfo.Description = string.Format("'{0}'���б�ʶ��Ϊ'{1}'��ͼ�����ʶ��Ϊ'{2}'��ͼ��'{3}({4})'�ֶ�ֵ({5})�����ظ�", pResInfo.LayerName, BSM, strListBSM, strField, m_structFrePara.arrayFields[0], listRow[0][0]);

                            m_pResAttr.Add(pResInfo);

                            ipRecordsetRes.Dispose();
                        }


                    }

                    checkResult = m_pResAttr;
                    return true;
                }
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }

            return true;
        }


        //�����ṹ��
        private RuleExpression.FREQUENCYPARA m_structFrePara = new RuleExpression.FREQUENCYPARA();


        private bool CheckCount(long count, ref string strErrorMsg)
        {
            //���Ϊ��һ����������ڵ��ڲ���С�ڵ���
            if (m_structFrePara.nType == 0)
            {
                if (count > m_structFrePara.nMinTime)
                {
                    if (m_structFrePara.nMaxTime != -1)
                    {
                        if (count < m_structFrePara.nMaxTime)
                            return true;
                        else
                        {
                            if (m_structFrePara.nMinTime == -1)
                            {
                                strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ�ȴ��ڵ���" + m_structFrePara.nMaxTime;
                            }
                            else
                            {
                                strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ�ȴ��ڵ���" + m_structFrePara.nMaxTime + "������С�ڵ���" +
                                              (m_structFrePara.nMinTime);
                            }
                            return false;
                        }
                    }
                    return true;
                }
                strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ�ȴ��ڵ���" + m_structFrePara.nMaxTime + "������С�ڵ���" +
                              (m_structFrePara.nMinTime);
            }
            //�ڶ��������С�ڵ��ڻ��ߴ��ڵ���
            else
            {
                if (m_structFrePara.nMinTime != -1)
                {
                    if (count < m_structFrePara.nMinTime)
                    {
                        if (count > m_structFrePara.nMaxTime)
                            return true;
                        strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ��С�ڵ���" + m_structFrePara.nMaxTime + "���ߴ��ڵ���" +
                                      m_structFrePara.nMinTime;
                        return false;
                    }
                    strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ��С�ڵ���" + m_structFrePara.nMaxTime + "���ߴ��ڵ���" +
                                  m_structFrePara.nMinTime;
                }
                else
                {
                    if (count > m_structFrePara.nMaxTime)
                        return true;
                    strErrorMsg = "���������ֵ�����ظ����趨���ظ�����Ϊ��Ŀ���ֶ�����ֵƵ��С�ڵ���" + m_structFrePara.nMaxTime;
                    return false;
                }
            }
            return false;
        }


        private bool CheckCount(long count)
        {
            //���Ϊ��һ����������ڵ��ڲ���С�ڵ���
            if (m_structFrePara.nType == 0)
            {
                if (count >= m_structFrePara.nMinTime)
                {
                    if (m_structFrePara.nMaxTime != -1)
                    {
                        if (count < m_structFrePara.nMaxTime)
                            return true;
                        else
                            return false;
                    }
                    return true;
                }
            }
            //�ڶ��������С�ڵ��ڻ��ߴ��ڵ���
            else
            {
                if (count <= m_structFrePara.nMinTime)
                {
                    if (m_structFrePara.nMinTime != -1)
                    {
                        if (count >= m_structFrePara.nMaxTime)
                            return true;
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        // ��ȡĿ���ֶ��ַ���
        private string GetTargetField()
        {
            string strTargetField = "";

            // Ŀ���ֶ�
            for (int i = 0; i < m_structFrePara.arrayFields.Count; i++)
            {
                strTargetField += m_structFrePara.arrayFields[i] + "|";
            }
            strTargetField = strTargetField.Substring(0, strTargetField.Length - 1);

            return strTargetField;
        }




    }
}