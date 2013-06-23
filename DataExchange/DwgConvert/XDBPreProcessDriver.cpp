// XDBPreProcessDriver.cpp: implementation of the XDBPreProcessDriver class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "XDBPreProcessDriver.h"
#include <math.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#define new DEBUG_NEW
#endif



//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

XDBPreProcessDriver::XDBPreProcessDriver()
{
    m_pInWS = NULL;
    m_pOutWS = NULL;
    //joinExdDriver = NULL;
    m_ipExtendTable = NULL;
    ipCompareTable = NULL;

    pTextProgressCtrl = NULL;

    m_pSysFtWs = NULL;
}

XDBPreProcessDriver::~XDBPreProcessDriver()
{
    //if (joinExdDriver)
    //    delete joinExdDriver;
}

//����vb�еĺ������ͷ�ϵͳ��Ȩ����ֹ�е�ʱ������޷�ˢ�µ�����
void XDBPreProcessDriver::DoEvents()
{
    MSG message;
    for (int i = 0; i < 10; i++)
    {
        if (::PeekMessage(&message, NULL, 0, 0, PM_REMOVE))
        {
            ::TranslateMessage(&message);
            ::DispatchMessage(&message);
        }
    }
}

/************************************************************************
��Ҫ���� :д������־
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::WriteLog(CString sLog)
{
    if (!sLog.IsEmpty())
    {
        CTime dtCur = CTime::GetCurrentTime();
        CString sLogTime = dtCur.Format("%Y/%m/%d %H:%M:%S");
        sLog = sLogTime + "-" + sLog;
        m_LogList.AddTail(sLog);
    }
}

void XDBPreProcessDriver::SaveLogList(BOOL bShow/*=TRUE */)
{

    if (m_LogList.GetCount() > 0)
    {
        CTime dtCur = CTime::GetCurrentTime();
        CString sName = dtCur.Format("%y%m%d_%H%M%S");
        CString sLogFileName;
        sLogFileName.Format("%sDwgת����־_%s.log", GetLogPath(), sName);

        CStdioFile f3(sLogFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
        for (POSITION pos = m_LogList.GetHeadPosition(); pos != NULL;)
        {
            f3.WriteString(m_LogList.GetNext(pos) + "\n");
        }
        f3.Close();
        WinExec("Notepad.exe " + sLogFileName, SW_SHOW);
        m_LogList.RemoveAll();
    }

}

/************************************************************************
��Ҫ���� : �ֲ㴦��
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
BOOL XDBPreProcessDriver::AutoSplitLayers(IWorkspace* pInWS, IWorkspace* pOutWS)
{
    //CWaitCursor w;
    CString sLogInfo;
    sLogInfo.Format("2 ��ʼ�Զ��ֲ�");
    WriteLog(sLogInfo);

    PrgbarSetText("��ʼ�Զ��ֲ�ǰ��׼������...");
    PrgbarRange(0, 4);
    PrgbarSetPos(0);
    PrgbarStepIt();

    CMapStringToString mapLayers;

    HRESULT hr;
    ITablePtr pFtClsLayers;
    hr = m_pSysFtWs->OpenTable(CComBSTR("ALL_LAYERS"), &pFtClsLayers);

    long numLayers;
    hr = pFtClsLayers->RowCount(NULL, &numLayers);

    if (numLayers <= 0) return FALSE;

    long lLayerName;
    //long lLayerAlias;
    pFtClsLayers->FindField(CComBSTR("GDB_LAYER"), &lLayerName);
    //pFtClsLayers->FindField(CComBSTR("LAYER_ALIAS"), &lLayerAlias);

    IEsriCursorPtr pRowCursor;
    hr = pFtClsLayers->Search(NULL, VARIANT_FALSE, &pRowCursor);

    IEsriRowPtr pRowLayerName;
    CComVariant vtVal;
    CString sLayerName;
    CString sLayerAlias;

    while (pRowCursor->NextRow(&pRowLayerName) == S_OK)
    {
        pRowLayerName->get_Value(lLayerName, &vtVal);
        sLayerName = vtVal.bstrVal;

        //pRowLayerName->get_Value(lLayerAlias, &vtVal);
        sLayerAlias = "";

        mapLayers.SetAt(sLayerName, sLayerAlias);
    }

    IFeatureWorkspacePtr pInFtWS = pInWS;


    //��
    PrgbarSetText("���ڶ���ͼ����зֲ�...");
    PrgbarStepIt();
    IFeatureClassPtr pLineFtCls;
    hr = pInFtWS->OpenFeatureClass(CComBSTR("Line"), &pLineFtCls);
    if (SplitOneLayer(&mapLayers, pLineFtCls, pOutWS))
    {
        WriteLog("���Lineͼ�����ݵķֲ㡣");
        m_pLogRec->WriteLog("���Lineͼ�����ݵķֲ㡣");
    }


    //��
    PrgbarSetText("���ڶԵ�ͼ����зֲ�...");
    PrgbarStepIt();
    IFeatureClassPtr pPointFtCls;
    hr = pInFtWS->OpenFeatureClass(CComBSTR("Point"), &pPointFtCls);
    if (SplitOneLayer(&mapLayers, pPointFtCls, pOutWS))
    {
        WriteLog("���Pointͼ�����ݵķֲ㡣");
        m_pLogRec->WriteLog("���Pointͼ�����ݵķֲ㡣");
    }

    //ע��
    PrgbarSetText("���ڶ�ע��ͼ����зֲ�...");
    PrgbarStepIt();
    IFeatureClassPtr pAnnoFtCls;
    hr = pInFtWS->OpenFeatureClass(CComBSTR("Annotation"), &pAnnoFtCls);
    if (SplitAnnotationLayer("Annotation", &mapLayers, pAnnoFtCls, pOutWS))
    {
        WriteLog("���Annotationͼ�����ݵķֲ㡣");
        m_pLogRec->WriteLog("���Annotationͼ�����ݵķֲ㡣");
    }

    hr = pInFtWS->OpenFeatureClass(CComBSTR("DwgDimension"), &pAnnoFtCls);
    if (SplitAnnotationLayer("DwgDimension", &mapLayers, pAnnoFtCls, pOutWS))
    {
        WriteLog("���DwgDimensionͼ�����ݵķֲ㡣");
        m_pLogRec->WriteLog("���DwgDimensionͼ�����ݵķֲ㡣");
    }


    //���ı�(ע��)
    /*IFeatureClassPtr pLabelTxtFtCls;
    hr = pInFtWS->OpenFeatureClass(CComBSTR("LabelTxt"), &pLabelTxtFtCls);
    SplitOneLayer(&mapLayers, pLabelTxtFtCls, m_pOutWS);
    WriteLog("���LabelTxtͼ�����ݵķֲ㡣");*/


    PrgbarSetText("");
    PrgbarSetPos(0);

    return TRUE;
}

ITablePtr XDBPreProcessDriver::GetExtendCompareTable(ITablePtr ipTable)
{
    if (m_pEnumConfigDatasetName == NULL)
    {
        IWorkspacePtr ipSysWksp = m_pSysFtWs;

        ipSysWksp->get_DatasetNames(esriDTTable, &m_pEnumConfigDatasetName);
    }
    else
    {
        m_pEnumConfigDatasetName->Reset();
        IDatasetNamePtr ipDatasetName = NULL;
        m_pEnumConfigDatasetName->Next(&ipDatasetName);
        IDatasetPtr ipDataset = ipTable;
        CComBSTR bsLayerName, bsTableName;
        ipDataset->get_Name(&bsLayerName);
        CString sLayerName = GetSdeFtClsName(bsLayerName);
        CString sTableName;
        while (ipDatasetName)
        {
            ipDatasetName->get_Name(&bsTableName);
            sTableName = bsTableName;
            if (sTableName.CompareNoCase(sLayerName) == 0)
            {
                IUnknownPtr ipUnknown;
                ITablePtr ipTable;
                INamePtr ipName = ipDatasetName;
                ipName->Open(&ipUnknown);
                ipTable = ipUnknown;
                return ipTable;
            }
            m_pEnumConfigDatasetName->Next(&ipDatasetName);
        }
    }

    return NULL;
}



BOOL XDBPreProcessDriver::JoinExtendTable()
{
    /*CWaitCursor w;
    CString sLogInfo;
    sLogInfo.Format("3 �ҽ���չ����");
    WriteLog(sLogInfo);
    //pTextProgressCtrl->ShowWindow(SW_SHOW);

    if (m_pOutWS)
    {
        IEnumDatasetPtr ipEnumDS;
        m_pOutWS->get_Datasets(esriDTFeatureDataset, &ipEnumDS);
        IFeatureDatasetPtr ipFeatureDS ;
        IDatasetPtr ipDataset = NULL;
        ipEnumDS->Next(&ipDataset);
        ipFeatureDS = ipDataset;
        if (ipFeatureDS)
        {
            ipFeatureDS->get_Subsets(&ipEnumDS);
        }
        else
        {
            m_pOutWS->get_Datasets(esriDTFeatureClass, &ipEnumDS);
        }

        CMapStringToString mapRegAppNames;
        //�õ�����FeatureClass��Ӧ����չ����ע��Ӧ������
        GetExtraAttribRegAppNames(mapRegAppNames);


        //�õ���չ�����ֶε�ע��Ӧ����
        CStringList lstAppNames;
        GetRegAppNames(lstAppNames);


        ipEnumDS->Reset();
        ipEnumDS->Next(&ipDataset);
        while (ipDataset)
        {
            ITablePtr ipTable = ipDataset;
            if (ipTable != NULL)
            {
                CComBSTR bsLayerName;
                ipDataset->get_Name(&bsLayerName);
                CString sLayerName = GetSdeFtClsName(bsLayerName);
                ITablePtr ipExtendFieldConfigTable = GetExtendFieldsConfigTable(sLayerName);//GetExtendCompareTable(ipTable);
                if (ipExtendFieldConfigTable)
                {
                    //pTextProgressCtrl->UpdateWindow();
                    //�ҽ���չ����
                    if (joinExdDriver == NULL)
                    {
                        joinExdDriver = new XJoinExtendTable;
                        joinExdDriver->m_pProgressCtrl = pTextProgressCtrl;

                        if (m_ipExtendTable == NULL)
                        {
                            IFeatureWorkspacePtr ipSourceFeatureWks = m_pInWS ;
                            ipSourceFeatureWks->OpenTable(CComBSTR("ExtendTable"), &m_ipExtendTable);
                        }
                        joinExdDriver->m_ipExtendTable = m_ipExtendTable;
                    }

                    CString sLogInfo;
                    sLogInfo.Format("���ڶ�����:%s������չ���Թҽ�", sLayerName);
                    WriteLog(sLogInfo);

                    joinExdDriver->m_pLogList = &m_LogList;
                    joinExdDriver->m_ipConfigTable = ipExtendFieldConfigTable;
                    joinExdDriver->m_ipTargetTable = ipTable;

                    joinExdDriver->m_mapRegAppName = &mapRegAppNames;//ͼ���Ӧ����չ����Ӧ������

                    joinExdDriver->m_lstRegApps = &lstAppNames;


              
                    joinExdDriver->AddExtendFieldsValue(sLayerName);

                    sLogInfo.Format("����:%s��չ���ԹҽӲ������", sLayerName);
                    WriteLog(sLogInfo);
                }
            }
            ipEnumDS->Next(&ipDataset);
        }
    }

    sLogInfo.Format("3 �ҽ���չ�������!");
    WriteLog(sLogInfo);*/

    return TRUE;
}

/********************************************************************
��Ҫ���� : �ҽ���չ����
������� :
�� �� ֵ :

//
�޸���־ :
*********************************************************************/
bool XDBPreProcessDriver::JoinExtendTable2(void)
{

    //CWaitCursor w;
    CString sLogInfo;
    sLogInfo.Format("3 �ҽ���չ����");
    WriteLog(sLogInfo);

    if (m_pInWS == NULL || m_pOutWS == NULL) return false;

    IFeatureDatasetPtr pFtDataset;
    ((IFeatureWorkspacePtr)m_pInWS)->OpenFeatureDataset(CComBSTR("DWG_X"), &pFtDataset);
    if (pFtDataset == NULL) return false;

    IEnumDatasetPtr pEnumDS;
    pFtDataset->get_Subsets(&pEnumDS);
    if (pEnumDS == NULL) return false;


    CMapStringToString mapRegAppNames;
    //�õ�����FeatureClass��Ӧ����չ����ע��Ӧ������
    GetExtraAttribRegAppNames(mapRegAppNames);

    //�õ���չ�����ֶε�ע��Ӧ����
    CStringList lstAppNames;
    GetRegAppNames(lstAppNames);

    //�ҽ���չ����
    XJoinExtendTable joinExdDriver;
    joinExdDriver.m_pProgressCtrl = pTextProgressCtrl;
    ITablePtr ipExtendTable;
    ((IFeatureWorkspacePtr)m_pInWS)->OpenTable(CComBSTR("ExtendTable"), &ipExtendTable);
    joinExdDriver.m_ipExtendTable = ipExtendTable;

    int iRst = 0;

    pEnumDS->Reset();
    IDatasetPtr ipDataset = NULL;
    pEnumDS->Next(&ipDataset);
    while (ipDataset != NULL)
    {
        ITablePtr ipTable = ipDataset;
        if (ipTable != NULL)
        {
            CComBSTR bsLayerName;
            ipDataset->get_Name(&bsLayerName);
            CString sLayerName = GetSdeFtClsName(bsLayerName);
            ITablePtr ipExtendFieldConfigTable = GetExtendFieldsConfigTable(sLayerName);
            if (ipExtendFieldConfigTable)
            {
                CString sLogInfo;
                sLogInfo.Format("���ڶ�����:%s������չ���Թҽ�", sLayerName);
                WriteLog(sLogInfo);

                joinExdDriver.m_pLogList = &m_LogList;
                joinExdDriver.m_ipConfigTable = ipExtendFieldConfigTable;
                joinExdDriver.m_ipTargetTable = ipTable;

                joinExdDriver.m_mapRegAppName = &mapRegAppNames;//ͼ���Ӧ����չ����Ӧ������

                joinExdDriver.m_lstRegApps = &lstAppNames;


                /******************************************
                �޸�ԭ�� :
                
                
                * *****************************************/
                joinExdDriver.AddExtendFieldsValue(sLayerName);

                sLogInfo.Format("����:%s��չ���ԹҽӲ������", sLayerName);
                WriteLog(sLogInfo);

                //iRst = ipExtendFieldConfigTable->Release();

            }
        }

        pEnumDS->Next(&ipDataset);
    }

    sLogInfo.Format("3 �ҽ���չ�������!");
    WriteLog(sLogInfo);

    return true;
}



