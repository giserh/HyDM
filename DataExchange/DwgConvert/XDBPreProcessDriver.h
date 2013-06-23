// XDBPreProcessDriver.h: interface for the XDBPreProcessDriver class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_XDBPREPROCESSDRIVER_H__858DD9F2_3E02_4CE2_8DF2_0A572D8E2465__INCLUDED_)
#define AFX_XDBPREPROCESSDRIVER_H__858DD9F2_3E02_4CE2_8DF2_0A572D8E2465__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "XJoinExtendTable.h"
#include "XDWGDirectReader.h"
#include "SharedUsed.h"

//////////////////////////////////////////////////////////////////////////
typedef CTypedPtrList<CPtrList, IDatasetName*>  CDsNameList;


class AFX_CLASS_EXPORT XDBPreProcessDriver  
{
public:

	CLogRecorder* m_pLogRec;

	IPolygon* CreatePolygon(IPolyline* pPLine, ISpatialReference* pSpRef);

	HRESULT PostBuildPolygon2(CString sPolygonLayerName, IFeatureClassPtr pInFtCls, CString sAreaField);


	//У׼ͼ�����
	HRESULT AdjustSdeLayerAlias();

	//��ֹ�е�ʱ������޷�ˢ�µ�����
	void DoEvents();

	void GetFtClsNames(IWorkspace* pWorkspace, CDsNameList& lstNames);

	//�޸�Ҫ�ؼ���Ҫ�������� Creator: zl
	HRESULT ChangeDatasetAndFtClsName(CString sPrefix, CString sPrjName);

	//������� by zl
	HRESULT DoPolygonPostProcess();

	void CopyFeatureUIDTable();

	XDBPreProcessDriver();
	virtual ~XDBPreProcessDriver();


	//Ҫ����cad�ļ�����
	CStringArray CadArrays;

	IFeatureWorkspace* m_pSysFtWs;

	CTextProgressCtrl* pTextProgressCtrl;


	//cad���ݸ�ʽת���Լ�ģ����պ�Ĺ����ռ�
	IWorkspace* m_pInWS;

	//���ݷֲ�Ĺ����ռ䣨Ԥ�������ݵ����չ����ռ�
	IWorkspace* m_pOutWS;

	//����ת�����Ŀ�����ݵ�����ϵͳ
	ISpatialReferencePtr ipSpatialReference;

	//��ʽת����Ŀ��Ҫ�ؼ�
	IFeatureDatasetPtr ipFeatureDataset;

	//�ֲ��������ڹ����ռ�Name
	IWorkspaceNamePtr ipWorkspace2Name;

	//���ձ��ڱ�����ö��
	IEnumDatasetNamePtr m_pEnumConfigDatasetName;

	//������չ����
	//XJoinExtendTable* joinExdDriver;


	//��չ���Ա�
	ITable* m_ipExtendTable;

	//���ձ�
	ITablePtr ipCompareTable;

	//��־�����б�
	CStringList m_LogList;

	//��ʽת����ģ�����
//	BOOL ReadCADAndConvert();

	//�Զ��ֲ�
	BOOL AutoSplitLayers(IWorkspace* pInWS, IWorkspace* pOutWS);

	//�ҽ���չ����
	BOOL JoinExtendTable();

	//��ȡ������չ���Ա�����Ҫ�Ķ��ձ�
	ITablePtr GetExtendCompareTable(ITablePtr ipTable);

	//��¼��������
	void WriteLog(CString sLog);

	//������־
	void SaveLogList(BOOL bShow =TRUE);

private:

	//ת��VARIANT��CString
	CString GetStringByVar(VARIANT var);

	CString GetSdeFtClsName(CComBSTR bsFtClsName);

protected:
	ITable* GetExtendFieldsConfigTable(CString sLayerName);
//	IFeatureWorkspacePtr GetSysWorkspace();
	void GetExtraAttribRegAppNames(CMapStringToString& mapRegAppNames);
	
public:
	bool SplitOneLayer(CMapStringToString* pSplitLayerNames, IFeatureClass* pInFtCls, IWorkspace* pTargetFtWS);
public:

	void ParseStr(CString sSrcStr, char chrSeparator, CStringList& lstItems);


	// ���߹���
	void BuildPolygon(void);

	void CopyFeatureAttr(IFeaturePtr pSourceFeature, IFeaturePtr pTargetFeature);
public:
	// //����ע�����͵�Ҫ����
	HRESULT CreateAnnoFtCls(IFeatureDataset* pTarFDS, BSTR bsAnnoName, IFeatureClass** ppAnnoFtCls);

	HRESULT CreateAnnoFtCls(IWorkspace* pWS, BSTR bsAnnoName, IFeatureClass** ppAnnoFtCls);

	//����ע��ͼ��
	HRESULT CreateAnnoFtCls(IFeatureDatasetPtr pTarFDS, BSTR bsAnnoName, IFieldsPtr pFields, IFeatureClass** ppAnnoFtCls);
public:
	// CAD�ļ�ע��ͼ������ת��
	BOOL CAD_AnnotationConvert(IWorkspace* pTargetWS, IDataset* pTargetDataset, CString sDwgFilePath, CString sShowedFilePath = "");

	//�õ��ļ����ڵ�Ŀ¼
	CString GetFileDirectory(const CString& sFullPath);

	// ����ע��Element
	ITextElement* MakeTextElementByStyle(CString strStyle, CString strText, double dblAngle,
		double dblHeight, double dblX,
		double dblY, double ReferenceScale);

	//��ע��ͼ��ֲ�
	bool SplitAnnotationLayer(CString sBaseLayerName, CMapStringToString* pSplitLayerNames, IFeatureClass* pInFtCls, IWorkspace* pTargetWS);

public:
	// CAD���ݶ�ȡ��
	XDWGReader* m_pDwgReader;
public:

	//�߹�����ڴ���
	HRESULT PostBuildPolygon(IFeatureClassPtr pInFtCls);
public:
	// �õ����е�ע��Ӧ����
	void GetRegAppNames(CStringList& lstAppNames);
public:
	// �ҽ���ӱ�
	void JoinAddinTable(ITablePtr pExtraTable);

	//����Ҫ��
	void CopyFeature(IFeaturePtr pSrcFeat, IFeaturePtr& pDestFeat);

	// �������ݵ�Ŀ���
	bool CopyToTargetDB(IFeatureWorkspacePtr pSrcFtWS, IFeatureWorkspacePtr pTargetFtWS);
	// �ҽ���չ����
	bool JoinExtendTable2(void);
	void PrgbarRange(int iLower, int iUpper);
	void PrgbarSetPos(int iPos);
	void PrgbarSetText(CString sText);
	void PrgbarStepIt(void);
	void SaveLogFile(CString sFilePath);
};

#endif // !defined(AFX_XDBPREPROCESSDRIVER_H__858DD9F2_3E02_4CE2_8DF2_0A572D8E2465__INCLUDED_)