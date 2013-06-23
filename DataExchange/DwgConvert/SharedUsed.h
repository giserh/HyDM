#pragma once



#include "DlgProgressBar.h"
#include "LogRecorder.h"


//////////////////////////////////////////////////////////////////////////

#define  PI  3.14159265358979323846
const double g_dAngleParam = 180 / PI;



class SharedUsed
{
public:
    SharedUsed(void);
    ~SharedUsed(void);
};

//��IFeatureClass��ȡ�ÿռ�ο�
IGeometryDef* API_GetGeometryDef(IFeatureClass* pFeatureClass);

//���ݲ�ѯ������ԴҪ����ת����Ŀ�깤���ռ�(MDB)
void API_ConvertFeatureClass(IFeatureClass* pSrcFtCls,
                             IQueryFilterPtr pQueryFilter,
                             IWorkspace* pTargetWorkspace,
                             LPSTR sTargetDatasetName,
                             LPSTR sTargetFeatureClassName,
                             IFieldsPtr pTargetFields = NULL);

ISpatialReference* API_GetSpatialReference(IFeatureClass* pFeatureClass);

CString GetLogPath();