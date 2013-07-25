using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Hy.Check.UI.UC.Sundary;
using Hy.Check.Task;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using Hy.Check.Define;
using Hy.Common.Utility.Esri;
using CheckTask = Hy.Check.Task.Task;

namespace Hy.Check.UI.UC
{
    public partial class UCResult : DevExpress.XtraEditors.XtraUserControl
    {
        public UCResult()
        {
            InitializeComponent();
        }

        public UCResult(object hook,CheckTask currentTask)
        {
            InitializeComponent();

            this.Hook = hook;
            this.m_CurrentTask = currentTask;
            this.m_ErrorHelper.ResultConnection = currentTask.ResultConnection;
        }

        private const string Field_Name_DefectLevel = "ȱ�ݼ���";              // ȱ�ݼ���
        private const string Field_Name_Exception = "�Ƿ�����";              // ����
        private const string Field_Name_TargetLayer = "ͼ��";       // �ʼ�����Ե�ͼ��
        private const string Field_Name_ReferLayer = "ͼ��2";        // ���ʱ�Ĳ���ͼ��
        private const string Field_Name_TargetOID = "ObjectID";         // �ʼ�����Ե�ͼ���OID
        private const string Field_Name_ReferOID = "ObjectID2";          // ���ʱ�Ĳ���ͼ���OID
        private const string Field_Name_TargetBSM = "��ʶ��";         // �ʼ�����Ե�ͼ��ı�ʶ��
        private const string Field_Name_ReferBSM = "��ʶ��2";          // ���ʱ�Ĳ���ͼ��ı�ʶ��
        private const string Field_Name_ErrorID = "ErrNum";
        private const string Field_Name_Remark = "��ע";

        private const string Field_Name_TopologyRuleType = "ArcGISRule";
        private const string Field_Name_TopologyGeometryType = "JHLX";
            

        private ErrorHelper m_ErrorHelper = new ErrorHelper();
        private CheckTask m_CurrentTask = null;
        /// <summary>
        /// ���õ�ǰCheckTask����
        /// </summary>
        public CheckTask CurrentTask
        {
            set
            {
                this.m_CurrentTask = value;
                this.m_ErrorHelper.ResultConnection = this.m_CurrentTask.ResultConnection;
            }
        }


        private HookHelper m_HookHelper = new HookHelper();
        /// <summary>
        /// ����ArcGIS Hook
        /// </summary>
        public object Hook
        {
            set
            {
                m_HookHelper.Hook = value;
            }
        }

        private enumContentType m_ContentType;
        /// <summary>
        /// ������������
        /// </summary>
        public enumContentType ContentType
        {
            set
            {
                m_ContentType = value;
                if (this.m_ContentType == enumContentType.Rule)
                {
                    ucNavigate.Visible = false;
                }
                else
                {
                    ucNavigate.Visible = true;
                    ucNavigate.PageIndex = 0;
                }
                this.gvResult.Columns.Clear();
                this.gcResult.Refresh();
                //this.DataSource = null;
            }
        }

        private enumDefectLevel m_ExeptionDefectLevel = enumDefectLevel.Serious;
        /// <summary>
        /// ���ò����������ȱ�ݼ���
        /// </summary>
        public enumDefectLevel ExeptionDefectLevel
        {
            set
            {
                m_ExeptionDefectLevel = value;
            }
        }

        private List<string> m_FieldList = new List<string>()
        {
            "�������",
            "ͼ��",
            "��ʶ��",
            "OID",
            "ͼ��2",
            "��ʶ��2",
            "OID2",
            "��������",
            "ȱ�ݼ���",
            "�Ƿ�����",
            "��ע"
        };

        private Hy.Check.UI.Forms.FrmAddRemark m_FrmRemark = new Hy.Check.UI.Forms.FrmAddRemark(null);        


        private DataTable m_DataSource;
        public DataTable DataSource
        {
            set
            {
                m_DataSource = value;
                gvResult.Columns.Clear();
                gcResult.Visible = true;
                treeListResult.Visible = false;

                gcResult.BeginUpdate();
                gcResult.DataSource = m_DataSource;
                gcResult.Refresh();
                //gcResult.RefreshDataSource();
                gvResult.RefreshData();
                //gvResult.BestFitColumns();
                gcResult.EndUpdate();


                gvResult.OptionsBehavior.Editable = false;

                if (m_DataSource == null)
                    return;

                if (this.m_ContentType == enumContentType.Error)
                {
                    for (int i = 0; i < gvResult.Columns.Count; i++)
                    {
                        if (!this.m_FieldList.Contains(m_DataSource.Columns[i].Caption))
                        {
                            gvResult.Columns[i].Visible = false;

                        }
                    }
                }

                if ( this.m_ContentType == enumContentType.Error&& m_DataSource.Columns.Contains(Field_Name_DefectLevel) && m_DataSource.Columns.Contains(Field_Name_Exception))
                {
                    SetRowColor(gvResult.Columns[Field_Name_DefectLevel], gvResult.Columns[Field_Name_Exception]);
                    SetExceptionColor(gvResult.Columns[Field_Name_Exception]);
                }
            }
        }

