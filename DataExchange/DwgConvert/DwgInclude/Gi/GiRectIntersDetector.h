///////////////////////////////////////////////////////////////////////////////
// Copyright � 2002, Open Design Alliance Inc. ("Open Design") 
// 
// This software is owned by Open Design, and may only be incorporated into 
// application programs owned by members of Open Design subject to a signed 
// Membership Agreement and Supplemental Software License Agreement with 
// Open Design. The structure and organization of this Software are the valuable 
// trade secrets of Open Design and its suppliers. The Software is also protected 
// by copyright law and international treaty provisions. You agree not to 
// modify, adapt, translate, reverse engineer, decompile, disassemble or 
// otherwise attempt to discover the source code of the Software. Application 
// programs incorporating this software must include the following statment 
// with their copyright notices:
//
//      DWGdirect � 2002 by Open Design Alliance Inc. All rights reserved. 
//
// By use of this software, you acknowledge and accept the terms of this 
// agreement.
///////////////////////////////////////////////////////////////////////////////



#ifndef __ODGIRECTINTERSDETECTOR__
#define __ODGIRECTINTERSDETECTOR__


#include "Gi/GiConveyorNode.h"
#include "Ge/GeDoubleArray.h"
#include "Ge/GePoint2dArray.h"

class OdGiDeviation;
class OdGiConveyorContext;

#include "DD_PackPush.h"

/** Description:

    {group:OdGi_Classes} 
    Conveyor node detecting if given primitive intersects with rectangular prism.
    If intersects - something is passed through. In other case - primitive is eaten
    by this conveyor node.
*/
class ODGI_EXPORT OdGiRectIntersDetector : public OdGiConveyorNode
{
public:
  ODRX_DECLARE_MEMBERS(OdGiRectIntersDetector);

  virtual void set(const OdGePoint2d* points, // points defining sides of clipping prism perpendicular to XY
                   bool   bClipLowerZ = false, // number of points is always two
                   double dLowerZ = 0.0,
                   bool   bClipUpperZ = false,
                   double dUpperZ = 0.0) = 0;

  virtual void set(const OdGePoint2dArray& points, // points defining sides of clipping prism perpendicular to XY
                   bool   bClipLowerZ = false, // number of points is always two
                   double dLowerZ = 0.0,
                   bool   bClipUpperZ = false,
                   double dUpperZ = 0.0) = 0;

  virtual void get(OdGePoint2dArray& points,
                   bool&   bClipLowerZ,
                   double& dLowerZ,
                   bool&   bClipUpperZ,
                   double& dUpperZ) const = 0;

  /**
    Sets max deviation for curve tesselation.
  */
  virtual void setDeviation(const OdGeDoubleArray& deviations) = 0;

  /**
    Sets deviation object to obtain max deviation for curve tesselation.
  */
  virtual void setDeviation(const OdGiDeviation* pDeviation) = 0;

  /**
    Sets the draw context object (to access to traits, etc).
  */
  virtual void setDrawContext(OdGiConveyorContext* pDrawCtx) = 0;
};

typedef OdSmartPtr<OdGiRectIntersDetector> OdGiRectIntersDetectorPtr;

#include "DD_PackPop.h"

#endif //#ifndef __ODGIRECTINTERSDETECTOR__

