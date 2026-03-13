namespace BatDongSan.Models.ERP
{
    partial class ERPTVWINDOWDataContext
    {
        public ERPTVWINDOWDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["ERPTVWINDOWConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
