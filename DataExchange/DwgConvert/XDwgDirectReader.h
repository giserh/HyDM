//////////////////////////////////////////////////////////////////////////
//�� �� �� : XDwgDirectReader.h
//�������� : 2008/09/26,BeiJing.
//////////////////////////////////////////////////////////////////////////
#ifndef AFX_XDWGREADER_H____INCLUDED_
#define AFX_XDWGREADER_H____INCLUDED_
#include <afxtempl.h>
#include "SharedUsed.h"

//////////////////////////////////////////////////////////////////////////
class OdDbDatabase;
class OdDbObjectId;
class OdResBuf;
class OdDbEntity;
class OdRxObject;
struct DwgPoint
{
	double x;
	double y;
	double z;
};

///////////////////XDWGReader/////////////////////////////////////////////////
class XDWGReader
{
public:
	XDWGReader();
	virtual ~XDWGReader();

	//////////////////////�ӿں���////////////////////////////////////////////////////
	//������ȡǰ��׼������
	BOOL PrepareReadDwg(IWorkspace* pTargetWS, IDataset* pTargetDataset, ISpatialReference* pSpRef);

	//�����ȡCAD�ļ�
	BOOL ReadFile(LPCTSTR lpdwgFilename);

	//������־���·��
	void PutLogFilePath(CString sLogFile);

	//����DWG�Ķ�ȡ����
	BOOL CommitReadDwg();

public:
	////////////////////////��CAD�Ĳ�����������//////////////////////////////////////////
	// �ռ�ο�
	ISpatialReference* m_pSpRef;

	// �ж��Ƿ��ȡ������ͼ��, Ĭ�϶�ȡ
	BOOL m_IsReadInvisible;

	// �ж��Ƿ��ɢ�飬Ĭ��TRUEΪ��ɢ
	BOOL m_IsBreakBlock;

	//�Ƿ��ȡ������,Ĭ�ϲ���ȡ
	BOOL m_IsReadPolygon;

	//�պ����Ƿ�������
	BOOL m_IsLine2Polygon;

	//�Ƿ�ֱ�ӹҽ���չ����
	BOOL m_IsJoinXDataAttrs;

	//�Ƿ�������ݵĶ�λ��
	BOOL m_IsReadBlockPoint;

	//�Ƿ�����ע��ͼ��
	BOOL m_IsCreateAnnotation;

	//����ɢ���ģʽ  0������ɢ������  1������ɢ��CADͼ����
	short m_iUnbreakBlockMode;

	//�Ƿ�ѻ���ת��Ϊ�Ƕ�,Ĭ�ϰ�CAD�еĻ���ת��Ϊ�Ƕ�
	BOOL m_bConvertAngle;

	//ע��ͼ����ʾ����
	double m_dAnnoScale;

	//������
	CTextProgressCtrl* m_pProgressBar;

	//CAD��չ����ע��Ӧ����
	CStringList m_Regapps;

	//����ɢ�Ŀ���
	CStringList m_unExplodeBlocks;

public:
	//////////////////////ʵ�ֺ���////////////////////////////////////////////////
	// ��Field���ֵ
	void AddAttributes(LPCTSTR csFieldName, LPCTSTR csFieldValue, IFeatureBuffer*& pFeatureBuffer);

	// ��ӻ������� ����ɫ����ȣ�
	HRESULT AddBaseAttributes(OdDbEntity* pEnt, LPCTSTR strEnType, IFeatureBuffer*& pFeatureBuffer);

	// ��ʼ��insert����
	//void IniBlockAttributes(IFeatureBuffer* pFeatureBuffer);

	// ��ʼ������
	void CleanFeatureBuffer(IFeatureBuffer* pFeatureBuffer);

	// ��ʼ������featurebuffer
	void CleanAllFeatureBuffers();

	// ��ʼ��insert������featurebuffer
	//void BlockIniAttributes();

	void Bspline(int n, int t, DwgPoint* control, DwgPoint* output, int num_output);

	// ��ô�����Ϣ
	CString CatchErrorInfo();

	//����־�ļ�
	//void OpenLogFile();

	ITextElement* MakeTextElementByStyle(CString strText, double dblAngle,
		double dblHeight, double dblX,
		double dblY, double ReferenceScale, esriTextHorizontalAlignment horizAlign, esriTextVerticalAlignment vertAlign);

	//��ʼ����������¼��
	void InitRenameLayers(ITable* pRenameTable);

	//����ע��ʵ��
	void InsertAnnoFeature(OdRxObject* pEnt);

	//��������ʵ��
	void InsertDwgAttribFeature(OdRxObject* pEnt);

	//д��־�ļ�
	void WriteLog(CString sLog);

	//�ж����Ӹ��ӹ����ֶ�
	BOOL CompareCodes(IFeatureBuffer*& pFeatureBuffer);

	// �ͷŽӿڶ���
	void ReleaseAOs(void);

	//��ʼ���ӿ�
	void InitAOPointers(void);

	//�����չ�����ֶ�
	void AddExtraFields(CStringList* pRegapps);

public:

	//ע��Ҫ������
	IFontDisp* m_pAnnoTextFont;

	CString m_strDwgName;

	long m_lEntityNum;

	long m_lUnReadEntityNum;

	// ��¼ insert���͸���
	long m_lBlockNum;

	// ��¼insert���͵�ͷ��Ϣ
	// �ж��Ƿ��ǵ�һ��insert
	long m_bn;	

