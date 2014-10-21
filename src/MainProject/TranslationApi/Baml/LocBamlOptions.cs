using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Security;

namespace TranslationApi.Baml
{
    internal sealed class LocBamlOptions
    {
        internal string InputFile;
        internal string OutputFileOrDirectory;
        internal CultureInfo CultureInfo;
        internal string TranslationsFile;

        internal bool ToParse;
        internal bool ToGenerate;
        internal bool HasNoLogo;
        internal bool IsVerbose;
        internal FileType TranslationFileType;
        internal FileType InputType;
        internal ArrayList AssemblyPaths;
        internal Assembly[] Assemblies;

        /// <summary>
        /// return true if the operation succeeded.
        /// otherwise, return false
        /// </summary>
        internal string CheckAndSetDefault()
        {
            // we validate the options here and also set default
            // if we can

            // Rule #1: One and only one action at a time
            // i.e. Can't parse and generate at the same time
            //      Must do one of them
            if ((ToParse && ToGenerate) ||
                (!ToParse && !ToGenerate))
                return StringLoader.Get("MustChooseOneAction");

            // Rule #2: Must have an input 
            if (string.IsNullOrEmpty(InputFile))
            {
                return StringLoader.Get("InputFileRequired");
            }
            else
            {
                if (!File.Exists(InputFile))
                {
                    return StringLoader.Get("FileNotFound", InputFile);
                }

                string extension = Path.GetExtension(InputFile);

                // Get the input file type.
                if (string.Compare(extension, "." + FileType.BAML.ToString(), true, CultureInfo.InvariantCulture) == 0)
                {
                    InputType = FileType.BAML;
                }
                else if (string.Compare(extension, "." + FileType.RESOURCES.ToString(), true, CultureInfo.InvariantCulture) == 0)
                {
                    InputType = FileType.RESOURCES;
                }
                else if (string.Compare(extension, "." + FileType.DLL.ToString(), true, CultureInfo.InvariantCulture) == 0)
                {
                    InputType = FileType.DLL;
                }
                else if (string.Compare(extension, "." + FileType.EXE.ToString(), true, CultureInfo.InvariantCulture) == 0)
                {
                    InputType = FileType.EXE;
                }
                else
                {
                    return StringLoader.Get("FileTypeNotSupported", extension);
                }
            }

            if (ToGenerate)
            {
                // Rule #3: before generation, we must have Culture string
                if (CultureInfo == null && InputType != FileType.BAML)
                {
                    // if we are not generating baml, 
                    return StringLoader.Get("CultureNameNeeded", InputType.ToString());
                }

                // Rule #4: before generation, we must have translation file
                if (string.IsNullOrEmpty(TranslationsFile))
                {

                    return StringLoader.Get("TranslationNeeded");
                }
                else
                {
                    string extension = Path.GetExtension(TranslationsFile);

                    if (!File.Exists(TranslationsFile))
                    {
                        return StringLoader.Get("TranslationNotFound", TranslationsFile);
                    }
                    else
                    {
                        if (string.Compare(extension, "." + FileType.CSV.ToString(), true, CultureInfo.InvariantCulture) == 0)
                        {
                            TranslationFileType = FileType.CSV;
                        }
                        else
                        {
                            TranslationFileType = FileType.TXT;
                        }
                    }
                }
            }



            // Rule #5: If the output file name is empty, we act accordingly
            if (string.IsNullOrEmpty(OutputFileOrDirectory))
            {
                // Rule #5.1: If it is parse, we default to [input file name].csv
                if (ToParse)
                {
                    string fileName = Path.GetFileNameWithoutExtension(InputFile);
                    OutputFileOrDirectory = fileName + "." + FileType.CSV.ToString();
                    TranslationFileType = FileType.CSV;
                }
                else
                {
                    // Rule #5.2: If it is generating, and the output can't be empty
                    return StringLoader.Get("OutputDirectoryNeeded");
                }

            }
            else
            {
                // output isn't null, we will determind the Output file type                
                // Rule #6: if it is parsing. It will be .csv or .txt.
                if (ToParse)
                {
                    string fileName;
                    string outputDir;

                    if (Directory.Exists(OutputFileOrDirectory))
                    {
                        // the output is actually a directory name
                        fileName = string.Empty;
                        outputDir = OutputFileOrDirectory;
                    }
                    else
                    {
                        // get the extension
                        fileName = Path.GetFileName(OutputFileOrDirectory);
                        outputDir = Path.GetDirectoryName(OutputFileOrDirectory);
                    }

                    // Rule #6.1: if it is just the output directory
                    // we append the input file name as the output + csv as default
                    if (string.IsNullOrEmpty(fileName))
                    {
                        TranslationFileType = FileType.CSV;
                        OutputFileOrDirectory = outputDir
                               + Path.DirectorySeparatorChar
                               + Path.GetFileName(InputFile)
                               + "."
                               + TranslationFileType.ToString();
                    }
                    else
                    {
                        // Rule #6.2: if we have file name, check the extension.
                        string extension = Path.GetExtension(OutputFileOrDirectory);

                        // ignore case and invariant culture
                        if (string.Compare(extension, "." + FileType.CSV.ToString(), true, CultureInfo.InvariantCulture) == 0)
                        {
                            TranslationFileType = FileType.CSV;
                        }
                        else
                        {
                            // just consider the output as txt format if it doesn't have .csv extension
                            TranslationFileType = FileType.TXT;
                        }
                    }
                }
                else
                {
                    // it is to generate. And Output should point to the directory name.                    
                    if (!Directory.Exists(OutputFileOrDirectory))
                        return StringLoader.Get("OutputDirectoryError", OutputFileOrDirectory);
                }
            }

            // Rule #7: if the input assembly path is not null
            if (AssemblyPaths != null && AssemblyPaths.Count > 0)
            {
                Assemblies = new Assembly[AssemblyPaths.Count];
                for (int i = 0; i < Assemblies.Length; i++)
                {
                    string errorMsg = null;
                    try
                    {
                        // load the assembly
                        Assemblies[i] = Assembly.LoadFrom((string)AssemblyPaths[i]);
                    }
                    catch (ArgumentException argumentError)
                    {
                        errorMsg = argumentError.Message;
                    }
                    catch (BadImageFormatException formatError)
                    {
                        errorMsg = formatError.Message;
                    }
                    catch (FileNotFoundException fileError)
                    {
                        errorMsg = fileError.Message;
                    }
                    catch (PathTooLongException pathError)
                    {
                        errorMsg = pathError.Message;
                    }
                    catch (SecurityException securityError)
                    {

                        errorMsg = securityError.Message;
                    }

                    if (errorMsg != null)
                    {
                        return errorMsg; // return error message when loading this assembly
                    }
                }
            }

            // if we come to this point, we are all fine, return null error message
            return null;
        }


        /// <summary>
        /// Write message line depending on IsVerbose flag
        /// </summary>
        internal void WriteLine(string message)
        {
            if (IsVerbose)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Write the message depending on IsVerbose flag
        /// </summary>        
        internal void Write(string message)
        {
            if (IsVerbose)
            {
                Console.Write(message);
            }
        }
    }
}
