﻿// ***********************************************************************
// Copyright (c) 2012-2014 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// Default implementation of ITestAssemblyRunner
    /// </summary>
    public class DefaultTestAssemblyRunner : ITestAssemblyRunner
    {
        static Logger log = InternalTrace.GetLogger("DefaultTestAssemblyRunner");

        private ITestAssemblyBuilder _builder;
        private ITest _loadedTest;
        private IDictionary _settings;
        private AutoResetEvent _runComplete = new AutoResetEvent(false);

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTestAssemblyRunner"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public DefaultTestAssemblyRunner(ITestAssemblyBuilder builder)
        {
            _builder = builder;
        }

        #endregion

        #region Properties

        /// <summary>
        /// TODO: Documentation needed for property
        /// </summary>
        public ITest LoadedTest
        {
            get
            {
                return _loadedTest;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the tests found in an Assembly
        /// </summary>
        /// <param name="assemblyName">File name of the assembly to load</param>
        /// <param name="settings">Dictionary of option settings for loading the assembly</param>
        /// <returns>True if the load was successful</returns>
        public ITest Load(string assemblyName, IDictionary settings)
        {
            _settings = settings;

            Randomizer.InitialSeed = GetInitialSeed(settings);

            return _loadedTest = _builder.Build(assemblyName, settings);
        }

        /// <summary>
        /// Loads the tests found in an Assembly
        /// </summary>
        /// <param name="assembly">The assembly to load</param>
        /// <param name="settings">Dictionary of option settings for loading the assembly</param>
        /// <returns>True if the load was successful</returns>
        public ITest Load(Assembly assembly, IDictionary settings)
        {
            _settings = settings;

            Randomizer.InitialSeed = GetInitialSeed(settings);

            return _loadedTest = _builder.Build(assembly, settings);
        }

        /// <summary>
        /// Count Test Cases using a filter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>The number of test cases found</returns>
        public int CountTestCases(ITestFilter filter)
        {
            return CountTestCases(_loadedTest, filter);
        }

        /// <summary>
        /// Run selected tests and return a test result. The test is run synchronously,
        /// and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">Interface to receive EventListener notifications.</param>
        /// <param name="filter">A test filter used to select tests to be run</param>
        /// <returns></returns>
        public ITestResult Run(ITestListener listener, ITestFilter filter)
        {
            log.Info("Running tests");
            if (_loadedTest == null)
                throw new InvalidOperationException("The Run method was called but no test has been loaded");

            // Save Console.Out and Error for later restoration
            TextWriter savedOut = Console.Out;
            TextWriter savedErr = Console.Error;

            TestExecutionContext initialContext = CreateTestExecutionContext(_settings);

#if NUNITLITE
            initialContext.Listener = listener;

            WorkItem workItem = WorkItem.CreateWorkItem(_loadedTest, initialContext, filter);
            workItem.Completed += new EventHandler(OnRunCompleted);
            workItem.Execute();

            _runComplete.WaitOne();

            return workItem.Result;
#else
            QueuingEventListener queue = new QueuingEventListener();

            if (_settings.Contains(DriverSettings.CaptureStandardOutput))
                initialContext.Out = new EventListenerTextWriter(queue, TestOutputType.Out);
            if (_settings.Contains(DriverSettings.CaptureStandardError))
                initialContext.Error = new EventListenerTextWriter(queue, TestOutputType.Error);

            initialContext.Listener = queue;

            int levelOfParallelization = _settings.Contains(DriverSettings.NumberOfTestWorkers)
                ? (int)_settings[DriverSettings.NumberOfTestWorkers]
                : _loadedTest.Properties.ContainsKey(PropertyNames.LevelOfParallelization)
                    ? (int)_loadedTest.Properties.Get(PropertyNames.LevelOfParallelization)
                    : Math.Max(Environment.ProcessorCount, 2);

            WorkItemDispatcher dispatcher = null;

            if (levelOfParallelization > 0)
            {
                dispatcher = new WorkItemDispatcher(levelOfParallelization);
                initialContext.Dispatcher = dispatcher;
                // Assembly does not have IApplyToContext attributes applied
                // when the test is built, so  we do it here.
                // TODO: Generalize this
                if (_loadedTest.Properties.ContainsKey(PropertyNames.ParallelScope))
                    initialContext.ParallelScope = 
                        (ParallelScope)_loadedTest.Properties.Get(PropertyNames.ParallelScope) & ~ParallelScope.Self;
            }

            WorkItem workItem = WorkItem.CreateWorkItem(_loadedTest, initialContext, filter);
            workItem.Completed += new EventHandler(OnRunCompleted);

            using (EventPump pump = new EventPump(listener, queue.Events))
            {
                pump.Start();

                if (dispatcher != null)
                {
                    dispatcher.Dispatch(workItem);
                    dispatcher.Start();
                }
                else
                    workItem.Execute();

                _runComplete.WaitOne();
            }

            Console.SetOut(savedOut);
            Console.SetError(savedErr);

            if (dispatcher != null)
            {
                dispatcher.Stop();
                dispatcher = null;
            }

            return workItem.Result;
#endif
        }

        #endregion

        #region Helper Methods

        private void OnRunCompleted(object sender, EventArgs e)
        {
            _runComplete.Set();
        }

        private static TestExecutionContext CreateTestExecutionContext(IDictionary settings)
        {
            TestExecutionContext context = new TestExecutionContext();

            if (settings.Contains(DriverSettings.DefaultTimeout))
                context.TestCaseTimeout = (int)settings[DriverSettings.DefaultTimeout];
            if (settings.Contains(DriverSettings.StopOnError))
                context.StopOnError = (bool)settings[DriverSettings.StopOnError];

            if (settings.Contains(DriverSettings.WorkDirectory))
                context.WorkDirectory = (string)settings[DriverSettings.WorkDirectory];
            else
#if NETCF || SILVERLIGHT
                context.WorkDirectory = Env.DocumentFolder;
#else
                context.WorkDirectory = Environment.CurrentDirectory;
#endif
            return context;
        }

        private int CountTestCases(ITest test, ITestFilter filter)
        {
            if (!test.IsSuite)
                return 1;
            
            int count = 0;
            foreach (ITest child in test.Tests)
                if (filter.Pass(child))
                    count += CountTestCases(child, filter);

            return count;
        }

        private static int GetInitialSeed(IDictionary settings)
        {
            return settings.Contains(DriverSettings.RandomSeed)
                ? (int)settings[DriverSettings.RandomSeed]
                : new Random().Next();
        }

        #endregion
    }
}
