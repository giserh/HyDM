#include "stdafx.h"
#include "db.h"
#include "DwgEntityDumper.h"
//#include "XDwgDirectReader.h"

ODRX_NO_CONS_DEFINE_MEMBERS(OdDbEntity_Dumper, OdRxObject)

void OdDbEntity_Dumper::dumpCommonData(OdDbEntity* pEnt)
{
}

// this method is called for all entities for which there
// aren't peculiar approach yet
IGeometry* OdDbEntity_Dumper::dump(OdDbEntity* pEnt)
{
    OdDbHandle hTmp;
    char buff[20];

    hTmp = pEnt->getDbHandle();
    hTmp.getIntoAsciiBuffer(buff);
    CString sztemp;
    sztemp.Format("ʵ��δ������  %s,%s", pEnt->isA()->name(), buff);
    m_DwgReader->WriteLog(sztemp);

    return 0;
}

class OdDbPoint_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;

        OdDbPointPtr pDbPoint = pEnt;

        pPoint->PutCoords(pDbPoint->position().x, pDbPoint->position().y);
        pShape = pPoint;
        // �������
        sztemp.Format("%f", pDbPoint->position().z);
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pPointFeatureBuffer);
        sztemp.Format("%.f", pDbPoint->thickness());
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPointFeatureBuffer);

        return pShape.Detach();
    }
};

class OdDbLine_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polyline);
        IGeometryPtr pShape;

        OdDbLinePtr pLine = pEnt;
        OdGePoint3d pos1, pos2 ;
        pLine->getStartPoint(pos1);
        pPoint->PutCoords(pos1.x, pos1.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pLine->getEndPoint(pos2);
        pPoint->PutCoords(pos2.x, pos2.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        // ����յ�����һ��ת�ɵ�
        if (pos1.x == pos2.x && pos1.y == pos2.y)
        {
            pShape = pPoint;
            sztemp.Format("%f", pos2.z);
            m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pPointFeatureBuffer);
            sztemp.Format("%.f", pLine->thickness());
            m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPointFeatureBuffer);
        }
        else
        {
            pShape = pPointColl;
            sztemp.Format("%f", pos2.z);
            m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pLineFeatureBuffer);
            sztemp.Format("%.f", pLine->thickness());
            m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        }
        return pShape.Detach();
    }
};

class OdDb2dPolyline_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl;
        ISegmentCollectionPtr pSegColl(CLSID_Polyline);
        IGeometryPtr pShape;
        pShape = pSegColl;
        pPointColl = pSegColl;

        // ����bulgeֵ
        IPointCollectionPtr pBugColl(CLSID_Polyline);
        // ����line���е�
        IPointCollectionPtr pLineColl(CLSID_Polyline);

        OdDb2dPolylinePtr pPoly = pEnt;
        OdDbObjectIteratorPtr pIter = pPoly->vertexIterator();

        for (; !pIter->done(); pIter->step())
        {
            OdDb2dVertexPtr pVertex = pIter->entity();
            if (pVertex.get())
            {
                OdGePoint3d pos = pVertex->position();
                //ȥ��spline���Ƶ�
                if (pVertex->vertexType() != OdDb::k2dSplineCtlVertex)
                {
                    pPoint->PutCoords(pos.x, pos.y);
                    pLineColl->AddPoint(pPoint, NULL, NULL);
                    double bulge = 0;
                    bulge = pVertex->bulge();
                    // Bulge is the tangent of 1/4 the included angle of the arc segment, measured counterclockwise
                    pPoint->PutCoords(4.0 * atan(bulge), 0);
                    pBugColl->AddPoint(pPoint, NULL, NULL);
                }
            }
        }
        if (pPoly->isClosed())
        {
            pLineColl->get_Point(0, &pPoint);
            pLineColl->AddPoint(pPoint, NULL, NULL);
            pBugColl->get_Point(0, &pPoint);
            pBugColl->AddPoint(pPoint, NULL, NULL);
        }
        long pointsNum = 0;
        pLineColl->get_PointCount(&pointsNum);
        double incl;
        for (int iarc = 0; iarc < pointsNum; iarc++)
        {
            pBugColl->get_Point(iarc, &pPoint);
            pPoint->get_X(&incl);
            pLineColl->get_Point(iarc, &pPoint);
            pPointColl->AddPoint(pPoint, NULL, NULL);
            if (incl != 0 && iarc < pointsNum - 1)
            {
                double from[2], to[2];
                IPointPtr pFromPoint(CLSID_Point);
                IPointPtr pToPoint(CLSID_Point);
                IPointPtr pCtlPoint(CLSID_Point);
                pLineColl->get_Point(iarc, &pFromPoint);
                pLineColl->get_Point(iarc + 1, &pToPoint);
                pFromPoint->get_X(&from[0]);
                pFromPoint->get_Y(&from[1]);
                pToPoint->get_X(&to[0]);
                pToPoint->get_Y(&to[1]);

                IConstructCircularArcPtr pConstructArc(CLSID_CircularArc);
                if (incl >= 0)
                    pConstructArc->ConstructEndPointsAngle(pFromPoint, pToPoint, esriArcCounterClockwise, incl);
                else
                    pConstructArc->ConstructEndPointsAngle(pFromPoint, pToPoint, esriArcClockwise, incl);
                pSegColl->AddSegment((ISegmentPtr) pConstructArc);
            }
        }

        // ͬʱ����������
        sztemp.Format("%f", pPoly->elevation());
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        sztemp.Format("%.f", pPoly->thickness());
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        sztemp.Format("%f", pPoly->defaultStartWidth());
        m_DwgReader->AddAttributes("Width", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Width", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        return pShape.Detach();
    }
};

class OdDb3dPolyline_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polyline);
        IGeometryPtr pShape;
        pShape = pPointColl;

        OdDb3dPolylinePtr pPoly = pEnt;
        OdDbObjectIteratorPtr pIter = pPoly->vertexIterator();
        for (; !pIter->done(); pIter->step())
        {
            OdDb3dPolylineVertexPtr pVertex = pIter->entity();
            if (pVertex.get())
            {
                OdGePoint3d pos = pVertex->position();
                pPoint->PutCoords(pos.x, pos.y);
                pPointColl->AddPoint(pPoint, NULL, NULL);
            }
        }
        if (pPoly->isClosed())
        {
            pPointColl->get_Point(0, &pPoint);
            pPointColl->AddPoint(pPoint, NULL, NULL);
        }

        return pShape.Detach();
    }
};


class OdDbPolyFaceMesh_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        HRESULT hr;
        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polyline);
        IGeometryPtr pShape;
        IGeometryCollectionPtr ipGeometryCollection(CLSID_GeometryBag);

        OdDbPolyFaceMeshPtr pPoly = pEnt;
        int nVerts = pPoly->numVertices();
        int nFaces = pPoly->numFaces();

        OdDbObjectIteratorPtr pIter = pPoly->vertexIterator();
        for (; !pIter->done(); pIter->step())
        {
            OdDbPolyFaceMeshVertexPtr pVertex = pIter->entity()->queryX(OdDbPolyFaceMeshVertex::desc());
            if (!pVertex.isNull())
            {
                OdGePoint3d pos(pVertex->position());
                pPoint->PutCoords(pos.x, pos.y);
                pPointColl->AddPoint(pPoint, NULL, NULL);
            }
            else
            {
                OdDbFaceRecordPtr pFace = pIter->entity()->queryX(OdDbFaceRecord::desc());
                if (!pFace.isNull())
                {
                    // ÿ��face����4���㹹��
                    for (int i = 0; i < 4; ++i)
                    {
                        IPointCollectionPtr pPointLine;
                        pPointLine.CreateInstance(CLSID_Polyline);
                        pShape = pPointLine;
                        int vertindex;
                        vertindex = pFace->getVertexAt(i);
                        //  ������ʾ
                        if (vertindex == 0)
                        {
                        }
                        if (vertindex > 0)
                        {
                            // vertindex�Ǵ�1��ʼ����
                            pPointColl->get_Point(vertindex - 1, &pPoint);
                            pPointLine->AddPoint(pPoint);
                            if (i == 3)
                                vertindex = pFace->getVertexAt(0);
                            else
                                vertindex = pFace->getVertexAt(i + 1);
                            if (vertindex < 0)
                                vertindex = 0 - vertindex;
                            pPointColl->get_Point(vertindex - 1, &pPoint);
                            pPointLine->AddPoint(pPoint);
                            ipGeometryCollection->AddGeometry(pShape, NULL, NULL);
                        }
                    }
                }
                else
                {
                    // Unknown entity type
                    sztemp.Format("PolyFaceMesh      %d", pVertex->isA()->name());
                    m_DwgReader->WriteLog(sztemp);
                }
            }
        }
        IEnumGeometryPtr pEnumGeom(ipGeometryCollection);
        ITopologicalOperatorPtr pTopo(CLSID_Polyline);
        hr = pTopo->ConstructUnion(pEnumGeom);
        hr = pTopo->Simplify();
        IPolygonPtr pPolygon(CLSID_Polygon);
        ((IPointCollectionPtr) pPolygon)->SetPointCollection((IPointCollectionPtr) pTopo);
        IGeometryPtr pGeometry;
        pGeometry = pPolygon;
        return pGeometry.Detach();
    }
};

class OdDbPolygonMesh_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polygon);
        IGeometryPtr pPolygonShape;
        pPolygonShape = pPointColl;

        OdDbPolygonMeshPtr pPoly = pEnt;
        OdDbObjectIteratorPtr pIter = pPoly->vertexIterator();
        for (; !pIter->done(); pIter->step())
        {
            OdDbPolygonMeshVertexPtr pVertex = pIter->entity();
            if (pVertex.get())
            {
                OdGePoint3d pos = pVertex->position();
                pPoint->PutCoords(pos.x, pos.y);
                pPointColl->AddPoint(pPoint, NULL, NULL);
            }
        }
        return pPolygonShape.Detach();
    }
};


