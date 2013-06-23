// XJoinExtendTable.cpp: implementation of the XJointExtendTable class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "XJoinExtendTable.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

XJoinExtendTable::XJoinExtendTable()
{
    m_ipExtendTable = NULL;
    m_ipConfigTable = NULL;
    m_ipTargetTable = NULL;

	m_pProgressCtrl = NULL;
}

XJoinExtendTable::~XJoinExtendTable()
{
}
//DEL void XJoinExtendTable::GenerateFieldsMap()
//DEL {
//DEL 	m_sMapOldNameToNewName.RemoveAll();
//DEL 	m_sMapNameToAlias.RemoveAll();
//DEL
//DEL 	long lXOldNameIndex, lXNewNameIndex, lXAliasNameIndex;
//DEL
//DEL 	m_ipConfigTable->FindField(CComBSTR("XOldName"), &lXOldNameIndex);
//DEL 	m_ipConfigTable->FindField(CComBSTR("XNewName"), &lXNewNameIndex);
//DEL 	m_ipConfigTable->FindField(CComBSTR("XAliasName"), &lXAliasNameIndex);
//DEL
//DEL 	if (lXOldNameIndex != -1 && lXNewNameIndex != -1 && lXAliasNameIndex != -1)
//DEL 	{
//DEL 		ICursorPtr ipCursor;
//DEL 		IEsriRowPtr ipRow;
//DEL 		m_ipConfigTable->Search(NULL, VARIANT_TRUE, &ipCursor);
//DEL 		ipCursor->NextRow(&ipRow);
//DEL 		CComVariant vtOld, vtNew, vtAlias;
//DEL
//DEL 		while (ipRow != NULL)
//DEL 		{
//DEL 			ipRow->get_Value(lXOldNameIndex, &vtOld);
//DEL 			ipRow->get_Value(lXNewNameIndex, &vtNew);
//DEL 			ipRow->get_Value(lXAliasNameIndex, &vtAlias);
//DEL
//DEL 			m_sMapOldNameToNewName[CString(vtOld.bstrVal)] = CString(vtNew.bstrVal);
//DEL 			m_sMapNameToAlias[CString(vtNew.bstrVal)] = CString(vtAlias.bstrVal);
//DEL
//DEL 			ipCursor->NextRow(&ipRow);
//DEL 		}
//DEL 	}
//DEL }
//DEL BOOL XJoinExtendTable::Run()
//DEL {
//DEL 	if (m_ipExtendTable != NULL && m_ipTargetTable != NULL && m_ipConfigTable != NULL)
//DEL 	{
//DEL 		GenerateFieldsMap();
//DEL 		HRESULT hr;
//DEL 		//����Ҫ���ӵ�Ҫ����
//DEL 		ICursorPtr ipCursor;
//DEL 		ICursorPtr ipExtendCursor;
//DEL 		ICursorPtr ipCompareCursor;
//DEL
//DEL 		//��ȡ���ӱ���Handle�ֶε�����
//DEL 		long lHandleIndex, lBaseNameIndex;
//DEL 		m_ipTargetTable->FindField(CComBSTR("BaseName"), &lBaseNameIndex);
//DEL 		m_ipTargetTable->FindField(CComBSTR("Handle"), &lHandleIndex);
//DEL
//DEL
//DEL 		//��ȡ��չ����XDataName,XDataValue������
//DEL 		long lXDataNameIndex, lXDataValueIndex;
//DEL 		m_ipExtendTable->FindField(CComBSTR("XDataName"), &lXDataNameIndex);
//DEL 		m_ipExtendTable->FindField(CComBSTR("XDataValue"), &lXDataValueIndex);
//DEL
//DEL 		//��ȡ���ձ���XOldName,XNewName,XAliasName������
//DEL
//DEL 		//XOldName��ӦXDataName
//DEL
//DEL 		long lXNewNameIndex, lXAliasNameIndex;
//DEL 		m_ipConfigTable->FindField(CComBSTR("XNewName"), &lXNewNameIndex);
//DEL 		m_ipConfigTable->FindField(CComBSTR("XAliasName"), &lXAliasNameIndex);
//DEL
//DEL 		IEsriRowPtr ipRow;
//DEL 		IEsriRowPtr ipExtendRow;
//DEL 		IEsriRowPtr ipCompareRow;
//DEL
//DEL 		CComVariant vt;
//DEL 		CComVariant vtXDataName, vtXDataValue;
//DEL 		CComVariant vtXAliasName, vtXNewName;
//DEL 		CString sHandle, sBaseName;
//DEL 		CString sTemp = "0";
//DEL 		IQueryFilterPtr ipExtendQueryFilter(CLSID_QueryFilter);
//DEL 		IQueryFilterPtr ipCompareFilter(CLSID_QueryFilter);
//DEL 		CString sWhereClause;
//DEL 		CString sCompareWhereClause;
//DEL
//DEL 		long lRowCount;
//DEL 		m_ProgressCtrl->SetPos(0);
//DEL 		m_ipTargetTable->RowCount(NULL, &lRowCount);
//DEL 		m_ProgressCtrl->SetRange(0, lRowCount + 1);
//DEL
//DEL 		CString sInfo;
//DEL 		sInfo.Format("���ڶ����ӱ��������ֶ�...");
//DEL
//DEL 		m_ProgressCtrl->ShowWindow(SW_SHOW);
//DEL 		WriteLog(sInfo);
//DEL 		m_ProgressCtrl->SetWindowText(sInfo);
//DEL 		m_ProgressCtrl->StepIt();
//DEL 		//�����ֶ�
//DEL 		//�����ӱ��а���m_sMapNameToAlias�ļ�¼������Ӧ���ֶ�
//DEL 		POSITION pos = m_sMapNameToAlias.GetStartPosition();
//DEL 		CString sName, sAlias;
//DEL 		IFieldsPtr ipFields;
//DEL 		m_ipTargetTable->get_Fields(&ipFields);
//DEL 		long lIndex;
//DEL
//DEL 		while (pos != NULL)
//DEL 		{
//DEL 			m_sMapNameToAlias.GetNextAssoc(pos, sName, sAlias);
//DEL 			ipFields->FindField(CComBSTR(sName), &lIndex);
//DEL 			if (lIndex == -1)
//DEL 			{
//DEL 				sInfo.Format("�ɹ������ֶ�:%s", sName);
//DEL 				WriteLog(sInfo);
//DEL 				//�����½��ֶ�
//DEL 				IFieldPtr ipField(CLSID_Field);
//DEL 				IFieldEditPtr ipFieldEdit = ipField;
//DEL 				ipFieldEdit->put_AliasName(CComBSTR(sAlias));
//DEL 				ipFieldEdit->put_Name(CComBSTR(sName));
//DEL
//DEL 				/******************************************
//DEL 								 �޸�ԭ�� :  �ѹҽӵĲ�������ߺ�����ֶ���������Ϊ���κ�double��
//DEL 								   * *****************************************/
//DEL 				if (sAlias == "����")
//DEL 					ipFieldEdit->put_Type(esriFieldTypeInteger);
//DEL 				else if (sAlias == "���" || sAlias == "���")
//DEL 					ipFieldEdit->put_Type(esriFieldTypeDouble);
//DEL 				else
//DEL 					ipFieldEdit->put_Type(esriFieldTypeString);
//DEL
//DEL 				hr = m_ipTargetTable->AddField(ipField);
//DEL 				if (FAILED(hr))
//DEL 				{
//DEL 					XConvertHelper::CatchErrorInfo();
//DEL 				}
//DEL 			}
//DEL 		}
//DEL
//DEL 		m_ipTargetTable->Update(NULL, VARIANT_FALSE, &ipCursor);
//DEL
//DEL 		ipCursor->NextRow(&ipRow);
//DEL
//DEL 		m_ipTargetTable->get_Fields(&ipFields);
//DEL
//DEL 		long lCount = 0;
//DEL
//DEL 		CString sProgressText;
//DEL 		long lValueIndex;
//DEL
//DEL 		long lUpdateCount = 0;
//DEL
//DEL 		while (ipRow != NULL)
//DEL 		{
//DEL 			lCount++;
//DEL 			sProgressText.Format("�������ӱ��%d��Ҫ����CAD��չ����֮��Ĺ�ϵ...", lCount);
//DEL 			m_ProgressCtrl->SetWindowText(sProgressText);
//DEL 			m_ProgressCtrl->StepIt();
//DEL
//DEL 			//��ȡHandle�ֶε�ֵ
//DEL 			ipRow->get_Value(lHandleIndex, &vt);
//DEL 			sHandle = vt.bstrVal;
//DEL 			ipRow->get_Value(lBaseNameIndex, &vt);
//DEL 			sBaseName = vt.bstrVal;
//DEL
//DEL 			//ȥExtendTableѰ�����Handle��Ӧ���ֶ�
//DEL 			sWhereClause.Format("Handle = '%s' and BaseName ='%s'", sHandle, sBaseName);
//DEL 			ipExtendQueryFilter->put_WhereClause(CComBSTR(sWhereClause));
//DEL 			m_ipExtendTable->Search(ipExtendQueryFilter, TRUE, &ipExtendCursor);
//DEL 			ipExtendCursor->NextRow(&ipExtendRow);
//DEL
//DEL 			CString sXDataName;
//DEL 			while (ipExtendRow != NULL)
//DEL 			{
//DEL 				ipExtendRow->get_Value(lXDataNameIndex, &vtXDataName);
//DEL 				sXDataName = vtXDataName.bstrVal;
//DEL
//DEL 				if (m_sMapOldNameToNewName.Lookup(sXDataName, sTemp))
//DEL 				{
//DEL 					ipFields->FindField(CComBSTR(sTemp), &lValueIndex);
//DEL 					if (lValueIndex != -1)
//DEL 					{
//DEL 						ipExtendRow->get_Value(lXDataValueIndex, &vtXDataValue);
//DEL 						ipRow->put_Value(lValueIndex, vtXDataValue);
//DEL 					}
//DEL 				}
//DEL
//DEL 				ipExtendCursor->NextRow(&ipExtendRow);
//DEL 			}
//DEL 			ipCursor->UpdateRow(ipRow);
//DEL 			ipCursor->NextRow(&ipRow);
//DEL 		}
//DEL
//DEL 		sInfo.Format("�ɹ��������ӱ�");
//DEL 		WriteLog(sInfo);
//DEL
//DEL 		ipCursor.Release();
//DEL 		m_ProgressCtrl->SetPos(0);
//DEL 		m_ProgressCtrl->ShowWindow(SW_HIDE);
//DEL 		return TRUE;
//DEL 	}
//DEL 	return FALSE;
//DEL }


