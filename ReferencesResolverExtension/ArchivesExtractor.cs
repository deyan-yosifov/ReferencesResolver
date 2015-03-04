using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Telerik.ReferencesResolverExtension
{
    public sealed class ArchivesExtractor : IDisposable
    {
        private const string ExtractDirName = "RefsResolverExtractionDir";
        private readonly Action<string> onFolderExtractedSuccessResult;
        private readonly Action<Exception> onExceptionResult;
        private readonly BackgroundWorker worker;

        public ArchivesExtractor(Action<string> onFolderExtractedSuccessResult, Action<Exception> onExceptionResult)
        {
            this.worker = new BackgroundWorker();
            this.onFolderExtractedSuccessResult = onFolderExtractedSuccessResult;
            this.onExceptionResult = onExceptionResult;
        }

        public static string ExtractorDocumentsFolder
        {
            get
            {
                string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string extractionFolder = Path.Combine(documentsFolder, ExtractDirName);

                if (!Directory.Exists(extractionFolder))
                {
                    Directory.CreateDirectory(extractionFolder);
                }

                return extractionFolder;
            }
        }

        public void ExtractZip(string fileName)
        {
            if (this.worker.IsBusy)
            {
                return;
            }

            this.AttachWorkerToZipExtractionEvents();
            this.worker.RunWorkerAsync(fileName); 
        }

        private void ZipExtractor_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {                
                string zipFileName = (string)e.Argument;
                string extractFolder = GetExtractionFolder(zipFileName);
                Directory.CreateDirectory(extractFolder);

                System.IO.Compression.ZipFile.ExtractToDirectory(zipFileName, extractFolder);
                e.Result = extractFolder;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ZipExtractor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DetachWorkerFromZipExtractionEvents();

            Exception exception = e.Result as Exception;

            if (exception != null)
            {
                this.onExceptionResult(exception);
            }
            else
            {
                this.onFolderExtractedSuccessResult((string)e.Result);                
            }
        }

        private void AttachWorkerToZipExtractionEvents()
        {
            this.worker.DoWork += this.ZipExtractor_DoWork;
            this.worker.RunWorkerCompleted += this.ZipExtractor_RunWorkerCompleted;
        }

        private void DetachWorkerFromZipExtractionEvents()
        {
            this.worker.DoWork -= this.ZipExtractor_DoWork;
            this.worker.RunWorkerCompleted -= this.ZipExtractor_RunWorkerCompleted;
        }

        private static string GetExtractionFolder(string zipFileName)
        {
            string documentsFolder = ArchivesExtractor.ExtractorDocumentsFolder;
            string shortZipFileName = Path.GetFileNameWithoutExtension(zipFileName);
            string extractionFolder = Path.Combine(documentsFolder, shortZipFileName);

            if (Directory.Exists(extractionFolder))
            {
                int count = 0;

                while (Directory.Exists(ArchivesExtractor.GetNumberedFolderName(extractionFolder, count)))
                {
                    count++;
                }

                extractionFolder = ArchivesExtractor.GetNumberedFolderName(extractionFolder, count);
            }

            return extractionFolder;
        }

        private static string GetNumberedFolderName(string folder, int number)
        {
            return string.Format("{0}--{1}", folder, number);
        }

        public void Dispose()
        {
            this.worker.Dispose();
        }
    }
}