        private int m_PageCount;
        /// <summary>
        /// ��ҳ��
        /// </summary>
        public int PageCount
        {
            get
            {
                return m_PageCount;
            }
            set
            {
                m_PageCount = value;
                this.ucNavigate.PageCount = this.m_PageCount;
            }
        }


        public GetDatasourceHandler DatasourceGetter;
        public event FeatureChangedHandler SelectedErrorFeatureChanged;
        //public event FeatureChangedHandler SelectedReferFeatureChanged;

        //private void SendFeatureChangedEvent(IFeature selErrorFeature)
        //{
        //    if (this.SelectedErrorFeatureChanged != null)
        //        this.SelectedErrorFeatureChanged.Invoke(selErrorFeature);
        //}

        /// <summary>
        /// ����ĳһ�е�ֵ����gridview�и��е���ɫ
        /// ʹ���ֳɵķ���
        /// </summary>
        private void SetExceptionColor(GridColumn gc)
        {
            StyleFormatCondition styleFormatCondition4 = new StyleFormatCondition();
            styleFormatCondition4.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Strikeout);
            styleFormatCondition4.Appearance.ForeColor = SystemColors.ControlDark;
            styleFormatCondition4.Appearance.Options.UseFont = true;
            styleFormatCondition4.Appearance.Options.UseForeColor = true;
            styleFormatCondition4.ApplyToRow = true;
            styleFormatCondition4.Column = gc;
            styleFormatCondition4.Condition = FormatConditionEnum.Equal;
            styleFormatCondition4.Value1 = true;

            gvResult.FormatConditions.AddRange(new StyleFormatCondition[]
                                                           {
                                                               styleFormatCondition4
                                                           });
        }

