// DwgWriter.cpp : CDwgWriter ��ʵ��

#include "stdafx.h"
#include "DwgWriter.h"
#include "XMLFile.h"

// CDwgWriter


//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : дҪ���ൽCAD�ļ�
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::FeatureClass2Dwgfile(IFeatureClass* pFtCls)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    // TODO: �ڴ����ʵ�ִ���

    if (pFtCls == NULL) return S_FALSE;

    //m_pgrsDlg.Show();

    CString sLog;

    CString sFtClsName;
    CComBSTR bsFtClsName;
    IDatasetPtr pDataset = pFtCls;
    if (pDataset != NULL)
    {
        pDataset->get_Name(&bsFtClsName);
    }
    else
    {
        pFtCls->get_AliasName(&bsFtClsName);
    }

    sFtClsName = bsFtClsName;

    int pos = sFtClsName.ReverseFind('.');
    if (pos > 0)
    {
        sFtClsName = sFtClsName.Right(sFtClsName.GetLength() - pos - 1);
    }

    try
    {
        if (m_XDataCfgs.GetCount() == 0) //û����չ����
        {
            m_dwgWriter.FeatureClass2Dwgfile(pFtCls, NULL);
        }
        else
        {
            XDataAttrLists* pXDataAttrLists = NULL;

            m_XDataCfgs.Lookup(sFtClsName, pXDataAttrLists);
            m_dwgWriter.FeatureClass2Dwgfile(pFtCls, pXDataAttrLists);
        }
    }
    catch (...)
    {
        sLog = sFtClsName + "ͼ��д�뵽CAD�ļ�ʱ����";
        m_dwgWriter.WriteLog(sLog);

        m_pgrsDlg.Hide();
        return S_FALSE;
    }

    //m_pgrsDlg.Hide();

    return S_OK;
}


