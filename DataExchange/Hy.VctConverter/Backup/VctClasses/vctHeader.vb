Imports ESRI.ArcGIS.Geometry
Imports System.IO
Imports System.Text.RegularExpressions
Imports ESRI.ArcGIS.Geodatabase

Public Class vctHeader

#Region "�ڲ�����"

    Private p_DataMark As String
    Private p_Version As String
    Private p_Unit As String
    Private p_Dim As Integer
    Private p_Topo As Integer
    Private p_Coordinate As String
    Private p_Projection As String
    Private p_Spheroid As String
    Private p_Parameters(1) As Double
    Private p_Meridinan As String
    Private p_SurvyDate As Date
    Private p_MinX As Double
    Private p_MinY As Double
    Private p_MaxX As Double
    Private p_MaxY As Double
    Private p_Scale As Integer
    Private p_Separator As String

    Private p_Paras As String

#End Region

#Region "����"

    ''' <summary>
    ''' ������;
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property DataMark() As String
        Get
            Return Me.p_DataMark
        End Get
        Set(ByVal value As String)
            Me.p_DataMark = value
        End Set
    End Property

    ''' <summary>
    ''' �汾
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Version() As String
        Get
            Return p_Version
        End Get
        Set(ByVal value As String)
            p_Version = value
        End Set
    End Property

    ''' <summary>
    ''' ��λ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>ƽ������:M,��������:D</remarks>
    Public Property Unit() As String
        Get
            Return p_Unit
        End Get
        Set(ByVal value As String)
            Me.p_Unit = value
        End Set
    End Property

    ''' <summary>
    ''' ����ά��
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>2:��ά��3����ά</remarks>
    Public Property Dimension() As Integer
        Get
            Return p_Dim
        End Get
        Set(ByVal value As Integer)
            p_Dim = value
        End Set
    End Property

    ''' <summary>
    ''' ����
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>2����������Ŀ��ı�ʶ����,0:û������,1:û����Щ��Ϣ���ж���ι�������Ŀ���ʶ��,��ʸ�����ݽ�����ʽ����1</remarks>
    Public Property Topo() As Integer
        Get
            Return Me.p_Topo
        End Get
        Set(ByVal value As Integer)
            Me.p_Topo = value
        End Set
    End Property

    ''' <summary>
    ''' ����ϵ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>G:��������ϵ,M:��������ϵ</remarks>
    Public Property Coordinate() As String
        Get
            Return Me.p_Coordinate
        End Get
        Set(ByVal value As String)
            Me.p_Coordinate = value
        End Set
    End Property

    ''' <summary>
    ''' ͶӰ����
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Projection() As String
        Get
            Return p_Projection
        End Get
        Set(ByVal value As String)
            p_Projection = value
        End Set
    End Property

    ''' <summary>
    ''' �ο�������
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Spheroid() As String
        Get
            Return p_Spheroid
        End Get
        Set(ByVal value As String)
            p_Spheroid = value
        End Set
    End Property

    ''' <summary>
    ''' ͶӰ����
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Parameters() As Double()
        Get
            Return p_Parameters
        End Get
        Set(ByVal value As Double())
            p_Parameters = value
        End Set
    End Property

    ''' <summary>
    ''' ���������߾���
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Meridinan() As String
        Get
            Return p_Meridinan
        End Get
        Set(ByVal value As String)
            p_Meridinan = value
        End Set
    End Property

    ''' <summary>
    ''' ��ҵ������ɵ�����
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SurvyDate() As Date
        Get
            Return Me.p_SurvyDate
        End Get
        Set(ByVal value As Date)
            Me.p_SurvyDate = value
        End Set
    End Property

    Public Property MinX() As Double
        Get
            Return Me.p_MinX
        End Get
        Set(ByVal value As Double)
            Me.p_MinX = value
        End Set
    End Property

    Public Property MinY() As Double
        Get
            Return Me.p_MinY
        End Get
        Set(ByVal value As Double)
            Me.p_MinY = value
        End Set
    End Property

    Public Property MaxX() As Double
        Get
            Return Me.p_MaxX
        End Get
        Set(ByVal value As Double)
            Me.p_MaxX = value
        End Set
    End Property

    Public Property MaxY() As Double
        Get
            Return Me.p_MaxY
        End Get
        Set(ByVal value As Double)
            Me.p_MaxY = value
        End Set
    End Property

    ''' <summary>
    ''' ԭͼ�����߷�ĸ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Scale() As Integer
        Get
            Return Me.p_Scale
        End Get
        Set(ByVal value As Integer)
            Me.p_Scale = value
        End Set
    End Property

    ''' <summary>
    ''' ���ⵥ�ֽڷǿհ��ַ������������ֶηָ���
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Separator() As String
        Get
            Return p_Separator
        End Get
        Set(ByVal value As String)
            p_Separator = value
        End Set
    End Property

