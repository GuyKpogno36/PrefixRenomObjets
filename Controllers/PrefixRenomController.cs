using OfficeOpenXml;
using PrefixRenomObjets.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PrefixRenomObjets.Controllers
{
    public class PrefixRenomController : Controller
    {
        private const string fileKeysSessionKey = "fileKeys";
        
        // GET: PrefixRenom
        public ActionResult Index()
        {
            var vm = new PrefixRenom
            {
                listTypesFichiers = new List<TYPE_FICHIERS>
                {
                    new TYPE_FICHIERS { TYPE_FICHIER = ".txt", LIB_TYPE_FICHIER = "Fichier Texte"},
                    new TYPE_FICHIERS { TYPE_FICHIER = ".csv", LIB_TYPE_FICHIER = "Fichier CSV" },
                    new TYPE_FICHIERS { TYPE_FICHIER = ".xlsx", LIB_TYPE_FICHIER = "Fichier Excel" },
                    new TYPE_FICHIERS { TYPE_FICHIER = ".cs", LIB_TYPE_FICHIER = "Fichier C#" },
                    new TYPE_FICHIERS { TYPE_FICHIER = ".sql", LIB_TYPE_FICHIER = "Fichier SQL" },
                }
            };
            
            return View(vm);
        }

        public JsonResult UploadFile()
        {
            HttpRequestBase fileStream = Request;
            var fileFolder = GetCheminFichier();
            ResultSet resultSet = new ResultSet();
            try
            {
                if (fileStream.Files.Count < 1)
                {
                    resultSet.errorMessage = "Veuillez sélectionner un fichier correct";
                    return Json(resultSet);
                }

                var fileContent = fileStream.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    var fileName = fileContent.FileName;
                    var stream = fileContent.InputStream;
                    var extensionTab = fileName.Split('.');
                    var posExtension = extensionTab[extensionTab.Length - 1].Length;
                    var fileNameWithoutExtension = fileName.Substring(0, (fileName.Length - posExtension - 1));
                    
                    var fileExtension = "";
                    if (fileNameWithoutExtension != "") fileExtension = fileName.Substring((fileName.Length - posExtension), posExtension);

                    if (!Directory.Exists(fileFolder))
                    {
                        Directory.CreateDirectory(fileFolder);
                    }

                    using (var localFileStream = System.IO.File.Create(fileFolder + fileName)) { stream.CopyTo(localFileStream); }

                    if (fileStream.Files.Keys[0] == "hideInputFileAppFichierCible") SetKeyValue("Cible", fileFolder + fileName);
                    else SetKeyValue("Liste", fileFolder + fileName);
                }
                else
                {
                    resultSet.errorMessage = "Veuillez sélectionner à nouveau un fichier";
                    return Json(resultSet);
                }
            }
            catch (Exception e)
            {
                resultSet.errorMessage = e.Message;
                return Json(resultSet);
            }
            resultSet.success = true;
            resultSet.statusDetail = "Chargement effectué avec succès !";
            return Json(resultSet);
        }

        [HttpPost]
        public JsonResult UploadFolder()
        {
            var files = Request.Files;
            string selectedFolder = string.Empty;

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                // Traitement du fichier
                if (file != null && file.ContentLength > 0)
                {
                    // Save file to server or perform other processing
                    string path = GetCheminFichier();
                    var fileName = Path.GetFileName(file.FileName);
                    //var fileExtension = Path.GetExtension(file.FileName);
                    //var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                    var dos = file.FileName.Substring(0, file.FileName.Length - (fileName.Length + 1));

                    // Définir le chemin du dossier sélectionné si ce n'est pas déjà fait
                    if (string.IsNullOrEmpty(selectedFolder))
                    {
                        selectedFolder = Path.Combine(path, dos.Split('/').First());
                        SetKeyValue("Dossier", selectedFolder);
                    }

                    // Créer les sous-dossiers si nécessaire
                    string fullPath = Path.Combine(path, dos);
                    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                    
                    if (!Directory.Exists(path + dos)) Directory.CreateDirectory(path + dos);
                    

                    string filePath = Path.Combine(path + dos, fileName);

                    using (var localFileStream = System.IO.File.Create(filePath))
                    {
                        file.InputStream.CopyTo(localFileStream);
                    }
                }
            }

            return Json(new { status = "success" });
        }

        public JsonResult OpererPrefixage(string prefix, string type_fichier = "")
        {
            var fileKeys = GetfileKeys();
            var references = LoadReferences(fileKeys["Liste"]);
            if (type_fichier == "") { 
                ApplyPrefixes(fileKeys["Cible"], references, prefix);
                //Download(fileKeys["Cible"]);
                //Download(Download(GetCheminFichier()););
                return Json(new { file = fileKeys["Cible"], type = "Cible" });
            }
            else
            {
                ApplyPrefixes(fileKeys["Dossier"], references, prefix, type_fichier);
                //Download(fileKeys["Dossier"]);
                return Json(new { file = fileKeys["Dossier"], type = "Dossier" });
            }
            
        }
        
        public FileResult Download(string tempFolderPath)
        {
            // Vérifier si nous devons zipper le dossier
            bool shouldZip = true;
            bool isDirectory = Directory.Exists(tempFolderPath);
            if (!isDirectory) shouldZip = false;
            string uploadFolderPath;

            if (shouldZip)
            {
                // Dossier pour stocker les fichiers avant compression
                uploadFolderPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                Directory.CreateDirectory(uploadFolderPath);

                string zipPath = Path.Combine(tempFolderPath, Guid.NewGuid().ToString() + ".zip");

                // Copie des fichiers et dossiers
                foreach (var directoryPath in Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(directoryPath.Replace(tempFolderPath, uploadFolderPath));
                }

                foreach (var filePath in Directory.GetFiles(tempFolderPath, "*.*", SearchOption.AllDirectories))
                {
                    string destFilePath = filePath.Replace(tempFolderPath, uploadFolderPath);
                    System.IO.File.Copy(filePath, destFilePath, true);
                }

                ZipFile.CreateFromDirectory(uploadFolderPath, zipPath);

                // Supprimer le dossier temporaire après compression
                Directory.Delete(uploadFolderPath, true);

                byte[] zipBytes = System.IO.File.ReadAllBytes(zipPath);

                // Supprimer le fichier zip temporaire après lecture
                System.IO.File.Delete(zipPath);

                return File(zipBytes, "application/zip", "Préfixation_" + Guid.NewGuid().ToString() + ".zip");
            }
            else
            {
                // Téléchargement du fichier unique
                if (tempFolderPath != null)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(tempFolderPath);

                    // Supprimer le fichier temporaire après lecture
                    //System.IO.File.Delete(file);

                    return File(fileBytes, "application/octet-stream", Path.GetFileName(tempFolderPath) + "_" + Guid.NewGuid().ToString());
                }
            }
            return null;
        }

        private Dictionary<string, string> GetfileKeys()
        {
            if (Session[fileKeysSessionKey] == null)
            {
                Session[fileKeysSessionKey] = new Dictionary<string, string>
                {
                    { "Cible", null },
                    { "Liste", null },
                    { "Dossier", null }
                };
            }
            return (Dictionary<string, string>)Session[fileKeysSessionKey];
        }

        // Méthode pour définir une valeur pour une clé
        public void SetKeyValue(string key, string value)
        {
            var fileKeys = GetfileKeys();
            if (fileKeys.ContainsKey(key)) fileKeys[key] = value;

            Session[fileKeysSessionKey] = fileKeys; // Met à jour la session
        }

        public string GetRepertoireCourant()
        {
            return Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/")).ToString();
        }

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string Section_ODBC_MSSQL, string Bloc_DNS, string DSN_Default, StringBuilder p1, int p2, string p3);

        public string GetCheminFichier()
        {
            var res = "";
            StringBuilder Buffer = new StringBuilder(1024);
            Buffer.Append(new String(' ', 1024));
            string exe_path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exe_path = GetRepertoireCourant();
            int lng = Path.GetDirectoryName(exe_path).Length;
            Buffer.Clear();
            Buffer.Append(Path.GetDirectoryName(exe_path));
            var DossierCourant = Buffer.ToString().Substring(0, Math.Min(lng, Buffer.ToString().Length));
            lng = Convert.ToInt32(GetPrivateProfileString("REP PrefixRenomObjets", "REP_PrefixRenomObjets", ".", Buffer, 1024, DossierCourant + "\\Ibis.ini"));
            var RepImageMAILO = Buffer.ToString().Substring((Buffer.ToString().Length - lng), lng);
            //var RepImageMAILO = Strings.Left(Buffer.ToString(), lng);
            res = RepImageMAILO;
            return res;
        }

        public Dictionary<string, string> LoadReferences(string filePath)
        {
            var references = new Dictionary<string, string>();
            var extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".xlsx")
            {
                // Code pour lire le fichier Excel
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        string key = worksheet.Cells[row, 1].Text.Trim();
                        if (!references.ContainsKey(key))
                        {
                            references[key] = key; // Assuming key and value are the same in the reference file
                        }
                    }
                }
            }
            else if (extension == ".txt")
            {
                // Code pour lire le fichier texte
                var lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    string key = line.Trim();
                    if (!references.ContainsKey(key))
                    {
                        references[key] = key; // Assuming key and value are the same in the reference file
                    }
                }
            }
            else return null;
            
            return references;
        }

        public void ApplyPrefixes(string filePath, Dictionary<string, string> references, string prefix, string type_fichier = "")
        {
            bool isDirectory = Directory.Exists(filePath);
            var filesToProcess = new List<string>();
            if (isDirectory)
            {
                if (!string.IsNullOrEmpty(type_fichier))
                {
                    filesToProcess.AddRange(Directory.GetFiles(filePath, $"*.{type_fichier.Replace(".", "")}"));
                    foreach (var file in Directory.GetFiles(filePath))
                    {
                        if (Path.GetExtension(file).Equals(type_fichier, StringComparison.OrdinalIgnoreCase)) filesToProcess.Add(file);
                        else System.IO.File.Delete(file);
                    }
                    foreach (var item in filesToProcess)
                    {
                        ProcessFiles(type_fichier, item, prefix, references);
                    }
                }
            }
            else
            {
                var extension = Path.GetExtension(filePath).ToLower();
                ProcessFiles(extension, filePath, prefix, references);
                //filesToProcess.AddRange(Directory.GetFiles(filePath));
            }

        }

        private void ProcessFiles(string extension, string filePath, string prefix, Dictionary<string, string> references)
        {
            if (extension == ".sql")
            {
                // Code pour préfixer dans un fichier SQL
                ProcessTextFile(filePath, references, prefix);
            }
            else if (extension == ".xlsx")
            {
                // Code pour préfixer dans un fichier Excel
                ProcessExcelFile(filePath, references, prefix);
            }
            else if (extension == ".csv" || extension == ".txt")
            {
                // Code pour préfixer dans un fichier CSV ou texte
                ProcessTextFile(filePath, references, prefix);
            }
            else if (extension == ".cs")
            {
                // Code pour préfixer dans un fichier C#
                ProcessCsFile(filePath, references, prefix);
            }
            else
            {
                throw new NotSupportedException("File type not supported.");
            }
        }

        private void ProcessTextFile(string filePath, Dictionary<string, string> references, string prefix)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (var reference in references)
                {
                    if (lines[i].Contains(reference.Key))
                    {
                        lines[i] = lines[i].Replace(reference.Key, $"{prefix}_{reference.Value}");
                    }
                }
            }
            System.IO.File.WriteAllLines(filePath, lines);
        }
        
        private void ProcessCsFile(string filePath, Dictionary<string, string> references, string prefix)
        {
            string fileContent = System.IO.File.ReadAllText(filePath);

            foreach (var reference in references)
            {
                string newReference = prefix + reference;

                fileContent = fileContent.Replace(reference.ToString(), newReference);
            }
            System.IO.File.WriteAllText(filePath, fileContent);
        }

        private void ProcessExcelFile(string filePath, Dictionary<string, string> references, string prefix)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                for (int row = 1; row <= rowCount; row++)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        string cellValue = worksheet.Cells[row, col].Text;
                        foreach (var reference in references)
                        {
                            if (cellValue.Contains(reference.Key))
                                worksheet.Cells[row, col].Value = cellValue.Replace(reference.Key, $"{prefix}_{reference.Value}");
                        }
                    }
                }
                package.Save();
            }
        }
    }
}