//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : ��ʼ��дDWG�ļ�
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::InitWriteDwg(BSTR sDwgFile, BSTR sTemplateFile)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    CComBSTR bsTempletFile = sTemplateFile;

    USES_CONVERSION;
    CString sOutFile = W2A(sDwgFile);
    m_dwgWriter.m_szCadTempFile = bsTempletFile;
    m_dwgWriter.PrepareOutPut(sOutFile);

    m_pgrsDlg.CreateDlg();
    m_dwgWriter.m_pProgressBar = &m_pgrsDlg.m_progressBar;
    m_pgrsDlg.Show();

    return S_OK;
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : ������չ���������ļ�
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::put_XDataXMLConfigFile(BSTR sXMLFile)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    if (sXMLFile == NULL) return S_OK;

    CString sXml = sXMLFile;
    if (sXml.IsEmpty()) return S_OK;

    m_XDataCfgs.RemoveAll();

    try
    {
        CXMLFile xmlfile;
        xmlfile.load(sXml);
        MSXML2::IXMLDOMNodePtr pNode;
        MSXML2::IXMLDOMNodePtr pExtAttrNode = NULL;
        xmlfile.GetNode("LAYERS", pNode);
        if (pNode == NULL)
        {
            //AfxMessageBox("XML�����ļ�����ȷ�����顣");
            m_dwgWriter.WriteLog("XML�����ļ�����ȷ�����顣");
            return S_FALSE;
        }
        pNode = pNode->GetfirstChild();
        if (pNode == NULL)
        {
            //AfxMessageBox("XML�����ļ�����ȷ�����顣");
            m_dwgWriter.WriteLog("XML�����ļ�����ȷ�����顣");
            return S_FALSE;
        }
        CComBSTR bsNodeName;
        CComBSTR bsExtAttrs;
        CString sLayerName;
        CString sRegAppName;
        CString sExtAttrs;
        while (pNode != NULL)
        {
            //�õ�ͼ����
            pNode->get_nodeName(&bsNodeName);
            sLayerName = bsNodeName;

            //ȥ��ǰ���_ǰ׺,������ֿ�ͷ�Ľڵ�����
            CString sSign = "";
            sSign = sLayerName.Mid(0, 1);
            if (sSign.CompareNoCase("_") == 0)
            {
                sLayerName = sLayerName.Mid(1);
            }

            XDataAttrLists* pExtAttrs = new XDataAttrLists();
            //�õ�ͼ���µ�ע��Ӧ����
            if (pNode->hasChildNodes())
            {
                pExtAttrNode = pNode->GetfirstChild();
                while (pExtAttrNode != NULL)
                {
                    pExtAttrNode->get_nodeName(&bsNodeName);
                    sRegAppName = bsNodeName;

                    //ȥ��ǰ���_ǰ׺,������ֿ�ͷ�Ľڵ�����
                    sSign = sRegAppName.Mid(0, 1);
                    if (sSign.CompareNoCase("_") == 0)
                    {
                        sRegAppName = sRegAppName.Mid(1);
                    }

                    pExtAttrNode->get_text(&bsExtAttrs);
                    sExtAttrs = bsExtAttrs;
                    CStringList* pAttrLst = new CStringList();
                    //����ע��Ӧ�����µ������ֶ�����
                    CString sAttr;
                    int iPos  = sExtAttrs.Find(',');
                    while (iPos > 0)
                    {
                        sAttr = sExtAttrs.Mid(0, iPos);
                        sExtAttrs = sExtAttrs.Mid(iPos + 1);
                        if (!sAttr.IsEmpty())
                        {
                            pAttrLst->AddTail(sAttr);
                        }
                        iPos  = sExtAttrs.Find(',');
                    }
                    if (iPos == -1)
                    {
                        if (!sExtAttrs.IsEmpty())
                        {
                            pAttrLst->AddTail(sExtAttrs);
                        }
                    }
                    pExtAttrs->SetAt(sRegAppName, pAttrLst);
                    //�õ���һ��ע��Ӧ����������
                    pExtAttrNode = pExtAttrNode->GetnextSibling();
                }
            }

            m_XDataCfgs.SetAt(sLayerName, pExtAttrs);
            //�õ���һ��ͼ�����չ���Ե�����
            pNode = pNode->GetnextSibling();
        }
    }
    catch (...)
    {
		m_dwgWriter.WriteLog("����XML�ļ��������顣");
        return S_FALSE;
    }

    return S_OK;
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : ������־����·��
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::put_LogFilePath(BSTR newVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    m_dwgWriter.PutLogFilePath(newVal);

    return S_OK;
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : ��ɶ�DWG�ļ���д����������������Դ
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::Close(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    m_pgrsDlg.Close();

    m_dwgWriter.WriteLog("���CAD�ļ���д�롣");

    return S_OK;
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : �α�д��DWG�ļ�
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::Cursor2Dwgfile(BSTR sFeatureClass, IFeatureCursor* pFtCur, LONG numFeatures)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    CString sFtCls = sFeatureClass;

    CString sLog;
    sLog.Format("��ʼ��%sͼ���ָ������д��CAD�ļ���", sFtCls);
    m_dwgWriter.WriteLog(sLog);

	try
	{
		m_XDataCfgs.Lookup(sFtCls, m_dwgWriter.m_pXDataAttrLists);
		m_dwgWriter.Cursor2Dwgfile(pFtCur, sFeatureClass, numFeatures);
	}
	catch (...)
	{
		sLog = sFtCls + "ͼ��д�뵽CAD�ļ�ʱ����";
		m_dwgWriter.WriteLog(sLog);
		m_pgrsDlg.Hide();
		return S_FALSE;
	}

    sLog.Format("��ɶ�%sͼ�����ݵ�д�룬д��Ҫ�ظ�����%d", sFtCls, numFeatures);
    m_dwgWriter.WriteLog(sLog);

    return S_OK;
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : ���ö��ձ�
//������� :
//�� �� ֵ :
//��    �� : 2008/10/29,BeiJing.
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::SetCompareTable(BSTR sCompareField, ITable* pCompareTable)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    m_dwgWriter.m_sCompareField = sCompareField;
    m_dwgWriter.m_pCompareTable = pCompareTable;

    return S_OK;

}


//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : �α�д��DWG�ļ�ָ����ͼ��
//������� :
//�� �� ֵ :
//
//
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
STDMETHODIMP CDwgWriter::Cursor2DwgLayer(BSTR sFeatureClass, IFeatureCursor* pFtCur, LONG numFeatures, BSTR sDwgLayer)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    CString sFtCls = sFeatureClass;
    CString sCadLayer = sDwgLayer;

    CString sLog;
    sLog.Format("��ʼ��%sͼ�������д�뵽CAD��%sͼ�㡣", sFtCls, sCadLayer);
    m_dwgWriter.WriteLog(sLog);

	try
	{
		m_dwgWriter.m_sDwgLayer = sDwgLayer;
		m_XDataCfgs.Lookup(sFtCls, m_dwgWriter.m_pXDataAttrLists);
		m_dwgWriter.Cursor2Dwgfile(pFtCur, sFeatureClass, numFeatures);
	}
	catch (...)
	{
		sLog = sFtCls + "ͼ��д�뵽CAD�ļ�ʱ����";
		m_dwgWriter.WriteLog(sLog);
		m_pgrsDlg.Hide();
		return S_FALSE;
	}

    sLog.Format("��ɰ�%sͼ������д�뵽CAD��%sͼ�㣬д��Ҫ�ظ�����%d", sFtCls, sCadLayer, numFeatures);
    m_dwgWriter.WriteLog(sLog);

    return S_OK;
}

STDMETHODIMP CDwgWriter::SetCompareField2(BSTR sConfigField, BSTR sGdbField)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	m_dwgWriter.m_bWidthCompareField2 = FALSE;
	CString csConfigField = sConfigField;
	CString csGdbField = sGdbField;
	if (csConfigField.IsEmpty() || csGdbField.IsEmpty())
	{
		return FALSE;
	}

	m_dwgWriter.m_bWidthCompareField2 = TRUE;
	m_dwgWriter.m_csConfigField2 = csConfigField;
	m_dwgWriter.m_csGdbField2 = csGdbField;

	return S_OK;
}
