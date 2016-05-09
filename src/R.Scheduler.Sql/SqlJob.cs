﻿using System;
using System.Data.Common;
using System.Reflection;
using Common.Logging;
using Quartz;

namespace R.Scheduler.Sql
{
    public class SqlJob : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> Db Connection string REQUIRED.</summary>
        public const string ConnectionString = "connectionString";

        /// <summary> Non query command (insert, update, delete, stored proc REQUIRED.</summary>
        public const string NonQueryCommand = "nonQueryCommand";

        /// <summary> Fully qualified provider assembly name. REQUIRED.</summary>
        public const string PoviderAssemblyName = "providerAssemblyName";

        /// <summary> Connection class. REQUIRED.</summary>
        public const string ConnectionClass = "connectionClass";

        /// <summary> Command class. REQUIRED.</summary>
        public const string CommandClass = "commandClass";

        /// <summary> Command style (StoredProcedure/SqlString). REQUIRED.</summary>
        public const string CommandStyle = "commandStyle";

        /// <summary> DataAdapter class. REQUIRED..</summary>
        public const string DataAdapterClass = "dataAdapterClass";

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap data = context.MergedJobDataMap;

            string providerAssemblyName = GetRequiredParameter(data, PoviderAssemblyName);
            string connectionClass = GetRequiredParameter(data, ConnectionClass);
            string commandClass = GetRequiredParameter(data, CommandClass);
            string dataAdapterClass = GetRequiredParameter(data, DataAdapterClass);
            string commandStyle = GetOptionalParameter(data, CommandStyle);

            string connectionString = GetRequiredParameter(data, ConnectionString);
            string nonQueryCommand = GetRequiredParameter(data, NonQueryCommand);

            // try load types 
            Assembly provider = Assembly.Load(providerAssemblyName);
            Type connectionType = provider.GetType(connectionClass, true);
            Type commandType = provider.GetType(commandClass, true);
            Type dataAdapterType = provider.GetType(dataAdapterClass, true);

            // execute methods using the provider types as generic arguments
            MethodInfo executeNonQueryMethod = typeof(SqlJob).GetMethod("ExecuteNonQuery");
            MethodInfo genericExecuteNonQueryMethod = executeNonQueryMethod.MakeGenericMethod(connectionType, commandType, dataAdapterType);
            genericExecuteNonQueryMethod.Invoke(this, new[] { connectionString, nonQueryCommand, commandStyle });
        }

        public void ExecuteNonQuery<CONNECTION_TYPE, COMMAND_TYPE, ADAPTER_TYPE>(string connectionString, string commandText, string commandStyle) 
            where CONNECTION_TYPE : DbConnection, new() 
            where COMMAND_TYPE : DbCommand 
            where ADAPTER_TYPE : DbDataAdapter, new()
        {
            using (var dbControl = new DbControl<CONNECTION_TYPE, COMMAND_TYPE, ADAPTER_TYPE>(connectionString))
            {
                DbCommand command = (null != commandStyle && commandStyle.ToLower() == "storedprocedure")
                    ? dbControl.GetStoredProcedureCommand(commandText)
                    : dbControl.GetSqlStringCommand(commandText);

                try
                {
                    dbControl.ExecuteNonQuery(command);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in SqlJob: ", ex);
                    throw new JobExecutionException(ex.Message, ex, false);
                }
            }
        }

        protected virtual string GetOptionalParameter(JobDataMap data, string propertyName)
        {
            string value = data.GetString(propertyName);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value;
        }

        protected virtual string GetRequiredParameter(JobDataMap data, string propertyName)
        {
            string value = data.GetString(propertyName);
            if (string.IsNullOrEmpty(value))
            {
                Logger.ErrorFormat("Error in SqlJob: {0} not specified.", propertyName);
                throw new JobExecutionException(string.Format("{0} not specified.", propertyName));
            }
            return value;
        }
    }
}
