#include "StdAfx.h"
#include "SharedUsed.h"

#include <math.h>


SharedUsed::SharedUsed(void)
{
}

SharedUsed::~SharedUsed(void)
{
}

/************************************************************************
��Ҫ���� : ϵͳ��־�ļ�Ŀ¼
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
CString GetLogPath()
{
	TCHAR lpFile[MAX_PATH];
	::GetModuleFileName(NULL, lpFile, MAX_PATH);
	*_tcsrchr(lpFile, '\\') = 0;
	CString sModuleFilePath = lpFile;
	CString sLogPath;
	sLogPath.Format("%s\\Log\\", sModuleFilePath);
	if (FILE_ATTRIBUTE_DIRECTORY != ::GetFileAttributes(sLogPath))
	{
		::CreateDirectory(sLogPath, NULL);
	}

	return sLogPath;
}


/************************************************************************
��Ҫ���� : ��IFeatureClass��ȡ�ÿռ�ο�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
IGeometryDef* API_GetGeometryDef(IFeatureClass* pFeatureClass)
{
	IGeometryDefPtr ipGeometryDef;
	IGeometryDefEditPtr ipGeomEdit;
	IFieldsPtr pFeatureClassFields;
	pFeatureClass->get_Fields(&pFeatureClassFields);
	IFieldPtr ipField;
	ISpatialReferencePtr ipSpf;

	BSTR bsShapeFieldName;
	LONG lFieldIndex = 0;
	pFeatureClass->get_ShapeFieldName(&bsShapeFieldName);
	pFeatureClassFields->FindField(bsShapeFieldName, &lFieldIndex);
	pFeatureClassFields->get_Field(lFieldIndex, &ipField);
	if (ipField != NULL)
	{
		ipField->get_GeometryDef(&ipGeometryDef);
		ipGeometryDef->get_SpatialReference(&ipSpf);

		//double dGridsize(1000);
		//VARIANT_BOOL bPrecision;
		//ipSpf->HasXYPrecision(&bPrecision);
		//if (bPrecision)
		//{
		//	double xmin, ymin, xmax, ymax, darea;
		//	ipSpf->GetDomain(&xmin, &xmax, &ymin, &ymax);
		//	darea = (xmax - xmin) * (ymax - ymin);
		//	dGridsize = sqrt(darea / 100);
		//}

		ipGeomEdit = ipGeometryDef;
		if (ipGeomEdit)
		{
			ipGeomEdit->put_GridCount(1);
			ipGeomEdit->put_AvgNumPoints(2);
			ipGeomEdit->put_GridSize(0, 120);
			ipGeomEdit->putref_SpatialReference(ipSpf);
		}
	}

	if (ipGeomEdit != NULL)
	{
		return ipGeomEdit.Detach();
	}

	return NULL;
}


/************************************************************************
��Ҫ���� : ���ݲ�ѯ������ԴҪ����ת����Ŀ�깤���ռ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void API_ConvertFeatureClass(IFeatureClass* pSrcFtCls,
															 IQueryFilterPtr pQueryFilter,
															 IWorkspace* pTargetWorkspace,
															 LPSTR sTargetDatasetName,
															 LPSTR sTargetFeatureClassName,
															 IFieldsPtr pTargetFields)
{
	if (pSrcFtCls == NULL) return ;

	IDatasetPtr pDs = pSrcFtCls;
	if (pDs == NULL) return ;

	INamePtr pOriName;
	pDs->get_FullName(&pOriName);

	IFeatureClassNamePtr pFtClsName(pOriName);
	if (pFtClsName == NULL) return ;

	HRESULT hr;

	//�ռ�ο�
	IGeometryDefPtr pGeometryDef = API_GetGeometryDef(pSrcFtCls);

	//Ŀ���ֶ�
	if (pTargetFields == NULL)
	{
		IFieldsPtr pFields;
		pSrcFtCls->get_Fields(&pFields);
		IFieldCheckerPtr pFieldChecker(CLSID_FieldChecker);
		IEnumFieldErrorPtr pEnumFieldErr;
		hr = pFieldChecker->Validate(pFields, &pEnumFieldErr, &pTargetFields);
	}

	//Ŀ�깤���ռ�Name
	IWorkspaceNamePtr pTargetWorkspaceName(CLSID_WorkspaceName);
	// Target Workspace path name
	CComBSTR bsPathName;
	hr = pTargetWorkspace->get_PathName(&bsPathName);
	pTargetWorkspaceName->put_WorkspaceFactoryProgID(CComBSTR(_T("esriDataSourcesGDB.AccessWorkspaceFactory.1")));
	hr = pTargetWorkspaceName->put_PathName(CComBSTR(bsPathName));

	//Target FeatureClassName
	IFeatureClassNamePtr pTargetFeatureClassName(CLSID_FeatureClassName);
	IDatasetNamePtr pTargetFCDatasetName = pTargetFeatureClassName;
	pTargetFCDatasetName->putref_WorkspaceName(pTargetWorkspaceName);
	//��֤Ҫ��������
	CComBSTR bsFinalSaveName;
	long lErr;
	IFieldCheckerPtr pFeatureClassNameChecker(CLSID_FieldChecker);
	pFeatureClassNameChecker->putref_ValidateWorkspace(pTargetWorkspace);
	pFeatureClassNameChecker->ValidateTableName(CComBSTR(sTargetFeatureClassName),
		&bsFinalSaveName,
		&lErr);
	pTargetFCDatasetName->put_Name(bsFinalSaveName);

	//Target FeatureDatasetName
	IFeatureDatasetNamePtr pTargetFeatureDatasetName = NULL;
	CString sTT = sTargetDatasetName;
	if (!sTT.IsEmpty())
	{
		pTargetFeatureDatasetName.CreateInstance(CLSID_FeatureDatasetName);
		IDatasetNamePtr pTargetDSName = pTargetFeatureDatasetName;
		pTargetDSName->putref_WorkspaceName(pTargetWorkspaceName);
		pTargetDSName->put_Name(CComBSTR(sTargetDatasetName));
	}

	IFeatureDataConverterPtr pFeatureDataConverter(CLSID_FeatureDataConverter);
	IEnumInvalidObjectPtr pEnumInvalidObject;

	hr = pFeatureDataConverter->ConvertFeatureClass(pFtClsName,
		pQueryFilter,
		pTargetFeatureDatasetName,
		pTargetFeatureClassName,
		pGeometryDef,
		pTargetFields,
		0,
		1000,
		0,
		&pEnumInvalidObject);

	/*if (SUCCEEDED(hr))
	{
		INamePtr pName(pTargetFeatureClassName);
		IUnknown* pUnk;
		hr = pName->Open(&pUnk);
		IFeatureClassPtr pTargetFC(pUnk);
		if (pTargetFC != NULL)
		{
			return pTargetFC.Detach();
		}
		else
		{
			return NULL;
		}
	}
	else
	{
		//CString sErrorInfo = API_CatchErrorInfo();
		return NULL;
	}*/

}