/************************************************************************
��Ҫ���� :
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XJoinExtendTable::WriteLog(CString sLog)
{
    if (!sLog.IsEmpty())
    {
        COleDateTime dtCur = COleDateTime::GetCurrentTime();
        CString sLogTime = dtCur.Format("%Y/%m/%d %H:%M:%S");
        sLog = sLogTime + "-" + sLog;
        m_pLogList->AddTail(sLog);
    }
}

/************************************************************************
��Ҫ���� : ������չ�ֶ�,����ֵ
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XJoinExtendTable::AddExtendFieldsValue(CString sLayerName)
{
    if (m_ipExtendTable == NULL || m_ipTargetTable == NULL || m_ipConfigTable == NULL) return;

    HRESULT hr;
    long lFieldIndex;
    CString sInfo;
    CString sProgressText;
    CComVariant vtFieldVal;
    CStringList lstNewFieldsName;

    //////////////////////////////////////////////////////////////////////////
    // 1 ������չ�ֶ�
    IFieldsPtr ipTargetTableFields;
    m_ipTargetTable->get_Fields(&ipTargetTableFields);

    //����˳��Ŷ�ȡ,�����ֶ����ñ������ֶ� �޷�ʹ��ITableSortPtr,��Ϊ��¼������ArcObject
    long lNumRows;
    m_ipConfigTable->RowCount(NULL, &lNumRows);
    for (int i = 1; i <= lNumRows; i++)
    {
        IQueryFilterPtr pConfigFilter(CLSID_QueryFilter);
        CString sWhereClause;
        sWhereClause.Format("FIELD_INDEX=%d", i);
        pConfigFilter->put_WhereClause(CComBSTR(sWhereClause));

        IEsriCursorPtr pConfigCursor;
        m_ipConfigTable->Search(pConfigFilter, VARIANT_FALSE, &pConfigCursor);
        if (pConfigCursor == NULL)
        {
            sInfo.Format("%sͼ�����ñ����ݴ���!����.", sLayerName);
            WriteLog(sInfo);
            return;
        }

        IEsriRowPtr pConfigRow;
        pConfigCursor->NextRow(&pConfigRow);
        if (pConfigRow == NULL)
        {
            sInfo.Format("%sͼ�����ñ����ݴ���!����.", sLayerName);
            WriteLog(sInfo);
            return;
        }

        CString sNewFieldName, sNewFieldAlias;

        IFieldsPtr pFields;
        pConfigRow->get_Fields(&pFields);

        //�����ֶ�����
        pFields->FindField(CComBSTR("FIELD_NAME"), &lFieldIndex);
        pConfigRow->get_Value(lFieldIndex, &vtFieldVal);
        if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
        {
            sNewFieldName = vtFieldVal.bstrVal;
        }

        //�����ֶα���
        pFields->FindField(CComBSTR("FIELD_ALIAS"), &lFieldIndex);
        pConfigRow->get_Value(lFieldIndex, &vtFieldVal);
        if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
        {
            sNewFieldAlias = vtFieldVal.bstrVal;
        }

        long lTargetFieldIndex;
        ipTargetTableFields->FindField(CComBSTR(sNewFieldName), &lTargetFieldIndex);
        if (lTargetFieldIndex == -1)
        {
            //Add new Field
            //�����ֶ�
            IFieldPtr ipField(CLSID_Field);
            IFieldEditPtr ipFieldEdit = ipField;
            ipFieldEdit->put_Name(CComBSTR(sNewFieldName));
            ipFieldEdit->put_AliasName(CComBSTR(sNewFieldAlias));
            ipFieldEdit->put_Type(esriFieldTypeString);
            ipFieldEdit->put_Length(1024);

            hr = m_ipTargetTable->AddField(ipField);
            if (SUCCEEDED(hr))
            {
                sInfo.Format("�ɹ������ֶ�:%s", sNewFieldAlias);
                WriteLog(sInfo);
            }

            lstNewFieldsName.AddTail(sNewFieldName);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // 2 ����չ�ֶθ�ֵ
    long lRowCount;
    if (m_pProgressCtrl != NULL)
    {
        m_pProgressCtrl->SetPos(0);
        m_ipTargetTable->RowCount(NULL, &lRowCount);
        m_pProgressCtrl->SetRange(0, lRowCount + 1);
    }


    IEsriCursorPtr pTargetCursor;
    IEsriCursorPtr ipExtendCursor;
    IEsriCursorPtr ipCompareCursor;

    IEsriRowPtr pTargetRow;
    IEsriRowPtr ipExtendRow;
    IEsriRowPtr ipCompareRow;


    long lTargetHandleFieldIndex, lTargetBaseNameFieldIndex;
    m_ipTargetTable->FindField(CComBSTR("Handle"), &lTargetHandleFieldIndex);
    m_ipTargetTable->FindField(CComBSTR("BaseName"), &lTargetBaseNameFieldIndex);
    if (lTargetHandleFieldIndex == -1 || lTargetHandleFieldIndex == -1)
    {
        sInfo.Format("%sͼ���ֶδ���!", sLayerName);
        WriteLog(sInfo);
        return;
    }

    long lExtendTableDataNameIndex, lExtendTableDataValueIndex;
    m_ipExtendTable->FindField(CComBSTR("XDataName"), &lExtendTableDataNameIndex);
    m_ipExtendTable->FindField(CComBSTR("XDataValue"), &lExtendTableDataValueIndex);
    if (lExtendTableDataNameIndex == -1 || lExtendTableDataValueIndex == -1)
    {
        sInfo.Format("��չ��ͼ���ֶδ���!");
        WriteLog(sInfo);
        return;
    }

    lRowCount = 0;
    m_ipTargetTable->Update(NULL, VARIANT_FALSE, &pTargetCursor);
    pTargetCursor->NextRow(&pTargetRow);
    while (pTargetRow != NULL)
    {
        CStringList lstNewFieldValues;
        CString sTargetHandle, sTargetBaseName;

        //Ŀ���Handle
        pTargetRow->get_Value(lTargetHandleFieldIndex, &vtFieldVal);
        if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
        {
            sTargetHandle = vtFieldVal.bstrVal;
        }

        //Ŀ���BaseName
        pTargetRow->get_Value(lTargetBaseNameFieldIndex, &vtFieldVal);
        if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
        {
            sTargetBaseName = vtFieldVal.bstrVal;
        }

        CString sExtendName, sExtendValues;
        IQueryFilterPtr pExtendQueryFilter(CLSID_QueryFilter);
        CString sWhereClause;

        CString sRegAppName = "";

        /*if (m_mapRegAppName != NULL) //���ж�ע��Ӧ����
        {
            m_mapRegAppName->Lookup(sLayerName, sRegAppName); //��չ����ע������
        }*/

        if (!sRegAppName.IsEmpty())
        {
            sWhereClause.Format("Handle='%s' and BaseName='%s' and XDataName='%s'", sTargetHandle, sTargetBaseName, sRegAppName);
        }
        else
        {
            sWhereClause.Format("Handle = '%s' and BaseName ='%s'", sTargetHandle, sTargetBaseName);
        }
        pExtendQueryFilter->put_WhereClause(CComBSTR(sWhereClause));
        m_ipExtendTable->Search(pExtendQueryFilter, FALSE, &ipExtendCursor);
        if (ipExtendCursor != NULL)
        {
            while (ipExtendCursor->NextRow(&ipExtendRow) == S_OK)
            {
                if (ipExtendRow != NULL)
                {

                    //�ж�ע��Ӧ����
                    {
                        CString sAppName;
                        //ע��Ӧ����
                        ipExtendRow->get_Value(lExtendTableDataNameIndex, &vtFieldVal);
                        if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
                        {
                            sAppName = vtFieldVal.bstrVal;
                        }
                        POSITION posRegApp = m_lstRegApps->Find(sAppName);
                        if (posRegApp == NULL) continue;
                    }

                    //��չ����
                    ipExtendRow->get_Value(lExtendTableDataValueIndex, &vtFieldVal);
                    if (vtFieldVal.vt != VT_NULL && vtFieldVal.vt != VT_EMPTY)
                    {
                        sExtendValues = vtFieldVal.bstrVal;
                    }

                    int iBeginTokenPos = 0, iEndTokenPos = 0;
                    if (!sExtendValues.IsEmpty())
                    {
                        CString sVal;
                        int iBeginTokenPos = sExtendValues.Find("[");
                        int iEndTokenPos = sExtendValues.Find("]");

                        while (iEndTokenPos != -1)
                        {
                            sVal = sExtendValues.Mid(iBeginTokenPos + 1,
                                                     iEndTokenPos - (iBeginTokenPos + 1));
                            sExtendValues = sExtendValues.Mid(iEndTokenPos + 1);
                            lstNewFieldValues.AddTail(sVal);

                            iBeginTokenPos = sExtendValues.Find("[");
                            iEndTokenPos = sExtendValues.Find("]");
                        }
                    }
                }
            }
        }

        //�ж�ֵ�Ƿ����¼��ֶθ�����ͬ
        if (lstNewFieldValues.GetCount() >= lstNewFieldsName.GetCount())
        {
            //��ֵ�������ֶ�
            POSITION posFields = lstNewFieldsName.GetHeadPosition();
            POSITION posValues = lstNewFieldValues.GetHeadPosition();

            CString sFieldName, sFieldValue;
            while (posFields != NULL)
            {
                sFieldName = lstNewFieldsName.GetNext(posFields);
                sFieldValue = lstNewFieldValues.GetNext(posValues);

                m_ipTargetTable->FindField(CComBSTR(sFieldName), &lFieldIndex);
                pTargetRow->put_Value(lFieldIndex, CComVariant(sFieldValue));
            }
        }

        if (m_pProgressCtrl != NULL)
        {
            sProgressText.Format("���ڴ���%s��ĵ�%d��Ҫ������չ���ԵĹҽ�...", sLayerName, lRowCount++);
            m_pProgressCtrl->SetWindowText(sProgressText);
            m_pProgressCtrl->StepIt();
        }

        pTargetCursor->UpdateRow(pTargetRow);
        //��һ��
        pTargetCursor->NextRow(&pTargetRow);
    }

    pTargetCursor.Release();

	if (m_pProgressCtrl != NULL)
	{
		m_pProgressCtrl->SetWindowText("");
		m_pProgressCtrl->SetPos(0);
	}

}
