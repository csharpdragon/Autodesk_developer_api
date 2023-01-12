namespace AdvLibrary.ForgeApi.Model
{
    public class AutoProject
    {
        private string hubId;
        private string projectId;
        private string projectName;
        private string projectType;

        #region Public Properties
        public string HubId
        {
            get { return hubId; }
            set { hubId = value; }
        }
        public string ProjectId
        {
            get { return projectId; }
            set { projectId = value; }
        }
        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }
        public string ProjectType
        {
            get { return projectType; }
            set { projectType = value; }
        }
        #endregion

        #region Constructor
        public AutoProject(string hubId, string projectId, string projectName, string projectType)
        {
            this.hubId = hubId;
            this.projectId = projectId;
            this.projectName = projectName;
            this.projectType = projectType;
        }
        public AutoProject()
        {
            this.hubId = string.Empty;
            this.projectId = string.Empty;
            this.projectName = string.Empty;
            this.projectType = string.Empty;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("HubId: {0}, ProjectId: {1}, ProjectName: {2}, ProjectType: {3}", hubId, projectId, projectName, projectType);
        }
        #endregion
    }
}
