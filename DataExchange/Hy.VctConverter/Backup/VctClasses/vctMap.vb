
Public Class vctMap

#Region "�ڲ�����"

    Private p_Layers As New List(Of vctLayer)

#End Region

#Region "����"

    Property Layers() As List(Of vctLayer)
        Get
            Return Me.p_Layers
        End Get
        Set(ByVal value As List(Of vctLayer))
            Me.p_Layers = value
        End Set
    End Property

#End Region

#Region "���к���"

    Public Function GetLayerByName(ByVal p_LayerName As String) As vctLayer
        For i As Integer = 0 To Me.Layers.Count - 1
            If Me.Layers(i).Name = p_LayerName Then
                Return Me.Layers(i)
            End If
        Next
        Return Nothing
    End Function

    Public Function GetLayerByChineseName(ByVal p_LayerChineseName As String) As vctLayer
        For i As Integer = 0 To Me.Layers.Count - 1
            If Me.Layers(i).ChineseName = p_LayerChineseName Then
                Return Me.Layers(i)
            End If
        Next
        Return Nothing
    End Function

    Public Function GetLayerByCode(ByVal p_LayerCode As String) As vctLayer
        For i As Integer = 0 To Me.Layers.Count - 1
            If Me.Layers(i).Code = p_LayerCode Then
                Return Me.Layers(i)
            End If
        Next
        Return Nothing
    End Function

    Public Function GetLineByID(ByVal p_ID As Integer) As vctLine
        For i As Integer = 0 To Me.Layers.Count - 1
            Dim mLayer As vctLayer = Me.Layers(i)
            For j As Integer = 0 To mLayer.Lines.Count - 1
                Dim mLine As vctLine = mLayer.Lines(j)
                If mLine.ID = p_ID Then
                    Return mLine
                End If
            Next
        Next
        Return Nothing
    End Function

#End Region

End Class
