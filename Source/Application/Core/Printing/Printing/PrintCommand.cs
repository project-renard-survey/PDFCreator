﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using NLog;
using pdfforge.PDFCreator.Core.Printing.Printer;
using pdfforge.PDFCreator.Utilities;
using pdfforge.PDFCreator.Utilities.Process;

namespace pdfforge.PDFCreator.Core.Printing.Printing
{
    public enum PrintType
    {
        Print,
        PrintTo,
        Unprintable
    }

    /// <summary>
    ///     The PrintCommand class provides a way to print a file with PDFCreator
    /// </summary>
    public class PrintCommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IFileAssoc _fileAssoc;

        /// <summary>
        ///     Create a new PrintCommand for the given file
        /// </summary>
        /// <param name="filename">The full path to the file that shall be printed</param>
        /// <param name="printer">The printer the command will print to</param>
        public PrintCommand(string filename, string printer, IFileAssoc fileAssoc)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            Filename = filename;
            Printer = printer;
            _fileAssoc = fileAssoc;

            if (!File.Exists(filename))
            {
                CommandType = PrintType.Unprintable;
                return;
            }

            var extension = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(extension))
            {
                CommandType = PrintType.Unprintable;
                return;
            }

            if (!SupportsPrint() && !SupportsPrintTo())
                CommandType = PrintType.Unprintable;
            else
            {
                CommandType = SupportsPrintTo() ? PrintType.PrintTo : PrintType.Print;
            }
        }

        public ProcessWrapperFactory ProcessWrapperFactory { get; set; } = new ProcessWrapperFactory();

        public PrintType CommandType { get; }

        public string Filename { get; }

        public bool Successful { get; private set; }

        public bool IsPrintable
        {
            get { return CommandType != PrintType.Unprintable; }
        }

        public bool RequiresDefaultPrinter
        {
            get { return CommandType != PrintType.PrintTo; }
        }

        public string Printer { get; }

        /// <summary>
        ///     Prints the file.
        /// </summary>
        /// <returns>true, if printing was successful</returns>
        public bool Print()
        {
            return Print(TimeSpan.FromSeconds(60));
        }

        /// <summary>
        ///     Prints the file.
        /// </summary>
        /// <param name="processTimeout">The timespan to wait for the process to finish</param>
        /// <returns>true, if printing was successful</returns>
        public bool Print(TimeSpan processTimeout)
        {
            if (CommandType == PrintType.Unprintable)
                throw new InvalidOperationException("File is not printable");

            var printerHelper = new PrinterHelper();
            if (CommandType == PrintType.Print && Printer != printerHelper.GetDefaultPrinter())
                throw new InvalidOperationException("The default printer needs to be set in order to print this file");

            var p = ProcessWrapperFactory.BuildProcessWrapper(new ProcessStartInfo(Filename));

            if (SupportsPrintTo())
            {
                Logger.Debug("Launch PrintTo for \"" + Filename + "\"");
                p.StartInfo.Verb = "Printto";
                p.StartInfo.Arguments = "\"" + Printer + "\"";
            }
            else
            {
                Logger.Debug("Launch Print for \"" + Filename + "\"");
                p.StartInfo.Verb = "Print";
            }

            try
            {
                p.Start();
                p.WaitForExit(processTimeout);

                if (!p.HasExited)
                {
                    Logger.Warn("Process was not finishing after {0} seconds, killing it now...", processTimeout.TotalSeconds);
                    p.Kill();
                }
                else
                {
                    Successful = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception during printing"
                             + "\r\nType: " + ex.GetType()
                             + "\r\nMessage: " + ex.Message
                    );
                return false;
            }

            return Successful;
        }

        private bool SupportsPrint()
        {
            return _fileAssoc.HasPrint(Path.GetExtension(Filename));
        }

        private bool SupportsPrintTo()
        {
            return _fileAssoc.HasPrintTo(Path.GetExtension(Filename));
        }
    }
}