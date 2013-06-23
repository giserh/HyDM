// XDwgWriter.cpp: implementation of the XDwgWriter class.
//
//////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <io.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "db.h"
#include "XDwgDirectWriter.h"
#include "atlbase.h"
#include "ExSystemServices.h"
#include "ExHostAppServices.h"
#include "DynamicLinker.h"
#include "RxDynamicModule.h"



//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

class DwgWriteServices : public ExSystemServices, public ExHostAppServices
{
protected:
    ODRX_USING_HEAP_OPERATORS(ExSystemServices);

public:
	virtual OdString findFile(const char* pcFilename, OdDbDatabase* pDb = NULL, OdDbHostAppServices::FindFileHint hint = kDefault)
	{
		TCHAR szFilePathName[MAX_PATH] = _T("\0");
		::GetModuleFileName(NULL, szFilePathName, MAX_PATH);
		CString csPathname = szFilePathName;
		int iPath = csPathname.ReverseFind('\\');
		CString csSpath = csPathname.Left(iPath) + _T("\\support\\") + CString(pcFilename);
		if (_access(csSpath, 0) != -1)
		{
			return OdString(csSpath);
		}
		return ExHostAppServices::findFile(pcFilename);
	}
};

OdStaticRxObject<DwgWriteServices> svcs;


XDwgWriter::XDwgWriter()
{
    m_isOutXData = TRUE;
    m_pXDataAttrLists = NULL;

    m_bConvertAngle = TRUE;

    odInitialize(&svcs);

    m_pProgressBar = NULL;

    m_pLogRec = NULL;

    m_sDwgLayer = "";

    //���Զ����ֶ�
    m_sCompareField = "";

    //���Զ��ձ�
    m_pCompareTable = NULL;

    m_bInit = FALSE;

	m_bWidthCompareField2 = FALSE;
	m_csConfigField2 = _T("");
	m_csGdbField2 = _T("");
}

XDwgWriter::~XDwgWriter()
{
    odUninitialize();

    if (m_pLogRec != NULL)
    {
        delete m_pLogRec;
    }
}

