using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Hy.Check.Define;
using Hy.Common.Utility.Data;

namespace Hy.Check.Rule
{
    public class RulePlotClass : BaseRule
    {
        //�����ṹ��
        private PlotClassPara m_structPara = new PlotClassPara();
        private  string m_strName="";
        //���ݱ���ȡͼ����
        private string layerName = "";
        private List<RuleExpression.RESULT> m_arrResult = null;
        public RulePlotClass()
        {
            m_strName = "��������Ա�ͳ�ƹ���";
            m_arrResult = new List<RuleExpression.RESULT>();
        }

        private bool CheckbyAdo(string strTableName)
        {
            DataTable ipRecordset = new DataTable();

            //���ݼ���ȡ��Ӧ�����е������
            string strSql = "";
            string strWhere = "";
            if (m_structPara.strClass.CompareTo("һ������") == 0)
            {
                strSql = "SELECT DISTINCT(LEFT(" + m_structPara.strClassField + ",1)) FROM " + strTableName + "";
                strWhere = "LEFT(" + m_structPara.strClassField + ",1)";
            }
            else if (m_structPara.strClass.CompareTo("��������") == 0)
            {
                strSql = "SELECT DISTINCT(LEFT(" + m_structPara.strClassField + ",2)) FROM " + strTableName + "";
                strWhere = "LEFT(" + m_structPara.strClassField + ",2)";
            }
            else if (m_structPara.strClass.CompareTo("��������") == 0)
            {
                strSql = "SELECT DISTINCT(LEFT(" + m_structPara.strClassField + ",3)) FROM " + strTableName + "";
                strWhere = "LEFT(" + m_structPara.strClassField + ",3)";
            }
            else if (m_structPara.strClass.CompareTo("�ļ�����") == 0)
            {
                strSql = "SELECT DISTINCT(LEFT(" + m_structPara.strClassField + ",4)) FROM " + strTableName + "";
                strWhere = "LEFT(" + m_structPara.strClassField + ",4)";
            }
            //�򿪼�¼����������
            ipRecordset = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql);

            if (ipRecordset.Rows.Count==0)
            {
                return false;
            }

