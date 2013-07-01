using System.Collections;
using System.Collections.Generic;

namespace Check.Utility
{
    public class COMMONCONST
    {
        public static bool IsGeographicCoordinateSystem = false;
        public static readonly string DB_Name_Temp = "tempdist.mdb";

        public static readonly string DB_Name_Base = "Base.gdb";
        public static readonly string DB_Name_Query = "Query.mdb";
        public static readonly string DB_Name_Topo = "Topo.gdb";
        public static readonly string DB_Name_Result = "Result.mdb";
        public static readonly string Dataset_Name = "Dataset";
        public static readonly string Topology_Name = "���˴���";

        /// <summary>
        /// topo���������Ƴ��� 
        /// </summary>
        public static readonly int TopoError_MaxCount = -1; ///-1Ϊ�������������
                                                            ///
        public static readonly string Folder_Name_PartlyCheck = "Ԥ��";

        public const string File_Name_SystemConfig = "SystemConfig.xml";
        public const string RelativePath_MXD = "Template\\Map";
        public const string File_Name_XSD  = "����ִ�и�ʽ��֤.xsd";


        /// <summary>
        /// Ĭ��ϵͳ������
        /// </summary>
        public static readonly string SYSDBName = "SysDb.mdb";

        public static readonly string MESSAGEBOX_WARING = "����";

        public static readonly string MESSAGEBOX_ERROR = "����";

        public static readonly string MESSAGEBOX_HINT = "��ʾ";

        public static readonly string TEMPLATE_MAP_PATH = @"\template\map";

        public static readonly string C_CONNECTION_TYPE = "System.Data.OleDb";

        /// <summary>
        /// ȡ���������������
        /// </summary>
        public static readonly int MAX_ROWS = 5000;

        /// <summary>
        /// ͼ���о���Ψһ�Ե��ֶ���������objectid֮�⣩��Ĭ��Ϊ��
        /// </summary>
        public static string m_strDistinctField = "";
        /// <summary>
        /// ����Ψһ���ֶκ�sql���ı仯
        /// </summary>
        private static string strDistinctField_SQL = "";
        /// <summary>
        /// ����Ψһ���ֶκ�sql���ı仯
        /// </summary>
        public static string m_strDistinctField_SQL
        {
            get
            {
                //if (strDistinctField_SQL != "")
                //    return strDistinctField_SQL;
                if (m_strDistinctField == "")
                {
                    strDistinctField_SQL = "OBJECTID";

                }
                else
                {
                    strDistinctField_SQL = "OBJECTID," + m_strDistinctField;
                }
                return strDistinctField_SQL;
            }
        }

        /// <summary>
        /// �����ͼ����ͳһ��ȥ��ǰ׺���ߺ�׺
        /// </summary>
        public static string RemovePlus = "";


        //���˴�������ֵ���û��ڴ�������ʱ����
        public static double TOPOTOLORANCE = 0.001;
        /// <summary>
        /// �������ݲ���ݱ����ߵĲ�ͬ����ͬ
        /// </summary>
        public static double dAreaThread = 400.0;
        /// <summary>
        /// �������ݲ�����߷�ĸ��dAreaThreadConst=dAreaThread
        /// </summary>
        public static double dAreaThreadConst = 0.04;

        /// <summary>
        /// ���߼���ݲ���ݱ����ߵĲ�ͬ����ͬ
        /// </summary>
        public static double dLengthThread = 0.2;
        /// <summary>
        /// ���߼���ݲ�����߷�ĸ��dLengthThreadConst=dLengthThread
        /// </summary>
        public static double dLengthTreadConst = 0.0002;
        /// <summary>
        /// ��ͼ������,����ʸ�������ȼ�鳬�޼��
        /// </summary>
        public static int nMapScale = 10000;




        /// <summary>
        /// �ӱ߼��ӱ߻��巶Χ
        /// </summary>
        public const double dEdgeBuffer = 300.0;
        /// <summary>
        /// �ֶ�����У�ͼ����ѡ�����
        /// </summary>
        public const double dSampleRatio = 0.1;

        //����80����ϵ�ĳ��̰������
        public const double SemiMajorAxis = 6378140;
        public const double SemiMinorAxis = 6356755.29;
        /// <summary>
        /// ��ƫ�ƾ���
        /// </summary>
        public const double FalseEasting = 500000;

        public const string HELP_CHECKRULE = "�ڶ���ȫ�����ص���ɹ������������ϸ��v1.1.chm";
        public const string HELP_SYSTEM="�ڶ���ȫ�����ص���ɹ���������������-�����ֲ�.chm";


        public const string CHECKTASKHISTORY = "Check.TaskHistory.xml";

        public const string TB_TKXS = "TKXS";
        public const string TB_TKXS1 = "TKXS1";
        public const string TB_DIST_TKXS = "DIST_TKXS";
        public const string DLTBFCName = "DLTB";

        public const string TABLENAME = "�������";

        public const string RESULT_TB_RESULT_ENTRY_RULE = "LR_ResultEntryRule";
    }
}