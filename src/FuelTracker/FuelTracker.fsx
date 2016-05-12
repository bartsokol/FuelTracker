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

type Percentage = byte

type FuelingKind = Full | Half
type AirCon = AcOff | AcOn of Percentage
type TyreKind = Summer | Winter | Universal
type Economy = High | Average | Low

type RouteSplit = {
    City : Percentage
    Country : Percentage
    Motorway : Percentage
}

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
    RouteSplit : RouteSplit
    Notes : string
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

let routeSplit city country motorway = {
        City = byte (100m * city)
        Country = byte (100m * country)
        Motorway = byte (100m * motorway)
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
                                    Currency = s.Currency
                                    Fuel = s.Fuel_name
                                    FuelingKind = parseFuelingKind s.Fueling_type
                                    TyreKind = parseTyreKind s.Tires
                                    Economy = parseEconomy s.Driving_style
                                    AirCon = parseAirCon s.Ac
                                    BcConsumption = Option.filter (fun x -> x <> 0.) (Some (float s.Bc_consumption))
                                    BcAvgSpeed = Option.filter (fun x -> x <> 0.) (Some (float s.Bc_avg_speed))
                                    RouteSplit = routeSplit s.Route_city s.Route_country s.Route_motorway
                                    Notes = s.Notes
                                })
            |> Seq.sortBy (fun f -> f.Date)
            |> JsonConvert.SerializeObject

let setJson = Suave.Writers.setHeader "Content-Type" "application/json"

let webPart =
    choose [
        path "/" >=> OK data >=> setJson
    ]

startWebServer defaultConfig webPart
