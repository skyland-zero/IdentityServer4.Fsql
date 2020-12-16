using IdentityServer4.Fsql.Storage.DbMark;
using IdentityServer4.Fsql.Storage.Extensions;
using System;
using System.Diagnostics;
using System.Threading;

namespace IdentityServer4.Fsql.IntegrationTests
{
    public class g
    {
        static IFreeSql<ConfigurationDb> configurationDbLazy = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=./configuration.db;")
            .UseAutoSyncStructure(false)
            //.UseGenerateCommandParameterWithLambda(true)
            .UseLazyLoading(true)
            .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                                                                                                                 //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
            .Build<ConfigurationDb>()
            .ConfigureClientContext()
            .ConfigureResourcesContext()
            .SyncStructureClient()
            .SyncStructureResources();

        public static IFreeSql<ConfigurationDb> configurationDb => configurationDbLazy;

        static IFreeSql<OperationalDb> operationalDbLazy = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=./operational.db;")
            .UseAutoSyncStructure(false)
            //.UseGenerateCommandParameterWithLambda(true)
            .UseLazyLoading(true)
            .UseMonitorCommand(
                cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前
                                                                                                                 //, (cmd, traceLog) => Console.WriteLine(traceLog)
                )
            .Build<OperationalDb>()
            .ConfigurePersistedGrantContext()
            .SyncStructurePersistedGrant();

        public static IFreeSql<OperationalDb> operationalDb => operationalDbLazy;
    }
}
