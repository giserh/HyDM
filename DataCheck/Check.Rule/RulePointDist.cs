using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Check.Define;

namespace Check.Rule
{
    /// <summary>
    /// 2012-02-20 �ź��� ���ע��
    /// �����ݲβ����Ϊ1����0.02��ʱ������Ĵ����������ٱ�����
    /// ���⣬�������������ڲ�ͬ�ռ�ο������ߵ��ռ�ο��ĵ�λ��ͬʱ��������ʱ�����޸�ϵͳ��ġ��ݲֵ
    /// </summary>
    public class RulePointDist : BaseRule
    {

        /// <summary>
        /// OID��Feature���չ�ϣ��
        /// </summary>
        private Hashtable m_MapOIDFeature1 = new Hashtable();

        /// <summary>
        /// OID��Feature���չ�ϣ��
        /// </summary>
        private Hashtable m_MapOIDFeature2 = new Hashtable();

        /// <summary>
        /// ����ֵ��OID�Ķ��չ�ϣ������ֵ���á�x_y���ķ�ʽ��x��y��������λ�ض�
        /// </summary>
        private Hashtable m_PointtoOID = new Hashtable();

        /// <summary>
        /// �ظ����֮��ļ�ֵ��ϣ��,keyֵΪ��x_y����valueֵΪx_y�ظ���oid�ַ���
        /// </summary>
        private Hashtable m_RepeatOIDtoOID = new Hashtable();

        ///// <summary>
        ///// Դͼ��ļ������� 
        ///// </summary>
        //private int m_nGeoType;

        ///// <summary>
        ///// ���ߺ����ɢ�ĵ�ͼ�������
        ///// </summary>
        //private string m_PointLayerName;

        /// <summary>
        ///  ������ͼ�� 
        /// </summary>
        private IFeatureClass m_ipBufferFeaCls;

        /// <summary>
        /// ���
        /// </summary>
        private List<PointDistInfo> m_aryResult; //C++��ΪCArray<PointDistInfo,PointDistInfo> m_aryResult        


        ///// <summary>
        ///// ������ͼ����
        ///// </summary>
        //private string m_strBufferLayer;

        private RuleExpression.LRPointDistPara m_pPara;

        private string m_strSrcLayer = null;
        private string m_strName;
        private IFeatureClass pSrcFeatClass = null;

        public struct PointDistInfo
        {
            public int OID1;
            public int OID2;
            public double dDistance;
        } ;

        public struct PointXY
        {
            public double dx;
            public double dy;
        } ;


        /// <summary>
        /// Initializes a new instance of the <see cref="RulePointDist"/> class.
        /// </summary>
        public RulePointDist()
        {
            m_pPara = null;
            m_strName = "��������ʼ����";
        }

