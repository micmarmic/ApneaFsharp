// based on https://stackoverflow.com/questions/49457111/fsharp-binaryreader-how-to-read-till-the-end-of-a-file

namespace apnea

module BinaryFileReader =

    open System  // convert
    open System.IO  // File, BinaryReader
    open System.Globalization
    open ApneaModel

    type EdfHeader = {        
        StartDate: DateOnly
        StartTime: TimeOnly
        BytesInHeader: int
        NumberOfRecords: int        
        RecordLength: int
    }

    //
    // HELPERS (GENERIC)
    //

    let edfStringTimeToDateOnly (stringDate : string) : DateOnly =
        DateOnly.ParseExact(stringDate.[0..7], "dd.MM.yy", CultureInfo.InvariantCulture)

    let edfStringTimeToTimeOnly (stringDate : string) : TimeOnly =
        TimeOnly.ParseExact(stringDate.[8..], "H.mm.s", CultureInfo.InvariantCulture)

    let fileLength (filepath : string): int =         
        let f : FileInfo = FileInfo filepath
        int(f.Length) // int64 -> int32 could be lossy, but EDF files are not that big

    let skipReaderToPosition (reader:BinaryReader) (position:int) =
        reader.BaseStream.Seek(position, SeekOrigin.Begin) |> ignore 

    let getNBytesAsString (reader : BinaryReader) (nBytes : int): string =    
        let byteArray = reader.ReadBytes(nBytes)
        System.Text.Encoding.Default.GetString(byteArray).Trim()

    let getNBytesAsStringAtPosition (reader:BinaryReader) (nBytes : int) (position:int) : string =
        skipReaderToPosition reader position
        getNBytesAsString reader nBytes

    let getStringFromPosToMarker (reader:BinaryReader) (position:int) (stopByte : byte) : string =

let NAK : byte = 21uy
let DC4 : byte = 20uy
let selectedBytes : byte list = [ 64uy; 65uy]
let isNAK x =  x = NAK
let result = selectedBytes |> List.findIndex (fun b -> isNAK b)
printfn "index %d list %s" (selectedBytes.ToString())


    //
    // EDF DOMAIN SPECIFIC
    //

    let processRecord (reader : BinaryReader) (pos : int) (recordLength : int): ApneaEvent = 
        skipReaderToPosition reader pos
        reader.ReadBytes(recordLength)

        // the first data is at byte 6
        let start = 6

        // string value for seconds from start goes to NAK (hex: 15 decimal: 21)        
        let stringValue : string = getStringFromPosToMarker reader start 21
        {
            EventType = EventType.Unknown
            StartDate = SleepStartDate "2022-11-11"
            EventTime = TimeString "12:44"
        }
    



    // string (dd.mm.yyhh.mm.ss) -> string (yymmdd hh.mm.ss)
    let formatEdfDateTime (edfDateTime : string) : string =
        sprintf "Start Date %s-%s-%s %s" 
            edfDateTime.[6..7]
            edfDateTime.[3..4]
            edfDateTime.[0..1]
            (edfDateTime.[8..].Replace(".", ":"))

    let testGetEvents (reader : BinaryReader) (header : EdfHeader)  =
        // loop from header + firstRecord (meaningless for ResMed data)
        let startPosition = header.BytesInHeader + header.RecordLength
        // loop to position of last record (-1 because we skipped the first record, -1 because loop is zero-based)
        let lastStart = startPosition + (header.NumberOfRecords - 2) * header.RecordLength
        let result : int list = [ for pos in startPosition .. header.RecordLength .. lastStart  do pos + 0 ]
        printfn "Num Records: %d Expect to read: %d LastStart: %d" header.NumberOfRecords (header.NumberOfRecords - 1) lastStart
        printfn "BytesInHeader: %d RecordLength: %d Expect start at: %d" header.BytesInHeader header.RecordLength (header.BytesInHeader + header.RecordLength)
        printfn "Actual start: %d Num times in loop (len of seq): %d" startPosition result.Length
        printfn "List: %O" result


    // filename -> EdfHeader
    // error if first eight bytes don't contain expected marker
    let readEdfHeader (filename : string) : EdfHeader =
        use stream = File.Open(filename, FileMode.Open, FileAccess.Read)
        use reader = new BinaryReader(stream)

        (*
            EDF Header Format (as used by ResMed in CPAP/BIPAP machines in 2022)
            position #bytes     description
            0           8       check: file type marker (bytes to string to int; expect 0)
            8           160     skip: 80 bytes patient Id (bytes to string); 80 bytes recording id (bytes to string)
            168         16      keep: date string dd.mm.yyhh.mm.ss (bytes to string)
            184         8       keep: # bytes in header (bytes to string to int)
            192         44      skip: reserved
            236         8       keep: number of records

            record length = (file length - bytes in header) / number of records
        *)


        // check first eight bytes for file type        
        let firstEight = getNBytesAsString reader 8
        if (firstEight <> "0") then failwith (sprintf "First eight bytes in file doesn't match expectations: %s" filename)
        // read values and calculate record length
        let dateString = getNBytesAsStringAtPosition reader 16 168
        // bytes in header is consecutive to datestring, don't need to position at 184, just read
        let bytesInHeader = int(getNBytesAsString reader 8)
        let numberOfRecords = int(getNBytesAsStringAtPosition reader 8 236)
        let recordLength = ((fileLength filename) - bytesInHeader) / numberOfRecords


        // construct EdfHeader
        let header : EdfHeader = 
            {
                StartDate = (edfStringTimeToDateOnly dateString)
                StartTime = (edfStringTimeToTimeOnly dateString)
                BytesInHeader = bytesInHeader
                NumberOfRecords = numberOfRecords
                RecordLength = recordLength
            }

        testGetEvents reader header
        header

    let testReadHeader : unit =
        let header = readEdfHeader "d:\\documents\\cpap\\DATALOG\\20220708\\20220709_014647_EVE.edf"
        printfn "%O" header

    
    // reader -> ApneaEvent
    // PRE-CONDITION: the file position must be set correctly
    // POST-CONDITION: 
    // let getEvent 


(* 

            // Position stream at beginning of first record
            // Skip header and first record that holds no data.
            int start = numberOfBytesInHeader + recordLength;
            for (int position = start; position < fileSize; position += recordLength)
            {
                // reposition reading head to start of next record
                br.BaseStream.Seek(position, SeekOrigin.Begin);
                byte[] byteArray = br.ReadBytes(recordLength);

                // SEQUENTIAL READING
                // each of the the following class will depend on the position of the previous ones to remain on track

                // the first data is at byte 6
                int nextPosition = 6;

                // Time of event
                string stringValue;
                // looking for NAK (hex: 15, dec: 21)
                (stringValue, nextPosition) = GetStringFromIndexToChar(byteArray, nextPosition, 21);
                int secondsSinceStart = int.Parse(stringValue);
                DateTime dateTimeOccurred = StartDateTime.AddSeconds(secondsSinceStart);

                // Duration of event
                // looking for DC4 (hex: 14, dec: 20)
                (stringValue, nextPosition) = GetStringFromIndexToChar(byteArray, nextPosition, 20);
                int durationSeconds = int.Parse(stringValue);

                // Description
                (stringValue, _) = GetStringFromIndexToChar(byteArray, nextPosition, 20);
                string description = stringValue;
                ApneaEvent apneaEvent = new(description: description, occurredDateTime: dateTimeOccurred, durationSeconds: durationSeconds);
                _events.Add(apneaEvent);
            }
        }


*)