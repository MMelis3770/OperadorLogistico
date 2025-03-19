Public Class SEI_VisorCrystal
    Protected _Report As CrystalDecisions.CrystalReports.Engine.ReportDocument

#Region "Constructor"
    Public Sub New(ByVal oReport As CrystalDecisions.CrystalReports.Engine.ReportDocument)
        Me._Report = oReport
        '
        ' Llamada necesaria para el Diseñador de Windows Forms.
        InitializeComponent()
        '
        Me.Text = Me._Report.FileName
    End Sub
    Public Sub ConfigureCrystalReports()
        CrystalReportViewer.ShowExportButton = True
        CrystalReportViewer.ShowGroupTreeButton = False
        CrystalReportViewer.ShowRefreshButton = False
        CrystalReportViewer.ReportSource = Me._Report
        Me.WindowState = FormWindowState.Maximized
    End Sub
#End Region

#Region "Events"
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
    Private Sub CrystalReportViewer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CrystalReportViewer.Load
        Me.ConfigureCrystalReports()
    End Sub
#End Region

    Private Sub SEI_VisorCrystal_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class