	// ��¼��һ��insert�����֡��㡢��ɫ������
	CString m_szblockname, m_szBlockLayer, m_szBlockColor, m_szBlockLT;

	//Ŀ��⹤���ռ�
	IWorkspace* m_pTargetWS;

	//����������ı�FeatureClass
	IFeatureClass* m_pFeatClassPoint;
	IFeatureClass* m_pFeatClassLine;
	IFeatureClass* m_pFeatClassPolygon;
	//ע��ͼ��
	IFeatureClass* m_pAnnoFtCls;
	//�ı���
	IFeatureClass* m_pFeatClassText;

	//��չ�������ݱ�
	ITable* m_pExtendTable; 

	// ������Ӧ��cursor��buffer
	IFeatureCursor* m_pPointFeatureCursor;
	IFeatureCursor* m_pLineFeatureCursor;
	IFeatureCursor* m_pPolygonFeatureCursor;
	IFeatureCursor* m_pAnnoFeatureCursor;
	IFeatureCursor* m_pTextFeatureCursor;
	IEsriCursor* m_pExtentTableRowCursor;


	
	IFeatureBuffer* m_pPointFeatureBuffer;
	IFeatureBuffer* m_pLineFeatureBuffer;
	IFeatureBuffer* m_pPolygonFeatureBuffer;
	IFeatureBuffer* m_pAnnoFeatureBuffer;
	IFeatureBuffer* m_pTextFeatureBuffer;
	IRowBuffer* m_pExtentTableRowBuffer;
	
	// table��
	CComVariant m_vID;
	CComVariant m_TableId;
	// д�����ݿ�ļ����
	long m_StepNum;
	CString m_sEntityHandle;

	//��־�ļ�����·��
	CString m_sLogFilePath;
	

protected:

	//����Ŀ��Ҫ����
	BOOL CreateTargetAllFeatureClass();

	//ɾ���Ѵ��ڵ�Ҫ����
	void CheckDeleteFtCls(IFeatureWorkspace* pFtWS, CString sFtClsName);

	// ����������ı�Field
	HRESULT CreateDwgPointFields(ISpatialReference* ipSRef, IFields** ppfields);
	HRESULT CreateDwgLineFields(ISpatialReference* ipSRef, IFields** ppfields);
	HRESULT CreateDwgPolygonFields(ISpatialReference* ipSRef, IFields** ppfields);
	HRESULT CreateDwgTextPointFields(ISpatialReference* ipSRef, IFields** ppfields);
	HRESULT CreateDwgAnnotationFields(ISpatialReference* ipSRef, IFields** ppfields);


	// ����ExtendTable��
	HRESULT CreateExtendTable(IFeatureWorkspace* pFeatWorkspace, BSTR bstrName, ITable** pTable);	
	// ����FTC
	HRESULT CreateDatasetFeatureClass(IFeatureWorkspace* pFWorkspace, IFeatureDataset* pFDS, IFields* pFields, BSTR bstrName, esriFeatureType featType, IFeatureClass*& ppFeatureClass);
	
	//��FeatureBuffer��ȡ�����ֶε�ֵ
	CString GetFeatureBufferFieldValue(IFeatureBuffer*& pFeatureBuffer, CString sFieldName);

	//�����չ����ֵ
	BOOL PutExtendAttribsValue(IFeatureBuffer*& pFtBuf, OdResBuf* xIter);
	
	// �Ƿ����FeatureClassҪ���÷�Χ
	BOOL IsResetDomain(IFeatureWorkspace* pFWorkspace, CString szFCName);
	
	// ���ô���FeatureClass��Χ
	void ResetDomain(IFeatureWorkspace* pFWorkspace, CString szFCName, ISpatialReference* ipSRef);
	
	// ��FeatureClass�õ�GeotryDef
	void GetGeometryDef(IFeatureClass* pClass, IGeometryDef** pDef);

	// spline���㷨
	void ComputeIntervals(int* u, int n, int t);
	double Blend(int k, int t, int* u, double v);
	void ComputePoint(int* u, int n, int t, double v, DwgPoint* control, DwgPoint* output);
	void ReleaseFeatureBuffer(IFeatureBufferPtr& pFeatureBuffer);

	HRESULT EndLoadOnlyMode(IFeatureClass*& pTargetClass);
	HRESULT BeginLoadOnlyMode(IFeatureClass*& pTargetClass);

	//����ע��ͼ��
	IFeatureClass* CreateAnnoFtCls(IWorkspace* pWS, CString sAnnoName, IFields* pFields);

	// �ͷŽӿ�ָ��
	int ReleasePointer(IUnknown*& pInterface);

private:
	//  ��dwg��ص�
	// ��dwgͷ��Ϣ
	void ReadHeader(OdDbDatabase* pDb);

	// ��dwgtable��Ϣ
	void ReadSymbolTable(OdDbObjectId tableId);

	//��dwg����Ϣ
	void ReadLayers(OdDbDatabase* pDb);

	//��dwg��չ��Ϣ
	void ReadExtendAttribs(OdResBuf* xIter, CString sEntityHandle);

	// ��ÿ��ʵ��
	void ReadEntity(OdDbObjectId id);

	// ������Ϣ����������ʵ�壩
	void ReadBlock(OdDbDatabase* pDb);

	// ��dwg�ļ����������п���Ϣ��
	//��־��¼��
	CLogRecorder* m_pLogRec;

	//������CADͼ��
	CStringList m_UnReadLayers;

	//�Ƿ��Ѿ�����Ŀ��Ҫ����
	BOOL m_bFinishedCreateFtCls;

};	
#endif