class OdDbText_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        OdDbTextPtr pText = pEnt;
        OdGePoint3dArray array;
        pText->getBoundingPoints(array);
        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;
        pShape = pPoint;


		OdGePoint3d textPos = pText->alignmentPoint();
		if (textPos.x <= 0.0001 && textPos.y <= 0.0001) //���û�ж���㣬��ʹ��λ�õ�
		{
			textPos = pText->position();
		}

		pPoint->PutCoords(textPos.x, textPos.y);

        // Ϊ���½�����
        //pPoint->PutCoords(pText->position().x, pText->position().y);

        //��Ϊ�������
        if (m_DwgReader->m_IsCreateAnnotation == FALSE)
        {
            CString tempstr;
            tempstr.Format("%s", pText->textString().c_str());
            m_DwgReader->AddAttributes("TextString", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pText->height());
            m_DwgReader->AddAttributes("Height", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pText->rotation());
            m_DwgReader->AddAttributes("Angle", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pText->widthFactor());
            m_DwgReader->AddAttributes("WidthFactor", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pText->oblique());
            m_DwgReader->AddAttributes("Oblique", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%d", pText->horizontalMode());
            m_DwgReader->AddAttributes("HzMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%d", pText->verticalMode());
            m_DwgReader->AddAttributes("VtMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%.f", pText->thickness());
            m_DwgReader->AddAttributes("Thickness", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%f", pText->alignmentPoint().x);
            m_DwgReader->AddAttributes("AlignPtX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pText->alignmentPoint().y);
            m_DwgReader->AddAttributes("AlignPtY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[2]).x);
            m_DwgReader->AddAttributes("PtMinX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[2]).y);
            m_DwgReader->AddAttributes("PtMinY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[1]).x);
            m_DwgReader->AddAttributes("PtMaxX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[1]).y);
            m_DwgReader->AddAttributes("PtMaxY", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            OdGiTextStyle style;
            giFromDbTextStyle(pText->textStyle(), style);
            tempstr.Format("%s", style.ttfdecriptor().fileName().c_str());
            m_DwgReader->AddAttributes("ShapeFilename", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", style.bigFontFileName().c_str());
            m_DwgReader->AddAttributes("BigFontname", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", (OdDbSymbolTableRecordPtr(pText->textStyle().safeOpenObject()))->getName());
            m_DwgReader->AddAttributes("ShapeName", tempstr, m_DwgReader->m_pTextFeatureBuffer);
        }

        return pShape.Detach();
    }
};


class OdDbBlockReference_Dumper : public OdDbEntity_Dumper
{
public:
    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        CString sztemp;
        OdDbBlockReferencePtr pBlkRef = pEnt;
        // �����ǵ�һ�����ͷ��Ϣ
        //if (m_DwgReader->m_bn + 1 == m_DwgReader->m_lBlockNum)
        {
            sztemp.Format("%d", m_DwgReader->m_lBlockNum);
            m_DwgReader->AddAttributes("Blocknumber", sztemp, m_DwgReader->m_pPointFeatureBuffer);
            m_DwgReader->AddAttributes("Blocknumber", sztemp, m_DwgReader->m_pLineFeatureBuffer);
            m_DwgReader->AddAttributes("Blocknumber", sztemp, m_DwgReader->m_pTextFeatureBuffer);
            m_DwgReader->AddAttributes("Blocknumber", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
            m_DwgReader->m_bn = m_DwgReader->m_lBlockNum;
            // �õ���Ĳ���
            m_DwgReader->m_szBlockLayer.Format("%s", pEnt->layer().c_str());
			//m_DwgReader->m_szBlockLayer.MakeUpper();
            
			// �õ������ɫ
            if (pEnt->colorIndex() > 255 || pEnt->colorIndex() < 1)
            {
                OdDbLayerTableRecordPtr pLayer = pEnt->layerId().safeOpenObject();
                m_DwgReader->m_szBlockColor.Format("%d", pLayer->colorIndex());
            }
            else
                m_DwgReader->m_szBlockColor.Format("%d", pEnt->colorIndex());
            // �õ� Linetype
            m_DwgReader->m_szBlockLT.Format("%s", pEnt->linetype().c_str());
            m_DwgReader->m_szBlockLT.MakeUpper();
            // �õ�����
            m_DwgReader->m_szblockname = (OdDbBlockTableRecordPtr(pBlkRef->blockTableRecord().safeOpenObject()))->getName();
            //m_DwgReader->m_szblockname.MakeUpper();
        }

        bool bExplode = true;

        // ���ɢ����
        if (m_DwgReader->m_IsBreakBlock)
        {
            if (m_DwgReader->m_unExplodeBlocks.GetCount() > 0)
            {
                POSITION posBlockName = NULL;

				if (m_DwgReader->m_iUnbreakBlockMode == 0)
				{
					posBlockName = m_DwgReader->m_unExplodeBlocks.Find(m_DwgReader->m_szblockname);
				}
				else if (m_DwgReader->m_iUnbreakBlockMode == 1)
				{
					posBlockName = m_DwgReader->m_unExplodeBlocks.Find(m_DwgReader->m_szBlockLayer);
				}

				if (posBlockName != NULL)
                {
                    //����ɢ
                    bExplode = false;
                }
            }

            if (bExplode)
            {
                //��ɢ
                OdRxObjectPtrArray pEntSet;
                pBlkRef->explodeGeometry(pEntSet);
                for (unsigned i = 0; i < pEntSet.size(); ++i)
                {
                    m_DwgReader->CleanAllFeatureBuffers();
                    OdSmartPtr<OdDbEntity_Dumper> pEntDumper = OdDbEntityPtr(pEntSet[i]);
                    HRESULT hr;
                    CString sztemp;
                    IGeometryPtr pShape;
                    pEntDumper->m_DwgReader = m_DwgReader;
                    pShape = pEntDumper->dump(OdDbEntityPtr(pEntSet[i]));

                    if (pShape != NULL)
                    {
                        OdRxObjectPtr pExplodedEnt = pEntSet[i];

                        // �ı�
                        CString sEntType = OdDbEntityPtr(pEntSet[i])->isA()->name();

                        if (strcmp(sEntType, "AcDbMText") == 0 || strcmp(sEntType, "AcDbText") == 0 || strcmp(sEntType, "AcDbShape") == 0)
                        {
                            if (m_DwgReader->m_IsCreateAnnotation)
                            {
                                //����ע��
                                m_DwgReader->InsertAnnoFeature(pExplodedEnt);
                            }
                            else
                            {
                                hr = m_DwgReader->m_pTextFeatureBuffer->putref_Shape(pShape);
                                if(SUCCEEDED(hr))
                                {
                                    m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pEntSet[i]), "Annotation", m_DwgReader->m_pTextFeatureBuffer);
                                    // ��ȡ��ͷ����Ϣ��ɫ
                                    m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pTextFeatureBuffer);
                                    m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pTextFeatureBuffer);
                                    m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pTextFeatureBuffer);
                                    m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pTextFeatureBuffer);

                                    //�������
                                    if (m_DwgReader->CompareCodes(m_DwgReader->m_pTextFeatureBuffer))
                                    {
                                        hr = m_DwgReader->m_pTextFeatureCursor->InsertFeature(m_DwgReader->m_pTextFeatureBuffer, &(m_DwgReader->m_vID));
                                        if (FAILED(hr))
                                        {
                                            sztemp = "Text����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                            m_DwgReader->WriteLog(sztemp);
                                        }
                                    }
                                }
								else
								{
									sztemp = "Text�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
									m_DwgReader->WriteLog(sztemp);
								}
                            }
                        }
                        else
                        {
                            esriGeometryType shapeType;
                            pShape->get_GeometryType(&shapeType);
                            // Ƕ�׵Ŀ鲻����
                            if (shapeType == esriGeometryPoint && strcmp(OdDbEntityPtr(pEntSet[i])->isA()->name(), "AcDbBlockReference") != 0)
                            {
                                hr = m_DwgReader->m_pPointFeatureBuffer->putref_Shape(pShape);
                                if(SUCCEEDED(hr))
                                {
                                    m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pEntSet[i]), "Point", m_DwgReader->m_pPointFeatureBuffer);
                                    // ��ȡ��ͷ����Ϣ��ɫ
                                    m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pPointFeatureBuffer);
                                    m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pPointFeatureBuffer);
                                    m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pPointFeatureBuffer);
                                    m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pPointFeatureBuffer);

                                    //�������
                                    if (m_DwgReader->CompareCodes(m_DwgReader->m_pPointFeatureBuffer))
                                    {
                                        hr = m_DwgReader->m_pPointFeatureCursor->InsertFeature(m_DwgReader->m_pPointFeatureBuffer, &(m_DwgReader->m_vID));
                                        if (FAILED(hr))
                                        {
                                            sztemp = "Point����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                            m_DwgReader->WriteLog(sztemp);
                                        }
                                    }
                                }
								else
								{
									sztemp = "Point�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
									m_DwgReader->WriteLog(sztemp);
								}
                            }
                            else if (shapeType == esriGeometryPolyline)
                            {
                                hr = m_DwgReader->m_pLineFeatureBuffer->putref_Shape(pShape);
                                if(SUCCEEDED(hr))
                                {
                                    m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pEntSet[i]), "Line", m_DwgReader->m_pLineFeatureBuffer);
                                    // ��ȡ��ͷ����Ϣ��ɫ
                                    m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pLineFeatureBuffer);
                                    m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pLineFeatureBuffer);
                                    m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pLineFeatureBuffer);
                                    m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pLineFeatureBuffer);

                                    //�������
                                    if (m_DwgReader->CompareCodes(m_DwgReader->m_pLineFeatureBuffer))
                                    {
                                        hr = m_DwgReader->m_pLineFeatureCursor->InsertFeature(m_DwgReader->m_pLineFeatureBuffer, &(m_DwgReader->m_vID));
                                        if (FAILED(hr))
                                        {
                                            sztemp = "Line����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                            m_DwgReader->WriteLog(sztemp);
                                        }
                                    }
                                }
								else
								{
									sztemp = "Line�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
									m_DwgReader->WriteLog(sztemp);
								}

                                // ����պϾ���������
                                VARIANT_BOOL isclosed;
                                IPolylinePtr pPolyline(CLSID_Polyline);
                                pPolyline = pShape;
                                pPolyline->get_IsClosed(&isclosed);
                                if (isclosed && m_DwgReader->m_IsLine2Polygon)
                                {
                                    IPolygonPtr pPolygon(CLSID_Polygon);
                                    ((ISegmentCollectionPtr) pPolygon)->AddSegmentCollection((ISegmentCollectionPtr) pPolyline);
                                    hr = m_DwgReader->m_pPolygonFeatureBuffer->putref_Shape((IGeometryPtr) pPolygon);
                                    if(SUCCEEDED(hr))
                                    {
                                        m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pEntSet[i]), "Polygon", m_DwgReader->m_pPolygonFeatureBuffer);
                                        // ��ȡ��ͷ����Ϣ��ɫ
                                        m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pPolygonFeatureBuffer);
                                        m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pPolygonFeatureBuffer);
                                        m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pPolygonFeatureBuffer);
                                        m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pPolygonFeatureBuffer);
                                        //�������
                                        if (m_DwgReader->CompareCodes(m_DwgReader->m_pPolygonFeatureBuffer))
                                        {
                                            hr = m_DwgReader->m_pPolygonFeatureCursor->InsertFeature(m_DwgReader->m_pPolygonFeatureBuffer, &(m_DwgReader->m_vID));
                                            if (FAILED(hr))
                                            {
                                                sztemp = "Polyline����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                                m_DwgReader->WriteLog(sztemp);
                                            }
                                        }
                                    }
									else
									{
										sztemp = "Polyline�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
										m_DwgReader->WriteLog(sztemp);
									}
                                }
                            }
                            else if ((shapeType == esriGeometryPolygon) && m_DwgReader->m_IsReadPolygon)
                            {
                                hr = m_DwgReader->m_pPolygonFeatureBuffer->putref_Shape(pShape);
                                if(SUCCEEDED(hr))
                                {
                                    m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pEntSet[i]), "Polygon", m_DwgReader->m_pPolygonFeatureBuffer);
                                    // ��ȡ��ͷ����Ϣ��ɫ
                                    m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pPolygonFeatureBuffer);
                                    m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pPolygonFeatureBuffer);
                                    m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pPolygonFeatureBuffer);
                                    m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pPolygonFeatureBuffer);

                                    //�������
                                    if (m_DwgReader->CompareCodes(m_DwgReader->m_pPolygonFeatureBuffer))
                                    {
                                        hr = m_DwgReader->m_pPolygonFeatureCursor->InsertFeature(m_DwgReader->m_pPolygonFeatureBuffer, &(m_DwgReader->m_vID));
                                        if (FAILED(hr))
                                        {
                                            sztemp = "Polygon����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                            m_DwgReader->WriteLog(sztemp);
                                        }
                                    }
                                }
								else
								{
									sztemp = "Polygon�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
									m_DwgReader->WriteLog(sztemp);
								}
                            }
                        }
                        if (i % m_DwgReader->m_StepNum == 0)
                        {
                            if (m_DwgReader->m_pPointFeatureCursor)
                                m_DwgReader->m_pPointFeatureCursor->Flush();
                            if (m_DwgReader->m_pTextFeatureCursor)
                                m_DwgReader->m_pTextFeatureCursor->Flush();
                            if (m_DwgReader->m_pLineFeatureCursor)
                                m_DwgReader->m_pLineFeatureCursor->Flush();
                            if (m_DwgReader->m_pAnnoFeatureCursor)
                                m_DwgReader->m_pAnnoFeatureCursor->Flush();
                            if (m_DwgReader->m_pPolygonFeatureCursor)
                                m_DwgReader->m_pPolygonFeatureCursor->Flush();
                        }
                    }
                }
                if (m_DwgReader->m_pPointFeatureCursor)
                    m_DwgReader->m_pPointFeatureCursor->Flush();
                if (m_DwgReader->m_pTextFeatureCursor)
                    m_DwgReader->m_pTextFeatureCursor->Flush();
                if (m_DwgReader->m_pLineFeatureCursor)
                    m_DwgReader->m_pLineFeatureCursor->Flush();
                if (m_DwgReader->m_pAnnoFeatureCursor)
                    m_DwgReader->m_pAnnoFeatureCursor->Flush();
                if (m_DwgReader->m_pPolygonFeatureCursor)
                    m_DwgReader->m_pPolygonFeatureCursor->Flush();

            }
        }

        //////////////////////////////////////////////////////////////////////////
        //��ȡ��������Ϊ�ı����ע��(����)
        OdDbObjectIteratorPtr pIter = pBlkRef->attributeIterator();
        for (; !pIter->done(); pIter->step())
        {
            OdDbAttributePtr pAttr = pIter->entity();
            if (!pAttr.isNull())
            {
                if (m_DwgReader->m_IsCreateAnnotation)
                {
                    //����ע�Ƕ���
                    m_DwgReader->InsertAnnoFeature(pAttr);
                }
                else
                {
					//�����ı������
                    m_DwgReader->CleanFeatureBuffer(m_DwgReader->m_pTextFeatureBuffer);

                    IGeometryPtr pShape;
                    CString sztemp;
                    OdDbTextPtr pText = pAttr;
                    OdGePoint3dArray array;
                    pText->getBoundingPoints(array);
                    IPointPtr pPoint(CLSID_Point);
                    pShape = pPoint;
                    // Ϊ���½�����
                    pPoint->PutCoords(pText->position().x, pText->position().y);

                    CString tempstr;
                    tempstr.Format("%s", pText->textString().c_str());
                    m_DwgReader->AddAttributes("TextString", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", pText->height());
                    m_DwgReader->AddAttributes("Height", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", pText->rotation());
                    m_DwgReader->AddAttributes("Angle", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", pText->widthFactor());
                    m_DwgReader->AddAttributes("WidthFactor", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", pText->oblique());
                    m_DwgReader->AddAttributes("Oblique", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%d", pText->horizontalMode());
                    m_DwgReader->AddAttributes("HzMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%d", pText->verticalMode());
                    m_DwgReader->AddAttributes("VtMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%.f", pText->thickness());
                    m_DwgReader->AddAttributes("Thickness", tempstr, m_DwgReader->m_pTextFeatureBuffer);

                    tempstr.Format("%f", pText->alignmentPoint().x);
                    m_DwgReader->AddAttributes("AlignPtX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", pText->alignmentPoint().y);
                    m_DwgReader->AddAttributes("AlignPtY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", ((OdGePoint3d) array[2]).x);
                    m_DwgReader->AddAttributes("PtMinX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", ((OdGePoint3d) array[2]).y);
                    m_DwgReader->AddAttributes("PtMinY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", ((OdGePoint3d) array[1]).x);
                    m_DwgReader->AddAttributes("PtMaxX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
                    tempstr.Format("%f", ((OdGePoint3d) array[1]).y);
                    m_DwgReader->AddAttributes("PtMaxY", tempstr, m_DwgReader->m_pTextFeatureBuffer);

                    OdGiTextStyle style;
                    giFromDbTextStyle(pText->textStyle(), style);
                    tempstr.Format("%s", style.ttfdecriptor().fileName().c_str());
                    m_DwgReader->AddAttributes("ShapeFilename", tempstr, m_DwgReader->m_pTextFeatureBuffer);

                    tempstr.Format("%s", style.bigFontFileName().c_str());
                    m_DwgReader->AddAttributes("BigFontname", tempstr, m_DwgReader->m_pTextFeatureBuffer);

                    tempstr.Format("%s", (OdDbSymbolTableRecordPtr(pText->textStyle().safeOpenObject()))->getName());
                    m_DwgReader->AddAttributes("ShapeName", tempstr, m_DwgReader->m_pTextFeatureBuffer);

                    HRESULT hr = m_DwgReader->m_pTextFeatureBuffer->putref_Shape(pShape);
                    if(SUCCEEDED(hr))
                    {
                        m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pAttr), "Annotation", m_DwgReader->m_pTextFeatureBuffer);
                        // ��ȡ��ͷ����Ϣ��ɫ
                        m_DwgReader->AddAttributes("Layer", m_DwgReader->m_szBlockLayer, m_DwgReader->m_pTextFeatureBuffer);
                        m_DwgReader->AddAttributes("Color", m_DwgReader->m_szBlockColor, m_DwgReader->m_pTextFeatureBuffer);
                        m_DwgReader->AddAttributes("Linetype", m_DwgReader->m_szBlockLT, m_DwgReader->m_pTextFeatureBuffer);
                        m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pTextFeatureBuffer);

                        //�������
                        if (m_DwgReader->CompareCodes(m_DwgReader->m_pTextFeatureBuffer))
                        {
                            hr = m_DwgReader->m_pTextFeatureCursor->InsertFeature(m_DwgReader->m_pTextFeatureBuffer, &(m_DwgReader->m_vID));
                            if (FAILED(hr))
                            {
                                sztemp = "Text����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                m_DwgReader->WriteLog(sztemp);
                            }
                        }
                    }
					else
					{
						sztemp = "Text�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
						m_DwgReader->WriteLog(sztemp);
					}
                }
            }
        }


        // ���ɵ�

		//���ԭ��buffer�е�����
		m_DwgReader->CleanAllFeatureBuffers();

		//����
        OdDbBlockReferencePtr pBlkRef_h = pEnt;
        m_DwgReader->AddAttributes("Blockname", m_DwgReader->m_szblockname, m_DwgReader->m_pPointFeatureBuffer);

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pGeometry;
        pPoint->PutCoords(pBlkRef_h->position().x, pBlkRef_h->position().y);
        pGeometry = pPoint;
        
        // Ŀǰֻ���趨��x����ı���������������ͬ��
        sztemp.Format("%f", pBlkRef_h->scaleFactors().sx);
        m_DwgReader->AddAttributes("Scale", sztemp, m_DwgReader->m_pPointFeatureBuffer);
        sztemp.Format("%f", pBlkRef_h->rotation());
        m_DwgReader->AddAttributes("Angle", sztemp, m_DwgReader->m_pPointFeatureBuffer);
        sztemp.Format("%f", pBlkRef_h->position().z);
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pPointFeatureBuffer);

        //�����ɢ�������ѡ���Ƿ�д������ڵĵ�
        if (m_DwgReader->m_IsBreakBlock && bExplode && (!m_DwgReader->m_IsReadBlockPoint))
        {
            return NULL;
        }

        return pGeometry.Detach();
    }
};

