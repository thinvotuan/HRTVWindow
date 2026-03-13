namespace BatDongSan.Models.ERP
{
    partial class ERPTVDataContext
    {
        public ERPTVDataContext()
            : base(global::System.Configuration.ConfigurationManager.ConnectionStrings["ERPConnectionString"].ToString(), mappingSource)
        {
            OnCreated();
            this.CommandTimeout = 3600;
        }
    }
}
