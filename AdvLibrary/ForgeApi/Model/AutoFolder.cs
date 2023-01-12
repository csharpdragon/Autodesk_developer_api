namespace AdvLibrary.ForgeApi.Model
{
    public class AutoFolder
    {
        #region Private Members
        private string hubId;
        private string projectId;
        private string folderId;
        private string folderName;
        private string folderType;
        private string folderPath;
        #endregion

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
        public string FolderId
        {
            get { return folderId; }
            set { folderId = value; }
        }
        public string FolderName
        {
            get { return folderName; }
            set { folderName = value; }
        }
        public string FolderType
        {
            get { return folderType; }
            set { folderType = value; }
        }
        #endregion

        #region Constructor
        public AutoFolder(string hubId, string projectId, string folderId, string folderName, string folderType)
        {
            this.hubId = hubId;
            this.projectId = projectId;
            this.folderId = folderId;
            this.folderName = folderName;
            this.folderType = folderType;
        }
        public AutoFolder()
        {
            this.hubId = string.Empty;
            this.projectId = string.Empty;
            this.folderId = string.Empty;
            this.folderName = string.Empty;
            this.folderType = string.Empty;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("HubId: {0}, ProjectId: {1}, FolderId: {2}, FolderName: {3}, FolderType: {4}", hubId, projectId, folderId, folderName, folderType);
        }
        #endregion
    }
}
