(*
namespace apnea

module RunEdfReaderEXE = 
*)
    open System.IO

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

    printfn "start ............"
    let workingDir = prepareWorkingDirectory configPathWorkingDirectory  false configEdf2AsciiExeName  
    printfn "Wdir %s" workingDir
    printfn "done ............"

