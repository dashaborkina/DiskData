using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace _27число
{
    class Program
    {
        private static DirectoryInfo _rootDirectory;
        private static string[] _specDirectories = new string[] { "Изображения", "Документы", "Прочее" };
        private static int _imagesCount = 0, _documentsCount = 0, _othersCount = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь:");
            string directPath = Console.ReadLine(); //путь

            var driveInfo = new DriveInfo(directPath);
            Console.WriteLine($"Disc: {driveInfo.VolumeLabel }");
            Console.WriteLine($"All: {driveInfo.TotalSize/1024/1024/1024 } gb");
            Console.WriteLine($"Free space: {driveInfo.AvailableFreeSpace/1024/1024/1024 } gb");

            _rootDirectory = driveInfo.RootDirectory;
            SearchDirectories(_rootDirectory);

            foreach (var directory in _rootDirectory.GetDirectories())
            {
                if (!_specDirectories.Contains(directory.Name))
                    directory.Delete(true);
            }

            var resultText = $"Всего обработано {_imagesCount + _documentsCount + _othersCount} файлов" +
                $"Из них {_imagesCount} изображений, {_documentsCount} документов, {_othersCount} прочих файлов";
            Console.WriteLine(resultText);
            File.WriteAllText(_rootDirectory + "\\Информация.txt", resultText);
            Console.ReadKey();
        }

        private static void SearchDirectories(DirectoryInfo currentDirectory)
        { 
            if (!_specDirectories.Contains(currentDirectory.Name))
            {
                FilterFiles(currentDirectory);
                foreach (var childDirectory in currentDirectory.GetDirectories())
                {
                    SearchDirectories(childDirectory);
                }
            }
        }

        private static void FilterFiles(DirectoryInfo currentDirectory)
        {
            var currentFiles = currentDirectory.GetFiles();

            foreach (var fileinfo in currentFiles)
            {
                if (new string[] {".jpg", ".jpeg", ".png", ".svg"}.Contains(fileinfo.Extension.ToLower()))
                {
                    var photoDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[0]}\\");
                    if (!photoDirectory.Exists)
                        photoDirectory.Create();

                    var yearDirectory = new DirectoryInfo(photoDirectory + $"{fileinfo.LastWriteTime.Date.Year}\\");
                    if (!yearDirectory.Exists)
                        yearDirectory.Create();

                    MoveFiles(fileinfo, yearDirectory);
                    _imagesCount++;
                }
                else if (new string[] {".doc",".docx",".pdf",".xls",".xlsx", ".ppt", ".pptx"}.Contains(fileinfo.Extension.ToLower()))
                {
                    var documentsDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[1]}\\");
                    if (documentsDirectory.Exists)
                        documentsDirectory.Create();

                    DirectoryInfo lengthDirectory = null;
                    if (fileinfo.Length / 1024 / 1024 < 1)
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "Менее 1 МБ\\");
                    else if (fileinfo.Length / 1024 / 1024 / 1024 > 10)
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "Более 10 МБ\\");
                    else
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "От 1 до 10 МБ\\");
                    if (!lengthDirectory.Exists)
                        lengthDirectory.Create();

                    MoveFiles(fileinfo, lengthDirectory);
                    _documentsCount++;
                }
                else
                {
                    var otherDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[2]}\\");
                    if (!otherDirectory.Exists)
                        otherDirectory.Create();

                    MoveFiles(fileinfo, otherDirectory);
                    _othersCount++;
                }
            }
        }

        private static void MoveFiles(FileInfo fileinfo, DirectoryInfo directoryInfo)
        {
            var newFileInfo = new FileInfo(directoryInfo + $"\\{fileinfo.Name}");
            while (newFileInfo.Exists)
                newFileInfo = new FileInfo(directoryInfo + $"\\{Path.GetFileNameWithoutExtension(fileinfo.FullName)} (1)"
                    + $"{newFileInfo.Extension}");
            fileinfo.MoveTo(newFileInfo.FullName);
        }
    }
}
