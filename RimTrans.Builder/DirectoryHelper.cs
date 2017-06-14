using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder {
    public static class DirectoryHelper {
        /// <summary>
        /// Delete Empty Directory Recursively
        /// </summary>
        public static void DeleteEmptyDirRecursively(DirectoryInfo dirInfo) {
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories()) {
                DeleteEmptyDirRecursively(subDirInfo);
            }
            try {
                dirInfo.Delete();
            } catch (Exception) {
            }
        }

        /// <summary>
        /// Delete all files in directory according to searchPattern (e.g. "*.txt"), as well as this directory and all empty subdirectories in this directory.
        /// </summary>
        public static void CleanDirectory(string path, string searchPattern) {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Cleaning directory failed: No write permission to directory.");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                }

                Log.Info();
                Log.Write("Cleaning \"{0}\" in directory: ", searchPattern);
                Log.WriteLine(ConsoleColor.Cyan, path);

                // Delete files
                int countValidFiles = 0;
                int countInvalidFiles = 0;
                foreach (FileInfo fileInfo in dirInfo.GetFiles(searchPattern, SearchOption.AllDirectories)) {
                    try {
                        fileInfo.Delete();
                        countValidFiles++;
                    } catch (Exception ex) {
                        Log.Error();
                        Log.Write("Deleting file failed: ");
                        Log.WriteLine(ConsoleColor.Red, fileInfo.FullName);
                        Log.Indent();
                        Log.WriteLine(ex.Message);
                        countInvalidFiles++;
                    }
                }

                // Delete this directory and all subdirectoies in this directory
                //DeleteEmptyDirRecursively(dirInfo); // Recursively

                if (countValidFiles > 0) {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Completed cleaning: {0} file(s).", countValidFiles);
                    } else {
                        Log.Warning();
                        Log.WriteLine("Completed cleaning: Success: {0} file(s), Failure: {1} file(s).", countValidFiles, countInvalidFiles);
                    }
                } else {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Directory is empty.");
                    } else {
                        Log.Error();
                        Log.WriteLine("Cleaning failed: {0} file(s).", countInvalidFiles);
                    }
                }
            } else {
                Log.Info();
                Log.WriteLine("Directory \"{0}\" does not extist, nothing to be delete.", Path.GetFileName(path));
                //Log.WriteLine(ConsoleColor.Cyan, path);
            }
        }

        /// <summary>
        /// Copy all files from source directory to target directory, according to searchPattern (e.g. "*.txt"), no overwrite.
        /// </summary>
        public static void CopyDirectoryEx(string source, string target, string searchPattern) {
            DirectoryInfo dirInfoSource = new DirectoryInfo(source);
            DirectoryInfo dirInfoTarget = new DirectoryInfo(target);
            if (dirInfoSource.Exists) {
                if (Directory.Exists(target)) {
                    DirectorySecurity dsTarget = new DirectorySecurity(target, AccessControlSections.Access);
                    if (dsTarget.AreAccessRulesProtected) {
                        Log.Error();
                        Log.WriteLine("Copying \"{0}\" files failed: No write permission to target directory.", searchPattern);
                        Log.Indent();
                        Log.Write("Source directory: ");
                        Log.WriteLine(ConsoleColor.Cyan, source);
                        Log.Indent();
                        Log.Write("Target directory: ");
                        Log.WriteLine(ConsoleColor.Red, target);
                        return;
                    }
                } else {
                    try {
                        Directory.CreateDirectory(target);
                    } catch (Exception ex) {
                        Log.Error();
                        Log.WriteLine("Copying files failed: Can not create directory: ", searchPattern);
                        Log.Indent();
                        Log.WriteLine(ConsoleColor.Red, target);
                        Log.Indent();
                        Log.WriteLine(ex.Message);
                        return;
                    }
                }

                Log.Info();
                Log.WriteLine("Copying \"{0}\" files:", searchPattern);
                Log.Indent();
                Log.Write("Source directory: ");
                Log.WriteLine(ConsoleColor.Cyan, source);
                Log.Indent();
                Log.Write("Target directory: ");
                Log.WriteLine(ConsoleColor.Cyan, target);
                int countExisted = 0;
                int countNew = 0;
                int countInvalidFiles = 0;
                int splitIndex = source.Length + 1;
                foreach (FileInfo fileInfoSource in dirInfoSource.GetFiles(searchPattern, SearchOption.AllDirectories)) {
                    string fileTarget = Path.Combine(target, fileInfoSource.FullName.Substring(splitIndex));
                    string subDirTarget = Path.GetDirectoryName(fileTarget);
                    if (subDirTarget != target) {
                        if (!Directory.Exists(subDirTarget)) {
                            try {
                                Directory.CreateDirectory(subDirTarget);
                            } catch (Exception ex) {
                                Log.Error();
                                Log.Write("Creating sub-directory failed: ");
                                Log.WriteLine(ConsoleColor.Red, subDirTarget);
                                Log.Indent();
                                Log.WriteLine(ex.Message);
                                countInvalidFiles++;
                                continue;
                            }
                        }
                    }
                    if (File.Exists(fileTarget)) {
                        countExisted++;
                    } else {
                        try {
                            fileInfoSource.CopyTo(fileTarget);
                            countNew++;
                        } catch (Exception ex) {
                            Log.Error();
                            Log.Write("Copying failed, target: ");
                            Log.WriteLine(ConsoleColor.Red, fileTarget);
                            Log.Indent();
                            Log.WriteLine(ex.Message);
                            countInvalidFiles++;
                        }
                    }
                }
                if (countExisted + countNew > 0) {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Completed copying \"{0}\" file(s): {1} existed, {2} new.",
                            searchPattern, countExisted, countNew);
                    }
                    if (countInvalidFiles > 0) {
                        Log.Warning();
                        Log.WriteLine("Completed copying \"{0}\" file(s): {1} existed, {2} new, {3} failed.",
                            searchPattern, countExisted, countNew, countInvalidFiles);
                    }
                } else {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("No \"{0}\" file to be copyed.", searchPattern);
                    } else {
                        Log.Error();
                        Log.WriteLine("Copying \"{0}\" file(s) failed: {1} file(s).", searchPattern);
                    }
                }
            } else {
                Log.Info();
                Log.Write("Directory \"{0}\" does not extist: ", Path.GetFileName(source));
                Log.WriteLine(ConsoleColor.Cyan, source);
            }
        }
    }
}
