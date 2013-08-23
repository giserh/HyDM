// DwgReader.h : CDwgReader ������

#pragma once
#include "resource.h"       // ������



#include "HyDwgConvert_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Windows CE ƽ̨(�粻�ṩ��ȫ DCOM ֧�ֵ� Windows Mobile ƽ̨)���޷���ȷ֧�ֵ��߳� COM ���󡣶��� _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA ��ǿ�� ATL ֧�ִ������߳� COM ����ʵ�ֲ�����ʹ���䵥�߳� COM ����ʵ�֡�rgs �ļ��е��߳�ģ���ѱ�����Ϊ��Free����ԭ���Ǹ�ģ���Ƿ� DCOM Windows CE ƽ̨֧�ֵ�Ψһ�߳�ģ�͡�"
#endif

using namespace ATL;


// CDwgReader

class ATL_NO_VTABLE CDwgReader :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDwgReader, &CLSID_DwgReader>,
	public IDispatchImpl<IDwgReader, &IID_IDwgReader, &LIBID_HyDwgConvert, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CDwgReader()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DWGREADER)

DECLARE_NOT_AGGREGATABLE(CDwgReader)

BEGIN_COM_MAP(CDwgReader)
	COM_INTERFACE_ENTRY(IDwgReader)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		Close();
	}

private:
	BSTR m_FileName;
	OdDbDatabasePtr m_DwgDatabase;
	OdDbObjectIteratorPtr m_EntityIterator;
public:
	STDMETHOD(GetEntityCount)(LONG* count);
	STDMETHOD(put_FileName)(BSTR* DwgFile);
	STDMETHOD(Init)(VARIANT_BOOL* succeed);
	STDMETHOD(Close)(void);
	STDMETHOD(Read)(IDwgEntity* curEntity);
};

OBJECT_ENTRY_AUTO(__uuidof(DwgReader), CDwgReader)
