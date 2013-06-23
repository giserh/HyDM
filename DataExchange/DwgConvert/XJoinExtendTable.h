//////////////////////////////////////////////////////////////////////////

#include "afxcoll.h"

#ifndef AFX_XJOINEXTENDTABLE_H____INCLUDED_
#define AFX_XJOINEXTENDTABLE_H____INCLUDED_

class AFX_CLASS_EXPORT XJoinExtendTable  
{
public:
	XJoinExtendTable();
	virtual ~XJoinExtendTable();

public:
	void AddExtendFieldsValue(CString sLayerName);
	CMapStringToString* m_mapRegAppName;
	ITable* m_ipExtendTable;
	ITable* m_ipConfigTable;
	ITable* m_ipTargetTable;
	CStringList* m_pLogList;

	CTextProgressCtrl* m_pProgressCtrl;

	// CAD�е�����ֶε�����ע��Ӧ����
	CStringList* m_lstRegApps;

private:
	void WriteLog(CString sLog);
	//�¾��ֶ����ƶ���
	CMapStringToString m_sMapOldNameToNewName;
	//�������ֶ����ƶԱ���
	CMapStringToString m_sMapNameToAlias;

};

#endif // !defined(AFX_XJOINTEXTENDTABLE_H__08A51BCA_6699_46BE_86BF_8343E1CF191C__INCLUDED_)