            foreach (DataRow dr in ipRecordset.Rows) //���������
            {
                if (dr != null)
                {
                    string strCode = dr[0].ToString();

                    DataTable ipRecordsetRes = new DataTable();

                    //��������Ͻ���ٲ�
                    string strSql1 = "Select SUM(Shape_Area),SUM(" + m_structPara.strExpression +
                                     "),SUM(Shape_Area-(" +
                                     m_structPara.strExpression + ")) FROM " + strTableName + " Where " + strWhere +
                                     "='" + strCode + "'";

                    //�򿪼�¼����������
                    ipRecordsetRes = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql1);

                    if (ipRecordsetRes.Rows.Count == 0)
                    {
                        continue;
                    }
                    foreach (DataRow dr1 in ipRecordsetRes.Rows)
                    {
                        RuleExpression.RESULT res = new RuleExpression.RESULT();

                        res.dbError = Convert.ToDouble(dr1[2]);

                        if (Math.Round(Math.Abs(res.dbError), 2) > m_structPara.dbThreshold)
                        {

                            res.dbCalArea = Convert.ToDouble(dr1[0]);
                            res.dbSurveyArea = Convert.ToDouble(dr1[1]);
                            res.IDName = strCode;

                            res.strErrInfo = "ABS(�������:" + Math.Round(res.dbCalArea, 2) + "-�������:" +
                                             res.dbSurveyArea.ToString("F2") + ")=" +
                                             Math.Abs(res.dbError).ToString("F2") +
                                             ",�����趨����ֵ" + m_structPara.dbThreshold + "";
                            
                            m_arrResult.Add(res);
                        }
                    }
                    ipRecordsetRes.Dispose();
                }
            }
            ipRecordset.Dispose();
            return true;
        }

        public override string Name
        {
            get { return m_structPara.AliasName; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            MemoryStream  stream=new MemoryStream(objParamters);
            BinaryReader pParameter = new BinaryReader(stream);

            pParameter.BaseStream.Position = 0;

            // �ַ����ܳ���
            int nStrSize = pParameter.ReadInt32();

            //�����ַ���
            Byte[] bb = new byte[nStrSize];
            pParameter.Read(bb, 0, nStrSize);
            string para_str = Encoding.Default.GetString(objParamters);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_structPara.AliasName = strResult[i++];
            m_structPara.Remark = strResult[i++];
            m_structPara.strFtName = strResult[i++];
            m_structPara.strClassField = strResult[i++];
            m_structPara.strExpression = strResult[i++];
            m_structPara.strClass = strResult[i];

            //��ֵ
            m_structPara.dbThreshold = pParameter.ReadDouble();

            return;
        }

        public override bool Verify()
        {
            if (base.m_QueryConnection == null)
            {
                return false;
            }
            if (base.m_ResultConnection== null) return false;

            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {

            layerName = base.GetLayerName(m_structPara.AliasName);

            if (!CheckbyAdo(layerName))
            {
                return false;
            }
            if (!SaveResult(m_structPara.strFtName))
            {
                return false;
            }
            return true;
        }

        // ������,��Ҫ����CRuleDistrict,CRulePlotClass,CRuleSheet,CRuleStatAdminRegion
        public bool SaveResult(string strTargetFc)
        {
            try
            {
                string strSql = "delete * from LR_ResAutoStat_PlotClass where RuleInstID='" + base.m_InstanceID + "'";
               Hy.Common.Utility.Data.AdoDbHelper.ExecuteSql(base.m_ResultConnection, strSql);
                //------------------------------------------------//
                //				�ڽ�����д洢���				  //														
                //------------------------------------------------//
                DataTable ipRecordset = new DataTable();

                if (! AdoDbHelper.OpenTable("LR_ResAutoStat_PlotClass", ref ipRecordset, base.m_ResultConnection))
                {
                    return false;
                }

                for (int i = 0; i < m_arrResult.Count; i++)
                {
                    RuleExpression.RESULT res = m_arrResult[i];
                    //------�����ֶ�����
                    DataRow dr = ipRecordset.NewRow();
                    dr["RuleInstID"] = base.m_InstanceID;
                    dr["Ŀ��ͼ��"] = strTargetFc;
                    dr["ͳ����������"] = res.IDName;
                    dr["�������"] = res.dbCalArea;
                    dr["�������"] = res.dbSurveyArea;
                    string strErr = "" + Math.Abs(res.dbError / res.dbCalArea) * 100 + "";
                    dr["���(�ٷֱ�)"] = strErr;
                    dr["������Ϣ"] = res.strErrInfo;

                    //------����¼�¼
                    ipRecordset.Rows.Add(dr);

                    //���¼�¼
                    ipRecordset.AcceptChanges();
                }

                AdoDbHelper.UpdateTable("LR_ResAutoStat_PlotClass", ipRecordset, base.m_ResultConnection);

                //�رռ�¼��
                ipRecordset.Dispose();
            }
            catch (Exception ex)
            {
                //Hy.Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                //��ʾ������Ϣ
                //XtraMessageBox.Show("CRulePlot::SaveResult():" + ex.Message + "");
                return false;
            }

            return true;
        }

    }

    /// <summary>
    ///  ��������ԱȲ�����
    /// </summary>
    public class PlotClassPara
    {
        public string AliasName;
        public string Remark;
        public string strFtName; //����ͼ����
        public string strExpression; //�������������ʽ
        public string strClassField; //��������ֶ���
        public double dbThreshold; //�ݲ���ֵ
        public string strClass; //ͳ�Ƽ���(һ�����࣬�������࣬�������࣬�ļ����ࣩ
    }
}