using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using Hy.Check.Define;
using Hy.Check.Utility;


namespace Hy.Check.Rule
{
    public class RuleLayerInt : BaseRule
    {
        //���ݱ�׼����,������״���滮���ؽ����ݵ�
        private RuleExpression.LRLayerCheckPara m_pLayerPara = new RuleExpression.LRLayerCheckPara();
        private DataTable dtLayer = new DataTable();
        private string m_RuleName;
        public RuleLayerInt()
        {
            m_RuleName = "ͼ�������Լ��";
        }

        public override enumErrorType ErrorType
        {
            get
            {
                return enumErrorType.LayerIntegrity;
            }
        }

        public override string Name
        {
            get { return m_RuleName; }
        }

        public override IParameterSetter GetParameterSetter()
        {
            return null;
        }

        public override void SetParamters(byte[] objParamters)
        {
            BinaryReader pParameter = new BinaryReader(new MemoryStream(objParamters));
            if (m_pLayerPara != null)
            {
                m_pLayerPara = null;
            }
            m_pLayerPara = new RuleExpression.LRLayerCheckPara();

            pParameter.BaseStream.Position = 0;

            int nCount1 = pParameter.ReadInt32();
            m_pLayerPara.bIsNull = Convert.ToBoolean(pParameter.ReadInt32());
            m_pLayerPara.bAttrubuteName = Convert.ToBoolean(pParameter.ReadInt32());
            m_pLayerPara.bLayerName = Convert.ToBoolean(pParameter.ReadInt32());

            //�����ַ���
            int nSize = sizeof(int) * 4;
            Byte[] bb = new byte[nCount1 - nSize];
            pParameter.Read(bb, 0, nCount1 - nSize);
            string para_str = Encoding.Default.GetString(bb);

            para_str.Trim();

            string[] strResult = para_str.Split('|');

            int i = 0;
            m_pLayerPara.strAlias = strResult[i++];
            m_pLayerPara.strRemark = strResult[i++];

            m_pLayerPara.strLyrList = new List<string>();

            for (int j = i; j < strResult.Length; j++)
            {
                m_pLayerPara.strLyrList.Add(strResult[j]);
            }
        }

        public override bool Verify()
        {
            string strSQL = "select AttrTableName,LayerName,LayerOption from LR_DicLayer";
            dtLayer = Common.Utility.Data.AdoDbHelper.GetDataTable(SysDbHelper.GetSysDbConnection(), strSQL);
            if (dtLayer == null)
            {
                SendMessage(enumMessageType.Exception, "��ϵͳ�����Ҳ�����׼��ͼ���б�");
                return false;
            }
            return true;
        }

