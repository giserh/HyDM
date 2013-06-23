// DwgWriter.h : CDwgWriter ������

#pragma once

#include "resource.h"       // ������
#include "DwgConvert.h"
#include "XDwgDirectWriter.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Windows CE ƽ̨(�粻�ṩ��ȫ DCOM ֧�ֵ� Windows Mobile ƽ̨)���޷���ȷ֧�ֵ��߳� COM ���󡣶��� _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA ��ǿ�� ATL ֧�ִ������߳� COM ����ʵ�ֲ�����ʹ���䵥�߳� COM ����ʵ�֡�rgs �ļ��е��߳�ģ���ѱ�����Ϊ��Free����ԭ���Ǹ�ģ���Ƿ� DCOM Windows CE ƽ̨֧�ֵ�Ψһ�߳�ģ�͡�"
#endif


typedef CTypedPtrMap<CMapStringToPtr, CString, XDataAttrLists*> XDataConfigs;


// CDwgWriter

class ATL_NO_VTABLE CDwgWriter :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDwgWriter, &CLSID_DwgWriter>,
	public IDispatchImpl<IDwgWriter, &IID_IDwgWriter, &LIBID_DwgConvertLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CDwgWriter()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DWGWRITER)


BEGIN_COM_MAP(CDwgWriter)
	COM_INTERFACE_ENTRY(IDwgWriter)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		//m_pgrsDlg.Close();
	}

private:
	XDwgWriter m_dwgWriter;
	XDataConfigs m_XDataCfgs;

	CDlgProgressBar m_pgrsDlg;

public:

	STDMETHOD(SetCompareTable)(BSTR sCompareField, ITable* pCompareTable);

	STDMETHOD(InitWriteDwg)(BSTR sDwgFile, BSTR sTemplateFile);
	STDMETHOD(FeatureClass2Dwgfile)(IFeatureClass* pFtCls);
	STDMETHOD(put_XDataXMLConfigFile)(BSTR sXMLFile);
	STDMETHOD(put_LogFilePath)(BSTR newVal);
	STDMETHOD(Close)(void);
	STDMETHOD(Cursor2Dwgfile)(BSTR sFeatureClass, IFeatureCursor* pFtCur, LONG numFeatures);
	STDMETHOD(Cursor2DwgLayer)(BSTR sFeatureClass, IFeatureCursor* pFtCur, LONG numFeatures, BSTR sDwgLayer);
	STDMETHOD(SetCompareField2)(BSTR sConfigField, BSTR sGdbField);
};

OBJECT_ENTRY_AUTO(__uuidof(DwgWriter), CDwgWriter)
