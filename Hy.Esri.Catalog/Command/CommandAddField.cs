﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Hy.Esri.Catalog.Command
{
    /// <summary>
    /// Command that works in ArcGlobe or GlobeControl
    /// </summary>
    [Guid("12ddfb9b-6d69-430e-82a9-b204b8e78266")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("Command.CommandAddField")]
    public sealed class CommandAddField : BaseCommand
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
            GMxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GMxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IGlobeHookHelper m_globeHookHelper = null;

        public CommandAddField()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "3D数据管理"; //localizable text
            base.m_caption = "添加自定义属性字段";  //localizable text
            base.m_message = "";  //localizable text 
            base.m_toolTip = "添加自定义属性字段";  //localizable text
            base.m_name = "添加自定义属性字段";   //unique id, non-localizable (e.g. "MyCategory_MyCommand")

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_globeHookHelper = new GlobeHookHelperClass();
                m_globeHookHelper.Hook = hook;
                if (m_globeHookHelper.ActiveViewer == null)
                {
                    m_globeHookHelper = null;
                }
            }
            catch
            {
                m_globeHookHelper = null;
            }

            if (m_globeHookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            ILayer lyr= (m_globeHookHelper.Globe as IMap).get_Layer(0);
            if (lyr is IFeatureLayer)
            {
                UI.FrmClassFields frmAddFields = new UI.FrmClassFields();
                frmAddFields.TargetClass = (lyr as IFeatureLayer).FeatureClass as IClass;
                frmAddFields.ShowDialog();
            }
        }

        #endregion
    }
}
