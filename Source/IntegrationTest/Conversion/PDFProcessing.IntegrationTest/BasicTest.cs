﻿using System;
using System.IO;
using SystemWrapper.IO;
using NUnit.Framework;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Processing.ITextProcessing;
using pdfforge.PDFCreator.Conversion.Processing.PdfProcessingInterface;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.Enums;
using pdfforge.PDFCreator.Editions.PDFCreator;
using PDFCreator.TestUtilities;

namespace pdfforge.PDFCreator.IntegrationTest.Conversion.PDFProcessing
{
    [TestFixture]
    [Category("LongRunning")]
    internal class BasicTest
    {
        [SetUp]
        public void SetUp()
        {
            var bootstrapper = new IntegrationTestBootstrapper();
            var container = bootstrapper.ConfigureContainer();
            _th = container.GetInstance<TestHelper>();
            _th.InitTempFolder("PDFProcessing basic test");
            PdfProcessor = new ITextPdfProcessor(new FileWrap(), new DefaultProcessingPasswordsProvider());
        }

        [TearDown]
        public void CleanUp()
        {
            _th.CleanUp();
        }

        private TestHelper _th;

        private ITextPdfProcessor PdfProcessor { get; set; }

        [Test]
        public void CheckIfFileExistsAndTempFilesAreDeleted_AllPdfPropertiesDisabled()
        {
            var pdfFile = _th.GenerateTestFile(TestFile.PDFCreatorTestpagePDF);

            var profile = new ConversionProfile();
            profile.OutputFormat = OutputFormat.Pdf;
            //UpdateXMPMetadata enabled if format is not PDF/A
            profile.PdfSettings.Security.Enabled = false;
            profile.PdfSettings.Signature.Enabled = false;
            profile.BackgroundPage.Enabled = false;
            
            var job = new Job(new JobInfo(), profile, new JobTranslations(), new Accounts());
            job.TempOutputFiles.Add(pdfFile);

            PdfProcessor.ProcessPdf(job);

            Assert.IsTrue(File.Exists(pdfFile), "Processed file does not exist.");
            File.Delete(pdfFile);
            var files = Directory.GetFiles(_th.TmpTestFolder);
            Assert.IsEmpty(files, "TmpTestFolder should be empty, after deleting processed file, but contained: "
                                  + Environment.NewLine + files);
        }

        [Test]
        public void DenyProcessingForJPEG_with_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Jpeg);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            _th.Job.Profile.BackgroundPage.Enabled = true;
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsFalse(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void DenyProcessingForPDF_without_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Pdf);
            _th.Job.Profile.PdfSettings.Security.Enabled = false;
            _th.Job.Profile.BackgroundPage.Enabled = false;
            _th.Job.Profile.PdfSettings.Signature.Enabled = false;
            Assert.IsFalse(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void DenyProcessingForPdfX_without_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfX);
            _th.Job.Profile.PdfSettings.Security.Enabled = false;
            _th.Job.Profile.BackgroundPage.Enabled = false;
            _th.Job.Profile.PdfSettings.Signature.Enabled = false;
            Assert.IsFalse(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void DenyProcessingForPng_with_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Png);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            _th.Job.Profile.BackgroundPage.Enabled = true;
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsFalse(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void DenyProcessingForTif_with_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Tif);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            _th.Job.Profile.BackgroundPage.Enabled = true;
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsFalse(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void PdfVersionTest_DefaultProfile_VersionIs4()
        {
            var profile = new ConversionProfile();
            var version = PdfProcessor.DeterminePdfVersion(profile.PdfSettings);
            Assert.AreEqual("1.4", version, "Wrong PDFVersion for default Profile");
        }

        [Test]
        public void PdfVersionTest_ProfileWith_EnabledSecurityAnd128AesLevel_VersionIs6()
        {
            var profile = new ConversionProfile();
            profile.PdfSettings.Security.Enabled = true;
            profile.PdfSettings.Security.EncryptionLevel = EncryptionLevel.Aes128Bit;
            var version = PdfProcessor.DeterminePdfVersion(profile.PdfSettings);
            Assert.AreEqual("1.6", version, "Wrong PDFVersion for enabled security with Aes128Bit");
        }

        [Test]
        public void RequireProcessingForPdf_with_Backgroundpage()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Pdf);
            _th.Job.Profile.BackgroundPage.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPDF_with_Backgroundpage()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfX);
            _th.Job.Profile.BackgroundPage.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPDF_with_Encryption()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Pdf);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPDF_with_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Pdf);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            _th.Job.Profile.BackgroundPage.Enabled = true;
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPDF_with_Signing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.Pdf);
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPdfA_without_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfA2B);
            _th.Job.Profile.PdfSettings.Security.Enabled = false;
            _th.Job.Profile.BackgroundPage.Enabled = false;
            _th.Job.Profile.PdfSettings.Signature.Enabled = false;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPdfX_with_Encryption()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfX);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPDFX_with_Encryption_Backgroundpage_Singing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfX);
            _th.Job.Profile.PdfSettings.Security.Enabled = true;
            _th.Job.Profile.BackgroundPage.Enabled = true;
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void RequireProcessingForPdfX_with_Signing()
        {
            _th.GenerateGsJob(PSfiles.PDFCreatorTestpage, OutputFormat.PdfX);
            _th.Job.Profile.PdfSettings.Signature.Enabled = true;
            Assert.IsTrue(PdfProcessor.ProcessingRequired(_th.Job.Profile));
        }

        [Test]
        public void TestingWithJob_CheckIfFileExistsAndTempFilesAreDeleted_AllPdfPropertiesDisabled()
        {
            _th.Profile.OutputFormat = OutputFormat.Pdf; //UpdateXMPMetadata enabled if format is not PDF/A
            _th.Profile.PdfSettings.Security.Enabled = false;
            _th.Profile.PdfSettings.Signature.Enabled = false;
            _th.Profile.BackgroundPage.Enabled = false;

            _th.GenerateGsJob_WithSettedOutput(TestFile.PDFCreatorTestpagePDF);
            File.Delete(_th.TmpInfFile);
            foreach (var psFile in _th.TmpPsFiles)
                File.Delete(psFile);

            PdfProcessor.ProcessPdf(_th.Job);

            foreach (var file in _th.Job.OutputFiles)
            {
                Assert.IsTrue(File.Exists(file), "File does not exist after processing: " + file);
                File.Delete(file);
            }

            var files = Directory.GetFiles(_th.TmpTestFolder);
            Assert.IsEmpty(files, "TmpTestFolder should be empty, after deleting processed file, but contained: "
                                  + Environment.NewLine + files);
        }
    }
}