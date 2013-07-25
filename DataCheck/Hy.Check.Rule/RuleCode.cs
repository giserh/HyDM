using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Hy.Check.Define;

using Hy.Check.Rule.Helper;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    /// <summary>
    /// ֵ�����Լ��
    /// </summary>
    public class RuleCode : BaseRule
    {

        public override string Name
        {
            get { return "ֵ�����Լ��"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
            if (m_psPara == null)
            {
                m_psPara = new RuleExpression.LRCodePara();
            }

            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();
            m_psPara.nVersionID = pParameter.ReadInt32();

            //�����ַ���
            int nSize = sizeof(int) * 2;
            Byte[] bb = new byte[nCount1 - nSize];
            pParameter.Read(bb, 0, nCount1 - nSize);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();


            string[] strResult = para_str.Split('|');

            int i = 0;
            m_psPara.strName = strResult[i++];
            m_psPara.strAlias = strResult[i++];
            m_psPara.strFtName = strResult[i++];
            m_psPara.strRemark = strResult[i++];
            m_psPara.strCodeField = strResult[i++];
            m_psPara.strCodeType = strResult[i++];
            m_psPara.strNameField = strResult[i++];
            m_psPara.strCodeLibTable = strResult[i++];
            m_psPara.strCodeDataTable = strResult[i++];
            m_psPara.strCodeNorm = strResult[i++];
            m_strStdName = strResult[i++];

        }

        public override bool Verify()
        {
            // ��ȡ��׼ID
            int nStdID = SysDbHelper.GetStandardID(this.m_strStdName);
            lyr = LayerReader.GetLayerByAliasName(m_psPara.strFtName, nStdID);
            if (lyr == null)
            {
                SendMessage(enumMessageType.VerifyError, string.Format("��׼�в�����ͼ�㡰{0}��", m_psPara.strFtName));
                return false;
            }

            // ��ȡʵ����ֵ��                
            Helper.StandardHelper helper = new Hy.Check.Rule.Helper.StandardHelper(this.m_QueryConnection);
            strFieldCode = FieldReader.GetNameByAliasName(m_psPara.strCodeField, lyr.ID);
            if (string.IsNullOrEmpty(strFieldCode))
            {
                SendMessage(enumMessageType.VerifyError, string.Format("��׼ͼ���в������ֶΡ�{0}��", m_psPara.strCodeField));
                return false;
            }

            strFieldCodeType = helper.GetLayerFieldType(strFieldCode, lyr.AttributeTableName);
            if (string.IsNullOrEmpty(strFieldCodeType))
            {
                SendMessage(enumMessageType.VerifyError, string.Format("����ͼ���в������ֶΡ�{0}��", m_psPara.strCodeField));
                return false;
            }

            if (m_psPara.strNameField != "")
            {
                strFieldName = FieldReader.GetNameByAliasName(m_psPara.strNameField, lyr.ID);
            }
            else
            {
                SendMessage(enumMessageType.VerifyError,"û�������ֶ�");
                return false;
            }

            return true;
        }

        private StandardLayer lyr;
        private string strFieldName = "";
        private string strFieldCode = "";
        private string strFieldCodeType;
        public override bool Check(ref List<Error> checkResult)
        {
            try
            {
                string strFeatAlias = m_psPara.strFtName;
                string strFieldCodeAlias = m_psPara.strCodeField;
                string strLibName = m_psPara.strCodeDataTable;
                string strSQL;
                string strFeat ;
                DataTable dt;
                string var = null;

                long nVersion = m_psPara.nVersionID;

                var = lyr.AttributeTableName;
                string pLayerName = var;
                strFeat = pLayerName;


                // �����б����
                List<Error> pResult = new List<Error>();
                checkResult = pResult;

                // 1�����е�һ�����������Բ����������ļ�¼�����������
                if (strFieldName == "")
                {
                    strSQL = "select distinct " + strFieldCode + " from " + strFeat + "";
                }
                else
                {
                    strSQL = "select distinct " + strFieldCode + "," + strFieldName + " from " + strFeat + "";
                }

                dt = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSQL);

                //strfieldcode �����ֶ��Ƿ�Ϊ�ַ����ͣ�
                bool bIsStrType = false;

                if (dt != null)
                {
                    // ����Ψһֵ
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr != null)
                        {
                            // ��ȡΨһֵ
                            string pCode = "";


                            DataRow dr1 = dr;

                            var = dr1[0].ToString();
                            pCode = var;

                            if (string.IsNullOrEmpty(var))
                            {
                                if (strFieldName == "")
                                {
                                    strSQL = "select OBJECTID,BSM from " + strFeat + " where (" + strFieldCode +
                                             " is Null or " + strFieldCode + "='')";
                                }
                                else
                                {
                                    var = dr1[1].ToString();

                                    if (var != "") //���var������Ϊstring��
                                    {
                                        string strName = var.ToString();

                                        strSQL = "select OBJECTID,BSM from " + strFeat + " where (" + strFieldCode +
                                                 " is null or " + strFieldCode + " ='' ) and " + strFieldName + " = '" +
                                                 strName + "'";
                                    }
                                    else if (var == null || var == "")
                                    {
                                        strSQL = "select OBJECTID,BSM from " + strFeat + " where (" + strFieldCode +
                                                 " is Null or " + strFieldCode + " ='' ) and (" +
                                                 strFieldName + " is null or " + strFieldName + " ='' )";
                                    }
                                }
                                DataTable pDt = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSQL);

                                if (pDt != null)
                                {
                                    // ������¼�������ش���
                                    foreach (DataRow datarow in pDt.Rows)
                                    {
                                        if (datarow != null)
                                        {
                                            // ��ȡOID
                                            //object varOID = datarow[0];

                                            // ���ɴ���ṹ
                                            Error pInfo = new Error();
                                            pInfo.DefectLevel = this.m_DefectLevel;
                                            pInfo.RuleID = this.InstanceID;

                                            pInfo.OID = Convert.ToInt32(datarow["OBJECTID"]);
                                            pInfo.BSM = datarow["BSM"].ToString();
                                            pInfo.LayerName = strFeatAlias;

                                            //if (strFieldName == "")
                                            //{
                                            //    //pInfo.strErrInfo = "��ͼ����ֶ�" + strFieldCodeAlias + "(" + strFieldCode +
                                            //    //                   ")ֵΪ�գ�";
                                            //    //pInfo.strErrInfo = string.Format(Helper.ErrMsgFormat.ERR_4201_1, strFeatAlias, pInfo.BSM, strFieldCodeAlias);
                                                pInfo.Description = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}'�ֶε�ֵ����ȷ������Ϊ��", strFeatAlias, pInfo.BSM, strFieldCodeAlias);
                                            //}

                                            pResult.Add(pInfo);
                                        }
                                    }

                                    pDt.Dispose();
                                }
                            }
                            else
                            {
                                bIsStrType = true;

                                pCode = var;
                            }

                            string strName1 = "";
                            if (strFieldName == "")
                            {
                                strSQL = "select ���� from " + strLibName + " where �淶�� = " + nVersion + " and ���� = '" +
                                         pCode +
                                         "'";
                            }
                            else
                            {
                                DataRow dr3 = dr;

                                var = dr3[1].ToString();

                                string pName = var;
                                strName1 = pName;
                                strSQL = "select ����,���� from " + strLibName + " where �淶�� = " + nVersion + " and ���� = '" +
                                         pCode +
                                         "' and ����='" + strName1 + "'";
                            }

                            // ���ֵ��в������Ψһֵ, ����Ҳ�������ô�ֶ�����Ϊ���ֵ�����м�¼�������ҳ�����������OID
                            DataTable ipRSCode = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(SysDbHelper.GetSysDbConnection(), strSQL);


                            if (ipRSCode == null || ipRSCode.Rows.Count == 0)
                            {
                                // �����ֶ�����Ϊ���Ψһֵ�����м�¼
                                if (strFieldName == "")
                                {
                                    if (bIsStrType)
                                    {
                                        if (strFieldCodeType == "System.Int32" || strFieldCodeType == "System.Int64" ||
                                            strFieldCodeType == "System.Double")
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " + strFieldCode +
                                                     " = " +
                                                     pCode +
                                                     "";
                                        else if (strFieldCodeType == "System.String" || strFieldCodeType == "System.Char")
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " +
                                                     strFieldCode + " = '" +
                                                     pCode +
                                                     "'";
                                        else
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " +
                                                     strFieldCode + " = '" +
                                                     pCode +
                                                     "'";
                                    }
                                    else
                                    {
                                        if (pCode.Length > 0)
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " + strFieldCode +
                                                     "= " +
                                                     pCode +
                                                     "";
                                        else
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " + strFieldCode +
                                                     " is null ";
                                    }
                                }
                                else
                                {
                                    if (bIsStrType)
                                        strSQL = "select OBJECTID,BSM from " + strFeat + " where " + strFieldCode +
                                                 "= '" +
                                                 pCode +
                                                 "' and " +
                                                 strFieldName + " = '" + strName1 + "'";
                                    else
                                    {
                                        if (pCode.Length > 0)
                                            strSQL = "select OBJECTID,BSM from '" + strFeat + "' where '" + strFieldCode +
                                                     "'= " +
                                                     pCode +
                                                     "and '" +
                                                     strFieldName + "'='" + strName1 + "'";
                                        else
                                            strSQL = "select OBJECTID,BSM from " + strFeat + " where " + strFieldCode +
                                                     " is null and " +
                                                     strFieldName + " = '" + strName1 + "'";
                                    }
                                }

                                DataTable pDt = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSQL);

                                // ������¼�������ش���
                                foreach (DataRow pDr in pDt.Rows)
                                {
                                    if (pDr != null)
                                    {
                                        if (pCode == "")
                                        {
                                            continue;
                                        }

                                        // ���ɴ���ṹ
                                        Error pInfo = new Error();
                                        pInfo.DefectLevel = this.m_DefectLevel;
                                        pInfo.RuleID = this.InstanceID;
                                        pInfo.LayerName = strFeatAlias;

                                        // ��ȡOID
                                        pInfo.OID = Convert.ToInt32(pDr["OBJECTID"]);

                                        pInfo.BSM = pDr["BSM"].ToString();


                                        if (strFieldName == "")
                                        {
                                            //pInfo.strErrInfo = "��ͼ����ֶ�" + strFieldCodeAlias + "(" + strFieldCode + ")����" +
                                            //                   pCode + "�ڱ�׼���в����ڣ�";
                                            //pInfo.strErrInfo = string.Format(Helper.ErrMsgFormat.ERR_RuleCode_1_1, strFeatAlias,pInfo.BSM, strFieldCodeAlias, strFieldCode, pCode);
                                            pInfo.Description = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}({3})'�ֶε�ֵ'{4}'�ڱ�׼�����в�����", strFeatAlias, pInfo.BSM, strFieldCodeAlias, strFieldCode, pCode);
                                        }
                                        else
                                        {
                                            pInfo.Description = pCode;
                                            pInfo.Description += "|";
                                            pInfo.Description += strName1;
                                        }

                                        pResult.Add(pInfo);
                                    }
                                }
                                if (pDt != null)
                                {
                                    pDt.Dispose();
                                }
                            }
                        }
                    }
                    dt.Dispose();
                } // ����Ψһֵ����


                // ���ֻ���б���ļ�飬�ǲ���Ҫ���еڶ�����
                if (strFieldName == "")
                {
                    return true;
                }

                int nCount = pResult.Count;

                // 2�����еڶ�����������������ڵ����óɲ�ƥ�䣬���벻���ڵ����óɴ��벻����
                for (int i = 0; i < nCount; i++)
                {
                    Error pInfo = pResult[i];


                    string[] strArray = pInfo.Description.Split('|');

                    string strCode = "";
                    string strName = "";
                    if (strArray.Length == 2)
                    {
                        strCode = strArray[0];
                        strName = strArray[1];
                    }

                    strSQL = "select ���� from " + strLibName + " where �淶�� = " + nVersion + " and ���� = '" + strCode + "'";

                    // ���ֵ��в�������룬�ҵ�˵�����������Ǵ�������Ʋ�ƥ�䣬��֮�Ǵ��벻����
                    DataTable ipRSCode = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(SysDbHelper.GetSysDbConnection(), strSQL);

                    if (ipRSCode != null && ipRSCode.Rows.Count != 0)
                    {
                        //pInfo.strErrInfo = strFieldCodeAlias + "'" + strCode + "'��" + m_psPara.strNameField + "'" + strAliasName +
                        //                   "'��ƥ�䣡";
                        //pInfo.strErrInfo = string.Format(Helper.ErrMsgFormat.ERR_RuleCode_2, strFeatAlias, pInfo.BSM, strFieldCodeAlias, strCode, m_psPara.strNameField, strAliasName);
                        pInfo.Description = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}'��ֵ'{3}'��'{4}'��ֵ'{5}'��ƥ��", strFeatAlias, pInfo.BSM, strFieldCodeAlias, strCode, m_psPara.strNameField, strName);
                    }
                    else
                    {
                        //pInfo.strErrInfo = strFieldCodeAlias + "'" + strCode + "'�ڱ�׼���в����ڣ�";
                        //pInfo.strErrInfo = string.Format(Helper.ErrMsgFormat.ERR_RuleCode_1_2, strFeatAlias,pInfo.BSM ,strFieldCodeAlias, strCode);
                        pInfo.Description = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}({3})'�ֶε�ֵ'{4}'�ڱ�׼�����в�����", strFeatAlias, pInfo.BSM, strFieldCodeAlias, strFieldCode, strCode);
                    }
                    if (pInfo.Description == null)
                    {
                        pInfo.Description = "";
                    }
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
        public RuleExpression.LRCodePara m_psPara;

        private string m_strStdName;
    }
}