using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using Hy.Check.Rule;
using Hy.Check.Utility;
using Hy.Check.Define;

namespace Rule
{
    public class RuleSpatailOp : BaseRule
    {

        private LRSpatialOpPara m_pPara;

        private string m_strName;

        private string strRelLayer = null;

        private string strSrcLayer = null;

        public RuleSpatailOp()
        {
            m_pPara = new LRSpatialOpPara();
            m_strName = "�ռ�����ʼ����";
        }

        // ��ȡ�����
        private void AddResult(ref List<Hy.Check.Define.Error> pRuleResult, IFeatureCursor pFeatCursor)
        {
            if (pFeatCursor == null)
            {
                return;
            }
            try
            {
                IFeature ipFeature = pFeatCursor.NextFeature();

                string strErrMsg = null;
                int OID = -1;

                while (ipFeature != null)
                {
                    // ��ҽ����¼
                    Error pResInfo = new Error();

                    OID = ipFeature.OID;

                    // OID
                    pResInfo.OID = OID;

                    // Ŀ��ͼ��
                    pResInfo.LayerName = m_pPara.strTargetLayer;

                    // ������Ϣ
                    string strTemp = "";
                    if (m_pPara.strRemark.Trim() != strTemp && m_pPara.strRemark != null)
                    {
                        strErrMsg = m_pPara.strRemark;
                    }
                    else
                    {
                        strErrMsg = "(" + m_pPara.strTargetLayer + ")<" + m_pPara.strSpatialRel + ">(" +
                                    m_pPara.strRelLayer +
                                    ")";
                    }
                    pResInfo.Description = strErrMsg;

                    pRuleResult.Add(pResInfo);

                    ipFeature = pFeatCursor.NextFeature();
                }
            }
            catch (Exception ex)
            {
                //Hy.Check.Rule.Helper.LogAPI.CheckLog.AppendErrLogs(ex.ToString());
                //XtraMessageBox.Show("RuleSpatialOp:AddResult()"+ex.Message, "��ʾ");
                return;
            }
        }

        public override string Name
        {
            get { return m_strName; }
        }

        public override Hy.Check.Define.IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {

            MemoryStream stream = new MemoryStream(objParamters);
            BinaryReader pParameter = new BinaryReader(stream);

            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();
            m_pPara.dBuffer = pParameter.ReadDouble();
            m_pPara.eSpatialRel = (esriSpatialRelEnum)pParameter.ReadInt32();
            m_pPara.bBuffer = Convert.ToBoolean(pParameter.ReadInt32());
            m_pPara.bCustomRel = Convert.ToBoolean(pParameter.ReadInt32());

            //�����ַ���
            int nStrSize = sizeof(double) + 4 * sizeof(int);
            Byte[] bb = new byte[nCount1 - nStrSize];
            pParameter.Read(bb, 0, nCount1 - nStrSize);
            string para_str = Encoding.Default.GetString(bb);
            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_pPara.strAlias = strResult[i++];
            m_pPara.strRemark = strResult[i++];
            m_pPara.strStdName = strResult[i++];
            m_pPara.strTargetLayer = strResult[i++];
            m_pPara.strUnit = strResult[i++];
            m_pPara.strRelLayer = strResult[i++];
            m_pPara.strSpatialRel = strResult[i++];
            m_pPara.strWhereClause = strResult[i];

            return ;
        }

