using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Hy.Check.Rule;
using Common.Utility.Data;
using Hy.Check.Utility;
using Hy.Check.Define;


namespace Rule
{
    public class RuleSheet : BaseRule
    {
        //�����ṹ��
        private SHEETPARA m_structPara = new SHEETPARA();
        private string m_strName;
        private string layerName;

        public RuleSheet()
        {
            m_strName = "ͼ������Ա�ͳ�ƹ���";
        }

        public override string Name
        {
            get { return m_strName; }
        }

        public override Hy.Check.Define.IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            MemoryStream stream = new MemoryStream(objParamters);
            BinaryReader pParameter = new BinaryReader(stream);

            pParameter.BaseStream.Position = 0;

            // �ַ����ܳ���
            int nStrSize = pParameter.ReadInt32();

            //�����ַ���
            Byte[] bb = new byte[nStrSize];
            pParameter.Read(bb, 0, nStrSize);
            string para_str = Encoding.Default.GetString(bb);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
             m_structPara.strAlias= strResult[i++];
             m_structPara.strRemark= strResult[i++];
            m_structPara.strFtName = strResult[i++];
            m_structPara.strSheetField = strResult[i++];
            m_structPara.strExpression = strResult[i];

            //��ֵ
            m_structPara.dbThreshold = pParameter.ReadDouble();
            return;
        }

        public override bool Verify()
        {
            //���ݱ���ȡfeatureclass������
            int standardID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            layerName = LayerReader.GetNameByAliasName(m_structPara.strFtName, standardID);

            //�����ǰ���
            if (this.m_QueryConnection == null)
            {
                return false;
            }
            return true;
        }

        public override bool Check(ref List<Hy.Check.Define.Error> checkResult)
        {
            if (!CheckbyAdo(ref checkResult))
            {
                return false;
            }

            return true;
        }

        private bool CheckbyAdo(ref List<Hy.Check.Define.Error> checkResult)
        {
            DataTable ipRecordset = new DataTable();
            try
            {
                string strSql = "Select " + m_structPara.strSheetField + ",SUM(Shape_Area),SUM(" +
                                m_structPara.strExpression + "),SUM(Shape_Area-(" + m_structPara.strExpression + ")) From " +
                                layerName + " GROUP BY " + m_structPara.strSheetField + "";

                ipRecordset = AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql);

                if (ipRecordset == null)
                {
                    return false;
                }

                checkResult = new List<Hy.Check.Define.Error>();

                foreach (DataRow dr in ipRecordset.Rows) //���������
                {
                    if (dr != null)
                    {
                        Hy.Check.Define.Error res = new Hy.Check.Define.Error();
                         
                        //���ֵ
                         double dbError = Convert.ToDouble(dr[3]);
                        //�������
                        double dbCalArea = Convert.ToDouble(dr[1]);
                        //�������
                        double dbSurveyArea = Convert.ToDouble(dr[2]);

                        res.Description = "ABS(�������:" + Math.Round(dbCalArea, 2) + "-�������:" +
                                             dbSurveyArea.ToString("F2") + ")=" +
                                             Math.Abs(dbError).ToString("F2") +
                                             ",�����趨����ֵ" + m_structPara.dbThreshold + "";

                        checkResult.Add(res);
                        }
                }
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
            }
            finally
            {
                if (ipRecordset != null)
                {
                    ipRecordset.Dispose();
                }
            }
            return true;
        }
    }

    /// <summary>
    /// ͼ������ԱȲ�����
    /// </summary>
    public class SHEETPARA
    {
        public string strFtName; //����ͼ����
        public string strSheetField; //����ͼ���ֶ���
        public string strExpression; //�������������ʽ
        public double dbThreshold; //�ݲ���ֵ
        public string strAlias;
        public string strRemark;
    }
}