/************************************************************************
��Ҫ���� : ��IFeatureClass��ȡ�ÿռ�ο�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
ISpatialReference* API_GetSpatialReference(IFeatureClass* pFeatureClass)
{
    IGeometryDefPtr ipGeometryDef;
    IGeometryDefEditPtr ipGeomEdit;
    IFieldsPtr pFeatureClassFields;
    pFeatureClass->get_Fields(&pFeatureClassFields);
    IFieldPtr ipField;
    ISpatialReference* ipSpf = NULL;

    BSTR bsShapeFieldName;
    LONG lFieldIndex = 0;
    pFeatureClass->get_ShapeFieldName(&bsShapeFieldName);
    pFeatureClassFields->FindField(bsShapeFieldName, &lFieldIndex);
    pFeatureClassFields->get_Field(lFieldIndex, &ipField);
    if (ipField != NULL)
    {
        ipField->get_GeometryDef(&ipGeometryDef);
        ipGeometryDef->get_SpatialReference(&ipSpf);
    }

    if (ipSpf != NULL)
    {
		IClonePtr pClone;
		IClone* pCloned;
		pClone = ipSpf;
		pClone->Clone(&pCloned);

		ISpatialReferencePtr pClonedSpRef = pCloned;


        return pClonedSpRef.Detach();
    }

    return NULL;
}