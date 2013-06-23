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

#ifndef OD_GE_POSITION_2D_H
#define OD_GE_POSITION_2D_H /* {Secret} */


#include "GePointEnt2d.h"

#include "DD_PackPush.h"

/**
    Description:
    This class represents points (positions) in 2D space.

    Library: Ge

    {group:OdGe_Classes} 
*/
class GE_TOOLKIT_EXPORT OdGePosition2d : public OdGePointEnt2d
{
public:
  /**
    Arguments:
    point (I) Any 2D *point*.
    x (I) The X coordinate of the *point*.
    y (I) The Y coordinate of the *point*.
    source (I) Object to be cloned.
  */
  OdGePosition2d ();
  OdGePosition2d (
    const OdGePoint2d& point);
  OdGePosition2d (
    double x, 
    double y);
  OdGePosition2d (
    const OdGePosition2d& source);

  /**
    Description:
    Sets the coordinates of, and returns a reference
    to, this *point*.

    Arguments:
    point (I) Any 2D *point*.
    x (I) The X coordinate of the position.
    y (I) The Y coordinate of the position.
  */
  OdGePosition2d& set (
    const OdGePoint2d& point);
  OdGePosition2d& set (
    double x, 
    double y);

  OdGePosition2d& operator = (
    const OdGePosition2d& pos);
};

#include "DD_PackPop.h"

#endif // OD_GE_POSITION_2D_H

