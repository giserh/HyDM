using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Hy.Check.Define;


namespace Hy.Check.Rule
{
    /// <summary>
    /// �ӱ߼��
    /// </summary>
    public class RuleJoinSide : BaseRule
    {
        private string m_strBufferLayer;
        private RuleExpression.LRJoinSidePara m_pPara;
        private int m_nGeoType;
        private List<RuleExpression.JoinSideInfo> m_aryResult = new List<RuleExpression.JoinSideInfo>();
        private IArray m_aryInErrFeatures;
        private string strSrcLayer;
        private string strBoundLayer;

        public RuleJoinSide()
        {
        }

        public override string Name
        {
            get { return "�ӱ߼��"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
            m_pPara = new RuleExpression.LRJoinSidePara();

            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();
            m_pPara.dLimit = pParameter.ReadDouble();

            //�����ַ���
            int nStrSize = nCount1 - sizeof(double) - sizeof(int);
            Byte[] bb = new byte[nStrSize];
            pParameter.Read(bb, 0, nStrSize);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();
            string[] strResult = para_str.Split('|');

            m_pPara.arrayFieldName = new List<string>();
            for (int k = 0; k < strResult.Length; k++)
            {
                switch (k)
                {
                    case 0:
                        m_pPara.strAlias = strResult[k];
                        break;
                    case 1:
                        m_pPara.strRemark = strResult[k];
                        break;
                    case 2:
                        m_pPara.strFeatureLayer = strResult[k];
                        break;
                    case 3:
                        m_pPara.strBoundLayer = strResult[k];
                        break;
                    case 4:
                        m_pPara.strInciseField = strResult[k];
                        break;
                    case 5:
                        m_pPara.strStdName = strResult[k];
                        break;
                    default:
                        m_pPara.arrayFieldName.Add(strResult[k]);
                        break;
                }
            }
        }

        public override bool Verify()
        {
            if (m_pPara == null)
            {
                return false;
            }
            if (this.m_BaseWorkspace == null)
                return false;

            // ��ȡԴͼ������
            strSrcLayer = base.GetLayerName(m_pPara.strFeatureLayer);
            strBoundLayer = base.GetLayerName(m_pPara.strBoundLayer);
            if (!(m_BaseWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, strSrcLayer))
            {
                SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ�Ľӱ�ͼ��" + strSrcLayer + "������!");
                return false;
            }

            // �򿪷�Χͼ��
            if (!(m_BaseWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, strBoundLayer))
            {
                SendMessage(enumMessageType.RuleError, "��ǰ�������ݿ�Ľӱ�ͼ��" + strBoundLayer + "������!");
                return false;
            }

            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        {
            IFeatureClass ipSrcFeatClass=null; //�ӱ�ͼ��Ҫ��
            IFeatureClass ipBoundFeatClass=null; //��Χͼ��Ҫ��
            try
            {
                IFeatureWorkspace ipFtWS = (IFeatureWorkspace)m_BaseWorkspace; // �����ռ�
                // ��Ҫ��ͼ��
                ipSrcFeatClass = ipFtWS.OpenFeatureClass(strSrcLayer);

                if (ipSrcFeatClass == null)
                {
                    return false;
                }

                // �򿪷�Χͼ��
                ipBoundFeatClass = ipFtWS.OpenFeatureClass(strBoundLayer);

                if (ipBoundFeatClass == null)
                {
                    return false;
                }

                m_nGeoType = GetLayerGeoType(ipSrcFeatClass);


                int nTuFuFieldIndex = -1;
                List<string> sTuFuCodeArray = new List<string>();

                IEnvelope ipSrcFeatureEnvelop = GetEnvelopByFeatureClass(ipSrcFeatClass);

                IPointCollection ipPointCollection = new PolylineClass(); // (CLSID_Polyline);
                IEnvelope ipErrorEnvelop =
                    GetCutlineNumber(ipSrcFeatClass, ipBoundFeatClass, m_pPara.strInciseField, ipSrcFeatureEnvelop,
                                     ref nTuFuFieldIndex, ref sTuFuCodeArray, ipPointCollection);

                if (ipErrorEnvelop == null)
                {
                    return false;
                }
                if (nTuFuFieldIndex == -1)
                {
                    SendMessage(enumMessageType.RuleError, "�ӱ��ֶ��ڽӱ߷�Χͼ��" + strBoundLayer + "������!");
                    return false;
                }

                int nTuFuCount = sTuFuCodeArray.Count;
                int nFieldCount = m_pPara.arrayFieldName.Count;

                if (nTuFuCount == 0)
                {
                    SendMessage(enumMessageType.RuleError, "����ѡ��һ��ͼ��Ҫ�ؽ��нӱ�!");
                    return false;
                }
                if (nFieldCount == 0)
                {
                    SendMessage(enumMessageType.RuleError, "����ѡ��һ�������ֶ���Ϊ�ӱ�����!");
                    return false;
                }

                IArray aryGriddingLine = GetTuFuGriddingLine(ipPointCollection, ipErrorEnvelop);

                string sWhereClause;

                int nMatchEdge = 0;
                int lGriddingLineCount = aryGriddingLine.Count;


                string sProgressMessage, sCurrentTuFuCode;
                string sTuFuName;

                sTuFuName = m_pPara.strInciseField;

                List<string> aryOIDs = new List<string>();
                List<string> aryCheckInfos = new List<string>();

                esriGeometryType eGeometryType = ipSrcFeatClass.ShapeType;

                List<int> aryFieldIndex = new List<int>(); //CUIntArray aryFieldIndex;
                for (int t = 0; t < nFieldCount; t++)
                {
                    string str = m_pPara.arrayFieldName[t];
                    int nFIndex = -1;
                    GetFieldIndexByFieldName(ipSrcFeatClass, str, ref nFIndex);
                    if (nFIndex != -1)
                    {
                        aryFieldIndex.Add(nFIndex);
                    }
                }

                IFields ipFields = ipBoundFeatClass.Fields;

                IField ipField = ipFields.get_Field(nTuFuFieldIndex);

                esriFieldType eFieldType = ipField.Type;

                string strSql = "";

                //���ýӱ߱�־
                for (int i = 0; i < lGriddingLineCount; i++)
                {
                    object obj = aryGriddingLine.get_Element(i);
                    IPolyline ipPolyline = (IPolyline)obj;

                    if (eGeometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        MatchEdgeCheckPolygon(ipSrcFeatClass, strSql, ipPolyline,
                                              aryFieldIndex, m_pPara.dLimit,
                                              ipErrorEnvelop, ref aryOIDs, ref aryCheckInfos);
                    }
                    else
                    {
                        MatchEdgeCheckLine(ipSrcFeatClass, strSql,
                                           ipPolyline, aryFieldIndex, m_pPara.dLimit,
                                           ipErrorEnvelop, ref aryOIDs, ref aryCheckInfos);
                    }
                }


                if (aryOIDs.Count > 0)
                {
                    long nErrCount = aryOIDs.Count;
                    for (int i = 0; i < nErrCount; i++)
                    {
                        RuleExpression.JoinSideInfo jsInfo;
                        jsInfo.OID1 = Convert.ToInt32(aryOIDs[i]);
                        jsInfo.strError = aryCheckInfos[i];
                        m_aryResult.Add(jsInfo);
                    }
                }

                List<Error> pResult = new List<Error>();
                if (!SaveData(ref pResult))
                {
                    pResult = null;
                    return false;
                }
                checkResult = pResult;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (ipSrcFeatClass != null)
                {
                    Marshal.ReleaseComObject(ipSrcFeatClass);
                }
                if (ipBoundFeatClass != null)
                {
                    Marshal.ReleaseComObject(ipBoundFeatClass);
                }
            }
        }

        /// <summary>
        /// ��ȡͼ��ļ�������
        /// </summary>
        /// <param name="ipSrcFeaCls">Ҫ����</param>
        /// <returns>��ȷ��������ֵ�����򷵻�0</returns>
        private int GetLayerGeoType(IFeatureClass ipSrcFeaCls)
        {
            int nGeoType = 0;

            esriGeometryType Type = ipSrcFeaCls.ShapeType;

            switch (Type)
            {
                case esriGeometryType.esriGeometryPoint:
                case esriGeometryType.esriGeometryMultipoint:
                    {
                        nGeoType = 1;
                        break;
                    }
                case esriGeometryType.esriGeometryLine:
                case esriGeometryType.esriGeometryPolyline:
                    {
                        nGeoType = 2;
                        break;
                    }
                case esriGeometryType.esriGeometryRing:
                case esriGeometryType.esriGeometryPolygon:
                case esriGeometryType.esriGeometryEnvelope:
                    {
                        nGeoType = 3;
                        break;
                    }
                default:
                    break;
            }

            return nGeoType;
        }

        /// <summary>
        /// ��ȡҪ�ؼ���������
        /// </summary>
        /// <param name="ipFeatureClass">Ҫ�ؼ�</param>
        /// <returns>��ȷ����Ҫ�ؼ�������</returns>
        private IEnvelope GetEnvelopByFeatureClass(IFeatureClass ipFeatureClass)
        {
            try
            {
                IEnvelope ipMaxEnvelop = new EnvelopeClass();

                double dMinX = 0.0;
                double dMaxX = 0.0;
                double dMinY = 0.0;
                double dMaxY = 0.0;


                ISelectionSet ipSelectionSet =
                    ipFeatureClass.Select(null, esriSelectionType.esriSelectionTypeHybrid,
                                          esriSelectionOption.esriSelectionOptionNormal, null);
                int nFeatureCount = ipSelectionSet.Count;


                if (nFeatureCount < 5000)
                {
                    ICursor ipCursor = null;
                    ipSelectionSet.Search(null, false, out ipCursor);
                    IFeatureCursor ipFeatureCursor = (IFeatureCursor) ipCursor;


                    if (ipFeatureCursor == null)
                    {
                        return null;
                    }

                    IFeature ipFeature = ipFeatureCursor.NextFeature();
                    IEnvelope ipEnvelop = ipFeature.Extent;
                    dMinX = ipEnvelop.XMin;
                    dMaxX = ipEnvelop.XMax;
                    dMinY = ipEnvelop.YMin;
                    dMaxY = ipEnvelop.YMax;


                    ipFeature = ipFeatureCursor.NextFeature();

                    double dtMinX = 0.0;
                    double dtMaxX = 0.0;
                    double dtMinY = 0.0;
                    double dtMaxY = 0.0;
                    while (ipFeature != null)
                    {
                        ipEnvelop = ipFeature.Extent;
                        dtMinX = ipEnvelop.XMin;
                        dtMaxX = ipEnvelop.XMax;
                        dtMinY = ipEnvelop.YMin;
                        dtMaxY = ipEnvelop.YMax;

                        if (dtMinX < dMinX)
                            dMinX = dtMinX;
                        if (dtMaxX > dMaxX)
                            dMaxX = dtMaxX;
                        if (dtMinY < dMinY)
                            dMinY = dtMinY;
                        if (dtMaxY > dMaxY)
                            dMaxY = dtMaxY;

                        ipFeature = ipFeatureCursor.NextFeature();
                    }
                }
                else
                {
                    IFeatureLayer ipFeatureLayer = new FeatureLayerClass();
                    ipFeatureLayer.FeatureClass = ipFeatureClass;

                    ipMaxEnvelop = ipFeatureLayer.AreaOfInterest;

                    //���䷶Χ������һ�㣬���ⲻ��Ҫ��ͼ����ѡ����
                    dMinX = ipMaxEnvelop.XMin;
                    dMaxX = ipMaxEnvelop.XMax;
                    dMinY = ipMaxEnvelop.YMin;
                    dMaxY = ipMaxEnvelop.YMax;
                }

                double dHeight = (dMaxY - dMinY)/500;
                double dWidth = (dMaxX - dMinX)/500;
                double dTempTol = dHeight > dWidth ? dWidth : dHeight;

                dMinX += dTempTol;
                dMaxX -= dTempTol;
                dMinY += dTempTol;
                dMaxY -= dTempTol;

                ipMaxEnvelop.XMin = dMinX;
                ipMaxEnvelop.XMax = dMaxX;
                ipMaxEnvelop.YMin = dMinY;
                ipMaxEnvelop.YMax = dMaxY;

                return ipMaxEnvelop;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// ��ȡ�ַ�ͼ����ָ��Ŀ��ͼ��ķ�Χ��С������ͼ�����ֶε�ֵ
        /// </summary>
        /// <param name="ipFeatrueClass">�ӱ�Ҫ����</param>
        /// <param name="ipBoundFeatureClass">��ΧҪ����</param>
        /// <param name="strTuFuNumFieldName">ͼ�����ֶ���</param>
        /// <param name="ipEnvelope"></param>
        /// <param name="nTuFuFieldIndex"></param>
        /// <param name="aryNumber">ͼ��������</param>
        /// <param name="ipPointCollection"></param>
        /// <returns>�ɹ�����true,���򷵻�false</returns>
        private IEnvelope GetCutlineNumber(IFeatureClass ipFeatrueClass,
                                           IFeatureClass ipBoundFeatureClass,
                                           string strTuFuNumFieldName,
                                           IEnvelope ipEnvelope,
                                           ref int nTuFuFieldIndex, ref List<string> aryNumber,
                                           IPointCollection ipPointCollection)
        {
            try
            {
                if (ipEnvelope == null) return null;

                // //���Դ���
                // BSTR bstrTuFuNum = strTuFuNumFieldName.AllocSysString();
                //ipTable.FindField(bstrTuFuNum,&nIndex);
                //���ͼ�����ֶε�����
                ITable ipTable = (ITable) ipBoundFeatureClass;
                int nIndex = ipTable.FindField(strTuFuNumFieldName);


                if (nIndex == -1)
                {
                    return null;
                }

                IEnvelope ipTuFuMaxEnvelope = new EnvelopeClass();
                nTuFuFieldIndex = nIndex;

                //���ÿռ�����
                ISpatialFilter ipSpatialFilter = new SpatialFilterClass();
                ipSpatialFilter.AddField(strTuFuNumFieldName);
                IGeometry ipGeo = ipEnvelope;
                ipSpatialFilter.Geometry = ipGeo;
                string bstrSharpFieldName = ipBoundFeatureClass.ShapeFieldName;
                ipSpatialFilter.GeometryField = bstrSharpFieldName;
                ipSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                //���пռ��ѯ,����ȡ��ͼ�����ֶε�ֵ
                IFeatureCursor ipFeatureCursor = ipBoundFeatureClass.Search(ipSpatialFilter, false);

                if (ipFeatureCursor == null)
                {
                    return null;
                }

                IGeometry ipGeometry;
                IGeometry ipPolygonGeometry;
                IPolygon2 ipPolygon;
                IRing ipRing;


                IFeature ipFeature = ipFeatureCursor.NextFeature();
                IEnvelope ipTuFuEnvelope = ipFeature.Extent;

                double dMinX = 0.0;
                double dMaxX = 0.0;
                double dMinY = 0.0;
                double dMaxY = 0.0;

                double dTMinX = 0.0;
                double dTMaxX = 0.0;
                double dTMinY = 0.0;
                double dTMaxY = 0.0;

                dMinX = ipTuFuEnvelope.XMin;
                dMaxX = ipTuFuEnvelope.XMax;
                dMinY = ipTuFuEnvelope.YMin;
                dMaxY = ipTuFuEnvelope.YMax;


                IPoint ipPoint = new PointClass();

                int nPointCount = 0;
                while (ipFeature != null)
                {
                    object varTuFu = ipFeature.get_Value(nIndex);
                    string str = varTuFu.ToString();
                    aryNumber.Add(str);

                    ipTuFuEnvelope = ipFeature.Extent;
                    dTMinX = ipTuFuEnvelope.XMin;
                    dTMaxX = ipTuFuEnvelope.XMax;
                    dTMinY = ipTuFuEnvelope.YMin;
                    dTMaxY = ipTuFuEnvelope.YMax;

                    if (dTMinX < dMinX)
                        dMinX = dTMinX;
                    if (dTMaxX > dMaxX)
                        dMaxX = dTMaxX;
                    if (dTMinY < dMinY)
                        dMinY = dTMinY;
                    if (dTMaxY > dMaxY)
                        dMaxY = dTMaxY;

                    object obj = Type.Missing;


                    ipPoint.X = dTMinX;
                    ipPoint.Y = dTMinY;
                    ipPointCollection.AddPoint(ipPoint, ref obj, ref obj);
                    ipPoint.X = dTMinX;
                    ipPoint.Y = dTMaxY;
                    ipPointCollection.AddPoint(ipPoint, ref obj, ref obj);
                    ipPoint.X = dTMaxX;
                    ipPoint.Y = dTMinY;
                    ipPointCollection.AddPoint(ipPoint, ref obj, ref obj);
                    ipPoint.X = dTMaxX;
                    ipPoint.Y = dTMaxY;
                    ipPointCollection.AddPoint(ipPoint, ref obj, ref obj);

                    ipFeature = ipFeatureCursor.NextFeature();
                }

                ipTuFuMaxEnvelope.XMin = dMinX;
                ipTuFuMaxEnvelope.XMax = dMaxX;
                ipTuFuMaxEnvelope.YMin = dMinY;
                ipTuFuMaxEnvelope.YMax = dMaxY;
                return ipTuFuMaxEnvelope;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return null;
            }
        }

        /// <summary>
        ///  ��ȡͼ��������
        /// </summary>
        /// <param name="ipPointCollection">����㼯��</param>
        /// <param name="ipTuFuEnvelope">ͼ����������</param>
        /// <returns></returns>
        private IArray GetTuFuGriddingLine(IPointCollection ipPointCollection, IEnvelope ipTuFuEnvelope)
        {
            try
            {
                IArray aryGriddingLine = new ArrayClass();
                double dMinX = 0.0;
                double dMaxX = 0.0;
                double dMinY = 0.0;
                double dMaxY = 0.0;

                dMinX = ipTuFuEnvelope.XMin;
                dMaxX = ipTuFuEnvelope.XMax;
                dMinY = ipTuFuEnvelope.YMin;
                dMaxY = ipTuFuEnvelope.YMax;


                int nPointCount = ipPointCollection.PointCount;

                IPoint ipPoint1 = null;
                IPoint ipPoint2 = null;

                double dX1 = 0.0;
                double dY1 = 0.0;
                double dX2 = 0.0;
                double dY2 = 0.0;

                double dHeight = (dMaxY - dMinY)/1000;
                double dWidth = (dMaxX - dMinX)/1000;
                double dTempTol = dHeight > dWidth ? dWidth : dHeight;

                List<int> aryVerticalIndex = new List<int>(); //CArray<long,long> aryVerticalIndex;//���д�ֱ�����ߵ��������
                List<int> aryHorizontal = new List<int>(); //CArray<long,long> aryHorizontal;//����ˮƽ�����ߵ��������

                int nVerCount = 0;
                int nHorCount = 0;
                int i, j;

                //�����߿��ϵĵ�,�м�ĵ�ȥ��
                for (i = 0; i < nPointCount;)
                {
                    ipPoint1 = ipPointCollection.get_Point(i);
                    if (!PointBIsOverBoundary(ipPoint1, ipTuFuEnvelope, dTempTol))
                    {
                        ipPointCollection.RemovePoints(i, 1);
                        nPointCount--;
                    }
                    else
                    {
                        i++;
                    }
                }

                nPointCount = ipPointCollection.PointCount;
                for (i = 0; i < nPointCount; i++)
                {
                    ipPoint1 = ipPointCollection.get_Point(i);

                    dX1 = ipPoint1.X;
                    dY1 = ipPoint1.Y;

                    nVerCount = aryVerticalIndex.Count;
                    if (nVerCount == 0 && Math.Abs(dY1 - dMinY) < dTempTol
                        && Math.Abs(dX1 - dMinX) > dTempTol
                        && Math.Abs(dX1 - dMaxX) > dTempTol)
                    {
                        nVerCount++;
                        aryVerticalIndex.Add(i);
                    }

                    nHorCount = aryHorizontal.Count;
                    if (nHorCount == 0 && Math.Abs(dX1 - dMinX) < dTempTol
                        && Math.Abs(dY1 - dMinY) > dTempTol
                        && Math.Abs(dY1 - dMaxY) > dTempTol)
                    {
                        nHorCount++;
                        aryHorizontal.Add(i);
                    }

                    for (j = 0; j < nVerCount; j++)
                    {
                        ipPoint2 = ipPointCollection.get_Point(aryVerticalIndex[j]);
                        dX2 = ipPoint2.X;
                        dY2 = ipPoint2.Y;

                        if (Math.Abs(dX1 - dX2) < dTempTol && Math.Abs(dY1 - dY2) < dTempTol)
                        {
                            break;
                        }
                    }

                    if (j >= nVerCount)
                    {
                        //ȡ�ײ�ȥ����������ĵ�Ϊ���
                        if (Math.Abs(dY1 - dMinY) < dTempTol
                            && Math.Abs(dX1 - dMinX) > dTempTol
                            && Math.Abs(dX1 - dMaxX) > dTempTol)
                        {
                            aryVerticalIndex.Add(i);
                        }
                    }

                    for (j = 0; j < nHorCount; j++)
                    {
                        ipPoint2 = ipPointCollection.get_Point(aryHorizontal[j]);
                        dX2 = ipPoint2.X;
                        dY2 = ipPoint2.Y;
                        if (Math.Abs(dX1 - dX2) < dTempTol && Math.Abs(dY1 - dY2) < dTempTol)
                        {
                            break;
                        }
                    }

                    if (j >= nHorCount)
                    {
                        //ȡ���ȥ����������ĵ�Ϊ���
                        if (Math.Abs(dX1 - dMinX) < dTempTol
                            && Math.Abs(dY1 - dMinY) > dTempTol
                            && Math.Abs(dY1 - dMaxY) > dTempTol)
                        {
                            aryHorizontal.Add(i);
                        }
                    }
                }

                nPointCount = ipPointCollection.PointCount;

                //��֯��ֱ������
                nVerCount = aryVerticalIndex.Count;
                for (i = 0; i < nVerCount; i++)
                {
                    ipPoint1 = ipPointCollection.get_Point(aryVerticalIndex[i]);
                    dX1 = ipPoint1.X;
                    dY1 = ipPoint1.Y;

                    //������
                    IPolyline ipPolyline = new PolylineClass();
                    ipPolyline.FromPoint = ipPoint1;

                    for (j = 0; j < nPointCount; j++)
                    {
                        ipPoint2 = ipPointCollection.get_Point(j);
                        dX2 = ipPoint2.X;
                        dY2 = ipPoint2.Y;
                        if (Math.Abs(dX1 - dX2) < dTempTol && Math.Abs(dY1 - dY2) > dTempTol)
                        {
                            //����յ�
                            ipPolyline.ToPoint = ipPoint2;
                            break;
                        }
                    }
                    aryGriddingLine.Add(ipPolyline);
                }

                //��֯ˮƽ������
                nHorCount = aryHorizontal.Count;
                for (i = 0; i < nHorCount; i++)
                {
                    ipPoint1 = ipPointCollection.get_Point(aryHorizontal[i]);
                    dX1 = ipPoint1.X;
                    dY1 = ipPoint1.Y;

                    //������
                    IPolyline ipPolyline = new PolylineClass();
                    ipPolyline.FromPoint = ipPoint1;

                    for (j = 0; j < nPointCount; j++)
                    {
                        ipPoint2 = ipPointCollection.get_Point(j);
                        dX2 = ipPoint2.X;
                        dY2 = ipPoint2.Y;
                        if (Math.Abs(dY1 - dY2) < dTempTol && Math.Abs(dX1 - dX2) > dTempTol)
                        {
                            //����յ�
                            ipPolyline.ToPoint = ipPoint2;
                            break;
                        }
                    }
                    aryGriddingLine.Add(ipPolyline);
                }
                return aryGriddingLine;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// �ж�һ�����Ƿ��������
        /// </summary>
        /// <param name="ipPoint">��</param>
        /// <param name="ipEnvelope">���</param>
        /// <param name="dblTolerance">�ݴ�ֵ</param>
        /// <returns></returns>
        private bool PointBIsOverBoundary(IPoint ipPoint, IEnvelope ipEnvelope, double dblTolerance)
        {
            double dX = 0.0;
            double dY = 0.0;

            double dMinX = ipEnvelope.XMin;
            double dMaxX = ipEnvelope.XMax;
            double dMinY = ipEnvelope.YMin;
            double dMaxY = ipEnvelope.YMax;

            dX = ipPoint.X;
            dY = ipPoint.Y;


            double dTempTol = dblTolerance;

            if (Math.Abs(dX - dMinX) < dTempTol ||
                Math.Abs(dX - dMaxX) < dTempTol ||
                Math.Abs(dY - dMinY) < dTempTol ||
                Math.Abs(dY - dMaxY) < dTempTol)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// ��ȡ�ֶε�����
        /// </summary>
        /// <param name="ipFeatrueClass">Ҫ����</param>
        /// <param name="strFieldName">�ֶ�����</param>
        /// <param name="nIndex">����</param>
        /// <returns>��ȷ����true�����򷵻�false</returns>
        private bool GetFieldIndexByFieldName(IFeatureClass ipFeatrueClass, string strFieldName, ref int nIndex)
        {
            try
            {
                nIndex = ipFeatrueClass.FindField(strFieldName);

                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// �߽ӱ߼��
        /// </summary>
        /// <param name="pMatchEdgeFeatureClass">Ҫ���нӱ߼���Ҫ��</param>
        /// <param name="szSql">���Թ�������</param>
        /// <param name="ipGriddingline"></param>
        /// <param name="nFieldArray">����ƥ���ֶ������б�</param>
        /// <param name="dblTolerance">�ݴ�ֵ</param>
        /// <param name="ipErrorFilterEnvelop">�����ڱ߽總����Ҫ��û�нӱ߶���Ĳ����ڴ���,�����һ�����˵Ķ���</param>
        /// <param name="sOIDs">�ӱ߼��Ķ���OID</param>
        /// <param name="sCheckInfos">�ӱ߼��ÿ����Ϣ</param>
        /// <returns></returns>
        private bool MatchEdgeCheckLine(IFeatureClass pMatchEdgeFeatureClass,
                                        string szSql,
                                        IPolyline ipGriddingline,
                                        List<int> nFieldArray,
                                        double dblTolerance,
                                        IEnvelope ipErrorFilterEnvelop,
                                        ref List<string> sOIDs,
                                        ref List<string> sCheckInfos)
        {
            try
            {
                //ʹ�������߽��пռ��ѯ
                IGeometry ipLineBufGeoInSide, ipLineBufGeoOutSide;


                //����ipSourceSelectionSet�е�Ҫ��(Buffer����)
                //Ȼ����ipTargetSelectionSet��ȡ�����ཻ��Ҫ��
                bool bIsInToOut = true; //�ͷ����ڵ���

                for (int k = 0; k < 2; k++)
                {
                    //��ȡ�ڻ�������Ҫ�ؼ�
                    IPolyline ipPolyline = LineBuffer(ipGriddingline, dblTolerance);

                    ISpatialFilter ipSpatialFilter = new SpatialFilterClass();
                    ipLineBufGeoInSide = ipPolyline;

                    ipSpatialFilter.Geometry = ipLineBufGeoInSide;
                    ipSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    //ipSpatialFilter.WhereClause = szSql;

                    ISelectionSet ipInSideSelectionSet = null;
                    ISelectionSet ipOutSideSelectionSet = null;
                    try
                    {
                        ipInSideSelectionSet =
                            pMatchEdgeFeatureClass.Select(ipSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                          esriSelectionOption.esriSelectionOptionNormal, null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        return false;
                    }
                    int lInSideSelectionCount = ipInSideSelectionSet.Count;


                    //�ڻ�����û�ж�����ô�⻺�����Ķ���Ҳ���ü��
                    if (lInSideSelectionCount < 1)
                    {
                        return true;
                    }

                    //��ȡ�⻺�������Ҫ�ؼ�
                    ipPolyline = LineBuffer(ipGriddingline, -dblTolerance);
                    ipLineBufGeoOutSide = ipPolyline;

                    ipSpatialFilter.Geometry = ipLineBufGeoOutSide;

                    try
                    {
                        ipOutSideSelectionSet =
                            pMatchEdgeFeatureClass.Select(ipSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                          esriSelectionOption.esriSelectionOptionNormal, null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        return false;
                    }
                    int lOutSideSelectionCount = ipOutSideSelectionSet.Count;

                    IGeometry ipGriddinglineBufferGeo = ((ITopologicalOperator) ipGriddingline).Buffer(dblTolerance);


                    ISelectionSet ipSourceSelectionSet = null;
                    ISelectionSet ipTargetSelectionSet = null;
                    ICursor ipSourceCursor = null;
                    if (bIsInToOut)
                    {
                        ipSourceSelectionSet = ipInSideSelectionSet;
                        ipTargetSelectionSet = ipOutSideSelectionSet;
                    }
                    else
                    {
                        ipSourceSelectionSet = ipOutSideSelectionSet;
                        ipTargetSelectionSet = ipInSideSelectionSet;
                    }
                    try
                    {
                        ipSourceSelectionSet.Search(null, false, out ipSourceCursor);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        return false;
                    }
                    if (ipSourceCursor == null)
                    {
                        return false;
                    }
                    IRow ipSourceRow = ipSourceCursor.NextRow();
                    IFields ipMatchEdgeFields = pMatchEdgeFeatureClass.Fields;

                    int nProcessingFeature = 0;
                    string sProgressMessage;
                    while (ipSourceRow != null)
                    {
                        nProcessingFeature++;
                        //������֤�ֶεõ�ipSourceRow�Ķ�Ӧ�ֶ�ֵ
                        int nFieldIndex;
                        int nFieldCount = nFieldArray.Count;
                        //��ǰ�ǵڼ���ӱ߼��
                        int lSourceRowOID = ipSourceRow.OID;
                        if (lSourceRowOID == 30)
                        {
                        }
                        string sSourceRowOID = lSourceRowOID.ToString();
                        string sMatchEdgeWhereClause = GetCondition(ipSourceRow, ipMatchEdgeFields, nFieldArray);

                        //�����������ж��Ƿ��пռ��ϵĽӱ߶�������У��ٽ�һ���ж��Ƿ������Խӱ�
                        IGeometry ipSourceGeometry, ipFromBufferGeometry, ipToBufferGeometry;
                        ipSourceGeometry = ((IFeature) ipSourceRow).Shape;

                        //����Ҫ��ipSourceGeometry�������յ��õ����ֱ��ж��������յ��Ƿ���Ҫ�нӱ߶���
                        IPoint ipFromPoint = ((ICurve) ipSourceGeometry).FromPoint;
                        IPoint ipToPoint = ((ICurve) ipSourceGeometry).ToPoint;

                        bool bIsFromOver = PointBIsOverBoundary(ipFromPoint, ipErrorFilterEnvelop, dblTolerance);
                        bool bIsToOver = PointBIsOverBoundary(ipToPoint, ipErrorFilterEnvelop, dblTolerance);
                        if (bIsFromOver && bIsToOver)
                        {
                            Marshal.ReleaseComObject(ipSourceRow);
                            ipSourceRow = ipSourceCursor.NextRow();
                            continue;
                        }

                        //�ֱ���������յ��Ƿ���ͼ���������
                        bool vtContainsFrom, vtContainsTo;
                        ipFromBufferGeometry = ((ITopologicalOperator) ipFromPoint).Buffer(dblTolerance*0.1);
                        ipToBufferGeometry = ((ITopologicalOperator) ipToPoint).Buffer(dblTolerance*0.1);


                        ((ITopologicalOperator) ipFromBufferGeometry).Simplify();
                        ((ITopologicalOperator) ipToBufferGeometry).Simplify();


                        vtContainsFrom = ((IRelationalOperator) ipGriddinglineBufferGeo).Contains(ipFromBufferGeometry);
                        vtContainsTo = ((IRelationalOperator) ipGriddinglineBufferGeo).Contains(ipToBufferGeometry);

                        if (vtContainsFrom == true || vtContainsTo == true)
                        {
                            //������������յ���Tolerance��Χ�ڵ�Ŀ��ӱ߶���
                            ipFromBufferGeometry = ((ITopologicalOperator) ipFromPoint).Buffer(dblTolerance);
                            ipToBufferGeometry = ((ITopologicalOperator) ipToPoint).Buffer(dblTolerance);
                            IGeometry ipFromBoundaryGeometry, ipToBoundaryGeometry;
                            ipFromBoundaryGeometry = ((ITopologicalOperator) ipFromBufferGeometry).Boundary;
                            ipToBoundaryGeometry = ((ITopologicalOperator) ipToBufferGeometry).Boundary;

                            ISpatialFilter ipFromSpatialFilter = new SpatialFilterClass();
                            ISpatialFilter ipToSpatialFilter = new SpatialFilterClass();

                            ipFromSpatialFilter.Geometry = ipFromBoundaryGeometry;
                            ipFromSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                            ipToSpatialFilter.Geometry = ipToBoundaryGeometry;
                            ipToSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                            //�����յ㶼��Ҫ�ӱ�
                            if (vtContainsFrom == true && vtContainsTo == true)
                            {
                                if (
                                    !JointFromToPoints(bIsFromOver, bIsToOver, ipToSpatialFilter, ipFromSpatialFilter,
                                                       ref ipSourceRow, ipSourceCursor, ipTargetSelectionSet,
                                                       sMatchEdgeWhereClause, lSourceRowOID, sSourceRowOID, ref sOIDs,
                                                       ref sCheckInfos))
                                {
                                    continue;
                                }
                            }
                            else if (vtContainsFrom == true)
                            {
                                //ֻ�������Ҫ�ӱ߶���
                                if (!JoinFromPoint(bIsFromOver, bIsToOver, ipToSpatialFilter, ipFromSpatialFilter,
                                                   ref ipSourceRow, ipSourceCursor, ipTargetSelectionSet,
                                                   sMatchEdgeWhereClause, lSourceRowOID, sSourceRowOID, ref sOIDs,
                                                   ref sCheckInfos))
                                {
                                    continue;
                                }
                            }
                            else if (vtContainsTo == true)
                            {
                                //ֻ���յ���Ҫ�ӱ߶���
                                if (!JoinToPoint(bIsFromOver, bIsToOver, ipToSpatialFilter, ipFromSpatialFilter,
                                                 ref ipSourceRow, ipSourceCursor, ipTargetSelectionSet,
                                                 sMatchEdgeWhereClause,
                                                 lSourceRowOID, sSourceRowOID, ref sOIDs, ref sCheckInfos))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                sOIDs.Add(sSourceRowOID);
                                //�����յ㶼û�нӱ߶���,�����������յ㲻��ͼ���������
                                string sInfo = sSourceRowOID + "Ҫ��ȱ�ٶ�Ӧ�Ľӱ߶���";
                                sCheckInfos.Add(sInfo);
                            }
                        }
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                    }
                    bIsInToOut = false;
                    Marshal.ReleaseComObject(ipSourceCursor);
                    if (ipInSideSelectionSet != null)
                    {
                        Marshal.ReleaseComObject(ipInSideSelectionSet);
                    }
                    if (ipOutSideSelectionSet != null)
                    {
                        Marshal.ReleaseComObject(ipOutSideSelectionSet);
                    }
                    if (ipSourceSelectionSet != null)
                    {
                        Marshal.ReleaseComObject(ipSourceSelectionSet);

                        if (ipTargetSelectionSet != null)
                        {
                            Marshal.ReleaseComObject(ipTargetSelectionSet);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// ��ӱ߼��
        /// </summary>
        /// <param name="pMatchEdgeFeatureClass">Ҫ���нӱ߼���Ҫ��</param>
        /// <param name="szSql">���Թ�������</param>
        /// <param name="ipGriddingline"></param>
        /// <param name="nFieldArray">����ƥ���ֶ������б�</param>
        /// <param name="dblTolerance">�ݴ�ֵ</param>
        /// <param name="ipErrorFilterEnvelop"></param>
        /// <param name="sOIDs">�ӱ߼��Ķ���OID</param>
        /// <param name="sCheckInfos">�ӱ߼��ÿ����Ϣ</param>
        /// <returns></returns>
        private bool MatchEdgeCheckPolygon(IFeatureClass pMatchEdgeFeatureClass, string szSql, IPolyline ipGriddingline,
                                           List<int> nFieldArray, double dblTolerance, IEnvelope ipErrorFilterEnvelop,
                                           ref List<string> sOIDs, ref List<string> sCheckInfos)
        {
            //���Դ���

            IPoint ipTestFromPoint = ipGriddingline.FromPoint;
            IPoint ipTestToPoint = ipGriddingline.ToPoint;

            double dFromTestX = ipTestFromPoint.X;
            double dFromTestY = ipTestFromPoint.Y;
            double dToTestX = ipTestToPoint.X;
            double dToTestY = ipTestToPoint.Y;
            try
            {
                //

                //ʹ�������߽��пռ��ѯ
                ISpatialFilter ipSpatialFilter = new SpatialFilterClass();

                IGeometry ipLineBufGeoInSide, ipLineBufGeoOutSide;

                ISelectionSet ipInSideSelectionSet = null;
                ISelectionSet ipOutSideSelectionSet = null;

                //��ȡ�ڻ�������Ҫ�ؼ�
                //IPolygon ipPolygon = LineBuffer(ipGriddingline,dblTolerance);
                IPolyline ipPolyline = LineBuffer(ipGriddingline, dblTolerance);

                ipLineBufGeoInSide = ipPolyline;

                ipSpatialFilter.Geometry = ipLineBufGeoInSide;
                ipSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                ipSpatialFilter.WhereClause = szSql;

                ipInSideSelectionSet =
                    pMatchEdgeFeatureClass.Select(ipSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                  esriSelectionOption.esriSelectionOptionNormal, null);
                int lInSideSelectionCount = ipInSideSelectionSet.Count;

                //�ڻ�����û�ж�����ô�⻺�����Ķ���Ҳ���ü��
                if (lInSideSelectionCount < 1)
                {
                    return true;
                }

                //��ȡ�⻺�������Ҫ�ؼ�
                ipPolyline = LineBuffer(ipGriddingline, -dblTolerance);
                ipLineBufGeoOutSide = ipPolyline;

                ipSpatialFilter.Geometry = (IGeometry) ipLineBufGeoOutSide;

                ipOutSideSelectionSet =
                    pMatchEdgeFeatureClass.Select(ipSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                  esriSelectionOption.esriSelectionOptionNormal, null);

                int lOutSideSelectionCount = ipOutSideSelectionSet.Count;

                IScratchWorkspaceFactory ipSwf = new ScratchWorkspaceFactoryClass();
                IWorkspace ipScratchWorkspace = ipSwf.CreateNewScratchWorkspace();

                bool bIsInToOut = true; //�ͷ����ڵ���
                for (int k = 0; k < 2; k++)
                {
                    ISelectionSet ipSourceSelectionSet = null;
                    ISelectionSet ipTargetSelectionSet = null;
                    if (bIsInToOut)
                    {
                        ipSourceSelectionSet = ipInSideSelectionSet;
                        ipTargetSelectionSet = ipOutSideSelectionSet;
                    }
                    else
                    {
                        ipSourceSelectionSet = ipOutSideSelectionSet;
                        ipTargetSelectionSet = ipInSideSelectionSet;
                    }

                    //����ipSourceSelectionSet�е�Ҫ��(Buffer����)
                    //Ȼ����ipTargetSelectionSet��ȡ�����ཻ��Ҫ��
                    ICursor ipSourceCursor;
                    ipSourceSelectionSet.Search(null, false, out ipSourceCursor);
                    IRow ipSourceRow = ipSourceCursor.NextRow();
                    IFields ipMatchEdgeFields = pMatchEdgeFeatureClass.Fields;
                    IField ipMatchEdgeField;

                    int nProcessingFeature = 0;
                    string sProgressMessage;
                    while (ipSourceRow != null)
                    {
                        nProcessingFeature++;

                        //������֤�ֶεõ�ipSourceRow�Ķ�Ӧ�ֶ�ֵ
                        int nFieldIndex;
                        string sMatchEdgeWhereClause;
                        int nFieldCount = nFieldArray.Count;
                        esriFieldType eMatchEdgeFieldType;
                        string sMatchEdgeFieldValue;

                        //ipSourceRow��OID
                        int lSourceRowOID = ipSourceRow.OID;
                        string sSourceRowOID = lSourceRowOID.ToString();

                        //�������Բ�ѯ����
                        sMatchEdgeWhereClause = GetCondition(ipSourceRow, ipMatchEdgeFields, nFieldArray);

                        //�����������ж��Ƿ��пռ��ϵĽӱ߶�������У��ٽ�һ���ж��Ƿ������Խӱ�

                        IGeometry ipSourceGeometry = ((IFeature) ipSourceRow).Shape;

                        ISpatialFilter ipSourceSpatialFilter = new SpatialFilterClass();

                        ipSourceSpatialFilter.Geometry = ipSourceGeometry;
                        ipSourceSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                        ISelectionSet ipSourceSelectionSet1 =
                            ipTargetSelectionSet.Select(ipSourceSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                        esriSelectionOption.esriSelectionOptionNormal,
                                                        ipScratchWorkspace);

                        int lSourceSelectionCount = ipSourceSelectionSet1.Count;

                        if (lSourceSelectionCount > 0)
                        {
                            //��ͼ�νӱ�
                            ipSourceSpatialFilter.WhereClause = sMatchEdgeWhereClause;

                            ISelectionSet ipSourceAttrSelectionSet1 =
                                ipSourceSelectionSet1.Select(ipSourceSpatialFilter,
                                                             esriSelectionType.esriSelectionTypeHybrid,
                                                             esriSelectionOption.esriSelectionOptionNormal,
                                                             ipScratchWorkspace);
                            int lSourceAttrSelectionCount = ipSourceAttrSelectionSet1.Count;

                            if (lSourceAttrSelectionCount > 0)
                            {
                                //�����Խӱ�
                                //�жϽӱ߶����Ƿ����һ��
                                ICursor ipSourceAttrCursor;
                                ipSourceAttrSelectionSet1.Search(null, false, out ipSourceAttrCursor);

                                IRow ipSourceAttrRow = ipSourceAttrCursor.NextRow();
                                int nSourceAttr = 0;
                                while (ipSourceAttrRow != null)
                                {
                                    int lSourceAttrOID = ipSourceAttrRow.OID;


                                    if (lSourceAttrOID != lSourceRowOID)
                                    {
                                        nSourceAttr++;
                                    }
                                    ipSourceAttrRow = ipSourceAttrCursor.NextRow();
                                }

                                string sInfo;
                                if (nSourceAttr > 1)
                                {
                                    sOIDs.Add(sSourceRowOID);
                                    sInfo = sSourceRowOID + "Ҫ�ش���" + nSourceAttr + "���ӱ߶���";
                                    sCheckInfos.Add(sInfo);
                                }
                            }
                            else
                            {
                                sOIDs.Add(sSourceRowOID);
                                //û�����Խӱ߶���
                                string sInfo = sSourceRowOID + "Ҫ���нӱ߶���,�����Բ�ͬ";
                                sCheckInfos.Add(sInfo);
                            }
                        }
                        else
                        {
                            sOIDs.Add(sSourceRowOID);
                            //û�нӱ߶���
                            string sInfo = sSourceRowOID + "Ҫ��ȱ�ٶ�Ӧ�Ľӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                    }
                    bIsInToOut = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// ��ȡ�ߵĻ��������ζ���
        /// </summary>
        /// <param name="ipPolyline">��</param>
        /// <param name="tolerance">����ֵ</param>
        /// <returns>��ȷ�����߼��ζ��󣬷��򷵻�null</returns>
        private IPolyline LineBuffer(IPolyline ipPolyline, double tolerance)
        {
            if (ipPolyline == null)
            {
                return null;
            }

            ISegmentCollection ipSegCollRes = (ISegmentCollection) ipPolyline;

            long nSegmentCount = ipSegCollRes.SegmentCount;

            ISegment ipSegment = null;
            IPointCollection ipPtColl = new PolylineClass();

            for (int i = 0; i < nSegmentCount; i++)
            {
                ipSegment = ipSegCollRes.get_Segment(i);

                ILine ipline1 = new LineClass();
                ILine ipline2 = new LineClass();

                ipSegment.QueryNormal(esriSegmentExtension.esriExtendTangentAtFrom, 0, false, -tolerance, ipline1);
                double dLength = ipSegment.Length;

                ipSegment.QueryNormal(esriSegmentExtension.esriExtendTangentAtFrom, dLength, false, -tolerance, ipline2);

                object obj = Type.Missing;
                IPoint ipFromPoint = ipline2.FromPoint;
                ipPtColl.AddPoint(ipFromPoint, ref obj, ref obj);

                IPoint ipPoint = ipline1.ToPoint;
                ipPtColl.AddPoint(ipPoint, ref obj, ref obj);

                double dFromX = ipFromPoint.X;
                double dFromY = ipFromPoint.Y;
                double dToX = ipPoint.X;
                double dToY = ipPoint.Y;
            }
            IPolyline ipResultPolyline = (IPolyline) ipPtColl;
            return ipResultPolyline;
        }


        /// <summary>
        /// ��ȡ����
        /// </summary>
        /// <param name="ipSourceRow">�м�</param>
        /// <param name="ipMatchEdgeFields">�ֶμ�</param>
        /// <param name="nFieldArray">Ҫ���˵��ֶ�����</param>
        /// <returns></returns>
        private string GetCondition(IRow ipSourceRow, IFields ipMatchEdgeFields, List<int> nFieldArray)
        {
            //�������Բ�ѯ����
            string sMatchEdgeFieldValue = "";
            string sMatchEdgeWhereClause = "";
            esriFieldType eMatchEdgeFieldType;
            object vtFieldValue;
            IField ipMatchEdgeField = null;
            Int64 nFieldCount = nFieldArray.Count;

            string strFieldName;

            int nFieldIndex = 0;

            for (int i = 0; i < nFieldCount; i++)
            {
                nFieldIndex = nFieldArray[i];
                vtFieldValue = ipSourceRow.get_Value(nFieldIndex);
                ipMatchEdgeField = ipMatchEdgeFields.get_Field(nFieldIndex);
                string bstrFieldName = ipMatchEdgeField.Name;

                strFieldName = bstrFieldName;
                eMatchEdgeFieldType = ipMatchEdgeField.Type;
                switch (eMatchEdgeFieldType)
                {
                    case esriFieldType.esriFieldTypeSmallInteger:
                        sMatchEdgeFieldValue = vtFieldValue.ToString();
                        sMatchEdgeWhereClause += strFieldName + " = " + sMatchEdgeFieldValue;
                        break;
                    case esriFieldType.esriFieldTypeInteger:
                        sMatchEdgeFieldValue = vtFieldValue.ToString();
                        sMatchEdgeWhereClause += strFieldName + " = " + sMatchEdgeFieldValue;
                        break;
                    case esriFieldType.esriFieldTypeSingle:
                        sMatchEdgeFieldValue = vtFieldValue.ToString();
                        sMatchEdgeWhereClause += strFieldName + " = " + sMatchEdgeFieldValue;
                        break;
                    case esriFieldType.esriFieldTypeDouble:
                        sMatchEdgeFieldValue = vtFieldValue.ToString();
                        sMatchEdgeWhereClause += strFieldName + " = " + sMatchEdgeFieldValue;
                        break;
                    case esriFieldType.esriFieldTypeString:
                        sMatchEdgeFieldValue = vtFieldValue.ToString();
                        sMatchEdgeWhereClause += strFieldName + " = '" + sMatchEdgeFieldValue + "'";
                        break;
                    default:
                        break;
                }
                //	sMatchEdgeWhereClause += string(bsFieldName) + " = '" + sMatchEdgeFieldValue + "'";
                if (i != (nFieldCount - 1))
                {
                    sMatchEdgeWhereClause += " and ";
                }
            }

            return sMatchEdgeWhereClause;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="pResult"></param>
        /// <returns></returns>
        private bool SaveData(ref List<Error> pResult)
        {
            try
            {
                string strSql;

                // �����д��Ŀ�������
                if (m_nGeoType == 1) // ��
                {
                    strSql = "update LR_ResultEntryRule set TargetFeatClass2='" + m_pPara.strFeatureLayer +
                             "|' where RuleInstID='" + this.m_SchemaID + "'";

                    Hy.Common.Utility.Data.AdoDbHelper.ExecuteSql(this.m_ResultConnection, strSql);
                }
                else if (m_nGeoType == 2) // ��
                {
                    strSql = "update LR_ResultEntryRule set TargetFeatClass1='" + m_pPara.strFeatureLayer +
                             "',TargetFeatClass2='" + m_pPara.strBoundLayer + "|" + m_pPara.strBoundLayer +
                             "'where RuleInstID='" + this.m_SchemaID + "'";

                    Hy.Common.Utility.Data.AdoDbHelper.ExecuteSql(this.m_ResultConnection, strSql);
                }
                else if (m_nGeoType == 3) // ��
                {
                    strSql = "update LR_ResultEntryRule set TargetFeatClass1='" + m_pPara.strFeatureLayer +
                             "',TargetFeatClass2='" +
                             m_pPara.strBoundLayer + "|" + m_pPara.strBoundLayer + "' where RuleInstID='" +
                             this.m_SchemaID + "'";

                    Hy.Common.Utility.Data.AdoDbHelper.ExecuteSql(this.m_ResultConnection, strSql);
                }

                int size = m_aryResult.Count;

                for (int i = 0; i < size; i++)
                {
                    Error pResInfo = new Error();
                    pResInfo.DefectLevel = this.m_DefectLevel;
                    pResInfo.RuleID = this.InstanceID;

                    pResInfo.OID = m_aryResult[i].OID1;
                    pResInfo.Description = m_aryResult[i].strError;
                    pResult.Add(pResInfo);
                }
                //�رռ�¼��
                //		ipRecordset.Close();
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// ��㡢�յ����Ҫ�ӱ߶���
        /// </summary>
        /// <param name="bIsFromOver"></param>
        /// <param name="bIsToOver"></param>
        /// <param name="ipToSpatialFilter"></param>
        /// <param name="ipFromSpatialFilter"></param>
        /// <param name="ipSourceRow"></param>
        /// <param name="ipSourceCursor"></param>
        /// <param name="ipTargetSelectionSet"></param>
        /// <param name="sMatchEdgeWhereClause"></param>
        /// <param name="lSourceRowOID"></param>
        /// <param name="sSourceRowOID"></param>
        /// <param name="sOIDs"></param>
        /// <param name="sCheckInfos"></param>
        /// <returns></returns>
        private bool JointFromToPoints(bool bIsFromOver, bool bIsToOver, ISpatialFilter ipToSpatialFilter,
                                       ISpatialFilter ipFromSpatialFilter,
                                       ref IRow ipSourceRow, ICursor ipSourceCursor, ISelectionSet ipTargetSelectionSet,
                                       string sMatchEdgeWhereClause, int lSourceRowOID, string sSourceRowOID,
                                       ref List<string> sOIDs, ref List<string> sCheckInfos)
        {
            try
            {
                int lFromSelectionCount, lToSelectionCount;
                ISelectionSet ipFromSelectionSet = null;
                ISelectionSet ipToSelectionSet = null;
                if (bIsFromOver == false)
                {
                    try
                    {
                        ipFromSelectionSet = ipTargetSelectionSet.Select(ipFromSpatialFilter,
                                                                         esriSelectionType.esriSelectionTypeHybrid,
                                                                         esriSelectionOption.esriSelectionOptionNormal,
                                                                         null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                        return false;
                    }

                    lFromSelectionCount = ipFromSelectionSet.Count;
                }
                else
                {
                    lFromSelectionCount = 0;
                }

                if (bIsToOver == false)
                {
                    try
                    {
                        ipToSelectionSet =
                            ipTargetSelectionSet.Select(ipToSpatialFilter,
                                                        esriSelectionType.esriSelectionTypeHybrid,
                                                        esriSelectionOption.esriSelectionOptionNormal,
                                                        null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                        return false;
                    }
                    lToSelectionCount = ipToSelectionSet.Count;
                }
                else
                {
                    lToSelectionCount = 0;
                }

                if (lFromSelectionCount > 0 && lToSelectionCount > 0)
                {
                    //�������յ��ͼ��ͼ�νӱ�
                    ipFromSpatialFilter.WhereClause = sMatchEdgeWhereClause;
                    ipToSpatialFilter.WhereClause = sMatchEdgeWhereClause;

                    ISelectionSet ipFromAttrSelectionSet, ipToAttrSelectionSet;
                    ipFromAttrSelectionSet =
                        ipFromSelectionSet.Select(ipFromSpatialFilter,
                                                  esriSelectionType.esriSelectionTypeHybrid,
                                                  esriSelectionOption.esriSelectionOptionNormal, null);

                    ipToAttrSelectionSet =
                        ipToSelectionSet.Select(ipToSpatialFilter,
                                                esriSelectionType.esriSelectionTypeHybrid,
                                                esriSelectionOption.esriSelectionOptionNormal, null);
                    int lFromAttrSelectionCount, lToAttrSelectionCount;
                    lFromAttrSelectionCount = ipFromAttrSelectionSet.Count;
                    lToAttrSelectionCount = ipToAttrSelectionSet.Count;

                    if (lFromAttrSelectionCount > 0 && lToAttrSelectionCount > 0)
                    {
                        //�����յ������Խӱ�
                        //�ж������յ�Ľӱ߶����Ƿ����һ��
                        ICursor ipFromAttrCursor, ipToAttrCursor;
                        ipFromAttrSelectionSet.Search(null, false, out ipFromAttrCursor);
                        ipToAttrSelectionSet.Search(null, false, out ipToAttrCursor);
                        //�ж����

                        IRow ipFromAttrRow = ipFromAttrCursor.NextRow();
                        int nFromAttr = 0;
                        while (ipFromAttrRow != null)
                        {
                            int lFromAttrOID = ipFromAttrRow.OID;
                            if (lFromAttrOID != lSourceRowOID)
                            {
                                nFromAttr++;
                            }
                            ipFromAttrRow = ipFromAttrCursor.NextRow();
                        }

                        //�ж��յ�
                        IRow ipToAttrRow = ipToAttrCursor.NextRow();
                        int nToAttr = 0;
                        while (ipToAttrRow != null)
                        {
                            int lToAttrOID = ipToAttrRow.OID;

                            if (lToAttrOID != lSourceRowOID)
                            {
                                nToAttr++;
                            }
                            ipToAttrRow = ipToAttrCursor.NextRow();
                        }

                        Marshal.ReleaseComObject(ipFromAttrCursor);
                        Marshal.ReleaseComObject(ipToAttrCursor);

                        string sInfo = null;
                        if (nFromAttr > 1 && nToAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            sInfo = sSourceRowOID + "Ҫ����㴦��" + nFromAttr + "���ӱ߶���;�յ㴦��" + nToAttr +
                                    "���ӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        else if (nFromAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            sInfo = sSourceRowOID + "Ҫ����㴦��" + nFromAttr + "���ӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        else if (nToAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            sInfo = sSourceRowOID + "Ҫ���յ㴦��" + nToAttr + "���ӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                    }
                    else if (lFromAttrSelectionCount > 0)
                    {
                        //��������Խӱ߶���,�յ�û�����Խӱ߶���
                        ICursor ipFromAttrCursor = null;
                        ipFromAttrSelectionSet.Search(null, false, out ipFromAttrCursor);
                        //�ж����
                        IRow ipFromAttrRow = ipFromAttrCursor.NextRow();
                        int nFromAttr = 0;
                        while (ipFromAttrRow != null)
                        {
                            int lFromAttrOID = ipFromAttrRow.OID;

                            if (lFromAttrOID != lSourceRowOID)
                            {
                                nFromAttr++;
                            }
                            ipFromAttrRow = ipFromAttrCursor.NextRow();
                        }

                        Marshal.ReleaseComObject(ipFromAttrCursor);

                        if (nFromAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ����㴦��" + nFromAttr + "���ӱ߶���,�����յ㴦�ӱ߶�������Բ�ͬ";
                            sCheckInfos.Add(sInfo);
                        }
                        else
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ���յ㴦�ӱ߶�������Բ�ͬ";
                            sCheckInfos.Add(sInfo);
                        }
                    }
                    else if (lToAttrSelectionCount > 0)
                    {
                        //�յ������Խӱ߶���,���û�����Խӱ߶���
                        //�ж��յ�
                        ICursor ipToAttrCursor = null;
                        ipToAttrSelectionSet.Search(null, false, out ipToAttrCursor);
                        IRow ipToAttrRow = ipToAttrCursor.NextRow();
                        int nToAttr = 0;
                        while (ipToAttrRow != null)
                        {
                            int lToAttrOID = ipToAttrRow.OID;

                            if (lToAttrOID != lSourceRowOID)
                            {
                                nToAttr++;
                            }
                            ipToAttrRow = ipToAttrCursor.NextRow();
                        }


                        Marshal.ReleaseComObject(ipToAttrCursor);
                        if (nToAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ���յ㴦��" + nToAttr + "���ӱ߶���,������㴦�ӱ߶�������Բ�ͬ";
                            sCheckInfos.Add(sInfo);
                        }
                        else
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ����㴦�ӱ߶�������Բ�ͬ";
                            sCheckInfos.Add(sInfo);
                        }
                    }
                    else
                    {
                        sOIDs.Add(sSourceRowOID);
                        //�����յ㶼û�����Խӱ߶���
                        string sInfo = sSourceRowOID + "Ҫ�������յ㴦�нӱ߶���,�����Բ�ͬ";
                        sCheckInfos.Add(sInfo);
                    }
                }
                else if (lFromSelectionCount > 0)
                {
                    ipFromSpatialFilter.WhereClause = sMatchEdgeWhereClause;

                    ISelectionSet ipFromAttrSelectionSet =
                        ipFromSelectionSet.Select(ipFromSpatialFilter,
                                                  esriSelectionType.esriSelectionTypeHybrid,
                                                  esriSelectionOption.esriSelectionOptionNormal, null);

                    int lFromAttrSelectionCount = ipFromAttrSelectionSet.Count;

                    if (lFromAttrSelectionCount > 0)
                    {
                        //��������Խӱ߶���
                        ICursor ipFromAttrCursor = null;
                        ipFromAttrSelectionSet.Search(null, false, out ipFromAttrCursor);
                        //�ж����
                        IRow ipFromAttrRow = ipFromAttrCursor.NextRow();
                        int nFromAttr = 0;
                        while (ipFromAttrRow != null)
                        {
                            int lFromAttrOID = ipFromAttrRow.OID;


                            if (lFromAttrOID != lSourceRowOID)
                            {
                                nFromAttr++;
                            }
                            ipFromAttrRow = ipFromAttrCursor.NextRow();
                        }

                        Marshal.ReleaseComObject(ipFromAttrCursor);

                        if (nFromAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ����㴦��" + nFromAttr + "���ӱ߶���,�����յ㴦ȱ�ٽӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        else
                        {
                            if (nFromAttr < 1)
                            {
                                sOIDs.Add(sSourceRowOID);
                                string sInfo = sSourceRowOID + "Ҫ���յ㴦ȱ�ٽӱ߶���";
                                sCheckInfos.Add(sInfo);
                            }
                        }
                    }
                    else
                    {
                        sOIDs.Add(sSourceRowOID);
                        string sInfo = sSourceRowOID + "Ҫ����㴦�нӱ߶���,�����Բ�ͬ;�����յ㴦ȱ�ٽӱ߶���";
                        sCheckInfos.Add(sInfo);
                    }
                }
                else if (lToSelectionCount > 0)
                {
                    ipToSpatialFilter.WhereClause = sMatchEdgeWhereClause;

                    ISelectionSet ipToAttrSelectionSet =
                        ipToSelectionSet.Select(ipToSpatialFilter,
                                                esriSelectionType.esriSelectionTypeHybrid,
                                                esriSelectionOption.esriSelectionOptionNormal, null);

                    int lToAttrSelectionCount = ipToAttrSelectionSet.Count;

                    if (lToAttrSelectionCount > 0)
                    {
                        //�յ������Խӱ߶���
                        ICursor ipToAttrCursor = null;
                        ipToAttrSelectionSet.Search(null, false, out ipToAttrCursor);
                        //�ж��յ�
                        IRow ipToAttrRow = ipToAttrCursor.NextRow();
                        int nToAttr = 0;
                        while (ipToAttrRow != null)
                        {
                            int lToAttrOID = ipToAttrRow.OID;

                            if (lToAttrOID != lSourceRowOID)
                            {
                                nToAttr++;
                            }
                            ipToAttrRow = ipToAttrCursor.NextRow();
                        }


                        Marshal.ReleaseComObject(ipToAttrCursor);

                        if (nToAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ���յ㴦��" + nToAttr + "���ӱ߶���,������㴦ȱ�ٽӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        else
                        {
                            if (nToAttr < 1)
                            {
                                sOIDs.Add(sSourceRowOID);
                                string sInfo = sSourceRowOID + "%sҪ����㴦ȱ�ٽӱ߶���";
                                sCheckInfos.Add(sInfo);
                            }
                        }
                    }
                    else
                    {
                        sOIDs.Add(sSourceRowOID);
                        string sInfo = sSourceRowOID + "%sҪ���յ㴦�нӱ߶���,�����Բ�ͬ;������㴦ȱ�ٽӱ߶���";
                        sCheckInfos.Add(sInfo);
                    }
                }
                else
                {
                    if (bIsFromOver == false && bIsToOver == false)
                    {
                        sOIDs.Add(sSourceRowOID);
                        //û��ͼ�νӱ�
                        string sInfo = sSourceRowOID + "Ҫ�������յ㴦��ȱ�ٶ�Ӧ�Ľӱ߶���";
                        sCheckInfos.Add(sInfo);
                    }
                }
                if (ipFromSelectionSet != null)
                {
                    Marshal.ReleaseComObject(ipFromSelectionSet);
                }
                if (ipToSelectionSet != null)
                {
                    Marshal.ReleaseComObject(ipToSelectionSet);
                }
                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                Marshal.ReleaseComObject(ipSourceRow);
                ipSourceRow = ipSourceCursor.NextRow();
                return false;
            }
        }

        /// <summary>
        /// ֻ�������Ҫ�ӱ߶���
        /// </summary>
        /// <param name="bIsFromOver"></param>
        /// <param name="bIsToOver"></param>
        /// <param name="ipToSpatialFilter"></param>
        /// <param name="ipFromSpatialFilter"></param>
        /// <param name="ipSourceRow"></param>
        /// <param name="ipSourceCursor"></param>
        /// <param name="ipTargetSelectionSet"></param>
        /// <param name="sMatchEdgeWhereClause"></param>
        /// <param name="lSourceRowOID"></param>
        /// <param name="sSourceRowOID"></param>
        /// <param name="sOIDs"></param>
        /// <param name="sCheckInfos"></param>
        /// <returns></returns>
        private bool JoinFromPoint(bool bIsFromOver, bool bIsToOver, ISpatialFilter ipToSpatialFilter,
                                   ISpatialFilter ipFromSpatialFilter,
                                   ref IRow ipSourceRow, ICursor ipSourceCursor, ISelectionSet ipTargetSelectionSet,
                                   string sMatchEdgeWhereClause, int lSourceRowOID, string sSourceRowOID,
                                   ref List<string> sOIDs, ref List<string> sCheckInfos)
        {
            try
            {
                //ֻ�������Ҫ�ӱ߶���
                ISelectionSet ipFromSelectionSet = null;
                int lFromSelectionCount;
                if (bIsFromOver == false)
                {
                    try
                    {
                        ipFromSelectionSet =
                            ipTargetSelectionSet.Select(ipFromSpatialFilter, esriSelectionType.esriSelectionTypeHybrid,
                                                        esriSelectionOption.esriSelectionOptionNormal, null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                        return false;
                    }
                    lFromSelectionCount = ipFromSelectionSet.Count;
                }
                else
                {
                    lFromSelectionCount = 0;
                }

                if (lFromSelectionCount > 0)
                {
                    //���Խӱ�
                    ipFromSpatialFilter.WhereClause = sMatchEdgeWhereClause;
                    ISelectionSet ipFromAttrSelectionSet;

                    ipFromAttrSelectionSet =
                        ipFromSelectionSet.Select(ipFromSpatialFilter,
                                                  esriSelectionType.esriSelectionTypeHybrid,
                                                  esriSelectionOption.esriSelectionOptionNormal, null);

                    int lFromAttrSelectionCount = ipFromAttrSelectionSet.Count;

                    if (lFromAttrSelectionCount > 0)
                    {
                        //��������Խӱ߶���
                        ICursor ipFromAttrCursor;
                        ipFromAttrSelectionSet.Search(null, false, out ipFromAttrCursor);
                        //�ж����
                        IRow ipFromAttrRow = ipFromAttrCursor.NextRow();
                        int nFromAttr = 0;
                        while (ipFromAttrRow != null)
                        {
                            int lFromAttrOID = ipFromAttrRow.OID;


                            if (lFromAttrOID != lSourceRowOID)
                            {
                                nFromAttr++;
                            }
                            ipFromAttrRow = ipFromAttrCursor.NextRow();
                        }

                        if (ipFromAttrCursor != null)
                        {
                            Marshal.ReleaseComObject(ipFromAttrCursor);
                        }
                        if (nFromAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ����㴦��" + nFromAttr + "���ӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                        else
                        {
                            if (nFromAttr < 1)
                            {
                                sOIDs.Add(sSourceRowOID);
                                string sInfo = sSourceRowOID + "Ҫ����㴦ȱ�ٽӱ߶���";
                                sCheckInfos.Add(sInfo);
                            }
                        }
                    }
                    else
                    {
                        sOIDs.Add(sSourceRowOID);
                        //���û�����Խӱ߶���
                        string sInfo = sSourceRowOID + "Ҫ����㴦�нӱ߶���,�����Բ�ͬ";
                        sCheckInfos.Add(sInfo);
                    }
                }
                else
                {
                    if (bIsFromOver == false)
                    {
                        sOIDs.Add(sSourceRowOID);
                        //
                        string sInfo = sSourceRowOID + "Ҫ����㴦û�нӱ߶���";
                        sCheckInfos.Add(sInfo);
                    }
                }
                if (ipFromSelectionSet != null)
                {
                    Marshal.ReleaseComObject(ipFromSelectionSet);
                }

                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                Marshal.ReleaseComObject(ipSourceRow);
                ipSourceRow = ipSourceCursor.NextRow();
                return false;
            }
        }

        /// <summary>
        /// ֻ���յ���Ҫ�ӱ߶���
        /// </summary>
        /// <param name="bIsFromOver"></param>
        /// <param name="bIsToOver"></param>
        /// <param name="ipToSpatialFilter"></param>
        /// <param name="ipFromSpatialFilter"></param>
        /// <param name="ipSourceRow"></param>
        /// <param name="ipSourceCursor"></param>
        /// <param name="ipTargetSelectionSet"></param>
        /// <param name="sMatchEdgeWhereClause"></param>
        /// <param name="lSourceRowOID"></param>
        /// <param name="sSourceRowOID"></param>
        /// <param name="sOIDs"></param>
        /// <param name="sCheckInfos"></param>
        /// <returns></returns>
        private bool JoinToPoint(bool bIsFromOver, bool bIsToOver, ISpatialFilter ipToSpatialFilter,
                                 ISpatialFilter ipFromSpatialFilter,
                                 ref IRow ipSourceRow, ICursor ipSourceCursor, ISelectionSet ipTargetSelectionSet,
                                 string sMatchEdgeWhereClause, int lSourceRowOID, string sSourceRowOID,
                                 ref List<string> sOIDs, ref List<string> sCheckInfos)
        {
            try
            {
                int lToSelectionCount;
                ISelectionSet ipToSelectionSet = null;
                if (bIsToOver == false)
                {
                    try
                    {
                        ipToSelectionSet =
                            ipTargetSelectionSet.Select(ipToSpatialFilter,
                                                        esriSelectionType.esriSelectionTypeHybrid,
                                                        esriSelectionOption.esriSelectionOptionNormal, null);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(enumMessageType.Exception, ex.ToString());
                        Marshal.ReleaseComObject(ipSourceRow);
                        ipSourceRow = ipSourceCursor.NextRow();
                        return false;
                    }
                    lToSelectionCount = ipToSelectionSet.Count;
                }
                else
                {
                    lToSelectionCount = 0;
                }
                if (lToSelectionCount > 0)
                {
                    //���Խӱ�

                    ipToSpatialFilter.WhereClause = sMatchEdgeWhereClause;

                    ISelectionSet ipToAttrSelectionSet =
                        ipToSelectionSet.Select(ipToSpatialFilter,
                                                esriSelectionType.esriSelectionTypeHybrid,
                                                esriSelectionOption.esriSelectionOptionNormal, null);
                    int lToAttrSelectionCount = ipToAttrSelectionSet.Count;

                    if (lToAttrSelectionCount > 0)
                    {
                        //�յ������Խӱ߶���
                        ICursor ipToAttrCursor;
                        ipToAttrSelectionSet.Search(null, false, out ipToAttrCursor);
                        //�ж����
                        IRow ipToAttrRow = ipToAttrCursor.NextRow();
                        int nToAttr = 0;
                        while (ipToAttrRow != null)
                        {
                            int lToAttrOID = ipToAttrRow.OID;


                            if (lToAttrOID != lSourceRowOID)
                            {
                                nToAttr++;
                            }
                            ipToAttrRow = ipToAttrCursor.NextRow();
                        }


                        Marshal.ReleaseComObject(ipToAttrCursor);

                        if (nToAttr > 1)
                        {
                            sOIDs.Add(sSourceRowOID);
                            string sInfo = sSourceRowOID + "Ҫ���յ㴦��" + nToAttr + "���ӱ߶���";
                            sCheckInfos.Add(sInfo);
                        }
                    }
                    else
                    {
                        sOIDs.Add(sSourceRowOID);
                        //�յ�û�����Խӱ߶���
                        string sInfo = sSourceRowOID + "Ҫ���յ㴦�нӱ߶���,�����Բ�ͬ";
                        sCheckInfos.Add(sInfo);
                    }
                }
                else
                {
                    if (bIsToOver == false)
                    {
                        sOIDs.Add(sSourceRowOID);
                        //
                        string sInfo = sSourceRowOID + "Ҫ���յ㴦û�нӱ߶���";
                        sCheckInfos.Add(sInfo);
                    }
                }
                if (ipToSelectionSet != null)
                {
                    Marshal.ReleaseComObject(ipToSelectionSet);
                }
                return true;
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                Marshal.ReleaseComObject(ipSourceRow);
                ipSourceRow = ipSourceCursor.NextRow();
                return false;
            }
        }



    }
}