// �ھ��������в���һ����Ķ��ʵ��
class OdDbMInsertBlock_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        OdDbMInsertBlockPtr pPoly = pEnt;
        OdDbObjectIteratorPtr pIter = pPoly->attributeIterator();
        for (; !pIter->done(); pIter->step())
        {
            OdDbAttributePtr pAttr = pIter->entity();
            if (!pAttr.isNull())
            {
                OdGePoint3d pos = pAttr->position();
                sztemp.Format("����ʵ�壺 %s,%f,%f", pAttr->isA()->name(), pos.x, pos.y);
                m_DwgReader->WriteLog(sztemp);
            }
        }
        return 0;
    }
};

class OdDbSpline_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polyline);
        IGeometryPtr pShape;
        pShape = pPointColl;

        OdDbSplinePtr pSpline = pEnt;

        int degree;
        bool rational, closed, periodic;
        OdGePoint3dArray ctrlPts;
        OdGeDoubleArray weights;
        OdGeKnotVector knots;
        double tol;

        pSpline->getNurbsData(degree, rational, closed, periodic, ctrlPts, knots, weights, tol);
        DwgPoint* pts;
        pts = new DwgPoint[ctrlPts.size()];
        unsigned i;
        for (i = 0; i < ctrlPts.size(); i++)
        {
            pts[i].x = ctrlPts[i].x;
            pts[i].y = ctrlPts[i].y;
            pts[i].z = 0;
        }
        // bspline
        {
            int n, t;
            int resolution ;  // how many points our in our output array
            DwgPoint* out_pts;
            t = 4;
            if (ctrlPts.size() > 0)
            {
                n = ctrlPts.size() - 1;
                // 15���ǿ��Ǽ��ܵĵ��������Ը��ݻָ�������������
                resolution = n * 15;
                out_pts = new DwgPoint[resolution];
                m_DwgReader->Bspline(n, t, pts, out_pts, resolution);
                for (int i = 0; i < resolution; i++)
                {
                    pPoint->PutCoords(out_pts[i].x, out_pts[i].y);
                    pPointColl->AddPoint(pPoint, NULL, NULL);
                }
                delete out_pts;
            }
            delete pts;
        }
        return pShape.Detach();
    }
};