/*BOOL XDBPreProcessDriver::GetExtendTable()
{
if (m_ipExtendTable == NULL)
{
IFeatureWorkspacePtr ipSourceFeatureWks = m_pInWS ;
ipSourceFeatureWks->OpenTable(CComBSTR("ExtendTable"), &m_ipExtendTable);
}
return m_ipExtendTable != NULL;
}
*/
/*BOOL XDBPreProcessDriver::GetCompareTable()
{
//IUnknown* pUnk = 0;
//Sys_Getparameter(SYSSET_MDB_CONNECT, (void * *) &pUnk);
//IWorkspacePtr ipSysWksp = pUnk;
IWorkspacePtr ipSysWksp = API_GetSysWorkspace();

IFeatureWorkspacePtr ipFeatureWorkspace(ipSysWksp);
if (ipFeatureWorkspace != NULL)
{
ipFeatureWorkspace->OpenTable(CComBSTR("CAD2GDB"), &ipCompareTable);
}
return ipCompareTable != NULL;
}*/

/*BOOL XDBPreProcessDriver::CopyFieldValue(IEsriRow* ipRow, IFeatureBuffer* pFeatureBuffer)
{
long lindex;
IFieldsPtr ipSFields, ipTFields;
long lFieldNum;
BSTR FieldName;

if (pFeatureBuffer == NULL)
return S_FALSE;
ipRow->get_Fields(&ipSFields);
pFeatureBuffer->get_Fields(&ipTFields);
ipSFields->get_FieldCount(&lFieldNum);
COleVariant var;
for (int i = 0; i < lFieldNum; i++)
{
IFieldPtr ipField;
ipSFields->get_Field(i, &ipField);
ipField->get_Name(&FieldName);

ipTFields->FindField(FieldName, &lindex);
if (lindex != -1)
{
ipRow->get_Value(i, var);
pFeatureBuffer->put_Value(lindex, var);
}
}
return TRUE;
}*/

/************************************************************************
��Ҫ���� : �������ƴ�ϵͳ���ݿ��еõ���չ�����ֶ����ñ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
ITable* XDBPreProcessDriver::GetExtendFieldsConfigTable(CString sLayerName)
{

    ITable* pRstTable = NULL;
    HRESULT hr = m_pSysFtWs->OpenTable(CComBSTR(sLayerName), &pRstTable);
    return pRstTable;

    /*if (m_pEnumConfigDatasetName == NULL)
    {
        IWorkspacePtr ipSysWksp = m_pSysFtWs;
        ipSysWksp->get_DatasetNames(esriDTTable, &m_pEnumConfigDatasetName);
    }

    ITable* pRstTable = NULL;

    CComBSTR bsTableName;
    CString sTableName;
    m_pEnumConfigDatasetName->Reset();
    IDatasetNamePtr ipDatasetName = NULL;
    m_pEnumConfigDatasetName->Next(&ipDatasetName);
    while (ipDatasetName)
    {
        ipDatasetName->get_Name(&bsTableName);
        sTableName = bsTableName;
        if (sTableName.CompareNoCase(sLayerName) == 0)
        {
            IUnknown* ipUnknown;
            ITablePtr ipTable;
            INamePtr ipName = ipDatasetName;
            ipName->Open(&ipUnknown);
            ipTable = ipUnknown;

    		pRstTable = ipTable.GetInterfacePtr();
    		//int iRst = ipTable->Release();
    		
    		//int iRst = ipUnknown->Release();
    		//ipName.Release();
    		ipDatasetName.Release();
    		break;
        }
    	ipDatasetName.Release();
        m_pEnumConfigDatasetName->Next(&ipDatasetName);
    }*/
}


/************************************************************************
��Ҫ���� :
������� :
�� �� ֵ :
//
�޸���־ :
************************************************************************/
//IFeatureWorkspacePtr XDBPreProcessDriver::GetSysWorkspace()
//{
//    if (m_pSysFtWs == NULL)
//    {
//        IUnknown* pUnk = 0;
//        pUnk = API_GetSysWorkspace();
//
//        m_pSysFtWs = pUnk;
//        if (m_pSysFtWs == NULL)
//        {
//            AfxMessageBox("�޷����ӵ�ϵͳ���ݿ⣬���顣");
//            return NULL;
//        }
//    }
//
//    return m_pSysFtWs;
//}

/************************************************************************
��Ҫ���� : �õ�FeatureClass��Ӧ����չ����ע��Ӧ������
������� :
�� �� ֵ :
//
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::GetExtraAttribRegAppNames(CMapStringToString& mapRegAppNames)
{
    mapRegAppNames.RemoveAll();
    if (m_pSysFtWs != NULL)
    {
        ITablePtr pTable;
        m_pSysFtWs->OpenTable(CComBSTR("EXTRA_ATTRIB_CONFIG"), &pTable);

        if (pTable != NULL)
        {
            HRESULT hr;
            IEsriCursorPtr pCursor;
            hr = pTable->Search(NULL, VARIANT_FALSE, &pCursor);
            if (pCursor != NULL)
            {
                long lFeatureClassNameFieldIndex, lRegAppName;
                pTable->FindField(CComBSTR("FeatureClassName"), &lFeatureClassNameFieldIndex);
                pTable->FindField(CComBSTR("RegAppName"), &lRegAppName);

                IEsriRowPtr pRow;
                CComVariant vtVal;
                pCursor->NextRow(&pRow);
                while (pRow != NULL)
                {
                    CString sFeatureClassName, sRegAppName;
                    //FeatureClassName
                    hr = pRow->get_Value(lFeatureClassNameFieldIndex, &vtVal);
                    if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
                    {
                        sFeatureClassName = vtVal.bstrVal;
                    }

                    //RegAppName
                    hr = pRow->get_Value(lRegAppName, &vtVal);
                    if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
                    {
                        sRegAppName = vtVal.bstrVal;
                    }

                    mapRegAppNames.SetAt(sFeatureClassName, sRegAppName);

                    pRow.Release();

                    hr = pCursor->NextRow(&pRow);
                }

                pCursor.Release();
            }

            pTable.Release();
        }
    }
}

/************************************************************************
��Ҫ���� : ����Ҫ��ID��MDB��ʽ����
������� :
�� �� ֵ :
//
//
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::CopyFeatureUIDTable()
{
    if (m_pInWS == NULL || m_pOutWS == NULL)
    {
        return;
    }

    IFeatureWorkspacePtr pTempFtWksp(m_pInWS);
    //IFeatureWorkspacePtr pTargetFtWksp(m_pOutWS);

    //��Դ���ݱ�
    ITablePtr pSrcTable;
    pTempFtWksp->OpenTable(CComBSTR("FeatureUID"), &pSrcTable);
    if (pSrcTable == NULL)
    {
        return;
    }

    // 	IFieldsEditPtr pFields(CLSID_Fields);
    // 	IFieldEditPtr pField(CLSID_Field);
    // 	pField->put_Name(CComBSTR("FEATURE_UID"));
    // 	pField->put_Type(esriFieldTypeString);
    // 	pField->put_Length(150);
    //
    // 	pFields->AddField(pField);
    //
    // 	//����Ŀ�ı�
    // 	ITablePtr pTargetTable;
    // 	pTargetFtWksp->CreateTable(CComBSTR("FeatureUID"), pFields, 0, 0, 0, &pTargetTable);
    // 	if (pTargetTable==NULL)
    // 	{
    // 		return;
    // 	}

    HRESULT hr;
    //src datasetname
    INamePtr pSrcName;
    IDatasetPtr pDataset(pSrcTable);
    pDataset->get_FullName(&pSrcName);
    IDatasetNamePtr pInputDsName(pSrcName);

    //target datasetname
    IDatasetNamePtr pOutDsName(CLSID_TableName);
    pOutDsName->put_Name(CComBSTR("FeatureUID"));
    //Target WorkspaceName
    IWorkspaceNamePtr ipTargetWorkspaceName(CLSID_WorkspaceName);
    ipTargetWorkspaceName->put_WorkspaceFactoryProgID(CComBSTR(_T("esriDataSourcesGDB.AccessWorkspaceFactory.1")));
    CComBSTR bsTargetPath;
    m_pOutWS->get_PathName(&bsTargetPath);
    hr = ipTargetWorkspaceName->put_PathName(bsTargetPath);
    pOutDsName->putref_WorkspaceName(ipTargetWorkspaceName);

    //IFields
    IFieldsPtr pSrcFields;
    pSrcTable->get_Fields(&pSrcFields);
    IFieldsPtr pTargetFields;
    IEnumFieldErrorPtr pEnumFieldErr;
    IFieldCheckerPtr ipFieldChecker(CLSID_FieldChecker);
    ipFieldChecker->Validate(pSrcFields, &pEnumFieldErr, &pTargetFields);

    IEnumInvalidObjectPtr pEnumInvalid;
    IFeatureDataConverterPtr pFeatureDataConverter(CLSID_FeatureDataConverter);
    hr = pFeatureDataConverter->ConvertTable(pInputDsName, NULL, pOutDsName, pTargetFields, 0, 1000, 0, &pEnumInvalid);
}


//ת��VARIANT��CString
CString XDBPreProcessDriver::GetStringByVar(VARIANT var)
{
    CString str;
    if (var.vt == VT_I2)
    {
        str.Format("%d", var.iVal);
    }
    else if (var.vt == VT_I4)
    {
        str.Format("%d", var.lVal);
    }
    else if (var.vt == VT_R4)
    {
        str.Format("%g", var.fltVal);
    }
    else if (var.vt == VT_R8)
    {
        str.Format("%g", var.dblVal);
    }
    else if (var.vt == VT_BSTR)
    {
        str = var.bstrVal;
    }
    else if (var.vt == VT_BOOL)
    {
        if (var.scode)
            str.Format( _T("%d") , TRUE );
        else
            str.Format( _T("%d") , FALSE );
    }
    else if (var.vt == VT_DATE)
    {
        COleDateTime time;
        time = var.date;
        str.Format("%d-%d-%d  %d:%d:%d", time.GetYear(), time.GetMonth(), time.GetDay(),
                   time.GetHour(), time.GetMinute(), time.GetSecond());
    }
    return str;
}

/************************************************************************
��Ҫ���� :�õ�ȥ��ǰ׺��Ҫ��������
������� :
�� �� ֵ :
//
�޸���־ :
************************************************************************/
CString XDBPreProcessDriver::GetSdeFtClsName(CComBSTR bsFtClsName)
{
    CString sFtClsName = CW2A(bsFtClsName);
    int iPos = sFtClsName.ReverseFind('.');
    if (iPos > 0)
    {
        sFtClsName = sFtClsName.Mid(iPos + 1);
    }
    return sFtClsName;
}

/************************************************************************
��Ҫ���� : �Ե���ͼ����зֲ�
������� :
�� �� ֵ :
//
�޸���־ :
************************************************************************/
bool XDBPreProcessDriver::SplitOneLayer(CMapStringToString* pSplitLayerNames, IFeatureClass* pInFtCls, IWorkspace* pTargetWS)
{

    if (pInFtCls == NULL) return false;


    HRESULT hr;
    POSITION pos = pSplitLayerNames->GetStartPosition();
    CString sLayerName;
    CString sLayerAlias;

    esriGeometryType geoType;
    hr = pInFtCls->get_ShapeType(&geoType);

    CString sBaseFtClsName;
    if (geoType == esriGeometryPolyline)
    {
        sBaseFtClsName = "Line";

    }
    else if (geoType == esriGeometryPoint)
    {
        sBaseFtClsName = "Point";
    }


    while (pos != NULL)
    {
        pSplitLayerNames->GetNextAssoc(pos, sLayerName, sLayerAlias);

        if (sLayerName.IsEmpty()) continue;

        //��Լ�����Ŀͼ�����ͽ��й���

        CString sGeoType = sLayerName.Right(2);

        //��
        if (geoType == esriGeometryPolyline)
        {
            if (sGeoType.CompareNoCase("LN") != 0)
            {
                continue;
            }
        }
        else if (geoType == esriGeometryPoint)
        {
            if (sGeoType.CompareNoCase("PT") != 0)
            {
                continue;
            }
        }


        //����LayerName�ֲ�
        IQueryFilterPtr pSplitFilter(CLSID_QueryFilter);
        CString sWhereClause;
        sWhereClause.Format("GDB_LAYER='%s'", sLayerName);
        hr = pSplitFilter->put_WhereClause(CComBSTR(sWhereClause));

        long numFeats;

        hr = pInFtCls->FeatureCount(pSplitFilter, &numFeats);
        if (numFeats <= 0) continue;

        CHAR strLayer[255] = {0};
        strcpy(strLayer, sLayerName);
        //�ֲ�
        try
        {
            API_ConvertFeatureClass(pInFtCls, pSplitFilter, pTargetWS, "DWG_X", strLayer);
        }
        catch (...)
        {
            CString sLog;
            sLog.Format("SplitOneLayer() �ڽ���%sͼ��� [%s] �ֲ�ʱ��������", sBaseFtClsName, sWhereClause);
            m_pLogRec->WriteLog(sLog);
        }
    }

    return true;
}

/********************************************************************
��Ҫ���� :  ���߹���
������� :
�� �� ֵ :
//
�޸���־ :
*********************************************************************/
IPolygon* XDBPreProcessDriver::CreatePolygon(IPolyline* pPLine, ISpatialReference* pSpRef)
{
    if (pPLine == NULL) return NULL;

    HRESULT hr;

    ITopologicalOperator3Ptr pTopo = pPLine;
    if (pTopo != NULL)
    {
        hr = pTopo->put_IsKnownSimple(VARIANT_FALSE);
        hr = pTopo->Simplify();
    }

    ///////����//////////////
    IPolygonPtr pGeoPolygon;
    ISegmentCollectionPtr pPolygonSegm(CLSID_Polygon);
    ISegmentCollectionPtr pSegColl;
    pSegColl = pPLine;
    if (pSegColl == NULL) return NULL;

    /*/������
    long numSegCol;
    pSegColl->get_SegmentCount(&numSegCol);
    for (int i = 0; i < numSegCol; i++)
    {
        ISegmentPtr pSegment;
        pSegColl->get_Segment(i, &pSegment);
        pPolygonSegm->AddSegment(pSegment);
    }*/

    pPolygonSegm->AddSegmentCollection(pSegColl);
    pGeoPolygon = pPolygonSegm;

    hr = pGeoPolygon->Project(pSpRef);

    ITopologicalOperator3Ptr pTopoPolygon = pGeoPolygon;
    if (pTopoPolygon != NULL)
    {
        hr = pTopoPolygon->put_IsKnownSimple(VARIANT_FALSE);
        hr = pTopoPolygon->Simplify();
    }


    return pGeoPolygon.Detach();

}


