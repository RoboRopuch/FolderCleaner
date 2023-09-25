using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.ComponentModel.Design;
using static System.Net.Mime.MediaTypeNames;

namespace MyNamespace
{


    class Cleaner
    {

        public Cleaner(string[] categories, string sourcedir)
        {
            _categories = categories;
            _sourceDirectory = sourcedir;
        }


        //PRIVATE FIELDS
        private readonly string _sourceDirectory;
        private IEnumerable<string>? _directories;
        private IEnumerable<string>? _files;
        private string[] _categories;


        //GETTERS AND SETTERS
        public string GetSourceDirectory
        {
            get { return _sourceDirectory; }
        }

        public IEnumerable<string>? GetDirectories
        {
            get { return _directories; }
        }

        public IEnumerable<string>? GetFiles
        {
            get { return _files; }
        }

        public string[] GetCategories
        {
            get { return _categories; }
        }

        public string[] SetCategories
        {
            set { _categories = value; }
        }



        //METHODS
        public void PopulateFiles()
        {
            _files = Directory.EnumerateFiles(_sourceDirectory);
        }

        public void PopulateDirectories()
        {
            
            _directories = Directory.EnumerateDirectories(_sourceDirectory);
        }

        public void CreateCategoriesFolders()
        {
            Directory.CreateDirectory(_sourceDirectory + "\\" +"Other files");

            foreach ( var ext in _categories)
            {
                //By default, Directory.CreateDirectory does not throw an error if the directory already exists
                //if it does exist, it won't be overwritten or modified.

                Directory.CreateDirectory( _sourceDirectory + "\\" + ext + " files");
            }

        }

        public void CategorizeFiles() 
        {
            foreach (var file in _files)
            {
                string fileExt = Path.GetExtension(file);

                string destinationDirectory;

                if (_categories.Contains(fileExt))
                {
                    destinationDirectory = _sourceDirectory+ "\\" + fileExt + " files";
                }
                else
                {
                    destinationDirectory = _sourceDirectory + "\\Other files";

                }

                string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(file));


                File.Move(file, destinationFile);

            }
        }

    }
       

    class Program
    {   
        

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            
            //folder source dir
            Console.WriteLine(args[0]);
            
            //
            Console.WriteLine(args[1]);
            Console.WriteLine(args[2]);

            Cleaner clean = new([".rar", ".txt"], @"C:\Users\madzi\OneDrive\Adobe\pukladane");
            
            clean.PopulateDirectories();
            clean.PopulateFiles();

            clean.CreateCategoriesFolders();
            clean.CategorizeFiles();

            ////TODO: ask user for preffered file folders
            ////(user can choose certain folders or
            ////the folders will be created according to files existing in the Downloads)

            ////TODO: what is user want to change folders, change the categorixation;
            ////you cant change folders. U can add new one - then other folder is checked for 
            ////files to transfer, or you can delete one - then all files from the dfolder are transferd to other folder 
            ////do it on flags


 
            Console.WriteLine("press Enter to continue");
            Console.ReadLine();

            //TODO: log system?; multithreading

            //listening for new files to categorize
            using var watcher = new FileSystemWatcher(clean.GetSourceDirectory);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            //TODO: cleaver way of encoding sourtceDirectory
            string sourceDirectory = 
                @"C:\Users\madzi\OneDrive\Adobe\pukladane";
            string value = $"Created: {e.FullPath}";
            var fileExt = Path.GetExtension(e.FullPath);
            var name = Path.GetFileName(e.FullPath);
            File.Move(e.FullPath, Path.Combine(sourceDirectory, fileExt + " files", name));
            Console.WriteLine(value);
            //TODO: what to do when in destination folder there is already file with same name
          
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
    }

}