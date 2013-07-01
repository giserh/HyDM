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
    public class RuleFtCode : BaseRule
    {

        //�����ṹ��
        public RuleExpression.LRFtCodePara m_psPara;

        public override string Name
        {
            get { return "Ҫ���������"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));

            m_psPara = new RuleExpression.LRFtCodePara();
            pParameter.BaseStream.Position = 0;
            int nCount1 = pParameter.ReadInt32();

            //�����ַ���
            Byte[] bb = new byte[nCount1];
            pParameter.Read(bb, 0, nCount1);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_psPara.strName = strResult[i++];
            m_psPara.strAlias = strResult[i++];
            m_psPara.strRemark = strResult[i++];
            m_psPara.strCodeField = strResult[i++];
            m_psPara.strTargetLayer = strResult[i++];

        }

        public override bool Verify()
        {
            standarID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);

            StandardLayer lyr = LayerReader.GetLayerByAliasName(m_psPara.strTargetLayer, standarID);
            if (lyr == null)
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ڵı�׼���Ҳ�����Ϊ��" + m_psPara.strTargetLayer + "����ͼ��");
                return false;
            }

            strLayerName = lyr.Name;
            strCodeField = FieldReader.GetNameByAliasName(m_psPara.strCodeField, lyr.ID);
            if (string.IsNullOrEmpty(strCodeField))
            {
                SendMessage(enumMessageType.VerifyError, string.Format("��ǰ�������ڵı�׼���Ҳ�����Ϊ��{0}����ͼ��", m_psPara.strCodeField));
                return false;
            }

            return true;
        }
        private string strLayerName;
        private string strCodeField;
        private int standarID;
        public override bool Check(ref List<Error> checkResult)
        {
            try
            {
                List<Error> m_pRuleResult = new List<Error>();

            List<string> aryFtCode = new List<string>();
            Helper.StandardHelper StdHelp = new Check.Rule.Helper.StandardHelper(SysDbHelper.GetSysDbConnection());
            StdHelp.GetLayerCodes(ref aryFtCode, m_psPara.strTargetLayer, standarID);


            if (aryFtCode == null) //�����������Ϊ��
            {
                string strSql = "select OBJECTID,BSM from " + strLayerName;

                DataTable ipRecordset = new DataTable();
                ipRecordset = Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql);
                if (ipRecordset == null)
                {
                    return false;
                }

                // ��DataTable�л�ȡ����
                foreach (DataRow dr in ipRecordset.Rows) // ���������
                {
                    if (dr != null)
                    {
                        int nOID = Convert.ToInt32(dr["ObjectID"]);

                        // ��ҽ����¼
                        Error pResInfo = new Error();
                        pResInfo.DefectLevel = this.m_DefectLevel;
                        pResInfo.RuleID = this.InstanceID;

                        pResInfo.OID = nOID;
                        pResInfo.BSM = dr["BSM"].ToString();
                        pResInfo.LayerName = m_psPara.strTargetLayer;                            // Ŀ��ͼ��

                        // ������Ϣ
                        string strMsg;
                        strMsg = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}'�ֶζ�Ӧ��Ҫ�����ʹ���Ϊ��", pResInfo.LayerName, pResInfo.BSM, strCodeField);
                        if (m_psPara.strRemark != null && m_psPara.strRemark.Trim() != "")
                        {
                            pResInfo.Description = m_psPara.strRemark;
                        }
                        else
                        {
                            pResInfo.Description = strMsg;
                        }
                        m_pRuleResult.Add(pResInfo);

                        break;
                    }
                }

                checkResult = m_pRuleResult;

                // �رռ�¼��
                ipRecordset.Dispose();
            }
            else
            {
                try
                {
                    string strSql;
                    string strFtCode = "";
                    for (int i = 0; i < aryFtCode.Count; i++)
                    {
                        string strTmp;
                        strTmp = aryFtCode[i];
                        strFtCode += strTmp;
                    }

                    strSql = "select OBJECTID,BSM,YSDM from " + strLayerName + " where (" + strCodeField + " not in ('" +
                             strFtCode.Substring(0, strFtCode.Length) + "')) or (" + strCodeField + " is null )";

                    DataTable ipRecordset = new DataTable();
                    ipRecordset = Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql);
                    if (ipRecordset == null)
                    {
                        return false;
                    }

                    // ��DataTable�л�ȡ����
                    foreach (DataRow dr in ipRecordset.Rows) // ���������
                    {
                        if (dr != null)
                        {

                            int nOID = Convert.ToInt32(dr["ObjectID"]);

                            // ��ҽ����¼
                            Error pResInfo = new Error();
                            pResInfo.DefectLevel = this.m_DefectLevel;
                            pResInfo.RuleID = this.InstanceID;

                            pResInfo.OID = nOID;
                            pResInfo.BSM = dr["BSM"].ToString();
                            pResInfo.LayerName = m_psPara.strTargetLayer;                            // Ŀ��ͼ��

                            // ������Ϣ
                            string strMsg;
                            strMsg = string.Format("'{0}'���ʶ��Ϊ'{1}'��'{2}({3})'�ֶε�ֵ'{4}'����ȷ��ӦΪ��{5}", pResInfo.LayerName, pResInfo.BSM, m_psPara.strCodeField, strCodeField, dr["YSDM"], strFtCode);
                            if (m_psPara.strRemark != null && !string.IsNullOrEmpty(m_psPara.strRemark.Trim()))
                            {
                                pResInfo.Description = m_psPara.strRemark;
                            }
                            else
                            {
                                pResInfo.Description = strMsg;
                            }
                            m_pRuleResult.Add(pResInfo);

                        }
                    }

                    checkResult = m_pRuleResult;

                    // �رռ�¼��
                    ipRecordset.Dispose();
                }
                catch (Exception ex)
                {
                    SendMessage(enumMessageType.Exception, ex.ToString());
                    return false;
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



    }
}