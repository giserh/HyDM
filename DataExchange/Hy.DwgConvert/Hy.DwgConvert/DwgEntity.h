// DwgEntity.h : CDwgEntity ������

#pragma once
#include "resource.h"       // ������
#include "ESRI_AoInterface.h"

#include "HyDwgConvert_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Windows CE ƽ̨(�粻�ṩ��ȫ DCOM ֧�ֵ� Windows Mobile ƽ̨)���޷���ȷ֧�ֵ��߳� COM ���󡣶��� _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA ��ǿ�� ATL ֧�ִ������߳� COM ����ʵ�ֲ�����ʹ���䵥�߳� COM ����ʵ�֡�rgs �ļ��е��߳�ģ���ѱ�����Ϊ��Free����ԭ���Ǹ�ģ���Ƿ� DCOM Windows CE ƽ̨֧�ֵ�Ψһ�߳�ģ�͡�"
#endif

using namespace ATL;


// CDwgEntity

class ATL_NO_VTABLE CDwgEntity :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDwgEntity, &CLSID_DwgEntity>,
	public IDwgEntity
{
public:
	CDwgEntity()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DWGENTITY)

DECLARE_NOT_AGGREGATABLE(CDwgEntity)

BEGIN_COM_MAP(CDwgEntity)
	COM_INTERFACE_ENTRY(IDwgEntity)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}


private:
	CComBSTR m_GeometryType;
	USHORT m_Color;
	CComBSTR m_Handle;
	IGeometry* m_Shape;
public:


	STDMETHOD(get_GeometryType)(BSTR*  pVal);
	STDMETHOD(put_GeometryType)(BSTR  newVal);
	STDMETHOD(get_Color)(USHORT* pVal);
	STDMETHOD(put_Color)(USHORT newVal);
	STDMETHOD(get_Handle)(BSTR* pVal);
	STDMETHOD(put_Handle)(BSTR newVal);
	STDMETHOD(get_Shape)(IGeometry** pVal);
	STDMETHOD(put_Shape)(IGeometry* newVal);
};

OBJECT_ENTRY_AUTO(__uuidof(DwgEntity), CDwgEntity)
