using System;
using System.Collections;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace Hy.Check.Rule
{
    /// <summary>
    /// ��¼�����Ӧ�Ľṹ�����������
    /// </summary>
    public class RuleExpression
    {
        /// <summary>
        /// ������
        /// </summary>
        public class LRCodePara
        {
            // Ϊ�˿���ʹ�ù淶�������������ʹ��ID��
            public int nVersionID;

            // �������(����)
            public string strName;

            // �������
            public string strAlias;

            // ����ͼ������
            public string strFtName;

            // ��������
            public string strRemark;

            // Ҫ���м��ı����ֶ����ƣ��硰������;����������롱
            public string strCodeField;

            // ���ı������ͣ��硰��ǰ��׼�������롱
            public string strCodeType;

            // ���������ֶ�,���Ϊ��,��������б�����������ƵĶԱȼ��,��֮������жԱȼ��
            public string strNameField;

            // �����ı������硰LR_DictFtCode_Description��
            public string strCodeLibTable;

            // ��������ݱ����ƣ��硰LR_DictFtCode��
            public string strCodeDataTable;

            // ����淶������,���Ա������"����"�ֶ�,���,����ֶ���ҪΨһ���硰�������ù滮����Ϣ��������롱
            public string strCodeNorm;
        }

        // ���Ա�ṹ�����������
        public class LRFieldCheckPara
        {
            // �������(����)
            public string strName;

            // �������
            public string strAlias;

            // ��������
            public string strRemark;

            // �ֶδ��룬�硰DLMC��
            public bool m_bCode;

            // �ֶ�С��λ��
            public bool m_bDecimal;

            // �ֶδ����Լ��
            public bool m_bNull;

            // �ֶγ���
            public bool m_bLength;

            // �ֶ����ƣ��������� ���硰������롱
            public bool m_bName;

            // �ֶ�����
            public bool m_bType;

            //C#��û��CMapStringToString������ڴ˲���list<string> hehy2008��1��25��.
            public List<string> m_LyrFldMap; //public CMapStringToString m_LyrFldMap;����ͼ�����ƺ��ֶζ��ձ�
        }

        // �ֶ������Լ��
        public class LRFieldErrrorInfo
        {
            // Ψһ��ʾID
            public int nErrID;

            // �����ֶ�����
            public string strFieldName;

            // ���Ա�
            public string strAttrTabName;

            // ��׼�ֶ�����
            public string strStdFieldType;

            // ����ԭ��
            public string strErrorMsg;

            // �Ƿ�������
            public bool m_bIsException;
        }

        /// <summary>
        /// Ҫ�����ͱ�������������
        /// </summary>
        public class LRFtCodePara
        {
            // �������(����)
            public string strName;

            // �������
            public string strAlias;

            // ע��
            public string strRemark;

            // Ŀ��ͼ������
            public string strTargetLayer;

            // Ҫ���м��ı����ֶ�����
            public string strCodeField;
        }

        // ͼ���������ʼ���������
        public class LRLayerCheckPara
        {
            // �������(����)
            public string strName;

            // �������
            public string strAlias;

            // ��������
            public string strRemark;

            // �Ƿ���ͼ�����Ա����� - ͼ���Ӣ������
            public bool bAttrubuteName;

            // �Ƿ���ͼ������ - ͼ�����������
            public bool bLayerName;

            // �Ƿ���ͼ��ȱʧ
            public bool bIsNull;

            //����ͼ�������б�
            //ԭ����ȫ����飬��������ͼ�α�����Ա�Ϸ���һ�飻Ϊͳ�ƿ�����ֿ������
            public List<string> strLyrList;
        }

        // ͼ�����������Լ��
        public class LRLayerErrrorInfo
        {
            // Ψһ��ʾID
            public int nErrID;

            // �����ļ�����
            public string strLayerName;

            // ����ԭ��
            public string strErrorMsg;

            // �Ƿ�������
            public bool m_bIsException;
        }

        /// <summary>
        /// ����RuleInterface.h����д
        /// ���صĴ�����Ϣ��
        /// </summary>
        public class LRResultInfo
        {
            // Ҫ��ID
            public int OID;
            /// <summary>
            /// ��ʶ��
            /// </summary>
            public int BSM;
            /// <summary>
            ///  ��OID������OID�ַ���
            /// </summary>           
            public string OID2;
            /// <summary>
            /// ��BSM������BSM��
            /// </summary>
            public string BSM2;
            // ������Ϣ
            public string strErrInfo;
            // ��ע��Ϣ
            public string strRemark;
            // Ŀ���ֶ�
            public string strTargetField;
            // Ŀ��ͼ��
            public string strTargetLayer;
            // Ŀ��ͼ��
            public string strTargetLayer2;
        }

        /// <summary>
        /// �����׼��
        /// </summary>
        public struct LRStandard
        {
            // ����
            public string strName;

            // id
            public int nStandadID;

            // ��׼����id
            public int nStandadType;

            // ��������
            public DateTime time;
        } ;

        /// <summary>
        /// ������������
        /// </summary>
        //public struct PubType
        //{
        //    // Ϊ�˿���ʹ�ù淶�������������ʹ��ID��
        //    private int nVersionID;
        //    // �������(����)
        //    private string strAliasName;
        //    // �������
        //    private string strAlias;
        //    // ����ͼ������
        //    private string strFtName;
        //    // ��������
        //    private string strRemark;
        //    // Ҫ���м��ı����ֶ����ƣ��硰������;����������롱
        //    private string strCodeField;
        //    // ���ı������ͣ��硰��ǰ��׼�������롱
        //    private string strCodeType;
        //    // ���������ֶ�,���Ϊ��,��������б�����������ƵĶԱȼ��,��֮������жԱȼ��
        //    private string strNameField;
        //    // �����ı������硰LR_DictFtCode_Description��
        //    private string strCodeLibTable;
        //    // ��������ݱ����ƣ��硰LR_DictFtCode��
        //    private string strCodeDataTable;
        //    // ����淶������,���Ա������"����"�ֶ�,���,����ֶ���ҪΨһ���硰�������ù滮����Ϣ��������롱
        //    private string strCodeNorm;
        //};
        /// <summary>
        /// ������Ϣ
        /// </summary>
        public class LRTopoRule
        {
            // Դͼ��
            public string strSourceLayerName;
            // ���˹���
            public string strTopoRuleName;
            // Ŀ��ͼ��
            public string strTargetLayerName;
            // ��������
            public string strRuleDesc;
            // �������	
            public string strRuleAliasName;
            // ��������ͼƬID
            public int lPicID;
            // Դͼ�㼸������
            public int nGeoTSrc;
            // Ŀ��ͼ�꼸������
            public int nGeoTTarget;
            // Դͼ��Rank
            public int nRankSrc;
            // Ŀ��ͼ��Rank
            public int nRankTarget;
            // �Ƿ���ʾ����
            public bool bShowError;
        }

        /// <summary>
        /// ���˹�����
        /// </summary>
        public class LRTopoParas
        {
            // Base : ��������
            public string strTopoName;
            // Base : ��׼����
            public string strStandardName;
            // Base : �����ͼ��,��Ŀ��ͼ��
            public string strSourceLayer;
            // Base : �ݲ�
            public double dTolerance;

            // Layer : ԭʼͼ��
            public List<string> arrayOrigLayers;
            // Layer : ԭʼͼ�㼸������
            public List<int> arrayOrigGeoT;
            // Layer : ѡ��ͼ�㡪������ͼ��
            public List<string> arraySeledLayers;
            // Layer : ѡ��ͼ�㼸������
            public List<int> arraySeledGeoT;

            // Rank : ͼ������ȼ�
            public List<int> arrayRanks;

            // Rule : ��������
            public List<LRTopoRule> arrayRules;
        }

        // ������������
        public class LRPointDistPara
        {
            public string strName;
            public string strAlias;

            public string strRemark;

            // ��׼����
            public string strStdName;

            // Ŀ��ͼ��
            public string strTargetLayer;

            // Ŀ��ͼ��
            public string strBufferLayer;

            // ������С����
            public double dPointDist;

            // ��������
            public int nSearchType;

            // �Ƿ�������ͬ��
            public bool bSearchSamePt;
        }

   

        // 	Ͻ������ԱȲ�����
        public class DISTRICTPARA
        {
            public string strFtName; //����ͼ����
            public string strExpression; //�������������ʽ
            public string strDistrictField; //����Ͻ���ֶ���
            public double dbThreshold; //�ݲ���ֵ
            public int iClass; //ͳ�Ƽ����ؼ����缶���弶��
        }

        /// <summary>
        /// ��������࣬��Ҫ����RuleDistrict��RulePlot,RulePlotClass,RuleSheet,RuleStatAdminRegion
        /// </summary>
        public class RESULT
        {
            public int nOID;
            public int BSM;
            public string IDName; //��ʶ��
            public double dbCalArea; //�������
            public double dbSurveyArea; //�������
            public double dbError; //���ֵ
            public string strErrInfo; //������Ϣ
        }

        public struct XMap
        {
            public int nID;

            public string strName;
        } ;

        /// <summary>
        /// ���˴�����
        /// </summary>
        public class LRTopoStruct
        {
            // DestinationClassID
            public int nOriLayerID;

            // Destination�������
            public int nDestinationSubtype;

            // OriginClassID
            public int nDesitLayerID;

            // Origin�������
            public int nOriginSubtype;

            // ����ID
            public int nRule;

            // ��������
            public string strRuleName;
        }

        /// <summary>
        /// �ӱ߼�������
        /// </summary>
        public class LRJoinSidePara
        {
            //����
            public string strAlias;

            //����
            public string strRemark;

            // ��׼����
            public string strStdName;

            // Ҫ��ͼ��
            public string strFeatureLayer;

            // ��Χͼ��
            public string strBoundLayer;

            // ����ݴ�ֵ
            public double dLimit;

            //�ӱ��ֶμ���
            public List<string> arrayFieldName;

            //�б����ֶ�
            public string strInciseField;
        }

        public struct JoinSideInfo
        {
            public int OID1; //Ҫ�ر��
            public string strError; //����ԭ��
        } ;

       

        //SQL��ѯ�������ṹ��
        public struct SQLPARA
        {
            public string strSQLName; //�ʼ�������
            public string strAlias; //��ѯ�������
            public string strScript; //����
            public string strFtName; //Ҫ������,�Ƿ���Ҫ
            public string strWhereClause; //��ѯ����
        } ;

        //�Ƿ��ַ��������ṹ��
        public struct INVALIDPARA
        {
            public string strInvalidName; //�ʼ��������
            public string strAlias; //�ʼ��������
            public string strScript; //����
            public string strFtName; //Ҫ������
            public List<string> fieldArray; //�����ֶ����б�
            public List<string> charSetArray; //����ķǷ��ַ�
        } ;

        //Ƶ�ȼ������ṹ��
        public struct FREQUENCYPARA
        {
            public string strName; //�ʼ��������
            public string strAlias; //�ʼ��������
            public string strScript; //����
            public string strFtName; //Ҫ������
            public List<string> arrayFields; //�����ֶ�������
            public int nType; //���ͣ�0Ϊand,1Ϊor
            public int nMaxTime; //���ֵ����Ĵ���
            public int nMinTime; //���ֵ���С����
        } ;

        //��ֵ�������ṹ��
        public class BLANKVALPARA
        {
            public string strName; //�ʼ��������
            public string strAlias; //�ʼ��������
            public string strScript; //����
            public string strFtName; //Ҫ������
            public int iType; //�������,0Ϊ���У�1Ϊ����
            public List<string> fieldArray; //�����ֶ����б�
            public ArrayList fieldTypeArray; //�ֶ������б�   C++��ΪCArray<int,int> fieldTypeArray; 
        } ;

        //Ƶ�ȼ�����ṹ��
        public struct FRERESULT
        {
            public string value; //ֵ
            public List<string> featureIDArray; //Ҫ��ID�б�
        } ;

        //����������ṹ��
        public class AreaParameter
        {
            public string strName; //�ʼ��������
            public string strAlias; //�ʼ��������
            public string strScript; //����
            public string strFtName; //Ҫ������
            public double dbThreshold; //�����ֵ
            public List<string> fieldArray; //�����ֶ����б�
            public ArrayList fieldTypeArray; //�ֶ������б� C++��ΪCArray<int,int> fieldTypeArray; 
        } ;

        //���ȼ������ṹ��
        public struct LENGTHPARA
        {
            public string strName; //�ʼ��������
            public string strAlias; //�ʼ��������
            public string strScript; //����
            public string strFtName; //Ҫ������
            public double dbThreshold; //�����ֵ
            public List<string> fieldArray; //�����ֶ����б�
            public List<int> fieldTypeArray; //�ֶ������б�  C++��ΪCArray<int,int> fieldTypeArray; 
        } ;

     
        /// <summary>
        /// �����غϲ����ṹ��
        /// </summary>
        public struct CONDITIONCOINCIDEPARA
        {
            /// <summary>
            /// �ʼ�������
            /// </summary>
            public string strName;

            /// <summary>
            /// ����ͼ��
            /// </summary>
            public string strFtName;

            /// <summary>
            /// strFtName���ϵĲ�ѯ����
            /// </summary>
            public string strWhereClause;

            /// <summary>
            /// ����ͼ��2
            /// </summary>
            public string strFtName2;

            /// <summary>
            /// ����ԭ��
            /// </summary>
            public string strErrorReason;
        } ;

        /// <summary>
        /// �ռ�������ϵ�ʼ����ṹ��
        /// </summary>
        public struct SPATIALCONDITIONPARA
        {
            /// <summary>
            /// �ʼ�������
            /// </summary>
            public string strName;

            /// <summary>
            /// ����ͼ��
            /// </summary>
            public string strFtName;

            /// <summary>
            /// ����ͼ��Ҫ�жϿռ��ϵ��ͼ���б�
            /// </summary>
            public List<string> listFtName;

            /// <summary>
            /// strFtName���ϵĲ�ѯ����
            /// </summary>
            public string strWhereClause;

            /// <summary>
            ///  �ռ��ϵ
            /// </summary>
            public esriSpatialRelEnum eSpatialRel;

            /// <summary>
            /// ���������Ŀռ�Ҫ�����ֵ
            /// </summary>
            public int nIndex;

            /// <summary>
            /// ����ԭ��
            /// </summary>
            public string strErrorReason;
        } ;

        /// <summary>
        /// �����ϵ�ʼ����ṹ��
        /// </summary>
        public struct LINE2POLYGONPARA
        {
            /// <summary>
            /// �ʼ�������
            /// </summary>
            public string strName;

            /// <summary>
            /// ��ͼ������
            /// </summary>
            public string strLineName;

            /// <summary>
            /// ��ͼ������
            /// </summary>
            public string strPolygonName;
        } ;

        /// <summary>
        /// ��Խ����ʼ����ṹ��
        /// </summary>
        public struct SPATIALTHROUGHPARA
        {
            /// <summary>
            /// �ʼ�������
            /// </summary>
            public string strName;

            /// <summary>
            /// ����ͼ����
            /// </summary>
            public string strFtName;

            /// <summary>
            /// ����ͼ��Ҫ��Խ��ͼ���б�
            /// </summary>
            public List<string> ListFtName;
        } ;


        /// <summary>
        /// �ռ����Թ�ϵ�ʼ����ṹ������࣬��ʾ����ͼ��ĳһ�ֶεĴ����ֶ�ֵ���Լ���ȡ��ֵʱ��Ҫ�жϵĿռ��ϵͼ���б�͸�ͼ���Ӧ��SQL���
        /// </summary>
        public class SpatialAttrParam
        {
            /// <summary>
            /// ����ͼ��ĳһ�ֶεĴ����ֶ�ֵ
            /// </summary>
            public string strFieldCode;

            /// <summary>
            /// ������ֶδ��ڿռ��ϵ��ͼ��
            /// </summary>
            public List<string> listFtName;

            /// <summary>
            /// listFtName�����������
            /// </summary>
            public List<string> listSQLClause;
        }

        public struct LRGraphAttributeCollectPara
        {
            /// <summary>
            /// ����
            /// </summary>
            public string strAlias;

            /// <summary>
            /// ����
            /// </summary>
            public string strRemark;

            /// <summary>
            /// ��׼����
            /// </summary>
            public string strStdName;

            /// <summary>
            /// �����
            /// </summary>
            public string strGeographyObject;

            /// <summary>
            /// ͼ�߲�
            /// </summary>
            public string strGraphSpeckle;

            /// <summary>
            /// ������ֶμ���
            /// </summary>
            public List<string> arrayGeographyObjectField;

            /// <summary>
            /// ͼ�߲��ֶμ���
            /// </summary>
            public List<string> arrayGraphSpeckleField;

            /// <summary>
            /// xj �ཻ��bh����
            /// </summary>
            public string strCheckKind;

            /// <summary>
            /// ����ݴ�ֵ
            /// </summary>
            public double dLimit;
        } ;

        public struct FREFIELDVALS
        {
            // ���ڸ�Ψһֵ�ļ�¼����
            public int nRecordCount;

            // ���ڸ�Ψһֵ�ĸ����ֶε�ֵ
            public List<string> arrayFieldsVals;

            /// <summary>
            /// ��Ψһֵ��ɵ��ַ���
            /// </summary>
            public string strFieldsValues;

            
        } ;
    }
}