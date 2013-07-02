using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.Reflection;
using System.Windows.Forms;

namespace Check.Command.MeasureCommand
{
    /// <summary>
    /// Summary description for ToolMeasureArea.
    /// </summary>
    [Guid("c7f187da-4a00-40a5-b942-c06d6a5de1cf")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Check.Command.MeasureCommand.ToolMeasureArea")]
    public sealed class ToolMeasureArea : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        public IHookHelper m_hookHelper;

        public IGeometry m_GeoMeasure;//��¼��������갴�µĵ�
        public IElement m_Element;//�����ʾͼ�ε�IElement
        private IFillSymbol m_FillSymbol;//��ʾ�ķ���
        public FormDis m_FormDis;//���㲢��ʾ��������ĶԻ���

        public ToolMeasureArea()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text 
            base.m_caption = "�������";  //localizable text 
            base.m_message = "�������";  //localizable text
            base.m_toolTip = "�ڵ�ͼ���ڵ�������������ʼ���㣻˫���������򵥻�����Ҽ�����������";  //localizable text
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_MyTool")
            try
            {


                Assembly thisExe = Assembly.GetExecutingAssembly();

                base.m_cursor =
                    new Cursor(thisExe.GetManifestResourceStream("Check.Command.Resources.ToolMeasureArea.cur"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overriden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            m_hookHelper.Hook = hook;

            // TODO:  Add ToolMeasureArea.OnCreate implementation
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add ToolMeasureArea.OnClick implementation

        }

        //����INewPolygonFeedback ��ʾ�켣
        private INewPolygonFeedback m_pNewPolygonFeed;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            //���ӵ����Ҽ��������� �ﾧ���20081028
            if (Button == 2)
            {
                OnDblClick();
                return;
            }

            IActiveView ipAV = this.m_hookHelper.ActiveView;
            ///////////�����µ�///////////////
            IPoint ipPt = new PointClass();
            ipPt = ipAV.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            SnapPoint(ipPt);
            ////////////�������///////////////
            //if (this.m_FormDis == null)
            //{
            //    MeasureType type = MeasureType.Area;
            //    this.m_FormDis = new FormDis(type, this);
            //    this.m_FormDis.TopMost = true;
            //    this.m_FormDis.Show();
            //}

            if (this.m_GeoMeasure == null)
            {
                m_pNewPolygonFeed = new NewPolygonFeedbackClass();
                IScreenDisplay pScreen = ipAV.ScreenDisplay;
                m_pNewPolygonFeed.Display = pScreen;
                m_pNewPolygonFeed.Start(ipPt);

                this.m_GeoMeasure = new PolylineClass();

            }
            else
            {
                m_pNewPolygonFeed.AddPoint(ipPt);
            }

            /*
            /////////����ʵ��///////////
            object obj = Type.Missing;
            if (this.m_GeoMeasure == null)
            {
                this.m_GeoMeasure = new PolygonClass();
                IGeometryCollection ipGeoCol = this.m_GeoMeasure as IGeometryCollection;
                ISegmentCollection ipSelCol = new RingClass();
                ILine ipLine = new LineClass();
                ipLine.PutCoords(ipPt, ipPt);
                ipSelCol.AddSegment(ipLine as ISegment, ref obj, ref obj);
                ipSelCol.AddSegment(ipLine as ISegment, ref obj, ref obj);
                ipGeoCol.AddGeometry(ipSelCol as IGeometry, ref obj, ref obj);
            }
            else
            {
                IGeometryCollection ipGeoCol = this.m_GeoMeasure as IGeometryCollection;
                ISegmentCollection ipSelCol = ipGeoCol.get_Geometry(0) as ISegmentCollection;
                ipSelCol.RemoveSegments(ipSelCol.SegmentCount - 1, 1, false);
                ILine ipLine = new LineClass();
                ipLine.PutCoords(ipSelCol.get_Segment(ipSelCol.SegmentCount - 1).ToPoint, ipPt);
                ISegment ipSeg = ipLine as ISegment;
                ipSelCol.AddSegment(ipSeg, ref obj, ref obj);
                ipLine = new LineClass();
                ipLine.PutCoords(ipPt, ipSelCol.get_Segment(0).FromPoint);
                ipSeg = ipLine as ISegment;
                ipSelCol.AddSegment(ipSeg, ref obj, ref obj);
            }

            ////////��ʾ/////////////
            IClone ipClone = this.m_GeoMeasure as IClone;
            IGeometry ipGeo = ipClone.Clone() as IGeometry;
            this.m_Element.Geometry = ipGeo;
            //�ֲ�ˢ��
            ipAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, ipGeo, null);
            //ipAV.Refresh();

            this.m_FormDis.WriteLabelText(ipGeo);

            this.m_FormDis.m_timer.Start();
             */
        }


        /// <summary>
        /// ��׽����׽��
        /// </summary>
        /// <param name="pSnapPnt"></param>
        /// <returns></returns>
        public bool SnapPoint(IPoint pSnapPnt)
        {
            IEngineSnapEnvironment pSnapEnv = new EngineEditorClass();
            return pSnapEnv.SnapPoint(pSnapPnt);
        }


        /// <summary>
        /// ����ƶ����ڴ˶�̬��m_GeoMeasure�м���������ڵĵ���ж�̬����
        /// </summary>
        /// <param name="Button"></param>
        /// <param name="Shift"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add ToolMeasureArea.OnMouseMove implementation
            if (this.m_GeoMeasure == null)
                return;

            IActiveView ipAV = this.m_hookHelper.ActiveView;
            ///////////�����µ�///////////////
            IPoint ipPt = new PointClass();
            ipPt = ipAV.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            SnapPoint(ipPt);
            if (m_pNewPolygonFeed != null)
            {
                m_pNewPolygonFeed.MoveTo(ipPt);
            }

            /*
            ///////������ͼ��/////////
            IClone ipClone = this.m_GeoMeasure as IClone;
            IGeometry ipGeo = ipClone.Clone() as IGeometry;
            object obj = Type.Missing;

            IGeometryCollection ipGeoCol = ipGeo as IGeometryCollection;
            ISegmentCollection ipSelCol = ipGeoCol.get_Geometry(0) as ISegmentCollection;
            ipSelCol.RemoveSegments(ipSelCol.SegmentCount - 1, 1, false);
            ILine ipLine = new LineClass();
            ipLine.PutCoords(ipSelCol.get_Segment(ipSelCol.SegmentCount - 1).ToPoint, ipPt);
            ISegment ipSeg = ipLine as ISegment;
            ipSelCol.AddSegment(ipSeg, ref obj, ref obj);
            ipLine = new LineClass();
            ipLine.PutCoords(ipPt, ipSelCol.get_Segment(0).FromPoint);
            ipSeg = ipLine as ISegment;
            ipSelCol.AddSegment(ipSeg, ref obj, ref obj);

            ////////��ʾ/////////////
            this.m_Element.Geometry = ipGeo;
            //�ֲ�ˢ��
            ipAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, ipGeo, null);
            //ipAV.Refresh();
            */
        }

        public override void OnKeyDown(int keyCode, int Shift)
        {
            if (keyCode.Equals(27))
            {
                m_GeoMeasure = null;
                m_pNewPolygonFeed = null;
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, m_hookHelper.ActiveView.Extent);
            }
        }
        /// <summary>
        /// ���˫���������������⣬������һ�����⣬�������йرն�ʱ��
        /// </summary>
        public override void OnDblClick()
        {
            //base.OnDblClick();

            //this.m_GeoMeasure = null;
            //this.m_Element.Geometry = new PolygonClass();
            //this.m_hookHelper.ActiveView.Refresh();
            //this.m_FormDis.WriteLabelText(this.m_GeoMeasure);

            //this.m_FormDis.m_timer.Stop();

            if (this.m_GeoMeasure == null)
                return;

            //�����ʾ����
            if (this.m_FormDis == null)
            {
                MeasureType type = MeasureType.Area;
                this.m_FormDis = new FormDis(type, this);
                this.m_FormDis.TopMost = true;
                //������ʾλ��
                this.m_FormDis.Location = new System.Drawing.Point(130, 150);
                this.m_FormDis.Show();

            }

            this.m_GeoMeasure = m_pNewPolygonFeed.Stop();
            this.m_FormDis.WriteLabelText(this.m_GeoMeasure);

            this.m_GeoMeasure = null;

        }