/************************************************************************
��Ҫ���� : ���߹���
������� :
�� �� ֵ :
//
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::BuildPolygon(void)
{
    //CWaitCursor w;
    CString sLogInfo;
    sLogInfo.Format("4 ��ʼ����");
    WriteLog(sLogInfo);

    CMapStringToString polygonLayers;

    HRESULT hr;
    ITablePtr pTbLayers;
    hr = m_pSysFtWs->OpenTable(CComBSTR("POLYGON_LAYERS"), &pTbLayers);
    if (pTbLayers == NULL) return;

    long numLayers;
    hr = pTbLayers->RowCount(NULL, &numLayers);
    if (numLayers <= 0) return ;

    long lLayerName;
    long lPolygonName;
    pTbLayers->FindField(CComBSTR("GDB_LAYER"), &lLayerName);
    pTbLayers->FindField(CComBSTR("GDB_POLYGONLAYER"), &lPolygonName);

    if (lLayerName == -1 || lPolygonName == -1)
    {
        WriteLog("POLYGON_LAYERS���ñ��ֶβ���ȷ�����顣");
        return;
    }

    IEsriCursorPtr pRowCursor;
    hr = pTbLayers->Search(NULL, VARIANT_FALSE, &pRowCursor);

    IEsriRowPtr pRowLayerName;
    CComVariant vtVal;
    CString sLayerName;
    CString sPolygonName;

    while (pRowCursor->NextRow(&pRowLayerName) == S_OK)
    {
        pRowLayerName->get_Value(lLayerName, &vtVal);
        sLayerName = vtVal.bstrVal;

        pRowLayerName->get_Value(lPolygonName, &vtVal);
        sPolygonName = vtVal.bstrVal;

        polygonLayers.SetAt(sLayerName, sPolygonName);
    }

    if (polygonLayers.GetCount() <= 0) return;

    CString sTextProgress;
    //pTextProgressCtrl->ShowWindow(SW_SHOW);
    //pTextProgressCtrl->SetPos(0);

    //pTextProgressCtrl->SetWindowText("��ʼ����ǰ��׼������...");
    ////pTextProgressCtrl->StepIt();

    //pTextProgressCtrl->SetRange(0, polygonLayers.GetCount());


    //PrgbarRange(0, polygonLayers.GetCount());
    //PrgbarSetPos(0);
    //PrgbarSetText("��ʼ����ǰ��׼������...");
    //PrgbarStepIt();

    IFeatureWorkspacePtr pFtWS = m_pOutWS;

    POSITION pos = polygonLayers.GetStartPosition();
    while (pos != NULL)
    {
        polygonLayers.GetNextAssoc(pos, sLayerName, sPolygonName);

        //sTextProgress.Format("���ڶ�%sͼ����й���...", sLayerName);
        //PrgbarSetText(sTextProgress);
        //PrgbarStepIt();

        IFeatureClassPtr pLineFtCls;
        IFeatureClassPtr pPolygonFtCls;
        IFeatureDatasetPtr pFtDS;
        IFieldsPtr pSrFields;
        IFieldsPtr pTargetFields;

        hr = pFtWS->OpenFeatureClass(CComBSTR(sLayerName), &pLineFtCls);

        //�򲻿��������һ��ͼ��Ĺ���
        if (pLineFtCls == NULL) continue;

        //�õ�Ŀ���ֶ�
        pTargetFields.CreateInstance(CLSID_Fields);
        IFieldsEditPtr pTargetFieldsEdit = pTargetFields;

        long numFields;
        hr = pLineFtCls->get_Fields(&pSrFields);
        pSrFields->get_FieldCount(&numFields);

        //������Ҫ�ص�ͼ�������ֶ�
        for (int i = 0; i < numFields; i++)
        {
            CComBSTR bsFieldName;
            VARIANT_BOOL vbEditable;
            esriFieldType fieldType;
            IFieldPtr pField;
            pSrFields->get_Field(i, &pField);
            hr = pField->get_Name(&bsFieldName);
            pField->get_Editable(&vbEditable);
            if (vbEditable == VARIANT_TRUE)
            {
                pField->get_Type(&fieldType);
                if (fieldType != esriFieldTypeGeometry)
                {
                    IClonePtr pClone = pField;
                    IClonePtr pClonedField;
                    hr = pClone->Clone(&pClonedField);
                    IFieldPtr pTarField = pClonedField;
                    hr = pTargetFieldsEdit->AddField(pTarField);
                }
            }
        }

        //���OBJECTID�ֶ�
        IFieldEditPtr pTarField(CLSID_Field);
        hr = pTarField->put_Name(CComBSTR("OBJECTID"));
        hr = pTarField->put_AliasName(CComBSTR("Ҫ��ID"));
        hr = pTarField->put_Type(esriFieldTypeOID);
        hr = pTargetFieldsEdit->AddField(pTarField);

        IGeometryDefEditPtr pGeoDef(CLSID_GeometryDef);
        hr = pGeoDef->put_GeometryType(esriGeometryPolygon);

        ISpatialReferencePtr pSpRef = API_GetSpatialReference(pLineFtCls);//GetSysSpatialRef();

        pGeoDef->put_GridCount(1);
        pGeoDef->put_AvgNumPoints(2);
        pGeoDef->put_HasM(VARIANT_FALSE);
        pGeoDef->put_HasZ(VARIANT_FALSE);

        //double dGridSize = 1000;
        //VARIANT_BOOL bhasXY;
        //pSpRef->HasXYPrecision(&bhasXY);
        //if (bhasXY)
        //{
        //    double xmin, ymin, xmax, ymax, dArea;
        //    pSpRef->GetDomain(&xmin, &xmax, &ymin, &ymax);
        //    dArea = (xmax - xmin) * (ymax - ymin);
        //    dGridSize = sqrt(dArea / 100);
        //}
        //if (dGridSize <= 0)
        //    dGridSize = 1000;
        pGeoDef->put_GridSize(0, 120);

        hr = pGeoDef->putref_SpatialReference(pSpRef);

        IFieldEditPtr pShapeField(CLSID_Field);
        hr = pShapeField->put_Name(CComBSTR("SHAPE"));
        hr = pShapeField->put_AliasName(CComBSTR("��Ҫ��"));
        hr = pShapeField->put_Type(esriFieldTypeGeometry);
        hr = pShapeField->putref_GeometryDef(pGeoDef);

        hr = pTargetFieldsEdit->AddField(pShapeField);

        hr = pLineFtCls->get_FeatureDataset(&pFtDS);
        if (pFtDS == NULL) continue;

        //������ͼ��
        hr = pFtDS->CreateFeatureClass(CComBSTR(sPolygonName), pTargetFields, 0, 0, esriFTSimple, CComBSTR("SHAPE"), NULL, &pPolygonFtCls);

        if (pPolygonFtCls == NULL) continue;

        long numFeatures;

        //������ͼ���е�ÿһ��Ҫ��,������Ҫ��, Ȼ����뵽��ͼ����
        hr = pLineFtCls->FeatureCount(NULL, &numFeatures);
        if (numFeatures <= 0) continue;

        PrgbarRange(0, numFeatures);
        PrgbarSetPos(0);
        sTextProgress.Format("���ڶ�%sͼ����й���...", sLayerName);
        PrgbarSetText(sTextProgress);

        IFeaturePtr pLineFeat;
        IFeaturePtr pPolygonFeat;
        IGeometryPtr pFeatGeo;
        IFeatureCursorPtr pFtCur;
        hr = pLineFtCls->Search(NULL, VARIANT_FALSE, &pFtCur);
        while (pFtCur->NextFeature(&pLineFeat) == S_OK)
        {
            PrgbarStepIt();

            //�õ�������Ҫ��
            hr = pLineFeat->get_ShapeCopy(&pFeatGeo);
            if (pFeatGeo == NULL) continue;

            IPolylinePtr pPolyLine(pFeatGeo);
            if (pPolyLine == NULL) continue;

            //������Ƿ�պ�,������պ��򲻹���
            VARIANT_BOOL IsClosed;
            pPolyLine->get_IsClosed(&IsClosed);
            if (IsClosed == VARIANT_FALSE) continue;

            ///////����//////////////
            IPolygonPtr pGeoPolygon = CreatePolygon(pPolyLine, pSpRef);

            /*ISegmentCollectionPtr pPolygonSegm(CLSID_Polygon);
            ISegmentCollectionPtr pSegColl;

            pSegColl = pFeatGeo;

            if (pSegColl == NULL) continue;

            //������
            long numSegCol;
            pSegColl->get_SegmentCount(&numSegCol);
            for (int i = 0; i < numSegCol; i++)
            {
                ISegmentPtr pSegment;
                pSegColl->get_Segment(i, &pSegment);
                pPolygonSegm->AddSegment(pSegment);
            }

            pGeoPolygon = pPolygonSegm;
            //ISpatialReferencePtr pSpRef = GetSysSpatialRef();
            hr = pGeoPolygon->Project(pSpRef);*/

            hr = pPolygonFtCls->CreateFeature(&pPolygonFeat);
            if (pPolygonFeat == NULL)
            {
                WriteLog("������״Ҫ��ʧ��.");
                continue;
            }

            //����������
            CopyFeatureAttr(pLineFeat, pPolygonFeat);

            /* /��������
            ITopologicalOperator3Ptr pPolygonOper = pGeoPolygon;
            hr = pPolygonOper->put_IsKnownSimple(VARIANT_FALSE);
            hr = pPolygonOper->Simplify();*/


            hr = pPolygonFeat->putref_Shape(pGeoPolygon);

            //������Ҫ��
            hr = pPolygonFeat->Store();

        }

        sLogInfo.Format("��ɶ�%sͼ��Ĺ��档", sLayerName);
        WriteLog(sLogInfo);
    }

    PrgbarSetPos(0);
    PrgbarSetText("");

    sLogInfo.Format("4 ��ɹ��档");
    WriteLog(sLogInfo);

}


/************************************************************************
��Ҫ���� :	����Ҫ������
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::CopyFeatureAttr(IFeaturePtr pSourceFeature, IFeaturePtr pTargetFeature)
{
    if (pSourceFeature == NULL || pTargetFeature == NULL) return;

    HRESULT hr;
    IFieldsPtr pTarFields;
    IFieldsPtr pSrcFields;
    IFieldPtr pTarField;
    IEsriRowPtr pRow;
    long lFieldCount;
    VARIANT_BOOL vbEditable;
    esriFieldType fieldType;
    CComVariant vtVal;
    IGeometryPtr pShape;
    CComBSTR bsFieldName;
    long lFieldInd;

    hr = pSourceFeature->get_Fields(&pSrcFields);

    hr = pTargetFeature->get_Fields(&pTarFields);
    pTarFields->get_FieldCount(&lFieldCount);
    for (int iTarFieldInd = 0; iTarFieldInd < lFieldCount; iTarFieldInd++)
    {
        pTarFields->get_Field(iTarFieldInd, &pTarField);
        pTarField->get_Editable(&vbEditable);
        if (vbEditable == VARIANT_TRUE)
        {
            pTarField->get_Type(&fieldType);
            if (fieldType != esriFieldTypeOID && fieldType != esriFieldTypeGeometry)
            {
                hr = pTarField->get_Name(&bsFieldName);
                hr = pSrcFields->FindField(bsFieldName, &lFieldInd);
                if (lFieldInd != -1)
                {
                    pSourceFeature->get_Value(lFieldInd, &vtVal);
                    pTargetFeature->put_Value(iTarFieldInd, vtVal);
                }
            }
        }
    }

}

/************************************************************************
��Ҫ���� : ����ע�����͵�Ҫ����
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
HRESULT XDBPreProcessDriver::CreateAnnoFtCls(IFeatureDataset* pTarFDS, BSTR bsAnnoName, IFeatureClass** ppAnnoFtCls)
{
    HRESULT hr;
    IWorkspacePtr pWS;
    hr = pTarFDS->get_Workspace(&pWS);

    IFeatureWorkspaceAnnoPtr PFWSAnno = pWS;

    IGraphicsLayerScalePtr pGLS(CLSID_GraphicsLayerScale);
    pGLS->put_Units(esriMeters);
    pGLS->put_ReferenceScale(1);

    //' set up symbol collection
    ISymbolCollectionPtr pSymbolColl(CLSID_SymbolCollection);

    ITextSymbolPtr myTxtSym(CLSID_TextSymbol);
    //Set the font for myTxtSym
    IFontDispPtr myFont(CLSID_StdFont);
    IFontPtr pFt = myFont;
    pFt->put_Name(CComBSTR("Courier New"));
    CY cy;
    cy.Hi = 0;
    cy.Lo = 9;
    pFt->put_Size(cy);
    myTxtSym->put_Font(myFont);

    // Set the Color for myTxtSym to be Dark Red
    IRgbColorPtr myColor(CLSID_RgbColor);
    myColor->put_Red(150);
    myColor->put_Green(0);
    myColor->put_Blue (0);
    myTxtSym->put_Color(myColor);

    // Set other properties for myTxtSym
    myTxtSym->put_Angle(0);
    myTxtSym->put_RightToLeft(VARIANT_FALSE);
    myTxtSym->put_VerticalAlignment(esriTVABaseline);
    myTxtSym->put_HorizontalAlignment(esriTHAFull);
    myTxtSym->put_Size(200);
    //myTxtSym->put_Case(esriTCNormal);

    ISymbolPtr pSymbol = myTxtSym;
    pSymbolColl->putref_Symbol(0, pSymbol);

    //set up the annotation labeling properties including the expression
    IAnnotateLayerPropertiesPtr pAnnoProps(CLSID_LabelEngineLayerProperties);
    pAnnoProps->put_FeatureLinked(VARIANT_TRUE);
    pAnnoProps->put_AddUnplacedToGraphicsContainer(VARIANT_FALSE);
    pAnnoProps->put_CreateUnplacedElements(VARIANT_TRUE);
    pAnnoProps->put_DisplayAnnotation(VARIANT_TRUE);
    pAnnoProps->put_UseOutput(VARIANT_TRUE);

    ILabelEngineLayerPropertiesPtr pLELayerProps = pAnnoProps;
    IAnnotationExpressionEnginePtr aAnnoVBScriptEngine(CLSID_AnnotationVBScriptEngine);
    pLELayerProps->putref_ExpressionParser(aAnnoVBScriptEngine);
    pLELayerProps->put_Expression(CComBSTR("[DESCRIPTION]"));
    pLELayerProps->put_IsExpressionSimple(VARIANT_TRUE);
    pLELayerProps->put_Offset(0);
    pLELayerProps->put_SymbolID(0);
    pLELayerProps->putref_Symbol(myTxtSym);

    IAnnotateLayerTransformationPropertiesPtr pATP = pAnnoProps;
    double dRefScale;
    pGLS->get_ReferenceScale(&dRefScale);
    pATP->put_ReferenceScale(dRefScale);
    pATP->put_Units(esriMeters);
    pATP->put_ScaleRatio(1);

    IAnnotateLayerPropertiesCollectionPtr pAnnoPropsColl(CLSID_AnnotateLayerPropertiesCollection);
    pAnnoPropsColl->Add(pAnnoProps);

    //' use the AnnotationFeatureClassDescription co - class to get the list of required fields and the default name of the shape field
    IObjectClassDescriptionPtr pOCDesc(CLSID_AnnotationFeatureClassDescription);
    IFeatureClassDescriptionPtr pFDesc = pOCDesc;

    IFieldsPtr pReqFields;
    IUIDPtr pInstCLSID;
    IUIDPtr pExtCLSID;
    CComBSTR bsShapeFieldName;

    pOCDesc->get_RequiredFields(&pReqFields);
    pOCDesc->get_InstanceCLSID(&pInstCLSID);
    pOCDesc->get_ClassExtensionCLSID(&pExtCLSID);
    pFDesc->get_ShapeFieldName(&bsShapeFieldName);

    IFieldsEditPtr ipFieldsEdit = pReqFields;

    //����CAD�ļ���ע��ͼ���ֶ�
    IFieldEditPtr ipFieldEdit;
    IFieldPtr ipField;

    // ���� Entity ����¼esriʵ������
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR("Entity"));
    ipFieldEdit->put_AliasName(CComBSTR("Entity"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldsEdit->AddField(ipField);


    // ���� Handle ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Handle"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Handle"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� BaseName ����¼DWGʵ�������DWG�ļ���
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"BaseName"));
    ipFieldEdit->put_AliasName(CComBSTR(L"BaseName"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(250);
    ipFieldsEdit->AddField(ipField);

    // ���� Layer ����¼DWGʵ�����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Layer"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Layer"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(250);
    ipFieldsEdit->AddField(ipField);

    // ���� Color ����¼DWGʵ�������ɫ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Color"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Color"));
    ipFieldEdit->put_Type(esriFieldTypeInteger);
    ipFieldsEdit->AddField(ipField);

    // ���� Thickness ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Thickness"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Thickness"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� Elevation ����¼DWGʵ��߳�ֵ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Elevation"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Elevation"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);

    /* / ���� Text ����¼Text
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Text"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Text"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);*/

    // ���� Height ����¼�߶�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Height"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Height"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);


    // ���� FontID ����¼FontID
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"FontId"));
    ipFieldEdit->put_AliasName(CComBSTR(L"FontId"));
    ipFieldEdit->put_Type(esriFieldTypeSmallInteger);
    ipFieldsEdit->AddField(ipField);


    //////////�������ձ��ֶ�/////////////////////////////////////
    // ���� GDB_LAYER�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    CComBSTR bsStr;
    bsStr = "GDB_LAYER";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    // ���� YSDM�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "YSDM";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    // ���� YSMC�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "YSMC";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    //' create the new class
    hr = PFWSAnno->CreateAnnotationClass(bsAnnoName, pReqFields, pInstCLSID, pExtCLSID, bsShapeFieldName, CComBSTR(""), pTarFDS, 0, pAnnoPropsColl, pGLS, pSymbolColl, VARIANT_TRUE, ppAnnoFtCls);

    return hr;
}