/********************************************************************
��Ҫ���� : ������־���·��
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
void XDwgWriter::PutLogFilePath(CString sLogFile)
{
    m_pLogRec = new CLogRecorder(sLogFile);
    m_sLogFilePath = sLogFile;
}


//////////////////////////////////////////////////////////////////////////
OdDbObjectId XDwgWriter::addLayer(OdDbDatabase* pDb, LPCTSTR layerName)
{
    OdDbLayerTablePtr pLayers;
    OdDbLayerTableRecordPtr pLayer;
    OdDbObjectId id;

    pLayers = pDb->getLayerTableId().safeOpenObject(OdDb::kForWrite);
    pLayer = OdDbLayerTableRecord::createObject();

    pLayer->setName((OdString) layerName);
    id = pLayers->add(pLayer);

    return id;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::FindLayerByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId)
{
    OdDbLayerTablePtr pLayers;
    pLayers = pDb->getLayerTableId().safeOpenObject(OdDb::kForWrite);
    *ObId = pLayers->getAt(name);
    if (*ObId)
        return TRUE;
    else
        return FALSE;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::FindLinttypeByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId)
{
    OdDbLinetypeTablePtr pTable;
    pTable = pDb->getLinetypeTableId().safeOpenObject(OdDb::kForWrite);
    *ObId = pTable->getAt(name);
    if (*ObId)
        return TRUE;
    else
        return FALSE;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::FindTextStyleByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId)
{
    OdDbTextStyleTablePtr pTable;
    pTable = pDb->getTextStyleTableId().safeOpenObject(OdDb::kForWrite);
    *ObId = pTable->getAt(name);
    if (*ObId)
        return TRUE;
    else
        return FALSE;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::FindBlockByName(OdDbDatabase* pDb, char* name, OdDbObjectId* ObId)
{
    OdDbBlockTablePtr pTable;
    pTable = pDb->getBlockTableId().safeOpenObject(OdDb::kForWrite);
    *ObId = pTable->getAt(name);
    if (*ObId)
        return TRUE;
    else
        return FALSE;
}


//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteGeometryPoint(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    // Field�д��� TextString �ֶ������ݲ�Ϊ����Ϊ�� text ���ͣ�����ΪPoint
    // Field�д��� Blockname   �ֶ������ݲ�Ϊ����Ϊ�� insert ���ͣ�����ΪPoint
    CString sTextString, csBlockName, csMsg;
    if (GetAttribute(pFeature, "TextString", sTextString))
    {
        //WriteLog("����ı�!");
        WriteText(pFeature, pDb, layerId, styleId);
    }
    else if (GetAttribute(pFeature, "BlockName", csBlockName))
    {
		//csMsg.Format("���Ϊ��:%s", csBlockName);
        //WriteLog(csMsg);
        // ���dwgģ���ļ���û���ҵ��� blockname��תΪ��
        OdDbObjectId blockid;
        char blockname[255];
        strcpy_s(blockname, csBlockName);
        if (FindBlockByName(pDb, blockname, &blockid))
        {
            WriteInsert(pFeature, pDb, layerId, blockid);
        }
        else
        {
			csMsg.Format("�Ҳ�������:%s,ֻ�����Ϊ��.", csBlockName);
            WriteLog(csMsg);
            WritePoint(pFeature, pDb, layerId, styleId);
        }
    }
    else
    {
        //WriteLog("�����!");
        WritePoint(pFeature, pDb, layerId, styleId);
    }
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteMultipoint(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    IPointCollectionPtr pPointColl;
    pPointColl = pFeature;
    long pointNum;
    pPointColl->get_PointCount(&pointNum);
    double ptx, pty, ptz;
    IPointPtr pPoint(CLSID_Point);
    for (int i = 0; i < pointNum; i++)
    {
        pPointColl->get_Point(i, &pPoint);
        pPoint->get_X(&ptx);
        pPoint->get_Y(&pty);
        pPoint->get_Z(&ptz);
        WriteGeometryPoint(pFeature, pDb, layerId, styleId);
    }
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteCircularArc(IFeature* pFeature, IGeometry* pShape, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString cstemp;
    if (!pShape)
        return;
    IPointPtr pPoint(CLSID_Point);
    ICircularArcPtr pCircularArc(CLSID_CircularArc);
    pCircularArc = pShape;

    OdDbArcPtr pArc = OdDbArc::createObject();

    OdDbObjectId objId = pDb->getModelSpaceId(); //�õ�obj id

    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pArc);

    OdGePoint3d center;
    double arcradius, stang, endang;
    pCircularArc->get_CenterPoint(&pPoint);
    pPoint->get_X(&center.x);
    pPoint->get_Y(&center.y);
    pPoint->get_Z(&center.z);
    pCircularArc->get_Radius(&arcradius);
    pCircularArc->get_FromAngle(&stang);
    pCircularArc->get_ToAngle(&endang);
    if (abs(stang - endang) < 0.000000001)
        endang += 2 * PI;
    pArc->setCenter(center);
    pArc->setRadius(arcradius);
    pArc->setStartAngle(stang);
    pArc->setEndAngle(endang);

    pArc->setLinetype(styleId, false);
    pArc->setLayer(layerId, false);
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pArc->setColorIndex(entcolor);
    double thickns;
    if (!GetAttribute(pFeature, "Thickness", cstemp))
        thickns = 0;
    else
        thickns = atof(cstemp);
    pArc->setThickness(thickns);

    addExtraAttribs(pFeature, pArc);
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteEllipticArc(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString cstemp;
    IPointPtr pPoint(CLSID_Point);
    IEllipticArcPtr pElpArc(CLSID_EllipticArc);
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pElpArc = pShape;

    OdDbEllipsePtr pEllipse = OdDbEllipse::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pEllipse);

    pEllipse->setLinetype(styleId, false);
    pEllipse->setLayer(layerId, false);
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pEllipse->setColorIndex(entcolor);

    double startangle, endangle, CentralAngle, rotationAngle, semiMajor, minorMajorRatio;
    pElpArc->QueryCoordsByAngle(VARIANT_TRUE, pPoint, &startangle, &CentralAngle, &rotationAngle, &semiMajor, &minorMajorRatio);
    OdGePoint3d center;
    pPoint->get_X(&center.x);
    pPoint->get_Y(&center.y);
    center.z = 0;
    pEllipse->setCenter(center);
    pEllipse->setStartAngle(startangle);
    endangle = CentralAngle + startangle;
    pEllipse->setEndAngle(endangle);
    pEllipse->setRadiusRatio(minorMajorRatio);

    //�����չ����
    addExtraAttribs(pFeature, pEllipse);
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WritePath(IFeature* pFeature, IGeometry* pShape, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId, int isClosed)
{
    // lwp�㼯,bulge����
    IPointCollectionPtr lwplinePointColl(CLSID_Polyline);
    IPointCollectionPtr lwpbulgeColl(CLSID_Polyline);
    IPointPtr pPoint(CLSID_Point);
    ISegmentCollectionPtr ipsegcoll(pShape);
    long segNums;
    ipsegcoll->get_SegmentCount(&segNums);
    ISegmentPtr pSeg;
    esriGeometryType segtype;
    double bulge;
    HRESULT hr;
    lwplinePointColl = pShape;
    for (int i = 0; i < segNums; i++)
    {
        ipsegcoll->get_Segment(i, &pSeg);
        pSeg->get_GeometryType(&segtype);
        if (segtype == esriGeometryCircularArc)
        {
            ICircularArcPtr pCircularArc = pSeg;
            VARIANT_BOOL IsClosed;

            pCircularArc->get_IsClosed(&IsClosed);
            // ��Բ����Ҫ����Բ������lwp
            if (IsClosed)
            {
                bulge = PI / 2;
                WriteCircularArc(pFeature, pCircularArc, pDb, layerId, styleId);
                return;
            }
            else
            {
                hr = pCircularArc->get_CentralAngle(&bulge);
                if (FAILED(hr))
                {
                    WriteLog(CatchErrorInfo());
                    bulge = 0;
                }
                bulge = tan(bulge / 4);
            }
            pPoint->PutCoords(bulge, 0);
            lwpbulgeColl->AddPoint(pPoint, NULL, NULL);
        }
        else if (segtype == esriGeometryEllipticArc)
        {
        }
        else if (segtype == esriGeometryBezier3Curve)
        {
        }
        else
        {
            pPoint->PutCoords(0.0, 0);
            lwpbulgeColl->AddPoint(pPoint, NULL, NULL);
        }
    }
    long pointNum;
    lwplinePointColl->get_PointCount(&pointNum);
    if (pointNum > 1)
    {
        OdDbPolylinePtr pPoly = OdDbPolyline::createObject();
        OdDbObjectId objId = pDb->getModelSpaceId();
        OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
        OdDbObjectId oidPoly = pBlock->appendOdDbEntity(pPoly);
        OdGePoint2d p2d;
        for (int n = 0; n < pointNum; n++)
        {
            lwplinePointColl->get_Point(n, &pPoint);
            pPoint->get_X(&p2d.x);
            pPoint->get_Y(&p2d.y);
            lwpbulgeColl->get_Point(n, &pPoint);
            pPoint->get_X(&bulge);
            if (bulge != 0)
                pPoly->addVertexAt(n, p2d, bulge);
            else
                pPoly->addVertexAt(n, p2d);
        }
        CString cstemp;
        pPoly->setLinetype(styleId, false);
        pPoly->setLayer(layerId, false);
        int entcolor;
        if (!GetAttribute(pFeature, "Color", cstemp))
            entcolor = 0;
        else
            entcolor = atoi(cstemp);
        if (entcolor < 0 || entcolor > 256)
            entcolor = 0;

        pPoly->setColorIndex(entcolor);

        double entElv;
        if (!GetAttribute(pFeature, "Elevation", cstemp))
            entElv = 0;
        else
            entElv = atof(cstemp);
        pPoly->setElevation(entElv);
        double entconstwidth;
        if (!GetAttribute(pFeature, "Width", cstemp))
            entconstwidth = 0;
        else
            entconstwidth = atof(cstemp);
        pPoly->setConstantWidth(entconstwidth);
        double thickns;
        if (!GetAttribute(pFeature, "Thickness", cstemp))
            thickns = 0;
        else
            thickns = atof(cstemp);
        pPoly->setThickness(thickns);
        if (isClosed)
		{
            pPoly->setClosed(TRUE);
		}

        //�����չ����
        addExtraAttribs(pFeature, pPoly);

		try
		{
			cstemp.Empty();
			if (isClosed && GetAttribute(pFeature, "HatchStyle", cstemp))
			{
				if (! cstemp.IsEmpty())
				{
					CString csPatternName = cstemp;
					OdDbHatch::HatchPatternType patType = GetHatchPatternName(cstemp, csPatternName);
					OdGeVector3d vNormal(0, 0, 1);
					OdDbHatchPtr pHatch = OdDbHatch::createObject();
					pHatch->setDatabaseDefaults(pBlock->database());
					OdDbObjectId oidHatch = pBlock->appendOdDbEntity(pHatch);
					pHatch->setColorIndex(entcolor);
					pHatch->setLayer(layerId);
					pHatch->setAssociative(false);
					pHatch->setPattern(patType, cstemp);
					if (cstemp.CompareNoCase(_T("solid")) != 0)
					{
						double dScale = 1.25;
						CString csScale = _T("");
						if (GetAttribute(pFeature, "TScale", csScale))
						{
							csScale.Trim();
							if (! csScale.IsEmpty())
							{
								dScale = _tstof(csScale);
								pHatch->setPatternScale(dScale);
							}
						}
					}
					pHatch->setHatchStyle(OdDbHatch::kNormal);
					pHatch->setNormal(vNormal);
					OdDbObjectIdArray arOids;
					arOids.append(oidPoly);
					pHatch->appendLoop(OdDbHatch::kDefault, arOids);
				}
			}
		}
		catch(OdError& err)
		{
			cstemp = svcs.getErrorDescription(err.code());
			WriteLog(_T(" �������ʧ��: ")+cstemp);
		}
    }
    else
    {
        WriteLog(" û��ȡ��������С��2,����!");
    }
}

// �������õ������ʽ�ж������ʽ�����ͺ�����
// ���磺 ��<###>����ʾ�û�����������ʽ
//        ��{###}����ʾ�Զ���������ʽ
//		  ��###����ʾԤ����������ʽ,��: SOLID,CROSS
OdDbHatch::HatchPatternType XDwgWriter::GetHatchPatternName(CString csText, CString& csPatternName)
{
	OdDbHatch::HatchPatternType patType = OdDbHatch::kPreDefined;

	int iLength = csText.GetLength();
	if ((csText[0] == '<') && (csText[iLength-1] == '>'))
	{
		patType = OdDbHatch::kUserDefined;
		csPatternName = csText.Mid(1, iLength - 2);
	}
	else if ((csText[0] == '{') && (csText[iLength-1] == '}'))
	{
		patType = OdDbHatch::kCustomDefined;
		csPatternName = csText.Mid(1, iLength - 2);
	}
	else
	{
		csPatternName = csText;
	}

	return patType;
}

//////////////////////////////////////////////////////////////////////////
OdResBuf* XDwgWriter::appendXDataPair(OdResBuf* pCurr, int code)
{
    pCurr->setNext(OdResBuf::newRb(code));
    return pCurr->next();
}

/************************************************************************
��Ҫ���� : ����ע��Ӧ�����ƺͶ�Ӧ����չ�����ֶ�д����չ����
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDwgWriter::addExtraAttribs(IFeature* pFeature, OdDbEntity* pEntity)
{
    if (m_pXDataAttrLists != NULL)
    {
        POSITION pos = m_pXDataAttrLists->GetStartPosition();
        while (pos)
        {
            CString sRegAppName;
            CStringList* pFieldsLst;
            m_pXDataAttrLists->GetNextAssoc(pos, sRegAppName, pFieldsLst);

            //Ӧ������
            OdResBufPtr xIter = OdResBuf::newRb(OdResBuf::kDxfRegAppName);
            OdResBufPtr pXRes = xIter;
            OdString odstr;
            odstr.format("%s", sRegAppName); //ע��Ӧ������
            pXRes->setString(odstr);

            POSITION pos = pFieldsLst->GetHeadPosition();
            while (pos != NULL)
            {
                CString sExtraField, sExtraValue;
                sExtraField = pFieldsLst->GetNext(pos);

                pXRes = appendXDataPair(pXRes, OdResBuf::kDxfXdAsciiString);

                if (!GetAttribute(pFeature, sExtraField, sExtraValue))
                {
                    pXRes->setString("");
                }
                else
                {
                    odstr.format("%s", sExtraValue); //��չ����
                    pXRes->setString(odstr);
                }
            }

            try
            {
                //д����չ����
                pEntity->setXData(xIter);
            }
            catch (OdError& err)
            {
                CString msg;
                msg.Format("д����չ���Գ����������%d", err.code());
                WriteLog(msg);
            }
        }
    }

}

void XDwgWriter::WritePolyline(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId, BOOL isClosed)
{
    IGeometryCollectionPtr pGeoCollection;
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pGeoCollection = pShape;
    long lParts;
    pGeoCollection->get_GeometryCount(&lParts);
    if (lParts > 0)
    {
        for (int i = 0; i < lParts; i++)
        {
            pGeoCollection->get_Geometry(i, &pShape);
            WritePath(pFeature, pShape, pDb, layerId, styleId, isClosed);
        }
    }
    else
    {
        WritePath(pFeature, pShape, pDb, layerId, styleId, isClosed);
    }
}

//////////////////////////////////////////////////////////////////////////
// A Ring is Closed Path that defines a Two Dimensional Area.
void XDwgWriter::WriteRing(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    WritePath(pFeature, pShape, pDb, layerId, styleId, 1);
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WritePolygon(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    IGeometryCollectionPtr pGeoCollection;
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pGeoCollection = pShape;
    long lParts;
    pGeoCollection->get_GeometryCount(&lParts);
    if (lParts > 0)
    {
        for (int i = 0; i < lParts; i++)
        {
            pGeoCollection->get_Geometry(i, &pShape);
            WritePath(pFeature, pShape, pDb, layerId, styleId, 1);
        }
    }
    else
    {
        WritePath(pFeature, pShape, pDb, layerId, styleId, 1);
    }
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteBag(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteBezier3Curve(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteLine(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString cstemp;
    OdDbLinePtr pLine;
    pLine = OdDbLine::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pLine);

    IPointPtr pPoint(CLSID_Point);

    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    ILinePtr pELine;
    OdGePoint3d start;
    OdGePoint3d end;
    pELine = pShape;
    pELine->get_FromPoint(&pPoint);
    pPoint->get_X(&start.x);
    pPoint->get_Y(&start.y);
    start.z = 0;
    pELine->get_ToPoint(&pPoint);
    pPoint->get_X(&end.x);
    pPoint->get_Y(&end.y);
    end.z = 0;
    pLine->setStartPoint(start);
    pLine->setEndPoint(end);

    pLine->setLinetype(styleId, false);
    pLine->setLayer(layerId, false);
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pLine->setColorIndex(entcolor);

    double thickns;
    if (!GetAttribute(pFeature, "Thickness", cstemp))
        thickns = 0;
    else
        thickns = atof(cstemp);
    pLine->setThickness(thickns);
    //�����չ����
    addExtraAttribs(pFeature, pLine);
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteInsert(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& blockId)
{
    CString cstemp;
    double rotang, xsc, ysc;
    short attribsfollow = 0;
    if (!GetAttribute(pFeature, "Angle", cstemp))
        rotang = 0;
    else
        rotang = atof(cstemp);
    if (!GetAttribute(pFeature, "Scale", cstemp))
        xsc = ysc = 1;
    else
    {
        if (atof(cstemp) == 0)
            xsc = ysc = 1;
        else
            xsc = ysc = atof(cstemp);
    }
    OdDbBlockReferencePtr pBlkRef = OdDbBlockReference::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pBlkRef);
    pBlkRef->setBlockTableRecord(blockId);
    pBlkRef->setScaleFactors(OdGeScale3d(xsc, ysc, 1.0));
    pBlkRef->setRotation(rotang);

    IPointPtr pPoint(CLSID_Point);
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pPoint = pShape;
    double insertx, inserty, insertz;
    pPoint->get_X(&insertx);
    pPoint->get_Y(&inserty);
    pPoint->get_Z(&insertz);
    pBlkRef->setPosition(OdGePoint3d(insertx, inserty, insertz));

    pBlkRef->setLayer(layerId, false);
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pBlkRef->setColorIndex(entcolor);
    //�����չ����
    addExtraAttribs(pFeature, pBlkRef);
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WritePoint(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString cstemp;

    OdDbPointPtr pDbPoint = OdDbPoint::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pDbPoint);

    IPointPtr pPoint(CLSID_Point);
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pPoint = pShape;
    double insertx, inserty, insertz;
    pPoint->get_X(&insertx);
    pPoint->get_Y(&inserty);
    pPoint->get_Z(&insertz);
    pDbPoint->setPosition(OdGePoint3d(insertx, inserty, insertz));

    pDbPoint->setLinetype(styleId);
    pDbPoint->setLayer(layerId, false);
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pDbPoint->setColorIndex(entcolor);
    double thickns;
    if (!GetAttribute(pFeature, "Thickness", cstemp))
        thickns = 0;
    else
        thickns = atof(cstemp);
    pDbPoint->setThickness(thickns);
    //�����չ����
    addExtraAttribs(pFeature, pDbPoint);
}

/********************************************************************
��Ҫ���� : дע������
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
void XDwgWriter::WriteAnnotation(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString sTextString = "";
	OdGePoint3d textPos;

    IAnnotationFeaturePtr pTarAnnoFeat = pFeature;
    if (pTarAnnoFeat != NULL)
    {
        IElementPtr pElement = NULL;
        HRESULT hr = pTarAnnoFeat->get_Annotation(&pElement);

		//�õ�λ�õ�
		if(pElement != NULL)
		{
			IGeometryPtr pGeoPt;
			pElement->get_Geometry(&pGeoPt);
			IPointPtr pPoint = (IPointPtr)pGeoPt;
			pPoint->get_X(&textPos.x);
			pPoint->get_Y(&textPos.y);
			textPos.z = 0;
		}
		else
		{
			IGeometryPtr pTxtShape;
			IEnvelopePtr pTxtRect;
			pFeature->get_Shape(&pTxtShape);
			pTxtShape->get_Envelope(&pTxtRect);

			IPointPtr pTxtPosPoint(CLSID_Point);
			pTxtRect->get_LowerLeft(&pTxtPosPoint);
			pTxtPosPoint->get_X(&textPos.x);
			pTxtPosPoint->get_Y(&textPos.y);
			textPos.z = 0;
		}

		//�ı�����
        ITextElementPtr pTextElement = pElement;
        if (pTextElement != NULL)
        {
            CComBSTR bsText;
            pTextElement->get_Text(&bsText);
            sTextString = bsText;
        }
        else
        {
            IGroupElementPtr pGroupElement = pElement;
            if (pGroupElement != NULL)
            {
                long numElements;
                pGroupElement->get_ElementCount(&numElements);
                for (int i = 0; i < numElements; i++)
                {
                    IElementPtr pChildElement = NULL;
                    pGroupElement->get_Element(i, &pChildElement);
                    ITextElementPtr pChildTextElmt = pChildElement;
                    if (pChildTextElmt != NULL)
                    {
                        CComBSTR bsText;
                        pChildTextElmt->get_Text(&bsText);
                        CString sText = bsText;
                        sTextString = sTextString + sText;
                    }
                }
            }
        }
    }

    if (sTextString.IsEmpty())
    {
        if (!GetAttribute(pFeature, "TextString", sTextString))
            return;
        if (sTextString.GetLength() > 255)
        {
            CString sLog;
            sLog.Format("�ַ�����255���ֽ�:%s", sTextString);
            WriteLog(sLog);
            return;
        }
    }

    OdDbTextPtr pText = OdDbText::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pText);

    CString sAttrVal;
    double rotang, height, widthfactor, oblique;
    if (!GetAttribute(pFeature, "Angle", sAttrVal))
        rotang = 0;
    else
        rotang = atof(sAttrVal);
    if (!GetAttribute(pFeature, "Height", sAttrVal))
        height = 1;
    else
    {
        if (atof(sAttrVal) == 0)
            height = 1;
        else
            height = atof(sAttrVal);
    }
    if (!GetAttribute(pFeature, "WidthFactor", sAttrVal))
        widthfactor = 1;
    else
    {
        if (atof(sAttrVal) == 0)
            widthfactor = 1;
        else
            widthfactor = atof(sAttrVal);
    }
    if (!GetAttribute(pFeature, "Oblique", sAttrVal))
        oblique = 0;
    else
        oblique = atof(sAttrVal);
    int hMode, vMode;
    if (!GetAttribute(pFeature, "HorizontalAlignment", sAttrVal))
        hMode = 0;
    else
        hMode = atoi(sAttrVal);
    if (!GetAttribute(pFeature, "VerticalAlignment", sAttrVal))
        vMode = 0;
    else
        vMode = atoi(sAttrVal);

    double dThickness;
    if (!GetAttribute(pFeature, "Thickness", sAttrVal))
        dThickness = 0;
    else
        dThickness = atof(sAttrVal);

    // ��������
    if (!GetAttribute(pFeature, "TextStyle", sAttrVal))
        sAttrVal = "STANDARD";
    char textstyle[255];
    strcpy_s(textstyle, sAttrVal);
    OdDbObjectId ObTextStyleId;
    if (!FindTextStyleByName(pDb, textstyle, &ObTextStyleId))
    {
        FindTextStyleByName(pDb, "STANDARD", &ObTextStyleId);
    }

    pText->recordGraphicsModified(false);
    
    pText->setTextStyle(ObTextStyleId);
    pText->setRotation(rotang);
    pText->setOblique(oblique);
    pText->setHeight(height);
    pText->setWidthFactor(widthfactor);
    pText->setTextString(sTextString);
    pText->setThickness(dThickness);


	//ˮƽ���뷽ʽ
	OdDb::TextHorzMode horz;
	switch (hMode)
	{
	case esriTHALeft: horz = OdDb::kTextLeft;
		break;
	case esriTHACenter: horz = OdDb::kTextCenter;
		break;
	case esriTHARight: horz = OdDb::kTextRight;
		break;
	case esriTHAFull: horz = OdDb::kTextFit;
		break;
	default:
		horz = OdDb::kTextLeft;
	}
	pText->setHorizontalMode(horz);


	//��ֱ���뷽ʽ
	OdDb::TextVertMode vert;
	switch (vMode)
	{
	case esriTVATop: vert = OdDb::kTextTop;
		break;
	case esriTVACenter: vert = OdDb::kTextVertMid;
		break;
	case esriTVABaseline: vert = OdDb::kTextBase;
		break;
	case esriTVABottom: vert = OdDb::kTextBottom;
		break;
	default:
		vert = OdDb::kTextBase;
	}
	pText->setVerticalMode(vert);


	//λ�õ�
	pText->setPosition(textPos);


	///////�����////////////
	CString cstemp;
	OdGePoint3d alignPt;
	if (GetAttribute(pFeature, "AlignPtX", cstemp))
	{
		alignPt.x = atof(cstemp);
	}
	else
	{
		alignPt.x = textPos.x;
	}
	if (GetAttribute(pFeature, "AlignPtY", cstemp))
	{
		alignPt.y = atof(cstemp);
	}
	else
	{
		alignPt.y = textPos.y;
	}
	pText->setAlignmentPoint(alignPt);

    pText->setLayer(layerId, false);


    //����ע�ǵ���ɫ���� 
    int entcolor;
    if (!GetAttribute(pFeature, "Color", sAttrVal))
        entcolor = 0;
    else
        entcolor = atoi(sAttrVal);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pText->setColorIndex(entcolor);


    //�����չ����
    addExtraAttribs(pFeature, pText);
}


//////////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteText(IFeature* pFeature, OdDbDatabase* pDb, const OdDbObjectId& layerId, const OdDbObjectId& styleId)
{
    CString sTextString;
    CString cstemp;
    if (!GetAttribute(pFeature, "TextString", sTextString))
        return;
    if (sTextString.GetLength() > 255)
    {
        cstemp.Format("�ַ�����255���ֽ�:%s", sTextString);
        WriteLog(cstemp);
        return;
    }
    OdDbTextPtr pText = OdDbText::createObject();
    OdDbObjectId objId = pDb->getModelSpaceId();
    OdDbBlockTableRecordPtr pBlock = objId.safeOpenObject(OdDb::kForWrite);
    pBlock->appendOdDbEntity(pText);

    IPointPtr pPoint(CLSID_Point);
    IGeometryPtr pShape;
    pFeature->get_Shape(&pShape);
    pPoint = pShape;
    OdGePoint3d position;
    pPoint->get_X(&position.x);
    pPoint->get_Y(&position.y);
    position.z = 0;

    double rotang, height, widthfactor, oblique;
    if (!GetAttribute(pFeature, "Angle", cstemp))
        rotang = 0;
    else
        rotang = atof(cstemp);
    if (!GetAttribute(pFeature, "Height", cstemp))
        height = 1;
    else
    {
        if (atof(cstemp) == 0)
            height = 1;
        else
            height = atof(cstemp);
    }
    if (!GetAttribute(pFeature, "WidthFactor", cstemp))
        widthfactor = 1;
    else
    {
        if (atof(cstemp) == 0)
            widthfactor = 1;
        else
            widthfactor = atof(cstemp);
    }
    if (!GetAttribute(pFeature, "Oblique", cstemp))
        oblique = 0;
    else
        oblique = atof(cstemp);
    int hMode, vMode;
    if (!GetAttribute(pFeature, "HzMode", cstemp))
        hMode = 0;
    else
        hMode = atoi(cstemp);
    if (!GetAttribute(pFeature, "vMode", cstemp))
        vMode = 0;
    else
        vMode = atoi(cstemp);

    double thickns;
    if (!GetAttribute(pFeature, "Thickness", cstemp))
        thickns = 0;
    else
        thickns = atof(cstemp);

    // ��������
    if (!GetAttribute(pFeature, "ShapeName", cstemp))
        cstemp = "STANDARD";
    char shapename[255];
    strcpy_s(shapename, cstemp);
    OdDbObjectId ObTextStyleId;
    if (FindTextStyleByName(pDb, shapename, &ObTextStyleId))
    {
        OdGiTextStyle style;
        giFromDbTextStyle(ObTextStyleId, style);
        char stylename[255];
        if (GetAttribute(pFeature, "ShapeFilename", cstemp))
        {
            strcpy_s(stylename, cstemp);
            style.setFileName(stylename);
        }
        if (GetAttribute(pFeature, "BigFontname", cstemp))
        {
            strcpy_s(stylename, cstemp);
            style.setBigFontFileName(stylename);
        }
    }
    else
        FindTextStyleByName(pDb, "STANDARD", &ObTextStyleId);

    pText->recordGraphicsModified(false);
    pText->setPosition(position);
    pText->setTextStyle(ObTextStyleId);
    pText->setRotation(rotang);
    pText->setOblique(oblique);
    pText->setHeight(height);
    pText->setWidthFactor(widthfactor);
    pText->setTextString(sTextString);
    pText->setThickness(thickns);


	//ˮƽ���뷽ʽ
	OdDb::TextHorzMode horz;
	switch (hMode)
	{
		case esriTHALeft: horz = OdDb::kTextLeft;
			break;
		case esriTHACenter: horz = OdDb::kTextCenter;
			break;
		case esriTHARight: horz = OdDb::kTextRight;
			break;
		case esriTHAFull: horz = OdDb::kTextFit;
			break;
		default:
			horz = OdDb::kTextLeft;
	}
    pText->setHorizontalMode(horz);
    

	//��ֱ���뷽ʽ
	OdDb::TextVertMode vert;
	switch (vMode)
	{
	case esriTVATop: vert = OdDb::kTextTop;
		break;
	case esriTVACenter: vert = OdDb::kTextVertMid;
		break;
	case esriTVABaseline: vert = OdDb::kTextBase;
		break;
	case esriTVABottom: vert = OdDb::kTextBottom;
		break;
	default:
		vert = OdDb::kTextBase;
	}
	pText->setVerticalMode(vert);
    
	
	OdGePoint3d alignPt;
    if (!GetAttribute(pFeature, "AlignPtX", cstemp))
        alignPt.x = position.x;
    else
        alignPt.x = atof(cstemp);
    if (!GetAttribute(pFeature, "AlignPtY", cstemp))
        alignPt.y = position.y;
    else
        alignPt.y = atof(cstemp);
    pText->setAlignmentPoint(alignPt);

    pText->setLayer(layerId, false);


    //����ע�ǵ���ɫ���� by zongliang
    int entcolor;
    if (!GetAttribute(pFeature, "Color", cstemp))
        entcolor = 0;
    else
        entcolor = atoi(cstemp);
    if (entcolor < 0 || entcolor > 256)
        entcolor = 0;
    pText->setColorIndex(entcolor);


    //�����չ����
    addExtraAttribs(pFeature, pText);
}

/************************************************************************
��Ҫ���� :�� Field ���ҵ�����ֵ��ȡֵ
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
BOOL XDwgWriter::GetAttribute(IFeature* pFeature, CString sAttrName, CString& sAttrValue)
{
    long lFldInd;
    CComVariant vtVal;
    CComBSTR bsStr;
    bsStr = sAttrName;
    IFieldsPtr pFields;
    pFeature->get_Fields(&pFields);
    pFields->FindField(bsStr, &lFldInd);

    sAttrValue = "";

    if (lFldInd != -1)
    {

        pFeature->get_Value(lFldInd, &vtVal);

        //�ѽǶ�ת��Ϊ����
        if (m_bConvertAngle && (sAttrName.CompareNoCase("Angle") == 0))
        {
            double dAngle = vtVal.dblVal;
            double dRadian = dAngle / g_dAngleParam;
            vtVal = dRadian;
        }

        sAttrValue = GetFieldValueAsString(vtVal);
        if (!sAttrValue.IsEmpty())
        {
            return TRUE;
        }
    }
    else
    {
        //�������Զ���
        if (!m_sCompareField.IsEmpty() && m_pCompareTable != NULL)
        {
            long lFtCmpField;
            pFields->FindField(CComBSTR(m_sCompareField), &lFtCmpField);
            if (lFtCmpField != -1)
            {
                pFeature->get_Value(lFtCmpField, &vtVal);
                if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
                {
                    CString sVal = GetFieldValueAsString(vtVal);
                    IQueryFilterPtr pFilter(CLSID_QueryFilter);
                    CString sWhereClause;

					// �������˵ڶ�����ֶ�����������ֶν������
					if (m_bWidthCompareField2)
					{
						long lFtCmpField2 = 0;
						pFields->FindField(CComBSTR(m_csGdbField2), &lFtCmpField2);
						if (lFtCmpField2 != -1)
						{
							pFeature->get_Value(lFtCmpField2, &vtVal);
							if (vtVal.vt != VT_NULL && vtVal.vt != VT_EMPTY)
							{
								CString csCmpField2Value = GetFieldValueAsString(vtVal);
								sWhereClause.Format("CompareValue='%s' AND %s='%s'", sVal, m_csConfigField2, csCmpField2Value);
							}
							else
							{
								return FALSE;
							}
						}
						else
						{
							return FALSE;
						}
					}
					else
					{
						sWhereClause.Format("CompareValue='%s'", sVal); //����Ҫ�ض�������ֵ,�Ҷ��ձ��ж�Ӧ�ļ�¼
					}

                    pFilter->put_WhereClause(CComBSTR(sWhereClause));
                    IEsriCursorPtr pCur = NULL;
                    m_pCompareTable->Search(pFilter, VARIANT_TRUE, &pCur);
					if (pCur != NULL)
                    {
                        IEsriRowPtr pRow = NULL;
                        pCur->NextRow(&pRow);
                        if (pRow != NULL)
                        {
                            IFieldsPtr pCompareTableFields;
                            pRow->get_Fields(&pCompareTableFields);
                            long lcmptbfield;
                            pCompareTableFields->FindField(CComBSTR(sAttrName), &lcmptbfield);
                            if (lcmptbfield != -1)
                            {
                                CComVariant vtCmpTableVal;
                                pRow->get_Value(lcmptbfield, &vtCmpTableVal);
                                if (vtCmpTableVal.vt != VT_NULL && vtCmpTableVal.vt != VT_EMPTY)
                                {
                                    sAttrValue = GetFieldValueAsString(vtCmpTableVal);
									if (!sAttrValue.IsEmpty())
									{
										//����ɫ�����⴦����������ֵ��ByLayer��ByBlock
										if(sAttrName.CompareNoCase("Color") ==0)
										{
											if(sAttrValue.CompareNoCase("ByLayer") == 0)
											{
												sAttrValue = "256";
											}
											else if (sAttrValue.CompareNoCase("ByBlock") == 0)
											{
												sAttrValue = "0";
											}
										}

										return TRUE;
									}
                                }
                            }
                            pCompareTableFields.Release();
                            pRow.Release();
                        }
                        pCur.Release();
                    }

                    pFilter.Release();
                }
            }

        }
    }

    return FALSE;
}

//////////////////////////////////////////////////////////////////////////
CString XDwgWriter::GetFieldValueAsString(CComVariant& var)
{
    CString strTemp;
    COleDateTime date;
    COleCurrency cy;

    switch (var.vt)
    {
    case VT_I2:
        strTemp.Format(_T("%d"), var.iVal);
        break;
    case VT_I4:
        strTemp.Format(_T("%d"), var.lVal);
        break;
    case VT_R4:
        strTemp.Format(_T("%.2f"), var.fltVal);
        break;
    case VT_R8:
        strTemp.Format(_T("%.4f"), var.dblVal);
        break;
    case VT_BSTR:
        strTemp = var.bstrVal;
        break;
    case VT_UI1:
        strTemp.Format(_T("%d"), var.bVal);
        break;
    case VT_BOOL:
        strTemp.Format(_T("%d"), (int) var.boolVal);
        break;
    case VT_DATE:
        date = var.date;
        strTemp = date.Format(_T("%Y-%m-%d %H:%M:%S"));
        break;
    case VT_CY:
        cy = var.cyVal;
        strTemp = cy.Format();
        break;
    case VT_UNKNOWN:
        strTemp = _T("Shape");
        break;
    case VT_NULL:
        strTemp = _T("");
        break;
    default:
        strTemp = _T("0");
        break;
    }

    return strTemp;
}

//////////////////////////////////////////////////////////////////////////
CString XDwgWriter::CatchErrorInfo()
{
    IErrorInfoPtr ipError;
    CComBSTR ss;
    ::GetErrorInfo(0, &ipError);
    if (ipError)
    {
        ipError->GetDescription(&ss);
        CString ssss = CW2A(ss.m_str);
        TRACE("---Error Info---" + ssss + "---\n");
        return ssss;
    }
    return "";
}



//////////////////////////////////////////////////////////////////////////
void XDwgWriter::OpenLogFile()
{
    if (m_LogInfo.GetCount() > 0)
    {
        COleDateTime dtCur = COleDateTime::GetCurrentTime();
        CString sTime;
        sTime.Format("%d%d%d%d%d%d", dtCur.GetYear(), dtCur.GetMonth(), dtCur.GetDay(), dtCur.GetHour(), dtCur.GetMinute(), dtCur.GetSecond());

        CString sLogFileName;
        sLogFileName.Format("%sת��ΪCAD������־_%s.log", GetLogPath(), sTime);

        CStdioFile f3(sLogFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
        for (POSITION pos = m_LogInfo.GetHeadPosition(); pos != NULL;)
        {
            f3.WriteString(m_LogInfo.GetNext(pos) + "\n");
        }
        f3.Close();
        WinExec("Notepad.exe " + sLogFileName, SW_SHOW);
        m_LogInfo.RemoveAll();
    }
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::GetDefaultTempFile()
{
    TCHAR lpFile[MAX_PATH];
    ::GetModuleFileName(NULL, lpFile, MAX_PATH);
    *_tcsrchr(lpFile, '\\') = 0;
    m_szCadTempFile.Format("%s\\temp2000.dwg", lpFile);
    if (GetFileAttributes(m_szCadTempFile) == -1)
    {
        return FALSE;
    }
    return TRUE;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::DoModalGetOutDwgSetting()
{
    //CDlgDwgOutSetting dlg;
    //if (dlg.DoModal() == IDOK)
    //{
    //	if (dlg.m_DwgTempFileName)
    //		m_szCadTempFile = dlg.m_DwgTempFileName;
    //	else
    //		return FALSE;
    //}
    //else
    //	return FALSE;
    return TRUE;
}

//////////////////////////////////////////////////////////////////////////
BOOL XDwgWriter::PrepareOutPut(LPCTSTR szCadOutFile)
{
    CString sztemp;
    CString sLog;

    sLog.Format("��ʼд���ݵ���%s", szCadOutFile);
    WriteLog(sLog);

    //WriteLog("��ʼдDWG�ļ�.");
    if (m_szCadTempFile.IsEmpty())
    {
        if (!GetDefaultTempFile())
        {
            sLog = "���ܼ���ģ���ļ�,Ҳ�Ҳ���Ĭ��ģ���ļ�!";
            WriteLog(sLog);
            AfxMessageBox(sLog);
            m_bInit = FALSE;
            return FALSE;
        }
    }

    char templfile[265] ;
    strcpy_s(templfile, m_szCadTempFile);
    strcpy_s(m_destfile, szCadOutFile);
    svcs.disableOutput(true);

    OdDbDatabasePtr pDb;
    OdRxObjectImpl<OdWrFileBuf> fb;

    try
    {
        pDb = svcs.readFile(templfile);
        if (pDb.isNull())
        {
            WriteLog("���ܼ���ģ���ļ�");
            AfxMessageBox("���ܼ���ģ���ļ�������ģ���ļ��Ƿ���ȷ��", MB_ICONERROR);
            m_bInit = FALSE;
            return FALSE;
        }

        fb.open(m_destfile);
        OdDbAuditInfo ainfo;
        ainfo.setFixErrors(true);
        pDb->auditDatabase(&ainfo);
        pDb->writeFile(&fb, pDb->originalFileType(), pDb->originalFileVersion(), true);
    }
    catch (OdError& err)
    {
        CString msg = svcs.getErrorDescription(err.code());
        //WriteLog(msg);
        AfxMessageBox("CADģ���ļ������ڣ�����д���CAD�ļ������ڴ�״̬���޷���������д�룬���顣", MB_ICONERROR);
        m_bInit = FALSE;
        return FALSE;
    }
    fb.close();
    pDb.release();

    m_bInit = TRUE;
    return TRUE;
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::FlushOutPut()
{
    OpenLogFile();
}

//////////////////////////////////////////////////////////////////////////
void XDwgWriter::Cursor2Dwgfile(IFeatureCursor* pFeatureCursor, CString sFeatureClassName, long lFeatureNums)
{
    if (!m_bInit)
    {
        return;
    }

    if (pFeatureCursor == NULL)
    {
        WriteLog(" pCursor Ϊ��!");
        return;
    }

    if (lFeatureNums <= 0)
    {
        return;
    }

    // д��Ŀ�����ݲ�����
    svcs.disableOutput(true);
    OdDbDatabasePtr pDb;
    OdRxObjectImpl<OdWrFileBuf> fb;
    try
    {
        pDb = svcs.readFile(m_destfile);
        if (pDb.isNull())
        {
            WriteLog("��������Ŀ���ļ�!");
            return ;
        }
        fb.open(m_destfile);
    }
    catch (OdError& err)
    {
        CString msg = "Ŀ���ļ�����д��:" + svcs.getErrorDescription(err.code());
        WriteLog(msg);
        return ;
    }

    // �޸������Χ
    if (m_ipOutExtent)
    {
        IPointPtr UpperRight, LowerLeft;
        m_ipOutExtent->get_LowerLeft(&LowerLeft);
        m_ipOutExtent->get_UpperRight(&UpperRight);
        OdGePoint3d extmin, extmax;
        LowerLeft->get_X(&extmin[0]);
        LowerLeft->get_Y(&extmin[1]);
        UpperRight->get_X(&extmax[0]);
        UpperRight->get_Y(&extmax[1]);
        extmin[2] = extmax[2] = 0.0;
        pDb->setEXTMIN(extmin);
        pDb->setEXTMAX(extmax);
    }
    // д���������

    CString cstemp;

    //�趨��������Χ
    if (m_pProgressBar != NULL)
    {
        m_pProgressBar->SetWindowText("���ڰ�" + sFeatureClassName + "ͼ�������д�뵽CAD�ļ�...");
        m_pProgressBar->SetRange(0, lFeatureNums);
        m_pProgressBar->SetPos(0);
    }

    m_sFeatureClassName = sFeatureClassName;//��¼��ǰͼ������

    // ��table��ת��dwg����
    if (sFeatureClassName.IsEmpty())
        sFeatureClassName = "0";

    ///////ע����չ����Ӧ��///////////////////////////////////////////////////////
    if (m_pXDataAttrLists != NULL)
    {
        POSITION pos = m_pXDataAttrLists->GetStartPosition();
        while (pos)
        {
            CString sRegAppName;
            CStringList* pFieldsLst;
            m_pXDataAttrLists->GetNextAssoc(pos, sRegAppName, pFieldsLst);

            RegAppName(pDb, sRegAppName);
        }
    }

    IFeaturePtr pFeature;
    pFeatureCursor->NextFeature(&pFeature);
    while (pFeature != NULL)
    {
        OdDbObjectId layerid, linetypeid;

        char layername[255] = {0};
        if (!m_sDwgLayer.IsEmpty())
        {
            //���CAD��ͼ�����Ʋ�Ϊ�գ��������д���ͼ��
            strcpy_s(layername, m_sDwgLayer);
        }
        else
        {
            CString sLayerName;
            // �ж��������Ƿ��� Layer �ֶ�,û�о��� Featname
            if (!GetAttribute(pFeature, "Layer", sLayerName))
            {
                sLayerName = sFeatureClassName;
            }
            strcpy_s(layername, sLayerName);
        }

        // �жϲ��Ƿ����,����²�,��ɫ��Ҫ��
        if (!FindLayerByName(pDb, layername, &layerid))
        {
            layerid = addLayer(pDb, layername);
        }
        // �õ�����id
        CString sLinetypeName;
        // �ж��������Ƿ��� Linetype �ֶ�,û�о��� continues
        if (GetAttribute(pFeature, "Linetype", sLinetypeName))
        {
            char linetypename[255];
            strcpy_s(linetypename, sLinetypeName);
            if (!FindLinttypeByName(pDb, linetypename, &linetypeid))
            {
                linetypeid = pDb->getLinetypeContinuousId();
            }
        }
        else
            linetypeid = pDb->getLinetypeContinuousId();
        IGeometryPtr pShape;
        HRESULT hr;
        hr = pFeature->get_Shape(&pShape);
        if (FAILED(hr))
        {
            cstemp = "getshapeʧ��" + CatchErrorInfo();
            WriteLog(cstemp);
        }
        if (pShape != NULL)
        {
            esriFeatureType featType;
            hr = pFeature->get_FeatureType(&featType);

            //ע��
            if (featType == esriFTAnnotation)
            {
                WriteAnnotation(pFeature, pDb, layerid, linetypeid);
            }
            else if (featType == esriFTSimple)
            {
                //��������
                esriGeometryType type;
                hr = pShape->get_GeometryType(&type);

                // ���� text��point��insert
                if (type == esriGeometryPoint)
                {
                    WriteGeometryPoint(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryMultipoint)
                {
                    // ���
                    //WriteLog("����㼯��!");
                    WriteMultipoint(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryLine)
                {
                    // ������
                    //WriteLog("���������Line!");
                    WriteLine(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryCircularArc)
                {
                    // Բ��
                    //WriteLog("���Բ��!");
                    WriteCircularArc(pFeature, pShape, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryEllipticArc)
                {
                    // ��Բ��
                    //WriteLog("�����Բ��!");
                    WriteEllipticArc(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryBezier3Curve)
                {
                    // Bezier3Curve
                    //WriteLog("���Bezier3Curve!");
                    WriteBezier3Curve(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryPath)
                {
                    // Path ��Щ����dwg�е�LWPline
                    // A Path is a connected, continuous sequence of Segments.
                    // A segment is a way of getting between two endpoints.
                    //WriteLog("���Path!");
                    WritePath(pFeature, pShape, pDb, layerid, linetypeid, 0);
                }
                else if (type == esriGeometryPolyline)
                {
                    // �ߴ��ļ���,���Բ��໥����
                    //�պϹ�����
                    BOOL bClosed = FALSE;

                    IPolylinePtr pPolyLine = pShape;
                    if (pPolyLine != NULL)
                    {
                        VARIANT_BOOL vbClosed;
                        pPolyLine->get_IsClosed(&vbClosed);
                        if (vbClosed == VARIANT_TRUE)
                        {
                            bClosed = TRUE;
                        }
                    }

                    WritePolyline(pFeature, pDb, layerid, linetypeid, bClosed);
                }
                else if (type == esriGeometryRing)
                {
                    // A Ring is Closed Path that defines a Two Dimensional Area.
                    //WriteLog("���Ring!");
                    WriteRing(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryPolygon)
                {
                    // A polygon is a collection of one or many exterior and interior rings.
                    //WriteLog("���Polygon!");
                    WritePolygon(pFeature, pDb, layerid, linetypeid);
                }
                else if (type == esriGeometryBag)
                {
                    // A GeometryBag is a heterogeneous collection of objects that support the IGeometry interface.
                    // �����������������͵ļ���
                    //WriteLog("��� Bag !");
                    WriteBag(pFeature, pDb, layerid, linetypeid);
                }
                else
                {
                    //cstemp.Format("δ��ӵ����� %d", type);
                    //WriteLog(cstemp);
                }
            }
        }

        if (m_pProgressBar != NULL)
        {
            m_pProgressBar->StepIt();
        }

        pFeature.Release();
        pFeatureCursor->NextFeature(&pFeature);

    }

    try
    {
        OdDbAuditInfo ainfo;
        ainfo.setFixErrors(true);
        pDb->auditDatabase(&ainfo);
        pDb->writeFile(&fb, pDb->originalFileType(), pDb->originalFileVersion(), true);

        fb.close();
    }
    catch (OdError& err)
    {
        CString msg = svcs.getErrorDescription(err.code());
        WriteLog(msg);
        return ;
    }

    pDb.release();


	ClearXDataAttrLists();

}

/************************************************************************
��Ҫ���� : ��FeatureDatasetд��DWG�ļ�, ��д����չ����
������� : pFeatureDataset����Ҫд���FeatureDataset
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDwgWriter::FeatureDataset2DwgFile(IFeatureDataset* pFeatureDataset)
{
    long lCount;
    IFeatureClassContainerPtr pFeatureClassContainer = pFeatureDataset;
    IFeatureClassPtr pFeatureClass;
    pFeatureClassContainer->get_ClassCount(&lCount);
    for (int i = 0; i < lCount; i++)
    {
        pFeatureClassContainer->get_Class(i, &pFeatureClass);
        FeatureClass2Dwgfile(pFeatureClass);
    }
}

void XDwgWriter::SelectionSet2Dwgfile(ISelectionSet* pSelectionSet, XDataAttrLists* pExtraAttribs)
{
    ClearXDataAttrLists();
    m_pXDataAttrLists = pExtraAttribs;

    CString sztemp;
    if (!pSelectionSet)
    {
        WriteLog("SelectionSetΪ��!");
        return;
    }
    // featureclass����
    CComBSTR csFTCName;
    ITablePtr pTable;
    pSelectionSet->get_Target(&pTable);
    ((IDatasetPtr) pTable)->get_BrowseName(&csFTCName);
    long lFtNums;
    pSelectionSet->get_Count(&lFtNums);
    IEsriCursorPtr pCursor;
    pSelectionSet->Search(NULL, VARIANT_TRUE, &pCursor);
    IFeatureCursorPtr pFeatureCursor(pCursor);
    Cursor2Dwgfile(pFeatureCursor, CString(csFTCName), lFtNums);
    return ;
}


//************************************************************************
//��Ҫ���� : Ҫ����д��CAD�ļ�
//������� :
//�� �� ֵ :
//�޸���־ :
//************************************************************************
void XDwgWriter::FeatureClass2Dwgfile(IFeatureClass* pFeatureClass, XDataAttrLists* pExtraAttribs)
{
    ClearXDataAttrLists();
    m_pXDataAttrLists = pExtraAttribs;

    if (pFeatureClass == NULL)
    {
        WriteLog("FeatureClass Ϊ��!");
        return;
    }

    IGeoDatasetPtr pGeoDataset(pFeatureClass);
    if (pGeoDataset != NULL)
    {
        pGeoDataset->get_Extent(&m_ipOutExtent);
    }

    long lFeatureCount = 0;
    pFeatureClass->FeatureCount(NULL, &lFeatureCount);
    if (lFeatureCount <= 0)
    {
        return;
    }

    CString sFeatureClassName;
    BSTR bsFeatureClassName;
    IDatasetPtr pDataset(pFeatureClass);
    if (pDataset != NULL)
    {
        pDataset->get_Name(&bsFeatureClassName);
    }
    else
    {
        pFeatureClass->get_AliasName(&bsFeatureClassName);
    }

    sFeatureClassName = bsFeatureClassName;

    int pos = sFeatureClassName.ReverseFind('.');
    if (pos > 0)
    {
        sFeatureClassName = sFeatureClassName.Right(sFeatureClassName.GetLength() - pos - 1);
    }

    CString sLog;

    sLog.Format("��ʼ��%sͼ��д��CAD�ļ���", sFeatureClassName);
    WriteLog(sLog);

    IFeatureCursorPtr pFeatureCursor;
    pFeatureClass->Search(NULL, VARIANT_FALSE, &pFeatureCursor);
    if (pFeatureCursor != NULL)
    {
        Cursor2Dwgfile(pFeatureCursor, sFeatureClassName, lFeatureCount);
    }
    else
    {
        WriteLog("FeatureClass pCursor Ϊ��!");
    }

    sLog.Format("��ɶ�%sͼ�����ݵ�д�롣��д��Ҫ�أ�%d", sFeatureClassName, lFeatureCount);
    WriteLog(sLog);

}

/************************************************************************
��Ҫ���� : �������Ӧ�ü��ֶ�
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDwgWriter::ClearXDataAttrLists()
{
    try
    {
	    if (m_pXDataAttrLists == NULL)
	        return;
	    POSITION pos = m_pXDataAttrLists->GetStartPosition();
	    while (pos)
	    {
	        CString sRegAppName;
	        CStringList* pLstAttribs;
	        m_pXDataAttrLists->GetNextAssoc(pos, sRegAppName, pLstAttribs);
	        pLstAttribs->RemoveAll();
	        delete pLstAttribs;
	    }
	    m_pXDataAttrLists->RemoveAll();
	    m_pXDataAttrLists = NULL;
    }
    catch (...)
    {
    	m_pXDataAttrLists = NULL;
    }
}

/************************************************************************
��Ҫ���� : ע����չ����Ӧ������
������� :
�� �� ֵ :
�޸���־ :
************************************************************************/
void XDwgWriter::RegAppName(OdDbDatabase* pDb, CString sRegAppName)
{
    POSITION pos = m_registeredAppNames.Find(sRegAppName);
    if (pos == NULL)
    {
        OdString odstr;
        odstr.format("%s", sRegAppName);
        pDb->newRegApp(odstr);

        m_registeredAppNames.AddTail(sRegAppName);
    }
}

/********************************************************************
��Ҫ���� :
������� :
�� �� ֵ :
�޸���־ :
*********************************************************************/
void XDwgWriter::SaveLogFile(CString sFilePath)
{
    try
    {
        if (m_LogInfo.GetCount() > 0)
        {
            CString sLogFileName = sFilePath;

            CStdioFile f3(sLogFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
            for (POSITION pos = m_LogInfo.GetHeadPosition(); pos != NULL;)
            {
                f3.WriteString(m_LogInfo.GetNext(pos) + "\n");
            }

            f3.Close();

        }
    }
    catch (...)
    {
		CString sErr;
		sErr.Format("д��־��%s���������ļ�·���Ƿ���ȷ��", sFilePath);
		AfxMessageBox(sErr);
    }
}

//////////////////////////////////////////////////////////////////////
//��Ҫ���� : д��־�ļ�
//������� :
//�� �� ֵ :
//�޸���־ :
//////////////////////////////////////////////////////////////////////
void XDwgWriter::WriteLog(CString sLog)
{
    if (m_pLogRec == NULL)
    {
        return;
    }

    if (!sLog.IsEmpty())
    {
        m_pLogRec->WriteLog(sLog);
    }
}