class OdDbEllipse_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        OdDbEllipsePtr pEll = pEnt;

        HRESULT hr;
        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;
        ISegmentCollectionPtr pSegColl(CLSID_Polyline);
        IEllipticArcPtr pElpArc(CLSID_EllipticArc);
        double semiMajor, minorMajorRatio, rotationAng, startangle, endangle, CentralAngle;
        startangle = pEll->startAngle();
        endangle = pEll->endAngle();
        minorMajorRatio = pEll->radiusRatio();
        CentralAngle = endangle - startangle;
        pPoint->PutCoords(pEll->center().x, pEll->center().y);
        // ��뾶
        semiMajor = pow(pow(pEll->majorAxis().x, 2) + pow(pEll->majorAxis().y, 2), 0.5);
        // ���� ��ת��,�������ת��ĳ�����ˮƽ��
        double offx, offy;
        offx = pEll->majorAxis().x;
        offy = pEll->majorAxis().y ;
        if (offx != 0)
        {
            rotationAng = atan(offy / offx);
        }
        else
        {
            if (offy > 0)
            {
                rotationAng = PI / 2;
            }
            else
                rotationAng = PI * 3 / 2;
        }
        if (offx < 0)
            rotationAng += PI;
        if (CentralAngle < 0)
            CentralAngle = 2 * PI + CentralAngle;
        hr = pElpArc->PutCoordsByAngle(VARIANT_TRUE, pPoint, startangle, CentralAngle, rotationAng, semiMajor, minorMajorRatio);
        pSegColl->AddSegment((ISegmentPtr) pElpArc);
        pShape = pSegColl;
        return pShape.Detach();
    }
};

// ��ʵ�����������κ��ı���
class OdDbSolid_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polygon);
        IGeometryPtr pShape;
        pShape = pPointColl;
        OdDbSolidPtr pSolid = pEnt;
        OdGePoint3d pt;
        pSolid->getPointAt(0, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pSolid->getPointAt(1, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pSolid->getPointAt(3, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pSolid->getPointAt(2, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pSolid->getPointAt(0, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        return pShape.Detach();
    }
};

// 3DFACE ����ά�ռ��е�����λ�ô���һ�����߻��ı�����
class OdDbFace_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polygon);
        IGeometryPtr pShape;
        pShape = pPointColl;
        OdDbFacePtr pFace = pEnt;
        OdGePoint3d pt;
        pFace->getVertexAt(0, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pFace->getVertexAt(1, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pFace->getVertexAt(2, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pFace->getVertexAt(3, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        pFace->getVertexAt(0, pt);
        pPoint->PutCoords(pt.x, pt.y);
        pPointColl->AddPoint(pPoint, NULL, NULL);
        return pShape.Detach();
    }
};

// �������Ķ���Ρ����ߵĶ˵��ڿ��ߵ��������ϣ��������Ǳ����гɾ���
class OdDbTrace_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        OdDbTracePtr pTrace = pEnt;
        OdGePoint3d pt;
        pTrace->getPointAt(0, pt);
        sztemp.Format("Traceʵ��δ����:  Point 0:%f,%f,%f", pt.x, pt.y, pt.z);
        m_DwgReader->WriteLog(sztemp);
        pTrace->getPointAt(1, pt);
        sztemp.Format("Traceʵ��δ����:  Point 1:%f,%f,%f", pt.x, pt.y, pt.z);
        m_DwgReader->WriteLog(sztemp);
        pTrace->getPointAt(2, pt);
        sztemp.Format("Traceʵ��δ����:  Point 2:%f,%f,%f", pt.x, pt.y, pt.z);
        m_DwgReader->WriteLog(sztemp);
        pTrace->getPointAt(3, pt);
        sztemp.Format("Traceʵ��δ����:  Point 3:%f,%f,%f", pt.x, pt.y, pt.z);
        m_DwgReader->WriteLog(sztemp);
        return 0;
    }
};

// �����塢Բ׶���Բ���嶼����άʵ�� (3DSolid) ����
class OdDb3dSolid_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        OdDb3dSolidPtr pSolid;

        dumpCommonData(pEnt);
        CString sztemp;
        pSolid = pEnt;
        m_DwgReader->WriteLog("3D Solidʵ��δ����");
        return 0;
    }
};

// ����ʵ�壬����ͨ��������ʾһЩ�����˵�����
class OdDbProxyEntity_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        OdDbEntity_Dumper::dump(pEnt);
        CString sztemp;

        OdDbProxyEntityPtr pProxy(pEnt);
        OdString satString;
        if (pProxy->originalClassName() == "AcAdPart" && odGetSatFromProxy(pProxy, satString))
        {
            sztemp.Format("ʵ��δ����:����ʵ���ļ���:%s", satString.c_str());
            m_DwgReader->WriteLog(sztemp);
        }
        return 0;
    }
};

class OdDbPolyline_Dumper : public OdDbEntity_Dumper
{
public:


    /********************************************************************
    ��Ҫ���� : �����߷����ͷ
    ������� :
    �� �� ֵ :
    ��    �� : 2008/06/27,BeiJing.
    ��    �� : 
    �޸���־ :
    *********************************************************************/
    void CalcLineArrow(OdDbEntity* pEnt, ISegmentCollectionPtr& pSegColl)
    {
        OdDbPolylinePtr pPoly = pEnt;
        if (pPoly.isNull())
        {
            return;
        }

        double startWidth, endWidth;
        pPoly->getWidthsAt(0, startWidth, endWidth);
        if (startWidth != endWidth)
        {
            if (pPoly->numVerts() < 2) return;

            //IPointPtr pt1, pt2;

            double x1, y1, x2, y2;

            OdGePoint3d pt;

            pPoly->getPointAt(0, pt);
            x1 = pt.x;
            y1 = pt.y;

            pPoly->getPointAt(1, pt);
            x2 = pt.x;
            y2 = pt.y;

            /*

            ��������A��(x1,y1)�Ĵ���
            1.����A(x1,y1),B(x2,y2��������ֱ��ƽ����x���y�ᣬҪ�õ������Ǻ����׵�
            if(x1==x2&&y1!=y2),��ֱ��ABƽ����y�ᣬ����Ϊy=y1��   ���ϵ�����Ϊ(x1,y1),(x3,y1),x3Ϊ������x1����һֵ
            if(x1!=x2&&y1==y2),��ֱ��ABƽ����x�ᣬ����Ϊx=x1,���ϵ�����Ϊ(x1,y1),(x1,y3),y3Ϊ������y1����һֵ
            2.��ֱ��AB��ƽ����x���y�ᣬ��
            б��k=(y2-y1)/(x2-x1)
            �䴹�ߵ�б��Ϊkv=-1/k
            ���ǿ��Լ���ֱ�ߵ�б��ʽ����Ϊ��y=kv*x+bv��bv�Ǵ�����y���ϵĽؾ�
            ��Ϊ���߹�A�㣨x1,y1)
            ������y1=kv*x1+bv,��bv=y1-kv*x1=y1+x1/k
            ������ô��ߵķ���Ϊ��y=-x/k+(y1+x1/k)
            ��Ȼ�����߹�A��x1,y1),ֻ����ȡһ��C��x3,y3),
            ����ȡx3=x1+2,y3=-x3/k+(y1+x1/k)
            ��ֱ��AC����
            */

            double pi = 3.1415926;

            double k = (y2 - y1) / (x2 - x1);

            double a = pi / 6;

            double k2 = -1.0 / k;
            double bv2 = y2 - k2 * x2;

            IPointPtr ptA(CLSID_Point);
            ptA->PutCoords(x1, y1);

            //tana = (k3 - k1)/( 1+ k1k3)

            double tan_a = tanf(a);
            double k3 = (tan_a + k) / (1 - tan_a * k);
            double bv3 = y1 - k3 * x1;

            double x3, y3;

            x3 = (bv3 - bv2) / (k2 - k3) ;
            y3 = k3 * x3 + bv3;

            ILinePtr pLineAC(CLSID_Line);
            IPointPtr ptC(CLSID_Point);
            ptC->PutCoords(x3, y3);
            pLineAC->PutCoords(ptA, ptC);

            IPolylinePtr pPolyL_AC(CLSID_Polyline);
            ISegmentCollectionPtr pPolyLSeg_AC = pPolyL_AC;
            pPolyLSeg_AC->AddSegment((ISegmentPtr)pLineAC);

            AddArrow(pEnt, pPolyLSeg_AC);
            //////////////////////////////////////////////////////////////////////////

            double b = pi - a;
            double tan_b = tanf(b);
            double k4 = (tan_b + k) / (1 - tan_b * k);
            double bv4 = y1 - k4 * x1;
            double x4, y4;

            x4 = (bv4 - bv2) / (k2 - k4) ;

            y4 = k4 * x4 + bv4;

            ILinePtr pLineAD(CLSID_Line);
            IPointPtr ptD(CLSID_Point);
            ptD->PutCoords(x4, y4);
            pLineAD->PutCoords(ptA, ptD);
            IPolylinePtr pPolyL_AD(CLSID_Polyline);
            ISegmentCollectionPtr pPolyLSeg_AD = pPolyL_AD;
            pPolyLSeg_AD->AddSegment((ISegmentPtr)pLineAD);

            AddArrow(pEnt, pPolyLSeg_AD);

        }

    }


