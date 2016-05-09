#r "..\..\packages/Suave/lib/net40/Suave.dll"
#r "..\..\packages/Suave.Experimental/lib/net40/Suave.Experimental.dll"
#r "..\..\packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "..\..\packages/SQLProvider/lib/FSharp.Data.SQLProvider.dll"
#r "..\..\packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Web
open FSharp.Data
open Newtonsoft.Json

type Fueling = {
    Date : DateTime
    Odometer : int
    TripOdometer : float
    Amount : float
    Cost : float
    Fuel : string
}

type Motostat = CsvProvider<"motostat48277.csv", ";">
let sourceData = Motostat.Load "motostat48277.csv"

let data = sourceData.Rows
            |> Seq.map (fun s -> {
                                    Date = s.Date
                                    Odometer = s.Odometer
                                    TripOdometer = float s.Trip_odometer
                                    Amount = float s.Quantity
                                    Cost = float s.Cost
                                    Fuel = s.Fuel_name
                                })
            |> Seq.sortBy (fun f -> f.Date)
            |> JsonConvert.SerializeObject

let webPart =
    choose [
        path "/" >=> OK data
    ]

startWebServer defaultConfig webPart
