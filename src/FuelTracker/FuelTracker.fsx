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

type FuelingKind = Full | Half
type AirCon = AcOff | AcOn of byte
type TyreKind = Summer | Winter | Universal
type Economy = High | Average | Low

type Fueling = {
    Date : DateTime
    Odometer : int
    TripOdometer : float
    Amount : float
    Cost : float
    Currency : string
    Fuel : string
    FuelingKind : FuelingKind
    TyreKind : TyreKind
    Economy : Economy
    AirCon : AirCon
    BcConsumption : float option
    BcAvgSpeed : float option
}

let parseFuelingKind = function
    | "full" -> Full
    | _ -> Half

let parseTyreKind = function
    | "summer" -> Summer
    | "snow" -> Winter
    | _ -> Universal

let parseEconomy = function
    | "economical" -> High
    | "speedy" -> Low
    | _ -> Average

let parseAirCon = function
    | 0 -> AcOff
    | p -> AcOn (byte p)

module Option =
    let someIf predicate value =
        match predicate value with
        | true -> Some value
        | _ -> None

type Motostat = CsvProvider<"motostat48277.csv", ";">
let sourceData = Motostat.Load "motostat48277.csv"
// let x = sourceData.Rows |> Seq.head
// x.
let data = sourceData.Rows
            |> Seq.map (fun s -> {
                                    Date = s.Date
                                    Odometer = s.Odometer
                                    TripOdometer = float s.Trip_odometer
                                    Amount = float s.Quantity
                                    Cost = float s.Cost
                                    Currency = s.Currency
                                    Fuel = s.Fuel_name
                                    FuelingKind = parseFuelingKind s.Fueling_type
                                    TyreKind = parseTyreKind s.Tires
                                    Economy = parseEconomy s.Driving_style
                                    AirCon = parseAirCon s.Ac
                                    BcConsumption = Option.someIf (fun x -> x <> 0.) (float s.Bc_consumption)
                                    BcAvgSpeed = Option.someIf (fun x -> x <> 0.) (float s.Bc_avg_speed)
                                })
            |> Seq.sortBy (fun f -> f.Date)
            |> JsonConvert.SerializeObject

let webPart =
    choose [
        path "/" >=> OK data
    ]

startWebServer defaultConfig webPart
