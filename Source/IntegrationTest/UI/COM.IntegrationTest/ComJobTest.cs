﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SystemWrapper.IO;
using SystemWrapper.Microsoft.Win32;
using NSubstitute;
using NUnit.Framework;
using pdfforge.DynamicTranslator;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using pdfforge.PDFCreator.Core.Controller;
using pdfforge.PDFCreator.Core.Printing;
using pdfforge.PDFCreator.Core.Printing.Port;
using pdfforge.PDFCreator.Core.Printing.Printer;
using pdfforge.PDFCreator.Core.Services.Licensing;
using pdfforge.PDFCreator.Core.Services.Logging;
using pdfforge.PDFCreator.Core.Services.Translation;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.UI.COM;
using pdfforge.PDFCreator.UI.ViewModels;
using pdfforge.PDFCreator.Utilities;

namespace pdfforge.PDFCreator.IntegrationTest.UI.COM
{
    [TestFixture]
    internal class ComJobTest
    {
        [SetUp]
        public void SetUp()
        {
            var builder = new ComDependencyBuilder();
            var dependencies = builder.ComDependencies;

            LoggingHelper.InitConsoleLogger("PDFCreatorTest", LoggingLevel.Off);

            var installationPathProvider = new InstallationPathProvider(@"Software\pdfforge\PDFCreator\Settings", @"Software\pdfforge\PDFCreator", "{00000000-0000-0000-0000-000000000000}");

            var settingsProvider = new DefaultSettingsProvider();

            var translationHelper = new TranslationHelper(new TranslationProxy(), settingsProvider, new AssemblyHelper());
            translationHelper.InitTranslator("None");

            var settingsLoader = new SettingsLoader(translationHelper, Substitute.For<ISettingsMover>(), installationPathProvider, Substitute.For<ILanguageDetector>(), Substitute.For<IPrinterHelper>(), Substitute.For<ITranslator>());

            var settingsManager = new SettingsManager(settingsProvider, settingsLoader);
            settingsManager.LoadAllSettings();

            var folderProvider = new FolderProvider(new PrinterPortReader(new RegistryWrap(), new PathWrapSafe()), new PathWrap());

            _queue = new Queue();
            _queue.Initialize();

            _testPageHelper = new TestPageHelper(new AssemblyHelper(), new OsHelper(), folderProvider, dependencies.QueueAdapter.JobInfoQueue, new JobInfoManager(new LocalTitleReplacerProvider(new List<TitleReplacement>())));
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Clear();
            _queue.ReleaseCom();
        }

        private Queue _queue;
        private TestPageHelper _testPageHelper;

        private void CreateTestPages(int n)
        {
            for (var i = 0; i < n; i++)
            {
                _testPageHelper.CreateTestPage();
            }
        }

        [Test]
        public void ConvertTo_IfFilenameDirectoryNotExisting_ThrowsCOMException()
        {
            CreateTestPages(1);

            const string filename = "basdeead\\aokdeaad.pdf";
            var comJob = _queue.NextJob;

            var ex = Assert.Throws<COMException>(() => comJob.ConvertTo(filename));
            StringAssert.Contains("Invalid path. Please check if the directory exists.", ex.Message);
        }

        [Test]
        public void ConvertTo_IfFilenameEmpty_ThrowsArgumentException()
        {
            CreateTestPages(1);

            const string filename = "";
            var comJob = _queue.NextJob;

            Assert.Throws<ArgumentException>(() => comJob.ConvertTo(filename));
        }

        [Test]
        public void ConvertTo_IfFilenameHasIllegalChars_ThrowsArgumentException()
        {
            CreateTestPages(1);

            const string filename = "testpage>testpage.pdf";
            var comJob = _queue.NextJob;

            Assert.Throws<ArgumentException>(() => comJob.ConvertTo(filename));
        }

        [Test]
        public void GetProfileSettings_IfEmptyPropertyname_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;
            var propertyName = "";
            var ex = Assert.Throws<COMException>(() => comJob.GetProfileSetting(propertyName));
            StringAssert.Contains($"The property '{propertyName}' does not exist!", ex.Message);
        }

        [Test]
        public void GetProfileSettings_IfInvalidPropertyname_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;

            var propertyName = "asdioajsd";
            var ex = Assert.Throws<COMException>(() => comJob.GetProfileSetting(propertyName));
            StringAssert.Contains($"The property '{propertyName}' does not exist!", ex.Message);
        }

        [Test]
        public void ProfileSettings_IfEmptyPropertyname_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;

            var propertyName = "";
            var ex = Assert.Throws<COMException>(() => comJob.SetProfileSetting(propertyName, "True"));
            StringAssert.Contains($"The property '{propertyName}' does not exist!", ex.Message);
        }

        [Test]
        public void ProfileSettings_IfNotExistingPropertyname_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;
            var propertyName = "NotExisting";
            var ex = Assert.Throws<COMException>(() => comJob.SetProfileSetting(propertyName, "True"));
            StringAssert.Contains($"The property '{propertyName}' does not exist!", ex.Message);
        }

        [Test]
        [ExpectedException(typeof (FormatException))]
        public void SetProfileSettings_IfEmptyValue_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;
            comJob.SetProfileSetting("PdfSettings.Security.Enabled", "");
        }

        [Test]
        [ExpectedException(typeof (FormatException))]
        public void SetProfileSettings_IfInappropriateValue_ThrowsCOMException()
        {
            CreateTestPages(1);

            var comJob = _queue.NextJob;
            comJob.SetProfileSetting("PdfSettings.Security.Enabled", "3");
        }
    }
}