using Newtonsoft.Json;

namespace AdvLibrary.ForgeApi.Model
{
    public class AutoFile
    {
        #region Private Properties
        private string contentId;//itemid
        private string projectId;
        private string folderId;
        private string name;
        private string type;
        #endregion

        #region Public Properties
        public string ContentId
        {
            get { return contentId; }
            set { contentId = value; }
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
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        #endregion

        #region Constructor
        public AutoFile(string contentId, string projectId, string folderId, string name, string type)
        {
            this.contentId = contentId;
            this.projectId = projectId;
            this.folderId = folderId;
            this.name = name;
            this.type = type;
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return "ContentId: " + contentId + ", ProjectId: " + projectId + ", FolderId: " + folderId + ", Name: " + name + ", Type: " + type;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