/************************************************************************
��Ҫ���� : ����ע�����͵�Ҫ����
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
HRESULT XDBPreProcessDriver::CreateAnnoFtCls(IWorkspace* pWS, BSTR bsAnnoName, IFeatureClass** ppAnnoFtCls)
{
    HRESULT hr;
    //IWorkspacePtr pWS = pFtWS;
    //hr = pTarFDS->get_Workspace(&pWS);

    IFeatureWorkspaceAnnoPtr PFWSAnno = pWS;

    IGraphicsLayerScalePtr pGLS(CLSID_GraphicsLayerScale);
    pGLS->put_Units(esriMeters);
    pGLS->put_ReferenceScale(1);

    //' set up symbol collection
    ISymbolCollectionPtr pSymbolColl(CLSID_SymbolCollection);

    ITextSymbolPtr myTxtSym(CLSID_TextSymbol);
    //Set the font for myTxtSym
    IFontDispPtr myFont(CLSID_StdFont);
    IFontPtr pFt = myFont;
    pFt->put_Name(CComBSTR("Courier New"));
    CY cy;
    cy.Hi = 0;
    cy.Lo = 9;
    pFt->put_Size(cy);
    myTxtSym->put_Font(myFont);

    // Set the Color for myTxtSym to be Dark Red
    IRgbColorPtr myColor(CLSID_RgbColor);
    myColor->put_Red(150);
    myColor->put_Green(0);
    myColor->put_Blue (0);
    myTxtSym->put_Color(myColor);

    // Set other properties for myTxtSym
    myTxtSym->put_Angle(0);
    myTxtSym->put_RightToLeft(VARIANT_FALSE);
    myTxtSym->put_VerticalAlignment(esriTVABaseline);
    myTxtSym->put_HorizontalAlignment(esriTHAFull);
    myTxtSym->put_Size(200);
    //myTxtSym->put_Case(esriTCNormal);

    ISymbolPtr pSymbol = myTxtSym;
    pSymbolColl->putref_Symbol(0, pSymbol);

    //set up the annotation labeling properties including the expression
    IAnnotateLayerPropertiesPtr pAnnoProps(CLSID_LabelEngineLayerProperties);
    pAnnoProps->put_FeatureLinked(VARIANT_TRUE);
    pAnnoProps->put_AddUnplacedToGraphicsContainer(VARIANT_FALSE);
    pAnnoProps->put_CreateUnplacedElements(VARIANT_TRUE);
    pAnnoProps->put_DisplayAnnotation(VARIANT_TRUE);
    pAnnoProps->put_UseOutput(VARIANT_TRUE);

    ILabelEngineLayerPropertiesPtr pLELayerProps = pAnnoProps;
    IAnnotationExpressionEnginePtr aAnnoVBScriptEngine(CLSID_AnnotationVBScriptEngine);
    pLELayerProps->putref_ExpressionParser(aAnnoVBScriptEngine);
    pLELayerProps->put_Expression(CComBSTR("[DESCRIPTION]"));
    pLELayerProps->put_IsExpressionSimple(VARIANT_TRUE);
    pLELayerProps->put_Offset(0);
    pLELayerProps->put_SymbolID(0);
    pLELayerProps->putref_Symbol(myTxtSym);

    IAnnotateLayerTransformationPropertiesPtr pATP = pAnnoProps;
    double dRefScale;
    pGLS->get_ReferenceScale(&dRefScale);
    pATP->put_ReferenceScale(dRefScale);
    pATP->put_Units(esriMeters);
    pATP->put_ScaleRatio(1);

    IAnnotateLayerPropertiesCollectionPtr pAnnoPropsColl(CLSID_AnnotateLayerPropertiesCollection);
    pAnnoPropsColl->Add(pAnnoProps);

    //' use the AnnotationFeatureClassDescription co - class to get the list of required fields and the default name of the shape field
    IObjectClassDescriptionPtr pOCDesc(CLSID_AnnotationFeatureClassDescription);
    IFeatureClassDescriptionPtr pFDesc = pOCDesc;

    IFieldsPtr pReqFields;
    IUIDPtr pInstCLSID;
    IUIDPtr pExtCLSID;
    CComBSTR bsShapeFieldName;

    pOCDesc->get_RequiredFields(&pReqFields);
    pOCDesc->get_InstanceCLSID(&pInstCLSID);
    pOCDesc->get_ClassExtensionCLSID(&pExtCLSID);
    pFDesc->get_ShapeFieldName(&bsShapeFieldName);

    IFieldsEditPtr ipFieldsEdit = pReqFields;

    //����CAD�ļ���ע��ͼ���ֶ�
    IFieldEditPtr ipFieldEdit;
    IFieldPtr ipField;

    // ���� Entity ����¼esriʵ������
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR("Entity"));
    ipFieldEdit->put_AliasName(CComBSTR("Entity"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldsEdit->AddField(ipField);


    // ���� Handle ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Handle"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Handle"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� BaseName ����¼DWGʵ�������DWG�ļ���
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"BaseName"));
    ipFieldEdit->put_AliasName(CComBSTR(L"BaseName"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(250);
    ipFieldsEdit->AddField(ipField);

    // ���� Layer ����¼DWGʵ�����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Layer"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Layer"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(250);
    ipFieldsEdit->AddField(ipField);

    // ���� Color ����¼DWGʵ�������ɫ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Color"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Color"));
    ipFieldEdit->put_Type(esriFieldTypeInteger);
    ipFieldsEdit->AddField(ipField);

    // ���� Thickness ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Thickness"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Thickness"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� Elevation ����¼DWGʵ��߳�ֵ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Elevation"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Elevation"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);

    /* / ���� Text ����¼Text
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Text"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Text"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);*/

    // ���� Height ����¼�߶�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Height"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Height"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);


    // ���� FontID ����¼FontID
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"FontId"));
    ipFieldEdit->put_AliasName(CComBSTR(L"FontId"));
    ipFieldEdit->put_Type(esriFieldTypeSmallInteger);
    ipFieldsEdit->AddField(ipField);


    //////////�������ձ��ֶ�/////////////////////////////////////
    // ���� GDB_LAYER�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    CComBSTR bsStr;
    bsStr = "GDB_LAYER";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    // ���� YSDM�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "YSDM";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    // ���� YSMC�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "YSMC";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(200);
    ipFieldsEdit->AddField(ipField);

    //' create the new class
    hr = PFWSAnno->CreateAnnotationClass(bsAnnoName, pReqFields, pInstCLSID, pExtCLSID, bsShapeFieldName, CComBSTR(""), NULL, 0, pAnnoPropsColl, pGLS, pSymbolColl, VARIANT_TRUE, ppAnnoFtCls);

    return hr;
}

/************************************************************************
��Ҫ���� : ����ע��ͼ��
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
HRESULT XDBPreProcessDriver::CreateAnnoFtCls(IFeatureDatasetPtr pTarFDS, BSTR bsAnnoName, IFieldsPtr pFields, IFeatureClass** ppAnnoFtCls)
{
    HRESULT hr;
    IWorkspacePtr pWS;
    hr = pTarFDS->get_Workspace(&pWS);

    IFeatureWorkspaceAnnoPtr PFWSAnno = pWS;

    IGraphicsLayerScalePtr pGLS(CLSID_GraphicsLayerScale);
    pGLS->put_Units(esriMeters);
    pGLS->put_ReferenceScale(1);

    //' set up symbol collection
    ISymbolCollectionPtr pSymbolColl(CLSID_SymbolCollection);

    ITextSymbolPtr myTxtSym(CLSID_TextSymbol);
    //Set the font for myTxtSym
    IFontDispPtr myFont(CLSID_StdFont);
    IFontPtr pFt = myFont;
    pFt->put_Name(CComBSTR("Courier New"));
    CY cy;
    cy.Hi = 0;
    cy.Lo = 9;
    pFt->put_Size(cy);
    myTxtSym->put_Font(myFont);

    // Set the Color for myTxtSym to be Dark Red
    IRgbColorPtr myColor(CLSID_RgbColor);
    myColor->put_Red(150);
    myColor->put_Green(0);
    myColor->put_Blue (0);
    myTxtSym->put_Color(myColor);

    // Set other properties for myTxtSym
    myTxtSym->put_Angle(0);
    myTxtSym->put_RightToLeft(VARIANT_FALSE);
    myTxtSym->put_VerticalAlignment(esriTVABaseline);
    myTxtSym->put_HorizontalAlignment(esriTHAFull);
    myTxtSym->put_Size(200);
    //myTxtSym->put_Case(esriTCNormal);

    ISymbolPtr pSymbol = myTxtSym;
    pSymbolColl->putref_Symbol(0, pSymbol);

    //set up the annotation labeling properties including the expression
    IAnnotateLayerPropertiesPtr pAnnoProps(CLSID_LabelEngineLayerProperties);
    pAnnoProps->put_FeatureLinked(VARIANT_TRUE);
    pAnnoProps->put_AddUnplacedToGraphicsContainer(VARIANT_FALSE);
    pAnnoProps->put_CreateUnplacedElements(VARIANT_TRUE);
    pAnnoProps->put_DisplayAnnotation(VARIANT_TRUE);
    pAnnoProps->put_UseOutput(VARIANT_TRUE);

    ILabelEngineLayerPropertiesPtr pLELayerProps = pAnnoProps;
    IAnnotationExpressionEnginePtr aAnnoVBScriptEngine(CLSID_AnnotationVBScriptEngine);
    pLELayerProps->putref_ExpressionParser(aAnnoVBScriptEngine);
    pLELayerProps->put_Expression(CComBSTR("[DESCRIPTION]"));
    pLELayerProps->put_IsExpressionSimple(VARIANT_TRUE);
    pLELayerProps->put_Offset(0);
    pLELayerProps->put_SymbolID(0);
    pLELayerProps->putref_Symbol(myTxtSym);

    IAnnotateLayerTransformationPropertiesPtr pATP = pAnnoProps;
    double dRefScale;
    pGLS->get_ReferenceScale(&dRefScale);
    pATP->put_ReferenceScale(dRefScale);
    pATP->put_Units(esriMeters);
    pATP->put_ScaleRatio(1);

    IAnnotateLayerPropertiesCollectionPtr pAnnoPropsColl(CLSID_AnnotateLayerPropertiesCollection);
    pAnnoPropsColl->Add(pAnnoProps);

    //' use the AnnotationFeatureClassDescription co - class to get the list of required fields and the default name of the shape field
    IObjectClassDescriptionPtr pOCDesc(CLSID_AnnotationFeatureClassDescription);
    IFeatureClassDescriptionPtr pFDesc = pOCDesc;

    IFieldsPtr pReqFields;
    IUIDPtr pInstCLSID;
    IUIDPtr pExtCLSID;
    CComBSTR bsShapeFieldName;

    pOCDesc->get_RequiredFields(&pReqFields);
    pOCDesc->get_InstanceCLSID(&pInstCLSID);
    pOCDesc->get_ClassExtensionCLSID(&pExtCLSID);
    pFDesc->get_ShapeFieldName(&bsShapeFieldName);

    /*IFieldsEditPtr ipFieldsEdit = pReqFields;

    //����CAD�ļ���ע��ͼ���ֶ�
    IFieldEditPtr ipFieldEdit;
    IFieldPtr ipField;

    // ���� Entity ����¼esriʵ������
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR("Entity"));
    ipFieldEdit->put_AliasName(CComBSTR("Entity"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldsEdit->AddField(ipField);


    // ���� Handle ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Handle"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Handle"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� BaseName ����¼DWGʵ�������DWG�ļ���
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"BaseName"));
    ipFieldEdit->put_AliasName(CComBSTR(L"BaseName"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� Layer ����¼DWGʵ�����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Layer"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Layer"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� Color ����¼DWGʵ�������ɫ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Color"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Color"));
    ipFieldEdit->put_Type(esriFieldTypeInteger);
    ipFieldsEdit->AddField(ipField);

    // ���� Thickness ����¼DWGʵ����
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Thickness"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Thickness"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� Elevation ����¼DWGʵ��߳�ֵ
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Elevation"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Elevation"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);

    / * / ���� Text ����¼Text
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Text"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Text"));
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);* /

    // ���� Height ����¼�߶�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"Height"));
    ipFieldEdit->put_AliasName(CComBSTR(L"Height"));
    ipFieldEdit->put_Type(esriFieldTypeDouble);
    ipFieldsEdit->AddField(ipField);


    // ���� FontID ����¼FontID
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    ipFieldEdit->put_Name(CComBSTR(L"FontId"));
    ipFieldEdit->put_AliasName(CComBSTR(L"FontId"));
    ipFieldEdit->put_Type(esriFieldTypeSmallInteger);
    ipFieldsEdit->AddField(ipField);


    //////////�������ձ��ֶ�/////////////////////////////////////
    // ���� FeatureCode�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    CComBSTR bsStr;
    bsStr = "FeatureCode";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� FeatureName�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "FeatureName";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);

    // ���� LayerName�ֶ�
    ipField.CreateInstance(CLSID_Field);
    ipFieldEdit = ipField;
    bsStr = "LayerName";
    ipFieldEdit->put_Name(bsStr);
    ipFieldEdit->put_AliasName(bsStr);
    ipFieldEdit->put_Type(esriFieldTypeString);
    ipFieldEdit->put_Length(150);
    ipFieldsEdit->AddField(ipField);*/

    //' create the new class
    hr = PFWSAnno->CreateAnnotationClass(bsAnnoName, pFields, pInstCLSID, pExtCLSID, bsShapeFieldName, CComBSTR(""), pTarFDS, 0, pAnnoPropsColl, pGLS, pSymbolColl, VARIANT_TRUE, ppAnnoFtCls);

    return hr;
}


