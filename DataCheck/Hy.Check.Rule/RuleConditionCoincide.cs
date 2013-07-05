using System;
using System.Collections.Generic;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Hy.Check.Define;

using System.Runtime.InteropServices;

namespace Hy.Check.Rule
{
    /// <summary>
    /// �ù������ж�����ĳһ������ĳһͼ���Ҫ���Ƿ����һͼ���Ҫ���غ�
    /// </summary>
    public class RuleConditionCoincide : BaseRule
    {
        public override string Name
        {
            get { return "�����ռ��غ��ʼ����"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            m_structPara = new RuleExpression.CONDITIONCOINCIDEPARA();
        }

        public override bool Verify()
        {
            if (m_structPara.strFtName == "" || m_structPara.strFtName2 == "" || m_structPara.strWhereClause == "")
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ�ļ��Ŀ�������ʽ�����ڣ��޷�ִ�м��!");
                return false;
            }

            //��ȡ��Ҫ���пռ��ϵ��ѯ��ILayer
            IFeatureWorkspace ipFtWS = (IFeatureWorkspace)m_BaseWorkspace;

            //�õ�Ŀ��ͼ��͹�ϵͼ���featureclass
            try
            {
                pSrcFeatClass = ipFtWS.OpenFeatureClass(m_structPara.strFtName);
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ��ͼ�㡰" + m_structPara.strFtName + "��������,�޷�ִ�м��!");
                return false;
            }

            try
            {
                pRelFeatClass = ipFtWS.OpenFeatureClass(m_structPara.strFtName2);
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.VerifyError, "��ǰ�������ݿ��ͼ�㡰" + m_structPara.strFtName2 + "��������,�޷�ִ�м��!");
                return false;
            }

            return true;
        }

        private IFeatureClass pSrcFeatClass = null;
        private IFeatureClass pRelFeatClass = null;
        public override bool Check(ref List<Error> checkResult)
        {
            IQueryFilter pQueryFilter = new QueryFilterClass();
            pQueryFilter.WhereClause = m_structPara.strWhereClause;
            //����������ѯ
            IFeatureCursor ipFeatCursor = pSrcFeatClass.Search(pQueryFilter, true);
            IFeature ipFeature = ipFeatCursor.NextFeature();
            IGeometryCollection pGeometryCollection = new GeometryBagClass();
            ///��ȡ�����������geometry
            while (ipFeature != null)
            {
                IGeometry ipGeometry = ipFeature.Shape;
                if (ipGeometry == null)
                {
                    ipFeature = ipFeatCursor.NextFeature();
                    continue;
                }
                object Missing = Type.Missing;
                pGeometryCollection.AddGeometry(ipGeometry, ref Missing, ref Missing);

                ipFeature = ipFeatCursor.NextFeature();
            }

            ISpatialIndex pSpatialIndex = (ISpatialIndex)pGeometryCollection;
            pSpatialIndex.AllowIndexing = true;
            pSpatialIndex.Invalidate();

            ///��������ͼ������ص��Ŀռ��ѯ
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelOverlaps;
            ///�����GeometryCollection����spatialfilter
            pSpatialFilter.Geometry = (IGeometry)pGeometryCollection;
            string Fields = "OBJECTID,Shape";
            pSpatialFilter.SubFields = Fields;

            IFeatureCursor ipResultFtCur = pRelFeatClass.Search(pSpatialFilter, true);

            //�������� 
            List<Error> pRuleResult = new List<Error>();
            AddResult(ref pRuleResult, ipResultFtCur);

            checkResult = pRuleResult;

            if (ipResultFtCur != null)
            {
                Marshal.ReleaseComObject(ipResultFtCur);
                ipResultFtCur = null;
            } if (pSrcFeatClass != null)
            {
                Marshal.ReleaseComObject(pSrcFeatClass);
                pSrcFeatClass = null;
            }
            if (pRelFeatClass != null)
            {
                Marshal.ReleaseComObject(pRelFeatClass);
                pRelFeatClass = null;
            }
            return true;

        }
     

        /// <summary>
        /// �����غ��ʼ����ṹ��
        /// </summary>
        private RuleExpression.CONDITIONCOINCIDEPARA m_structPara = new RuleExpression.CONDITIONCOINCIDEPARA();

       

        /// <summary>
        ///  ��ȡ�����
        /// </summary>
        /// <param name="pRuleResult"></param>
        /// <param name="pFeatCursor"></param>
        private void AddResult(ref List<Error> pRuleResult, IFeatureCursor pFeatCursor)
        {
            if (pFeatCursor == null)
            {
                return;
            }

            try
            {
                IFeature ipFeature = pFeatCursor.NextFeature();
                while (ipFeature != null)
                {
                    // ��ҽ����¼
                    Error pResInfo = new Error();
                    pResInfo.DefectLevel = this.m_DefectLevel;
                    pResInfo.RuleID = this.InstanceID;

                    // OID
                    pResInfo.OID = ipFeature.OID;
                    // Ŀ��ͼ��
                    pResInfo.LayerName = m_structPara.strFtName2;
                    pResInfo.Description =m_structPara.strErrorReason;
                    pRuleResult.Add(pResInfo);

                    ipFeature = pFeatCursor.NextFeature();
                }
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return;
            }
        }



    }
}