        public override bool Verify()
        {
            //���ݱ���ȡfeatureclass������
            int standardID = SysDbHelper.GetStandardIDBySchemaID(this.m_SchemaID);
            strSrcLayer = LayerReader.GetNameByAliasName(m_pPara.strTargetLayer, standardID);
            strRelLayer = LayerReader.GetNameByAliasName(m_pPara.strRelLayer, standardID);

            if (this.m_BaseWorkspace != null)
            {
                if (!(this.m_BaseWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, strSrcLayer))
                {
                    string strLog = "��ǰ�������ݿ�Ĺ�ϵͼ��" + strSrcLayer + "������,�޷�ִ�м��!";
                    SendMessage(enumMessageType.RuleError, strLog);
                    return false;
                }

                if (!(this.m_BaseWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, strSrcLayer))
                {
                    string strLog = "��ǰ�������ݿ��Ŀ��ͼ��" + strRelLayer + "������,�޷�ִ�м��!";
                    SendMessage(enumMessageType.RuleError, strLog);
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Check(ref List<Hy.Check.Define.Error> checkResult)
        {
            //�õ�Ŀ��ͼ��͹�ϵͼ���featureclass
            IFeatureClass pSrcFeatClass = null;
            IFeatureClass pRelFeatClass = null;
            IFeatureCursor ipFeatCursor = null;
            checkResult = new List<Error>();
            try
            {

                string shapeFieldName;
                IFeature ipFeature;
                ISpatialFilter pSpatialFilter = new SpatialFilterClass(); //(CLSID_SpatialFilter);

                //��ȡ��Ҫ���пռ��ϵ��ѯ��ILayer
                IFeatureWorkspace ipFtWS = null;

                ipFtWS = (IFeatureWorkspace)this.m_BaseWorkspace;
                    
                pSrcFeatClass = ipFtWS.OpenFeatureClass(strSrcLayer);
                pRelFeatClass = ipFtWS.OpenFeatureClass(strRelLayer);

                // 1. ���ÿռ��ϵ
                if (!m_pPara.bCustomRel) // ʹ��engineԤ����Ŀռ��ϵ
                {
                    pSpatialFilter.SpatialRel = m_pPara.eSpatialRel;
                }
                else //�Զ���ռ��ϵ
                {
                    pSpatialFilter.SpatialRelDescription = m_pPara.strSpatialRel;
                }

                // 2.���ù��˼���
                shapeFieldName = pSrcFeatClass.ShapeFieldName;
                pSpatialFilter.GeometryField = shapeFieldName;

                // 3.����ѡ���Ⱥ��ϵ����Ȼ��PGDB�����ã�������Ҳ�����������
                // Sets the order in which spatial searches are applied by the RDBMS (ArcSDE). 
                //pSpatialFilter.put_SearchOrder(esriSearchOrderSpatial);

                // 4.����where���
                if (m_pPara.strWhereClause.Length > 0)
                //hehyע�ͣ�2008��2��1�գ������ж�������ΪpSpatialFilter.SpatialRel = m_pPara.eSpatialRel;
                {
                    pSpatialFilter.WhereClause = m_pPara.strWhereClause;
                }

                //pSpatialFilter.SpatialRel = m_pPara.eSpatialRel;
                // 5.Ŀ�������һ�����GeometryCollection
                IGeometryCollection pGeometryCollection = new GeometryBagClass(); //(CLSID_GeometryBag);
                IQueryFilter pQueryFilter = new QueryFilterClass(); //(CLSID_QueryFilter);
                string SubFields = "Shape";
                pQueryFilter.SubFields = SubFields;
                ipFeatCursor = pRelFeatClass.Search(pQueryFilter, true);

                ipFeature = ipFeatCursor.NextFeature();

                while (ipFeature != null)
                {
                    IGeometry ipGeometry = ipFeature.Shape;
                    if (ipGeometry == null)
                    {
                        ipFeature = ipFeatCursor.NextFeature();
                        continue;
                    }

                    object Missing = Type.Missing;

                    if (!(m_pPara.bBuffer)) //���û�����
                    {
                        pGeometryCollection.AddGeometry(ipGeometry, ref Missing, ref Missing);
                    }
                    else //ʹ�û���
                    {
                        ITopologicalOperator ipTopo = (ITopologicalOperator)ipGeometry;
                        ipTopo.Simplify();
                        IGeometry ipGeobuffer = ipTopo.Buffer(m_pPara.dBuffer);
                        pGeometryCollection.AddGeometry(ipGeobuffer, ref Missing, ref Missing);
                    }

                    ipFeature = ipFeatCursor.NextFeature();
                }

                ISpatialIndex pSpatialIndex = (ISpatialIndex)pGeometryCollection;
                pSpatialIndex.AllowIndexing = true;
                pSpatialIndex.Invalidate();

                // 6.�����GeometryCollection����spatialfilter
                pSpatialFilter.Geometry = (IGeometry)pGeometryCollection;

                // 7.Ŀ��ͼ���в������ɵ�spatialfilter���в�ѯ
                IFeatureCursor ipResultFtCur;
                string Fields = "OBJECTID,Shape";
                pSpatialFilter.SubFields = Fields;

                //IQueryFilter queryFilter = new QueryFilterClass();
                //queryFilter = (IQueryFilter) pSpatialFilter;

                ipResultFtCur = pSrcFeatClass.Search(pSpatialFilter, true);

                // 8.�������� 
                AddResult(ref checkResult, ipResultFtCur);
            }

            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (ipFeatCursor != null)
                {
                    Marshal.ReleaseComObject(ipFeatCursor);
                    ipFeatCursor = null;
                }
                if (pSrcFeatClass != null)
                {
                    Marshal.ReleaseComObject(pSrcFeatClass);
                    pSrcFeatClass = null;
                }
                if (pRelFeatClass != null)
                {
                    Marshal.ReleaseComObject(pRelFeatClass);
                    pRelFeatClass = null;
                }
            }
            return true;
        }
    }

    // �ռ�ѡ���ϵ����
    public class LRSpatialOpPara
    {
        public string strAlias;
        public string strRemark;

        // ��׼����
        public string strStdName;

        // Ŀ��ͼ��
        public string strTargetLayer;

        // ��λ
        public string strUnit;

        // ��ϵͼ��
        public string strRelLayer;

        // �ռ��ϵ����
        public string strSpatialRel;

        // sql���where
        public string strWhereClause;

        // �ռ��ϵ
        public esriSpatialRelEnum eSpatialRel;

        // �Ƿ�ʹ�û�����
        public bool bBuffer;

        // �Ƿ��Զ���ռ��ϵ
        public bool bCustomRel;

        // ������
        public double dBuffer;
    }
}