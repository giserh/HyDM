using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Hy.Check.Define;

using Hy.Check.Rule.Helper;
using Hy.Check.Utility;

namespace Hy.Check.Rule
{
    public class RuleTopo : BaseRule,ITopologicalRule
    {       

        // ��������
        private RuleExpression.LRTopoParas m_psRuleParas;

        public override enumErrorType ErrorType
        {
            get
            {
                return enumErrorType.Topology;
            }
        }

        public override string Name
        {
            get { return "���˼��"; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {

            try
            {
                BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
                BinaryReader pBinaryReader = pParameter;

                m_psRuleParas = new RuleExpression.LRTopoParas();
                pBinaryReader.BaseStream.Position = 0;

                int nStrSize = pParameter.ReadInt32(); // �ַ����ܳ���
                int origT_size = pParameter.ReadInt32(); // Layer : ԭʼͼ�㼸�������ܳ���
                int selT_size = pParameter.ReadInt32(); // Layer : ѡ��ͼ�㼸������
                int rank_size = pParameter.ReadInt32(); // Rank : ͼ������ȼ�
                int rule_count = pParameter.ReadInt32(); // ����ṹ��ĸ���

                List<int> rule_size = new List<int>(); // ÿ������ṹ��ĳ���

                for (int i = 0; i < rule_count; i++)
                {
                    int nRuleSize = pParameter.ReadInt32();
                    rule_size.Add(nRuleSize);
                }

                // LRTopoParas�Ĳ���
                m_psRuleParas.dTolerance = pParameter.ReadDouble(); //	�ݲ� 

                //�����ַ���
                Byte[] bb = new byte[nStrSize];
                pParameter.Read(bb, 0, nStrSize);
                string para_str = Encoding.Default.GetString(bb);
                para_str.Trim();

                // �����ַ���
                ParseTopoPara(para_str, m_psRuleParas);


                // Layer : ԭʼͼ�㼸������
                m_psRuleParas.arrayOrigGeoT = new List<int>();
                for (int k = 0; k < origT_size / 4; k++)
                {
                    int orig_geoT = pParameter.ReadInt32();
                    m_psRuleParas.arrayOrigGeoT.Add(orig_geoT);
                }

                int j = 0;

                // Layer : ѡ��ͼ�㼸������
                m_psRuleParas.arraySeledGeoT = new List<int>();
                for (j = 0; j < (selT_size / 4); j++)
                {
                    int sel_geoT = pParameter.ReadInt32();
                    m_psRuleParas.arraySeledGeoT.Add(sel_geoT);
                }
                // Rank : ͼ������ȼ�
                m_psRuleParas.arrayRanks = new List<int>();
                for (j = 0; j < (rank_size / 4); j++)
                {
                    int rank = pParameter.ReadInt32();
                    m_psRuleParas.arrayRanks.Add(rank);
                }

                // ---------------------------����------------------------------------
                m_psRuleParas.arrayRules = new List<RuleExpression.LRTopoRule>();
                for (j = 0; j < rule_count; j++)
                {
                    int nRuleSize = rule_size[j];

                    RuleExpression.LRTopoRule pRule = new RuleExpression.LRTopoRule();
                    pRule.lPicID = pParameter.ReadInt32(); // ��������ͼƬID
                    pRule.nGeoTSrc = pParameter.ReadInt32(); // Դͼ�㼸������
                    pRule.nGeoTTarget = pParameter.ReadInt32(); // Ŀ��ͼ�꼸������
                    pRule.nRankSrc = pParameter.ReadInt32(); // Դͼ��Rank		
                    pRule.nRankTarget = pParameter.ReadInt32(); // Ŀ��ͼ��Rank
                    pRule.bShowError = Convert.ToBoolean(pParameter.ReadInt32()); // �Ƿ���ʾ����

                    //�����ַ���
                    Byte[] bb1 = new byte[nRuleSize - sizeof(int) * 6];
                    pParameter.Read(bb1, 0, nRuleSize - sizeof(int) * 6);
                    string para_str1 = Encoding.Default.GetString(bb1);
                    para_str1.Trim();

                    string[] strResult1 = para_str1.Split('|');

                    int k = 0;

                    pRule.strSourceLayerName = strResult1[k++];
                    pRule.strTopoRuleName = strResult1[k++];
                    pRule.strTargetLayerName = strResult1[k++];
                    pRule.strRuleDesc = strResult1[k++];
                    pRule.strRuleAliasName = strResult1[k];

                    // �����������
                    m_psRuleParas.arrayRules.Add(pRule);
                }

                pParameter = pBinaryReader;

                this.m_InstanceName = m_psRuleParas.arrayRules[0].strRuleAliasName;
            }
            catch
            {
            }
        }


        public override bool Verify()
        {
            try
            {
                IWorkspace2 ws2 = m_TopoWorkspace as IWorkspace2;
                m_LayerName = this.m_psRuleParas.arraySeledLayers[0];
                m_LayerName = this.GetLayerName(m_LayerName);

                if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, m_LayerName))
                {
                    Common.Utility.Esri.GPTool gpTool = new Common.Utility.Esri.GPTool();
                    gpTool.CopyFeatureClass(m_BaseWorkspace.PathName + "\\" + m_LayerName, m_TopoWorkspace.PathName + "\\" + this.m_Topology.FeatureDataset.Name + "\\" + m_LayerName);
                }

                if (this.m_psRuleParas.arraySeledLayers.Count > 1)
                {
                    m_ReferLayerName = this.m_psRuleParas.arraySeledLayers[1];
                    m_ReferLayerName = this.GetLayerName(m_ReferLayerName);

                    if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, m_ReferLayerName))
                    {
                        Common.Utility.Esri.GPTool gpTool = new Common.Utility.Esri.GPTool();
                        gpTool.CopyFeatureClass(m_BaseWorkspace.PathName + "\\" + m_ReferLayerName, m_TopoWorkspace.PathName + "\\" + this.m_Topology.FeatureDataset.Name + "\\" + m_ReferLayerName);
                    }
                }
            }
            catch
            {
                SendMessage(enumMessageType.VerifyError, string.Format("ͼ�㡰{0}��������", this.m_psRuleParas.arraySeledLayers[0]));
                return false;
            }

