using Newtonsoft.Json;

namespace AdvLibrary.ForgeApi.Model
{
    public class AutoFileExtension
    {
        public AutoFileExtension(){ }
        public string version { get; set; }
        public string modelVersion { get; set; }
        public string projectGuid { get; set; }
        public string originalItemUrn { get; set; }
        public string isCompositeDesign { get; set; }
        public string modelType { get; set; }
        public string latestEpisodeGuid { get; set; }
        public string mimeType { get; set; }
        public string modelGuid { get; set; }
        public string processState { get; set; }
        public string extractionState { get; set; }
        public string splittingState { get; set; }
        public string reviewState { get; set; }
        public string revisionDisplayLabel { get; set; }
        public string sourceFileName { get; set; }
        public string conformingStatus { get; set; }
    }

    public class AutoFile
    {


        #region Private Properties
        private AutoFileExtension extension;
        private string contentId;//itemid
        private string projectId;
        private string folderId;
        private string name;
        private string type;
        private string createTime;
        private string createUserId;
        private string createUserName;
        private string lastModifiedTime;
        private string lastModifiedUserId;
        private string lastModifiedUserName;
        private string versionNumber;
        private string mimeType;
        private string storageSize;
        private string fileType;

        #endregion
        #region Constructor
        public AutoFile() { }
        public AutoFile(string contentId, string projectId, string folderId, string name, string type, string createTime, string createUserId, string createUserName, string lastModifiedTime, string lastModifiedUserId, string lastModifiedUserName, string versionNumber, string mimeType, string storageSize, string fileType, AutoFileExtension extension)
        {
            this.contentId = contentId;
            this.projectId = projectId;
            this.folderId = folderId;
            this.name = name;
            this.type = type;
            this.createTime = createTime;
            this.createUserId = createUserId;
            this.createUserName = createUserName;
            this.lastModifiedTime = lastModifiedTime;
            this.lastModifiedUserId = lastModifiedUserId;
            this.lastModifiedUserName = lastModifiedUserName;
            this.versionNumber = versionNumber;
            this.mimeType = mimeType;
            this.storageSize = storageSize;
            this.fileType = fileType;
            this.extension = extension;
        }
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
        public string CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }
        public string CreateUserId
        {
            get { return createUserId; }
            set { createUserId = value; }
        }
        public string CreateUserName
        {
            get { return createUserName; }
            set { createUserName = value; }
        }
        public string LastModifiedTime
        {
            get { return lastModifiedTime; }
            set { lastModifiedTime = value; }
        }
        public string LastModifiedUserId
        {
            get { return lastModifiedUserId; }
            set { lastModifiedUserId = value; }
        }
        public string LastModifiedUserName
        {
            get { return lastModifiedUserName; }
            set { lastModifiedUserName = value; }
        }
        public string VersionNumber
        {
            get { return versionNumber; }
            set { versionNumber = value; }
        }
        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }
        public string StorageSize
        {
            get { return storageSize; }
            set { storageSize = value; }
        }

        public string FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }

        public AutoFileExtension Extension
        {
            get { return extension; }
            set { extension = value; }
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
