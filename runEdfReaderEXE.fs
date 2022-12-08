(*
namespace apnea

module RunEdfReaderEXE = 
*)
    open System.IO
    open System.Threading.Tasks
    open System.Diagnostics // ProcessStartInfo

    let configEdf2AsciiExeName = "edf2ascii.exe"
    let configPathWorkingDirectory = "edfreader"

    let deleteFile (keepExe : bool) (exeFileName : string) (fileInfoToDelete : FileInfo)  =
        if keepExe && fileInfoToDelete.Name.Contains(exeFileName)
        then 
            ()
        else
            fileInfoToDelete.Delete()

    let clearDirectory (pathDirectory : string) (keepExeFile : bool) (exeFileName : string)   : unit =
        printfn "path %s" pathDirectory
        let di = DirectoryInfo(pathDirectory)
        let fi= di.GetFiles()         //di.GetFiles
        fi |> Array.map(deleteFile keepExeFile exeFileName) |> ignore
  
    
    let prepareWorkingDirectory  (pathWorkingDirectory : string) (keepExeFile : bool) (exeFileName : string): string =
        let directory = Path.Combine(Path.GetTempPath(), pathWorkingDirectory)
        printfn "Path working directory: %s" directory
        if Directory.Exists(directory) 
        then 
            clearDirectory directory keepExeFile exeFileName 
        else
            Directory.CreateDirectory(directory)
            ()
        directory

    open System.Threading.Tasks
    open System.Diagnostics
    let runExeProcess (pathToExe : string) (pathFileToConvert : string) (pathWorkDir : string)=
        // run the exe with the input file name
(*         let procInfo = ProcessStartInfo(
                FileName = pathToExe,
                Arguments = pathFileToConvert,
                WorkingDirectory = pathWorkDir,
                CreateNoWindow = true)
 *)     
        let proc = new Process()
        proc.StartInfo.FileName = pathToExe
        proc.StartInfo.Arguments = pathFileToConvert
        proc.StartInfo.WorkingDirectory = pathWorkDir
        proc.StartInfo.CreateNoWindow = true

        proc.Start()
        proc.WaitForExit()

    runExeProcess @"d:\Documents\programming\c#\CPAPGraphsCSharp\EDFFileReader\edf2ascii\edf2ascii.exe"  @"D:\TEMP2\20220712\20220628_235657_EVE.edf" @"D:\TEMP2\20220712"
(*     printfn "start ............"
    let workingDir = prepareWorkingDirectory configPathWorkingDirectory  false configEdf2AsciiExeName  
    printfn "Wdir %s" workingDir
    printfn "done ............"
 *)