        /// <summary>
        /// ��ʼ����Ϣ������IElementʵ�����˻�����������ͼ�Σ����趨����ʾ���ţ��رն�ʱ��
        /// </summary>
        public void MyInit()
        {
            //��ʾͼ������
            ISimpleFillSymbol ipSimpleFillSymbol = new SimpleFillSymbolClass();
            ipSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
            IRgbColor ipColor = new RgbColor();
            ipColor.Red = 0;
            ipColor.Green = 255;
            ipColor.Blue = 0;
            ipSimpleFillSymbol.Color = ipColor;
            ILineSymbol ipLineSymple = new SimpleLineSymbolClass();
            ipLineSymple.Width = 2;
            ipLineSymple.Color = ipColor;
            ipSimpleFillSymbol.Outline = ipLineSymple;
            this.m_FillSymbol = ipSimpleFillSymbol as IFillSymbol;

            //����ʵ��
            IFillShapeElement ipFillShapeElement = new PolygonElementClass();
            ipFillShapeElement.Symbol = this.m_FillSymbol;
            this.m_Element = ipFillShapeElement as IElement;
            IGraphicsContainer ipGraphicContainer = this.m_hookHelper.ActiveView.GraphicsContainer;
            ipGraphicContainer.AddElement(this.m_Element, 0);
        }

        public override bool Enabled
        {
            get
            {
                if (m_hookHelper.FocusMap.LayerCount == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }

                //return CAuditApplication.m_CurrentAuditTask != null;
            }
        }
        #endregion
    }
}