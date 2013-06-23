// XDwgWriter.h: interface for the XDwgWriter class.
//
//////////////////////////////////////////////////////////////////////


#ifndef AFX_XDWGWriter_H____INCLUDED_
#define AFX_XDWGWriter_H____INCLUDED_

#include "SharedUsed.h"


//��չ����Map
//��չ����ע��Ӧ������+���ݿ��е���չ�����ֶ�����
typedef CTypedPtrMap<CMapStringToPtr, CString, CStringList*> XDataAttrLists;



class XDwgWriter
{
public:
    XDwgWriter();
    virtual ~XDwgWriter();


	//���ò�������־�ļ�
	void PutLogFilePath(CString sLogFile);

    //ָ�����ɵ�DWG�ļ�·������
    BOOL PrepareOutPut(LPCTSTR szCadOutFile);

    //��Feature Classд��DWG�ļ�, pExtraAttribs:������չ�������ƺͣ���Ӧ���ֶ�
    void FeatureClass2Dwgfile(IFeatureClass* pFeatureClass, XDataAttrLists* pExtraAttribs = NULL);

    //��SelectionSetд��DWG�ļ�, pExtraAttribs:������չ�������ƺͣ���Ӧ���ֶ�
    void SelectionSet2Dwgfile(ISelectionSet* pSelectionSet, XDataAttrLists* pExtraAttribs = NULL);


    //��FeatureDatasetд��DWG�ļ�, ��д����չ����
    void FeatureDataset2DwgFile(IFeatureDataset* pFeatureDataset);

    //��Cursorд��DWG�ļ�, ��д����չ����
    void Cursor2Dwgfile(IFeatureCursor* pFeatureCursor,
                        CString sFeatureClassName,
                        long lFeatureNums);

    //ָ��ģ���ļ�
    BOOL DoModalGetOutDwgSetting();

    //���д������
    void FlushOutPut();

    //������־
    void SaveLogFile(CString sFilePath);

    //�����Χ
    IEnvelopePtr m_ipOutExtent;
    // dwg��ģ���ļ�
    CString m_szCadTempFile;
    //����־�ļ�
    void OpenLogFile();
    // �Ƿ������չ���Բ���
    BOOL m_isOutXData;

    //��ǰ���Ҫ��������
    CString m_sFeatureClassName;


    //�Ƿ�ѽǶ�ת��Ϊ����, Ĭ����
    BOOL m_bConvertAngle;

    //д��־�ļ�
    void WriteLog(CString sLog);

	CTextProgressCtrl* m_pProgressBar;

	//д�뵽CAD��ͼ������
	CString m_sDwgLayer;



	//���Զ����ֶ�
	CString m_sCompareField;

	//���Զ��ձ�
	ITable* m_pCompareTable;


	//����FeatureClassӦ��������Ҫд�����չ�����ֶ�
	XDataAttrLists* m_pXDataAttrLists;


private:

	//�Ƿ�ɹ���ʼ��
	BOOL m_bInit;

    // ģ���ļ���׺DXF��DWG
    CString m_FileExt;
    // Dwg����ļ���
    char m_destfile[265];
    // ��־�ļ�
    CStringList m_LogInfo;
    CString GetFieldValueAsString(CComVariant& var);
    BOOL GetAttribute(IFeature* pFeature, CString sAttrName, CString& sAttrValue);

    OdDbObjectId addLayer(OdDbDatabase* pDb, LPCTSTR layerName);
    BOOL FindLayerByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId);
    BOOL FindLinttypeByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId);
    BOOL FindBlockByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId);
    BOOL FindTextStyleByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId);

    void WriteInsert(IFeature* pFeature,
                     OdDbDatabase* pDb,
                     const OdDbObjectId& layerId,
                     const OdDbObjectId& blockId);

    void WritePoint(IFeature* pFeature,
                    OdDbDatabase* pDb,
                    const OdDbObjectId& layerId,
                    const OdDbObjectId& styleId);


	//дע������
    void WriteAnnotation(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId);


    void WriteText(IFeature* pFeature,
                   OdDbDatabase* pDb,
                   const OdDbObjectId& layerId,
                   const OdDbObjectId& styleId);

    void WriteGeometryPoint(IFeature* pFeature,
                            OdDbDatabase* pDb,
                            const OdDbObjectId& layerId,
                            const OdDbObjectId& styleId);
    void WriteLine(IFeature* pFeature,
                   OdDbDatabase* pDb,
                   const OdDbObjectId& layerId,
                   const OdDbObjectId& styleId);
    void WriteCircularArc(IFeature* pFeature,
                          IGeometry* pShape,
                          OdDbDatabase* pDb,
                          const OdDbObjectId& layerId,
                          const OdDbObjectId& styleId);
    void WriteEllipticArc(IFeature* pFeature,
                          OdDbDatabase* pDb,
                          const OdDbObjectId& layerId,
                          const OdDbObjectId& styleId);
    void WriteMultipoint(IFeature* pFeature,
                         OdDbDatabase* pDb,
                         const OdDbObjectId& layerId,
                         const OdDbObjectId& styleId);
    void WriteBezier3Curve(IFeature* pFeature,
                           OdDbDatabase* pDb,
                           const OdDbObjectId& layerId,
                           const OdDbObjectId& styleId);
    void WritePath(IFeature* pFeature,
                   IGeometry* pShape,
                   OdDbDatabase* pDb,
                   const OdDbObjectId& layerId,
                   const OdDbObjectId& styleId,
                   int isClosed);
    void WriteBag(IFeature* pFeature,
                  OdDbDatabase* pDb,
                  const OdDbObjectId& layerId,
                  const OdDbObjectId& styleId);
    void WritePolygon(IFeature* pFeature,
                      OdDbDatabase* pDb,
                      const OdDbObjectId& layerId,
                      const OdDbObjectId& styleId);
    void WriteRing(IFeature* pFeature,
                   OdDbDatabase* pDb,
                   const OdDbObjectId& layerId,
                   const OdDbObjectId& styleId);

    void WritePolyline(IFeature* pFeature,
                       OdDbDatabase* pDb,
                       const OdDbObjectId& layerId,
                       const OdDbObjectId& styleId,
                       BOOL isClosed = FALSE);

    OdResBuf* appendXDataPair(OdResBuf* pCurr, int code);
    void addExtraAttribs(IFeature* pFeature, OdDbEntity* pEntity);
    BOOL GetDefaultTempFile();

private:
    // ��ô�����Ϣ
    CString CatchErrorInfo();
	OdDbHatch::HatchPatternType GetHatchPatternName(CString csText, CString& csPatternName);

protected:
    //��������DWG�ļ���Ҫע�����չ����Ӧ������
    CStringList m_registeredAppNames;

    //ע����չ����Ӧ������
    void RegAppName(OdDbDatabase* pDb, CString sRegAppName);

    //�����չ�������Ͷ�Ӧ���ֶ�MAP
    void ClearXDataAttrLists();

	//��־��¼��
	CLogRecorder* m_pLogRec;

	//��־�ļ�·��
	CString m_sLogFilePath;

public:
	BOOL m_bWidthCompareField2;
	CString m_csConfigField2;
	CString m_csGdbField2;
};

#endif