        /// <summary>
        /// ��ȡͼ��ļ�������
        /// </summary>
        /// <param name="ipSrcFeaCls"></param>
        /// <returns></returns>
        private int GetLayerGeoType(IFeatureClass ipSrcFeaCls)
        {
            int nGeoType = 0;

            esriGeometryType Type = ipSrcFeaCls.ShapeType;
            switch (Type)
            {
                case esriGeometryType.esriGeometryPoint:
                case esriGeometryType.esriGeometryMultipoint:
                    nGeoType = 1;
                    break;
                case esriGeometryType.esriGeometryLine:
                case esriGeometryType.esriGeometryPolyline:
                    nGeoType = 2;
                    break;
                case esriGeometryType.esriGeometryRing:
                case esriGeometryType.esriGeometryPolygon:
                case esriGeometryType.esriGeometryEnvelope:
                    nGeoType = 3;
                    break;
                default:
                    break;
            }

            return nGeoType;
        }


    
        /// <summary>
        /// ����λ�ڻ������еĵ�Ҫ��- ʹ��ISpatialFilter
        /// </summary>
        /// <param name="ipPtFeaCls">��ͼ��</param>
        /// <param name="ipBufferFeaCls">������ͼ��</param>
        /// <returns></returns>
        private bool SearchSpatialFilter(IFeatureClass ipPtFeaCls,
                                         IFeatureClass ipBufferFeaCls)
        {
            if (ipBufferFeaCls == null || ipPtFeaCls == null) return false;

            ISpatialFilter pSpatialFilter = new SpatialFilterClass(); //(CLSID_SpatialFilter);

            // 1. ���ÿռ��ϵ
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;


            // 2.���ù��˼���
            string shapeFieldName = ipPtFeaCls.ShapeFieldName;
            pSpatialFilter.GeometryField = shapeFieldName;


            // ���cursor
            IFeatureCursor ipResultFtCur = null;

            // ����Դͼ��Ҫ��
            IFeatureCursor ipFeatCursor;
            IFeature ipFeature;

            IQueryFilter pQueryFilter = new QueryFilterClass(); //(CLSID_QueryFilter);
            string SubFields = "OID,Shape";
            pQueryFilter.SubFields = SubFields;
            ipFeatCursor = ipBufferFeaCls.Search(pQueryFilter, true);
            ipFeature = ipFeatCursor.NextFeature();

            m_aryResult = new List<PointDistInfo>();
            while (ipFeature != null)
            {
                IGeometry ipGeometry1, ipGeometry2;
                int OID1, OID2;
                ipGeometry1 = ipFeature.Shape;
                OID1 = ipFeature.OID;

                if (ipGeometry1 == null)
                {
                    ipFeature = ipFeatCursor.NextFeature();
                    continue;
                }

                pSpatialFilter.Geometry = ipGeometry1;

                if (ipResultFtCur != null)
                {
                    Marshal.ReleaseComObject(ipResultFtCur);
                }
                ipResultFtCur = ipPtFeaCls.Search(pSpatialFilter, true);


                // ��ӵ��������
                if (ipResultFtCur == null)
                {
                    ipFeature = ipFeatCursor.NextFeature();
                    continue;
                }

                IFeature ipResFeature = ipResultFtCur.NextFeature();


                while (ipResFeature != null)
                {
                    OID2 = ipResFeature.OID;

                    // ID��ͬ��˵����ͬһ���㡣ID1:��������ID2:�����ڻ������еĵ�
                    if (OID1 == OID2)
                    {
                        ipResFeature = ipResultFtCur.NextFeature();
                        continue;
                    }

                    // ��ҽ����¼
                    PointDistInfo PtInfo;
                    // OID1
                    PtInfo.OID1 = OID1;
                    // OID2
                    PtInfo.OID2 = OID2;

                    IFeature ipFeat = null;
                    //if (OID1 == 58)
                    //{
                    //   //XtraMessageBox.Show("RulePointDist:SearchSpatialFiler :OIDΪ58�ĵ����޷���ȡ������ԭ��");
                    //   break;

                    //}

                    try
                    {
                        ipFeat = ipPtFeaCls.GetFeature(OID1);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, "��ȡҪ�س���"+ex.ToString());
                        break;
                    }

                    if (ipFeat != null)
                    {
                        IGeometry ipPtGeo1 = ipFeat.Shape;
                        ipGeometry2 = ipResFeature.Shape;

                        // ����
                        PtInfo.dDistance = CalDist(ipPtGeo1, ipGeometry2);

                        // �Ƿ������ͬ�㣬���Ƿ���Ӿ���Ϊ0�ĵ�
                        if (m_pPara.bSearchSamePt ||
                            (!m_pPara.bSearchSamePt && PtInfo.dDistance != 0))
                        {
                            m_aryResult.Add(PtInfo);
                        }
                    }

                    ipResFeature = ipResultFtCur.NextFeature();
                }

                ipFeature = ipFeatCursor.NextFeature();
            }

            return true;
        }


        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="ipGeo1"></param>
        /// <param name="ipGeo2"></param>
        /// <returns></returns>
        private double CalDist(IGeometry ipGeo1, IGeometry ipGeo2)
        {
            if (ipGeo1 == null || ipGeo1 == null)
                return -1;

            IPoint ipPt1 = (IPoint)ipGeo1;
            IPoint ipPt2 = (IPoint)ipGeo2;

            if (ipPt1 == null || ipPt2 == null)
                return -1;

            double x1, y1, x2, y2;
            x1 = ipPt1.X;
            y1 = ipPt1.Y;
            x2 = ipPt2.X;
            y2 = ipPt2.Y;

            double dDist = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);