/************************************************************************
��Ҫ���� : CAD�ļ�ע��ͼ������ת��
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
BOOL XDBPreProcessDriver::CAD_AnnotationConvert(IWorkspace* pTargetWS, IDataset* pTargetDataset, CString sDwgFilePath, CString sShowedFilePath)
{
    HRESULT hr;

    IFeatureWorkspacePtr pFWS = pTargetWS;

    PrgbarSetText("����׼����ȡע������...");
    PrgbarSetPos(0);


    if (sShowedFilePath.IsEmpty())
    {
        sShowedFilePath = sDwgFilePath;
    }

    //IFeatureDatasetPtr pFDataset;
    //hr = pFWS->OpenFeatureDataset(CComBSTR("DWG_X"), &pFDataset);

    //����Ŀ��ע��ͼ��
    IFeatureClassPtr pTarAnnoFtCls;

    hr = pFWS->OpenFeatureClass(CComBSTR("Annotation"), &pTarAnnoFtCls);
    if (pTarAnnoFtCls == NULL)
    {
        if (pTargetDataset != NULL)
        {
            IFeatureDatasetPtr pFDataset = pTargetDataset;
            CreateAnnoFtCls(pFDataset, CComBSTR("Annotation"), &pTarAnnoFtCls);
        }
        else
        {
            CreateAnnoFtCls(pTargetWS, CComBSTR("Annotation"), &pTarAnnoFtCls);
        }

        if (pTarAnnoFtCls == NULL)
        {
            return FALSE;
        }
    }

    //��CADע��ͼ������
    IWorkspaceFactory2Ptr pCadWSFact(CLSID_CadWorkspaceFactory);
    IWorkspacePtr pCadWS;
    hr = pCadWSFact->OpenFromFile(CComBSTR(GetFileDirectory(sDwgFilePath)), 0, &pCadWS);

    IFeatureWorkspacePtr pCadFtWS = pCadWS;

    int iPos = -1;
    iPos = sDwgFilePath.ReverseFind('\\');
    if ( iPos == -1 )
    {
        iPos = sDwgFilePath.ReverseFind('/');
    }


    CString  sFileName = sDwgFilePath.Mid(iPos + 1);
    CString sBaseName;
    sBaseName = sFileName.Mid(0, sFileName.Find('.'));
    IFeatureDatasetPtr pCadFtDS;
    pCadFtWS->OpenFeatureDataset(CComBSTR(sFileName), &pCadFtDS);

    IFeatureClassContainerPtr pFeatureClassContainer = pCadFtDS;

    long numFtCls;
    pFeatureClassContainer->get_ClassCount(&numFtCls);

    IFeatureClassPtr pCadFtCls;
    for (int i = 0; i < numFtCls; i++)
    {
        hr = pFeatureClassContainer->get_Class(i, &pCadFtCls);

        BSTR bsFtName;
        pCadFtCls->get_AliasName(&bsFtName);
        CString sCadAnnoLayerName = bsFtName ;
        esriFeatureType ftType;
        hr = pCadFtCls->get_FeatureType(&ftType);
        if (ftType == esriFTCoverageAnnotation)
        {
            break;
        }

    }

    long numCadFeats;
    pCadFtCls->FeatureCount(NULL, &numCadFeats);

    if (numCadFeats <= 0)
    {
        return TRUE;
    }

    //pTextProgressCtrl->SetPos(0);
    //pTextProgressCtrl->SetRange(0, numCadFeats);
    //pTextProgressCtrl->SetWindowText("���ڶ�ȡ" + sDwgFilePath + "�е�ע������...");

    PrgbarRange(0, numCadFeats);
    PrgbarSetText("���ڶ�ȡ" + sShowedFilePath + "�е�ע������...");
    PrgbarSetPos(0);


    //��ʼ��������ձ�
    //if (m_dwgReader.m_aryCodes.GetCount() <= 0)
    //{
    //    m_dwgReader.InitCompareCodes();
    //}

    IFeatureCursorPtr pCadCur;
    hr = pCadFtCls->Search(NULL, VARIANT_FALSE, &pCadCur);

    long lStyleFldInd;
    long lTextFldInd;
    long lHeightFldInd;
    long lAngleFldInd;
    long lLayerIdx;

    //Ŀ��Ҫ�����е�BaseName����ֵ
    long lBaseNameFldInd;

    CString sStyle = "";
    CString sText = "";
    double dHeight = 0;
    double dAngle = 0;
    CString sLayer = "";

    CComVariant vtVal;

    pCadFtCls->FindField(CComBSTR("Style"), &lStyleFldInd);
    pCadFtCls->FindField(CComBSTR("Text"), &lTextFldInd);
    pCadFtCls->FindField(CComBSTR("Height"), &lHeightFldInd);
    //pCadFtCls->FindField(CComBSTR("TxtAngle"), &lAngleFldInd);
    pCadFtCls->FindField(CComBSTR("TxtAngle"), &lAngleFldInd);
    pCadFtCls->FindField(CComBSTR("Layer"), &lLayerIdx);

    pTarAnnoFtCls->FindField(CComBSTR("BaseName"), &lBaseNameFldInd);

    IFeaturePtr pCreatedFeat;
    IAnnotationFeaturePtr pTarAnnoFeat;
    IFeaturePtr pCadFeature;
    IGeometryPtr pCadGeo;
    while (pCadCur->NextFeature(&pCadFeature) == S_OK)
    {
        //pTextProgressCtrl->StepIt();
        PrgbarStepIt();

        //����ע��ͼ��Ҫ��
        hr = pTarAnnoFtCls->CreateFeature(&pCreatedFeat);
        pTarAnnoFeat = pCreatedFeat;

        //Style
        pCadFeature->get_Value(lStyleFldInd, &vtVal);
        if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
        {
            sStyle = vtVal.bstrVal;
        }

        //Text
        pCadFeature->get_Value(lTextFldInd, &vtVal);
        if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
        {
            sText = vtVal.bstrVal;
        }

        //Height
        pCadFeature->get_Value(lHeightFldInd, &vtVal);
        if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
        {
            dHeight = vtVal.dblVal;
        }

        //Angle
        pCadFeature->get_Value(lAngleFldInd, &vtVal);
        if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
        {
            dAngle = vtVal.dblVal;
        }

        //Layer
        pCadFeature->get_Value(lLayerIdx, &vtVal);
        if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
        {
            sLayer = vtVal.bstrVal;
        }


        //���� Element
        hr = pCadFeature->get_ShapeCopy(&pCadGeo);

        IEnvelopePtr pCadEnvelope;
        hr = pCadGeo->get_Envelope(&pCadEnvelope);

        double dMinX, dMaxX, dMinY, dMaxY;
        pCadEnvelope->get_XMin(&dMinX);
        pCadEnvelope->get_XMax(&dMaxX);
        pCadEnvelope->get_YMin(&dMinY);
        pCadEnvelope->get_YMax(&dMaxY);

        double pos_x, pos_y;

        pos_x = (dMinX + dMaxX) / 2;
        pos_y = (dMinY + dMaxY) / 2;

        //����40
        //if (sLayer.Find("�߱�ע")>=0)
        {
            //pos_y = pos_y - 40;
            sText.Replace('\\', 0x0a);
        }



        //ITextElementPtr pTextElement = MakeTextElementByStyle(sStyle, sText, dAngle, dHeight, (dMinX + dMaxX) / 2, (dMinY + dMaxY) / 2, 1);
        ITextElementPtr pTextElement = MakeTextElementByStyle(sStyle, sText, dAngle, dHeight, pos_x, pos_y, 1);

        IElementPtr pElement = pTextElement;
        hr = pTarAnnoFeat->put_Annotation(pElement);

        //����Ҫ������
        CopyFeatureAttr(pCadFeature, pCreatedFeat);

        //���BASENAMEֵ
        pCreatedFeat->put_Value(lBaseNameFldInd, CComVariant(sBaseName));


        //�������
        IFeatureBufferPtr pFeatBuf = pCreatedFeat;
        m_pDwgReader->CompareCodes(pFeatBuf.GetInterfacePtr());

        hr = pCreatedFeat->Store();

    }

    //������ձ�
    //    m_dwgReader.CleanCompareCodes();

    return TRUE;

}


//�õ��ļ����ڵ�Ŀ¼
CString XDBPreProcessDriver::GetFileDirectory(const CString& sFullPath)
{
    int iPos = -1;
    iPos = sFullPath.ReverseFind('\\');
    if (iPos == -1)
    {
        iPos = sFullPath.ReverseFind('/');
    }

    if (iPos >= 0)
    {
        return sFullPath.Left(iPos);
    }

    return "";
}

/************************************************************************
��Ҫ���� : ����ע��Element
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
ITextElement* XDBPreProcessDriver::MakeTextElementByStyle(CString strStyle, CString strText, double dblAngle,
        double dblHeight, double dblX,
        double dblY, double ReferenceScale)
{

    HRESULT hr;

    ITextElementPtr pTextElement;

    ISimpleTextSymbolPtr pTextSymbol;

    CString strHeight;

    pTextSymbol.CreateInstance(CLSID_TextSymbol);

    IFontDispPtr fntDisp(CLSID_StdFont);
    IFontPtr fnt = fntDisp;
    fnt->put_Name(CComBSTR("����"));
    CY cy;
    cy.int64 = 9;
    fnt->put_Size(cy);

    //'Set the text symbol font by getting the IFontDisp interface
    pTextSymbol->put_Font(fntDisp);

    double mapUnitsInches;

    IUnitConverterPtr pUnitConverter(CLSID_UnitConverter);

    pUnitConverter->ConvertUnits(dblHeight, esriMeters, esriInches, &mapUnitsInches);

    strHeight.Format("%f", (mapUnitsInches * 72) / ReferenceScale);

    double dSize = atof(strHeight);
    pTextSymbol->put_Size(dSize);

    pTextSymbol->put_HorizontalAlignment(esriTHALeft);
    pTextSymbol->put_VerticalAlignment(esriTVABaseline);
    //pTextSymbol->put_XOffset(strText.GetLength() / 4*dSize);

    pTextElement.CreateInstance(CLSID_TextElement);
    hr = pTextElement->put_ScaleText(VARIANT_FALSE);// VARIANT_TRUE);

    hr = pTextElement->put_Text(CComBSTR(strText));
    hr = pTextElement->put_Symbol(pTextSymbol);

    IElementPtr pElement = pTextElement;
    IPointPtr pPoint(CLSID_Point);
    hr = pPoint->PutCoords(dblX, dblY);
    hr = pElement->put_Geometry(pPoint);

    if (fabs(dblAngle) > 0)
    {
        ITransform2DPtr pTransform2D = pTextElement;
        double rotationAngle = dblAngle / 180 * 3.1415926;
        pTransform2D->Rotate(pPoint, rotationAngle);
    }

    return pTextElement.Detach();

}

/************************************************************************
��Ҫ���� : ��ע��ͼ����зֲ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
bool XDBPreProcessDriver::SplitAnnotationLayer(CString sBaseLayerName, CMapStringToString* pSplitLayerNames, IFeatureClass* pInFtCls, IWorkspace* pTargetWS)
{

    if (pInFtCls == NULL) return false;

    HRESULT hr;
    POSITION pos = pSplitLayerNames->GetStartPosition();
    CString sLayerName;
    CString sLayerAlias;

    while (pos != NULL)
    {
        pSplitLayerNames->GetNextAssoc(pos, sLayerName, sLayerAlias);

        if (sLayerName.IsEmpty()) continue;

        //��Լ�����Ŀ,�ж�ͼ�����ĺ���λ�ǲ���AN
        CString sGeoType = sLayerName.Right(2);
        if (sGeoType.CompareNoCase("AN") != 0 )
        {
            continue;
        }

        //����LayerName�ֲ�
        IQueryFilterPtr pSplitFilter(CLSID_QueryFilter);
        CString sWhereClause;
        sWhereClause.Format("GDB_LAYER='%s'", sLayerName);
        hr = pSplitFilter->put_WhereClause(CComBSTR(sWhereClause));

        long numFeats;

        hr = pInFtCls->FeatureCount(pSplitFilter, &numFeats);
        if (numFeats <= 0) continue;

        IFeatureWorkspacePtr pTarFtWS = pTargetWS;

        ISpatialReferencePtr pSpRef = API_GetSpatialReference(pInFtCls);

        IFeatureDatasetPtr pTarAnnoDS;
        hr = pTarFtWS->OpenFeatureDataset(CComBSTR("DWG_X"), &pTarAnnoDS);
        if (pTarAnnoDS == NULL)
        {
            hr = pTarFtWS->CreateFeatureDataset(CComBSTR("DWG_X"), pSpRef, &pTarAnnoDS);
        }

        try
        {
            //����Ŀ��ע��ͼ��
            IFeatureClassPtr pTarAnnoFtCls;
            hr = pTarFtWS->OpenFeatureClass(CComBSTR(sLayerName), &pTarAnnoFtCls);
            if (pTarAnnoFtCls == NULL)
            {
                CreateAnnoFtCls(pTarAnnoDS, CComBSTR(sLayerName), &pTarAnnoFtCls);
            }

            IFeatureCursorPtr pTarAnnoCur;
            pTarAnnoFtCls->Insert(VARIANT_TRUE, &pTarAnnoCur);

            //����Ҫ�ص�Ŀ��ͼ��
            IFeaturePtr pSrcFeat;
            CComVariant OID;
            IFeatureCursorPtr pFtCur;
            pInFtCls->Search(pSplitFilter, VARIANT_FALSE, &pFtCur);
            while (pFtCur->NextFeature(&pSrcFeat) == S_OK)
            {
                IFeatureBufferPtr pTarAnnoFtBuf = pSrcFeat;
                hr = pTarAnnoCur->InsertFeature(pTarAnnoFtBuf, &OID);

                hr = pTarAnnoCur->Flush();

                pSrcFeat.Release();
            }
        }
        catch (...)
        {
            CString sLog;
            sLog.Format("SplitAnnotationLayer() �ڽ���%sͼ��� [%s] �ֲ�ʱ��������", sBaseLayerName, sWhereClause);
            m_pLogRec->WriteLog(sLog);
        }

    }

    return true;
}

/********************************************************************
��Ҫ���� : �ֽ��ַ���
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
void XDBPreProcessDriver::ParseStr(CString sSrcStr, char chrSeparator, CStringList& lstItems)
{
    int iPos = -1;
    while ((iPos = sSrcStr.Find(chrSeparator)) != -1)
    {
        CString stmp = sSrcStr.Mid(0, iPos);
        sSrcStr = sSrcStr.Mid(iPos + 1);

        stmp.Trim();
        if (!stmp.IsEmpty())
        {
            lstItems.AddTail(stmp);
        }

    }
    sSrcStr.Trim();
    if (!sSrcStr.IsEmpty())
    {
        lstItems.AddTail(sSrcStr);
    }
}


/********************************************************************
��Ҫ���� : �߹�����ڴ���(��Լ�����Ŀ��û������ֵ����Ҫ������������)
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
HRESULT XDBPreProcessDriver::PostBuildPolygon2(CString sPolygonLayerName, IFeatureClassPtr pInFtCls, CString sAreaField)
{
    //try
    //{
    HRESULT hr;

    ISpatialReferencePtr pSpRef = API_GetSpatialReference(pInFtCls);

    //����ȫ��������Ҫ�ص���ֻ���ɻ�״��
    IQueryFilterPtr pBigRegionFilter(CLSID_QueryFilter);
    CString sWhereClause;
    sWhereClause.Format("%s like '*-*'", sAreaField);
    hr = pBigRegionFilter->put_WhereClause(CComBSTR(sWhereClause));

    long numFeats;
    hr = pInFtCls->FeatureCount(pBigRegionFilter , &numFeats);

    if (numFeats <= 0)
    {
        return S_OK;
    }

    long lAreaFld;
    pInFtCls->FindField(CComBSTR(sAreaField), &lAreaFld);

    PrgbarRange(0, numFeats);
    PrgbarSetText("���ڽ���" + sPolygonLayerName + "ͼ��Ĺ������...");
    PrgbarSetPos(0);

    CList<long, long> bigPolygonOIDsList;
    CList<long, long> delOIDsList;

    IFeaturePtr pFeature;

    IFeatureCursorPtr pFtCur;
    hr = pInFtCls->Update(pBigRegionFilter, VARIANT_FALSE, &pFtCur);
    while (pFtCur->NextFeature(&pFeature) == S_OK)
    {
        PrgbarStepIt();

        IGeometryPtr pBigGeoShape;
        hr = pFeature->get_ShapeCopy(&pBigGeoShape);
        hr = pBigGeoShape->Project(pSpRef);

        ISpatialFilterPtr pSpFilter(CLSID_SpatialFilter);
        pSpFilter->putref_Geometry(pBigGeoShape);
        pSpFilter->put_SpatialRel(esriSpatialRelIntersects);//esriSpatialRelContains);

        long numInsidePolygon;
        hr = pInFtCls->FeatureCount(pSpFilter, &numInsidePolygon);
        if (numInsidePolygon <= 0) continue;

        //�ⲿ�������Ҫ��ID
        long lBigPolygonOID;
        hr = pFeature->get_OID(&lBigPolygonOID);

        bigPolygonOIDsList.AddTail(lBigPolygonOID);

        IClonePtr pSClone = pBigGeoShape;
        IClonePtr pTClone;
        hr = pSClone->Clone(&pTClone);
        IGeometryPtr pBigGeoCopy = pTClone;
        hr = pBigGeoCopy->Project(pSpRef);

        //���¹���
        ISegmentCollectionPtr pPolygonSegm(pBigGeoCopy);

        IGeometryPtr pInsidePolygonShape;
        IFeaturePtr pInsidePolygonFeat;
        IFeatureCursorPtr pInsidePolygonCur;
        hr = pInFtCls->Search(pSpFilter, VARIANT_FALSE, &pInsidePolygonCur);
        while (pInsidePolygonCur->NextFeature(&pInsidePolygonFeat) == S_OK)
        {
            //�ⲿ�������Ҫ��ID
            //ȥ�������汾��
            long lInsideOID;
            hr = pInsidePolygonFeat->get_OID(&lInsideOID);
            if (lBigPolygonOID == lInsideOID)
            {
                continue;
            }

            hr = pInsidePolygonFeat->get_ShapeCopy(&pInsidePolygonShape);
            hr = pInsidePolygonShape->Project(pSpRef);

            //���˵�������������
            IRelationalOperatorPtr pRelaOper = pInsidePolygonShape ;
            VARIANT_BOOL vbWithin;
            hr = pRelaOper->Within(pBigGeoShape, &vbWithin);
            if (vbWithin == VARIANT_FALSE) continue;

            ISegmentCollectionPtr pSegColl;
            pSegColl = pInsidePolygonShape;
            if (pSegColl == NULL) continue;

            hr = pPolygonSegm->AddSegmentCollection(pSegColl);

            //������Ҫɾ���ڲ���С���OID
            if (delOIDsList.Find(lInsideOID) == NULL && bigPolygonOIDsList.Find(lInsideOID) == NULL)
            {
                delOIDsList.AddTail(lInsideOID);
            }

            //hr = pInsidePolygonFeat->Delete();

        }

        ITopologicalOperator3Ptr pBigPolygonOper = pPolygonSegm;//pBigGeoShape;
        hr = pBigPolygonOper->put_IsKnownSimple(VARIANT_FALSE);
        hr = pBigPolygonOper->Simplify();

        //�޸Ĵ��������
        hr = pFeature->putref_Shape(pBigGeoCopy);


        try
        {
            //�޸�����ֶ�����ֵ
            CComVariant vtVal;
            pFeature->get_Value(lAreaFld, &vtVal);
            CString sArea = vtVal.bstrVal;
            CStringList lstArea;

            ParseStr(sArea, '-', lstArea);

            double dArea = atof(lstArea.GetAt(lstArea.FindIndex(0)));
            for (int i = 1;i < lstArea.GetCount();i++)
            {
                double dtmp = atof(lstArea.GetAt(lstArea.FindIndex(i)));
                dArea -= dtmp;
            }

            sArea.Format("%0.2f", dArea);
            pFeature->put_Value(lAreaFld, CComVariant(sArea));
        }
        catch (...)
        {
            int iErr = ::GetLastError();
        }

        //����
        int ihr = pFeature->Store();

        pFeature.Release();

    }

    /*/ɾ���ڲ�С��
    if (delOIDsList.GetCount() > 0)
    {
    	POSITION pos = delOIDsList.GetHeadPosition();
    	while (pos != NULL)
    	{
    		IFeaturePtr pDelFeat;
    		long lOID = delOIDsList.GetNext(pos);

    		hr = pInFtCls->GetFeature(lOID, &pDelFeat);
    		if (pDelFeat != NULL)
    		{
    			hr = pDelFeat->Delete();
    		}
    		
    	}
    }*/

    return S_OK;
    /*}
    catch (...)
    {
    	return S_FALSE;
    }*/

}