    void AddArrow(OdDbEntity* pEnt, IGeometryPtr pGeoShape)
    {
        HRESULT hr;

        CString sInfoText;
        IFeatureBuffer* pFeatBuffer;
        m_DwgReader->m_pFeatClassLine->CreateFeatureBuffer(&pFeatBuffer);



        hr = pFeatBuffer->putref_Shape(pGeoShape);
        if (FAILED(hr))
        {

            sInfoText = "Line�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
            m_DwgReader->WriteLog(sInfoText);
            m_DwgReader->m_lUnReadEntityNum++;
        }
        else
        {
            m_DwgReader->AddBaseAttributes(pEnt, "Line", pFeatBuffer);

            CString sDwgLayer;
            sDwgLayer.Format("%s", pEnt->layer().c_str());
            //sDwgLayer.MakeUpper();

            //m_DwgReader->RenameEntityLayerName(sDwgLayer, pFeatBuffer);

            if (m_DwgReader->CompareCodes(pFeatBuffer))
            {
                VARIANT OID;
                hr = m_DwgReader->m_pLineFeatureCursor->InsertFeature(pFeatBuffer, &OID);
                if (FAILED(hr))
                {
                    sInfoText = "Line����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sInfoText);
                    m_DwgReader->m_lUnReadEntityNum++;
                }
            }
        }
    }


    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl;
        ISegmentCollectionPtr pSegColl(CLSID_Polyline);
        IGeometryPtr pShape;
        pShape = pSegColl;
        pPointColl = pSegColl;

        OdDbPolylinePtr pPoly = pEnt;
        double width = 0;
        width = pPoly->getConstantWidth();
        for (unsigned int i = 0; i < pPoly->numVerts(); i++)
        {
            OdGePoint3d pt;
            pPoly->getPointAt(i, pt);
            pPoint->PutCoords(pt.x, pt.y);
            pPointColl->AddPoint(pPoint, NULL, NULL);
            double bulge = pPoly->getBulgeAt(i);
            // �����в�ͬ�߿�Ĵ�ɢ�����������������߿���յ��߿�δ����
            /*
            						if (pPoly->hasWidth())
            						{
            							pPoly->getWidthsAt(i,width,width);
            						}
            						*/
            if (pPoly->segType(i) == OdDbPolyline::kArc)
            {
                if (i < (pPoly->numVerts() - 1) || pPoly->isClosed())
                {
                    OdGeCircArc3d arc;
                    pPoly->getArcSegAt(i, arc);
                    OdGePoint3d start, end, cent;
                    cent = arc.center();
                    start = arc.startPoint();
                    end = arc.endPoint();

                    ICircularArcPtr pCircularArc(CLSID_CircularArc);
                    IPointPtr pFromPoint(CLSID_Point);
                    IPointPtr pToPoint(CLSID_Point);
                    IPointPtr pCtlPoint(CLSID_Point);
                    pCtlPoint->PutCoords(cent.x, cent.y);
                    pFromPoint->PutCoords(start.x, start.y);
                    pToPoint->PutCoords(end.x, end.y);
                    if (bulge >= 0)
                        pCircularArc->PutCoords(pCtlPoint, pFromPoint, pToPoint, esriArcCounterClockwise);
                    else
                        pCircularArc->PutCoords(pCtlPoint, pFromPoint, pToPoint, esriArcClockwise);
                    pSegColl->AddSegment((ISegmentPtr) pCircularArc);
                }
            }
        }
        if (pPoly->isClosed())
        {
            pPointColl->get_Point(0, &pPoint);
            pPointColl->AddPoint(pPoint, NULL, NULL);
        }

        CalcLineArrow(pEnt, pSegColl);

        // ͬʱ����������
        sztemp.Format("%f", pPoly->elevation());
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Elevation", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        sztemp.Format("%.f", pPoly->thickness());
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        sztemp.Format("%f", width);
        m_DwgReader->AddAttributes("Width", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Width", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        return pShape.Detach();
    }
};

class OdDbHatch_Dumper : public OdDbEntity_Dumper
{
private:
    IRingPtr dumpPolylineType(int loopIndex, OdDbHatchPtr& pHatch)
    {
        OdGePoint2dArray vertices;
        OdGeDoubleArray bulges;
        CString sztemp;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl;
        IRingPtr pRing(CLSID_Ring);
        ISegmentCollectionPtr pSegColl;
        pSegColl = pRing;
        pPointColl = pSegColl;

        pHatch->getLoopAt(loopIndex, vertices, bulges);
        bool hasbulges = vertices.size() == bulges.size() ? true : false ;
        OdGeDoubleArray::const_iterator blgs = bulges.begin();
        double bulge;

        // ����bulgeֵ
        IPointCollectionPtr pBugColl(CLSID_Polyline);
        // ����line���е�
        IPointCollectionPtr pLineColl(CLSID_Polyline);
        for (OdGePoint2dArray::const_iterator verts = vertices.begin(); vertices.end() != verts; ++verts)
        {
            if (hasbulges)
            {
                bulge = *blgs;
                blgs++;
            }
            else
                bulge = 0.0;

            pPoint->PutCoords((*verts).x, (*verts).y);
            pLineColl->AddPoint(pPoint, NULL, NULL);
            pPoint->PutCoords(4.0 * atan(bulge), 0);
            pBugColl->AddPoint(pPoint, NULL, NULL);
        }
        long pointsNum = 0;
        pLineColl->get_PointCount(&pointsNum);
        double incl;
        for (int iarc = 0; iarc < pointsNum; iarc++)
        {
            pBugColl->get_Point(iarc, &pPoint);
            pPoint->get_X(&incl);
            pLineColl->get_Point(iarc, &pPoint);
            pPointColl->AddPoint(pPoint, NULL, NULL);
            if (incl != 0 && iarc < pointsNum - 1)
            {
                double from[2], to[2];
                IPointPtr pFromPoint(CLSID_Point);
                IPointPtr pToPoint(CLSID_Point);
                IPointPtr pCtlPoint(CLSID_Point);
                pLineColl->get_Point(iarc, &pFromPoint);
                pLineColl->get_Point(iarc + 1, &pToPoint);
                pFromPoint->get_X(&from[0]);
                pFromPoint->get_Y(&from[1]);
                pToPoint->get_X(&to[0]);
                pToPoint->get_Y(&to[1]);

                IConstructCircularArcPtr pConstructArc(CLSID_CircularArc);
                if (incl >= 0)
                    pConstructArc->ConstructEndPointsAngle(pFromPoint, pToPoint, esriArcCounterClockwise, incl);
                else
                    pConstructArc->ConstructEndPointsAngle(pFromPoint, pToPoint, esriArcClockwise, incl);
                pSegColl->AddSegment((ISegmentPtr) pConstructArc);
            }
        }
        return pRing;
    }

    IEllipticArcPtr dumpEllipticedge(OdGeEllipArc2d* pEllipArc)
    {
        if (pEllipArc == NULL)
            return 0;

        HRESULT hr;
        CString sztemp;
        IPointPtr pCenter(CLSID_Point);
        IPointPtr pFrom(CLSID_Point);
        IPointPtr pTo(CLSID_Point);
        IEllipticArcPtr pElpArc(CLSID_EllipticArc);

        pCenter->PutCoords(pEllipArc->center().x, pEllipArc->center().y);
        pFrom->PutCoords(pEllipArc->startPoint().x, pEllipArc->startPoint().y);
        pTo->PutCoords(pEllipArc->endPoint().x, pEllipArc->endPoint().y);

        double semiMajor, minorMajorRatio, rotationAng, startangle, endangle, CentralAngle;
        minorMajorRatio = pEllipArc->minorRadius() / pEllipArc->majorRadius();
        startangle = angleFromParam(pEllipArc->startAng(), minorMajorRatio);
        endangle = angleFromParam(pEllipArc->endAng(), minorMajorRatio);
        CentralAngle = endangle - startangle;
        pCenter->PutCoords(pEllipArc->center().x, pEllipArc->center().y);
        semiMajor = pEllipArc->majorRadius();
        // ���� ��ת��,�������ת��ĳ�����ˮƽ��
        double offx, offy;
        offx = pEllipArc->majorAxis().x;
        offy = pEllipArc->majorAxis().y ;
        if (offx != 0)
        {
            rotationAng = atan(offy / offx);
        }
        else
        {
            if (offy > 0)
            {
                rotationAng = PI / 2;
            }
            else
                rotationAng = PI * 3 / 2;
        }
        // rotationAng����
        //hr=pElpArc->PutCoordsByAngle(VARIANT_TRUE,pCenter,startangle,CentralAngle,rotationAng,semiMajor,minorMajorRatio);

        if (pEllipArc->isClockWise())
            hr = pElpArc->PutCoords(VARIANT_FALSE, pCenter, pFrom, pTo, rotationAng, minorMajorRatio, esriArcClockwise);
        else
        {
            rotationAng += PI;
            hr = pElpArc->PutCoords(VARIANT_FALSE, pCenter, pFrom, pTo, rotationAng, minorMajorRatio, esriArcCounterClockwise);
        }
        if (FAILED(hr))
        {
            sztemp = "Ҫ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
            m_DwgReader->WriteLog(sztemp);
        }
        return pElpArc;
    }

    ILinePtr dumpLineSeg2d(OdGeLineSeg2d* pLineSeg)
    {
        if (pLineSeg == NULL)
            return 0 ;
        ILinePtr pLine(CLSID_Line);
        IPointPtr pPoint(CLSID_Point);
        pPoint->PutCoords(pLineSeg->startPoint().x, pLineSeg->startPoint().y);
        pLine->put_FromPoint(pPoint);
        pPoint->PutCoords(pLineSeg->endPoint().x, pLineSeg->endPoint().y);
        pLine->put_ToPoint(pPoint);
        return pLine;
    }

