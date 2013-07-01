using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using IWorkspaceFactory=ESRI.ArcGIS.Geodatabase.IWorkspaceFactory;

namespace Check.Rule.Helper
{
    /// <summary>
    /// �漰��Engine������һЩ���ú�����
    /// </summary>
    public class TopoHelper
    {
        /* -------------------------------------------------------------------- */
        /*                            ���˹������ƶ���                          */
        /* -------------------------------------------------------------------- */


        /// <summary>
        /// ��Ҫ�����˹���
        /// </summary>
        //1
        public const string TopoName_PointCoveredByLineEndpoint = "��Ҫ�ر��뱻�ߵĶ˵㸲��";
        //2
        public const string TopoName_PointCoveredByLine = "����뱻�߸���";
        //3
        public const string TopoName_PointProperlyInsideArea = "�������ȫλ�����ڲ�";
        //4
        public const string TopoName_PointCoveredByAreaBoundary = "����뱻��߽縲��";

        //---------------��Ҫ�����˹���-------
        //1
        public const string TopoName_LineNoOverlap = "�߲���Ҫ�ز������ص�";
        //2
        public const string TopoName_LineNoIntersection = "�߲���Ҫ�ز������ཻ";
        //3
        public const string TopoName_LineNoDangles = "�߲���Ҫ��û�����ҽڵ�";
        //4
        public const string TopoName_LineNoPseudos = "�߲���Ҫ��û��α�ڵ�";
        //5-
        public const string TopoName_LineNoIntersectOrInteriorTouch = "�߲���Ҫ�ز����ཻ���ڲ�����";
        //6
        public const string TopoName_LineNoOverlapLine = "�߲����߲��Ҫ�ز��໥�ص�";
        //7
        public const string TopoName_LineCoveredByLineClass = "�߲�Ҫ�ر��뱻��һ�߲�Ҫ�ظ���";
        //8
        public const string TopoName_LineCoveredByAreaBoundary = "�߲���뱻���߽縲��";
        //9
        public const string TopoName_LineEndpointCoveredByPoint = "�߲�Ҫ�ض˵���뱻��㸲��";
        //10
        public const string TopoName_LineNoSelfOverlap = "�߲���Ҫ�ز����ص�";
        //11
        public const string TopoName_LineNoSelfIntersect = "�߲���Ҫ�ز����ཻ";
        //12-
        public const string TopoName_LineNoMultipart = "�߲���Ҫ�ر���Ϊ������";

        //---------------��Ҫ�����˹���---------
        //1
        public const string TopoName_AreaNoOverlap = "�����Ҫ�ز��໥�ص�";
        //2
        public const string TopoName_AreaNoGaps = "�����Ҫ��֮��û�з�϶";
        //3
        public const string TopoName_AreaNoOverlapArea = "��㲻����һ����ص�";
        //4
        public const string TopoName_AreaCoveredByAreaClass = "�����Ҫ�ر���һ�����Ҫ�ظ���";
        //5
        public const string TopoName_AreaAreaCoverEachOther = "������һ������໥����";
        //6-
        public const string TopoName_AreaCoveredByArea = "�����뱻��һ����㸲��";
        //7-
        public const string TopoName_AreaBoundaryCoveredByLine = "���߽类��һ���߲㸲��";
        //8
        public const string TopoName_AreaBoundaryCoveredByAreaBoundary = "������һ���߽�һ��";
        //9
        public const string TopoName_AreaContainPoint = "���������Ҫ��";


        /// <summary>
        /// ���ݱ�ϵͳ�ֶ����ͱ��ת���õ�esri�ֶ�����
        /// </summary>
        /// <param name="nFldType"></param>
        /// <returns></returns>
        public static esriFieldType en_GetEsriFieldByEnum(int nFldType)
        {
            esriFieldType esriFldType = esriFieldType.esriFieldTypeSmallInteger;

            switch (nFldType)
            {
                case 1:
                    {
                        esriFldType = esriFieldType.esriFieldTypeOID;
                        break;
                    }
                case 2:
                    {
                        esriFldType = esriFieldType.esriFieldTypeInteger;
                        break;
                    }
                case 3:
                    {
                        esriFldType = esriFieldType.esriFieldTypeSingle;
                        break;
                    }
                case 4:
                    {
                        esriFldType = esriFieldType.esriFieldTypeDouble;
                        break;
                    }
                case 5:
                    {
                        esriFldType = esriFieldType.esriFieldTypeString;
                        break;
                    }
                case 6:
                    {
                        esriFldType = esriFieldType.esriFieldTypeDate;
                        break;
                    }
                case 8:
                    {
                        esriFldType = esriFieldType.esriFieldTypeBlob;
                        break;
                    }
            }

            return esriFldType;
        }

        /// <summary>
        /// ����esri�ֶ�����ת���õ��ֶ�����
        /// </summary>
        /// <param name="esriFldType"></param>
        /// <returns></returns>
        public static string en_GetFieldTypebyEsriField(esriFieldType esriFldType)
        {
            string strFldType = "δ֪����";

            switch (esriFldType)
            {
                case esriFieldType.esriFieldTypeOID:
                    {
                        strFldType = "Ψһ��־������";
                        break;
                    }
                case esriFieldType.esriFieldTypeInteger:
                    {
                        strFldType = "����";
                        break;
                    }
                case esriFieldType.esriFieldTypeSingle:
                    {
                        strFldType = "�����ȸ�����";
                        break;
                    }
                case esriFieldType.esriFieldTypeDouble:
                    {
                        strFldType = "˫���ȸ�����";
                        break;
                    }
                case esriFieldType.esriFieldTypeString:
                    {
                        strFldType = "�ַ���";
                        break;
                    }
                case esriFieldType.esriFieldTypeDate:
                    {
                        strFldType = "������";
                        break;
                    }
                case esriFieldType.esriFieldTypeBlob:
                    {
                        strFldType = "�����������";
                        break;
                    }
            }

            return strFldType;
        }

        /// <summary>
        /// ����esri�ֶ����ͣ��ж�sql����ѯ�Ƿ���Ҫ''
        /// </summary>
        /// <param name="esriFldType"></param>
        /// <returns>����true,��˵����string�ͣ���Ҫ''</returns>
        public static bool en_GetTypebyEsriField(esriFieldType esriFldType)
        {
            bool bTest = true;

            switch (esriFldType)
            {
                case esriFieldType.esriFieldTypeOID:
                case esriFieldType.esriFieldTypeInteger:
                case esriFieldType.esriFieldTypeSingle:
                case esriFieldType.esriFieldTypeDouble:
                    {
                        bTest = false;
                        break;
                    }
                case esriFieldType.esriFieldTypeString:
                case esriFieldType.esriFieldTypeDate:
                case esriFieldType.esriFieldTypeBlob:
                    {
                        bTest = true;
                        break;
                    }
            }

            return bTest;
        }

    }
}