/************************************************************************
��Ҫ���� : �߹�����ڴ���
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
HRESULT XDBPreProcessDriver::PostBuildPolygon(IFeatureClassPtr pInFtCls)
{
    HRESULT hr;

    IQueryFilterPtr pFilter(CLSID_QueryFilter);

    CString sWhereClause;
    long numFeats;

    ISpatialReferencePtr pSpRef = API_GetSpatialReference(pInFtCls);//GetSysSpatialRef();

    CComVariant vtVal;
    long lFldThickness;
    CString sThickness;
    pInFtCls->FindField(CComBSTR("Thickness"), &lFldThickness);

    //����ȫ��������Ҫ�ص���ֻ���ɻ�״��
    sWhereClause.Format("Thickness like '%s'", "*5");
    hr = pFilter->put_WhereClause(CComBSTR(sWhereClause));
    hr = pInFtCls->FeatureCount(pFilter, &numFeats);
    if (numFeats > 0)
    {
        IFeaturePtr pFeature;

        IFeatureCursorPtr pFtCur;
        hr = pInFtCls->Search(pFilter, VARIANT_FALSE, &pFtCur);
        while (pFtCur->NextFeature(&pFeature) == S_OK)
        {
            IGeometryPtr pGeoShape;
            hr = pFeature->get_ShapeCopy(&pGeoShape);
            hr = pGeoShape->Project(pSpRef);

            ISpatialFilterPtr pSpFilter(CLSID_SpatialFilter);
            pSpFilter->putref_Geometry(pGeoShape);
            pSpFilter->put_SpatialRel(esriSpatialRelWithin);

            long numBigPolygon;
            hr = pInFtCls->FeatureCount(pSpFilter, &numBigPolygon);
            if (numBigPolygon <= 0) continue;

            //�ڲ�С���Ҫ��ID
            long lInsideOID;
            hr = pFeature->get_OID(&lInsideOID);

            //�ڲ�С��Thickness
            pFeature->get_Value(lFldThickness, &vtVal);
            sThickness = vtVal.bstrVal;

            IGeometryPtr pBigPolygonShape;
            IFeaturePtr pBigPolygonFeat;
            IFeatureCursorPtr pBigPolygonCur;
            hr = pInFtCls->Update(pSpFilter, VARIANT_FALSE, &pBigPolygonCur);
            while (pBigPolygonCur->NextFeature(&pBigPolygonFeat) == S_OK)
            {
                //�ⲿ�������Ҫ��ID
                //ȥ�������汾��
                long lOutsideOID;
                hr = pBigPolygonFeat->get_OID(&lOutsideOID);
                if (lInsideOID == lOutsideOID)
                {
                    continue;
                }

                //���˵���С����벻һ�µ��������
                CString sBigPolygonThickness;
                pBigPolygonFeat->get_Value(lFldThickness, &vtVal);
                sBigPolygonThickness = vtVal.bstrVal;
                if (sThickness.CompareNoCase(sBigPolygonThickness) != 0)
                {
                    continue;
                }

                hr = pBigPolygonFeat->get_ShapeCopy(&pBigPolygonShape);
                hr = pBigPolygonShape->Project(pSpRef);

                ISegmentCollectionPtr pPolygonSegm(pBigPolygonShape);
                ISegmentCollectionPtr pSegColl;

                pSegColl = pGeoShape;

                if (pSegColl == NULL) continue;

                //������
                long numSegCol;
                pSegColl->get_SegmentCount(&numSegCol);
                for (int i = 0; i < numSegCol; i++)
                {
                    ISegmentPtr pSegment;
                    pSegColl->get_Segment(i, &pSegment);
                    pPolygonSegm->AddSegment(pSegment);
                }

                ITopologicalOperator3Ptr pBigPolygonOper = pBigPolygonShape;
                hr = pBigPolygonOper->put_IsKnownSimple(VARIANT_FALSE);
                hr = pBigPolygonOper->Simplify();

                hr = pBigPolygonFeat->putref_Shape(pBigPolygonShape);

                //�޸Ĵ��������
                hr = pBigPolygonFeat->Store();

                //ɾ���ڲ���С��
                hr = pFeature->Delete();

            }
        }
    }


    //����ȫ��������Ҫ�ص������ɻ��ͱ������ڲ�����
    sWhereClause.Format("Thickness like '%s'", "*6");
    hr = pFilter->put_WhereClause(CComBSTR(sWhereClause));
    hr = pInFtCls->FeatureCount(pFilter, &numFeats);
    if (numFeats > 0)
    {
        IFeaturePtr pFeature;

        IFeatureCursorPtr pFtCur;
        hr = pInFtCls->Search(pFilter, VARIANT_FALSE, &pFtCur);
        while (pFtCur->NextFeature(&pFeature) == S_OK)
        {
            IGeometryPtr pGeoShape;
            hr = pFeature->get_ShapeCopy(&pGeoShape);
            hr = pGeoShape->Project(pSpRef);

            ISpatialFilterPtr pSpFilter(CLSID_SpatialFilter);
            pSpFilter->putref_Geometry(pGeoShape);
            pSpFilter->put_SpatialRel(esriSpatialRelWithin);

            long numBigPolygon;
            hr = pInFtCls->FeatureCount(pSpFilter, &numBigPolygon);
            if (numBigPolygon <= 0) continue;

            //�ڲ�С���Ҫ��ID
            long lInsideOID;
            hr = pFeature->get_OID(&lInsideOID);

            IGeometryPtr pBigPolygonShape;
            IFeaturePtr pBigPolygonFeat;
            IFeatureCursorPtr pBigPolygonCur;
            hr = pInFtCls->Update(pSpFilter, VARIANT_FALSE, &pBigPolygonCur);
            while (pBigPolygonCur->NextFeature(&pBigPolygonFeat) == S_OK)
            {
                //�ⲿ�������Ҫ��ID
                //ȥ�������汾��
                long lOutsideOID;
                hr = pBigPolygonFeat->get_OID(&lOutsideOID);
                if (lInsideOID == lOutsideOID)
                {
                    continue;
                }

                hr = pBigPolygonFeat->get_ShapeCopy(&pBigPolygonShape);
                hr = pBigPolygonShape->Project(pSpRef);

                ISegmentCollectionPtr pPolygonSegm(pBigPolygonShape);
                ISegmentCollectionPtr pSegColl;

                pSegColl = pGeoShape;

                if (pSegColl == NULL) continue;

                //������
                long numSegCol;
                pSegColl->get_SegmentCount(&numSegCol);
                for (int i = 0; i < numSegCol; i++)
                {
                    ISegmentPtr pSegment;
                    pSegColl->get_Segment(i, &pSegment);
                    pPolygonSegm->AddSegment(pSegment);
                }

                ITopologicalOperator3Ptr pBigPolygonOper = pBigPolygonShape;
                hr = pBigPolygonOper->put_IsKnownSimple(VARIANT_FALSE);
                hr = pBigPolygonOper->Simplify();

                hr = pBigPolygonFeat->putref_Shape(pBigPolygonShape);

                //�޸Ĵ��������
                hr = pBigPolygonFeat->Store();

                //��ɾ���ڲ���С��

            }

        }

    }

    return S_OK;
}

/************************************************************************
��Ҫ���� : �õ����е�ע��Ӧ����
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::GetRegAppNames(CStringList& lstAppNames)
{
    lstAppNames.RemoveAll();
    if (m_pSysFtWs != NULL)
    {
        ITablePtr pTable;
        m_pSysFtWs->OpenTable(CComBSTR("EXTRA_ATTRIB_CONFIG"), &pTable);

        if (pTable != NULL)
        {
            HRESULT hr;
            IEsriCursorPtr pCursor;
            hr = pTable->Search(NULL, VARIANT_FALSE, &pCursor);
            if (pCursor != NULL)
            {
                long lRegAppName;
                //pTable->FindField(CComBSTR("FeatureClassName"), &lFeatureClassNameFieldIndex);
                pTable->FindField(CComBSTR("RegAppName"), &lRegAppName);

                IEsriRowPtr pRow;
                CComVariant vtVal;
                pCursor->NextRow(&pRow);
                while (pRow != NULL)
                {
                    CString sRegAppName;
                    //RegAppName
                    hr = pRow->get_Value(lRegAppName, &vtVal);
                    if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
                    {
                        sRegAppName = vtVal.bstrVal;

                        lstAppNames.AddTail(sRegAppName);
                    }

                    hr = pCursor->NextRow(&pRow);
                }
            }
        }
    }

}

/************************************************************************
��Ҫ���� : �ҽ���ӱ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::JoinAddinTable(ITablePtr pExtraTable)
{

    HRESULT hr;

    WriteLog("��ʼ������ұ�����Թҽ�...");
    //////////�õ������ļ�����////////////////////////////////////////////////
    TCHAR lpFile[MAX_PATH];
    ::GetModuleFileName(NULL, lpFile, MAX_PATH);
    *_tcsrchr(lpFile, '\\') = 0;

    CString sModuleDir = lpFile;

    // ���������ļ�
    CString sIniPath;
    sIniPath.Format("%s\\sys_config.ini", lpFile);

    CHAR inBuf[MAX_PATH];

    CString sJoinedLayerName;
    //CString sExtraMdbPath;
    CString sExtraTableName;

    ::GetPrivateProfileString("APP", "JoinedLayerName", "", inBuf, 255, sIniPath);
    sJoinedLayerName = inBuf;
    //////////////////////////////////////////////////////////////////////////
    //�õ����ݼ�����
    CComBSTR bsDsName;
    IEnumDatasetNamePtr pDsNames;
    hr = m_pOutWS->get_DatasetNames(esriDTFeatureDataset, &pDsNames);
    IDatasetNamePtr pDsName;
    hr = pDsNames->Next(&pDsName);
    if (pDsName != NULL)
    {
        hr = pDsName->get_Name(&bsDsName);
    }

    sJoinedLayerName = CString(bsDsName) + "_" + sJoinedLayerName;

    //��Ҫ�ҽ���ӱ����ݵ�Ҫ����
    IFeatureWorkspacePtr pFtWS = m_pOutWS;
    IFeatureClassPtr pTargetFtCls;
    hr = pFtWS->OpenFeatureClass(CComBSTR(sJoinedLayerName), &pTargetFtCls);
    if (pTargetFtCls == NULL)
    {
        WriteLog("��ʱ���в�����" + sJoinedLayerName + "ͼ�㣬�޷�������ұ�Ĺҽӡ�");
        return;
    }


    //::GetPrivateProfileString("APP", "ExtraMdb", "", inBuf, 255, sIniPath);
    //sExtraMdbPath = inBuf;
    //sExtraMdbPath = sModuleDir + "\\" + sExtraMdbPath;

    /*::GetPrivateProfileString("APP", "ExtraTableName", "", inBuf, 255, sIniPath);
    sExtraTableName = inBuf;



    //////////////////////////////////////////////////////////////////////////
    // ����ұ�
    ITablePtr pExtraTable;
    IWorkspacePtr pExtraWS;
    IWorkspaceFactoryPtr pWSFact(CLSID_AccessWorkspaceFactory);
    hr = pWSFact->OpenFromFile(CComBSTR(sExtraFilePath), 0, &pExtraWS);
    IFeatureWorkspacePtr pExtraFtWS = pExtraWS;
    if (pExtraFtWS == NULL)
    {
    WriteLog("ָ�������MDB�ļ����������ļ����޷�����������ԵĹҽӡ�");
    return;
    }
    hr = pExtraFtWS->OpenTable(CComBSTR(sExtraTableName), &pExtraTable);
    if (pExtraTable == NULL)
    {
    WriteLog("�����ļ���ָ������ұ����ڣ��޷�����������ԵĹҽӡ�");
    return;
    }*/


    //����������Ҫ��ӵ��ֶ�����
    CStringList lstAddFieldNames;

    //////////////////////////////////////////////////////////////////////////
    //��չ�ֶε���Ӵ���
    IFieldsPtr pExtraFields;
    IFieldPtr pExtFld;
    CComBSTR bsFldName;
    hr = pExtraTable->get_Fields(&pExtraFields);
    long numExtraField;
    hr = pExtraFields->get_FieldCount(&numExtraField);
    for (int i = 0; i < numExtraField; i++)
    {
        hr = pExtraFields->get_Field(i, &pExtFld);
        hr = pExtFld->get_Name(&bsFldName);

        long lFldInd;
        hr = pTargetFtCls->FindField(bsFldName, &lFldInd);
        //Ŀ��Ҫ�����в����ڸ��ֶ������
        if (lFldInd == -1)
        {
            hr = pTargetFtCls->AddField(pExtFld);
        }

        CString sAddFieldName = bsFldName;
        lstAddFieldNames.AddTail(sAddFieldName);

    }


    //////////////////////////////////////////////////////////////////////////
    //����DKBH���ؿ��ţ��ҽ�����ֵ
    IQueryFilterPtr pFilter(CLSID_QueryFilter);
    CString sWhereClause;
    sWhereClause = "DKBH<>''";
    pFilter->put_WhereClause(CComBSTR(sWhereClause));
    long numFeats;
    hr = pTargetFtCls->FeatureCount(pFilter, &numFeats);
    if (numFeats <= 0) return;

    //pTextProgressCtrl->SetRange(0, numFeats);
    //pTextProgressCtrl->SetPos(0);
    //pTextProgressCtrl->SetWindowText("���ڽ�����ұ����ݵĹҽӴ���...");

    int iSucceedJoined = 0;
    CString sLog;

    IFeaturePtr pTarFeat;
    IFeatureCursorPtr pTarFtCur;
    long lTarFldInd;
    CComVariant vtVal;

    hr = pTargetFtCls->FindField(CComBSTR("DKBH"), &lTarFldInd);
    hr = pTargetFtCls->Update(pFilter, VARIANT_FALSE, &pTarFtCur);
    while (pTarFtCur->NextFeature(&pTarFeat) == S_OK)
    {
        IQueryFilterPtr pExtTabFilter(CLSID_QueryFilter);
        CString sExtTabWhereClause;
        CString sDKBH;

        hr = pTarFeat->get_Value(lTarFldInd, &vtVal);
        if (vtVal.vt == VT_NULL || vtVal.vt == VT_EMPTY) continue;
        sDKBH = vtVal.bstrVal;

        sExtTabWhereClause.Format("DKBH='%s'", sDKBH);
        pExtTabFilter->put_WhereClause(CComBSTR(sExtTabWhereClause));

        long numRows;
        hr = pExtraTable->RowCount(pExtTabFilter, &numRows);
        if (numRows <= 0)
        {
            sLog.Format("�ؿ���Ϊ%s�ļ�¼����ұ��в����ڣ����顣", sDKBH);
            WriteLog(sLog);
            continue;
        }

        IEsriCursorPtr pTabCur;
        pExtraTable->Search(pExtTabFilter, VARIANT_FALSE, &pTabCur);

        IEsriRowPtr pTabRow;
        hr = pTabCur->NextRow(&pTabRow);

        CComVariant vtExtVal;
        CString sAddFieldName;
        POSITION pos = lstAddFieldNames.GetHeadPosition();
        while (pos != NULL)
        {
            sAddFieldName = lstAddFieldNames.GetNext(pos);

            long lSrcFldInd, lTarFldInd;

            hr = pExtraTable->FindField(CComBSTR(sAddFieldName), &lSrcFldInd);
            hr = pTargetFtCls->FindField(CComBSTR(sAddFieldName), &lTarFldInd);

            hr = pTabRow->get_Value(lSrcFldInd, &vtExtVal);

            if (vtExtVal.vt == VT_NULL || vtExtVal.vt == VT_EMPTY) continue;

            ////ȥ�ַ��ո�
            //if(vtExtVal.vt == VT_BSTR)
            //{
            //	CString sTmp = vtExtVal.bstrVal;
            //	sTmp.Trim();
            //	vtExtVal = sTmp;
            //}

            //����չ����ֵ
            hr = pTarFeat->put_Value(lTarFldInd, vtExtVal);


        }

        hr = pTarFeat->Store();

        if (SUCCEEDED(hr))
        {
            iSucceedJoined++;
        }

        //        pTextProgressCtrl->StepIt();
    }

    sLog.Format("�����ұ�����Թҽӡ��ɹ��ҽ�%d����¼��ʧ��%d����", iSucceedJoined, numFeats - iSucceedJoined);
    WriteLog(sLog);

}

