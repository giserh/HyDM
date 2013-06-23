// DwgConvert.cpp : DLL ������ʵ�֡�


#include "stdafx.h"
#include "resource.h"
#include "DwgConvert.h"


class CDwgConvertModule : public CAtlDllModuleT< CDwgConvertModule >
{
public :
	DECLARE_LIBID(LIBID_DwgConvertLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_DwgConvert, "{8F8C08F9-B061-447C-AA02-CE247A95E95C}")
};

CDwgConvertModule _AtlModule;

class CDwgConvertApp : public CWinApp
{
public:

// ��д
    virtual BOOL InitInstance();
    virtual int ExitInstance();

    DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CDwgConvertApp, CWinApp)
END_MESSAGE_MAP()

CDwgConvertApp theApp;

BOOL CDwgConvertApp::InitInstance()
{
    return CWinApp::InitInstance();
}

int CDwgConvertApp::ExitInstance()
{
    return CWinApp::ExitInstance();
}


// ����ȷ�� DLL �Ƿ���� OLE ж��
STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _AtlModule.GetLockCount()==0) ? S_OK : S_FALSE;
}


// ����һ���๤���Դ������������͵Ķ���
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}


// DllRegisterServer - ������ӵ�ϵͳע���
STDAPI DllRegisterServer(void)
{
    // ע��������Ϳ�����Ϳ��е����нӿ�
    HRESULT hr = _AtlModule.DllRegisterServer();
	return hr;
}


// DllUnregisterServer - �����ϵͳע������Ƴ�
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	return hr;
}

