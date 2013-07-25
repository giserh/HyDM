using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Hy.Check.Utility
{
   
    /// <summary>
    /// ͳ����
    /// </summary>
    public class StateItem
    {
        private string checkType;

        /// <summary>
        /// �������
        /// </summary>
        public string CheckType
        {
            get { return checkType; }
            set { checkType = value; }
        }

        private int light;

        /// <summary>
        /// ��ȱ��
        /// </summary>
        public int Light
        {
            get { return light; }
            set { light = value; }
        }

        private int weight;

        /// <summary>
        /// ��ȱ��
        /// </summary>
        public int Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        private int graveness;

        /// <summary>
        /// ����ȱ��
        /// </summary>
        public int Graveness
        {
            get { return graveness; }
            set { graveness = value; }
        }


        /// <summary>
        /// ����
        /// </summary>
        public int MaxCount
        {
            get { return light + weight + graveness; }
        }
    }

    /// <summary>
    /// ͳ������
    /// </summary>
    public class StateAppraise
    {
        #region �ڲ���Ա

        private DataTable m_pTable = null;

        //private RuleRightWeight[] arrRuleRightWeight = null;
        private const double MARK_WEIGHT = 2;
        private const double MARK_LIGHT = 0.5;
        //private static int gravenessCoeff = 100; //����ȱ�ݷ���
        //private static int weightCoeff = 10; //��ȱ�ݷ���
        //private static int weightNumber = 3; //��ȱ�ݵ�������
        //private static int lightCoeff = 1; //��ȱ�ݷ���
        //private static int lightNumber = 9; //��ȱ�ݵ�������

        private Hashtable hashRuleWeight = new Hashtable();
        private Hashtable hashArcGisRule = new Hashtable();

        private OleDbConnection _resultConn;

        #endregion

        public StateAppraise()
        {
        }

        public OleDbConnection ResultConnection
        {
            set
            {
                _resultConn = value;
                //OleDbConnection oleconn = CCheckApplication.m_CurrentModelTask.
                if (_resultConn != null)
                {
                    try
                    {

                        // 2012-07-12 �ź���
                        // ֱ�Ӵӽ�����ȡ������¼�����ر�ResultTable���� 

                        this.ResultTable = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(_resultConn, @"SELECT
                            b.CheckType,b.RuleInstID,b.ArcGisRule ,a.TargetFeatClass1 as YSTC,
                            a.BSM as SourceBSM,'LR_ResAutoAttr' as SysTab,a.IsException from LR_ResAutoAttr as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                            union all

                            SELECT
                            b.CheckType,b.RuleInstID,b.ArcGisRule ,a.YSTC as YSTC,
                            a.SourceBSM as SourceBSM,'LR_ResAutoTopo' as SysTab,a.IsException from LR_ResAutoTopo as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                            union all

                            SELECT
                            b.CheckType,b.RuleInstID,b.ArcGisRule ,a.AttrTabName as YSTC,
                            '' as SourceBSM,'LR_ResIntField' as SysTab,a.IsException from LR_ResIntField as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                            union all

                            SELECT
                            b.CheckType,b.RuleInstID,b.ArcGisRule ,a.ErrorLayerName as YSTC,
                            '' as SourceBSM,'LR_ResIntLayer' as SysTab,a.IsException from LR_ResIntLayer as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID");


                        DataTable dt = null;
                        // = GT_CARTO.CommonAPI.ado_OpenTable() .ado_Execute(CommonAPI.Get_DBConnection(), sql);
                        string str = string.Format("select * from LR_ResultEntryRule where ArcGisRule<>''");
                        dt = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(_resultConn, str);
                        if (dt == null) return;
                        if (dt.Rows.Count < 1) return;
                        foreach (DataRow dr in dt.Rows)
                        {
                            string key = dr["ArcGisRule"].ToString();
                            if (key != "" && !hashArcGisRule.ContainsKey(key))
                                hashArcGisRule[key] = dr["RuleInstID"].ToString();
                        }
                    }
                    catch (Exception exp)
                    {
                        Hy.Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                        //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                    }
                }
            }
        }

        // 2012-07-12 �ź���
        // ֱ�Ӵӽ�����ȡ������¼�����ر�ResultTable����
        private DataTable ResultTable
        {
            set
            {
                m_pTable = value;
                if (m_pTable != null)
                {
                    if (hashRuleWeight.Count < 1) GetRuleRightWeight();

                    //�������˹����Ӧ��RuleInstID
                    int RuleIDColumnIndex = m_pTable.Columns.IndexOf("RuleInstID");
                    int ArcGISRuleColumnIndex = m_pTable.Columns.IndexOf("ArcGISRule");

                    DataRow[] rows = m_pTable.Select("CheckType like '%���˹�ϵ%'");
                    //DataTable dt = new DataTable();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        //string ruleid = rows[i][RuleIDColumnIndex].ToString();
                        string arcgisrule = rows[i][ArcGISRuleColumnIndex].ToString();
                        if (hashArcGisRule.Contains(arcgisrule)) //�Ѵ���
                        {
                            rows[i][RuleIDColumnIndex] = hashArcGisRule[arcgisrule];
                        }
                    }
                    m_pTable.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// ��ȡ���е�Ȩ����Ϣ�Ĺ�ϣ��
        /// </summary>
        /// <returns></returns>
        private void GetRuleRightWeight()
        {
            try
            {
                //DataTable dt = null; // = GT_CARTO.CommonAPI.ado_OpenTable() .ado_Execute(CommonAPI.Get_DBConnection(), sql);
                //GT_CARTO.CommonAPI.ado_OpenTable("LR_EvaHMWeight", ref dt, GT_CARTO.CommonAPI.Get_DBConnection());
                DataTable dt = Hy.Common.Utility.Data.AdoDbHelper.GetDataTable(SysDbHelper.GetSysDbConnection(), "select * from LR_EvaHMWeight");
                if (dt.Rows.Count < 1) return;
                foreach (DataRow dr in dt.Rows)
                {
                    string key = dr["ElementID"].ToString();
                    if (key != "" && !hashRuleWeight.ContainsKey(key))
                        hashRuleWeight[key] = dr["ErrType"].ToString();
                }
            }
            catch (Exception exp)
            {
                Hy.Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
            }
        }

        /// <summary>
        /// �����������ͽ����
        /// </summary>
        public DataTable BuilderRuleStyleTable()
        {
            DataTable resultTable = new DataTable("ResultCollect");
            resultTable.Columns.Add("�������", Type.GetType("System.String"));
            resultTable.Columns.Add("����ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("��ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("��ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("����ϼ�", Type.GetType("System.Int32"));
            resultTable.Columns.Add("����BSM", Type.GetType("System.String"));

            //��ͳ������
            DataRow[] rows = m_pTable.Select("isException=0");
            DataRow dr = null;
            DataRow pRow = null;
            //foreach (DataRow dr in m_pTable.Rows)
            for (int i = 0; i < rows.Length;i++ )
            {
                
                try
                {
                    dr = rows[i];
                    string checktype = dr["CheckType"].ToString();
                    string ruleid = dr["RuleInstID"].ToString();

                    DataRow[] drw = resultTable.Select(string.Format("�������='{0}'", checktype));
                    if (drw.Length > 0)
                    {
                        pRow = drw[0];
                    }
                    else
                    {
                        pRow = resultTable.NewRow();
                        pRow["�������"] = checktype;
                        pRow["����ϼ�"] = 0;
                        pRow["����ȱ��"] = 0;
                        pRow["��ȱ��"] = 0;
                        pRow["��ȱ��"] = 0;
                        pRow["����BSM"] = "";
                        resultTable.Rows.Add(pRow);
                    }

                    if ((!hashRuleWeight.ContainsKey(ruleid)) &&
                        !dr["SysTab"].ToString().Equals("LR_ManualCheckError",StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    //���ڱ�ʾ����ͬ������IDҲ��ͬ�ģ���ͬһ������
                    // 2012-07-26 �ź���
                    // ͬһ����ʶ����ͬһ�������´���ֻ��һ�η�
                    string srbsm = dr["SourceBSM"].ToString().Trim();
                    string tarbsm = pRow["����BSM"].ToString().Trim();
                    if (!srbsm.Equals(""))
                    {
                        srbsm = ruleid + "_" + srbsm;
                        if (tarbsm.IndexOf(srbsm) == -1)
                            pRow["����BSM"] = srbsm + "," + tarbsm;
                        else
                            continue;
                    }

                    string value = dr["SysTab"].ToString().Equals("LR_ManualCheckError") ? "��ȱ��" : hashRuleWeight[ruleid].ToString();
                    switch (value)
                    {
                        case "����ȱ��":
                            {
                                pRow["����ȱ��"] = (Int32)pRow["����ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32)pRow["����ϼ�"] + 1;
                            }
                            break;
                        case "��ȱ��":
                            {
                                pRow["��ȱ��"] = (Int32)pRow["��ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32)pRow["����ϼ�"] + 1;
                            }
                            break;
                        case "��ȱ��":
                            {
                                pRow["��ȱ��"] = (Int32)pRow["��ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32)pRow["����ϼ�"] + 1;
                            }
                            break;
                    }
                }
                catch (Exception exp)
                {
                    Hy.Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                    //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                }
            }
            resultTable.Columns.Remove("����BSM");
            return resultTable;
        }

        /// <summary>
        /// ����ͼ�������б�
        /// </summary>
        public DataTable BuilderLayerStyleTable()
        {
            DataTable resultTable = new DataTable("ResultCollect");
            resultTable.Columns.Add("ͼ��", Type.GetType("System.String"));
            resultTable.Columns.Add("����ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("��ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("��ȱ��", Type.GetType("System.Int32"));
            resultTable.Columns.Add("����ϼ�", Type.GetType("System.Int32"));
            //resultTable.Columns.Add("��¼����", Type.GetType("System.Int32"));
            resultTable.Columns.Add("����BSM", Type.GetType("System.String"));

            //��ͳ������
            DataRow[] rows = m_pTable.Select("isException=0");
            DataRow dr = null;
            //foreach (DataRow dr in m_pTable.Rows)
            for (int i = 0; i < rows.Length; i++)
            {
                try
                {
                    dr = rows[i];
                    string LayerName = dr["YSTC"].ToString();
                    if (LayerName.Equals("")) continue;

                    //�ٶ�̫��
                    //IFeatureLayer pFeatLayer = EngineAPI.GetLayerFromMapByName(m_Map, LayerName) as IFeatureLayer;
                    //if (pFeatLayer == null)
                    //{
                    //    continue;
                    //}

                    DataRow pRow = null;
                    DataRow[] drw = resultTable.Select(string.Format("ͼ��='{0}'", LayerName));
                    if (drw.Length > 0)
                    {
                        pRow = drw[0];
                    }
                    else
                    {
                        pRow = resultTable.NewRow();
                        pRow["ͼ��"] = LayerName;
                        pRow["����ϼ�"] = 0;
                        pRow["����ȱ��"] = 0;
                        pRow["��ȱ��"] = 0;
                        pRow["��ȱ��"] = 0;

                        //IFeatureLayer pFeatLayer = EngineAPI.GetLayerFromMapByName(m_Map, LayerName) as IFeatureLayer;
                        //IFeatureClass pFeatClass = pFeatLayer.FeatureClass;
                        //pRow["��¼����"] = pFeatClass.FeatureCount(null);
                        resultTable.Rows.Add(pRow);
                    }

                    string ruleid = dr["RuleInstID"].ToString();
                    if (!hashRuleWeight.ContainsKey(ruleid) && !dr["SysTab"].ToString().Equals("LR_ManualCheckError")) continue;

                    //���ڱ�ʾ����ͬ������IDҲ��ͬ�ģ���ͬһ������
                    string srbsm = dr["SourceBSM"].ToString().Trim();
                    string tarbsm = pRow["����BSM"].ToString().Trim();
                    if (!srbsm.Equals(""))
                    {
                        srbsm = ruleid + "_" + srbsm;
                        if (tarbsm.IndexOf(srbsm) == -1)
                            pRow["����BSM"] = srbsm + "," + tarbsm;
                        else
                            continue;
                    }

                    string value = dr["SysTab"].ToString().Equals("LR_ManualCheckError") ? "��ȱ��" : hashRuleWeight[ruleid].ToString();
                    switch (value)
                    {
                        case "����ȱ��":
                            {
                                pRow["����ȱ��"] = (Int32) pRow["����ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32) pRow["����ϼ�"] + 1;
                            }
                            break;
                        case "��ȱ��":
                            {
                                pRow["��ȱ��"] = (Int32) pRow["��ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32) pRow["����ϼ�"] + 1;
                            }
                            break;
                        case "��ȱ��":
                            {
                                pRow["��ȱ��"] = (Int32) pRow["��ȱ��"] + 1;
                                pRow["����ϼ�"] = (Int32) pRow["����ϼ�"] + 1;
                            }
                            break;
                    }
                }
                catch (Exception exp)
                {
                    Hy.Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                    //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                }
            }
            resultTable.Columns.Remove("����BSM");
            return resultTable;
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="resultTb"></param>
        /// <returns></returns>
        public double GetResultMark(DataTable resultTb)
        {
            long zqx = 0;
            double totalMark = 100;
            foreach (DataRow dr in resultTb.Rows)
            {
                if (Convert.ToInt32(dr["����ȱ��"]) > 0) return 0;

                zqx += Convert.ToInt32(dr["��ȱ��"]);
                if (zqx * MARK_WEIGHT > 100) return 0;
            }

            totalMark -= zqx * MARK_WEIGHT;
            try
            {
                //Hashtable hashLayerWeight = new Hashtable();
                //hashLayerWeight["������"] = 2/11*0.9;
                //hashLayerWeight["����������"] = 3/11*1/7*0.9;
                //hashLayerWeight["����ͼ��"] = 3/11*0.9;
                //hashLayerWeight["�������"] = 3/11*1/7*0.9;
                //hashLayerWeight["��״����"] = 1/11*0.9;
                //hashLayerWeight["�ڵ�"] = 2/11*0.9;
                //hashLayerWeight["��ַ��"] = 3/11*1/7*0.9;
                //hashLayerWeight["��ַ��"] = 3/11*1/7*0.9;
                //hashLayerWeight["�¶�ͼ"] = 3/11*1/7*0.9;
                //hashLayerWeight["����ũ�ﱣ��Ƭ"] = 3/11*1/7*0.9;
                //hashLayerWeight["����ũ�ﱣ��ͼ��"] = 3/11*1/7*0.9;

                DataTable pTb = BuilderRuleStyleTable();
                DataRow[] drRow = pTb.Select("����ȱ��=0");
                for (int i = 0; i < drRow.Length; i++)
                {
                    DataRow dr = drRow[i];
                    long lErrorcount = Convert.ToInt32(dr["��ȱ��"]);
                    //long lRecordCount = Convert.ToInt32(dr["��¼����"]);
                    if (lErrorcount > 0)
                    {
                        //if (hashLayerWeight.ContainsKey(dr["ͼ��"].ToString()))
                        {
                            //totalMark -= Math.Ceiling((double) (lErrorcount/9))*0.1;
                            totalMark -= lErrorcount * MARK_LIGHT;
                        }
                    }
                }
                return totalMark > 0 ? totalMark : 0;
            }
            catch(Exception exp)
            {
                Hy.Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

            }
            return totalMark;
        }

        /// <summary>
        /// ����ȱ�ݵȼ���
        /// </summary>
        /// <param name="resultTable"></param>
        public void BuilderBugGradeTable(ref DataTable resultTable, StateItem[] arrItem)
        {
            resultTable = new DataTable("ResultCollect");
            DataColumn col = resultTable.Columns.Add();
            col.ColumnName = "ȱ�ݵȼ�";
            col.DataType = Type.GetType("System.String");

            col = resultTable.Columns.Add();
            col.ColumnName = "����ϼ�";
            col.DataType = Type.GetType("System.Int32");

            int LesserLight = 0;
            int Deadliness = 0;
            int Graveness = 0;
            int LesserWeight = 0;
            int Light = 0;
            int Weight = 0;
            for (int i = 0; i < arrItem.Length; i++)
            {
                Graveness += arrItem[i].Graveness;
                Light += arrItem[i].Light;
                Weight += arrItem[i].Weight;
            }

            DataRow dr = resultTable.NewRow();
            dr["ȱ�ݵȼ�"] = "��";
            dr["����ϼ�"] = Light;
            resultTable.Rows.Add(dr);

            dr = resultTable.NewRow();
            dr["ȱ�ݵȼ�"] = "��";
            dr["����ϼ�"] = Weight;
            resultTable.Rows.Add(dr);

            dr = resultTable.NewRow();
            dr["ȱ�ݵȼ�"] = "����";
            dr["����ϼ�"] = Graveness;
            resultTable.Rows.Add(dr);
        }
    }
}