/********************************************************************
��Ҫ���� : ���������С���ͼ��
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
HRESULT XDBPreProcessDriver::DoPolygonPostProcess()
{
    HRESULT hr;
    IWorkspaceNamePtr pWSName;

    IFeatureWorkspace* pSysFtWS = m_pSysFtWs;
    ITablePtr pTbLayers;
    hr = pSysFtWS->OpenTable(CComBSTR("POLYGON_LAYERS"), &pTbLayers);
    if (pTbLayers == NULL) return S_FALSE;

    long numLayers;
    hr = pTbLayers->RowCount(NULL, &numLayers);

    if (numLayers <= 0) return S_FALSE;

    long lPolygonName;
    pTbLayers->FindField(CComBSTR("GDB_POLYGONLAYER"), &lPolygonName);
    if (lPolygonName == -1)
    {
        return S_FALSE;
    }

    //����ж��ֶ�
    long lAreaField;
    pTbLayers->FindField(CComBSTR("GDB_AREAFIELD"), &lAreaField);

    IEsriCursorPtr pRowCursor;
    hr = pTbLayers->Search(NULL, VARIANT_FALSE, &pRowCursor);

    CMapStringToString mapPolygonLayer;

    IEsriRowPtr pRowLayerName;
    CComVariant vtVal;
    CString sPolygonName;
    CString sAreaField;

    while (pRowCursor->NextRow(&pRowLayerName) == S_OK)
    {
        pRowLayerName->get_Value(lPolygonName, &vtVal);
        sPolygonName = vtVal.bstrVal;

        if (lAreaField != -1)
        {
            pRowLayerName->get_Value(lAreaField, &vtVal);
            sAreaField  = vtVal.bstrVal;
        }
        else
        {
            sAreaField  = "";
        }

        mapPolygonLayer.SetAt(sPolygonName, sAreaField);

    }

    if (mapPolygonLayer.GetCount() <= 0)
    {
        WriteLog("�����ں��ڴ����ͼ�㡣");
        return S_FALSE;
    }

    POSITION pos;
    pos = mapPolygonLayer.GetStartPosition();
    while (pos != NULL)
    {
        mapPolygonLayer.GetNextAssoc(pos, sPolygonName, sAreaField);

        IFeatureClassPtr pFtCls;
        hr = ((IFeatureWorkspacePtr)m_pOutWS)->OpenFeatureClass(CComBSTR(sPolygonName), &pFtCls);

        if (pFtCls == NULL)
        {
            continue;
        }

        if (!sAreaField.IsEmpty())
        {
            PostBuildPolygon2(sPolygonName, pFtCls, sAreaField);
        }

    }

    return S_OK;

}

/************************************************************************
��Ҫ���� :�õ�ͼ������
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::GetFtClsNames(IWorkspace* pWorkspace, CDsNameList& lstNames)
{
    IEnumDatasetNamePtr pEnumDatasetName;
    pWorkspace->get_DatasetNames(esriDTFeatureDataset, &pEnumDatasetName);
    if (pEnumDatasetName != NULL)
    {
        IDatasetName* pDatasetName;
        pEnumDatasetName->Next(&pDatasetName);
        while (pDatasetName != NULL)
        {
            IEnumDatasetName* pEnumFeatureName;
            pDatasetName->get_SubsetNames(&pEnumFeatureName);
            if (pEnumFeatureName == NULL)
            {
                continue;
            }
            IDatasetName* pFtName;
            pEnumFeatureName->Next(&pFtName);
            while (pFtName != NULL)
            {
                lstNames.AddTail(pFtName);
                pEnumFeatureName->Next(&pFtName);
            }
            pEnumDatasetName->Next(&pDatasetName);
        }
    }
    else
    {
        pWorkspace->get_DatasetNames(esriDTFeatureClass, &pEnumDatasetName);
        if (pEnumDatasetName == NULL)
        {
            //AfxMessageBox("�����ļ�����ͼ������.", MB_ICONINFORMATION);
            return ;
        }
        IDatasetName* pFtName;
        pEnumDatasetName->Next(&pFtName);
        while (pFtName != NULL)
        {
            lstNames.AddTail(pFtName);
            pEnumDatasetName->Next(&pFtName);
        }
    }
}

/************************************************************************
��Ҫ���� : ɾ�������ֶ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
HRESULT DeleteUnusedField(IFeatureClassPtr pFtCls, CString sUnusedField)
{
    if (pFtCls == NULL)  return S_FALSE;

    HRESULT hr;
    long lFlgIdx;
    IFieldPtr pDelField;
    IFieldsPtr pFields;
    hr = pFtCls->get_Fields(&pFields);
    hr = pFields->FindField(CComBSTR(sUnusedField), &lFlgIdx);
    if (lFlgIdx != -1 )
    {
        hr = pFields->get_Field(lFlgIdx, &pDelField);
        hr = pFtCls->DeleteField(pDelField);
    }

    return hr;
}

//�޸�Ҫ�ؼ���Ҫ�������� Creator: zl
HRESULT XDBPreProcessDriver::ChangeDatasetAndFtClsName(CString sPrefix, CString sPrjName)
{

    CString sDatasetName = sPrefix;
    CString sFtClsNamePrefix;
    if (sPrjName.IsEmpty())
    {
        sFtClsNamePrefix = sPrefix + "_";
    }
    else
    {
        sFtClsNamePrefix = sPrefix + "_" + sPrjName + "_";
    }

    HRESULT hr;

    IFeatureDatasetPtr pFtDS;

    ((IFeatureWorkspacePtr)m_pOutWS)->OpenFeatureDataset(CComBSTR("DWG_X"), &pFtDS);
    if (pFtDS != NULL)
    {
        hr = pFtDS->Rename(CComBSTR(sDatasetName));
    }


    //�õ�ͼ�����
    CMapStringToString mapLayers;
    ITablePtr pFtClsLayers;
    /*   if (m_pSysFtWs == NULL)
    {
    IUnknown* pUnk = 0;
    pUnk = API_GetSysWorkspace();

    m_pSysFtWs = pUnk;
    if (m_pSysFtWs == NULL)
    {
    AfxMessageBox("�޷����ӵ�ϵͳ���ݿ⣬���顣");
    return S_FALSE;
    }
    }*/


    //m_pSysFtWs = g_pSysFtWS;

    hr = m_pSysFtWs->OpenTable(CComBSTR("ALL_LAYERS"), &pFtClsLayers);
    long numLayers;
    hr = pFtClsLayers->RowCount(NULL, &numLayers);

    if (numLayers > 0)
    {
        long lLayerName;
        long lLayerAlias;
        pFtClsLayers->FindField(CComBSTR("LAYER_NAME"), &lLayerName);
        pFtClsLayers->FindField(CComBSTR("LAYER_ALIAS"), &lLayerAlias);

        IEsriCursorPtr pRowCursor;
        hr = pFtClsLayers->Search(NULL, VARIANT_FALSE, &pRowCursor);

        IEsriRowPtr pRowLayerName;
        CComVariant vtVal;
        CString sLayerName;
        CString sLayerAlias;

        while (pRowCursor->NextRow(&pRowLayerName) == S_OK)
        {
            pRowLayerName->get_Value(lLayerName, &vtVal);
            sLayerName = vtVal.bstrVal;

            pRowLayerName->get_Value(lLayerAlias, &vtVal);
            sLayerAlias = vtVal.bstrVal;

            mapLayers.SetAt(sLayerName, sLayerAlias);
        }
    }


    CComBSTR bsFtCls;
    CString sFtCls;

    CDsNameList lstFtClsName;
    GetFtClsNames(m_pOutWS, lstFtClsName);

    long numFtCls = lstFtClsName.GetCount();
    //pTextProgressCtrl->SetRange(0, numFtCls);
    //pTextProgressCtrl->SetPos(0);
    //pTextProgressCtrl->SetWindowText("���ڽ���ͼ�����ƺ��ֶδ���...");

    if ( numFtCls > 0)
    {
        POSITION pos = lstFtClsName.GetHeadPosition();
        while (pos != NULL)
        {
            IDatasetName* pFtClsName = lstFtClsName.GetNext(pos);

            hr = pFtClsName->get_Name(&bsFtCls);

            CString sLayerAlias;
            mapLayers.Lookup(CString(bsFtCls), sLayerAlias);


            sFtCls = sFtClsNamePrefix + CString(bsFtCls);

            IUnknown* pUnk;
            ((INamePtr)pFtClsName)->Open(&pUnk);
            IDatasetPtr pFtClsDs(pUnk);
            if (pFtClsDs != NULL)
            {
                hr = pFtClsDs->Rename(CComBSTR(sFtCls));
            }

            IFeatureClassPtr pFtCls(pUnk);

            //�޸�ͼ�����
            IClassSchemaEditPtr pFtClsSchemaEdit = pFtCls;
            if (pFtClsSchemaEdit != NULL)
            {
                hr = pFtClsSchemaEdit->AlterAliasName(CComBSTR(sLayerAlias));
            }

            //ɾ�������ֶ�
            hr = DeleteUnusedField(pFtCls, "ENTITY");
            hr = DeleteUnusedField(pFtCls, "ENTITY_TYPE");
            hr = DeleteUnusedField(pFtCls, "DwgGeometry");
            hr = DeleteUnusedField(pFtCls, "Handle");
            hr = DeleteUnusedField(pFtCls, "BaseName");
            hr = DeleteUnusedField(pFtCls, "Layer");
            hr = DeleteUnusedField(pFtCls, "Color");
            hr = DeleteUnusedField(pFtCls, "Linetype");
            hr = DeleteUnusedField(pFtCls, "Thickness");
            hr = DeleteUnusedField(pFtCls, "Scale");
            hr = DeleteUnusedField(pFtCls, "Elevation");
            hr = DeleteUnusedField(pFtCls, "Blockname");
            hr = DeleteUnusedField(pFtCls, "Blocknumber");
            hr = DeleteUnusedField(pFtCls, "Visible");
            hr = DeleteUnusedField(pFtCls, "Angle");
            hr = DeleteUnusedField(pFtCls, "Width");
            hr = DeleteUnusedField(pFtCls, "FeatureCode");
            hr = DeleteUnusedField(pFtCls, "FeatureName");
            hr = DeleteUnusedField(pFtCls, "LayerName");

            //            pTextProgressCtrl->StepIt();
        }
    }

    return S_OK;
}

