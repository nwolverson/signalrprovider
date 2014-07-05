<%@ Application Inherits="TestFMVC.Global" Language="C#" %>
<script Language="C#" RunAt="server">

// *** NOTE On Mac and Linux you may need to manually edit the project file to use 
// *** NOTE v9.0/WebApplications/Microsoft.WebApplication.targets instead of v10.0

  protected void Application_Start(Object sender, EventArgs e) {
    // Delegate event handling to the F# Application class
    base.Start();
  }

</script>
