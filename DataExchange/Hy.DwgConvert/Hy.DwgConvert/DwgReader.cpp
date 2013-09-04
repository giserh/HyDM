// DwgReader.cpp : CDwgReader ��ʵ��

#include "stdafx.h"
#include "DwgReader.h"

#include "ExSystemServices.h"
#include "ExHostAppServices.h"
#include "RxDynamicModule.h"
#include "EntityAdapter.h"
#include "DwgInclude/DbRegAppTable.h";

class DwgReaderServices : public ExSystemServices, public ExHostAppServices
{
protected:
    ODRX_USING_HEAP_OPERATORS(ExSystemServices);
};

OdRxObjectImpl<DwgReaderServices> svcs;
static ExProtocolExtension theProtocolExtensions;
   
HRESULT  CDwgReader::GetEntityCount(LONG* count)
{
	 // Open ModelSpace
	OdDbBlockTableRecordPtr pBlock = this->m_DwgDatabase->getModelSpaceId().safeOpenObject();

    LONG eCount=0;	
    for (OdDbObjectIteratorPtr pEntIter = pBlock->newIterator(); !pEntIter->done(); pEntIter->step())
    {
        eCount++;
    }

	*count=eCount;

	return S_OK;
}

STDMETHODIMP CDwgReader::put_FileName(BSTR* DwgFile)
{
	this->m_FileName=*DwgFile;

	return S_OK;
}

STDMETHODIMP CDwgReader::Init(VARIANT_BOOL* succeed)
{
	try
	{
		odInitialize(&svcs);

		CString strFile=this->m_FileName;
		this->m_DwgDatabase = svcs.readFile(strFile , false, false, Oda::kShareDenyReadWrite);
        *succeed=this->m_DwgDatabase.isNull();

		theProtocolExtensions.initialize();
	}
	catch(...)
	{
		return S_FALSE;
	}


	return S_OK;
}

STDMETHODIMP CDwgReader::Close(void)
{
	
	this->m_DwgDatabase.release();

	theProtocolExtensions.uninitialize();

	return S_OK;
}



STDMETHODIMP CDwgReader::Read(IDwgEntity** curEntity)
{
	// ��ȡ��ȡ��
	if(this->m_EntityIterator.isNull())
	{ 
		OdDbBlockTableRecordPtr pBlock = this->m_DwgDatabase->getModelSpaceId().safeOpenObject();
		this->m_EntityIterator=pBlock->newIterator();
	}

	// ѭ����ȡ
    this->m_EntityIterator->step();
	if(!this->m_EntityIterator->done())
	{
		
		// �������
		OdDbObjectId pID= this->m_EntityIterator->objectId();
		OdDbEntityPtr pEnt = pID.safeOpenObject();
		
		
		CComObject<CDwgEntity>* pEntity;
		CComObject<CDwgEntity>::CreateInstance(&pEntity);
		pEntity->setInnerEntity(pEnt);		
		pEntity->QueryInterface(IID_IDwgEntity, (void**)&(*curEntity));

		// *curEntity=EntityAdapter::AdaptToEntity(pEnt);
		
		//IDwgEntity* newEntity;//=coEntity;
		//::CoCreateInstance(CLSID_DwgEntity,NULL,CLSCTX_SERVER,IID_IDwgEntity,(void **)&newEntity);
		//
		//OdDbHandle hTmp;
  //      char szEntityHandle[50] = {0};
  //      hTmp = pEnt->getDbHandle();
  //      hTmp.getIntoAsciiBuffer(szEntityHandle);
		//CString strHandle=szEntityHandle;
		//newEntity->put_Handle(strHandle.AllocSysString());
		//
		//*curEntity=newEntity;
	}

	return S_OK;
}



STDMETHODIMP CDwgReader::GetRegAppNames(SAFEARRAY** pVal)
{
	OdDbRegAppTablePtr pRegAppTable= this->m_DwgDatabase->getRegAppTableId().safeOpenObject();
	OdDbSymbolTableIteratorPtr pRegAppIter= pRegAppTable->newIterator();
	// ����
	long lCount=0;
	for(;! pRegAppIter->done();pRegAppIter->step(),lCount++)
	{	
	}

	// ����Com����ֵ
	SAFEARRAYBOUND rgsabound[1];
	rgsabound[0].cElements = lCount;
	rgsabound[0].lLbound = 0;

	*pVal= ::SafeArrayCreate(VT_BSTR,UINT(1),rgsabound);
	pRegAppIter= pRegAppTable->newIterator();
	for(long index=0;! pRegAppIter->done();pRegAppIter->step(),index++)
	{
		CComBSTR strName= pRegAppIter->getRecord()->getName();	
		::SafeArrayPutElement(*pVal,&index,(void*)strName);
	}
	
	return S_OK;
}
