using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;
//using Measure.Tool;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;

namespace Check.Command.MeasureCommand
{
    /// <summary>
    /// �������ͣ�Length���ȣ�Area���
    /// </summary>
    public enum MeasureType
    {
        Length,
        Area
    };

    public partial class FormDis : DevExpress.XtraEditors.XtraForm
    {
        /// <summary>
        /// ����
        /// </summary>
        public MeasureType m_MeasureType;  //��������
        public ITool m_Tool;               //��������axMapControl��IToolʵ��

        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="type">��������</param>
        /// <param name="ipTool">��������axMapControl��IToolʵ��</param>
        public FormDis(MeasureType type,ITool ipTool)
        {
            InitializeComponent();
            this.m_MeasureType = type;
            this.m_Tool = ipTool;
            this.TopMost = true;
            this.WriteLabelText(null);

            if (this.m_Tool.GetType() == typeof(ToolMeasureLength))
            {
                (this.m_Tool as ToolMeasureLength).MyInit();
            }
            if (this.m_Tool.GetType() == typeof(ToolMeasureArea))
            {
                (this.m_Tool as ToolMeasureArea).MyInit();
            }
        }

        /// <summary>
        /// ������������������FormDis�Ͻ�����ʾ
        /// </summary>
        /// <param name="ipGeo">�˻��������ɵ�ʵ��</param>
        public void WriteLabelText(IGeometry ipGeo)
        {
            switch(m_MeasureType)
            {
                case MeasureType.Length:          //���ⳤ��
                    {
                        string strUnit = " " + (this.m_Tool as ToolMeasureLength).m_hookHelper.FocusMap.MapUnits.ToString().Substring(4);
                        //this.m_LabelMeasureType.Text = "Line Measurement";
                        this.m_LabelMeasureType.Text = "��������";
                        this.m_labelArea.Visible = true;
                        if(ipGeo!=null)
                        {
                            IGeometryCollection ipGeoCol = ipGeo as IGeometryCollection;
                            ISegmentCollection ipSegmentColl = ipGeoCol.get_Geometry(0) as ISegmentCollection;
                            ILine ipLine = ipSegmentColl.get_Segment(ipSegmentColl.SegmentCount-1) as ILine;
                            //this.m_labelSegment.Text = "�γ���:" + ipLine.Length.ToString() + strUnit;
                            //this.m_labelLength.Text = "����:" + (ipGeoCol.get_Geometry(0) as ICurve).Length.ToString() + "��";
                            this.m_labelArea.Text = "����:" + (ipGeoCol.get_Geometry(0) as ICurve).Length.ToString("f3") + "��";
                        }
                        else
                        {
                            this.m_labelArea.Text = "����:" + "0" + "��";
                            //this.m_labelLength.Text = "�ܳ���:" + "0" + strUnit;
                        }
                    }
                    break;
                case MeasureType.Area:  //�������
                    {
                        string strUnit = " " + (this.m_Tool as ToolMeasureArea).m_hookHelper.FocusMap.MapUnits.ToString().Substring(4);
                        //this.m_LabelMeasureType.Text = "Area Measurement";
                        this.m_LabelMeasureType.Text = "�������";
                        this.m_labelArea.Visible = true;
                        if (ipGeo != null)
                        {
                            IGeometryCollection ipGeoCol = ipGeo as IGeometryCollection;
                            ISegmentCollection ipSegmentColl = ipGeoCol.get_Geometry(0) as ISegmentCollection;
                            ILine ipLine = ipSegmentColl.get_Segment(ipSegmentColl.SegmentCount-1) as ILine;
                            //this.m_labelSegment.Text = "Segment:" + ipLine.Length.ToString() + strUnit;
                            //this.m_labelLength.Text = "Perimeter:" + (ipGeoCol.get_Geometry(0) as IRing).Length.ToString() + strUnit;
                            //this.m_labelSegment.Text = "�γ���:" + ipLine.Length.ToString() + strUnit;
                            //this.m_labelLength.Text = "�ܳ�:" + (ipGeoCol.get_Geometry(0) as IRing).Length.ToString() + strUnit;
                            
                            IClone ipClone = ipGeo as IClone;
                            IGeometry ipGeo1 = ipClone.Clone() as IGeometry;
                            ITopologicalOperator ipTopo = ipGeo1 as ITopologicalOperator;
                            ipTopo.Simplify();

                            this.m_labelArea.Text = "���:" + ((ipGeo1 as IPolygon) as IArea).Area.ToString(".###") + "ƽ����";
                        }
                        else
                        {
                            this.m_labelArea.Text = "���:" + "0" + "ƽ����";                            
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// �رմ��ڣ��������Ӧ������m_Element��m_GeoMeasure��m_FormDis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormDis_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            if (this.m_Tool.GetType() == typeof(ToolMeasureLength))
            {
                IElement m_Element = (this.m_Tool as ToolMeasureLength).m_Element;
                if (m_Element != null)
                {
                    (this.m_Tool as ToolMeasureLength).m_hookHelper.ActiveView.GraphicsContainer.DeleteElement(m_Element);
                    m_Element = null;
                }
                (this.m_Tool as ToolMeasureLength).m_hookHelper.ActiveView.Refresh();
            }
            if (this.m_Tool.GetType() == typeof(ToolMeasureArea))
            {
                IElement m_Element = (this.m_Tool as ToolMeasureArea).m_Element;
                if (m_Element != null)
                {
                    (this.m_Tool as ToolMeasureArea).m_hookHelper.ActiveView.GraphicsContainer.DeleteElement(m_Element);
                    m_Element = null;
                }
                (this.m_Tool as ToolMeasureArea).m_hookHelper.ActiveView.Refresh();
            }*/

            if (this.m_Tool.GetType() == typeof(ToolMeasureLength))
            {
                (this.m_Tool as ToolMeasureLength).m_GeoMeasure = null;
                (this.m_Tool as ToolMeasureLength).m_FormDis = null;
                (this.m_Tool as ToolMeasureLength).m_Element = null;
            }
            if (this.m_Tool.GetType() == typeof(ToolMeasureArea))
            {
                (this.m_Tool as ToolMeasureArea).m_GeoMeasure = null;
                (this.m_Tool as ToolMeasureArea).m_FormDis = null;
                (this.m_Tool as ToolMeasureArea).m_Element = null;
            }
            
        }

        /// <summary>
        /// ����Ķ�ʱ����ز��������˻�����������ͼ����FormDis����ʾ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_timer_Tick(object sender, EventArgs e)
        {
            IGeometry ipGeo = (this.m_Tool as ToolMeasureArea).m_Element.Geometry;
            if (this.m_Tool.GetType() == typeof(ToolMeasureArea))
            {
                if (ipGeo != null)
                {
                    this.WriteLabelText(ipGeo);
                }
            }
            
        }
    }
}