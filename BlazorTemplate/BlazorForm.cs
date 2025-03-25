using BlazorTemplate.Components;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace BlazorTemplate;

public partial class BlazorForm : Form
{
    public BlazorForm(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.Services = serviceProvider;
        blazorWebView1.RootComponents.Add<App>("#app");
    }
}