#End Region

#Region "˽�к���"

    Private Function GetGeographicCoordinateSystem() As IGeographicCoordinateSystem
        '����
        Dim mMaxAxis As Double = Math.Max(Me.Parameters(0), Me.Parameters(1))
        '����
        Dim mMinAxis As Double = Math.Min(Me.Parameters(0), Me.Parameters(1))
        '����
        Dim mFlattening As Double = (mMaxAxis - mMinAxis) / mMaxAxis
        '����
        Dim mSpheroid As ISpheroid = New Spheroid
        Dim mSphereEdit As ISpheroidEdit = mSpheroid
        mSphereEdit.Define(Name:=Me.Spheroid, Alias:="", Abbreviation:="", Remarks:="", majorAxis:=mMaxAxis, Flattening:=mFlattening)

        Dim mDatum As IDatum = New Datum
        Dim mDatumEdit As IDatumEdit = mDatum
        mDatumEdit.Define(Me.Spheroid, "", "", "", mSpheroid)

        '����������
        Dim mPrimeMeridian As IPrimeMeridian = New PrimeMeridian
        Dim mPrimeMeridianEdit As IPrimeMeridianEdit = mPrimeMeridian
        mPrimeMeridianEdit.Define("Greenwich", "", "", "", 0)

        Dim mSpatRefFact As ISpatialReferenceFactory = New SpatialReferenceEnvironment
        Dim mUnit As IUnit = mSpatRefFact.CreateUnit(esriSRUnitType.esriSRUnit_Degree)

        Dim mGCSEdit As IGeographicCoordinateSystemEdit = New GeographicCoordinateSystem

        mGCSEdit.Define(Me.Spheroid, "", "", "", "", mDatum, mPrimeMeridian, mUnit)

        Return mGCSEdit
    End Function

    Private Function GetProjectedCoordinateSystem() As IProjectedCoordinateSystem
        Dim mGeographicCoordinateSystem As IGeographicCoordinateSystem = Me.GetGeographicCoordinateSystem()

        Dim mSpatRefFact As ISpatialReferenceFactory = New SpatialReferenceEnvironment
        Dim mUnit As IUnit = mSpatRefFact.CreateUnit(esriSRUnitType.esriSRUnit_Meter)
        Dim mProjection As IProjection
        Dim mProjectionName As String = Me.Projection
        mProjectionName = mProjectionName.Replace("-", "")
        mProjectionName = mProjectionName.Replace("_", "")
        mProjectionName = mProjectionName.Replace(" ", "")
        mProjectionName = mProjectionName.ToUpper
        mProjectionName = mProjectionName.Replace("PROJECTION", "")
        If mProjectionName.Contains("GaussKruger".ToUpper) Or mProjectionName.Contains("GaussKrueger".ToUpper) Then
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_GaussKruger)
        ElseIf mProjectionName.Contains("LambertConformalConic".ToUpper) Then
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_LambertConformalConic)
        ElseIf mProjectionName.Contains("LambertAzimuthalEqualArea".ToUpper) Then
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_LambertAzimuthal)
        ElseIf mProjectionName.Contains("Mercator".ToUpper) Then
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_Mercator)
        ElseIf mProjectionName.Contains("TransverseMercator".ToUpper) Then
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_TransverseMercator)
        Else
            mProjection = mSpatRefFact.CreateProjection(esriSRProjectionType.esriSRProjection_GaussKruger)
        End If

        Dim mProjectedCoordinateSystem As IProjectedCoordinateSystem = New ProjectedCoordinateSystem
        Dim mProjectedCoordinateSystemEdit As IProjectedCoordinateSystemEdit = mProjectedCoordinateSystem
        Dim mProjectedCoordinateSystem4GEN As IProjectedCoordinateSystem4GEN = mProjectedCoordinateSystem

        Dim mParamters(25) As IParameter
        mProjectedCoordinateSystem4GEN.GetParameters(mParamters)

        Dim mParamter As Object = mParamters

        'mSpatRefFact.CreateParameter(esriSRParameterType.esriSRParameter_ScaleFactor)
        Dim a As Object = System.Reflection.Missing.Value

        mProjectedCoordinateSystemEdit.Define(Me.Projection, a, a, a, a, mGeographicCoordinateSystem, mUnit, mProjection, mParamter)

        Return mProjectedCoordinateSystem
    End Function

