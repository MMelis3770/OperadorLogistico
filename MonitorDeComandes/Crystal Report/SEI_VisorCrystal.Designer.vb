<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SEI_VisorCrystal
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SEI_VisorCrystal))
        Me.CrystalReportViewer = New CrystalDecisions.Windows.Forms.CrystalReportViewer()
        Me.SuspendLayout()
        '
        'CrystalReportViewer
        '
        Me.CrystalReportViewer.ActiveViewIndex = -1
        Me.CrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.CrystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default
        Me.CrystalReportViewer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.CrystalReportViewer.Location = New System.Drawing.Point(0, 0)
        Me.CrystalReportViewer.Name = "CrystalReportViewer"
        Me.CrystalReportViewer.Size = New System.Drawing.Size(834, 526)
        Me.CrystalReportViewer.TabIndex = 0
        '
        'SEI_VisorCrystal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(834, 526)
        Me.Controls.Add(Me.CrystalReportViewer)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "SEI_VisorCrystal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Crystal Report"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents CrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
End Class
