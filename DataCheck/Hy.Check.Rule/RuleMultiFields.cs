using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Hy.Check.Define;
using System.Runtime.InteropServices;
using Common.Utility.Data;
using System.Data.OleDb;
using Hy.Check.Rule.Helper;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    /// <summary>
    /// ���ֶι�ϵ�ʼ������
    /// </summary>
    public class RuleMultiFields : BaseRule
    {
        //����Ĺ���������Ϣ���磺���˹��򣭻���ũ�����Ҫ�ز��ܵ��ǵ�
        //private string m_strDesc;

        //��ѯ�����ṹ��
        private MULTIFIELDSPARA m_structPara = new MULTIFIELDSPARA();

        private string FtName1 = "";
        private string FtName2 = "";

        private IFeatureWorkspace m_ipFtWS;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleMultiFields"/> class.
        /// </summary>
        public RuleMultiFields()
        {
            //��ղ�ѯ����
            m_structPara.strName = "���ֶι�ϵ�ʼ����";
        }

        /// <summary>
        ///  ��ȡSQL����ļ����
        /// </summary>
        /// <param name="pRecord"></param>
        /// <param name="bTable"></param>
        /// <returns></returns>
        private List<Error> GetResult(DataTable pRecord,bool bTable)
        {
            if (pRecord == null || pRecord.Rows.Count == 0) return null;

            string strErrIfo = ConstructErrorInfo();
            List<Error> pResAttr = new List<Error>();

            foreach (DataRow dr in pRecord.Rows)
            {
                // ��ҽ����¼
                Error pResInfo = new Error();
                pResInfo.DefectLevel = this.m_DefectLevel;
                pResInfo.RuleID = this.InstanceID;

                if (bTable)
                {
                    pResInfo.LayerName = COMMONCONST.TABLENAME;
                }
                else
                {
                    pResInfo.LayerName = FtName1;
                }

                pResInfo.ReferLayerName = FtName2;
                //pResInfo.strErrInfo = strErrIfo;

                if(dr.Table.Columns.Contains("ObjectID"))
                {
                    pResInfo.OID = Convert.ToInt32(dr["ObjectID"]);

                }

                if(dr.Table.Columns.Contains("BSM"))
                {
                    pResInfo.BSM =dr["BSM"].ToString();
                }

                if (m_structPara.strScript == "�������������������������������Ʋ�ƥ��")
                {
                    //pResInfo.strErrInfo = "����������Ϊ'" + dr["XZQDM"].ToString() + "',����������Ϊ'" + dr["XZQMC"].ToString() + "',��Ȩ����λ�����������������Ϊ'" + dr["qsdwmc"].ToString() + "'";
                    pResInfo.Description = string.Format(Helper.ErrMsgFormat.ERR_4201_5_2, pResInfo.LayerName,pResInfo.BSM,"����������" ,dr["XZQDM"].ToString(), "Ȩ����λ�����");
                }
                else if (m_structPara.strScript == "���������е�������������Ȩ�������һ��")
                {
                    pResInfo.Description = string.Format("��������ObjectIDΪ{0}������������({1})����ȷ����ȷֵ�μ���Ȩ�������", dr[0], dr[1]);
                }
                else if (m_structPara.strAlias == "�۳�����������" || m_structPara.strAlias == "����ũ��ͼ�߿۳�����������")
                {
                    //pResInfo.strErrInfo = m_structPara.strScript + ",�������" + Convert.ToDouble(dr["diff"]).ToString("F2") + "ƽ����";
                    pResInfo.Description = string.Format("��ʶ��Ϊ{0}�Ŀ۳����������'��{2}���-��״�������-���ǵ��������*�۳�����ϵ��'�������һ�£��������{1}", dr[1].ToString(), dr[2].ToString(), FtName1);
                    //pResInfo.strErrInfo = string.Format("{0}{1}��{2}{3}��һ��,�������{4}", dr.Table.Columns[2].ColumnName, Str2Double(dr[2]).ToString("F2"), dr.Table.Columns[3].ColumnName, Str2Double(dr[3]).ToString("F2"), Str2Double(dr["diff"]).ToString("f2"));
                }
                else if (m_structPara.strScript == "��״������е�Ȩ����λ����1��Ȩ����λ����2��Ȩ����λ�����һ��")
                {
                    string qsdwdm1 = dr[2].ToString();
                    string qsdwdm2 = dr[3].ToString();
                    if (string.IsNullOrEmpty(qsdwdm1))
                    {
                        pResInfo.Description = "Ȩ����λ����2��Ȩ����λ�����һ��";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(qsdwdm2))
                        {
                            pResInfo.Description = "Ȩ����λ����1��Ȩ����λ�����һ��";
                        }
                        else
                        {
                            pResInfo.Description = "Ȩ����λ����1��Ȩ����λ����2��Ȩ����λ�����һ��";
                        }
                    }
                }
                else if (m_structPara.strAlias.Contains("һ���Լ��") && m_structPara.strAlias.Contains("����"))
                {
                    string table_name = m_structPara.strScript.Substring(0, m_structPara.strScript.IndexOf("����"));
                    string xzqdm = dr[0].ToString();
                    //pResInfo.strErrInfo =string.Format("{0},����������Ϊ{1},��ֵΪ{2}����",m_structPara.strScript ,dr[0].ToString(),Convert.ToDouble(dr[1]).ToString("F2"));
                    if (xzqdm.Length == 12)
                    {
                        pResInfo.Description = string.Format("{0}����{1}��{2}({3})��{4}({5})��һ�£����߲�ֵ({6})", table_name, dr[0], dr.Table.Columns[2].ColumnName, Str2Double(dr[2]).ToString("f2"), dr.Table.Columns[3].ColumnName, Str2Double(dr[3]).ToString("f2"), Str2Double(dr[1]).ToString("f2"));
                    }
                    else
                    {
                        pResInfo.Description = string.Format("{0}����{2}({3})��{4}({5})��һ�£����߲�ֵ({6})", table_name, dr[0], dr.Table.Columns[2].ColumnName, Str2Double(dr[2]).ToString("f2"), dr.Table.Columns[3].ColumnName, Str2Double(dr[3]).ToString("f2"), Str2Double(dr[1]).ToString("f2"));
                    }
                }

                else if (m_structPara.strScript == "ũ������������״һ������������ܱ���ũ������������״��������������ܱ������һ��")
                {
                    foreach (DataColumn pColumn in pRecord.Columns)
                    {
                        if (pColumn.Caption.Equals("xzqdm") || (pColumn.Caption.Contains("����") == false)) continue;
                        double val = Convert.ToDouble(dr[pColumn]);
                        if (val > 0.0)
                        {
                           Error tempResInfo = new Error();
                           tempResInfo.DefectLevel = this.m_DefectLevel;
                           tempResInfo.RuleID = this.InstanceID;

                           tempResInfo.LayerName = pResInfo.LayerName;
                           tempResInfo.ReferLayerName = pResInfo.ReferLayerName;
                            tempResInfo.OID = pResInfo.OID;
                            tempResInfo.BSM = pResInfo.BSM;
                            //tempResInfo.strErrInfo = pColumn.Caption.Replace("����", "��һ��") + ",��ֵΪ" + val.ToString();
                            string col_name = pColumn.Caption.Replace("����", "");
                            string mj_dist = Str2Double(dr[col_name + "_DIST"]).ToString("f2");
                            string mj = Str2Double(dr[col_name]).ToString("f2");
                            tempResInfo.Description = string.Format("ũ������������״һ������������ܱ��{0}({1})��ũ������������״��������������ܱ��{0}({2})��һ�£����߲�ֵ({3})", col_name, mj_dist, mj, val.ToString());
                            pResAttr.Add(tempResInfo);
                            break;
                        }
                    }
                    return pResAttr;
                }
                else if (m_structPara.strScript == "ũ������������״һ�����������Ȩ�����ʻ��ܱ���ũ������������״��������������ܱ��������һ��" ||
                         m_structPara.strScript == "ũ������������״һ�����������Ȩ�����ʻ��ܱ���ũ������������״һ������������ܱ��������һ��" ||
                         m_structPara.strScript == "�����¶ȷּ�������ܱ���ũ������������״һ������������ܱ���������һ��")
                {
                    double douDiff = 0.00;
                    double.TryParse(dr[3].ToString(), out douDiff);//"�������"
                    if (Math.Round(douDiff, 3) > 0.001)
                    {
                        if (m_structPara.strScript == "�����¶ȷּ�������ܱ���ũ������������״һ������������ܱ���������һ��")
                        {
                            pResInfo.Description = string.Format("�����¶ȷּ�������ܱ��еĸ������({0})��ũ������������״һ������������ܱ��еĸ������({1})��һ�£����߲�ֵ{2}", Str2Double(dr[1]).ToString("f2"), Str2Double(dr[2]).ToString("f2"), douDiff);
                        }
                        else if (m_structPara.strScript == "ũ������������״һ�����������Ȩ�����ʻ��ܱ���ũ������������״��������������ܱ��������һ��")
                        {
                            pResInfo.Description = string.Format("ũ������������״һ�����������Ȩ�����ʻ��ܱ������({0})��ũ������������״��������������ܱ������({1})��һ�£����߲�ֵ({2})", Str2Double(dr[1]).ToString("f2"), Str2Double(dr[2]).ToString("f2"), douDiff);
                        }
                        else if (m_structPara.strScript == "ũ������������״һ�����������Ȩ�����ʻ��ܱ���ũ������������״һ������������ܱ��������һ��")
                        {
                            pResInfo.Description = string.Format("ũ������������״һ�����������Ȩ�����ʻ��ܱ������({0})��ũ������������״һ������������ܱ������({1})��һ�£����߲�ֵ({2})", Str2Double(dr[1]).ToString("f2"), Str2Double(dr[2]).ToString("f2"), douDiff);
                        }
                        else
                        {
                            pResInfo.Description = string.Format("{0},��������Ĳ���ֵΪ{1}", m_structPara.strScript, Math.Round(douDiff, 2).ToString());
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (m_structPara.strAlias == "����ͼ�ߺ���״���������ϵ������" || m_structPara.strAlias == "����ͼ�ߺ����ǵ��������ϵ������")
                {
                    double dDiff = Convert.ToDouble(dr["DIfferent"]);
                    if (dDiff < 0.1)
                    {
                        continue;
                    }
                    if (m_structPara.strScript.Trim() != "" && m_structPara.strScript != null)
                    {
                        pResInfo.Description = string.Format("{0}��������Ĳ���ֵΪ{1}", m_structPara.strScript, Math.Round(dDiff, 3).ToString());
                    }
                }
                else if (m_structPara.strAlias == "ͼ�߲��е�Ȩ����λ�����Ƿ���Ȩ�������һ�¼��" || m_structPara.strAlias == "ͼ�߲��е����䵥λ�����Ƿ���Ȩ�������һ�¼��")
                {
                    pResInfo.Description = string.Format("����ͼ�߲��б�ʶ��Ϊ{0}��ͼ�ߵ�Ȩ����λ����({0})��Ȩ����λ������е�Ȩ����λ���벻ƥ��", dr["bsm"], dr[3].ToString());
                }
                else
                {
                    if (m_structPara.strScript.Trim() != "" && m_structPara.strScript != null)
                    {
                        pResInfo.Description = m_structPara.strScript;
                    }
                }

                pResAttr.Add(pResInfo);
            }
            return pResAttr;
        }

        private string ConstructErrorInfo()
        {
            int layerid = -1;
            StandardHelper pStand = new StandardHelper(SysDbHelper.GetSysDbConnection());
            pStand.GetLayerIDByTableName(ref layerid, this.m_SchemaID, m_structPara.strFtName);

            string strSql1 = "Select FieldCode,FieldName From LR_DicField Where LayerID = " + layerid;

            List<FIELDMAP> arrFieldMap = new List<FIELDMAP>();
            DataTable dt = new DataTable();
            AdoDbHelper.GetDataTable(SysDbHelper.GetSysDbConnection(), strSql1);
            if (dt.Rows.Count==0)
            {
                return null;
            }
            foreach (DataRow dr in dt.Rows)
            {
                string FName = dr["FieldCode"].ToString(); //�ֶ���
                string FAlias = dr["FieldName"].ToString(); //�ֶα���

                FIELDMAP fMap = new FIELDMAP();
                fMap.strAlias = FAlias;
                fMap.strName = FName;
                arrFieldMap.Add(fMap);
            }

            /*string Alias = "", Alias2 = "";
            pStand.GetAliasByLayerName(ref Alias, m_structPara.strFtName, m_strStdName);
            pStand.GetAliasByLayerName(ref Alias2, m_structPara.strFtName2, m_strStdName);*/
            int standardID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            if (!string.IsNullOrEmpty(FtName1))
            {

                FtName1 = LayerReader.GetNameByAliasName(m_structPara.strFtName, standardID);
            }
            if (!string.IsNullOrEmpty(FtName2))
            {
                FtName2=LayerReader.GetNameByAliasName(m_structPara.strFtName, standardID);
            }
            string strErrInfo = m_structPara.strClause;

            for (int i = 0; i < arrFieldMap.Count; i++)
            {
                string strR = arrFieldMap[i].strAlias;
                string strS = arrFieldMap[i].strName;
                strErrInfo.Replace(strS, strR);
            }
            strErrInfo.Replace(m_structPara.strFtName, FtName1);
            strErrInfo.Replace(m_structPara.strFtName2, FtName2);
            strErrInfo = "�ֶ�ֵ���ϱ��ʽ (" + strErrInfo + ")";

            return strErrInfo;
        }


        /// <summary>
        /// ������Ŀ�ʼ�����Լ��
        /// </summary>
        /// <param name="pRecord"></param>
        /// <returns></returns>
        private List<Error> GetAttrResult(DataTable pRecord)
        {
            List<Error> pResAttr = new List<Error>();
            //string strZLDWDM = "���䵥λ����";
            //string strXZQDM = "����������";
            if(pRecord.Rows.Count>0)
            {
                DataRow dr = pRecord.Rows[0];
                DataColumnCollection dcCollection = pRecord.Columns;
                for (int i = 0; i < dcCollection.Count; i++)
                {
                    DataColumn dc = dcCollection[i];
                    string strColumnName = dc.ColumnName;
                    if (strColumnName.Trim().Equals( "zldwdm" ,StringComparison.OrdinalIgnoreCase)
                        || strColumnName.Trim().Equals("xzqdm", StringComparison.OrdinalIgnoreCase)
                        || strColumnName.Trim().Equals("szxzqdm", StringComparison.OrdinalIgnoreCase)
                        || (strColumnName.Contains("����")==false)
                        )
                    {
                       continue;
                    }
                    else if (strColumnName.Trim() == "�ؿ��������ͼ�߻����������")
                    {
                        double value = Math.Abs(Convert.ToDouble(dr[strColumnName]));
                        if (Math.Round(value, 2) > 10.00)
                        {
                            Error pResInfo = new Error();
                            pResInfo.DefectLevel = this.m_DefectLevel;
                            pResInfo.RuleID = this.InstanceID;

                            //string strCY = strColumnName.Replace("����", "");
                            pResInfo.LayerName = COMMONCONST.TABLENAME;
                            pResInfo.ReferLayerName = m_structPara.strFtName2;
                            //pResInfo.strErrInfo = strColumnName +"Ϊ"+ value.ToString("F2") + "ƽ����";
                            pResInfo.Description = string.Format(Helper.ErrMsgFormat.ERR_450101031, Str2Double(dr[0]).ToString("F2"), Str2Double(dr[1]).ToString("F2"), value.ToString("F2"));
                            pResAttr.Add(pResInfo);
                        }
                    }
                    else
                    {
                        double value = Convert.ToDouble(dr[strColumnName]);
                        if (Math.Round(value, 2) > 0.10)
                        {
                            // ��ҽ����¼
                            Error pResInfo = new Error();
                            pResInfo.DefectLevel = this.m_DefectLevel;
                            pResInfo.RuleID = this.InstanceID;

                            string strCY = strColumnName.Replace("����", "");
                            pResInfo.LayerName = COMMONCONST.TABLENAME;
                            pResInfo.ReferLayerName = m_structPara.strFtName2;
                            //pResInfo.strErrInfo = FtName1 + "��" + FtName2 + "��" + strColumnName + "����ȣ��������" + value.ToString("F2") + "����";
                            string tempColumnName = strCY + "_DIST";
                            if (!pRecord.Columns.Contains(tempColumnName))
                            {
                                continue;
                            }
                            string mj_dist = Str2Double(dr[tempColumnName]).ToString("f2");
                            string mj = Str2Double(dr[strCY]).ToString("f2");
                            pResInfo.Description = string.Format("{0}��{1}({2})�����ݿ�������({3})��һ�£����߲�ֵ({4})", FtName1, strCY, mj,mj_dist, value.ToString("F2"));
                            pResAttr.Add(pResInfo);

                        }
                    }
                }
            }

            return pResAttr;
        }

        private List<Error> GetTKXSResult(DataTable pRecord)
        {
            DataTable pDt = null;
            DataTable tkxs1Dt = null;
            try
            {
                List<Error> pResAttr = new List<Error>();
                string strZLDWDM = "���䵥λ����";
                string strXZQDM = "����������";
                if (pRecord.Rows.Count > 0)
                {
                    DataRow dr = pRecord.Rows[0];
                    DataColumnCollection dcCollection = pRecord.Columns;

                    string sqlStr = "select * from " + COMMONCONST.TB_DIST_TKXS;

                    AdoDbHelper.OpenTable(COMMONCONST.TB_DIST_TKXS,ref pDt, base.m_QueryConnection);

                    if (pDt == null || pDt.Rows.Count == 0) return null;

                    //��TKXS1���в�����Ӧ���ݣ�ȫ��tkxs�ļ��ϣ����ֶ�˳����DIST_TKXSһ��
                    sqlStr = "select XZQDM,XZQMC,T2DEGREE,T6DEGREE,T15DEGREE,T25DEGREE,P2DEGREE,P6DEGREE,P15DEGREE,P25DEGREE from " + COMMONCONST.TB_TKXS1 + " where XZQDM = '" + pDt.Rows[0]["XZQDM"].ToString() + "'";

                    tkxs1Dt = AdoDbHelper.GetDataTable(base.m_QueryConnection,sqlStr);

                    for (int i = 0; i < dcCollection.Count; i++)
                    {
                        DataColumn dc = dcCollection[i];
                        string strColumnName = dc.ColumnName;

                        double value;
                        if (!double.TryParse(dr[strColumnName].ToString(), out value) || 
                            strColumnName.Trim().Equals("xzqdm", StringComparison.OrdinalIgnoreCase) ||
                            strColumnName.Trim().Equals("xzqmc", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (Math.Round(value, 4) > 0.10)
                        {
                            bool flag = false;
                            int index = (i == 2 ? i : ((2 * i) + (2 * (i - 3))));
                            string[] tkxs1Values = tkxs1Dt.Rows[0][i].ToString().Split(',');
                            Hashtable tkxs1HsTable = new Hashtable();
                            foreach (string dnStr in tkxs1Values)
                            {
                                if (!tkxs1HsTable.ContainsKey(dnStr))
                                    tkxs1HsTable.Add(double.Parse(dnStr), null);
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                double tempValue;
                                double.TryParse(pDt.Rows[0][index + j].ToString(), out tempValue);
                                if (tempValue != 0 && !tkxs1HsTable.ContainsKey(tempValue))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                pRecord.Rows[0][i] = 0;
                                continue;
                            }
                            // ��ҽ����¼
                           Error  err = new Error();
                           err.DefectLevel = this.m_DefectLevel;
                           err.RuleID = this.InstanceID;

                           err.LayerName = COMMONCONST.TABLENAME;
                           err.ReferLayerName = m_structPara.strFtName2;
                            string str = strColumnName.Replace(COMMONCONST.TB_DIST_TKXS, "�￲ϵ��");
                            err.Description = "�������ȡ��'" + str + "'��ʡ���ϱ���'" + str + "'�����";
                            pResAttr.Add(err);
                        }
                    }
                }

                return pResAttr;
            }
            catch (Exception ex)
            {
                //Hy.Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                return null;
            }
            finally
            {
                if(pDt != null)
                {
                    pDt.Dispose();
                }
                if(tkxs1Dt!=null)
                {
                    tkxs1Dt.Dispose();
                }
            }
        }
        
        /// <summary>
        /// ��ȡ��Ҫ���⴦��Ĺ������
        /// </summary>
        /// <returns></returns>
        private Hashtable GetEnumHash()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("ũ������������״һ������������ܱ������ݿ�������һ���Լ��","");
            hashtable.Add("ũ������������״��������������ܱ������ݿ�������һ���Լ��", "");
            hashtable.Add("ũ������������״һ�����������Ȩ�����ʻ��ܱ������ݿ�������һ���Լ��", "");
            hashtable.Add("����ũ�����ͳ�Ʊ������ݿ�������һ���Լ��", "");
            hashtable.Add("�����¶ȷּ�������ܱ������ݿ�������һ���Լ��", "");

            hashtable.Add("����ͼ�߲�ͼ��������������������һ���Լ��","");

            hashtable.Add("ʡ���ϱ��￲ϵ�����ϱ��������￲ϵ���Ƿ�һ�¼��", "");                 //����
            //hashtable.Add("����ϵ�ʼ����", "");

            hashtable.Add("����ũ�ﲹ��������ܱ������ݿ�������һ���Լ��","");
            //hashtable.Add(" ����ũ�ﲹ��������ܱ����ؼ�����������缶�������֮��һ���Լ��", "");
            //hashtable.Add("����ũ�ﲹ��������ܱ����缶���������弶�������֮��һ���Լ��", "");
            //hashtable.Add("����ũ�ﲹ��������ܱ���С���������ֵ������֮��һ���Լ��", "");

            //���Ӻ����������һ��������ͳ�Ʊ���Ĺ���
            //jinjj  2010-11-09
            hashtable.Add("���������������״һ������������ܱ������ݿ�������һ���Լ��", "");
            hashtable.Add("���������������״��������������ܱ������ݿ�������һ���Լ��", "");
            hashtable.Add("��������������״һ������������ܱ������ݿ�������һ���Լ��", "");
            hashtable.Add("��������������״��������������ܱ������ݿ�������һ���Լ��", "");
            return hashtable;
        }

        private double Str2Double(object obj)
        {
            try
            {
                return Convert.ToDouble(obj);
            }
            catch
            {
                return 0.0;
            }
        }



        public override string Name
        {
            get { return m_structPara.strAlias; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(objParamters));
            reader.BaseStream.Position = 0;
            int nStrSize = reader.ReadInt32(); // �ַ����ܳ���

            //�����ַ���
            Byte[] bb = new byte[nStrSize];
            reader.Read(bb, 0, nStrSize);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();
            string[] strResult = para_str.Split('|');

            int i = 0;

            m_structPara.strAlias = strResult[i++];
            m_structPara.strScript = strResult[i++];
            m_structPara.strFtName = strResult[i++];
            m_structPara.strFtName2 = strResult[i++];
            if (strResult.Length == 8)
            {
                m_structPara.strWhereClause = strResult[6];
                m_structPara.strClause = strResult[7];
            }
            else
            {
                m_structPara.strWhereClause = strResult[i++];
                m_structPara.strClause = strResult[i];
            }
            //if (m_structPara.strAlias.Contains("һ���Լ��") && m_structPara.strAlias.Contains("����"))
            //{
            //}

            // �����ԣ����ݿ��б������ͼ�㱾��
            FtName1 = m_structPara.strFtName;
            FtName2 = m_structPara.strFtName2;
            return;
        }

        public override bool Verify()
        {
            if (m_structPara.strFtName == "" || m_structPara.strFtName2 == "" || m_structPara.strWhereClause == "")
            {
                //XtraMessageBox.Show("���Ŀ�������ʽ�����ڣ��޷�ִ�м�飡");
                string strLog = "��ǰ�������ݿ�ļ��Ŀ�������ʽ�����ڣ��޷�ִ�м��!";
                SendMessage(enumMessageType.VerifyError, strLog);
                return false;
            }
            if(base.m_QueryWorkspace is IFeatureWorkspace)
            {

                m_ipFtWS= (IFeatureWorkspace)base.m_QueryWorkspace;

                if(!(m_ipFtWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable,m_structPara.strFtName))
                {
                    //Hy.Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                    string strLog = "��ǰ�������ݿ�ļ��Ŀ��ͼ��" + FtName1 + "�����ڣ��޷�ִ�м��!";
                    SendMessage(enumMessageType.VerifyError, strLog);
                    return false;
                }

                if (!m_structPara.strAlias.Equals("������һ����") && 
                    !m_structPara.strAlias.Equals("ͼ�ߵ���һ����"))
                {
                    if(!(m_ipFtWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTTable,m_structPara.strFtName))
                    {
                        //Hy.Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                       string strLog = "��ǰ�������ݿ�ļ��Ŀ��ͼ��" + FtName2 + "�����ڣ��޷�ִ�м��!";
                        SendMessage(enumMessageType.VerifyError, strLog);
                        return false;
                    }
                }
            }
            else
            {
                string strLog = "��ǰ�������ݿ�Ĺ����ռ�" + base.m_QueryWorkspace.PathName + "�����ڣ��޷�ִ�м��!";
                SendMessage(enumMessageType.VerifyError, strLog);
                return false;
            }
        
            // �����ԣ����ݿ��б������ͼ�㱾��
            //FtName1=base.GetLayerName(m_structPara.strFtName);
            //FtName2=base.GetLayerName(m_structPara.strFtName2);

            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
            //System.Diagnostics.Stopwatch MyWatch = new System.Diagnostics.Stopwatch();
            //MyWatch.Start();
            System.Data.OleDb.OleDbDataReader reader = null;
            try
            {

                DataTable pRecordset = new DataTable();

                if (m_structPara.strScript == "�������������������������������Ʋ�ƥ��")
                {
                    string strTemp = "select objectid,BSM,xzqdm, xzqmc,qsdwmc from(select a2.objectid,a2.BSM,a2.xzqdm,a2.xzqmc, a1.qsdwdm,a1.qsdwmc from qsdmb a1 inner join xzq a2 on left(a1.qsdwdm,12)=left(a2.xzqdm,12) where mid(qsdwdm,12,1)<>'0'  and right(qsdwdm,7)='0000000') where Trim(a2.xzqmc)<>Trim(a1.qsdwmc)";
                    pRecordset = AdoDbHelper.GetDataTable(base.m_QueryConnection, strTemp);
                    if (pRecordset==null || pRecordset.Rows.Count==0)
                    {
                        string strLog = "��ǰ�����SQL��ѯ������������޷�ִ�ж���ϵ���!";
                        SendMessage(enumMessageType.RuleError, strLog);
                        return false;
                    }
                }
                else if (m_structPara.strScript.Contains("Ȩ�������һ��") ||
                           m_structPara.strScript.Contains("Ȩ����λ�����һ��"))
                {
                    reader = AdoDbHelper.GetQueryReader(base.m_QueryConnection, m_structPara.strClause) as OleDbDataReader;
                    if (reader == null)
                    {
                        string strLog = "��ǰ�����SQL��ѯ������������޷�ִ�ж���ϵ���!";
                        SendMessage(enumMessageType.RuleError, strLog);
                        return false;
                    }
                    pRecordset.Load(reader);
                }
                else
                {
                    pRecordset = AdoDbHelper.GetDataTable(base.m_QueryConnection, m_structPara.strClause);
                    if (pRecordset == null || pRecordset.Rows.Count==0)
                    {
                        string strLog = "��ǰ�����SQL��ѯ������������޷�ִ�ж���ϵ���!";
                        SendMessage(enumMessageType.RuleError, strLog);
                        return false;
                    }
                }

                string strSql = "";
                bool bTable = false;

                if (FtName1.Contains("��") || FtName2.Contains("��"))
                {
                    if (m_structPara.strFtName2.Equals("qsdmb", StringComparison.OrdinalIgnoreCase))
                    {
                        strSql = "update LR_ResultEntryRule set TargetFeatClass1= '" + FtName1 + "',TargetFeatClass2='" +
                             FtName2 + "|' where RuleInstID='" + base.m_InstanceID + "'";
                    }
                    else
                    {
                        strSql = "update LR_ResultEntryRule set TargetFeatClass1= '" + COMMONCONST.TABLENAME +
                             "',TargetFeatClass2='|' where RuleInstID='" + base.m_InstanceID + "'";
                        bTable = true;
                    }
                }
                else
                {
                    strSql = "update LR_ResultEntryRule set TargetFeatClass1= '" + FtName1 + "',TargetFeatClass2='" +
                             FtName2 + "|' where RuleInstID='" + base.m_InstanceID + "'";
                }
                AdoDbHelper.ExecuteSql(m_ResultConnection, strSql);

                //��ȡ��Ҫ���⴦��Ĺ������
                Hashtable hashtable = GetEnumHash();
                if (hashtable.Contains(m_structPara.strAlias))
                {
                    if (m_structPara.strAlias.Equals("ʡ���ϱ��￲ϵ�����ϱ��������￲ϵ���Ƿ�һ�¼��"))
                    {
                        checkResult = GetTKXSResult(pRecordset);
                    }
                    else
                    {
                        checkResult = GetAttrResult(pRecordset);
                    }
                }
                else
                {
                    checkResult = GetResult(pRecordset, bTable);
                }
                if (pRecordset != null)
                {
                    pRecordset.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.RuleError, string.Format("����ʧ�ܣ���Ϣ��{0}",ex.Message));
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader.Close();
                    GC.Collect();
                }
            }
            //MyWatch.Stop();
            //System.Windows.Forms.MessageBox.Show("ʱ��:" + MyWatch.ElapsedMilliseconds.ToString() + "����");
            return true;
        }
    }

    //���ֶ��ʼ�����ṹ��
    public struct MULTIFIELDSPARA
    {
        public string strName; //�ʼ�������
        public string strAlias; //��ѯ�������
        public string strScript; //����
        public string strFtName; //����ͼ��
        public string strFtName2; //����ͼ��2
        public string strWhereClause; //��ѯ����
        public List<string> strMLyrList; //ͼ���б�
        public string strClause;
    } ;
    /// <summary>
    /// �ֶ�ӳ��
    /// </summary>
    public struct FIELDMAP
    {
        public string strAlias; //�ֶα���
        public string strName; //�ֶ���
        public int fieldType; //�ֶ�����
        public int fieldLength; //�ֶγ���
    } ;
}