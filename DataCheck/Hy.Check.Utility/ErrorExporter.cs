using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using Microsoft.Office.Interop.Excel;

using Common.UI;
using Path = System.IO.Path;
using System.Collections.Specialized;

using Application = Microsoft.Office.Interop.Excel.Application;
using DataTable = System.Data.DataTable;
using IPoint = ESRI.ArcGIS.Geometry.IPoint;

namespace Hy.Check.Utility
{
    public class ErrorExporter
    {

        public static DataTable GetError(IDbConnection resultConnection)
        {
            string strSQL = @"
                            SELECT
                            b.CheckType,
                            a.TargetFeatClass1 as YSTC,
                            b.GZBM,
                            a.TargetFeatClass2 as MBTC,
                            a.BSM2 as BSM2,
                            a.BSM as SourceBSM,
                            a.ErrMsg as Description,
                            '' as TopoLayerName,
                            '' as ArcGisRule,
                            '' as JHLX,
                            'LR_ResAutoAttr' as SysTab,
                            a.IsException ,
                            a.Remark
                            from LR_ResAutoAttr as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                                union all

                            SELECT
                            b.CheckType,
                            a.YSTC as YSTC,
                            b.GZBM ,
                            a.MBTC as MBTC,
                            a.TargetBSM as BSM2,
                            a.SourceBSM as SourceBSM,
                            a.Reason as Description,
                            a.TPTC as TopoLayerName,
                            a.ArcGisRule as ArcGisRule,
                            a.JHLX as JHLX,
                            'LR_ResAutoTopo' as SysTab,
                            a.IsException ,
                            a.Remark
                            from LR_ResAutoTopo as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                                union all

                            SELECT
                            b.CheckType,
                            a.AttrTabName as YSTC,
                            b.GZBM,
                            '' as MBTC,
                            '' as BSM2,
                            '' as SourceBSM,
                            a.ErrorReason as Description,
                            '' as TopoLayerName,
                            '' as ArcGisRule,
                            '' as JHLX,
                            'LR_ResIntField' as SysTab,
                            a.IsException ,
                            a.Remark
                            from LR_ResIntField as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID

                                union all

                            SELECT
                            b.CheckType,
                            a.ErrorLayerName as YSTC,
                            b.GZBM,
                            '' as MBTC,
                            '' as BSM2,
                            '' as SourceBSM,
                            a.ErrorReason as Description,
                            '' as TopoLayerName,
                            '' as ArcGisRule,
                            '' as JHLX,
                            'LR_ResIntLayer' as SysTab,
                            a.IsException ,
                            a.Remark
                            from LR_ResIntLayer as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID";

            DataTable dtError = Common.Utility.Data.AdoDbHelper.GetDataTable(resultConnection, strSQL);

            // �޸��ֶ�Caption
            dtError.Columns["CheckType"].Caption = "�������";
            dtError.Columns["YSTC"].Caption = "ͼ����";
            dtError.Columns["SourceBSM"].Caption = "��ʶ��";
            dtError.Columns["MBTC"].Caption = "ͼ��2";
            dtError.Columns["BSM2"].Caption = "��ʶ��2";
            dtError.Columns["TopoLayerName"].Caption = "����ͼ����";
            dtError.Columns["Description"].Caption = "��������";
            dtError.Columns["IsException"].Caption = "�Ƿ�����";
            dtError.Columns["Remark"].Caption = "˵��";
            dtError.Columns["GZBM"].Caption = "�������";


            return dtError;
        }


