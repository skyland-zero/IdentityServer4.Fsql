namespace IdentityServer4.Fsql.Storage.Options
{
    public class ConfigurationStoreOptions
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public FreeSql.DataType DataType { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