#End Region

#Region "���к���"

    Public Function GetSpatialReference() As ISpatialReference
        Dim mSpatialRefrence As ISpatialReference = Nothing
        If Me.Coordinate.Trim.ToUpper = "G" Then
            mSpatialRefrence = Me.GetGeographicCoordinateSystem
        ElseIf Me.Coordinate.Trim.ToUpper = "M" Then
            mSpatialRefrence = Me.GetProjectedCoordinateSystem
        End If
        If mSpatialRefrence Is Nothing Then
            mSpatialRefrence = New UnknownCoordinateSystem
        End If

        Dim mXSub As Double = Me.MaxX - Me.MinX
        Dim mYSub As Double = Me.MaxY - Me.MinY

        mSpatialRefrence.SetDomain(Me.MinX - mXSub, Me.MaxX + mXSub, Me.MinY - mYSub, Me.MaxY + mYSub)
        Return mSpatialRefrence
    End Function

    Public Sub SetSpatialReference(ByVal p_SpatialReference As ISpatialReference, ByRef p_ErrMessage As String)
        If TypeOf p_SpatialReference Is ProjectedCoordinateSystem Then
            Dim mProcoordinateSystem As ProjectedCoordinateSystem = p_SpatialReference

            Me.Version = "2.0"
            Me.Projection = mProcoordinateSystem.Projection.Name
            Me.Topo = 1
            Me.Dimension = 2
            Me.DataMark = "LANDUSE.VCT"
            Me.Coordinate = "M"
            Me.Separator = ","
            Try
                mProcoordinateSystem.GetDomain(Me.MinX, Me.MaxX, Me.MinY, Me.MaxY)
            Catch ex As Exception
                'Me.MinX = mGeoDataset.Extent.XMin
                'Me.MaxX = mGeoDataset.Extent.XMax
                'Me.MinY = mGeoDataset.Extent.YMin
                'Me.MaxY = mGeoDataset.Extent.YMax
            End Try

            Me.Spheroid = mProcoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.Name
            Me.p_Paras = mProcoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.SemiMajorAxis.ToString + "," + mProcoordinateSystem.GeographicCoordinateSystem.Datum.Spheroid.SemiMinorAxis.ToString
            Me.Unit = "M" 'mProcoordinateSystem.GeographicCoordinateSystem.CoordinateUnit.Name.Remove(1)

            Try
                Me.Scale = mProcoordinateSystem.ScaleFactor
            Catch ex As Exception
                p_ErrMessage &= "����ϵ�����߶�ȡʧ�ܣ�����������Ϊ1��" & ex.Message & vbCrLf
                Me.Scale = 1
            End Try

            Me.SurvyDate = Now.Date
            Me.Meridinan = mProcoordinateSystem.CentralMeridian(True)
        ElseIf TypeOf p_SpatialReference Is GeographicCoordinateSystem Then
            Dim mGeoCoordinateSystem As IGeographicCoordinateSystem = p_SpatialReference

            Me.Version = "2.0"
            Me.Projection = ""
            Me.Topo = 1
            Me.Dimension = 2
            Me.DataMark = "LANDUSE.VCT"
            Me.Coordinate = "G"
            Me.Separator = ","
            Try
                mGeoCoordinateSystem.GetDomain(Me.MinX, MaxX, MinY, MaxY)
            Catch ex As Exception
                'Me.MinX = mGeoDataset.Extent.XMin
                'Me.MaxX = mGeoDataset.Extent.XMax
                'Me.MinY = mGeoDataset.Extent.YMin
                'Me.MaxY = mGeoDataset.Extent.YMax
            End Try

            Me.Spheroid = mGeoCoordinateSystem.Datum.Spheroid.Name
            Me.p_Paras = mGeoCoordinateSystem.Datum.Spheroid.SemiMajorAxis.ToString + "," + mGeoCoordinateSystem.Datum.Spheroid.SemiMinorAxis.ToString
            Me.Unit = mGeoCoordinateSystem.CoordinateUnit.Name.Remove(1)

            Try
                Me.Scale = mGeoCoordinateSystem.ScaleFactor
            Catch ex As Exception
                p_ErrMessage &= "����ϵ�����߶�ȡʧ�ܣ�ϵͳ�Զ�����������Ϊ1��" & ex.Message & vbCrLf
                Me.Scale = 1
            End Try

            Me.SurvyDate = Now.Date
            Me.Meridinan = 0
        End If
    End Sub

    Public Sub WriteToVctFile(ByVal p_StreamWriter As StreamWriter)
        p_StreamWriter.WriteLine("HeadBegin")
        p_StreamWriter.WriteLine("Datamark: " + Me.DataMark)
        p_StreamWriter.WriteLine("Version: " + Me.Version)
        p_StreamWriter.WriteLine("Unit: " + Me.Unit)
        p_StreamWriter.WriteLine("Dim: " + Me.Dimension.ToString)
        p_StreamWriter.WriteLine("Topo: " + Me.Topo.ToString)
        p_StreamWriter.WriteLine("Coordinate: " + Me.Coordinate)
        p_StreamWriter.WriteLine("Projection: " + Me.Projection)
        p_StreamWriter.WriteLine("Spheroid: " + Me.Spheroid)
        p_StreamWriter.WriteLine("Parameters: " + Me.p_Paras)
        p_StreamWriter.WriteLine("Meridinan: " + Me.Meridinan)
        p_StreamWriter.WriteLine("MinX: " + Me.MinX.ToString)
        p_StreamWriter.WriteLine("MinY: " + Me.MinY.ToString)
        p_StreamWriter.WriteLine("MaxX: " + Me.MaxX.ToString)
        p_StreamWriter.WriteLine("MaxY: " + Me.MaxY.ToString)
        p_StreamWriter.WriteLine("Scale: " + Me.Scale.ToString)
        p_StreamWriter.WriteLine("Date: " + Me.SurvyDate.Year.ToString.PadLeft(4, "0") & Me.SurvyDate.Month.ToString.PadLeft(2, "0") & Me.SurvyDate.Day.ToString.PadLeft(2, "0"))
        p_StreamWriter.WriteLine("Separator: " + Me.Separator)
        p_StreamWriter.WriteLine("HeadEnd")
    End Sub

#End Region

End Class