    static ICircularArcPtr dumpCircArc(OdGeCircArc2d* pCircArc)
    {
        if (pCircArc == NULL)
            return 0 ;

        IPointPtr pCenter(CLSID_Point);
        IPointPtr pFrom(CLSID_Point);
        IPointPtr pTo(CLSID_Point);
        ICircularArcPtr pCircularArc(CLSID_CircularArc);
        pCenter->PutCoords(pCircArc->center().x, pCircArc->center().y);
        pFrom->PutCoords(pCircArc->startPoint().x, pCircArc->startPoint().y);
        pTo->PutCoords(pCircArc->endPoint().x, pCircArc->endPoint().y);
        if (pCircArc->isClockWise())
            pCircularArc->PutCoords(pCenter, pFrom, pTo, esriArcClockwise);
        else
            pCircularArc->PutCoords(pCenter, pFrom, pTo, esriArcCounterClockwise);
        return pCircularArc;
    }
    IGeometryPtr dumpNurbCurve2d(OdGeNurbCurve2d* pNurbCurve)
    {
        if (pNurbCurve == NULL)
            return 0 ;

        IPointPtr pPoint(CLSID_Point);
        IPointCollectionPtr pPointColl(CLSID_Polyline);
        IGeometryPtr pShape;
        pShape = pPointColl;

        int degree;
        bool rational, periodic;
        OdGePoint2dArray ctrlPts;
        OdGeDoubleArray weights;
        OdGeKnotVector knots;

        pNurbCurve->getDefinitionData(degree, rational, periodic, knots, ctrlPts, weights);
        DwgPoint* pts;
        pts = new DwgPoint[ctrlPts.size()];
        unsigned i;
        for (i = 0; i < ctrlPts.size(); i++)
        {
            pts[i].x = ctrlPts[i].x;
            pts[i].y = ctrlPts[i].y;
            pts[i].z = 0;
        }
        // bspline
        {
            int n, t;
            int resolution ;
            DwgPoint* out_pts;
            t = 4;
            if (ctrlPts.size() > 0)
            {
                n = ctrlPts.size() - 1;
                resolution = n * 15;
                out_pts = new DwgPoint[resolution];
                m_DwgReader->Bspline(n, t, pts, out_pts, resolution);
                for (int i = 0; i < resolution; i++)
                {
                    pPoint->PutCoords(out_pts[i].x, out_pts[i].y);
                    pPointColl->AddPoint(pPoint, NULL, NULL);
                }
                delete out_pts;
            }
            delete pts;
        }
        return pShape;
    }

    IRingPtr dumpEdgesType(int loopIndex, OdDbHatchPtr& pHatch)
    {
        CString sztemp;
        EdgeArray edges;

        pHatch->getLoopAt(loopIndex, edges);

        HRESULT hr;
        IRingPtr pRing(CLSID_Ring);
        ISegmentCollectionPtr pSegColl;
        pSegColl = pRing;
        for (EdgeArray::const_iterator edge = edges.begin(); edge != edges.end(); ++edge)
        {
            ISegmentPtr pSeg;
            OdGeCurve2d* pEdge = *edge;
            switch (pEdge->type())
            {
            case OdGe::kLineSeg2d:
            {
                OdGeLineSeg2d* pLineSeg = (OdGeLineSeg2d*) pEdge;
                pSeg = dumpLineSeg2d(pLineSeg);
                pSegColl->AddSegment(pSeg);
            }
            break;
            case OdGe::kCircArc2d:
            {
                OdGeCircArc2d* pCircArc = (OdGeCircArc2d*) pEdge;
                pSeg = dumpCircArc(pCircArc);
                pSegColl->AddSegment(pSeg);
            }
            break;
            case OdGe::kEllipArc2d:
            {
                OdGeEllipArc2d* pEllipArc = (OdGeEllipArc2d*) pEdge;
                pSeg = dumpEllipticedge(pEllipArc);
                pSegColl->AddSegment(pSeg);
            }
            break;
            case OdGe::kNurbCurve2d:
            {
                OdGeNurbCurve2d* pNurbCurve = (OdGeNurbCurve2d*) pEdge;
                IGeometryPtr pGeometry;
                pGeometry = dumpNurbCurve2d(pNurbCurve);
                hr = pSegColl->AddSegmentCollection(ISegmentCollectionPtr(pGeometry));
                if (FAILED(hr))
                {
                    sztemp = "Ҫ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
                break;
            }
            break;
            }
        }
        return pRing;
    }

public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        CString sztemp;

        HRESULT hr;
        IGeometryCollectionPtr ipGeometryCollection(CLSID_Polygon);
        OdDbHatchPtr pHatch = pEnt;
        if (!pHatch->numLoops())
        {
            return 0;
        }
        for (int i = 0 ; i < pHatch->numLoops() ; i++)
        {
            OdInt32 loopType = pHatch->loopTypeAt(i);

            if (loopType & OdDbHatch::kExternal)
            {
                sztemp = " External";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kDerived)
            {
                sztemp = " Derived";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kTextbox)
            {
                sztemp = " Textbox";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kOutermost)
            {
                sztemp = " Outermost";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kNotClosed)
            {
                sztemp = " NotClosed";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kSelfIntersecting)
            {
                sztemp = " SelfIntersecting";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kTextIsland)
            {
                sztemp = " TextIsland";
                //m_DwgReader->WriteLog(sztemp);
            }
            if (loopType & OdDbHatch::kPolyline)
            {
                hr = ipGeometryCollection->AddGeometry(IGeometryPtr(dumpPolylineType(i, pHatch)));
                if (FAILED(hr))
                {
                    sztemp = "hatch.PolylineҪ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
            }
            else
            {
                hr = ipGeometryCollection->AddGeometry(IGeometryPtr(dumpEdgesType(i, pHatch)));
                if (FAILED(hr))
                {
                    sztemp = "hatch.edgesҪ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
            }
        }
        IGeometryPtr pShape;
        pShape = ipGeometryCollection;
        return pShape.Detach();
    }
};

//OdDbArc
class OdDbArc_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        CString sztemp;
        OdDbArcPtr pArc = pEnt;

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;
        ISegmentCollectionPtr pSegColl(CLSID_Polyline);
        ICircularArcPtr pCircularArc(CLSID_CircularArc);
        pPoint->PutCoords(pArc->center().x, pArc->center().y);
        double start, end;
        pArc->getStartParam(start);
        pArc->getEndParam(end);
        double CentralAngle;
        CentralAngle = end - start;
        pCircularArc->PutCoordsByAngle(pPoint, start, CentralAngle, pArc->radius());
        pSegColl->AddSegment((ISegmentPtr) pCircularArc);
        pShape = pSegColl;

        sztemp.Format("%.f", pArc->thickness());
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        return pShape.Detach();
    }
};

class OdDbCircle_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        CString sztemp;
        OdDbCirclePtr pDbCircle = pEnt;

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;
        ISegmentCollectionPtr pSegColl(CLSID_Polyline);
        IConstructCircularArcPtr pCircle(CLSID_CircularArc);
        pPoint->PutCoords(pDbCircle->center().x, pDbCircle->center().y);
        pCircle->ConstructCircle(pPoint, pDbCircle->radius(), VARIANT_FALSE);
        pSegColl->AddSegment((ISegmentPtr) pCircle);
        pShape = pSegColl;
        sztemp.Format("%.f", pDbCircle->thickness());
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pLineFeatureBuffer);
        m_DwgReader->AddAttributes("Thickness", sztemp, m_DwgReader->m_pPolygonFeatureBuffer);
        return pShape.Detach();
    }
};

// �����Ǵӱպϵ��λ򻷴����Ķ�ά����
// �պ϶���ߡ�ֱ�ߺ����߶�����Ч��ѡ�����
// ���߰���Բ����Բ����Բ������Բ���������ߡ�
// ��ɢ�������
class OdDbRegion_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        OdDbRegionPtr pRegion = pEnt;
        HRESULT hr;
        CString sztemp;

        IGeometryCollectionPtr ipGeometryCollection(CLSID_GeometryBag);
        // ��ɢ�������
        OdRxObjectPtrArray pEntSet;
        // Ϊ�� explode������?
        OdResult result = pRegion->explode(pEntSet);
        if (result != eOk)
            return 0;
        for (unsigned i = 0; i < pEntSet.size(); ++i)
        {
            OdSmartPtr<OdDbEntity_Dumper> pEntDumper = OdDbEntityPtr(pEntSet[i]);
            IGeometryPtr pShape;
            pEntDumper->m_DwgReader = m_DwgReader;
            pShape = pEntDumper->dump(OdDbEntityPtr(pEntSet[i]));
            if (pShape)
            {
                hr = m_DwgReader->m_pLineFeatureBuffer->putref_Shape(pShape);
                if (FAILED(hr))
                {
                    sztemp = "Line��ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
                m_DwgReader->AddBaseAttributes(pEnt, "Line", m_DwgReader->m_pLineFeatureBuffer);

                //�������
                if (m_DwgReader->CompareCodes(m_DwgReader->m_pLineFeatureBuffer))
                {
                    m_DwgReader->m_pLineFeatureCursor->InsertFeature(m_DwgReader->m_pLineFeatureBuffer, &(m_DwgReader->m_vID));
                }

                ipGeometryCollection->AddGeometry(pShape, NULL, NULL);
            }
        }

        IEnumGeometryPtr pEnumGeom(ipGeometryCollection);
        ITopologicalOperatorPtr pTopo(CLSID_Polyline);
        hr = pTopo->ConstructUnion(pEnumGeom);
        hr = pTopo->Simplify();
        IPolygonPtr pPolygon(CLSID_Polygon);
        ((IPointCollectionPtr) pPolygon)->SetPointCollection((IPointCollectionPtr) pTopo);
        IGeometryPtr pGeometry;
        pGeometry = pPolygon;

        OdWrFileBuf ow("RegionAcisOut.acis");
        pRegion->acisOut(&ow, kAfTypeASCII);
        return pGeometry.Detach();
    }
};