        public override bool Check(ref List<Error> checkResult)
        { 
            try
            {
                List<Error> pResult = new List<Error>();
                checkResult = pResult;

                IFeatureWorkspace ipFtWS = (IFeatureWorkspace) m_BaseWorkspace;

                IWorkspace ipWks = (IWorkspace) ipFtWS;
                IEnumDatasetName ipDatasetNames = ipWks.get_DatasetNames(esriDatasetType.esriDTFeatureDataset);
                IDatasetName ipDatasetName = ipDatasetNames.Next();
                IFeatureDataset ipDataset = null;
                if (ipDatasetName != null)
                {
                    ipDataset = ipFtWS.OpenFeatureDataset(ipDatasetName.Name);
                }

                List<IFeatureLayer> listFtLayer = new List<IFeatureLayer>();
                Common.Utility.Esri.FeatClsOperAPI.GetFeatLayerInDs(ipDataset, ref listFtLayer);
       
                //����forѭ��������������add by wangxiang 20111201
                int flag = 0;
                foreach (DataRow drLayer in dtLayer.Rows)
                {
                    if (drLayer != null)
                    {
                        string strLayer = drLayer["AttrTableName"].ToString();
                        string strLayerName = drLayer["LayerName"].ToString();
                        IFeatureClass pFtCls = null;
                        int i = 0;
                        for (i = 0; i < listFtLayer.Count && flag < listFtLayer.Count; i++)
                        {
                            IFeatureLayer pFtLayer = listFtLayer[i];
                            //IDataset pDs = (IDataset) pFtLayer.FeatureClass;

                            if (strLayerName == pFtLayer.Name)
                            {
                                try
                                {
                                    pFtCls = ipFtWS.OpenFeatureClass(strLayer);
                                }
                                catch
                                {
                                    LayerError LayerErrInfo = new LayerError();
                                    LayerErrInfo.DefectLevel = this.DefectLevel;
                                    LayerErrInfo.m_strRuleInstID = this.m_InstanceID;
                                    LayerErrInfo.strLayerName = strLayerName;
                                    //LayerErrInfo.strErrorMsg = "ͼ���������ϱ�׼(��׼��" + strLayer + "(" + strLayerName + "))��";
                                    LayerErrInfo.strErrorMsg = strLayerName + "(" + strLayer + ")���ʧ�ܣ�";

                                    pResult.Add(LayerErrInfo);
                                }
                                flag++;
                                break;
                            }
                        }

                        if (i >= listFtLayer.Count)
                        {
                            try
                            {
                                pFtCls = ipFtWS.OpenFeatureClass(strLayer);
                            }
                            catch
                            {
                                if (drLayer["LayerOption"].ToString() == "bx")
                                {
                                    LayerError LayerErrInfo = new LayerError();
                                    LayerErrInfo.DefectLevel = this.DefectLevel;
                                    LayerErrInfo.m_strRuleInstID = this.m_InstanceID;
                                    LayerErrInfo.strLayerName = strLayerName;
                                    //LayerErrInfo.strErrorMsg = "ȱʧ��ѡͼ�㣺" + strLayer + "(" + strLayerName + ")";
                                    LayerErrInfo.strErrorMsg = string.Format(Helper.ErrMsgFormat.ERR_310100001_1,strLayerName + "(" + strLayer + ")");
                                   
                                    pResult.Add(LayerErrInfo);
                                }
                            }
                        }
                        if (pFtCls != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFtCls);
                        }
                    }
                }

                if (ipDataset != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ipDataset);
                }                
            }
            catch (Exception ex)
            {
                SendMessage(enumMessageType.Exception, ex.ToString());
                return false;
            }

            return true;
        }


        private class LayerError : Error
        {
            public LayerError()
            {
                this.strLayerName = "";
                this.strErrorMsg = "";
                this.m_strRuleInstID = "";
            }
            // Ψһ��ʾID
            public int nErrID;

            // �����ļ�����
            public string strLayerName;

            // ����ԭ��
            public string strErrorMsg;

            // �Ƿ�������
            public bool m_bIsException;

            public string m_strRuleInstID;

            public override string ToSQLString()
            {
                if (this.m_strRuleInstID == null)
                    this.m_strRuleInstID = "";

                StringBuilder strBuilder = new StringBuilder("Insert into LR_ResIntLayer(");
                strBuilder.Append("ErrID,ErrorReason,ErrorLayerName,IsException,RuleInstID,Remark,DefectLevel)");
                strBuilder.Append(" Values(");

                strBuilder.Append(this.nErrID); strBuilder.Append(",'");
                strBuilder.Append(this.strErrorMsg.Replace("'", "''")); strBuilder.Append("','");
                strBuilder.Append(this.strLayerName.Replace("'", "''")); strBuilder.Append("',");
                strBuilder.Append(this.IsException); strBuilder.Append(",'");
                strBuilder.Append(this.m_strRuleInstID.Replace("'", "''")); strBuilder.Append("','");
                strBuilder.Append(this.Description.Replace("'", "''"));strBuilder.Append("',");
                strBuilder.Append((int)this.DefectLevel);
                strBuilder.Append(")");

                return strBuilder.ToString();

            }
        }

    }
}