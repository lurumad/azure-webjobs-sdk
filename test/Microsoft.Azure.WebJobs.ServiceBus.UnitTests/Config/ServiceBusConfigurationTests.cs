﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.TestCommon;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Azure.WebJobs.ServiceBus.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.ServiceBus.UnitTests.Config
{
    public class ServiceBusConfigurationTests
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly TestLoggerProvider _loggerProvider;

        public ServiceBusConfigurationTests()
        {
            _loggerFactory = new LoggerFactory();
            var filter = new LogCategoryFilter();
            filter.DefaultLevel = LogLevel.Debug;
            _loggerProvider = new TestLoggerProvider(filter.Filter);
            _loggerFactory.AddProvider(_loggerProvider);
        }

        [Fact]
        public void Constructor_SetsExpectedDefaults()
        {
            ServiceBusOptions config = new ServiceBusOptions();
            Assert.Equal(16, config.MessageOptions.MaxConcurrentCalls);
            Assert.Equal(0, config.PrefetchCount);
        }

        [Fact]
        public void ConnectionString_ReturnsExpectedDefaultUntilSetExplicitly()
        {
            ServiceBusOptions config = new ServiceBusOptions();

            string defaultConnection = null; // AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);
            Assert.Equal(defaultConnection, config.ConnectionString);

            string testConnection = "testconnection";
            config.ConnectionString = testConnection;
            Assert.Equal(testConnection, config.ConnectionString);
        }

        [Fact]
        public void PrefetchCount_GetSet()
        {
            ServiceBusOptions config = new ServiceBusOptions();
            Assert.Equal(0, config.PrefetchCount);
            config.PrefetchCount = 100;
            Assert.Equal(100, config.PrefetchCount);
        }

        [Fact]
        public void LogExceptionReceivedEvent_NonTransientEvent_LoggedAsError()
        {
            var ex = new ServiceBusException(false);
            Assert.False(ex.IsTransient);
            ExceptionReceivedEventArgs e = new ExceptionReceivedEventArgs(ex, "TestAction", "TestEndpoint", "TestEntity", "TestClient");
            ServiceBusExtensionConfig.LogExceptionReceivedEvent(e, _loggerFactory);

            var expectedMessage = $"MessageReceiver error (Action=TestAction, ClientId=TestClient, EntityPath=TestEntity, Endpoint=TestEndpoint)";
            var logMessage = _loggerProvider.GetAllLogMessages().Single();
            Assert.Equal(LogLevel.Error, logMessage.Level);
            Assert.Same(ex, logMessage.Exception);
            Assert.Equal(expectedMessage, logMessage.FormattedMessage);
        }

        [Fact]
        public void LogExceptionReceivedEvent_TransientEvent_LoggedAsVerbose()
        {
            var ex = new ServiceBusException(true);
            Assert.True(ex.IsTransient);
            ExceptionReceivedEventArgs e = new ExceptionReceivedEventArgs(ex, "TestAction", "TestEndpoint", "TestEntity", "TestClient");
            ServiceBusExtensionConfig.LogExceptionReceivedEvent(e, _loggerFactory);

            var expectedMessage = $"MessageReceiver error (Action=TestAction, ClientId=TestClient, EntityPath=TestEntity, Endpoint=TestEndpoint)";
            var logMessage = _loggerProvider.GetAllLogMessages().Single();
            Assert.Equal(LogLevel.Debug, logMessage.Level);
            Assert.Same(ex, logMessage.Exception);
            Assert.Equal(expectedMessage, logMessage.FormattedMessage);
        }

        [Fact]
        public void LogExceptionReceivedEvent_NonMessagingException_LoggedAsError()
        {
            var ex = new MissingMethodException("What method??");
            ExceptionReceivedEventArgs e = new ExceptionReceivedEventArgs(ex, "TestAction", "TestEndpoint", "TestEntity", "TestClient");
            ServiceBusExtensionConfig.LogExceptionReceivedEvent(e, _loggerFactory);

            var expectedMessage = $"MessageReceiver error (Action=TestAction, ClientId=TestClient, EntityPath=TestEntity, Endpoint=TestEndpoint)";
            var logMessage = _loggerProvider.GetAllLogMessages().Single();
            Assert.Equal(LogLevel.Error, logMessage.Level);
            Assert.Same(ex, logMessage.Exception);
            Assert.Equal(expectedMessage, logMessage.FormattedMessage);
        }
    }
}
