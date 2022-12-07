namespace apnea

// like an apnea machine, sleep starts on a given date and goes over into the next day
// so start date is the key, and times are consecutive after the start date
// start "2010-12-21" events at 23:32, 23:45, 1:23, and 3:49

module ApneaModel =
    type ApneaEventType =
        | Apnea
        | Obstructive
        | Unknown

    
(*     let apneaEventTypeToString (eType: ApneaEventType) = 
        match eType with 
        | Apnea -> "Apnea"
        | Obstructive -> "Obstructive"
        | Unknown -> "Obstructive" *)
        

    // date, no time
    type SleepStartDate = SleepStartDate of string
    let sleepStartDateToString = function SleepStartDate x -> x

    // time, no date
    type TimeString = TimeString of string
    let timeStringToString = function TimeString x-> x

    type ApneaEvent = {
        EventType: ApneaEventType
        StartDate: SleepStartDate
        EventTime: TimeString
    }

    // start date is start date of sleep, not session
    type CpapSession = {
        StartDate: SleepStartDate
        StartTime: TimeString
        ApneaEvents: ApneaEvent list
    }

    type CpapDay = {
        StartDate: SleepStartDate
        // Sessions: CpapSession list
        Sessions : string array
    }

    let apneaEventToString (event: ApneaEvent) : string = 
        sprintf "%s %s %s"
            (sleepStartDateToString event.StartDate) 
            (timeStringToString event.EventTime)
            (event.EventType.ToString())

    let getFileNameOnly (filepath : string): string = 
        filepath.Substring(filepath.LastIndexOf("\\") + 1)


module ApneaFileReader =


    (*
        TOP LEVEL FILE PROCESSING

        * access directory on disk
        * sort files by name -> results in chronological order
        * If date changed from previous file
            * create day with empty sessions list
        * For each file:
            * add session to current day
            * extract data from file
    *)
    open System.IO
    open System.Collections.Generic
    open ApneaModel

(*
    let importAllFolders =         
        Directory.GetFiles("d:\\documents\\cpap\\DATALOG", "*.*") 
        |> Array.map Path.GetFileName 
        |> Array.iter (printfn "%s") 
*)
    // string -> [CpapSession]    
    let importSessions folderPath =
        Directory.GetFiles(folderPath) 
        |> Array.map Path.GetFileName 

    // importDay: string -> CpapDay
    // stores all info about day in CpapDay, including sessions, apnea events, etc.
    let importDay (folderPath : string): CpapDay =
        (*
        let index : int = folderPath.LastIndexOf("\\")
        printfn ("%d %s") index (folderPath.Substring (index + 1))
        *)

        let yyyymmdd : string = getFileNameOnly folderPath
        // printfn "??? %d"  (temp.Length)
        { 
            StartDate =  (SleepStartDate yyyymmdd)
            Sessions = (importSessions folderPath)
        }           
        

    let importAllFolders =         
        // get directories in source folder - each folder is a day
        let sourceDirectory = "d:\\documents\\cpap\\DATALOG"
        // let allFolders= Array.sort [| for path in Directory.EnumerateDirectories(sourceDirectory) -> path|] 
        // return list of days based on list of folders
        Array.map importDay (Array.sort [| for path in Directory.EnumerateDirectories(sourceDirectory) -> path|] )
        