            return Math.Sqrt(dDist);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipPtFeaCls"></param>
        /// <returns></returns>
        private bool SearchPoints(IFeatureClass ipPtFeaCls)
        {
            if (ipPtFeaCls == null) return false;
            try
            {
                // ����Դͼ��Ҫ��
                IQueryFilter ipQuery = new QueryFilterClass();
                ipQuery.SubFields = "OBJECTID,Shape,BSM";
                IFeatureCursor ipFeatCursor = ipPtFeaCls.Search(ipQuery, true);
                IFeature ipFeature = ipFeatCursor.NextFeature();

                IGeometry ipGeometry = null;
                int OID1 = -1;
                m_aryResult = new List<PointDistInfo>();
                int iDecimal = 0;

                int iParmer = GetParameter(m_pPara.dPointDist, out iDecimal);
                while (ipFeature != null)
                {
                    ipGeometry = ipFeature.Shape;
                    OID1 = ipFeature.OID;
                    if (ipGeometry == null)
                    {
                        ipFeature = ipFeatCursor.NextFeature();
                        continue;
                    }
                    else
                    {
                        if (ipGeometry.GeometryType == esriGeometryType.esriGeometryPoint ||
                            ipGeometry.GeometryType == esriGeometryType.esriGeometryMultipoint)
                        {
                            IPoint ipT = (IPoint)ipGeometry;
                            int dx = Convert.ToInt32(Math.Round(ipT.X, iDecimal) * iParmer);
                            int dy = Convert.ToInt32(Math.Round(ipT.Y, iDecimal) * iParmer);
                            string str = dx.ToString() + "_" + dy.ToString();
                            if (!m_PointtoOID.Contains(str))
                            //����һhashtable���Ե�ġ�x_y��������ɵ��ַ���Ϊkey�������У�x��y��ȡһλС����������������Ĳ�������"<=0.1"��Ч,�����x_y��keyֵ��ͬ������������϶�>=0.1
                            {
                                m_PointtoOID.Add(str, OID1);
                            }
                            else
                            {
                                object OID = m_PointtoOID[str];
                                IFeature OriginFeature = ipPtFeaCls.GetFeature((int)OID);
                                IPoint OriginPoint = (IPoint)OriginFeature.Shape;
                                // ��������
                                double dDist = (ipT.X - OriginPoint.X) * (ipT.X - OriginPoint.X) +
                                               (ipT.Y - OriginPoint.Y) * (ipT.Y - OriginPoint.Y);
                                ///���Ҵ�������
                                if ((int)OID != OID1 && Math.Round(Math.Sqrt(dDist), 2) < m_pPara.dPointDist)
                                {
                                    //m_OIDtoOID.Add(OID1, OID);

                                    if (!m_RepeatOIDtoOID.Contains(str))
                                    {
                                        string strTemp = OID1.ToString();
                                        m_RepeatOIDtoOID.Add(str, strTemp);
                                    }
                                    else
                                    {
                                        string strTemp = m_RepeatOIDtoOID[str].ToString() + "," + OID1;
                                        m_RepeatOIDtoOID[str] = strTemp;
                                    }

                                    //// ��ҽ����¼
                                    //PointDistInfo PtInfo = new PointDistInfo();

                                    //PtInfo.dDistance = Math.Round(Math.Sqrt(dDist), 2);

                                    //PtInfo.OID1 = Convert.ToInt32(OID1);
                                    //PtInfo.OID2 = Convert.ToInt32(OID);

                                    //m_aryResult.Add(PtInfo);
                                }
                            }
                        }
                    }
                    ipFeature = ipFeatCursor.NextFeature();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        //���ݾ�γ������͵�������Ĳ�ͬ���ò�ͬ����������
        //2009-714
        private int GetParameter(double par1, out int iDecimal)
        {
            int iReturn = 0;
            int n = 1;
            while (par1 * 10 < 1)
            {
                par1 *= 10;
                n++;
            }
            iDecimal = n;
            iReturn = Convert.ToInt32(Math.Pow(10, iDecimal));
            return iReturn;
        }

        /// <summary>
        /// ֱ��ʹ��Ƕ��ѭ���ķ�����������������˵�ǡ���������
        /// </summary>
        /// <param name="ipPtFeaCls">��ͼ��</param>
        /// <returns></returns>
        private bool SearchStupid(IFeatureClass ipPtFeaCls)
        {
            if (ipPtFeaCls == null) return false;

            // ����Դͼ��Ҫ��

            IQueryFilter ipQuery = new QueryFilterClass();
            ipQuery.SubFields = "OBJECTID,Shape";
            IFeatureCursor ipFeatCursor = ipPtFeaCls.Search(ipQuery, true);
            IFeature ipFeature = ipFeatCursor.NextFeature();

            IGeometry ipGeometry = null;
            int OID1 = -1;
            string str;

            while (ipFeature != null)
            {
                ipGeometry = ipFeature.Shape;
                OID1 = ipFeature.OID;
                if (ipGeometry == null)
                {
                    ipFeature = ipFeatCursor.NextFeature();
                    continue;
                }
                else
                {
                    if (ipGeometry.GeometryType == esriGeometryType.esriGeometryPoint ||
                        ipGeometry.GeometryType == esriGeometryType.esriGeometryMultipoint)
                    {
                        PointXY pt = new PointXY();
                        IPoint ipT = (IPoint)ipGeometry;
                        pt.dx = ipT.X;
                        pt.dy = ipT.Y;
                        str = OID1.ToString();
                        m_MapOIDFeature1.Add(str, pt);
                        m_MapOIDFeature2.Add(str, pt);
                    }
                }
                Marshal.ReleaseComObject(ipFeature);
                ipFeature = ipFeatCursor.NextFeature();
            }


            string rsOID, rsOID2;
            PointXY rsGeometry;
            PointXY rsGeometry2;
            m_aryResult = new List<PointDistInfo>();
            foreach (DictionaryEntry dn in m_MapOIDFeature1)
            {
                rsOID = (string)dn.Key;
                rsGeometry = (PointXY)dn.Value;
                foreach (DictionaryEntry dn2 in m_MapOIDFeature2)
                {
                    rsOID2 = (string)dn2.Key;
                    rsGeometry2 = (PointXY)dn2.Value;

                    // ��������
                    double dDist = (rsGeometry2.dx - rsGeometry.dx) * (rsGeometry2.dx - rsGeometry.dx) +
                                   (rsGeometry2.dy - rsGeometry.dy) * (rsGeometry2.dy - rsGeometry.dy);

                    if (rsOID != rsOID2 && Math.Sqrt(dDist) < m_pPara.dPointDist)
                    {
                        // ��ҽ����¼
                        PointDistInfo PtInfo = new PointDistInfo();

                        PtInfo.dDistance = Math.Round(Math.Sqrt(dDist), 2);

                        PtInfo.OID1 = Convert.ToInt32(rsOID);
                        PtInfo.OID2 = Convert.ToInt32(rsOID2);
                        m_aryResult.Add(PtInfo);
                        break;
                    }
                }
            }


            ClearMap();

            return true;
        }

        /// <summary>
        ///  ��������
        /// </summary>
        /// <returns></returns>
        private bool SaveData(ref List<Error> pResult)
        {
            try
            {
                string strSql;
                // �ж���ͼ��ļ������ͣ��㡢�ߡ���
                int nGeoType = GetLayerGeoType(pSrcFeatClass);
                // �����д��Ŀ�������
                if (nGeoType == 1) // ��
                {
                    strSql = "update LR_ResultEntryRule set TargetFeatClass2='" + m_pPara.strTargetLayer +
                             "|' where RuleInstID='" +
                             this.m_InstanceID + "'";
                    Common.Utility.Data.AdoDbHelper.ExecuteSql(null, strSql);
                }
                //else if (nGeoType == 2) // ��
                //{
                //    strSql = "update LR_ResultEntryRule set TargetFeatClass1='" + m_PointLayerName +
                //             "',TargetFeatClass2='" + m_PointLayerName + "|" + m_pPara.strTargetLayer +
                //             "' where RuleInstID='" + m_RuleInfo.strID + "'";
                //    GT_CARTO.CommonAPI.ado_ExecuteSQL(m_pResultAdoConn, strSql);
                //}
                //else if (nGeoType == 3) // ��
                //{
                //    strSql = "update LR_ResultEntryRule set TargetFeatClass1='" + m_PointLayerName +
                //             "',TargetFeatClass2='" + m_PointLayerName + "|" + m_pPara.strTargetLayer +
                //             "' where RuleInstID='" + m_RuleInfo.strID + "'";
                //    GT_CARTO.CommonAPI.ado_ExecuteSQL(m_pResultAdoConn, strSql);
                //}

                IFields pFields = pSrcFeatClass.Fields;
                int nIndex = pFields.FindField("BSM");

                foreach (DictionaryEntry dn in m_RepeatOIDtoOID)
                {
                    string key = dn.Key.ToString();
                    Error err = new Error();
                    err.DefectLevel = this.m_DefectLevel;
                    err.RuleID = this.InstanceID;
                    err.LayerName = this.m_strSrcLayer;

                    RuleExpression.LRResultInfo pResInfo = new RuleExpression.LRResultInfo();
                    err.OID = Convert.ToInt32(m_PointtoOID[key]);
                    err.ReferOID = dn.Value.ToString();
                    if (nIndex >= 0)
                    {
                        IFeature pFt = pSrcFeatClass.GetFeature(pResInfo.OID);
                        err.BSM =pFt.get_Value(nIndex).ToString();
                        string[] listOID = err.ReferOID.Split(',');
                        string listBSM2 = "";
                        for (int i = 0; i < listOID.Length; i++)
                        {
                            int nOIDTemp = Convert.ToInt32(listOID[i]);
                            IFeature pFt1 = pSrcFeatClass.GetFeature(nOIDTemp);
                            listBSM2 += pFt1.get_Value(nIndex).ToString() + ",";
                        }
                        listBSM2 = listBSM2.Substring(0, listBSM2.Length - 1);
                        err.ReferBSM = listBSM2;
                    }
                    err.Description = string.Format("'{0}'�б�ʶ��Ϊ'{1}'���ʶ��Ϊ'{2}'�ĵ����໥�ص���ʵ�������ڵ���벻ӦС��{3}��", m_pPara.strTargetLayer, pResInfo.BSM, pResInfo.BSM2, m_pPara.dPointDist);
                    pResult.Add(err);
                }
            }
            catch (Exception ex)
            {
                //Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                //��ʾ������Ϣ; 
                //XtraMessageBox.Show("RulePointDist:SaveData():" + ex.Message + " ");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ����������ݵ��Ƿ��غ�
        /// </summary>
        /// <param name="srcPt"></param>
        /// <param name="tarPt"></param>
        /// <returns></returns>
        private bool PointIsEqual(PointXY srcPt, PointXY tarPt)
        {
            if (Math.Abs(srcPt.dx - tarPt.dx) < m_pPara.dPointDist && Math.Abs(srcPt.dy - tarPt.dy) < m_pPara.dPointDist)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ���OID��Feature���ձ�
        /// </summary>
        private void ClearMap()
        {
            m_MapOIDFeature1.Clear();
            m_MapOIDFeature2.Clear();
        }

        public override string Name
        {
            get { return "��������ʼ����"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {

            m_pPara = new RuleExpression.LRPointDistPara();
            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();
            m_pPara.dPointDist = pParameter.ReadDouble();
            m_pPara.nSearchType = pParameter.ReadInt32();
            m_pPara.bSearchSamePt = Convert.ToBoolean(pParameter.ReadInt32());


            int nStrSize = sizeof(double) + 3 * sizeof(int);
            Byte[] bb = new byte[nCount1 - nStrSize];
            pParameter.Read(bb, 0, nCount1 - nStrSize);
            string para_str = Encoding.Default.GetString(bb);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_pPara.strAlias = strResult[i++];
            m_pPara.strRemark = strResult[i++];
            m_pPara.strTargetLayer = strResult[i++];
            m_pPara.strStdName = strResult[i++];
            m_pPara.strBufferLayer = strResult[i];

        }

        public override bool Verify()
        {
            IFeatureWorkspace ipFtWS;
            try
            {
                if (m_pPara == null)
                {
                    return false;
                }
                // �����ռ�
                ipFtWS = base.m_BaseWorkspace as IFeatureWorkspace;                

                // ��ȡԴͼ������
                m_strSrcLayer=base.GetLayerName(m_pPara.strTargetLayer);
                
                // ��Դͼ��
                if (!(ipFtWS as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, m_strSrcLayer))
                {
                    return false;
                }

                pSrcFeatClass = ipFtWS.OpenFeatureClass(m_strSrcLayer);
  
                if (pSrcFeatClass == null)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                return false;
            }
            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
            try
            {
                SearchPoints(pSrcFeatClass);
                //SearchStupid(pSrcFeatClass);
                checkResult = new List<Error>();

                if (!SaveData(ref checkResult))
                {
                    checkResult = null;
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pSrcFeatClass != null)
                {
                    Marshal.ReleaseComObject(pSrcFeatClass);
                    pSrcFeatClass = null;
                }
            }
        }
    }
}