            return true;
        }

        public override bool Pretreat()
        {
            try
            {
                //SendMessage(enumMessageType.RuleError, "����׼������");

                IFeatureWorkspace fws = m_TopoWorkspace as IFeatureWorkspace;
                IWorkspace2 ws2 = m_TopoWorkspace as IWorkspace2;

                IFeatureClassContainer fClassContainer = this.m_Topology as IFeatureClassContainer;
                IEnumFeatureClass enFeatureClass = fClassContainer.Classes;
                IFeatureClass fClass = enFeatureClass.Next();
                IFeatureClass fClassRefer = null;
                bool isAdded = false;
                while (fClass != null)
                {
                    IDataset ds = fClass as IDataset;
                    if (ds.Name == m_LayerName)
                    {
                        isAdded = true;
                        break;
                    }
                    fClass = enFeatureClass.Next();
                }
                if (!isAdded)
                {
                    if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, m_LayerName))
                    {
                        SendMessage(enumMessageType.RuleError, string.Format("ͼ��{0}�������˿�ʧ�ܣ��޷����", this.m_psRuleParas.arraySeledLayers[0]));
                        return false;
                    }
                    fClass = fws.OpenFeatureClass(m_LayerName);
                    this.m_Topology.AddClass(fClass as IClass, 1, m_psRuleParas.arrayRanks[0], 1, false);
                }

                if (this.m_psRuleParas.arraySeledLayers.Count > 1)
                {

                    enFeatureClass.Reset();
                    fClassRefer = enFeatureClass.Next();
                    isAdded = false;
                    while (fClass != null)
                    {
                        IDataset ds = fClassRefer as IDataset;
                        if (ds.Name == m_ReferLayerName)
                        {
                            isAdded = true;
                            break;
                        }
                        fClassRefer = enFeatureClass.Next();
                    }
                    if (!isAdded)
                    {
                        if (!ws2.get_NameExists(esriDatasetType.esriDTFeatureClass, m_ReferLayerName))
                        {
                            SendMessage(enumMessageType.RuleError, string.Format("ͼ��{0}�����ڣ��޷����", this.m_psRuleParas.arraySeledLayers[1]));
                            return false;
                        }
                        fClassRefer = fws.OpenFeatureClass(m_ReferLayerName);
                        this.m_Topology.AddClass(fClass as IClass, 1, m_psRuleParas.arrayRanks[1], 1, false);
                    }
                }

                m_TopologylRule = new TopologyRuleClass();
                m_TopologylRule.Name = m_psRuleParas.arrayRules[0].strRuleAliasName;
                m_TopologylRule.OriginClassID = fClass.ObjectClassID;
                if (fClassRefer != null)
                    m_TopologylRule.DestinationClassID = fClassRefer.ObjectClassID;

                m_TopologylRule.AllOriginSubtypes = true; 
                m_TopologylRule.AllDestinationSubtypes = true;
                m_TopologylRule.TopologyRuleType = GetTopologyTypeByName(m_psRuleParas.arrayRules[0].strTopoRuleName);


                (this.m_Topology as ITopologyRuleContainer).AddRule(m_TopologylRule);

                return true;

            }
            catch (Exception exp)
            {
                SendMessage(enumMessageType.PretreatmentError, "���뵽����ʱ������Ϣ��"+exp.Message);
                SendMessage(enumMessageType.Exception, exp.ToString());
                return false;
            }
        }

        private ITopologyRule m_TopologylRule;
        private string m_LayerName;
        private string m_ReferLayerName;

        /// <summary>
        /// �����������ƻ�ȡ��������
        /// </summary>
        /// <param name="strRuleName">��������</param>
        /// <returns>��������</returns>
        private esriTopologyRuleType GetTopologyTypeByName(string strRuleName)
        {
            //---------------��Ҫ�����˹���----------
            if (strRuleName == TopoHelper.TopoName_PointCoveredByLineEndpoint) //1. "��Ҫ�ر��뱻�ߵĶ˵㸲��";
            {
                return esriTopologyRuleType.esriTRTPointCoveredByLineEndpoint;
            }
            else if (strRuleName == TopoHelper.TopoName_PointCoveredByLine) //2. "����뱻�߸���";
            {
                return esriTopologyRuleType.esriTRTPointCoveredByLine;
            }
            else if (strRuleName == TopoHelper.TopoName_PointProperlyInsideArea) //3. "�������ȫλ�����ڲ�";
            {
                return esriTopologyRuleType.esriTRTPointProperlyInsideArea;
            }
            else if (strRuleName == TopoHelper.TopoName_PointCoveredByAreaBoundary) //4. "����뱻��߽縲��";
            {
                return esriTopologyRuleType.esriTRTPointCoveredByAreaBoundary;
            }

                //---------------��Ҫ�����˹���---------
            else if (strRuleName == TopoHelper.TopoName_LineNoOverlap) //1. "�߲���Ҫ�ز������ص�";
            {
                return esriTopologyRuleType.esriTRTLineNoOverlap;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoIntersection) //2. "�߲���Ҫ�ز������ཻ";
            {
                return esriTopologyRuleType.esriTRTLineNoIntersection;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoDangles) //3. "�߲���Ҫ��û�����ҽڵ�";
            {
                return esriTopologyRuleType.esriTRTLineNoDangles;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoPseudos) //4. "�߲���Ҫ��û��α�ڵ�";
            {
                return esriTopologyRuleType.esriTRTLineNoPseudos;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoIntersectOrInteriorTouch) //5. "�߲���Ҫ�ز����ཻ���ڲ�����";
            {
                return esriTopologyRuleType.esriTRTLineNoIntersectOrInteriorTouch;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoOverlapLine) //6. "�߲����߲��Ҫ�ز��໥�ص�";
            {
                return esriTopologyRuleType.esriTRTLineNoOverlapLine;
            }
            else if (strRuleName == TopoHelper.TopoName_LineCoveredByLineClass) //7. "�߲�Ҫ�ر��뱻��һ�߲�Ҫ�ظ���";
            {
                return esriTopologyRuleType.esriTRTLineCoveredByLineClass;
            }
            else if (strRuleName == TopoHelper.TopoName_LineCoveredByAreaBoundary) //8. "�߲���뱻���߽縲��";
            {
                return esriTopologyRuleType.esriTRTLineCoveredByAreaBoundary;
            }
            else if (strRuleName == TopoHelper.TopoName_LineEndpointCoveredByPoint) //9. "�߲�Ҫ�ض˵���뱻��㸲��";
            {
                return esriTopologyRuleType.esriTRTLineEndpointCoveredByPoint;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoSelfOverlap) //10. "�߲���Ҫ�ز����ص�";
            {
                return esriTopologyRuleType.esriTRTLineNoSelfOverlap;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoSelfIntersect) //11. "�߲���Ҫ�ز����ཻ";
            {
                return esriTopologyRuleType.esriTRTLineNoSelfIntersect;
            }
            else if (strRuleName == TopoHelper.TopoName_LineNoMultipart) //12. "�߲���Ҫ�ر���Ϊ������";
            {
                return esriTopologyRuleType.esriTRTLineNoMultipart;
            }

                //---------------��Ҫ�����˹���---------
            else if (strRuleName == TopoHelper.TopoName_AreaNoOverlap) //1. "�����Ҫ�ز��໥�ص�";
            {
                return esriTopologyRuleType.esriTRTAreaNoOverlap;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaNoGaps) //2. "�����Ҫ��֮��û�з�϶";
            {
                return esriTopologyRuleType.esriTRTAreaNoGaps;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaNoOverlapArea) //3. "��㲻����һ����ص�";
            {
                return esriTopologyRuleType.esriTRTAreaNoOverlapArea;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaCoveredByAreaClass) //6. "�����Ҫ�ر���һ�����Ҫ�ظ���";
            {
                return esriTopologyRuleType.esriTRTAreaCoveredByAreaClass;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaAreaCoverEachOther) //5. "������һ������໥����";
            {
                return esriTopologyRuleType.esriTRTAreaAreaCoverEachOther;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaCoveredByArea) //4. "�����뱻��һ����㸲��";
            {
                return esriTopologyRuleType.esriTRTAreaCoveredByArea;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaBoundaryCoveredByLine) //7. "���߽类��һ���߲㸲��";
            {
                return esriTopologyRuleType.esriTRTAreaBoundaryCoveredByLine;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaBoundaryCoveredByAreaBoundary) //8. "������һ���߽�һ��";
            {
                return esriTopologyRuleType.esriTRTAreaBoundaryCoveredByAreaBoundary;
            }
            else if (strRuleName == TopoHelper.TopoName_AreaContainPoint) //9. "���������Ҫ��";
            {
                return esriTopologyRuleType.esriTRTAreaContainPoint;
            }
            else
            {
                return esriTopologyRuleType.esriTRTAny;
            }
        }

        public override bool Check(ref List<Error> checkResult)
        {
            try
            {
                if (m_TopologylRule == null)
                {
                    SendMessage(enumMessageType.RuleError, "���˹���δ�����ɹ�");
                    return false;
                }

                checkResult = new List<Error>();
                IEnumTopologyErrorFeature enErrorFeature = (m_Topology as IErrorFeatureContainer).get_ErrorFeatures((m_Topology.FeatureDataset as IGeoDataset).SpatialReference, this.m_TopologylRule, (m_Topology.FeatureDataset as IGeoDataset).Extent, true, true);
                ITopologyErrorFeature errFeature= enErrorFeature.Next();
                while (errFeature != null)
                {
                    TopoError err = new TopoError();
                    err.DefectLevel = this.DefectLevel;
                    err.ErrorType = enumErrorType.Topology;
                    err.LayerName = this.m_psRuleParas.arraySeledLayers[0];
                    err.LayerID= m_TopologylRule.OriginClassID;
                    if (this.m_psRuleParas.arraySeledLayers.Count > 1)
                        err.ReferLayerName = this.m_psRuleParas.arraySeledLayers[1];

                    err.ReferLayerID = m_TopologylRule.DestinationClassID;
                    err.RuleType = (int)m_TopologylRule.TopologyRuleType;
                    err.RuleID = this.m_InstanceID;
                    err.Description = this.InstanceName;
                    err.OID = errFeature.OriginOID;
                    err.ReferOID = errFeature.DestinationOID.ToString();
                    err.JHLX = errFeature.ShapeType;

                    checkResult.Add(err);

                    errFeature = enErrorFeature.Next();
                }

                return true;
            }
            catch(Exception exp)
            {
                SendMessage(enumMessageType.RuleError, "��ȡ���˽��ʧ��");
                SendMessage(enumMessageType.OperationalLog, exp.ToString());
                return false;
            }
        }


        private ITopology m_Topology;
        public ITopology Topology
        {
            set { this.m_Topology = value; }
        }

        private Dictionary<string, int> m_DictRank;
        public Dictionary<string, int> RankDictionary
        {
            set
            {
                this.m_DictRank = value;
            }
        }

        

        

        //public override bool Check(ref ICheckResult ppResult)
        //{
        //    return true;

        //    // ������ͼ��ı���ת��Ϊ��ʵ����
        //    InitTopoLayerArray();

        //    CTopoConstruct topoObj = new CTopoConstruct();

        //    // ��ʼ��
        //    string strTopoName = "" + m_strID + "_Topology";

        //    if (!topoObj.Init(ref m_psRuleParas, strTopoName, m_pSonEngineWks))
        //    {
        //        return false;
        //    }

        //    // ��������
        //    bool state = topoObj.ConstructTopo();
        //    if (state != true)
        //    {
        //        return state;
        //    }

        //    ppResult = null;

        //    return true;
        //}

          

        // �������˽ṹ����Ĳ���
        private bool ParseTopoPara(string strTopoPara, RuleExpression.LRTopoParas psRulePara)
        {
            if (strTopoPara == "")
            {
                return false;
            }
            string para_str = strTopoPara;
            m_psRuleParas.arraySeledLayers = new List<string>();
            m_psRuleParas.arrayOrigLayers = new List<string>();

            string para_tmp1, para_tmp2, para_tmp3;

            string[] strList = para_str.Split('#');
            para_tmp1 = strList[0];
            para_tmp2 = strList[1];
            para_tmp3 = strList[2];

            int j = 0;
            string[] strResult = para_tmp1.Split('|');
            m_psRuleParas.strTopoName = strResult[j++]; //��������
            m_psRuleParas.strStandardName = strResult[j++]; //��׼����
            m_psRuleParas.strSourceLayer = strResult[j]; //�����ͼ��,��Ŀ��ͼ��


            string[] strResult1 = para_tmp2.Split('|');
            for (j = 0; j < strResult1.Length; j++)
            {
                m_psRuleParas.arrayOrigLayers.Add(strResult1[j]);
            }

            string[] strResult2 = para_tmp3.Split('|');
            for (j = 0; j < strResult2.Length; j++)
            {
                if (strResult2[j] != "")
                {
                    m_psRuleParas.arraySeledLayers.Add(strResult2[j]);
                }
            }


            return true;
        }

        private class TopoError : Error
        {
            /// <summary>
            /// SourceLayerID
            /// </summary>
            public int LayerID { get; set; }

            /// <summary>
            /// TargetLayerID
            /// </summary>
            public int ReferLayerID { get; set; }

            /// <summary>
            /// �������ͣ�ָEsri�����˹���
            /// </summary>
            public int RuleType { get; set; }

            /// <summary>
            /// ��������
            /// </summary>
            public ESRI.ArcGIS.Geometry.esriGeometryType JHLX { get; set; }

            public override string ToSQLString()
            {
                StringBuilder strBuilder=new StringBuilder("Insert Into");
                // Fields
                strBuilder.Append(" LR_ResAutoTopo (CheckType,RuleInstID,DefectLevel,IsException,Reason,YSTC,MBTC,TPTC,SourceLayerID,TargetLayerID,SourceOID,TargetOID,JHLX,ArcGisRule) Values(");

                // Values
                strBuilder.Append("'','");
                strBuilder.Append(this.RuleID); strBuilder.Append("',");
                strBuilder.Append((int)this.DefectLevel); strBuilder.Append(",");
                strBuilder.Append(this.IsException); strBuilder.Append(",'");
                strBuilder.Append(this.Description.Replace("'", "''")); strBuilder.Append("','");

                strBuilder.Append(this.LayerName.Replace("'", "''")); strBuilder.Append("','");
                strBuilder.Append(this.ReferLayerName.Replace("'", "''")); strBuilder.Append("','");
                strBuilder.Append(COMMONCONST.Topology_Name);strBuilder.Append("',");
                strBuilder.Append(this.LayerID); strBuilder.Append(",");
                strBuilder.Append(this.ReferLayerID); strBuilder.Append(",");
                strBuilder.Append(this.OID); strBuilder.Append(",'");
                strBuilder.Append(this.ReferOID.Replace("'", "''")); strBuilder.Append("',");
                strBuilder.Append((int)this.JHLX);strBuilder.Append(",'");
                strBuilder.Append((int)this.RuleType); strBuilder.Append("'");
                strBuilder.Append(")");

                return strBuilder.ToString();

            }
        }
    }
}