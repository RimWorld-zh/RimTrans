using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Builder.Wiki {
    public static class WikiTextures {
        public static void Build(string source, string destination, string luaPath) {
            if (!Directory.Exists(source)) {
                Log.Error();
                Log.Write("Directory no found: ");
                Log.WriteLine(ConsoleColor.Red, source);
            }

            if (Directory.Exists(destination)) {
                DirectorySecurity ds = new DirectorySecurity(destination, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, destination);
                    return;
                }
                DirectoryHelper.CleanDirectory(destination, "*.png");
            } else {
                try {
                    Directory.CreateDirectory(destination);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, destination);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start copy textures, source: ");
            Log.Write(ConsoleColor.Cyan, source);
            Log.Write(" destination: ");
            Log.WriteLine(ConsoleColor.Cyan, destination);

            Dictionary<string, List<string>> pathsTable = new Dictionary<string, List<string>>();
            DirectoryInfo dirInfo = new DirectoryInfo(source);
            int countValidFiles = 0;
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories("*", SearchOption.AllDirectories)) {
                if (subDirInfo.FullName.Contains(".git"))
                    continue;
                var fileInfos = subDirInfo.GetFiles();
                if (fileInfos.Count() > 0) {
                    List<string> filesList = new List<string>();
                    foreach (FileInfo fileInfo in fileInfos) {
                        string filePath = fileInfo.FullName.Replace(source, "Textures").Replace('\\', '_');
                        filesList.Add(filePath);
                        string copyTo = Path.Combine(destination, filePath);
                        try {
                            fileInfo.CopyTo(copyTo);
                            countValidFiles++;
                        } catch (Exception) {
                            Log.Error();
                            Log.Write($"Copying texture failed: ");
                            Log.WriteLine(ConsoleColor.Red, filePath);
                        }
                    }
                    string subDirPath = subDirInfo.FullName.Replace(source, "Textures").Replace('\\', '_');
                    pathsTable.Add(subDirPath, filesList);
                }
            }
            if (countValidFiles > 0) {
                Log.Info();
                Log.WriteLine($"Completed copying: {countValidFiles} textures(s).");
            } else {
                Log.Error();
                Log.WriteLine("Copying textures failed.");
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("return {");
            int i = 1;
            int countDir = pathsTable.Count;
            foreach (var kPaths in pathsTable) {
                sb.Append(' ', 4);
                sb.Append(kPaths.Key);
                sb.AppendLine(" = {");
                int j = 1;
                int countFiles = kPaths.Value.Count;
                foreach (var path in kPaths.Value) {
                    sb.Append(' ', 8);
                    sb.Append($"\"{path}\"");
                    if (j < countFiles)
                        sb.Append(',');
                    sb.AppendLine();
                    j++;
                }
                sb.Append(' ', 4);
                sb.Append("}");
                if (i < countDir)
                    sb.Append(',');
                sb.AppendLine();
                i++;
            }
            sb.AppendLine("}");
            string luaDir = Path.GetDirectoryName(luaPath);
            if (!Directory.Exists(luaDir))
                Directory.CreateDirectory(luaDir);

            try {
                File.WriteAllText(luaPath, sb.ToString());
                Log.Info();
                Log.Write($"Completed outputing textures dict: ");
                Log.WriteLine(ConsoleColor.Cyan, luaPath);
            } catch (Exception ex) {
                Log.Error();
                Log.Write($"Outputing textures dict failure: ");
                Log.WriteLine(ConsoleColor.Red, luaPath);
                Log.Indent();
                Log.WriteLine(ex.Message);
            }
        }
    }
}
