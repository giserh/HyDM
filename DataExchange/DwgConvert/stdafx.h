// stdafx.h : ��׼ϵͳ�����ļ��İ����ļ���
// ���Ǿ���ʹ�õ��������ĵ�
// �ض�����Ŀ�İ����ļ�

#pragma once

#ifndef STRICT
#define STRICT
#endif

// ������뽫λ������ָ��ƽ̨֮ǰ��ƽ̨��ΪĿ�꣬���޸����ж��塣
// �йز�ͬƽ̨��Ӧֵ��������Ϣ����ο� MSDN��
#ifndef WINVER				// ����ʹ���ض��� Windows XP ����߰汾�Ĺ��ܡ�
#define WINVER 0x0501		// ����ֵ����Ϊ��Ӧ��ֵ���������� Windows �������汾��
#endif

#ifndef _WIN32_WINNT		// ����ʹ���ض��� Windows XP ����߰汾�Ĺ��ܡ�
#define _WIN32_WINNT 0x0501	// ����ֵ����Ϊ��Ӧ��ֵ���������� Windows �������汾��
#endif						

#ifndef _WIN32_WINDOWS		// ����ʹ���ض��� Windows 98 ����߰汾�Ĺ��ܡ�
#define _WIN32_WINDOWS 0x0410 // ����ֵ����Ϊ�ʵ���ֵ����ָ���� Windows Me ����߰汾��ΪĿ�ꡣ
#endif

#ifndef _WIN32_IE			// ����ʹ���ض��� IE 6.0 ����߰汾�Ĺ��ܡ�
#define _WIN32_IE 0x0600	// ����ֵ����Ϊ��Ӧ��ֵ���������� IE �������汾��
#endif

#define _ATL_APARTMENT_THREADED
#define _ATL_NO_AUTOMATIC_NAMESPACE

//#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// ĳЩ CString ���캯��������ʽ��


#include <afxwin.h>
#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdisp.h>        // MFC �Զ�����
#endif // _AFX_NO_OLE_SUPPORT

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>

#include "ESRI_AoInterface.h"
#include <windows.h>


#include "OdaCommon.h"
#include "DbDatabase.h"
#include "DbAudit.h"

#include "Db2LineAngularDimension.h"
#include "Db2dPolyline.h"
#include "Db2dVertex.h"
#include "Db3PointAngularDimension.h"
#include "Db3dPolyline.h"
#include "Db3dPolylineVertex.h"
#include "Db3dSolid.h"
#include "DbAlignedDimension.h"
#include "DbArc.h"
#include "DbArcAlignedText.h"
#include "DbAttribute.h"
#include "DbAttributeDefinition.h"
#include "DbBlockTableRecord.h"
#include "DbBlockReference.h"
#include "DbBlockTable.h"
#include "DbBody.h"
#include "DbCircle.h"
#include "DbDiametricDimension.h"
#include "DbDimAssoc.h"
#include "DbDimStyleTable.h"
#include "DbEllipse.h"
#include "DbFace.h"
#include "DbFaceRecord.h"
#include "DbFcf.h"
#include "DbField.h"
#include "DbGroup.h"
#include "DbHyperlink.h"
#include "DbLayerTable.h"
#include "DbLayout.h"
#include "DbLeader.h"
#include "DbLine.h"
#include "DbLinetypeTable.h"
#include "DbMText.h"
#include "DbOrdinateDimension.h"
#include "DbPoint.h"
#include "DbPolyFaceMesh.h"
#include "DbPolyFaceMeshVertex.h"
#include "DbPolygonMesh.h"
#include "DbPolygonMeshVertex.h"
#include "DbPolyline.h"
#include "DbRasterImage.h"
#include "DbRasterImageDef.h"
#include "DbRasterVariables.h"
#include "DbRay.h"
#include "DbRegion.h"
#include "DbRotatedDimension.h"
#include "DbShape.h"
#include "DbSolid.h"
#include "DbSortentsTable.h"
#include "DbSpline.h"
#include "DbTable.h"
#include "DbText.h"
#include "DbTextStyleTable.h"
#include "DbTrace.h"
#include "DbViewport.h"
#include "DbViewportTable.h"
#include "DbViewportTableRecord.h"
#include "DbWipeout.h"
#include "DbXline.h"
#include "DbXrecord.h"
#include "Ge/GeCircArc2d.h"
#include "Ge/GeScale3d.h"
#include "Ge/GeExtents3d.h"
#include "Gi/GiMaterial.h"
#include "DbSymUtl.h"
#include "DbHostAppServices.h"
#include "HatchPatternManager.h"
#include "DbLayerTableRecord.h"

#include "SharedUsed.h"


using namespace ATL;