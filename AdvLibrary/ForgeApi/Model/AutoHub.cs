namespace AdvLibrary.ForgeApi.Model
{
    public class AutoHub
    {
        #region Private Members
        private string hubId;
        private string hubName;
        private string hubType;
        #endregion

        #region Public Properties
        public string HubId
        {
            get { return hubId; }
            set { hubId = value; }
        }
        public string HubName
        {
            get { return hubName; }
            set { hubName = value; }
        }
        public string HubType
        {
            get { return hubType; }
            set { hubType = value; }
        }
        #endregion

        #region Constructor
        public AutoHub(string hubId, string hubName, string hubType)
        {
            this.hubId = hubId;
            this.hubName = hubName;
            this.hubType = hubType;
        }
        public AutoHub()
        {
            this.hubId = string.Empty;
            this.hubName = string.Empty;
            this.hubType = string.Empty;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("HubId: {0}, HubName: {1}, HubType: {2}", hubId, hubName, hubType);
        }
        #endregion
    }
}
