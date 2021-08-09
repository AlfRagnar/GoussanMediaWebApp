namespace GoussanMedia
{
    internal class Config
    {
        public static string AzureStorageConnectionString { get; internal set; }
        public static string AzureCosmosConnectionString { get; internal set; }
        public static string AzureStorageBlob { get; internal set; }
        public static string AzureStorageQueue { get; internal set; }
        public static string AzureAppInsight { get; internal set; }
        public static string AppName { get; internal set; }
        public static string AppRegion { get; internal set; }
        public static string CosmosDBName { get; internal set; }
        public static string CosmosDocuments { get; internal set; }
        public static string CosmosVideos { get; internal set; }
    }
}