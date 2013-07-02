using System;
using System.Collections.Generic;
using System.Windows.Forms;
//using DevExpress.LocalizationCHS;
using DevExpress.XtraBars.Localization;
using DevExpress.XtraCharts.Localization;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Localization;
using DevExpress.XtraTreeList.Localization;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using Common.UI;
using Check.Command;
using Check.Demo.Helper;

namespace Check.Demo
{
    public partial class RibbonFrmMain : RibbonForm
    {
        //private UcMapCheck m_UcMapCheck;
        private CmdDevExpressAdapter m_CmdDevExpressAdapter;
        //private SystemHotKeyManager m_SystemHotKeyManager;

        //private UCMapControl ucPropertyErrMap;
        //private UCMapControl ucTopoErrMap;
        //private UCMapControl m_UCDataMap;


        private readonly string[] strprogids =
        {
            //�������
            "Check.Command.CustomCommand.CreateTaskCommand",
             "Check.Command.CustomCommand.OpenTaskCommand",
             "Check.Command.CustomCommand.BatchCreateTaskCommand",
             "Check.Command.CustomCommand.ExcuteTaskCommand",
             "Check.Command.CustomCommand.CheckWorkFlowCommand",
             "Check.Command.CustomCommand.PreCheckCommand",
             "Check.Command.CustomCommand.CheckErrorsEvaluate",
             "Check.Command.CustomCommand.SqlQueryCommand",
             "Check.Command.CustomCommand.SystemHelpCommand",
             "Check.Command.CustomCommand.ExportErrosToExcelCommand",
             "Check.Command.CustomCommand.ExportErrorRecordCommand",
             "Check.Command.CustomCommand.ExportToVCTCommand",
             "Check.Command.CustomCommand.ViewTaskCheckLogCommand",
             "Check.Command.CustomCommand.ViewFeaturesStatisticCommand",

            //��ͼ����
            "Check.Command.CustomCommand.Identify",
            "Check.Command.MeasureCommand.ToolMeasureArea",
            "Check.Command.MeasureCommand.ToolMeasureLength",
            "Check.Command.CustomCommand.MapIdentifyCommand",
            "Check.Command.CustomCommand.ClearSelectionCommand"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonFrmMain"/> class.
        /// </summary>
        public RibbonFrmMain()
        {
            InitializeComponent();

            InitForm();
            SplashScreen.CloseForm();
        }

        /// <summary>
        /// ��ʼ��������
        /// </summary>
        private void InitForm()
        {
            LocalizeSystem();

            //CheckApplication.CurrentTask = new Check.Task.Task();
            //���������ͼUC
            CheckApplication.m_UCDataMap = new Check.UI.UC.UCMapControl();

            CheckApplication.m_UCDataMap.MapMouseMove += ucTopoErrMap_MouseMove;
            CheckApplication.m_UCDataMap.InitUc();
            CheckApplication.m_UCDataMap.Dock = DockStyle.Fill;
            pnlMapCheck.Controls.Add(CheckApplication.m_UCDataMap);
            pnlMapCheck.Dock = DockStyle.Fill;
            pnlMapCheck.Visible = true;

            ribbon.SelectedPage = rpDbCheck;
            //ע��cmd
            m_CmdDevExpressAdapter = new CmdDevExpressAdapter();
            m_CmdDevExpressAdapter.ToolbarControl = CheckApplication.m_UCDataMap.ToolbarControl;
            m_CmdDevExpressAdapter.RibbonCtrl = ribbon;
            m_CmdDevExpressAdapter.AddCommands(strprogids);
            m_CmdDevExpressAdapter.UpdateToolbar();

            //m_SystemHotKeyManager = new SystemHotKeyManager();
            //CCheckApplication.m_arrAllButtonItem = m_SystemHotKeyManager.BuilderSystemHotKey(1,this.ribbon);
        }

        /// <summary>
        /// ���ػ�DevExpress�ؼ�
        /// </summary>
        private static void LocalizeSystem()
        {
            //BarLocalizer.Active = new DevExpressXtraBarsLocalizationCHS();
            //Localizer.Active = new DevExpressXtraEditorsLocalizationCHS();
            //GridLocalizer.Active = new DevExpressXtraGridLocalizationCHS();
            //BarLocalizer.Active = new DevExpressXtraBarsLocalizationCHS();
            //TreeListLocalizer.Active = new DevExpressXtraTreeListLocalizationCHS();
        }

        private void ucTopoErrMap_MouseMove(IPoint pPoint)
        {
            barStaticXY.Caption = string.Format("X���꣺{0}, Y���꣺{1}", pPoint.X.ToString("f3"), pPoint.Y.ToString("f3"));
        }

        private void RibbonFrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// ִ���������������չ���
        /// </summary>
        private void PostTaskWork()
        {
            //if (CCheckApplication.ucPropertyErrMap != null)
            //{
            //    CCheckApplication.ucPropertyErrMap.SetTask(null);
            //    //CCheckApplication.ucTopoErrMap.SetTask(null);
            //}
            //�ڴ����
            //GC.Collect();
        }

        private void barButtonItem20_ItemClick(object sender, ItemClickEventArgs e)
        {
     
        }

        private void barButtonItem21_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private void RibbonFrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            PostTaskWork();
        }
    }
}