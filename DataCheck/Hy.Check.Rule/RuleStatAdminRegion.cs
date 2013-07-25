using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Hy.Check.Rule;
using Hy.Check.Utility;
using Hy.Common.Utility.Data;
using Hy.Check.Define;

namespace Rule
{

    public class RuleStatAdminRegion : BaseRule
    {

        //���ṹ��
        private REGIONPARA m_structPara = new REGIONPARA();

        private string m_strName;
        //���ݱ���ȡͼ����
        private string FatherLayerName = "";
        private string ChildLayerName = "";

        public RuleStatAdminRegion()
        {
            m_strName = "ͳ�ƶԱ�_������������Ա��ʼ����";
            m_structPara.strChildFtName = "";
            m_structPara.strFatherFtName = "";
        }

        private bool CheckbyAdo(ref List<Hy.Check.Define.Error> checkResult)
        {
            DataTable ipRecordset = new DataTable();

            try
            {
                //���ݼ���ȡ��Ӧ�����е������
                string strSql = "";
                strSql = "SELECT " + m_structPara.strCodeField + ",Shape_Area FROM " + FatherLayerName + "";

                //�򿪼�¼��
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

                       string IDName = dr[0].ToString();
                       double dbCalArea = Convert.ToDouble(dr[1]);

                        //����ͼ��������Ӷ�ֵ���磺��������
                        DataTable ipRecordsetRes = new DataTable();

                        //��������������ָ���������ͳ��
                        string strSql1 = "Select SUM(" + m_structPara.strCompareField + ") FROM " + ChildLayerName +
                                         " Where " + m_structPara.strOwnerField + "='" +IDName + "'";

                        ipRecordsetRes = AdoDbHelper.GetDataTable(this.m_QueryConnection, strSql1);
                        //���ֶα��¼��
                        if (ipRecordsetRes==null)
                        {
                            continue;
                        }

                        foreach (DataRow dr1 in ipRecordsetRes.Rows)
                        {

                            Error res = new Error();

                            double dbSurveyArea = Convert.ToDouble(dr1[0]);
                            double dbError = dbCalArea - dbSurveyArea;
                            res.LayerName = FatherLayerName;
                            res.ReferLayerName = ChildLayerName;

                            if (Math.Round(Math.Abs(dbError), 2) > m_structPara.dbThreshold)
                            {
                                res.Description = "ABS(�������:" + Math.Round(dbCalArea, 2) + "-�������:" +
                                                 dbSurveyArea.ToString("F2") + ")=" +
                                                 Math.Abs(dbError).ToString("F2") +
                                                 ",�����趨����ֵ" + m_structPara.dbThreshold + "";
                                checkResult.Add(res);
                            }
                        }
                        ipRecordsetRes.Dispose();
                    }
                }
            }
            catch
            {
                return false;
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
            m_structPara.strAlias = strResult[i++];
            m_structPara.strRemark = strResult[i++];
            m_structPara.strFatherFtName = strResult[i++];
            m_structPara.strChildFtName = strResult[i++];
            m_structPara.strCodeField = strResult[i++];
            m_structPara.strOwnerField = strResult[i++];
            m_structPara.strCompareField = strResult[i];


            //��ֵ
            m_structPara.dbThreshold = pParameter.ReadDouble();

            return;
        }

        public override bool Verify()
        {
            //���ݱ���ȡfeatureclass������
            int standardID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            FatherLayerName = LayerReader.GetNameByAliasName(m_structPara.strFatherFtName, standardID);
            ChildLayerName = LayerReader.GetNameByAliasName(m_structPara.strChildFtName, standardID);

            if (this.m_QueryConnection == null)
            {
                return false;
            }

            return true;
        }

        public override bool Check(ref List<Hy.Check.Define.Error> checkResult)
        {
            checkResult = new List<Error>();

            if (!CheckbyAdo(ref checkResult))
            {
                return false;
            }
            return true;
        }
    }

    public class REGIONPARA
    {
        public string strFatherFtName; //�����һ������
        public string strChildFtName; //�����һ������
        public string strCodeField; //��������������ֶ�
        public string strOwnerField; //������������������ֶΣ����¼����У�
        public string strCompareField; //����Ƚ��ֶ�
        public double dbThreshold; //��ֵ
       public string  strAlias;
        public string strRemark;
    }
}