class OdDbMText_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        OdDbMTextPtr pMText = pEnt;
        OdGePoint3dArray array;
        pMText->getBoundingPoints(array);

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;
        pShape = pPoint;
        sztemp.Format("      %f,%f", pMText->location().x, pMText->location().y);
        pPoint->PutCoords(pMText->location().x, pMText->location().y);

        //��Ϊ�������
        if (m_DwgReader->m_IsCreateAnnotation == FALSE)
        {
            CString tempstr;
            tempstr.Format("%s", pMText->contents().c_str());
            // ��ȥ��Mtextǰ�������
            // �磺{\f����ϸ���߼���|b0|i0|c134|p2;�½�ά���������}
            {
                int index;
                index = tempstr.Find(';');
                int ilength = tempstr.GetLength();
                tempstr = tempstr.Right(ilength - 1 - index);
                index = tempstr.ReverseFind('}');
                if (index != -1)
                    tempstr = tempstr.Left(index);
            }
            m_DwgReader->AddAttributes("TextString", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pMText->textHeight());
            m_DwgReader->AddAttributes("Height", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pMText->rotation());
            m_DwgReader->AddAttributes("Angle", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pMText->width() / pMText->textHeight());
            m_DwgReader->AddAttributes("WidthFactor", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", "0");
            m_DwgReader->AddAttributes("Oblique", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%d", pMText->horizontalMode());
            m_DwgReader->AddAttributes("HzMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%d", pMText->verticalMode());
            m_DwgReader->AddAttributes("VtMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%f", pMText->location().x);
            m_DwgReader->AddAttributes("AlignPtX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pMText->location().y);
            m_DwgReader->AddAttributes("AlignPtY", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%f", ((OdGePoint3d) array[2]).x);
            m_DwgReader->AddAttributes("PtMinX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[2]).y);
            m_DwgReader->AddAttributes("PtMinY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[1]).x);
            m_DwgReader->AddAttributes("PtMaxX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", ((OdGePoint3d) array[1]).y);
            m_DwgReader->AddAttributes("PtMaxY", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            OdGiTextStyle style;
            giFromDbTextStyle(pMText->textStyle(), style);
            tempstr.Format("%s", style.ttfdecriptor().fileName().c_str());
            m_DwgReader->AddAttributes("ShapeFilename", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", style.bigFontFileName().c_str());
            m_DwgReader->AddAttributes("BigFontname", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", (OdDbSymbolTableRecordPtr(pMText->textStyle().safeOpenObject()))->getName());
            m_DwgReader->AddAttributes("ShapeName", tempstr, m_DwgReader->m_pTextFeatureBuffer);
        }
        return pShape.Detach();
    }
};

class OdDbMline_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        CString sztemp;
        OdDbMlinePtr pMline = pEnt;
        HRESULT hr;
        IGeometryPtr pShape;
        IPointPtr pPoint(CLSID_Point);
        IGeometryCollectionPtr ipGeometryCollection;

        //��ɢ���
        OdRxObjectPtrArray entitySet;
        OdResult result = pMline->explode(entitySet);
        int numMline = pMline->numVertices();

        // ���ݵ�������2Ϊpline�ߡ�
        if (numMline > 2)
        {
            ipGeometryCollection.CreateInstance(CLSID_Polyline);
            for (unsigned i = 0; i < entitySet.size(); ++i)
            {
                IPointCollectionPtr pPointColl(CLSID_Path);
                pShape = pPointColl;
                OdDb2dPolylinePtr pPline = OdDb2dPolyline::cast(entitySet[i]);
                if (!pPline.isNull())
                {
                    OdDbObjectIteratorPtr pIter = pPline->vertexIterator();
                    for (; !pIter->done(); pIter->step())
                    {
                        OdDb2dVertexPtr pVertex = pIter->entity();
                        if (pVertex.get())
                        {
                            OdGePoint3d pos = pVertex->position();
                            if (pVertex.get())
                            {
                                pPoint->PutCoords(pos.x, pos.y);
                                pPointColl->AddPoint(pPoint, NULL, NULL);
                            }
                        }
                    }
                    if (pPline->isClosed())
                    {
                        pPointColl->get_Point(0, &pPoint);
                        pPointColl->AddPoint(pPoint, NULL, NULL);
                    }
                }
                hr = ipGeometryCollection->AddGeometry(pShape, NULL, NULL);
                if (FAILED(hr))
                {
                    sztemp = "Ҫ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
            }
        }
        else if (numMline == 2)
        {
            // ���2�������Line
            for (unsigned i = 0; i < entitySet.size(); ++i)
            {
                OdDbLinePtr pLine = OdDbLine::cast(entitySet[i]);

                OdGePoint3d pos1, pos2 ;
                pLine->getStartPoint(pos1);
                pLine->getEndPoint(pos2);
                ipGeometryCollection.CreateInstance(CLSID_Polyline);
                IPointCollectionPtr pPointColl(CLSID_Path);
                pShape = pPointColl;
                pPoint->PutCoords(pos1.x, pos1.y);
                pPointColl->AddPoint(pPoint, NULL, NULL);
                pPoint->PutCoords(pos2.x, pos2.y);
                pPointColl->AddPoint(pPoint, NULL, NULL);

                hr = ipGeometryCollection->AddGeometry(pShape, NULL, NULL);
                if (FAILED(hr))
                {
                    sztemp = "Ҫ�ض�ȡʧ��" + m_DwgReader->CatchErrorInfo();
                    m_DwgReader->WriteLog(sztemp);
                }
            }
        }
        IGeometryPtr pGeometry;
        pGeometry = ipGeometryCollection;

        return pGeometry.Detach();
    }
};

// ���Ʋο�ͼ�񣬿�����ɨ��ʸ����
class OdDbRasterImage_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        OdDbRasterImagePtr pImage = pEnt;
        OdGePoint3d origin;
        OdGeVector3d u, v, u2, v2;
        pImage->getOrientation(origin, u, v);
        u.normalize();
        v.normalize();
        int i = 4;
        return 0;
    }
};

// ��ע�Ķ�������
class OdDbArcAlignedText_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        OdDbArcAlignedTextPtr pAAT = pEnt;
        return 0;
    }
};

// �����ⲿ�Ŀؼ�����excelͼ��
class OdDbOle2Frame_Dumper : public OdDbEntity_Dumper
{
public:
    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        OdDbOle2FramePtr pOle = pEnt;

        CString sztemp;
        int type = pOle->getType();

        OdString str = pOle->getUserType();
        m_DwgReader->WriteLog("===== �����ⲿ�ؼ�ʵ�壺OLE2FRAME entity data =====");

        sztemp.Format("      type = %s user type string", str.c_str());
        m_DwgReader->WriteLog("===== OLE2FRAME entity data =====");
        return 0;
    }
};

// �ֿ��ļ�
class OdDbShape_Dumper : public OdDbEntity_Dumper
{
public:
    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);
        OdDbShapePtr pDbShape = pEnt;

        IPointPtr pPoint(CLSID_Point);
        IGeometryPtr pShape;

        pPoint->PutCoords(pDbShape->position().x, pDbShape->position().y);
        pShape = pPoint;

        //�����ı��㣬��Ϊ�������
        if (m_DwgReader->m_IsCreateAnnotation == FALSE)
        {
            CString tempstr;
            // shapenumberΪ�ֵ����ݣ���Ҫ�������ο������ձ��滻
            tempstr.Format("%d", pDbShape->shapeNumber());
            m_DwgReader->AddAttributes("TextString", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->size());
            m_DwgReader->AddAttributes("Height", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->rotation());
            m_DwgReader->AddAttributes("Angle", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->widthFactor());
            m_DwgReader->AddAttributes("WidthFactor", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->oblique());
            m_DwgReader->AddAttributes("Oblique", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%s", "1");
            m_DwgReader->AddAttributes("HzMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%s", "0");
            m_DwgReader->AddAttributes("VtMode", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%.f", pDbShape->thickness());
            m_DwgReader->AddAttributes("Thickness", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%f", pDbShape->position().x);
            m_DwgReader->AddAttributes("AlignPtX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->position().y);
            m_DwgReader->AddAttributes("AlignPtY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->position().x);
            m_DwgReader->AddAttributes("PtMinX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->position().y);
            m_DwgReader->AddAttributes("PtMinY", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->position().x);
            m_DwgReader->AddAttributes("PtMaxX", tempstr, m_DwgReader->m_pTextFeatureBuffer);
            tempstr.Format("%f", pDbShape->position().y);
            m_DwgReader->AddAttributes("PtMaxY", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            OdGiTextStyle style;
            giFromDbTextStyle(pDbShape->styleId(), style);
            tempstr.Format("%s", style.ttfdecriptor().fileName().c_str());
            m_DwgReader->AddAttributes("ShapeFilename", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", style.bigFontFileName().c_str());
            m_DwgReader->AddAttributes("BigFontname", tempstr, m_DwgReader->m_pTextFeatureBuffer);

            tempstr.Format("%s", pDbShape->name());
            m_DwgReader->AddAttributes("ShapeName", tempstr, m_DwgReader->m_pTextFeatureBuffer);
        }

        return pShape.Detach();
    }
};

/********************************************************************
��Ҫ���� : ��Dimension�Ĵ���
������� :
�� �� ֵ :
��    �� : 2008/05/27,BeiJing.
��    �� : 
�޸���־ :
*********************************************************************/
class OdDbDimension_Dumper : public OdDbEntity_Dumper
{
public:

    IGeometry* dump(OdDbEntity* pEnt)
    {
        dumpCommonData(pEnt);

        CString sDwgLayer;
        sDwgLayer.Format("%s", pEnt->layer().c_str());
        sDwgLayer.MakeUpper();

        OdRxObjectPtrArray pEntSet;
        OdResult odr =  pEnt->explodeGeometry(pEntSet);
        if (odr == eOk)
        {
            unsigned iSize = pEntSet.size();
            for (unsigned i = 0; i < iSize; ++i)
            {
                OdRxObjectPtr pExplodedEnt = pEntSet[i];

                m_DwgReader->CleanAllFeatureBuffers();
                OdSmartPtr<OdDbEntity_Dumper> pEntDumper = OdDbEntityPtr(pExplodedEnt);
                HRESULT hr;
                CString sztemp;
                IGeometryPtr pShape;
                pEntDumper->m_DwgReader = m_DwgReader;
                pShape = pEntDumper->dump(OdDbEntityPtr(pExplodedEnt));

                if (pShape != NULL)
                {
                    esriGeometryType shapeType;
                    pShape->get_GeometryType(&shapeType);

                    //////////////////////////////////////////////////////////////////////////
                    //������
                    if (shapeType == esriGeometryPolyline)
                    {
                        hr = m_DwgReader->m_pLineFeatureBuffer->putref_Shape(pShape);
                        if (FAILED(hr))
                        {
                            sztemp = "Line�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
                            m_DwgReader->WriteLog(sztemp);
                        }
                        else
                        {
                            m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pExplodedEnt), "Line", m_DwgReader->m_pLineFeatureBuffer);

                            //�������
                            if (m_DwgReader->CompareCodes(m_DwgReader->m_pLineFeatureBuffer))
                            {
                                hr = m_DwgReader->m_pLineFeatureCursor->InsertFeature(m_DwgReader->m_pLineFeatureBuffer, &(m_DwgReader->m_vID));
                                if (FAILED(hr))
                                {
                                    sztemp = "Line����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                    m_DwgReader->WriteLog(sztemp);
                                }
                            }
                        }
                    }

                }

                /////////////////////////////////////////////////////////////////////
                //����ע�Ƕ���
                CString sEntType = OdDbEntityPtr(pExplodedEnt)->isA()->name();
                if (sEntType.Compare("AcDbMText") == 0 || sEntType.Compare("AcDbText") == 0)
                {
                    if (m_DwgReader->m_IsCreateAnnotation)
                    {
                        m_DwgReader->InsertAnnoFeature(pExplodedEnt);
                    }
                    else
                    {
                        hr = m_DwgReader->m_pTextFeatureBuffer->putref_Shape(pShape);
                        if (FAILED(hr))
                        {
                            sztemp = "Text�������겻��ȷ��" + m_DwgReader->CatchErrorInfo();
                            m_DwgReader->WriteLog(sztemp);
                            m_DwgReader->m_lUnReadEntityNum++;
                        }
                        else
                        {
                            m_DwgReader->AddBaseAttributes(OdDbEntityPtr(pExplodedEnt), "Annotation", m_DwgReader->m_pTextFeatureBuffer);

                            //�������
                            if (m_DwgReader->CompareCodes(m_DwgReader->m_pTextFeatureBuffer))
                            {
                                hr = m_DwgReader->m_pTextFeatureCursor->InsertFeature(m_DwgReader->m_pTextFeatureBuffer, &(m_DwgReader->m_vID));
                                if (FAILED(hr))
                                {
                                    sztemp = "Text����д�뵽PGDBʧ�ܡ�" + m_DwgReader->CatchErrorInfo();
                                    m_DwgReader->WriteLog(sztemp);
                                    m_DwgReader->m_lUnReadEntityNum++;
                                }
                            }
                        }
                    }

                }
                else if (sEntType.Compare("AcDbLine") == 0)
                {
                    //�Ѵ���
                }
                else if (sEntType.Compare("AcDbBlockReference") == 0)
                {
                }
                else
                {
                }
            }
        }

        return 0;
    }
};

ExProtocolExtension::ExProtocolExtension()
{
}

ExProtocolExtension::~ExProtocolExtension()
{
    if (m_pDumpers)
        uninitialize();
}

class Dumpers
{
    OdStaticRxObject<OdDbEntity_Dumper> m_entityDumper;
    OdStaticRxObject<OdDbRegion_Dumper> m_regionDumper;
    OdStaticRxObject<OdDbPolyline_Dumper> m_polylineDumper;
    OdStaticRxObject<OdDbLine_Dumper> m_LineDumper;
    OdStaticRxObject<OdDbPoint_Dumper> m_pointDumper;
    OdStaticRxObject<OdDbArc_Dumper> m_ArcDumper;
    OdStaticRxObject<OdDb2dPolyline_Dumper> m_2dPolylineDumper;
    OdStaticRxObject<OdDb3dPolyline_Dumper> m_3dPolylineDumper;
    OdStaticRxObject<OdDbPolyFaceMesh_Dumper> m_polyFaceMeshDumper;
    OdStaticRxObject<OdDbPolygonMesh_Dumper> m_polygonMesh;
    OdStaticRxObject<OdDbBlockReference_Dumper> m_blockReference;
    OdStaticRxObject<OdDbMInsertBlock_Dumper> m_mInsertBlock;
    OdStaticRxObject<OdDbSpline_Dumper> m_splineDumper;
    OdStaticRxObject<OdDbEllipse_Dumper> m_ellipseDumper;
    OdStaticRxObject<OdDbSolid_Dumper> m_solidDumper;
    OdStaticRxObject<OdDbTrace_Dumper> m_traceDumper;
    OdStaticRxObject<OdDb3dSolid_Dumper> m_3DSolidDumper;
    OdStaticRxObject<OdDbProxyEntity_Dumper> m_proxyEntityDumper;
    OdStaticRxObject<OdDbHatch_Dumper> m_hatchDumper;
    OdStaticRxObject<OdDbCircle_Dumper> m_circleDumper;
    OdStaticRxObject<OdDbMText_Dumper> m_mTextDumper;
    OdStaticRxObject<OdDbText_Dumper> m_textDumper;
    OdStaticRxObject<OdDbMline_Dumper> m_mlineDumper;
    OdStaticRxObject<OdDbRasterImage_Dumper> m_imageDumper;
    OdStaticRxObject<OdDbArcAlignedText_Dumper> m_arcAlignedTextDumper;
    OdStaticRxObject<OdDbOle2Frame_Dumper> m_ole2FrameDumper;
    OdStaticRxObject<OdDbShape_Dumper> m_ShapeDumper;
    OdStaticRxObject<OdDbFace_Dumper> m_FaceDumper;

    //add by zl
    OdStaticRxObject<OdDbDimension_Dumper> m_dimensionDumper;


public:
    void addXs()
    {
        OdDbEntity::desc()->addX(OdDbEntity_Dumper::desc(), &m_entityDumper);
        OdDbRegion::desc()->addX(OdDbEntity_Dumper::desc(), &m_regionDumper);
        OdDbPolyline::desc()->addX(OdDbEntity_Dumper::desc(), &m_polylineDumper);
        OdDb2dPolyline::desc()->addX(OdDbEntity_Dumper::desc(), &m_2dPolylineDumper);
        OdDb3dPolyline::desc()->addX(OdDbEntity_Dumper::desc(), &m_3dPolylineDumper);
        OdDbPolyFaceMesh::desc()->addX(OdDbEntity_Dumper::desc(), &m_polyFaceMeshDumper);
        OdDbPolygonMesh::desc()->addX(OdDbEntity_Dumper::desc(), &m_polygonMesh);
        OdDbBlockReference::desc()->addX(OdDbEntity_Dumper::desc(), &m_blockReference);
        OdDbMInsertBlock::desc()->addX(OdDbEntity_Dumper::desc(), &m_mInsertBlock);
        OdDbSpline::desc()->addX(OdDbEntity_Dumper::desc(), &m_splineDumper);
        OdDbEllipse::desc()->addX(OdDbEntity_Dumper::desc(), &m_ellipseDumper);
        OdDbSolid::desc()->addX(OdDbEntity_Dumper::desc(), &m_solidDumper);
        OdDbTrace::desc()->addX(OdDbEntity_Dumper::desc(), &m_traceDumper);
        OdDb3dSolid::desc()->addX(OdDbEntity_Dumper::desc(), &m_3DSolidDumper);
        OdDbProxyEntity::desc()->addX(OdDbEntity_Dumper::desc(), &m_proxyEntityDumper);
        OdDbHatch::desc()->addX(OdDbEntity_Dumper::desc(), &m_hatchDumper);
        OdDbCircle::desc()->addX(OdDbEntity_Dumper::desc(), &m_circleDumper);
        OdDbMText::desc()->addX(OdDbEntity_Dumper::desc(), &m_mTextDumper);
        OdDbText::desc()->addX(OdDbEntity_Dumper::desc(), &m_textDumper);
        OdDbMline::desc()->addX(OdDbEntity_Dumper::desc(), &m_mlineDumper);
        OdDbRasterImage::desc()->addX(OdDbEntity_Dumper::desc(), &m_imageDumper);
        OdDbArcAlignedText::desc()->addX(OdDbEntity_Dumper::desc(), &m_arcAlignedTextDumper);
        OdDbOle2Frame::desc()->addX(OdDbEntity_Dumper::desc(), &m_ole2FrameDumper);
        OdDbPoint::desc()->addX(OdDbEntity_Dumper::desc(), &m_pointDumper);
        OdDbLine::desc()->addX(OdDbEntity_Dumper::desc(), &m_LineDumper);
        OdDbArc::desc()->addX(OdDbEntity_Dumper::desc(), &m_ArcDumper);
        OdDbShape::desc()->addX(OdDbEntity_Dumper::desc(), &m_ShapeDumper);
        OdDbFace::desc()->addX(OdDbEntity_Dumper::desc(), &m_FaceDumper);

        //add by zl
        OdDbDimension::desc()->addX(OdDbDimension_Dumper::desc(), &m_dimensionDumper);


    } // end addXs

    void delXs()
    {
        OdDbEntity::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbRegion::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbPolyline::desc()->delX(OdDbEntity_Dumper::desc());
        OdDb2dPolyline::desc()->delX(OdDbEntity_Dumper::desc());
        OdDb3dPolyline::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbPolyFaceMesh::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbPolygonMesh::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbBlockReference::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbMInsertBlock::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbSpline::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbEllipse::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbSolid::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbTrace::desc()->delX(OdDbEntity_Dumper::desc());
        OdDb3dSolid::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbProxyEntity::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbHatch::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbCircle::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbMText::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbText::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbMline::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbRasterImage::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbArcAlignedText::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbOle2Frame::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbPoint::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbLine::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbArc::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbShape::desc()->delX(OdDbEntity_Dumper::desc());
        OdDbFace::desc()->delX(OdDbEntity_Dumper::desc());

        //add by zl
        OdDbDimension::desc()->delX(OdDbDimension_Dumper::desc());

    }
};

void ExProtocolExtension::initialize()
{
    OdDbEntity_Dumper::rxInit();
    m_pDumpers = new Dumpers;
    m_pDumpers->addXs();
}

void ExProtocolExtension::uninitialize()
{
    m_pDumpers->delXs();
    OdDbEntity_Dumper::rxUninit();
    delete m_pDumpers;
    m_pDumpers = 0;
}