        /// <summary>
        /// /// <summary>
        /// ����ĳһ�е�ֵ����gridview�и��е���ɫ,����ȱ�ݵȼ����Ƿ����������ֶ�
        /// ʹ���ֳɵķ���
        /// </summary>
        /// </summary>
        /// <param name="gcErrType"></param>
        private void SetRowColor(GridColumn gcErrType, GridColumn gcIsException)
        {
            StyleFormatCondition styleFormatCondition1 = new StyleFormatCondition();
            styleFormatCondition1.Appearance.ForeColor = Color.Red;
            styleFormatCondition1.Appearance.Options.UseBackColor = true;
            styleFormatCondition1.Appearance.Options.UseForeColor = true;
            styleFormatCondition1.ApplyToRow = true;
            styleFormatCondition1.Column = gcErrType;
            styleFormatCondition1.Condition = FormatConditionEnum.Equal;
            styleFormatCondition1.Value1 = "����ȱ��";

            StyleFormatCondition styleFormatCondition2 = new StyleFormatCondition();
            //styleFormatCondition2.Appearance.BackColor = System.Drawing.Color.Red;
            styleFormatCondition2.Appearance.ForeColor = Color.Orange;
            styleFormatCondition2.Appearance.Options.UseBackColor = true;
            styleFormatCondition2.Appearance.Options.UseForeColor = true;
            styleFormatCondition2.ApplyToRow = true;
            styleFormatCondition2.Column = gcErrType;
            styleFormatCondition2.Condition = FormatConditionEnum.Equal;
            styleFormatCondition2.Value1 = "��ȱ��";

            StyleFormatCondition styleFormatCondition3 = new StyleFormatCondition();
            styleFormatCondition3.Appearance.ForeColor = Color.YellowGreen;
            styleFormatCondition3.Appearance.Options.UseBackColor = true;
            styleFormatCondition3.Appearance.Options.UseForeColor = true;
            styleFormatCondition3.ApplyToRow = true;
            styleFormatCondition3.Column = gcErrType;
            styleFormatCondition3.Condition = FormatConditionEnum.Equal;
            styleFormatCondition3.Value1 = "��ȱ��";

            if (gcIsException.Visible)
            {
                StyleFormatCondition styleFormatCondition4 = new StyleFormatCondition();
                styleFormatCondition4.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Strikeout);
                styleFormatCondition4.Appearance.ForeColor = SystemColors.ControlDark;
                styleFormatCondition4.Appearance.Options.UseFont = true;
                styleFormatCondition4.Appearance.Options.UseForeColor = true;
                styleFormatCondition4.ApplyToRow = true;
                styleFormatCondition4.Column = gcIsException;
                styleFormatCondition4.Condition = FormatConditionEnum.Equal;
                styleFormatCondition4.Value1 = true;

                gvResult.FormatConditions.AddRange(new StyleFormatCondition[]
                                                           {
                                                               styleFormatCondition1,
                                                               styleFormatCondition2,
                                                               styleFormatCondition3,
                                                               styleFormatCondition4
                                                           });
            }
            else
            {
                gvResult.FormatConditions.AddRange(new StyleFormatCondition[]
                                                           {
                                                               styleFormatCondition1,
                                                               styleFormatCondition2,
                                                               styleFormatCondition3
                                                           });
            }
        }

        private void ucNavigate_RequiredPageChanged(int pageIndex)
        {
            if (this.DatasourceGetter != null)
            {
                DataTable dtSource= this.DatasourceGetter.Invoke(pageIndex);
                this.DataSource = dtSource;
            }
        }


        private void gvResult_FocusedColumnChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventArgs e)
        {
            if (e.FocusedColumn == null)
                return;

            if (e.FocusedColumn.FieldName == Field_Name_Exception)
            {
                gvResult.OptionsBehavior.Editable = true;
            }
            else
            {
                gvResult.OptionsBehavior.Editable = false;
            }
        }

        // ��ȱ�ݼ���������--�������޸ĵ�
        private void gvResult_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            // ����༭��ֻ������
            if (e.Column != gvResult.Columns[Field_Name_Exception])
                return;

            DataRow rowError = gvResult.GetFocusedDataRow();
            if (rowError == null)
                return;


            if ((bool)e.Value)
            {
                if ((int)rowError["DefectLevel"] == (int)this.m_ExeptionDefectLevel)
                {
                    XtraMessageBox.Show("����ȱ�ݼ���Ĵ�����������");
                    rowError[Field_Name_Exception] = false;
                    gcResult.RefreshDataSource();
                    return;
                }
                if (m_FrmRemark.ShowDialog() == DialogResult.OK)
                {
                    rowError[Field_Name_Remark] = m_FrmRemark.m_strRemark;
                }
                else
                {
                    rowError[Field_Name_Exception] = false;
                    //gvResult.SetFocusedRowCellValue(e.Column, false);
                    gcResult.RefreshDataSource(); ;
                }
            }
            else
            {
                rowError[Field_Name_Remark] = null;
            }
        }
        // ˢ�����ݿ�
        private void gvResult_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if(e.Column!=gvResult.Columns[Field_Name_Exception])
                return;

            DataRow rowError=gvResult.GetDataRow(e.RowHandle);
            if (rowError == null)
                return;

            string strID = rowError[Field_Name_ErrorID].ToString();
            enumErrorType errType=(enumErrorType)rowError["ErrorType"];
            
            this.m_ErrorHelper.CommitExceptionEdit(errType, strID, (bool)e.Value,rowError[Field_Name_Remark] as string);
        }

        private void gvResult_DoubleClick(object sender, EventArgs e)
        {
            if (this.m_ContentType == enumContentType.Rule)
                return;

            DataRow rowError = gvResult.GetFocusedDataRow();
            if (rowError == null)
                return;

            if (this.m_HookHelper.FocusMap != null)
            {
                IMap theMap = this.m_HookHelper.FocusMap;
                theMap.ClearSelection();

                string strTargetLayer = rowError[Field_Name_TargetLayer] as string;
                if ((enumErrorType)rowError["ErrorType"] == enumErrorType.Topology)
                    strTargetLayer = rowError["TPTC"] as string;

                MapLayersController mapController = new MapLayersController(theMap);
                mapController.SetAllLayersVisible(false);
                mapController.SetLayerVisible(strTargetLayer, true);
                if (rowError.Table.Columns.Contains(Field_Name_ReferLayer))
                {
                    string strReferLayer = rowError[Field_Name_ReferLayer] as string;
                    mapController.SetLayerVisible(strReferLayer, true);
                }

                IFeature fRefer = null;
                IFeature fError=GetErrorFeature(rowError,ref fRefer);
                if (this.SelectedErrorFeatureChanged != null)
                    this.SelectedErrorFeatureChanged.Invoke(fError,fRefer);

                //if (fRefer!=null && this.SelectedReferFeatureChanged != null)
                //    this.SelectedReferFeatureChanged.Invoke(fRefer);

                if (fError != null)
                {
                    Hy.Common.Utility.Esri.MapOperAPI.ZoomToFeature(Control.FromHandle((IntPtr)(this.m_HookHelper.Hook as IMapControl2).hWnd) as AxMapControl, fError.Shape);
                    Hy.Common.Utility.Esri.MapOperAPI.FlashGeometry(this.m_HookHelper.Hook as IMapControl4, fError.Shape);
                }
                
            }
        }

        /// <summary>
        /// ��DataRow��ȡFeature������вο�ͼ�㣬ͬʱ���زο�Feature��
        /// </summary>
        /// <param name="rowError"></param>
        /// <param name="fRefer"></param>
        /// <returns></returns>
        private IFeature GetErrorFeature(DataRow rowError, ref IFeature fRefer)
        {
            string strTargetLayer = rowError[Field_Name_TargetLayer] as string;
            if ((enumErrorType)rowError["ErrorType"] == enumErrorType.Topology)
                strTargetLayer = rowError["TPTC"] as string;

            IFeature fTarget = GetFeature(rowError, strTargetLayer);
            if (rowError.Table.Columns.Contains(Field_Name_ReferLayer))
            {
                string strReferLayer = rowError[Field_Name_ReferLayer] as string;
                fRefer = GetFeature(rowError, strReferLayer);
            }

            return fTarget;
        }


        private IFeature GetFeature(DataRow rowError, string strLayerName)
        {
            IMap theMap = this.m_HookHelper.FocusMap;
            MapLayersController mapController = new MapLayersController(theMap);
            enumErrorType errType = (enumErrorType)Convert.ToInt32(rowError["ErrorType"]);
            switch (errType)
            {
                case enumErrorType.Topology:
                    {
                        ITopologyLayer topoLayerTarget = mapController.GetLayer(strLayerName) as ITopologyLayer;
                        if (topoLayerTarget == null)
                            return null;

                        ITopology topology = topoLayerTarget.Topology;
                        IErrorFeatureContainer errFeatureContainer = topology as IErrorFeatureContainer;
                        esriTopologyRuleType ruleType = (esriTopologyRuleType)Convert.ToInt32( rowError[Field_Name_TopologyRuleType]);
                        //string strGeoType = rowError[Field_Name_TopologyGeometryType] as string;
                        //esriGeometryType geoType = (strGeoType == "��" ? esriGeometryType.esriGeometryPoint : (strGeoType == "��" ? esriGeometryType.esriGeometryLine : esriGeometryType.esriGeometryPolygon));
                        esriGeometryType geoType = (esriGeometryType)Convert.ToInt32(rowError[Field_Name_TopologyGeometryType]);
                        int sourceClassID = (int)rowError["SourceLayerID"];
                        int sourceOID = (int)rowError["OID"];
                        int targetClassID = (int)rowError["TargetLayerID"];
                        int targetOID = Convert.ToInt32( rowError["OID2"]);

                        return errFeatureContainer.get_ErrorFeature((topology as IGeoDataset).SpatialReference, ruleType, geoType, sourceClassID, sourceOID, targetClassID, targetOID) as IFeature;

                    }
                    break;

                case enumErrorType.Normal:
                    {
                        IFeatureLayer flyrTarget = mapController.GetLayer(strLayerName) as IFeatureLayer;
                        if (flyrTarget == null)
                            return null;

                        // ��Ϊ���ȴ�OID���Ҷ�λ
                        if (rowError.Table.Columns.Contains(Field_Name_TargetOID) && rowError[Field_Name_TargetOID] != DBNull.Value)
                        {
                            return flyrTarget.FeatureClass.GetFeature((int)rowError[Field_Name_TargetOID]);
                        }
                        else if (rowError.Table.Columns.Contains(Field_Name_TargetBSM) && !string.IsNullOrEmpty(rowError[Field_Name_TargetBSM] as string))
                        {

                            IQueryFilter qFilter = new QueryFilterClass();
                            IFields fields = flyrTarget.FeatureClass.Fields;
                            int fieldIndex = fields.FindField("BSM");
                            if (fieldIndex < 0)
                                return null;

                            IField bsmField = fields.get_Field(fieldIndex);
                            if (bsmField.Type == esriFieldType.esriFieldTypeInteger)
                            {
                                qFilter.WhereClause = string.Format("BSM={0}", rowError[Field_Name_TargetBSM]);
                            }
                            else
                            {
                                qFilter.WhereClause = string.Format("BSM='{0}'", rowError[Field_Name_TargetBSM]);
                            }
                            IFeatureCursor fCursor = flyrTarget.FeatureClass.Search(qFilter, false);

                            return fCursor.NextFeature();
                        }
                        else
                        {
                            return null;
                        }
                    }

                default: return null;
            }
        }


        private void barBtnSetAllException_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_FrmRemark.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i <m_DataSource.Rows.Count; i++)
                {
                    if ((int)m_DataSource.Rows[i]["DefectLevel"] == (int)this.m_ExeptionDefectLevel)
                        continue;

                    gvResult.SetRowCellValue(gvResult.GetRowHandle(i), Field_Name_Remark, m_FrmRemark.m_strRemark);
                    gvResult.SetRowCellValue(gvResult.GetRowHandle(i), Field_Name_Exception, true);
                }
            }
        }

        private void barBtnSetAllNotException_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            for (int i = 0; i < m_DataSource.Rows.Count; i++)
            {
                if ((int)m_DataSource.Rows[i]["DefectLevel"] == (int)this.m_ExeptionDefectLevel)
                    continue;

                gvResult.SetRowCellValue(gvResult.GetRowHandle(i), Field_Name_Remark, null);
                gvResult.SetRowCellValue(gvResult.GetRowHandle(i), Field_Name_Exception, false);

            }
        }

        private void gcResult_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (this.m_ContentType == enumContentType.Rule)
                return;

            if (gvResult.RowCount == 0)
                return;

            if (m_DataSource.Select(string.Format("DefectLevel={0}", (int)this.m_ExeptionDefectLevel)).Length == m_DataSource.Rows.Count)
            {
                barBtnSetAllException.Enabled = false;
            }
            else
            {
                barBtnSetAllException.Enabled = true;
            }
            popupMenuExceptions.ShowPopup(gcResult.PointToScreen(  e.Location));
        }

        /// <summary>
        /// ������TreeListNode��Ϊ����Դ
        /// </summary>
        /// <param name="nodeSource"></param>
        public void SetTreeListNode(TreeListNode treeListNode)
        {
            gcResult.Visible = false;
            treeListResult.Visible = true;
            treeListResult.Columns.Clear();
            treeListResult.Nodes.Clear();
            if (treeListNode.Nodes.Count > 0)
            {
                for (int i = 0; i < treeListNode.TreeList.Columns.Count; i++)
                {
                    DevExpress.XtraTreeList.Columns.TreeListColumn curColumn = treeListNode.TreeList.Columns[i];
                    DevExpress.XtraTreeList.Columns.TreeListColumn colNew = treeListResult.Columns.Add();
                    colNew.Caption = curColumn.Caption;
                    colNew.Name = curColumn.Name;
                    colNew.FieldName = curColumn.FieldName;
                    colNew.Visible = curColumn.Visible;
                    colNew.OptionsColumn.AllowEdit = curColumn.OptionsColumn.AllowEdit;
                }
                AddNode(treeListNode, null);
            }
        }

        private void AddNode(TreeListNode nodeSource, TreeListNode nodeParent)
        {
            int fieldCount = nodeSource.TreeList.Columns.Count;
            object[] objValues = new object[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                objValues[i] = nodeSource.GetValue(i);
            }

            TreeListNode nodeCurrent=null;
            if (objValues[0] != "root")
            {
                nodeCurrent = this.treeListResult.AppendNode(objValues, nodeParent);
            }

            foreach (DevExpress.XtraTreeList.Nodes.TreeListNode nodeSub in nodeSource.Nodes)
            {
                AddNode(nodeSub, nodeCurrent);
            }
        }
    }


    /// <summary>
    /// ������ݿؼ����⽻�����ݵ�EventArgs
    /// </summary>
    public class DatasourceEventArgs : EventArgs
    {
        /// <summary>
        /// ����ҳ��
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// ��ǰ������ģ�ҳ
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// ������������
        /// </summary>
        public enumContentType ContentType { get; set; }
        /// <summary>
        /// �����������ȱ�ݼ���
        /// </summary>
        public enumDefectLevel ExeptionDefectLevel { get; set; }
    }

    public delegate DataTable GetDatasourceHandler(int pageIndex);

    public delegate void FeatureChangedHandler(IFeature selErrorFeature,IFeature selReferFeature);
}
