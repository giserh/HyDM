using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Hy.Check.Define;
using Common.Utility.Data;


namespace Hy.Check.Rule
{
    public class RulePlot : BaseRule
    {
        //�����ṹ��
        public PLOTPARA m_structPara = new PLOTPARA();
        private string m_strName;
        private List<RuleExpression.RESULT> m_arrResult = null;
        private string m_LayerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RulePlot"/> class.
        /// </summary>
        public RulePlot()
        {
            m_strName = "ͼ������Ա�ͳ�ƹ���";
            m_arrResult = new List<RuleExpression.RESULT>();
        }

        private bool CheckbyAdo(string strTableName)
        {
            DataTable ipRecordset = new DataTable();

            string strSql = "Select OBJECTID,BSM," + m_structPara.strExpression + ",TBMJ,TBMJ-(" +
                            m_structPara.strExpression + ") From " + strTableName + " WHERE ABS(TBMJ-(" +
                            m_structPara.strExpression + ")) > " + m_structPara.dbThreshold + "";

            ipRecordset=AdoDbHelper.GetDataTable(base.m_ResultConnection, strSql);

            if (ipRecordset==null || ipRecordset.Rows.Count==0)
            {
                SendMessage(enumMessageType.RuleError, string.Format("���ʧ�ܣ����{0}ִ��ʧ��",strSql));
                return false;
            }

            foreach (DataRow dr in ipRecordset.Rows)
            {
                if (dr != null)
                {
                    RuleExpression.RESULT res = new RuleExpression.RESULT();

                    res.dbError = Convert.ToDouble(dr[4]);

                    if (Math.Round(Math.Abs(res.dbError), 5) > 0.01)
                    {

                        res.nOID = Convert.ToInt32(dr[0]);
                        res.BSM = Convert.ToInt32(dr[1]);
                        res.dbCalArea = Convert.ToDouble(dr[2]);
                        res.dbSurveyArea = Convert.ToDouble(dr[3]);

                        //res.strErrInfo = "��ʶ��Ϊ{0}��ͼ�������(ͼ�ߵ������+���ǵ������+�۳��������+��״�������)֮��Ϊ" +
                        //                 Math.Abs(res.dbError).ToString("F2") +
                        //                 "ƽ����,�����趨����ֵ0.01ƽ����";
                        res.strErrInfo = string.Format("{0}��ʶ��Ϊ{1}��ͼ�����{2}��(ͼ�ߵ������+���ǵ������+�۳��������+��״�������){3}֮��Ϊ{4}ƽ����,Ӧ��0.01ƽ����", m_structPara.strFtName, res.BSM, res.dbSurveyArea, res.dbCalArea, Math.Abs(res.dbError).ToString("F2"));
                       
                        m_arrResult.Add(res);
                    }
                }
            }
            ipRecordset.Dispose();

            return true;
        }

        // ��ȡ�����
        private List<Error> GetResult()
        {
            List<Error> pResAttr = new List<Error>();

            for (int i = 0; i < m_arrResult.Count; i++)
            {

                // ��ҽ����¼
                Error pResInfo = new Error();
                pResInfo.DefectLevel = this.m_DefectLevel;
                pResInfo.RuleID = this.InstanceID;

                // OID
                pResInfo.OID = m_arrResult[i].nOID;
                pResInfo.BSM = m_arrResult[i].BSM.ToString();

                // Ŀ��ͼ��
                pResInfo.LayerName = m_structPara.strFtName;

                // ������Ϣ
                pResInfo.Description = m_arrResult[i].strErrInfo;

                pResAttr.Add(pResInfo);
            }

            return pResAttr;
        }

        public override string Name
        {
            get { return m_structPara.Alias; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            MemoryStream  stream=new MemoryStream(objParamters);
            BinaryReader reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0;

            // �ַ����ܳ���
            int nStrSize = reader.ReadInt32();

            //�����ַ���
            Byte[] bb = new byte[nStrSize];
            reader.Read(bb, 0, nStrSize);
            string para_str = Encoding.Default.GetString(bb);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_structPara.Alias = strResult[i++];
            m_structPara.Remark = strResult[i++];
            m_structPara.strFtName = strResult[i++];
            m_structPara.strExpression = strResult[i];

            //��ֵ
            m_structPara.dbThreshold = reader.ReadDouble();

            return;
        }

        public override bool Verify()
        {
            if (base.m_QueryConnection == null)
            {
                return false;
            }
            if (base.m_ResultConnection == null)
            {
                return false;
            }
            m_LayerName = base.GetLayerName(m_structPara.strFtName);
            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
           
            if (!CheckbyAdo(m_LayerName))
            {
                return false;
            }

            checkResult = GetResult();

            return true;
        }
    }

    //ͼ������ԱȲ�����
    public class PLOTPARA
    {
        public string Alias;
        public string Remark;
        public string strFtName; //����ͼ����
        public string strExpression; //�������������ʽ
        public double dbThreshold; //�ݲ���ֵ
    }
}