        /// <summary>
        /// ��������Excel�ļ�
        /// </summary>
        public static bool ExportToExcel(XProgress xProgress, DataTable dtError, string strFilePath)
        {
            try
            {


                if (File.Exists(strFilePath))
                {
                    try
                    {
                        File.Delete(strFilePath);
                    }
                    catch(Exception exp)
                    {
                        Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                        return false;
                    }

                }

                Application xlApp = new Application();

                Workbook workbook = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                Worksheet worksheet = null;

                xProgress.ShowGifProgress(null);
                xProgress.ShowHint("���ڵ�����������Excel�ļ�...");

                try
                {

                    //д����ֵ 
                    DataTable PropertyTable = dtError;
                    //DataTable TopoTable = CCheckApplication.ucTopoErrMap.m_DataTable;

                    #region �������Լ�¼������˼�¼�����
                    if (PropertyTable != null && PropertyTable.Rows.Count != 0)
                    {
                        //����ÿ���ʼ����ͣ�����һ��worksheet
                        Hashtable hashtable = new Hashtable();
                        int index = 1;
                        foreach (DataRow dr in PropertyTable.Rows)
                        {
                            string strChkType = dr["CheckType"].ToString();
                            if (strChkType == "")
                            {
                                continue;
                            }
                            if (!hashtable.Contains(strChkType))
                            {
                                hashtable.Add(strChkType, "");
                                if (index == 1)
                                {
                                    worksheet = (Worksheet)workbook.Worksheets[1]; //ȡ��sheet1 
                                    worksheet.Name = strChkType;
                                }
                                else
                                {
                                    worksheet =
                                        (Worksheet)
                                        workbook.Sheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                    worksheet.Name = strChkType;
                                }
                                index++;
                            }
                        }

                        ///��worksheet��д���ֶ���
                        //д���ֶ�
                        List<string> listPropertyFields = GetPropertyListFields();
                        List<string> listTableStructFields = GetStructFields();
                        List<string> listTopoFields = GetTopoListFields();

                        Worksheet tempSheet = null;
                        for (int k = 1; k <= workbook.Sheets.Count; k++)
                        {
                            tempSheet = (Worksheet)workbook.Sheets[k];
                            if (tempSheet.Name.Contains("���˹�ϵ") && tempSheet.Name != "��������˹�ϵ")
                            {
                                for (int i = 0; i < listTopoFields.Count; i++)
                                {
                                    tempSheet.Cells[1, i + 1] = listTopoFields[i];
                                }


                            }
                            else if (tempSheet.Name == "�ṹ������")
                            {
                                for (int i = 0; i < listTableStructFields.Count; i++)
                                {
                                    tempSheet.Cells[1, i + 1] = listTableStructFields[i];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < listPropertyFields.Count; i++)
                                {
                                    tempSheet.Cells[1, i + 1] = listPropertyFields[i];
                                }
                            }

                            Marshal.ReleaseComObject(tempSheet);
                        }


                        int PropertyRowCount = PropertyTable.Rows.Count;
                        int PropertyColumnCount = PropertyTable.Columns.Count;

                        xProgress.ShowProgress(0, PropertyRowCount, 1, null);
                        DataRow[] listDr = null;
                        DataColumn dc;

                        tempSheet = new Worksheet();
                        for (int l = 1; l <= workbook.Sheets.Count; l++)
                        {
                            tempSheet = (Worksheet)workbook.Sheets[l];
                            string strChkType = tempSheet.Name;
                            listDr = PropertyTable.Select("CheckType = '" + strChkType + "'");
                            for (int r = 0; r < listDr.Length; r++)
                            {
                                xProgress.Step();
                                for (int i = 0; i < PropertyColumnCount; i++)
                                {
                                    dc = PropertyTable.Columns[i];

                                    if (strChkType.Contains("���˹�ϵ") && strChkType != "��������˹�ϵ")
                                    {
                                        string Exception = "˳��ţ�Դͼ��ID��Ŀ��ͼ��ID��Ŀ��OID��ԴOID���������ͣ�����ID��ϵͳ����";
                                        if (Exception.Contains(dc.Caption))
                                        {
                                            continue;
                                        }
                                        for (int k = 0; k < listTopoFields.Count; k++)
                                        {
                                            if (dc.Caption == listTopoFields[k])
                                            {
                                                if (dc.Caption == "�Ƿ�����")
                                                {
                                                    if (Convert.ToBoolean(listDr[r][i]) == false)
                                                    {
                                                        tempSheet.Cells[r + 2, k + 1] = "��";
                                                    }
                                                    else
                                                    {
                                                        tempSheet.Cells[r + 2, k + 1] = "��";
                                                    }
                                                }
                                                else
                                                {
                                                    tempSheet.Cells[r + 2, k + 1] = listDr[r][i];
                                                }
                                                break;
                                            }
                                        }

                                    }
                                    else if (tempSheet.Name == "�ṹ������")
                                    {
                                        if (dc.Caption == "ͼ��2" || dc.Caption == "��ʶ��2" || dc.Caption == "����ͼ����" ||
                                            dc.Caption == "˳���" || dc.Caption == "Դͼ��ID" || dc.Caption == "Ŀ��ͼ��ID" ||
                                            dc.Caption == "Ŀ��OID" || dc.Caption == "ԴOID" || dc.Caption == "��������" ||
                                            dc.Caption == "����ID" || dc.Caption == "ϵͳ����")
                                        {
                                            continue;
                                        }
                                        for (int k = 0; k < listTableStructFields.Count; k++)
                                        {
                                            if (dc.Caption == listTableStructFields[k])
                                            {
                                                if (dc.Caption == "�Ƿ�����")
                                                {
                                                    object bIsException = listDr[r][i];

                                                    if (bIsException == null || bIsException.ToString() == "")
                                                    {
                                                        tempSheet.Cells[r + 2, k + 1] = "��";
                                                    }
                                                    else
                                                    {

                                                        if (Convert.ToBoolean(bIsException) == false)
                                                        {
                                                            tempSheet.Cells[r + 2, k + 1] = "��";
                                                        }
                                                        else
                                                        {
                                                            tempSheet.Cells[r + 2, k + 1] = "��";
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    tempSheet.Cells[r + 2, k + 1] = listDr[r][i];
                                                }

                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dc.Caption == "ͼ��2" || dc.Caption == "��ʶ��2" || dc.Caption == "����ͼ����" ||
                                            dc.Caption == "˳���" || dc.Caption == "Դͼ��ID" || dc.Caption == "Ŀ��ͼ��ID" ||
                                            dc.Caption == "Ŀ��OID" || dc.Caption == "ԴOID" || dc.Caption == "��������" ||
                                            dc.Caption == "����ID" || dc.Caption == "ϵͳ����")
                                        {
                                            continue;
                                        }
                                        for (int k = 0; k < listPropertyFields.Count; k++)
                                        {
                                            if (dc.Caption == listPropertyFields[k])
                                            {
                                                if (dc.Caption == "�Ƿ�����")
                                                {
                                                    object bIsException = listDr[r][i];

                                                    if (bIsException == null || bIsException.ToString() == "")
                                                    {
                                                        tempSheet.Cells[r + 2, k + 1] = "��";
                                                    }
                                                    else
                                                    {

                                                        if (Convert.ToBoolean(bIsException) == false)
                                                        {
                                                            tempSheet.Cells[r + 2, k + 1] = "��";
                                                        }
                                                        else
                                                        {
                                                            tempSheet.Cells[r + 2, k + 1] = "��";
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    tempSheet.Cells[r + 2, k + 1] = listDr[r][i];
                                                }

                                                break;
                                            }
                                        }
                                    }


                                }
                            }
                            tempSheet.Columns.AutoFit();
                        }

                        if (tempSheet != null)
                        {
                            Marshal.ReleaseComObject(tempSheet);
                            tempSheet = null;
                        }


                    }
                    #endregion

                    xProgress.Hide();
                    //XtraMessageBox.Show("���������¼��Excel�ļ��ɹ�!", "��ʾ");

                    ///����excel�ļ�
                    workbook.Close(true, strFilePath, null);
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                    Marshal.ReleaseComObject(workbook);
                    if (worksheet != null)
                        Marshal.ReleaseComObject(worksheet);
                    GC.Collect();
                    if (xlApp != null)
                    {
                        Process[] pProcess;
                        pProcess = Process.GetProcessesByName("EXCEL"); //�ر�excel����
                        pProcess[0].Kill();
                    }
                    PropertyTable = null;
                    xlApp = null;
                    workbook = null;
                    worksheet = null;

                }
                catch (Exception exp)
                {
                    Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                    xProgress.Hide();

                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                    Marshal.ReleaseComObject(workbook);
                    if (worksheet != null)
                        Marshal.ReleaseComObject(worksheet);
                    GC.Collect();
                    if (xlApp != null)
                    {
                        Process[] pProcess;
                        pProcess = Process.GetProcessesByName("EXCEL"); //�ر�excel����
                        pProcess[0].Kill();
                    }
                    //XtraMessageBox.Show(ex.Message);
                    return false;
                }
                finally
                {
                    xProgress.Hide();

                }
            }
            catch (Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());
                //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                xProgress.Hide();
                //XtraMessageBox.Show(ex.Message);

                return false;

            }
            return true;
        }

        #region �����б������

        /// <summary>
        /// excel�������˴����ֶ���
        /// </summary>
        /// <returns></returns>
        private static List<string> GetTopoListFields()
        {
            List<string> listFields = new List<string>();
            //listFields.Add("˳���");
            listFields.Add("�������");
            listFields.Add("ͼ����");
            listFields.Add("��ʶ��");
            listFields.Add("ͼ��2");
            listFields.Add("��ʶ��2");
            listFields.Add("����ͼ����");
            listFields.Add("��������");
            listFields.Add("�Ƿ�����");
            listFields.Add("˵��");

            // 2012-04-24 �ź���
            // ��ӹ������
            listFields.Add("�������");

            return listFields;

        }

        /// <summary>
        /// excel�������Դ����ֶ���
        /// </summary>
        /// <returns></returns>
        private static List<string> GetPropertyListFields()
        {
            List<string> listFields = new List<string>();
            //listFields.Add("˳���");
            listFields.Add("�������");
            listFields.Add("ͼ����");
            listFields.Add("��ʶ��");
            listFields.Add("��������");
            listFields.Add("�Ƿ�����");
            listFields.Add("˵��");

            // 2012-04-24 �ź���
            // ��ӹ������
            listFields.Add("�������");

            return listFields;
        }

        /// <summary>
        /// �ṹ��������Ҫ�����ֶ������ֶ�
        /// </summary>
        /// <returns></returns>
        private static List<string> GetStructFields()
        {
            List<string> listFields = new List<string>();
            //listFields.Add("˳���");
            listFields.Add("�������");
            listFields.Add("ͼ����");
            listFields.Add("�ֶ�����");
            listFields.Add("��������");
            listFields.Add("�Ƿ�����");
            listFields.Add("˵��");

            // 2012-04-24 �ź���
            // ��ӹ������
            listFields.Add("�������");

            return listFields;
        }

        #endregion


        public string SchemaID { private get; set; }

        public ITopology Topology { private get; set; }

        public IWorkspace BaseWorkspace { private get; set; }

        public IDbConnection ResultConnection { private get; set; }


        public bool ExportToShp(string strPath, string strName)
        {
            IFeatureClass fClassDest = CreateShpFile(strPath, strName);
            bool isSucceed = (fClassDest != null);
            isSucceed = isSucceed && InsertNomarlError(fClassDest);
            isSucceed = isSucceed && InsertTopoError(fClassDest);

            System.Runtime.InteropServices.Marshal.ReleaseComObject(fClassDest);
            fClassDest = null;

            return isSucceed;
        }

        private IFeatureClass CreateShpFile(string strPath, string strName)
        {
            ISpatialReference spatialRef = (this.Topology as IGeoDataset).SpatialReference;
            IFeatureClass fClassDest = CreateShpFile(strPath, strName, "�����¼", spatialRef);
            AddFields(fClassDest);

            return fClassDest;
        }

        private List<string> m_FieldCaptions = new List<string>{
            "�������",
            "ͼ����",
            "��ʶ��",
            "ͼ��2",
            "��ʶ��2",
            "����ͼ����",
            "��������", 
            "�Ƿ�����",
            "˵��",
            "�������"
    };
        private bool InsertTopoError(IFeatureClass destFClass)
        {
            try
            {
                string strSQL = @"SELECT
                            b.CheckType,
                            IIF(b.TargetFeatClass1 is Null,'',b.TargetFeatClass1) as YSTC,
                            IIF(a.SourceBSM is Null,'',a.SourceBSM) as SourceBSM,
                            IIF(a.MBTC is Null,'',a.MBTC) as MBTC,
                            IIF(a.TargetBSM is Null,'',a.TargetBSM) as BSM2,
                            a.TPTC as TopoLayerName,
                            a.Reason as Description,
                            a.IsException as IsException,
                            IIf(a.Remark is Null,'',a.Remark) as Remark,
                            b.GZBM ,
                            a.ArcGisRule as ArcGisRule,
                            a.JHLX as JHLX,
                            a.SourceLayerID,
                            a.TargetLayerID,
                            a.SourceOID as OID,
                            a.TargetOID as OID2
                            from LR_ResAutoTopo as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID
                            ";


                DataTable dtError = Common.Utility.Data.AdoDbHelper.GetDataTable(this.ResultConnection, strSQL);
                
                IFeatureCursor fCusorInsert = destFClass.Insert(false);
                Dictionary<int, int> dictFieldIndex = new Dictionary<int, int>();
                for (int i = 0; i < m_FieldCaptions.Count; i++)
                {
                    dictFieldIndex.Add(i,destFClass.FindField(m_FieldCaptions[i]));
                }
                int xFieldIndex = destFClass.FindField("X����");
                int yFieldIndex = destFClass.FindField("Y����");

                IErrorFeatureContainer errFeatureContainer = this.Topology as IErrorFeatureContainer;
                ISpatialReference spatialRef = (this.Topology as IGeoDataset).SpatialReference;
                for (int i = 0; i < dtError.Rows.Count; i++)
                {
                    DataRow rowError = dtError.Rows[i];
                    int fClassID = Convert.ToInt32(rowError["SourceLayerID"]);
                    int fClassID2 = Convert.ToInt32(rowError["TargetLayerID"]);
                    int oid = Convert.ToInt32(rowError["OID"]);
                    int oid2 = Convert.ToInt32(rowError["OID2"]);
                    esriGeometryType geoType = (esriGeometryType)Convert.ToInt32(rowError["JHLX"]);
                    esriTopologyRuleType ruleType = (esriTopologyRuleType)Convert.ToInt32(rowError["ArcGISRule"]);

                    IFeature srcFeature = errFeatureContainer.get_ErrorFeature(spatialRef, ruleType, geoType, fClassID, fClassID2, oid, oid2) as IFeature;

                    IFeatureBuffer fNew = destFClass.CreateFeatureBuffer();
                    for (int j = 0; j < m_FieldCaptions.Count; j++)
                    {
                        int fIndex = dictFieldIndex[j];
                        if (fIndex < 0)
                            continue;

                        fNew.set_Value(fIndex, rowError[j]);
                    }
                    fNew.Shape = GetErrorGeometry(srcFeature);
                    IPoint point = fNew.Shape as IPoint;
                    fNew.set_Value(xFieldIndex, point.X);
                    fNew.set_Value(yFieldIndex, point.Y);

                    fCusorInsert.InsertFeature(fNew);

                    if (i % 2000 == 0)
                        fCusorInsert.Flush();

                }

                fCusorInsert.Flush();

                return true;
            }
            catch(Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());
                return false;
            }
        }


        private bool InsertNomarlError(IFeatureClass destFClass)
        {
            try
            {
                string strSQL = @"
                            SELECT
                            b.CheckType,
                            a.TargetFeatClass1 as YSTC,
                            a.BSM as SourceBSM,
                            a.TargetFeatClass2 as MBTC,
                            a.BSM2 as BSM2,
                            '' as TopoLayerName,
                            a.ErrMsg as Description,
                            IIF(a.IsException,1,0) as IsException,
                            IIf(a.Remark is Null,'',a.Remark) as Remark,
                            b.GZBM,
                            a.OID as OID
                            
                            from LR_ResAutoAttr as a, LR_ResultEntryRule as b where a.RuleInstID=b.RuleInstID";


                DataTable dtError = Common.Utility.Data.AdoDbHelper.GetDataTable(this.ResultConnection, strSQL);
                IFeatureCursor fCusorInsert = destFClass.Insert(false);
                Dictionary<string, IFeatureClass> dictFeatureClass = new Dictionary<string, IFeatureClass>();
                Dictionary<int, int> dictFieldIndex = new Dictionary<int, int>();
                for (int i = 0; i < m_FieldCaptions.Count; i++)
                {
                    dictFieldIndex.Add(i,destFClass.FindField(m_FieldCaptions[i]));
                }
                int xFieldIndex = destFClass.FindField("X����");
                int yFieldIndex = destFClass.FindField("Y����");

                for (int i = 0; i < dtError.Rows.Count; i++)
                {
                    DataRow rowError = dtError.Rows[i];
                    IFeatureClass curFClass;
                    string strFClassAlias = rowError["YSTC"] as string;
                    if (!dictFeatureClass.ContainsKey(strFClassAlias))
                    {
                        int standardID = SysDbHelper.GetStandardIDBySchemaID(this.SchemaID);
                        string strFClass = LayerReader.GetNameByAliasName(strFClassAlias, standardID);
                        IFeatureClass fClass = (this.BaseWorkspace as IFeatureWorkspace).OpenFeatureClass(strFClass);
                        dictFeatureClass.Add(strFClassAlias, fClass);
                        curFClass = fClass;
                    }
                    else
                    {
                        curFClass = dictFeatureClass[strFClassAlias];
                    }

                    if (curFClass == null)
                        continue;

                    object objOID = rowError["OID"];
                    if (objOID == null)
                        continue;

                    int oid = Convert.ToInt32(objOID);
                    IFeature srcFeature = curFClass.GetFeature(oid);
                    if (srcFeature == null)
                        continue;

                    IFeatureBuffer fNew = destFClass.CreateFeatureBuffer();
                    for (int j = 0; j < m_FieldCaptions.Count; j++)
                    {
                        int fIndex = dictFieldIndex[j];
                        if (fIndex < 0)
                            continue;

                        fNew.set_Value(fIndex, rowError[j]);
                    }
                    fNew.Shape = GetErrorGeometry(srcFeature);
                    IPoint point = fNew.Shape as IPoint;
                    fNew.set_Value(xFieldIndex, point.X);
                    fNew.set_Value(yFieldIndex, point.Y);

                    fCusorInsert.InsertFeature(fNew);

                    if (i % 2000 == 0)
                        fCusorInsert.Flush();
                }

                fCusorInsert.Flush();

                return true;
            }
            catch(Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                return false;
            }
        }

        /// <summary>
        /// ����shapeͼ��
        /// </summary>
        /// <param name="strShpPath"></param>
        /// <param name="strFtName"></param>
        /// <returns></returns>
        private IFeatureClass CreateShpFile(string strShpPath, string strFtName, string strAliasFtName, ISpatialReference pSpatial)
        {
            string connectionstring = "DATABASE=" + strShpPath;
            IWorkspaceFactory2 pFactory = (IWorkspaceFactory2)new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
            IWorkspace workspace = pFactory.OpenFromString(connectionstring, 0);
            IFeatureWorkspace ipFtWs = (IFeatureWorkspace)workspace;

            //�����ֶ�IFields
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;
            ///�������������ֶ�
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;

            ////����FID�ֶ�
            //IFieldEdit ipFldEdit = new FieldClass(); //(__uuidof(Field));
            //ipFldEdit.Name_2 = "FID";
            //ipFldEdit.AliasName_2 = "Ψһ��־��";
            //ipFldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            //pFieldsEdit.AddField(ipFldEdit);


            pFieldEdit.Name_2 = "Shape";
            pFieldEdit.AliasName_2 = "��������";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;


            IGeometryDef pGeomDef = new GeometryDefClass();
            IGeometryDefEdit pGeomDefEdit = (IGeometryDefEdit)pGeomDef;
            pGeomDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            pGeomDefEdit.SpatialReference_2 = pSpatial;
            pFieldEdit.GeometryDef_2 = pGeomDef;
            pFieldsEdit.AddField(pField);


            IFeatureClass _featureClass =
                ipFtWs.CreateFeatureClass(strFtName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");

            //����ͼ�����
            //IClassSchemaEdit ipEdit = (IClassSchemaEdit)_featureClass;
            //ipEdit.AlterAliasName(strAliasFtName);

            pFactory = null;
            workspace = null;
            ipFtWs = null;

            return _featureClass;
        }

        /// <summary>
        /// ��ͼ������ֶ�
        /// </summary>
        /// <param name="pFtCls"></param>
        private void AddFields(IFeatureClass pFtCls)
        {
            try
            {
                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 80;
                pFieldEdit.Name_2 = "�������";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 80;
                pFieldEdit.Name_2 = "ͼ����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 80;
                pFieldEdit.Name_2 = "�ֶ�����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 20;
                pFieldEdit.Name_2 = "��ʶ��";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 80;
                pFieldEdit.Name_2 = "ͼ��2";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 20;
                pFieldEdit.Name_2 = "��ʶ��2";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 50;
                pFieldEdit.Name_2 = "����ͼ����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 255;
                pFieldEdit.Name_2 = "��������";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);


                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 50;
                pFieldEdit.Name_2 = "�Ƿ�����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 255;
                pFieldEdit.Name_2 = "˵��";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 255;
                pFieldEdit.Name_2 = "�������";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                pFtCls.AddField(pField);


                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 50;
                pFieldEdit.Name_2 = "X����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                pFtCls.AddField(pField);

                pField = new FieldClass();
                pFieldEdit = (IFieldEdit)pField;
                pFieldEdit.Length_2 = 50;
                pFieldEdit.Name_2 = "Y����";
                pFieldEdit.IsNullable_2 = true;
                pFieldEdit.Required_2 = false;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                pFtCls.AddField(pField);

            }
            catch (Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

                //GT_CONST.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
            }
        }

        /// <summary>
        /// ��������Ҫ�ض�ת��Ϊ��Ҫ�أ������ص�Ҫ�ص�IGeometry
        /// </summary>
        /// <param name="pFeature">Ҫ��</param>
        /// <returns></returns>
        private IGeometry GetErrorGeometry(IFeature pFeature)
        {
            IGeometry pGeo = null;
            esriGeometryType type = pFeature.Shape.GeometryType;
            switch (type)
            {
                case esriGeometryType.esriGeometryPoint:
                    pGeo = pFeature.ShapeCopy;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    {
                        IPolygon pPolygon = (IPolygon)pFeature.Shape;
                        IArea pArea = (IArea)pPolygon;
                        IPoint pPoint = pArea.Centroid;
                        pGeo = (IGeometry)pPoint;
                        break;
                    }
                case esriGeometryType.esriGeometryPolyline:
                    {
                        IPolyline pPolyLine = (IPolyline)pFeature.Shape;
                        IPoint pPoint = GetMidPoint(pPolyLine);
                        pGeo = (IGeometry)pPoint;
                        break;
                    }
            }
            return pGeo;
        }

        /// <summary>
        /// ��ȡָ���ߵ����ĵ�
        /// </summary>
        /// <param name="pPolyline">��ͼ��</param>
        /// <returns>������ͼ�ε����µ�����</returns>
        private IPoint GetMidPoint(IPolyline pPolyline)
        {
            IPoint pPoint = null;

            if (pPolyline == null) return null;

            IPointCollection pPoints = pPolyline as IPointCollection;

            long lPointCount;
            //��¼��������
            lPointCount = pPoints.PointCount;

            long nMid = (lPointCount - 1) / 2 + 1;
            pPoint = pPoints.get_Point((int)nMid);
            return pPoint;
        }
    }

    public delegate void ErrorExported();
}
