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
        Sessions: CpapSession list
    }

    let apneaEventToString (event: ApneaEvent) : string = 
        sprintf "%s %s %s"
            (sleepStartDateToString event.StartDate) 
            (timeStringToString event.EventTime)
            (event.EventType.ToString())


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

    let importDay (allCpapDays : List<CpapDay>) (folderPath : string): unit =
        (*
        let index : int = folderPath.LastIndexOf("\\")
        printfn ("%d %s") index (folderPath.Substring (index + 1))
        *)

        let yyyymmdd : string = folderPath.Substring(folderPath.LastIndexOf("\\") + 1)
        let cpapDay : CpapDay = { 
            StartDate =  (SleepStartDate yyyymmdd)
            Sessions = []
        }   
        allCpapDays.Add(cpapDay)                
        

    let importAllFolders =         
        // get directories in source folder - each folder is a day
        let sourceDirectory = "d:\\documents\\cpap\\DATALOG"
        let allCpapDays: List<CpapDay> = new List<CpapDay>()
    
        
        [| for path in Directory.EnumerateDirectories(sourceDirectory) -> path|] 
            |> Array.iter (fun item -> importDay allCpapDays item)

        allCpapDays

