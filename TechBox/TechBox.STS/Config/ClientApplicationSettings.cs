namespace TechBox.STS.Config
{
    /// <summary>
    /// Settings for the Application pulled in from the Appsettings.json file.
    /// </summary>
    public class ClientApplicationSettings
    {
        public string AngularBaseUrl { get; set; }

        public string ApiSecret { get; set; }
    }
}