/********************************************************************
��Ҫ���� : У׼����
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
HRESULT XDBPreProcessDriver::AdjustSdeLayerAlias()
{
    HRESULT hr;
    //CWaitCursor wait;

    //pTextProgressCtrl->SetRange(0, 4);
    //pTextProgressCtrl->SetWindowText("��������SDE���ݷ�����...");
    //pTextProgressCtrl->SetPos(0);

    //sde���ݿ�
    IWorkspacePtr pSdeWS = NULL;//API_GetSdeWorkspace();
    if (pSdeWS == NULL)
    {
        MessageBox(NULL, "�޷����ӵ�SDE�����������顣", "����", MB_ICONERROR);
        return S_FALSE;
    }

    //pTextProgressCtrl->SetWindowText("���ڶ�ȡͼ�����...");
    //pTextProgressCtrl->SetPos(2);
    //�õ�ͼ�����
    CMapStringToString mapLayers;
    ITablePtr pFtClsLayers;
    //if (m_pSysFtWs == NULL)
    //{
    //    IUnknown* pUnk = 0;
    //    pUnk = API_GetSysWorkspace();

    //    m_pSysFtWs = pUnk;
    //    if (m_pSysFtWs == NULL)
    //    {
    //        AfxMessageBox("�޷����ӵ�ϵͳ���ݿ⣬���顣");
    //        return S_FALSE;
    //    }
    //}
    //m_pSysFtWs = g_pSysFtWS;
    hr = m_pSysFtWs->OpenTable(CComBSTR("ALL_LAYERS"), &pFtClsLayers);
    long numLayers;
    hr = pFtClsLayers->RowCount(NULL, &numLayers);

    if (numLayers > 0)
    {
        long lLayerName;
        long lLayerAlias;
        pFtClsLayers->FindField(CComBSTR("LAYER_NAME"), &lLayerName);
        pFtClsLayers->FindField(CComBSTR("LAYER_ALIAS"), &lLayerAlias);

        IEsriCursorPtr pRowCursor;
        hr = pFtClsLayers->Search(NULL, VARIANT_FALSE, &pRowCursor);

        IEsriRowPtr pRowLayerName;
        CComVariant vtVal;
        CString sLayerName;
        CString sLayerAlias;

        while (pRowCursor->NextRow(&pRowLayerName) == S_OK)
        {
            pRowLayerName->get_Value(lLayerName, &vtVal);
            sLayerName = vtVal.bstrVal;

            pRowLayerName->get_Value(lLayerAlias, &vtVal);
            sLayerAlias = vtVal.bstrVal;

            mapLayers.SetAt(sLayerName, sLayerAlias);
        }
    }


    CComBSTR bsFtCls;
    CString sFtCls;

    //pTextProgressCtrl->SetWindowText("����׼����Ҫ�淶����ͼ��...");
    //pTextProgressCtrl->SetPos(3);

    CDsNameList lstFtClsName;
    GetFtClsNames(pSdeWS, lstFtClsName);

    //	pTextProgressCtrl->SetPos(4);

    long numFtCls = lstFtClsName.GetCount();
    //pTextProgressCtrl->SetRange(0, numFtCls);
    //pTextProgressCtrl->SetPos(0);
    //pTextProgressCtrl->SetWindowText("���ڽ���ͼ������Ĺ淶��...");

    if ( numFtCls > 0)
    {
        POSITION pos = lstFtClsName.GetHeadPosition();
        while (pos != NULL)
        {
            IDatasetName* pFtClsName = lstFtClsName.GetNext(pos);
            hr = pFtClsName->get_Name(&bsFtCls);
            sFtCls = GetSdeFtClsName(bsFtCls);

            int iPos = sFtCls.Find('_');
            //if (iPos == -1) continue;

            CString sHead = sFtCls.Mid(0, iPos + 1);

            sFtCls = sFtCls.Mid(iPos + 1);//����ǰ׺

            CString sLayerAlias;
            mapLayers.Lookup(sFtCls, sLayerAlias);
            if (!sLayerAlias.IsEmpty())
            {
                IUnknown* pUnk;
                ((INamePtr)pFtClsName)->Open(&pUnk);
                IFeatureClassPtr pFtCls(pUnk);
                //�޸�ͼ�����
                IClassSchemaEditPtr pFtClsSchemaEdit = pFtCls;
                if (pFtClsSchemaEdit != NULL)
                {
                    hr = pFtClsSchemaEdit->AlterAliasName(CComBSTR(sHead + sLayerAlias));
                }
            }

            //pTextProgressCtrl->StepIt();
        }
    }

    return S_OK;
}


/************************************************************************
��Ҫ���� :	����Ҫ��
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDBPreProcessDriver::CopyFeature(IFeaturePtr pSrcFeat, IFeaturePtr& pDestFeat)
{
    if (pSrcFeat == NULL || pDestFeat == NULL) return;

    IFieldPtr pField;
    IFieldsPtr pFields;
    long lFieldCount;
    VARIANT_BOOL vbEditable;
    esriFieldType fieldType;
    CComVariant vtVal;
    CComBSTR bsFldName;
    HRESULT hr;

    IFieldsPtr pDestFields;
    hr = pDestFeat->get_Fields(&pDestFields);

    hr = pSrcFeat->get_Fields(&pFields);
    pFields->get_FieldCount(&lFieldCount);
    for (int i = 0; i < lFieldCount; i++)
    {
        pFields->get_Field(i, &pField);
        pField->get_Editable(&vbEditable);
        if (vbEditable == VARIANT_TRUE)
        {
            pField->get_Type(&fieldType);
            if (fieldType != esriFieldTypeOID && fieldType != esriFieldTypeGeometry)
            {

                hr = pField->get_Name(&bsFldName);

                long lDestFldInd = -1;
                hr = pDestFields->FindField(bsFldName, &lDestFldInd);
                if (lDestFldInd != -1)
                {
                    CString sValue = "";
                    hr = pSrcFeat->get_Value(i, &vtVal);
                    if (vtVal.vt == VT_BSTR)
                    {
                        sValue = vtVal.bstrVal;
                        if (sValue.CompareNoCase("-") == 0 || sValue.CompareNoCase("��") == 0)
                        {
                            sValue = "-1";
                        }
                        hr = pDestFeat->put_Value(lDestFldInd, CComVariant(sValue));
                    }
                    else
                    {
                        hr = pDestFeat->put_Value(lDestFldInd, vtVal);
                    }

                }
            }
        }
    }

    IGeometry* pShape;
    hr = pSrcFeat->get_ShapeCopy(&pShape);

    ITopologicalOperator2Ptr pTopo = pShape;
    if (pTopo != NULL)
    {
        pTopo->put_IsKnownSimple(VARIANT_FALSE);
        pTopo->Simplify();
    }

    hr = pDestFeat->putref_Shape(pShape);

}

/********************************************************************
��Ҫ���� : �������ݵ�Ŀ���
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
bool XDBPreProcessDriver::CopyToTargetDB(IFeatureWorkspacePtr pSrcFtWS, IFeatureWorkspacePtr pTargetFtWS)
{
    CString sLogInfo;
    sLogInfo.Format("5 �������ݵ�Ŀ���");
    WriteLog(sLogInfo);

    if ( pSrcFtWS == NULL || pTargetFtWS == NULL)
    {
        WriteLog("Ŀ�����ݿ��Դ���ݿ�Ϊ�գ��޷�������ݿ�����");
        return false;
    }

    HRESULT hr;

    IFeatureDatasetPtr pFtDataset;
    pSrcFtWS->OpenFeatureDataset(CComBSTR("DWG_X"), &pFtDataset);

    if (pFtDataset == NULL) return false;

    IEnumDatasetPtr pEnumDS;
    pFtDataset->get_Subsets(&pEnumDS);
    if (pEnumDS == NULL) return false;

    pEnumDS->Reset();
    IDatasetPtr ipDataset = NULL;
    while (pEnumDS->Next(&ipDataset) == S_OK)
    {
        CComBSTR bsLayerName;
        ipDataset->get_Name(&bsLayerName);
        CString sLayerName = GetSdeFtClsName(bsLayerName);

        IFeatureClassPtr pInFtCls;
        IFeatureClassPtr pTarFtCls;
        pSrcFtWS->OpenFeatureClass(CComBSTR(sLayerName), &pInFtCls);
        pTargetFtWS->OpenFeatureClass(CComBSTR(sLayerName), &pTarFtCls);

        if (pTarFtCls == NULL)
        {
            sLogInfo.Format("Ŀ�����ݿ��в�����%sͼ�㣬�޷�������ݿ�����", sLayerName);
            WriteLog(sLogInfo);
            continue;
        }

        long numFeats;
        pInFtCls->FeatureCount(NULL, &numFeats);
        if (numFeats <= 0) continue;

        CString sProgressText;

        sProgressText.Format("�����ύ%sͼ�������", sLayerName);

        PrgbarRange(0, numFeats);
        PrgbarSetPos(0);
        PrgbarSetText(sProgressText);

        IFeatureCursorPtr pCur;
        pInFtCls->Search(NULL, VARIANT_FALSE, &pCur);

        IFeaturePtr pTargetFeat;

        IFeaturePtr pFeat;
        pCur->NextFeature(&pFeat);
        while (pFeat != NULL)
        {
            pTarFtCls->CreateFeature(&pTargetFeat);

            CopyFeature(pFeat, pTargetFeat);

            hr = pTargetFeat->Store();

            pTargetFeat.Release();

            pFeat.Release();
            pCur->NextFeature(&pFeat);

            PrgbarStepIt();
        }

    }

    PrgbarSetPos(0);
    PrgbarSetText("");

    sLogInfo.Format("5 �������ݵ�Ŀ������!");
    WriteLog(sLogInfo);

    return false;
}

/********************************************************************
��Ҫ���� : ��������Χ
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
void XDBPreProcessDriver::PrgbarRange(int iLower, int iUpper)
{
    if (pTextProgressCtrl != NULL)
    {
        pTextProgressCtrl->SetRange(iLower, iUpper);
    }
}

void XDBPreProcessDriver::PrgbarSetPos(int iPos)
{
    if (pTextProgressCtrl != NULL)
    {
        pTextProgressCtrl->SetPos(iPos);
    }
}

void XDBPreProcessDriver::PrgbarSetText(CString sText)
{
    if (pTextProgressCtrl != NULL)
    {
        pTextProgressCtrl->SetWindowText(sText);
        pTextProgressCtrl->DoEvents();
    }
}

void XDBPreProcessDriver::PrgbarStepIt(void)
{
    if (pTextProgressCtrl != NULL)
    {
        pTextProgressCtrl->StepIt();
    }
}

void XDBPreProcessDriver::SaveLogFile(CString sFilePath)
{
    try
    {
        if (m_LogList.GetCount() > 0)
        {
            //COleDateTime dtCur = COleDateTime::GetCurrentTime();
            //CString sName = dtCur.Format("%y%m%d_%H%M%S");
            CString sLogFileName = sFilePath;
            //sLogFileName.Format("%sDwgת����־_%s.log", GetLogPath(), sName);

            CStdioFile f3(sLogFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
            for (POSITION pos = m_LogList.GetHeadPosition(); pos != NULL;)
            {
                f3.WriteString(m_LogList.GetNext(pos) + "\n");
            }

            f3.Close();

            //WinExec("Notepad.exe " + sLogFileName, SW_SHOW);
            //m_LogList.RemoveAll();
        }
    }
    catch (...)
    {
        CString sErr;
        sErr.Format("д��־��%s���������ļ�·���Ƿ���ȷ��", sFilePath);
        AfxMessageBox(sErr);
    }
}
